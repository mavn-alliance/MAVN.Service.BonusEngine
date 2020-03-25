using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.BonusEngine.Domain.Repositories
{
    public interface IActiveCampaignRepository
    {
        Task<ICollection<Guid>> GetAll();
        Task<Guid> InsertAsync(Guid campaignId);
        Task DeleteAsync(Guid campaignId);
    }
}
