using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using MAVN.Numerics;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.BonusEngine.Contract.Events;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using MAVN.Service.Campaign.Client;
using MAVN.Service.Campaign.Client.Models.Campaign.Responses;
using MAVN.Service.Campaign.Client.Models.Enums;

namespace MAVN.Service.BonusEngine.DomainServices
{
    public class CampaignService : ICampaignService
    {
        private const string StakedCampaignIdKey = "StakedCampaignId";
        private const string UnitLocationCodeKey = "UnitLocationCode";
        private const string ReferralId = "ReferralId";
        private const string PaymentId = "PaymentId";

        private readonly ICampaignClient _campaignClient;
        private readonly ICampaignCompletionService _campaignCompletionService;
        private readonly IConditionCompletionService _conditionCompletionService;
        private readonly IBonusOperationService _bonusOperationService;
        private readonly IBonusCalculatorService _bonusCalculatorService;
        private readonly ILog _log;
        private readonly IRabbitPublisher<ParticipatedInCampaignEvent> _rabbitParticipatedInCampaignEventPublisher;
        private readonly IActiveCampaignRepository _activeCampaignRepository;
        private readonly ICampaignCacheService _campaignCacheService;
        private readonly IMapper _mapper;

        public CampaignService(
            ICampaignClient campaignClient,
            ICampaignCompletionService campaignCompletionService,
            IConditionCompletionService conditionCompletionService,
            IBonusOperationService bonusOperationService,
            IRabbitPublisher<ParticipatedInCampaignEvent> rabbitParticipatedInCampaignEventPublisher,
            IBonusCalculatorService bonusCalculatorService,
            ILogFactory logFactor,
            IActiveCampaignRepository activeCampaignRepository,
            ICampaignCacheService campaignCacheService,
            IMapper mapper)
        {
            _campaignCompletionService = campaignCompletionService;
            _conditionCompletionService = conditionCompletionService;
            _bonusOperationService = bonusOperationService;
            _rabbitParticipatedInCampaignEventPublisher = rabbitParticipatedInCampaignEventPublisher;
            _bonusCalculatorService = bonusCalculatorService;
            _campaignClient = campaignClient;
            _log = logFactor.CreateLog(this);
            _activeCampaignRepository = activeCampaignRepository;
            _campaignCacheService = campaignCacheService;
            _mapper = mapper;
        }

        public async Task ProcessEventForCustomerAsync(
            string customerId,
            string partnerId,
            string locationId,
            IReadOnlyDictionary<string, string> data,
            string conditionType)
        {
            var operationId = Guid.NewGuid();

            if (data.ContainsKey(StakedCampaignIdKey))
            {
                var campaign = await _campaignCacheService.GetCampaignFromCacheOrServiceAsync(data[StakedCampaignIdKey]);

                if (campaign.Conditions.Any(x => x.BonusType.Type == conditionType) && campaign.Conditions.Any(y => y.HasStaking))
                {
                    await ProcessCampaignAsync(
                        customerId,
                        partnerId,
                        locationId,
                        data,
                        conditionType,
                        campaign,
                        operationId);
                }
            }

            var activeCampaigns = await _campaignCacheService.GetCampaignsByTypeAsync(conditionType);

            foreach (var campaign in activeCampaigns.Where(x => x.Conditions.Any(y => y.BonusType.Type == conditionType) && x.Conditions.All(y => !y.HasStaking)))
            {
                await ProcessCampaignAsync(
                    customerId,
                    partnerId,
                    locationId,
                    data,
                    conditionType,
                    campaign,
                    operationId);
            }
        }

        private async Task ProcessCampaignAsync(
            string customerId,
            string partnerId,
            string locationId,
            IReadOnlyDictionary<string, string> data,
            string conditionType,
            Domain.Models.Campaign campaign,
            Guid operationId)
        {
            var campaignCompletion = await _campaignCompletionService.GetByCampaignAsync(campaign.Id, customerId);

            if (campaignCompletion == null)
            {
                campaignCompletion = new CampaignCompletion
                {
                    CampaignCompletionCount = 0,
                    CampaignId = campaign.Id,
                    CustomerId = customerId,
                    IsCompleted = false
                };

                await _campaignCompletionService.InsertAsync(campaignCompletion);
            }

            if (!campaignCompletion.IsCompleted)
            {
                await PublishParticipationEventAsync(campaign.Id, customerId);

                var campaignModel = _mapper.Map<Domain.Models.Campaign>(campaign);

                var conditions = campaignModel.Conditions
                    .Where(c => c.BonusType.Type == conditionType)
                    .ToList();

                await ProcessConditionsForCustomerAsync(
                    customerId,
                    partnerId,
                    locationId,
                    campaignModel.Id,
                    conditions,
                    data,
                    operationId);

                await ProcessCampaignForCustomerAsync(campaignModel, campaignCompletion, operationId, data);
            }
        }

