using MAVN.Service.BonusEngine.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.BonusEngine.Domain.Repositories
{
    public interface IConditionCompletionRepository
    {
        Task<Guid> InsertAsync(ConditionCompletion conditionCompletion);

        Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync();

        Task<ConditionCompletion> GetConditionCompletion(Guid conditionCompletionId);

        Task<ConditionCompletion> GetConditionCompletion(Guid customerId, Guid conditionId);
        
        Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(Guid customerId, Guid campaignId);

        Task IncreaseCompletionCountAsync(Guid conditionCompletionId, Dictionary<string, string> data, int count);

        Task SetConditionCompletedAsync(Guid conditionCompletionId);

        Task UpdateAsync(ConditionCompletion conditionCompletion);

        Task DeleteAsync(IEnumerable<ConditionCompletion> conditionCompletions);

        Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(Guid campaignId);
    }
}
