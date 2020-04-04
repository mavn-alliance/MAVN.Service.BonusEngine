using System;
using System.Collections.Generic;
using Autofac;
using System.Threading.Tasks;
using CampaignModel = Lykke.Service.BonusEngine.Domain.Models.Campaign;

namespace MAVN.Service.BonusEngine.Domain.Services
{
    public interface ICampaignCacheService : IStartable
    {
        Task<bool> UpdateActiveCampaigns(bool deleteCache = false);
        Task<IReadOnlyCollection<CampaignModel>> GetCampaignsByTypeAsync(string conditionType);
        Task UpdateActiveCampaignsInDb(List<Guid> activeCampaignsIds);
        Task AddOrUpdateCampaignInCache(CampaignModel campaignModel);
        Task DeleteCampaignFromCache(string campaignId);
        Task RemoveCampaignFromBonusType(string bonusTypeType, Guid campaignId);
        Task<CampaignModel> GetCampaignFromCache(string campaignId);
        Task<CampaignModel> GetCampaignFromCacheOrServiceAsync(string campaignId);
        Task AddOrUpdateBonusType(string typeName, string campaignId);
    }
}
