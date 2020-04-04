using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using Lykke.Service.Campaign.Client;
using Lykke.Service.Campaign.Client.Models.Campaign.Requests;
using Lykke.Service.Campaign.Client.Models.Campaign.Responses;
using Lykke.Service.Campaign.Client.Models.Enums;
using Newtonsoft.Json;
using StackExchange.Redis;
using CampaignModel = Lykke.Service.BonusEngine.Domain.Models.Campaign;

namespace MAVN.Service.BonusEngine.DomainServices
{
    public class CampaignCacheService : ICampaignCacheService
    {
        private readonly ILog _log;
        private readonly IMapper _mapper;
        private readonly IDatabase _db;

        private readonly ICampaignClient _campaignClient;
        private readonly IActiveCampaignRepository _activeCampaignRepository;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        private readonly string _redisInstanceName;
        private readonly string _redisConnectionString;

        private const string CampaignPattern = "{0}:campaignId:{1}";
        private const string BonusTypePattern = "{0}:bonusType:{1}";
        public CampaignCacheService(
            ILogFactory logFactory,
            IConnectionMultiplexer connectionMultiplexer,
            ICampaignClient campaignClient,
            IActiveCampaignRepository activeCampaignRepository,
            string redisInstanceName,
            string redisConnectionString,
            IMapper mapper)
        {
            _log = logFactory.CreateLog(this);
            _mapper = mapper;
            _campaignClient = campaignClient;
            _activeCampaignRepository = activeCampaignRepository;
            _connectionMultiplexer = connectionMultiplexer;
            _db = _connectionMultiplexer.GetDatabase();
            _redisInstanceName = redisInstanceName ??
                                 throw new ArgumentNullException(nameof(redisInstanceName));
            _redisConnectionString = redisConnectionString
                ?? throw new ArgumentNullException(nameof(redisConnectionString));
        }

        public void Start()
        {
            if (_connectionMultiplexer.IsConnected)
            {
                UpdateActiveCampaigns(true).Wait(timeout: TimeSpan.FromSeconds(60));
            }
            else
            {
                _log.Error(null,
                    "CampaignCache service can not update Active Campaigns because connection to cache service is not available");
            }
        }

        public async Task<bool> UpdateActiveCampaigns(bool deleteCache = false)
        {
            PaginatedCampaignListResponseModel campaignsPagedResponse;

            try
            {
                campaignsPagedResponse = await _campaignClient.Campaigns.GetAsync(new CampaignsPaginationRequestModel()
                {
                    CurrentPage = 1,
                    PageSize = 500,
                    CampaignStatus = CampaignStatus.Active
                });
            }
            catch (ClientApiException e)
            {
                _log.Error("An error has occured while getting active campaigns from the Campaign service", e);

                return false;
            }

            if (deleteCache)
            {
                try
                {
                    await ClearCache();
                }
                catch (Exception e)
                {
                    _log.Error("An error has occured while deleting BonusEngine cache", e);

                    return false;
                }
            }

            var camps = _mapper.Map<IReadOnlyList<CampaignModel>>(campaignsPagedResponse.Campaigns);

            var activeCampaignsIds = new List<Guid>();

            foreach (var campaign in camps)
            {
                activeCampaignsIds.Add(Guid.Parse(campaign.Id));

                await AddOrUpdateCampaignInCache(campaign);
            }

            await UpdateActiveCampaignsInDb(activeCampaignsIds);

            return true;
        }

        public async Task<IReadOnlyCollection<CampaignModel>> GetCampaignsByTypeAsync(string conditionType)
        {
            var campaigns = new List<CampaignModel>();

            var conditionTypeKey = GetBonusKeyFromPattern(conditionType);

            var bonusCampaign = await _db.StringGetAsync(conditionTypeKey);

            if (!bonusCampaign.HasValue)
            {
                return campaigns;
            }

            var type = JsonConvert.DeserializeObject<BonusTypeCampaigns>(bonusCampaign);

            foreach (var campaignId in type.CampaignsIds)
            {
                var campaign = await GetCampaignFromCache(campaignId.ToString());

                if (campaign != null)
                {
                    campaigns.Add(campaign);
                }
            }

            return campaigns;
        }

        public async Task UpdateActiveCampaignsInDb(List<Guid> activeCampaignsIds)
        {
            var activeCampaignsFromDataBase = await _activeCampaignRepository.GetAll();

            var toBeDeleted = activeCampaignsFromDataBase.Except(activeCampaignsIds);

            var toBeInserted = activeCampaignsIds.Except(activeCampaignsFromDataBase);

            foreach (var campId in toBeInserted)
            {
                await _activeCampaignRepository.InsertAsync(campId);
            }

            foreach (var id in toBeDeleted)
            {
                await _activeCampaignRepository.DeleteAsync(id);

                await DeleteCampaignFromCache(id.ToString());
            }
        }

