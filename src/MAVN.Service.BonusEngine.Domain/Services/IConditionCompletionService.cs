using MAVN.Service.BonusEngine.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.BonusEngine.Domain.Services
{
    public interface IConditionCompletionService
    {
        Task<string> InsertAsync(ConditionCompletion conditionCompletion);

        Task<ConditionCompletion> IncreaseCompletionCountAsync(ConditionCompletion conditionCompletion,
            Dictionary<string, string> data, int count);

        Task<ConditionCompletion> GetConditionCompletionAsync(string customerId, string conditionId);

        Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(string customerId,
            string campaignId);

        Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(string campaignId);

        Task<ConditionCompletion> IncreaseOrCreateAsync(string customerId, ConditionCompletion conditionCompletion,
            IReadOnlyDictionary<string, string> data, Condition condition);

        Task SetConditionCompletedAsync(string conditionId);

        Task UpdateAsync(ConditionCompletion conditionCompletion);

        Task DeleteAsync(IEnumerable<ConditionCompletion> conditionCompletions);

        decimal SetConditionCompletionLastGivenRatioReward(IReadOnlyDictionary<string, string> data, Condition condition, 
            ConditionCompletion conditionCompletion, out bool allThresholdGiven);
    }
}
