using Common.Log;
using MAVN.Numerics;
using Lykke.Common.Log;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVN.Service.EligibilityEngine.Client;
using MAVN.Service.EligibilityEngine.Client.Enums;
using MAVN.Service.EligibilityEngine.Client.Models.ConversionRate.Requests;
using Newtonsoft.Json;

namespace MAVN.Service.BonusEngine.DomainServices
{
    public class BonusCalculatorService : IBonusCalculatorService
    {
        private readonly IEligibilityEngineClient _eligibilityEngine;

        private const string AmountName = "Amount";
        private readonly string _baseCurrencyCode;
        private readonly string _tokenName;
        private readonly ILog _log;

        public BonusCalculatorService(
            string baseCurrencyCode,
            string tokenName,
            ILogFactory logFactory,
            IEligibilityEngineClient eligibilityEngine)
        {
            _baseCurrencyCode = baseCurrencyCode;
            _tokenName = tokenName;
            _eligibilityEngine = eligibilityEngine;
            _log = logFactory.CreateLog(this);
        }

        public async Task<Money18> CalculateRewardAmountAsync(Domain.Models.Campaign campaign, string customerId,
            IEnumerable<ConditionCompletion> conditionCompletions)
        {
            switch (campaign.RewardType)
            {
                case RewardType.Fixed:
                    return campaign.Reward;

                case RewardType.Percentage:
                    return await CalculateEarnRuleRewardByPercentageAsync(campaign.Id, customerId, campaign.Reward, conditionCompletions);

                case RewardType.ConversionRate:
                    return await CalculateRewardByConversionRate(campaign.Id, customerId, conditionCompletions);

                default:
                    throw new Exception($"Unknown reward type '{campaign.RewardType}'.");
            }
        }

        public async Task<Money18> CalculateConditionRewardAmountAsync(Condition condition, ConditionCompletion conditionCompletion)
        {
            switch (condition.RewardType)
            {
                case RewardType.Fixed:
                    return condition.ImmediateReward;

                case RewardType.Percentage:
                    return await CalculateConditionRewardByPercentageAsync(condition.ImmediateReward, conditionCompletion);

                case RewardType.ConversionRate:
                    return await CalculateConditionRewardByConversionRate(conditionCompletion);

                default:
                    throw new ArgumentOutOfRangeException($"Unknown reward type '{condition.RewardType}'.");
            }
        }

        public async Task<Money18> CalculateConditionRewardRatioAmountAsync(Condition condition, ConditionCompletion conditionCompletion, string paymentId)
        {
            var dictionary = conditionCompletion.Data.FirstOrDefault(c => c.ContainsKey(paymentId));

            if (dictionary != null)
            {
                switch (condition.RewardType)
                {
                    case RewardType.Fixed:
                    {
                        return CalculateRatioReward(condition.RewardRatio.Ratios, dictionary,
                            condition.ImmediateReward);
                    }

                    case RewardType.Percentage:
                    {
                        var conditionReward =
                            await CalculateConditionRewardByPercentageAsync(condition.ImmediateReward,
                                conditionCompletion, paymentId);

                        return CalculateRatioReward(condition.RewardRatio.Ratios, dictionary, conditionReward);
                    }

                    case RewardType.ConversionRate:
                    {
                        var reward = await CalculateConditionRewardByConversionRate(conditionCompletion);

                        return CalculateRatioReward(condition.RewardRatio.Ratios, dictionary, reward);
                    }

                    default:
                        throw new ArgumentOutOfRangeException($"Unknown reward type '{condition.RewardType}'.");
                }
            }

            return 0;
        }

        public Money18 CalculateRatioReward(IReadOnlyList<RatioAttribute> conditionRatios, Dictionary<string, string> conditionCompletionRewardDictionary, Money18 conditionReward)
        {
            Money18 reward = 0m;
            var ratiosForBonus = new List<RatioAttribute>();
            var savedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(conditionCompletionRewardDictionary.Values.FirstOrDefault());

            if (savedData != null)
            {
                var purchaseCompletion = Convert.ToDecimal(savedData["PurchaseCompletionPercentage"]);
                var givenBonusFor = Convert.ToDecimal(savedData["GivenRatioBonusPercent"]);

                foreach (var ratio in conditionRatios.OrderBy(r => r.Order))
                {
                    if (purchaseCompletion >= ratio.Threshold)
                    {
                        //in order not to give two times reward for 10%
                        if (givenBonusFor < ratio.Threshold)
                        {
                            ratiosForBonus.Add(ratio);
                        }
                    }
                }
            }

            foreach (var ratio in ratiosForBonus)
            {
                reward += ((Money18)ratio.RewardRatio / 100M) * conditionReward;
            }

            return reward;
        }

