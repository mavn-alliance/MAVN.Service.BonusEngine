using System;
using MAVN.Service.BonusEngine.Domain.Extensions;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MAVN.Service.BonusEngine.DomainServices
{
    public class ConditionCompletionService : IConditionCompletionService
    {
        private const string GivenRatioBonusPercent = "GivenRatioBonusPercent";
        private const string PurchaseCompletionPercentage = "PurchaseCompletionPercentage";
        private const string PaymentId = "PaymentId";

        private readonly IConditionCompletionRepository _conditionCompletionRepository;

        public ConditionCompletionService(IConditionCompletionRepository conditionCompletionRepository)
        {
            _conditionCompletionRepository = conditionCompletionRepository;
        }

        public async Task<string> InsertAsync(ConditionCompletion conditionCompletion)
        {
            var conditionCompletionId = await _conditionCompletionRepository.InsertAsync(conditionCompletion);
            return conditionCompletionId.ToString("D");
        }

        public async Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync()
        {
            return await _conditionCompletionRepository.GetConditionCompletionsAsync();
        }

        public async Task<ConditionCompletion> GetConditionCompletionAsync(string customerId, string conditionId)
        {
            return await _conditionCompletionRepository.GetConditionCompletion(customerId.ToGuid(), conditionId.ToGuid());
        }

        public async Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(string customerId, string campaignId)
        {
            return await _conditionCompletionRepository.GetConditionCompletionsAsync(customerId.ToGuid(), campaignId.ToGuid());
        }

        public async Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(string campaignId)
        {
            return await _conditionCompletionRepository.GetConditionCompletionsAsync(campaignId.ToGuid());
        }

        public async Task<ConditionCompletion> IncreaseOrCreateAsync(string customerId, ConditionCompletion conditionCompletion, IReadOnlyDictionary<string, string> data, Condition condition)
        {
            var dataToInsert = data.ToDictionary(o => o.Key, o => o.Value);

            // if the condition completion is not null user has not started the condition
            if (conditionCompletion == null)
            {
                if (condition.RewardHasRatio)
                {
                    dataToInsert.Add(GivenRatioBonusPercent, "0");

                    dataToInsert = PrepareDataForRewardRatio(dataToInsert);
                }

                conditionCompletion = new ConditionCompletion
                {
                    ConditionId = condition.Id,
                    CurrentCount = 1,
                    CustomerId = customerId,
                    CampaignId = condition.CampaignId,
                    Data = new[] { dataToInsert }
                };

                conditionCompletion.Id = await InsertAsync(conditionCompletion);

                return conditionCompletion;
            }

            // completion count is null when conditionCompletionCount can be infinity
            if (condition.CompletionCount == null || conditionCompletion.CurrentCount < condition.CompletionCount)
            {
                if (condition.RewardHasRatio)
                {
                    var paymentId = dataToInsert[PaymentId];

                    var oldData = conditionCompletion.Data.FirstOrDefault(c => c.ContainsKey(paymentId));

                    //this is the case that we have payment for new paymentId
                    if (oldData == null)
                    {
                        if (condition.CompletionCount == null || conditionCompletion.Data.Count() < condition.CompletionCount)
                        {
                            dataToInsert.Add(GivenRatioBonusPercent, "0");

                            dataToInsert = PrepareDataForRewardRatio(dataToInsert);

                            conditionCompletion = await IncreaseCompletionCountAsync(conditionCompletion,
                                dataToInsert.ToDictionary(o => o.Key, o => o.Value), 1);
                        }
                    }
                    else
                    {
                        await UpdatePaymentRatioDataAsync(conditionCompletion, oldData, dataToInsert, paymentId);
                    }
                }
                else
                {
                    conditionCompletion = await IncreaseCompletionCountAsync(conditionCompletion,
                        dataToInsert.ToDictionary(o => o.Key, o => o.Value), 1);
                }
            }
            else
            {
                if (condition.RewardHasRatio)
                {
                    var paymentId = dataToInsert[PaymentId];

                    var oldData = conditionCompletion.Data.FirstOrDefault(c => c.ContainsKey(paymentId));

                    if (oldData != null)
                    {
                        await UpdatePaymentRatioDataAsync(conditionCompletion, oldData, dataToInsert, paymentId);
                    }
                }
                else
                {
                    await SetConditionCompletedAsync(conditionCompletion.Id);
                }
            }

            return conditionCompletion;
        }

        private async Task UpdatePaymentRatioDataAsync(ConditionCompletion conditionCompletion, Dictionary<string, string> oldData,
            Dictionary<string, string> dataToInsert, string paymentId)
        {
            var given = JsonConvert.DeserializeObject<Dictionary<string, string>>(oldData.Values.FirstOrDefault());

            var givenBonus = given[GivenRatioBonusPercent];

            if (givenBonus != null)
                dataToInsert.Add(GivenRatioBonusPercent, givenBonus);

            oldData[paymentId] = JsonConvert.SerializeObject(dataToInsert);

            await _conditionCompletionRepository.UpdateAsync(conditionCompletion);
        }

        private Dictionary<string, string> PrepareDataForRewardRatio(Dictionary<string, string> dataToInsert)
        {
            var newDictionary = new Dictionary<string, string>();

            var paymentId = dataToInsert[PaymentId];

            dataToInsert.Remove(PaymentId);

            newDictionary.Add(paymentId, JsonConvert.SerializeObject(dataToInsert));

            return newDictionary;
        }

        public decimal SetConditionCompletionLastGivenRatioReward(IReadOnlyDictionary<string, string> data, Condition condition,
            ConditionCompletion conditionCompletion, out bool allThresholdGiven)
        {
            allThresholdGiven = true;

            if (condition.RewardRatio == null)
                return 0;

            var paymentId = data["PaymentId"];

            var paymentIdData = conditionCompletion.Data.FirstOrDefault(d => d.ContainsKey(paymentId));

            var lastThreshold = 0m;

            if (paymentIdData != null)
            {
                foreach (var d in conditionCompletion.Data)
                {
                    var dataDictionary =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(d.Values
                            .FirstOrDefault());
                    var dataToUpdate = dataDictionary.ToDictionary(o => o.Key, o => o.Value);

                    if (d.ContainsKey(paymentId))
                    {
                        if (dataToUpdate.TryGetValue(PurchaseCompletionPercentage, out string percentage))
                        {
                            var value = Convert.ToDecimal(percentage);

                            foreach (var ratio in condition.RewardRatio.Ratios.OrderBy(r => r.Order))
                            {
                                if (value >= ratio.Threshold)
                                {
                                    lastThreshold = ratio.Threshold;
                                }
                            }

                            //check if we have GivenRatioBonusPercent and update value
                            if (lastThreshold != 0m)
                                dataToUpdate[GivenRatioBonusPercent] = lastThreshold.ToString();
                        }

                        paymentIdData[paymentId] = JsonConvert.SerializeObject(dataToUpdate);
                    }
                    else
                    {
                        dataToUpdate.TryGetValue(GivenRatioBonusPercent, out string percentage);

                        if (Convert.ToDecimal(percentage) < 100)
                            allThresholdGiven = false;
                    }
                }

                return lastThreshold;
            }

            return 0;
        }

        public async Task SetConditionCompletedAsync(string conditionCompletionId)
        {
            await _conditionCompletionRepository.SetConditionCompletedAsync(conditionCompletionId.ToGuid());
        }

        public async Task UpdateAsync(ConditionCompletion conditionCompletion)
        {
            await _conditionCompletionRepository.UpdateAsync(conditionCompletion);
        }

        public async Task DeleteAsync(IEnumerable<ConditionCompletion> conditionCompletions)
        {
            await _conditionCompletionRepository.DeleteAsync(conditionCompletions);
        }

        public async Task<ConditionCompletion> IncreaseCompletionCountAsync(ConditionCompletion conditionCompletion, Dictionary<string, string> data, int count)
        {
            await _conditionCompletionRepository.IncreaseCompletionCountAsync(conditionCompletion.Id.ToGuid(), data, count);

            return await _conditionCompletionRepository.GetConditionCompletion(conditionCompletion.Id.ToGuid());
        }
    }
}
