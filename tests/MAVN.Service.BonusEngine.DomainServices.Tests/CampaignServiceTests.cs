using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.BonusEngine.Contract.Events;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using Lykke.Service.Campaign.Client;
using Moq;
using Xunit;

namespace MAVN.Service.BonusEngine.DomainServices.Tests
{
    public class CampaignServiceTests
    {
        private readonly Mock<ICampaignClient> _campaignClientMock = new Mock<ICampaignClient>();
        private readonly Mock<ICampaignCompletionService> _campaignCompletionServiceMock = new Mock<ICampaignCompletionService>();
        private readonly Mock<IConditionCompletionService> _conditionCompletionServiceMock = new Mock<IConditionCompletionService>();
        private readonly Mock<IBonusOperationService> _bonusOperationServiceMock = new Mock<IBonusOperationService>();
        private readonly Mock<IRabbitPublisher<ParticipatedInCampaignEvent>> _publisherMock = new Mock<IRabbitPublisher<ParticipatedInCampaignEvent>>();
        private readonly Mock<IBonusCalculatorService> _bonusCalculatorServiceMock = new Mock<IBonusCalculatorService>();
        private readonly Mock<IActiveCampaignRepository> _activeCampaignRepositoryMock = new Mock<IActiveCampaignRepository>();
        private readonly Mock<ICampaignCacheService> _campaignCacheServiceMock = new Mock<ICampaignCacheService>();

        private const string BonusType = "bonus-type";

        private readonly List<Domain.Models.Campaign> _campaigns = new List<Domain.Models.Campaign>
        {
            new Domain.Models.Campaign
            {
                Id= "e8a6cc63-5362-472e-39b6-08d752c0dc29",
                Name = "string",
                Description = "string",
                Reward = Money18.Parse("19"),
                RewardType = RewardType.Fixed,
                AmountInTokens = Money18.Parse("0"),
                AmountInCurrency = 0,
                UsePartnerCurrencyRate = true,
                FromDate = DateTime.Parse("2019-10-18T08:44:14.773"),
                ToDate = DateTime.Parse("2019-10-19T08:44:14.773"),
                CompletionCount = 1,
                IsEnabled = true,
                CreationDate = DateTime.Parse("2019-10-18T08:48:54.183114"),
                CreatedBy = "string",
                CampaignStatus = CampaignStatus.Active,
                Conditions = new []
                {
                    new Condition{
                      Id = "b6346347-45d4-41b3-49b1-08d752c0dc2a",
                      CampaignId = "e8a6cc63-5362-472e-39b6-08d752c0dc29",
                      BonusType = new BonusType
                      {
                          Type = BonusType
                      },
                      ImmediateReward = Money18.Parse("0"),
                      CompletionCount = 1,
                      PartnerIds = new Guid[0],
                      HasStaking = true
                    }
                }
            },
            new Domain.Models.Campaign
            {
                Id= "4c1e905f-a46d-45fb-39b7-08d752c0dc29",
                Name = "string",
                Description = "string",
                Reward = Money18.Parse("19"),
                RewardType = RewardType.Fixed,
                AmountInTokens = Money18.Parse("0"),
                AmountInCurrency = 0,
                UsePartnerCurrencyRate = true,
                FromDate = DateTime.Parse("2019-10-18T08:44:14.773"),
                ToDate = DateTime.Parse("2019-10-19T08:44:14.773"),
                CompletionCount = 1,
                IsEnabled = true,
                CreationDate = DateTime.Parse("2019-10-18T08:48:54.183114"),
                CreatedBy = "string",
                CampaignStatus = CampaignStatus.Active,
                Conditions = new []
                {
                    new Condition{
                        Id = "22759350-1244-4e7f-49b2-08d752c0dc2a",
                        CampaignId = "4c1e905f-a46d-45fb-39b7-08d752c0dc29",
                        BonusType = new BonusType
                        {
                            Type = BonusType
                        },
                        ImmediateReward = Money18.Parse("0"),
                        CompletionCount = 1,
                        PartnerIds = new Guid[0],
                        HasStaking = true
                    }
                }
            },
            new Domain.Models.Campaign
            {
                Id= "b0a1f15b-966d-4efd-39b8-08d752c0dc29",
                Name = "string",
                Description = "string",
                Reward = Money18.Parse("19"),
                RewardType = RewardType.Fixed,
                AmountInTokens = Money18.Parse("0"),
                AmountInCurrency = 0,
                UsePartnerCurrencyRate = true,
                FromDate = DateTime.Parse("2019-10-18T08:44:14.773"),
                ToDate = DateTime.Parse("2019-10-19T08:44:14.773"),
                CompletionCount = 1,
                IsEnabled = true,
                CreationDate = DateTime.Parse("2019-10-18T08:48:54.183114"),
                CreatedBy = "string",
                CampaignStatus = CampaignStatus.Active,
                Conditions = new []
                {
                    new Condition{
                        Id = "08c58df8-551c-41f9-49b3-08d752c0dc2a",
                        CampaignId = "b0a1f15b-966d-4efd-39b8-08d752c0dc29",
                        BonusType = new BonusType
                        {
                            Type = BonusType
                        },
                        ImmediateReward = Money18.Parse("0"),
                        CompletionCount = 1,
                        PartnerIds = new Guid[0],
                        HasStaking = false
                    }
                }
            }
        };
        
