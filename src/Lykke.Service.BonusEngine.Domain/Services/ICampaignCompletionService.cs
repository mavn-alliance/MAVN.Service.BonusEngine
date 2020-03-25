using Lykke.Service.BonusEngine.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BonusEngine.Domain.Services
{
    public interface ICampaignCompletionService
    {
        Task IncreaseCompletionCountAsync(CampaignCompletion campaignCompletion, Campaign campaign, IEnumerable<ConditionCompletion> conditionCompletions);

        Task<CampaignCompletion> GetByCampaignAsync(string campaignId, string customerId);

        Task<string> InsertAsync(CampaignCompletion campaignCompletion);

        Task<IEnumerable<CampaignCompletion>> GetByCampaignAsync(Guid campaignId);

        Task DeleteAsync(IEnumerable<CampaignCompletion> campaignCompletions);
    }
}
