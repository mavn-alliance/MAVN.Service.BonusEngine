using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.BonusEngine.Contract.Events;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using MAVN.Service.BonusEngine.DomainServices;
using Lykke.Service.Campaign.Client.Models.Enums;
using Lykke.Service.Campaign.Client;
using Moq;
using Xunit;
using CampaignModel = MAVN.Service.BonusEngine.Domain.Models.Campaign;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class CampaignServiceTests
    {
        private readonly Mock<ICampaignClient> _campaignClientMock;
        private readonly Mock<ICampaignCompletionService> _campaignCompletionServiceMock;
        private readonly Mock<IConditionCompletionService> _conditionCompletionServiceMock;
        private readonly Mock<IBonusOperationService> _bonusOperationServiceMock;
        private readonly Mock<IBonusCalculatorService> _bonusCalculatorServiceMock;
        private readonly Mock<IRabbitPublisher<ParticipatedInCampaignEvent>> _rabbitParticipatedInCampaignEventPublisherMock;
        private readonly Mock<IActiveCampaignRepository> _activeCampaignRepositoryMock;
        private readonly Mock<ICampaignCacheService> _campaignCacheServiceMock;

        private readonly ICampaignService _campaignService;
        public CampaignServiceTests()
        {
            var mapper = MapperHelper.CreateAutoMapper();

            _campaignClientMock = new Mock<ICampaignClient>();
            _campaignCompletionServiceMock = new Mock<ICampaignCompletionService>();
            _conditionCompletionServiceMock = new Mock<IConditionCompletionService>();
            _bonusOperationServiceMock = new Mock<IBonusOperationService>();
            _bonusCalculatorServiceMock = new Mock<IBonusCalculatorService>();
            _rabbitParticipatedInCampaignEventPublisherMock = new Mock<IRabbitPublisher<ParticipatedInCampaignEvent>>();
            _activeCampaignRepositoryMock = new Mock<IActiveCampaignRepository>();
            _campaignCacheServiceMock = new Mock<ICampaignCacheService>();

            _campaignService = new CampaignService(
                _campaignClientMock.Object,
                _campaignCompletionServiceMock.Object,
                _conditionCompletionServiceMock.Object,
                _bonusOperationServiceMock.Object,
                _rabbitParticipatedInCampaignEventPublisherMock.Object,
                _bonusCalculatorServiceMock.Object,
                EmptyLogFactory.Instance,
                _activeCampaignRepositoryMock.Object,
                _campaignCacheServiceMock.Object,
                mapper);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusActiveAndErrorThrownFromClient_ReturnFalse()
        {
            _campaignClientMock.Setup(c => c.History.GetEarnRuleByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new ClientApiException(System.Net.HttpStatusCode.BadRequest, 
                    new ErrorResponse { ErrorMessage = "Test error" }));

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Active,
                ActionType.Created);

            //Assert
            _campaignClientMock
                .Verify(c => c.History.GetEarnRuleByIdAsync(It.IsAny<Guid>()), Times.Once);

            Assert.False(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusActiveAndErrorCodeReturnedFromClient_ReturnFalse()
        {
            _campaignClientMock.Setup(c => c.History.GetEarnRuleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Lykke.Service.Campaign.Client.Models.Campaign.Responses.CampaignDetailResponseModel
                {
                    ErrorCode = CampaignServiceErrorCodes.GuidCanNotBeParsed,
                    ErrorMessage = "Test error"
                });

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Active,
                ActionType.Created);

            //Assert
            _campaignClientMock
                .Verify(c => c.History.GetEarnRuleByIdAsync(It.IsAny<Guid>()), Times.Once);

            Assert.False(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusActiveAndCampaignReturnedFromClient_ReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.History.GetEarnRuleByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid earnRuleId) =>  new Lykke.Service.Campaign.Client.Models.Campaign.Responses.CampaignDetailResponseModel
                {
                    Id = earnRuleId.ToString(),
                    Name = "Test campaign",
                    ErrorCode = CampaignServiceErrorCodes.None
                });

            _activeCampaignRepositoryMock.Setup(a => a.InsertAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Guid.NewGuid);

            _campaignCacheServiceMock.Setup(c => c.AddOrUpdateCampaignInCache(It.IsAny<CampaignModel>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(
                messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Active,
                ActionType.Activated);

            //Assert
            _campaignClientMock
                .Verify(c => c.History.GetEarnRuleByIdAsync(It.IsAny<Guid>()), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.InsertAsync(It.IsAny<Guid>()), Times.Once);
            _campaignCacheServiceMock
                .Verify(c => c.AddOrUpdateCampaignInCache(It.IsAny<CampaignModel>()), Times.Once);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusCompletedAndNotExistingConCompletion_ReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Lykke.Service.Campaign.Client.Models.Campaign.Responses.CampaignDetailResponseModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test campaign",
                    ErrorCode = CampaignServiceErrorCodes.None
                });

            _conditionCompletionServiceMock.Setup(c => c.GetConditionCompletionsAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);

           _activeCampaignRepositoryMock.Setup(a => a.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _campaignCacheServiceMock.Setup(c => c.DeleteCampaignFromCache(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(
                messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Completed,
                ActionType.Completed);

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _conditionCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<IReadOnlyCollection<ConditionCompletion>>()), Times.Never);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Once);
            _conditionCompletionServiceMock
                .Verify(c => c.GetConditionCompletionsAsync(It.IsAny<string>()), Times.Once);
            _campaignCacheServiceMock
                .Verify(c => c.DeleteCampaignFromCache(It.IsAny<string>()), Times.Once);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusCompletedAndExistingConCompletion_ReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Lykke.Service.Campaign.Client.Models.Campaign.Responses.CampaignDetailResponseModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test campaign",
                    ErrorCode = CampaignServiceErrorCodes.None
                });

            _conditionCompletionServiceMock.Setup(c => c.GetConditionCompletionsAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new List<ConditionCompletion>()
                {
                    new ConditionCompletion()
                    {
                         Id = Guid.NewGuid().ToString()
                    }
                });

            _activeCampaignRepositoryMock.Setup(a => a.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _campaignCacheServiceMock.Setup(c => c.DeleteCampaignFromCache(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(
                messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Completed,
                ActionType.Completed);

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _conditionCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<IReadOnlyCollection<ConditionCompletion>>()), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Once);
            _conditionCompletionServiceMock
                .Verify(c => c.GetConditionCompletionsAsync(It.IsAny<string>()), Times.Once);
            _campaignCacheServiceMock
                .Verify(c => c.DeleteCampaignFromCache(It.IsAny<string>()), Times.Once);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusDeletedAndExistingConCompletion_ReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Lykke.Service.Campaign.Client.Models.Campaign.Responses.CampaignDetailResponseModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test campaign",
                    ErrorCode = CampaignServiceErrorCodes.None
                });

            _conditionCompletionServiceMock.Setup(c => c.GetConditionCompletionsAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new List<ConditionCompletion>()
                {
                    new ConditionCompletion()
                    {
                         Id = Guid.NewGuid().ToString()
                    }
                });
            _campaignCompletionServiceMock.Setup(c => c.GetByCampaignAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => new List<CampaignCompletion>()
                {
                    new CampaignCompletion
                    {
                         Id = Guid.NewGuid().ToString()
                    }
                });
            _activeCampaignRepositoryMock.Setup(a => a.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _campaignCacheServiceMock.Setup(c => c.DeleteCampaignFromCache(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(
                messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Pending,
                ActionType.Deleted);

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _conditionCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<IReadOnlyCollection<ConditionCompletion>>()), Times.Once);
            _campaignCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<List<CampaignCompletion>>()), Times.Once);
            _campaignCompletionServiceMock
                .Verify(c => c.GetByCampaignAsync(It.IsAny<Guid>()), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Once);
            _conditionCompletionServiceMock
                .Verify(c => c.GetConditionCompletionsAsync(It.IsAny<string>()), Times.Once);
            _campaignCacheServiceMock
                .Verify(c => c.DeleteCampaignFromCache(It.IsAny<string>()), Times.Once);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusDeletedAndNotExistingConCompletion_ReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Lykke.Service.Campaign.Client.Models.Campaign.Responses.CampaignDetailResponseModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test campaign",
                    ErrorCode = CampaignServiceErrorCodes.None
                });

            _conditionCompletionServiceMock.Setup(c => c.GetConditionCompletionsAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);
            _campaignCompletionServiceMock.Setup(c => c.GetByCampaignAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => null);
            _activeCampaignRepositoryMock.Setup(a => a.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _campaignCacheServiceMock.Setup(c => c.DeleteCampaignFromCache(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(
                messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Pending,
                ActionType.Deleted
                );

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _conditionCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<IReadOnlyCollection<ConditionCompletion>>()), Times.Never);
            _campaignCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<List<CampaignCompletion>>()), Times.Never);
            _campaignCompletionServiceMock
                .Verify(c => c.GetByCampaignAsync(It.IsAny<Guid>()), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Once);
            _conditionCompletionServiceMock
                .Verify(c => c.GetConditionCompletionsAsync(It.IsAny<string>()), Times.Once);
            _campaignCacheServiceMock
                .Verify(c => c.DeleteCampaignFromCache(It.IsAny<string>()), Times.Once);

            Assert.True(result.isSuccessful);
        }

        [Fact]
        public async Task ProcessEventForCampaignChange_WhenCampaignStatusPending_ReturnTrue()
        {
            //Act
            var result = await _campaignService.ProcessEventForCampaignChangeAsync(
                messageCampaignId: Guid.NewGuid(),
                messageStatus: CampaignChangeEventStatus.Pending,
                ActionType.Edited);

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _conditionCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<IReadOnlyCollection<ConditionCompletion>>()), Times.Never);
            _campaignCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<List<CampaignCompletion>>()), Times.Never);
            _campaignCompletionServiceMock
                .Verify(c => c.GetByCampaignAsync(It.IsAny<Guid>()), Times.Never);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Once);
            _conditionCompletionServiceMock
                .Verify(c => c.GetConditionCompletionsAsync(It.IsAny<string>()), Times.Never);
            _campaignCacheServiceMock
                .Verify(c => c.DeleteCampaignFromCache(It.IsAny<string>()), Times.Once);

            Assert.True(result.isSuccessful);
        }
    }
}