        private readonly ICampaignService _service;

        public CampaignServiceTests()
        {
            _campaignCacheServiceMock.Setup(x => x.GetCampaignFromCacheOrServiceAsync(It.IsAny<string>()))
                .ReturnsAsync((string value) => _campaigns.Single(x => x.Id == value));

            _campaignCacheServiceMock.Setup(x => x.GetCampaignsByTypeAsync(It.IsAny<string>()))
                .ReturnsAsync(_campaigns);

            _conditionCompletionServiceMock.Setup(x => x.IncreaseOrCreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<ConditionCompletion>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<Condition>()))
                .ReturnsAsync(new ConditionCompletion());

            _conditionCompletionServiceMock.Setup(x => x.GetConditionCompletionsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new List<ConditionCompletion>
                {
                    new ConditionCompletion
                    {
                        IsCompleted = true
                    }
                });

            var mapper = MapperHelper.CreateAutoMapper();

            _service = new CampaignService(
                _campaignClientMock.Object,
                _campaignCompletionServiceMock.Object,
                _conditionCompletionServiceMock.Object,
                _bonusOperationServiceMock.Object,
                _publisherMock.Object,
                _bonusCalculatorServiceMock.Object,
                EmptyLogFactory.Instance,
                _activeCampaignRepositoryMock.Object,
                _campaignCacheServiceMock.Object,
                mapper);
        }

        [Fact]
        public async Task OneStakingCampaign()
        {
            await _service.ProcessEventForCustomerAsync(
                Guid.NewGuid().ToString(),
                null,
                null,
                new Dictionary<string, string>
                {
                    {"StakedCampaignId", "e8a6cc63-5362-472e-39b6-08d752c0dc29"}
                },
                BonusType);
            
            _bonusOperationServiceMock.Verify(x => x.AddBonusOperationAsync(
                    It.Is<BonusOperation>(
                        y => y.CampaignId == "e8a6cc63-5362-472e-39b6-08d752c0dc29" ||
                             y.CampaignId == "b0a1f15b-966d-4efd-39b8-08d752c0dc29")),
                Times.Exactly(2));
        }

        [Fact]
        public async Task NoStakingCampaign()
        {
            await _service.ProcessEventForCustomerAsync(
                Guid.NewGuid().ToString(),
                null,
                null,
                new Dictionary<string, string>(),
                BonusType);

            _bonusOperationServiceMock.Verify(x => x.AddBonusOperationAsync(
                    It.Is<BonusOperation>(
                        y => y.CampaignId == "b0a1f15b-966d-4efd-39b8-08d752c0dc29")),
                Times.Once);
        }
    }
}
