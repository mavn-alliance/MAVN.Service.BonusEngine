using MAVN.Numerics;
using MAVN.Service.BonusEngine.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.BonusEngine.Domain.Services
{
    public interface IBonusCalculatorService
    {
        Task<Money18> CalculateRewardAmountAsync(Campaign campaign, string customerId, IEnumerable<ConditionCompletion> conditionCompletions);
        Task<Money18> CalculateConditionRewardAmountAsync(Condition condition, ConditionCompletion conditionCompletion);
        Task<Money18> CalculateConditionRewardRatioAmountAsync(Condition condition, ConditionCompletion conditionCompletion, string paymentId);
        Money18 CalculateRatioReward(IReadOnlyList<RatioAttribute> conditionRatios,
            Dictionary<string, string> conditionCompletionRatioDictionary, Money18 conditionReward);
    }
}
