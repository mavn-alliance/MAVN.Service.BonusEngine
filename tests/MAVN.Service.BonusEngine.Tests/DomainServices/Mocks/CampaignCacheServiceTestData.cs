using MAVN.Service.BonusEngine.Domain.Models;
using Lykke.Service.Campaign.Client.Models.Campaign.Responses;
using Lykke.Service.Campaign.Client.Models.Condition;
using System;
using System.Collections.Generic;
using CampaignModel = Lykke.Service.BonusEngine.Domain.Models.Campaign;
using Condition = Lykke.Service.BonusEngine.Domain.Models.Condition;

namespace MAVN.Service.BonusEngine.Tests.DomainServices.Mocks
{

    public static class CampaignCacheServiceTestData
    {
        public const string ActiveCampaignId = "279ede56-50dc-4f17-9f0a-ffcd392f0f31";
        public const string NotActiveCampaignId = "279ede56-50dc-4f17-9f0a-ffcd392f0f32";
        public const string ConditionId = "279ede56-50dc-4f17-9f0a-ffcd392f0f35";

        public static BonusType BonusType =>
            new BonusType
            {
                Type = "Type",
                DisplayName = "DisplayName"
            };

        public static BonusTypeCampaigns BonusTypeCampaigns =>
            new BonusTypeCampaigns
            {
                TypeName = "Type",
                CampaignsIds = new List<Guid>
                {
                      Guid.Parse(NotActiveCampaignId),
                      Guid.Parse(ActiveCampaignId)
                }
            };

        public static Condition Condition =>
            new Condition()
            {
                CampaignId = ActiveCampaignId,
                CompletionCount = 1,
                BonusType = BonusType,
                Id = ConditionId,
                ImmediateReward = 10
            };

        public static CampaignModel CampaignModel =>
            new CampaignModel()
            {
                Name = "SignUp Campaign",
                Reward = 20,
                Id = ActiveCampaignId,
                CompletionCount = 1,
                Conditions = new List<Condition>()
                {
                    Condition
                }
            };

        public static List<Guid> ActiveCampaignIdsFromDb =>
            new List<Guid>()
            {
                Guid.Parse(ActiveCampaignId),
                Guid.Parse(NotActiveCampaignId)
            };

        public static PaginatedCampaignListResponseModel GetPaginatedCampaignListResponseModel()
        {
            var bonusType = new BonusType
            {
                Type = "Type",
                DisplayName = "DisplayName"
            };

            var conditionResponseModel = new ConditionModel()
            {
                CampaignId = new Guid(ActiveCampaignId),
                CompletionCount = 1,
                Type = bonusType.Type,
                Id = Guid.NewGuid().ToString("D"),
                ImmediateReward = 10
            };

            var campaignResponseModel = new CampaignResponse()
            {
                Name = "SignUp Campaign",
                Reward = 20,
                Id = ActiveCampaignId,
                CompletionCount = 1,
                Conditions = new List<ConditionModel>()
                {
                    conditionResponseModel
                }
            };

            var campaignResponseModels = new List<CampaignResponse>()
            {
                campaignResponseModel
            };

            return new PaginatedCampaignListResponseModel()
            {
                Campaigns = campaignResponseModels
            };
        }
    }
}