        public async Task<(bool isSuccessful, string errorMessage)> ProcessEventForCampaignChangeAsync(
            Guid messageCampaignId, CampaignChangeEventStatus messageStatus, ActionType actionType)
        {
            if (actionType == ActionType.Deleted)
            {
                await CleanCampaignConditionsAsync(messageCampaignId);

                await CleanCampaignCompletionsAsync(messageCampaignId);

                await DeleteCampaignAsync(messageCampaignId);

                return (true, null);
            }

            if (messageStatus == CampaignChangeEventStatus.Active)
            {
                CampaignDetailResponseModel campaign;

                try
                {
                    campaign = await _campaignClient.History.GetEarnRuleByIdAsync(messageCampaignId);
                }
                catch (ClientApiException e)
                {
                    _log.Error(e, "An error calling Campaign.Service has occured");

                    return (false, e.Message);
                }

                if (campaign.ErrorCode != CampaignServiceErrorCodes.None)
                {
                    _log.Error("ProcessEventForCampaignChange",
                        null,
                        "An error calling Campaign.Service has occured",
                        context: new { campaign.ErrorCode, campaign.ErrorMessage });

                    return (false, campaign.ErrorMessage);
                }

                var campaignModel = _mapper.Map<Domain.Models.Campaign>(campaign);

                await _activeCampaignRepository.InsertAsync(messageCampaignId);

                await _campaignCacheService.AddOrUpdateCampaignInCache(campaignModel);
            }
            else if (messageStatus == CampaignChangeEventStatus.Completed)
            {
                await CleanCampaignConditionsAsync(messageCampaignId);

                await DeleteCampaignAsync(messageCampaignId);
            }
            else if (messageStatus == CampaignChangeEventStatus.Inactive ||
                     messageStatus == CampaignChangeEventStatus.Pending)
            {
                await DeleteCampaignAsync(messageCampaignId);
            }

            return (true, null);
        }

        private async Task ProcessConditionsForCustomerAsync(
            string customerId,
            string partnerId,
            string locationId,
            string campaignId,
            IEnumerable<Condition> conditions,
            IReadOnlyDictionary<string, string> data,
            Guid operationId)
        {
            // Doesn't make sense to have multiple of the same conditions in the same campaign, but just in case
            foreach (var condition in conditions)
            {
                // If condition has any partners we need to make sure we match one them, otherwise just skip the check
                if (condition.PartnerIds != null && condition.PartnerIds.Any())
                {
                    if (!Guid.TryParse(partnerId, out var partnerIdGuid) ||
                        !condition.PartnerIds.Contains(partnerIdGuid))
                    {
                        _log.Info("The partner identifier does not match campaign condition partners.",
                            context: $"partnerId: {partnerId}'; conditionId: {condition.Id}");
                        continue;
                    }
                }

                var conditionCompletion = await _conditionCompletionService
                    .GetConditionCompletionAsync(customerId, condition.Id);

                // ConditionCompletion being null is a valid case handled by IncreaseOrCreateAsync
                if (conditionCompletion != null && conditionCompletion.IsCompleted)
                    continue;

                conditionCompletion = await _conditionCompletionService
                    .IncreaseOrCreateAsync(customerId, conditionCompletion, data, condition);

                Money18 reward = 0;

                if (condition.RewardHasRatio && condition.RewardRatio != null)
                {
                    if (data.TryGetValue(PaymentId, out string paymentId))
                    {
                        reward = await _bonusCalculatorService.CalculateConditionRewardRatioAmountAsync(condition, conditionCompletion, paymentId);
                    }
                    else
                    {
                        _log.Error("No paymentId was passed");
                    }
                }
                else
                {
                    reward = await _bonusCalculatorService.CalculateConditionRewardAmountAsync(condition, conditionCompletion);
                }

                if (reward > 0.0M)
                {
                    var bonusOperation = new BonusOperation
                    {
                        CustomerId = customerId,
                        CampaignId = campaignId,
                        ConditionId = condition.Id,
                        ExternalOperationId = operationId,
                        PartnerId = partnerId,
                        LocationId = locationId,
                        Reward = reward,
                        TimeStamp = DateTime.UtcNow,
                        BonusOperationType = BonusOperationType.ConditionReward
                    };

                    if (data.TryGetValue(UnitLocationCodeKey, out string locationCode))
                        bonusOperation.UnitLocationCode = locationCode;

                    if (data.TryGetValue(ReferralId, out string referralId))
                        bonusOperation.ReferralId = referralId;
                    else if (data.TryGetValue(PaymentId, out string paymentId))
                        bonusOperation.ReferralId = paymentId;

                    await _bonusOperationService.AddBonusOperationAsync(bonusOperation);
                }

                if (conditionCompletion.CurrentCount == condition.CompletionCount && !condition.RewardHasRatio)
                {
                    await _conditionCompletionService.SetConditionCompletedAsync(conditionCompletion.Id);

                    _log.Info("Customer completed condition.",
                        context:
                        $"operationId: {operationId}; campaignId: {campaignId}; conditionId: {condition.Id}; customerId: {customerId}");
                }
                else if (condition.RewardHasRatio)
                {
                    var lastThreshold = _conditionCompletionService.SetConditionCompletionLastGivenRatioReward(data, condition, conditionCompletion, out bool allGiven);

                    if (lastThreshold >= 100m && conditionCompletion.CurrentCount == condition.CompletionCount && allGiven)
                    {
                        conditionCompletion.IsCompleted = true;

                        _log.Info("Customer completed condition.",
                            context:
                            $"operationId: {operationId}; campaignId: {campaignId}; conditionId: {condition.Id}; customerId: {customerId}");
                    }
                    else
                    {
                        _log.Info("Customer not complete condition.",
                            context:
                            $"operationId: {operationId}; campaignId: {campaignId}; conditionId: {condition.Id}; customerId: {customerId}; currentCount: {conditionCompletion.CurrentCount}; requiredCount {condition.CompletionCount ?? int.MaxValue}");
                    }

                    await _conditionCompletionService.UpdateAsync(conditionCompletion);
                }
                else
                {
                    _log.Info("Customer not complete condition.",
                        context:
                        $"operationId: {operationId}; campaignId: {campaignId}; conditionId: {condition.Id}; customerId: {customerId}; currentCount: {conditionCompletion.CurrentCount}; requiredCount {condition.CompletionCount ?? int.MaxValue}");
                }
            }
        }

