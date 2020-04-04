using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.BonusEngine.Contract.Events;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using MAVN.Service.BonusEngine.DomainServices;
using Lykke.Service.Campaign.Client;
using Moq;
using StackExchange.Redis;
using CampaignModel = Lykke.Service.BonusEngine.Domain.Models.Campaign;
using Condition = Lykke.Service.BonusEngine.Domain.Models.Condition;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class CampaignServiceBonusProcessingTestsFixture
    {
        //private readonly int _currentCampaign = 0;
        private int _currentCampaignCompletion = 0;

        public CampaignServiceBonusProcessingTestsFixture()
        {
            var mapper = MapperHelper.CreateAutoMapper();

            ConditionCompletionServiceMock = new Mock<IConditionCompletionService>(MockBehavior.Strict);
            BonusOperationServiceMock = new Mock<IBonusOperationService>(MockBehavior.Strict);
            CampaignCompletionServiceMock = new Mock<ICampaignCompletionService>(MockBehavior.Strict);
            ParticipatedInCampaignEventPublisher = new Mock<IRabbitPublisher<ParticipatedInCampaignEvent>>();
            BonusCalculatorServiceMock = new Mock<IBonusCalculatorService>(MockBehavior.Strict);
            CampaignClientMock = new Mock<ICampaignClient>();
            ConnectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
            ActiveCampaignRepositoryMock = new Mock<IActiveCampaignRepository>();
            CampaignCacheServiceMock = new Mock<ICampaignCacheService>();

            CreateConditionTestData();

            SetupCommonMocks();

            CampaignServiceInstance = new CampaignService(
                CampaignClientMock.Object,
                CampaignCompletionServiceMock.Object,
                ConditionCompletionServiceMock.Object,
                BonusOperationServiceMock.Object,
                ParticipatedInCampaignEventPublisher.Object,
                BonusCalculatorServiceMock.Object,
                EmptyLogFactory.Instance,
                ActiveCampaignRepositoryMock.Object,
                CampaignCacheServiceMock.Object,
                mapper);
        }

        public Mock<IConnectionMultiplexer> ConnectionMultiplexerMock { get; set; }
        public Mock<ICampaignClient> CampaignClientMock { get; set; }
        public Mock<ICampaignCompletionService> CampaignCompletionServiceMock { get; set; }
        public Mock<IBonusCalculatorService> BonusCalculatorServiceMock { get; set; }
        public Mock<IConditionCompletionService> ConditionCompletionServiceMock { get; set; }
        public CampaignService CampaignServiceInstance { get; set; }
        public Mock<IBonusOperationService> BonusOperationServiceMock { get; set; }
        public Mock<IRabbitPublisher<ParticipatedInCampaignEvent>> ParticipatedInCampaignEventPublisher { get; set; }
        public Mock<ICampaignCacheService> CampaignCacheServiceMock { get; set; }
        public Mock<IActiveCampaignRepository> ActiveCampaignRepositoryMock { get; set; }

        public BonusType BonusType { get; set; }
        public ConditionCompletion NewConditionCompletion { get; set; }
        public List<ConditionCompletion> ConditionCompletions { get; set; }
        public BonusOperation NewConditionBonusOperation { get; set; }
        public CampaignCompletion CampaignCompletion { get; set; }
        public List<CampaignCompletion> CampaignCompletions { get; set; }
        public BonusOperation NewCampaignBonusOperation { get; set; }

        //Campaign's service  models
        public Condition ConditionModel { get; set; }
        public List<Condition> ConditionModels { get; set; }
        public CampaignModel CampaignModel { get; set; }
        public List<CampaignModel> CampaignModels { get; set; }

        public string BonusTypeName { get; set; } = "signup";

        public Dictionary<string, string> EventDataEmpty { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> EventDataWithAmount { get; set; } = new Dictionary<string, string> { { "amount", "19.34" } };
        public string CustomerId { get; set; } = Guid.NewGuid().ToString("D");
        public string PartnerId { get; set; }
        public string LocationId { get; set; }
        public string CampaignId { get; set; } = Guid.NewGuid().ToString("D");
        public string ConditionId { get; set; } = Guid.NewGuid().ToString("D");
        public string ConditionCompletionsId { get; set; } = Guid.NewGuid().ToString("D");

        private void SetupCommonMocks()
        {
            CampaignCompletionServiceMock.Setup(c => c.GetByCampaignAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    _currentCampaignCompletion++;
                    return CampaignCompletions[_currentCampaignCompletion - 1];
                });

            ConditionCompletionServiceMock
                .Setup(c => c.GetConditionCompletionsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(() => ConditionCompletions.AsReadOnly());

            ConditionCompletionServiceMock
                .Setup(c => c.GetConditionCompletionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string customerId, string conditionId) =>
                {
                    return ConditionCompletions.FirstOrDefault(x =>
                        x.CustomerId == customerId && x.ConditionId == conditionId)
                        ?? new ConditionCompletion();
                });

            ConditionCompletionServiceMock
                .Setup(c => c.IncreaseOrCreateAsync(It.IsAny<string>(), It.IsAny<ConditionCompletion>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Condition>()))
                .ReturnsAsync(() =>
                {
                    NewConditionCompletion.CurrentCount++;
                    return NewConditionCompletion;
                });

            CampaignCompletionServiceMock.Setup(c =>
                    c.IncreaseCompletionCountAsync(It.IsAny<CampaignCompletion>(), It.IsAny<CampaignModel>(), It.IsAny<IEnumerable<ConditionCompletion>>()))
                .Returns(Task.CompletedTask);

            BonusCalculatorServiceMock
                .Setup(c => c.CalculateRewardAmountAsync(It.IsAny<CampaignModel>(),
                    It.IsAny<string>(), It.IsAny<IEnumerable<ConditionCompletion>>()))
                .ReturnsAsync((CampaignModel campaign, string customerId ,IEnumerable<ConditionCompletion> conditionCompletions) => campaign.Reward);
        }

        public CampaignServiceBonusProcessingTestsFixture SetupConditionProcessingMocks()
        {
            // Condition Mocks
            ConditionCompletionServiceMock
                .Setup(c => c.InsertAsync(It.IsAny<ConditionCompletion>()))
                .ReturnsAsync("Id");

            ConditionCompletionServiceMock
                .Setup(c => c.IncreaseCompletionCountAsync(It.IsAny<ConditionCompletion>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<int>()))
                .Callback(() => { NewConditionCompletion.CurrentCount++; })
                .ReturnsAsync(NewConditionCompletion);

            return this;
        }

        public CampaignServiceBonusProcessingTestsFixture SetupConditionRewardMocks()
        {
            // Condition Reward Mocks
            ConditionCompletionServiceMock
                .Setup(c => c.SetConditionCompletedAsync(It.IsAny<string>()))
                .Callback(() => { NewConditionCompletion.IsCompleted = true; })
                .Returns(Task.CompletedTask);

            BonusOperationServiceMock
                .Setup(c => c.AddBonusOperationAsync(It.IsAny<BonusOperation>()))
                .Returns(Task.CompletedTask);

            return this;
        }

        public CampaignServiceBonusProcessingTestsFixture SetupAllMocks()
        {
            return this.SetupConditionProcessingMocks()
                .SetupConditionRewardMocks()
                .SetupCampaignCacheGetCampaignsByTypeAsync();
        }

        public CampaignServiceBonusProcessingTestsFixture SetupCampaignCacheGetCampaignsByTypeAsync()
        {
            CampaignCacheServiceMock
                .Setup(c => c.GetCampaignsByTypeAsync(It.IsAny<string>()))
                .ReturnsAsync(() => CampaignModels);

            CampaignCacheServiceMock
                .Setup(c => c.GetCampaignFromCacheOrServiceAsync(It.IsAny<string>()))
                .ReturnsAsync((string x) => CampaignModels.SingleOrDefault(y => y.Id == x));
            
            return this;
        }

        private void CreateConditionTestData()
        {
            BonusType = new BonusType
            {
                Type = BonusTypeName,
                DisplayName = BonusTypeName
            };

            ConditionModel = new Condition()
            {
                CampaignId = CampaignId,
                CompletionCount = 1,
                BonusType = BonusType,
                Id = ConditionId,
                ImmediateReward = 10
            };

            ConditionModels = new List<Condition>()
            {
                ConditionModel
            };

            NewConditionCompletion = new ConditionCompletion
            {
                CustomerId = CustomerId,
                ConditionId = ConditionId,
                Id = ConditionCompletionsId,
                CurrentCount = 0,
                IsCompleted = false
            };

            ConditionCompletions = new List<ConditionCompletion>();

            NewConditionBonusOperation = new BonusOperation
            {
                CustomerId = CustomerId,
                Reward = ConditionModel.ImmediateReward,
                BonusOperationType = BonusOperationType.CampaignReward
            };

            CampaignModel = new CampaignModel()
            {
                Name = "SignUp Campaign",
                Reward = 20,
                Id = CampaignId,
                CompletionCount = 1,
                Conditions = new List<Condition>()
                {
                    ConditionModel
                }
            };

            CampaignModels = new List<CampaignModel>()
            {
                CampaignModel
            };

            CampaignCompletion = new CampaignCompletion()
            {
                CustomerId = CustomerId,
                CampaignId = CampaignId,
                CampaignCompletionCount = 0
            };

            CampaignCompletions = new List<CampaignCompletion>()
            {
                CampaignCompletion
            };

            NewCampaignBonusOperation = new BonusOperation
            {
                CustomerId = CustomerId,
                Reward = CampaignModel.Reward,
                BonusOperationType = BonusOperationType.CampaignReward
            };
        }
    }
}