        private async Task<Money18> CalculateConditionRewardByConversionRate(ConditionCompletion conditionCompletion)
        {
            Money18 rewardsAmount = 0;

            foreach (var completionData in conditionCompletion.Data)
            {
                if (completionData != null && completionData.ContainsKey(AmountName))
                {
                    rewardsAmount += Money18.Parse(completionData[AmountName]);
                }
            }

            return await GetEligibilityEngineAmountByCondition(conditionCompletion, rewardsAmount);
        }

        private async Task<Money18> GetEligibilityEngineAmountByCondition(ConditionCompletion conditionCompletion, Money18 rewardsAmount)
        {
            var response = await _eligibilityEngine.ConversionRate.GetAmountByConditionAsync(
                new ConvertAmountByConditionRequest()
                {
                    CustomerId = Guid.Parse(conditionCompletion.CustomerId),
                    ConditionId = Guid.Parse(conditionCompletion.ConditionId),
                    Amount = rewardsAmount,
                    FromCurrency = _baseCurrencyCode,
                    ToCurrency = _tokenName
                });

            if (response.ErrorCode != EligibilityEngineErrors.None)
            {
                _log.Error(message: "An error occured while converting currency amount",
                    context: $"from: {_baseCurrencyCode}; to: {_tokenName}; error: {response.ErrorCode}");

                return 0;
            }

            return response.Amount;
        }

        private async Task<Money18> CalculateConditionRewardByPercentageAsync(Money18 percentage,
            ConditionCompletion conditionCompletion, string paymentId = null)
        {
            Money18 rewardsAmount = 0;

            foreach (var completionData in conditionCompletion.Data)
            {
                if (paymentId != null && completionData.ContainsKey(paymentId))
                {
                    var rewardDictionary = completionData[paymentId];

                    var paymentDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(rewardDictionary);

                    rewardsAmount += Money18.Parse(paymentDictionary[AmountName]);
                }
                else if (paymentId==null && completionData != null && completionData.ContainsKey(AmountName))
                {
                    rewardsAmount += Money18.Parse(completionData[AmountName]);
                }
            }

            var amountInCurrency = rewardsAmount * (percentage / 100m);

            return await GetEligibilityEngineAmountByCondition(conditionCompletion, amountInCurrency);
        }

        private async Task<Money18> CalculateEarnRuleRewardByPercentageAsync(string campaignId, string customerId, Money18 percentage,
            IEnumerable<ConditionCompletion> conditionCompletions)
        {
            Money18 rewardsAmount = 0;

            foreach (var completion in conditionCompletions)
            {
                foreach (var completionData in completion.Data)
                {
                    if (completionData != null && completionData.ContainsKey(AmountName))
                    {
                        rewardsAmount += Money18.Parse(completionData[AmountName]);
                    }
                }
            }

            var amountInCurrency = rewardsAmount * (percentage / 100m);

            return await GetEligibilityEngineAmountByEarnRuleAsync(campaignId, customerId, amountInCurrency);
        }

        private async Task<Money18> CalculateRewardByConversionRate(
            string campaignId,
            string customerId,
            IEnumerable<ConditionCompletion> conditionCompletions)
        {
            Money18 rewardsAmount = 0;

            foreach (var completion in conditionCompletions)
            {
                foreach (var completionData in completion.Data)
                {
                    if (completionData != null && completionData.ContainsKey(AmountName))
                    {
                        rewardsAmount += Money18.Parse(completionData[AmountName]);
                    }
                }
            }

            return await GetEligibilityEngineAmountByEarnRuleAsync(campaignId, customerId, rewardsAmount);
        }

        private async Task<Money18> GetEligibilityEngineAmountByEarnRuleAsync(string campaignId, string customerId,
            Money18 rewardsAmount)
        {
            var response = await _eligibilityEngine.ConversionRate.GetAmountByEarnRuleAsync(new ConvertAmountByEarnRuleRequest()
            {
                CustomerId = Guid.Parse(customerId),
                EarnRuleId = Guid.Parse(campaignId),
                Amount = rewardsAmount,
                FromCurrency = _baseCurrencyCode,
                ToCurrency = _tokenName
            });

            if (response.ErrorCode != EligibilityEngineErrors.None)
            {
                _log.Error(message: "An error occured while converting currency amount",
                    context: $"from: {_baseCurrencyCode}; to: {_tokenName}; error: {response.ErrorCode}");

                return 0;
            }

            return response.Amount;
        }
    }
}
