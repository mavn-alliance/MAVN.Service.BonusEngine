using MAVN.Service.BonusEngine.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.BonusEngine.Domain.Repositories
{
    public interface ICampaignCompletionRepository
    {
        Task<Guid> InsertAsync(CampaignCompletion campaignCompletion);

        Task<CampaignCompletion> GetByCampaignAsync(Guid campaignId, Guid customerId);

        Task UpdateAsync(CampaignCompletion campaignCompletion);

        Task DeleteAsync(IEnumerable<CampaignCompletion> campaignCompletions);

        Task<IEnumerable<CampaignCompletion>> GetByCampaignAsync(Guid campaignId);
    }
}