        public async Task AddOrUpdateCampaignInCache(CampaignModel campaign)
        {
            var key = GetCampaignKeyFromPattern(campaign.Id);

            var campaignCached = await _db.StringGetAsync(key);

            if (campaignCached.HasValue)
            {
                await DeleteCampaignFromCache(campaign.Id);
            }

            foreach (var type in campaign.Conditions)
            {
                await AddOrUpdateBonusType(type.BonusType.Type, campaign.Id);
            }

            var serialized = JsonConvert.SerializeObject(campaign);

            await _db.StringSetAsync(key, serialized);
        }

        public async Task DeleteCampaignFromCache(string campaignId)
        {
            var key = GetCampaignKeyFromPattern(campaignId);

            var cachedCampaign = await _db.StringGetAsync(key);

            if (cachedCampaign.HasValue)
            {
                var campaign = JsonConvert.DeserializeObject<CampaignModel>(cachedCampaign);

                await _db.KeyDeleteAsync(key);

                foreach (var type in campaign.Conditions)
                {
                    await RemoveCampaignFromBonusType(type.BonusType.Type, Guid.Parse(campaign.Id));
                }
            }
        }

        public async Task RemoveCampaignFromBonusType(string bonusTypeType, Guid campaignId)
        {
            var key = GetBonusKeyFromPattern(bonusTypeType);

            var cachedType = await _db.StringGetAsync(key);

            if (cachedType.HasValue)
            {
                var bonusType = JsonConvert.DeserializeObject<BonusTypeCampaigns>(cachedType);

                if (bonusType.CampaignsIds.Contains(campaignId))
                {
                    bonusType.CampaignsIds.Remove(campaignId);
                }

                var serialized = JsonConvert.SerializeObject(bonusType);

                await _db.StringSetAsync(key, serialized);
            }
        }

        public async Task<CampaignModel> GetCampaignFromCache(string campaignId)
        {
            var key = GetCampaignKeyFromPattern(campaignId);

            var cachedCampaign = await _db.StringGetAsync(key);

            if (!cachedCampaign.HasValue)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<CampaignModel>(cachedCampaign);
        }

        public async Task<CampaignModel> GetCampaignFromCacheOrServiceAsync(string campaignId)
        {
            var key = GetCampaignKeyFromPattern(campaignId);

            var cachedCampaign = await _db.StringGetAsync(key);

            if (cachedCampaign.HasValue)
                return JsonConvert.DeserializeObject<CampaignModel>(cachedCampaign);

            return _mapper.Map<CampaignModel>(await _campaignClient.History.GetEarnRuleByIdAsync(Guid.Parse(campaignId)));
        }

        public async Task AddOrUpdateBonusType(string typeName, string campaignId)
        {
            var key = GetBonusKeyFromPattern(typeName);

            var cachedType = await _db.StringGetAsync(key);

            BonusTypeCampaigns bonusType;

            if (cachedType.HasValue)
            {
                bonusType = JsonConvert.DeserializeObject<BonusTypeCampaigns>(cachedType);

                if (!bonusType.CampaignsIds.Contains(Guid.Parse(campaignId)))
                {
                    bonusType.CampaignsIds.Add(Guid.Parse(campaignId));
                }
            }
            else
            {
                bonusType = new BonusTypeCampaigns()
                {
                    TypeName = typeName,
                    CampaignsIds = new List<Guid>() { Guid.Parse(campaignId) }
                };

            }
            var serialized = JsonConvert.SerializeObject(bonusType);

            await _db.StringSetAsync(key, serialized);
        }

        private string GetCampaignKeyFromPattern(string campaignId)
        {
            return string.Format(CampaignPattern, _redisInstanceName, campaignId);
        }

        private string GetBonusKeyFromPattern(string bonusType)
        {
            return string.Format(BonusTypePattern, _redisInstanceName, bonusType);
        }

        private async Task ClearCache()
        {
            var hostAndPort = _redisConnectionString.Split(",")[0];
            var server = _connectionMultiplexer.GetServer(hostAndPort);

            var keys = server.Keys(pattern: $"{_redisInstanceName}:*", pageSize:1000000).ToArray();

            foreach (var key in keys)
            {
                await _db.KeyDeleteAsync(key);
            }
        }
    }
}