        private async Task ProcessCampaignForCustomerAsync(MAVN.Service.BonusEngine.Domain.Models.Campaign campaign,
            CampaignCompletion campaignCompletion, Guid operationId, IReadOnlyDictionary<string, string> data)
        {
            var customerId = campaignCompletion.CustomerId;
            var campaignId = campaign.Id;

            var conditionCompletions = await _conditionCompletionService
                .GetConditionCompletionsAsync(customerId, campaignId);

            if (IsCampaignCompleted(conditionCompletions, campaign) == false)
                return;

            var reward = await _bonusCalculatorService.CalculateRewardAmountAsync(campaign, customerId, conditionCompletions);

            var bonusOperation = new BonusOperation
            {
                CustomerId = customerId,
                CampaignId = campaignId,
                ExternalOperationId = operationId,
                Reward = reward,
                TimeStamp = DateTime.UtcNow,
                BonusOperationType = BonusOperationType.CampaignReward
            };

            if (data.TryGetValue(ReferralId, out string referralId))
                bonusOperation.ReferralId = referralId;

            await _bonusOperationService.AddBonusOperationAsync(bonusOperation);

            _log.Info("Customer completed all condition in campaign.",
                context: $"operationId: {operationId}; campaignId: {campaignId}; customerId: {customerId}");

            await _campaignCompletionService.IncreaseCompletionCountAsync(campaignCompletion, campaign,
                conditionCompletions);
        }

        private async Task PublishParticipationEventAsync(string campaignId, string customerId)
        {
            await _rabbitParticipatedInCampaignEventPublisher.PublishAsync(new ParticipatedInCampaignEvent
            {
                CampaignId = campaignId,
                CustomerId = customerId
            });
        }

        private async Task CleanCampaignCompletionsAsync(Guid messageCampaignId)
        {
            var campaignCompletionsToDelete = await _campaignCompletionService
                .GetByCampaignAsync(messageCampaignId);

            if (campaignCompletionsToDelete != null)
            {
                await _campaignCompletionService.DeleteAsync(campaignCompletionsToDelete);
            }
        }

        private async Task CleanCampaignConditionsAsync(Guid messageCampaignId)
        {
            var conCompletionsToDelete = await _conditionCompletionService
                .GetConditionCompletionsAsync(messageCampaignId.ToString());

            if (conCompletionsToDelete != null)
            {
                await _conditionCompletionService.DeleteAsync(conCompletionsToDelete);
            }
        }

        private async Task DeleteCampaignAsync(Guid campaignId)
        {
            await _activeCampaignRepository.DeleteAsync(campaignId);

            await _campaignCacheService.DeleteCampaignFromCache(campaignId.ToString());
        }

        private static bool IsCampaignCompleted(IReadOnlyCollection<ConditionCompletion> conditionCompletions,
            MAVN.Service.BonusEngine.Domain.Models.Campaign campaign)
        {
            return conditionCompletions.Count == campaign.Conditions.Count &&
                   conditionCompletions.All(c => c.IsCompleted);
        }
    }
}
