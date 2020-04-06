using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Logs;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using MAVN.Service.BonusEngine.DomainServices;
using MAVN.Service.BonusEngine.Tests.DomainServices.Mocks;
using Lykke.Service.Campaign.Client;
using Lykke.Service.Campaign.Client.Models.Campaign.Requests;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;
using Xunit;
using CampaignModel = MAVN.Service.BonusEngine.Domain.Models.Campaign;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class CampaignCacheServiceTests
    {
        private readonly Mock<ICampaignClient> _campaignClientMock;
        private readonly Mock<IActiveCampaignRepository> _activeCampaignRepositoryMock;
        private readonly Mock<IDatabase> _dbMock;
        private readonly Mock<IConnectionMultiplexer> _mockMultiplexer;
        private readonly ICampaignCacheService _campaignCacheService;

        public CampaignCacheServiceTests()
        {
            var mapper = MapperHelper.CreateAutoMapper();

            _campaignClientMock = new Mock<ICampaignClient>();
            _activeCampaignRepositoryMock = new Mock<IActiveCampaignRepository>();
            _dbMock = new Mock<IDatabase>();
            _mockMultiplexer = new Mock<IConnectionMultiplexer>();

            _mockMultiplexer
                .Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_dbMock.Object);

            _campaignCacheService = new CampaignCacheService(
                EmptyLogFactory.Instance,
                _mockMultiplexer.Object,
                _campaignClientMock.Object,
                _activeCampaignRepositoryMock.Object,
                "BonusEngine",
                "connectionString",
                mapper);
        }

        #region Start
        [Fact]
        public void Start_WhenNotConnectedToCacheService_LogError()
        {
            _mockMultiplexer.Setup(m => m.IsConnected)
                .Returns(false);

            _campaignCacheService.Start();

            _campaignClientMock
                .Verify(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()), Times.Never);
        }
        #endregion

        #region GetCampaignFromCache
        [Fact]
        public async Task GetCampaignFromCache_WhenCalled_ReturnCampaignFromCache()
        {
            var fixture = new Fixture();
            var campaign = fixture.Create<CampaignModel>();
            var serialized = JsonConvert.SerializeObject(campaign);

            _dbMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(serialized);

            var result = await _campaignCacheService.GetCampaignFromCache("test");

            Assert.IsType<CampaignModel>(result);
            Assert.Equal(result.Id, campaign.Id);
        }
        #endregion

        #region UpdateActiveCampaigns

        [Fact]
        public async Task UpdateActiveCampaigns_WhenErrorFromClientIsThrown_ReturnFalse()
        {
            _campaignClientMock.Setup(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()))
                .ThrowsAsync(new ClientApiException(System.Net.HttpStatusCode.BadRequest,
                    new ErrorResponse { ErrorMessage = "test" }));

            var result = await _campaignCacheService.UpdateActiveCampaigns();

            _campaignClientMock.Verify(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()),
                Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateActiveCampaigns_WhenNewActiveCampaignsShouldBeInserted_InsertThemAndReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()))
                .ReturnsAsync(CampaignCacheServiceTestData.GetPaginatedCampaignListResponseModel());
            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.ActiveCampaignId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: RedisValue.Null);
            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:bonusType:{CampaignCacheServiceTestData.BonusTypeCampaigns.TypeName}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: RedisValue.Null);
            _activeCampaignRepositoryMock.Setup(a => a.GetAll())
                .ReturnsAsync(new List<Guid>());
            _activeCampaignRepositoryMock.Setup(a => a.InsertAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Guid.NewGuid());

            //Act
            var result = await _campaignCacheService.UpdateActiveCampaigns();

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()), Times.Once);
            _dbMock
                .Verify(db => db.StringGetAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.ActiveCampaignId}", It.IsAny<CommandFlags>()), Times.Once);
            _dbMock
                .Verify(db => db.StringGetAsync($"BonusEngine:bonusType:{CampaignCacheServiceTestData.BonusTypeCampaigns.TypeName}", It.IsAny<CommandFlags>()), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.GetAll(), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.InsertAsync(It.IsAny<Guid>()), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateActiveCampaigns_WhenNewActiveCampaignsShouldBeInsertedAndNotActiveShouldBeRemoved_UpdateThemAndReturnTrue()
        {
            //Arrange
            _campaignClientMock.Setup(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()))
                .ReturnsAsync(CampaignCacheServiceTestData.GetPaginatedCampaignListResponseModel());

            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.ActiveCampaignId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: JsonConvert.SerializeObject(CampaignCacheServiceTestData.CampaignModel));

            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.NotActiveCampaignId}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: JsonConvert.SerializeObject(CampaignCacheServiceTestData.CampaignModel));

            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:bonusType:{CampaignCacheServiceTestData.BonusTypeCampaigns.TypeName}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: JsonConvert.SerializeObject(CampaignCacheServiceTestData.BonusTypeCampaigns));

            _activeCampaignRepositoryMock.Setup(a => a.GetAll())
                .ReturnsAsync(CampaignCacheServiceTestData.ActiveCampaignIdsFromDb);

            _activeCampaignRepositoryMock.Setup(a => a.InsertAsync(It.IsAny<Guid>()))
                .ReturnsAsync(Guid.NewGuid());

            //Act
            var result = await _campaignCacheService.UpdateActiveCampaigns();

            //Assert
            _campaignClientMock
                .Verify(c => c.Campaigns.GetAsync(It.IsAny<CampaignsPaginationRequestModel>()), Times.Once);
            _dbMock
                .Verify(db => db.StringGetAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.ActiveCampaignId}", It.IsAny<CommandFlags>()), Times.Exactly(2));
            //when update existing active campaign in the cache
            _dbMock
                .Verify(db => db.KeyDeleteAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.ActiveCampaignId}", It.IsAny<CommandFlags>()), Times.Once);
            _dbMock
                .Verify(db => db.KeyDeleteAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.NotActiveCampaignId}", It.IsAny<CommandFlags>()), Times.Once);
            _dbMock
                .Verify(db => db.StringGetAsync($"BonusEngine:bonusType:{CampaignCacheServiceTestData.BonusTypeCampaigns.TypeName}", It.IsAny<CommandFlags>()), Times.Exactly(3));
            _activeCampaignRepositoryMock
                .Verify(a => a.GetAll(), Times.Once);
            _activeCampaignRepositoryMock
                .Verify(a => a.InsertAsync(It.IsAny<Guid>()), Times.Never);
            _activeCampaignRepositoryMock
                .Verify(a => a.DeleteAsync(It.IsAny<Guid>()), Times.Once);
            Assert.True(result);
        }

        #endregion

        #region GetCampaignsByTypeAsync

        [Fact]
        public async Task GetCampaignsByTypeAsync_WhenBonusTypePassed_CampaignsFromThisTypeAreReturned()
        {
            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:campaignId:{CampaignCacheServiceTestData.ActiveCampaignId}",
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: JsonConvert.SerializeObject(CampaignCacheServiceTestData.CampaignModel));
            _dbMock.Setup(db => db.StringGetAsync($"BonusEngine:bonusType:{CampaignCacheServiceTestData.BonusTypeCampaigns.TypeName}", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: JsonConvert.SerializeObject(CampaignCacheServiceTestData.BonusTypeCampaigns));

            //Act
            var result =
             await _campaignCacheService.GetCampaignsByTypeAsync(CampaignCacheServiceTestData.BonusTypeCampaigns.TypeName);

            //Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<CampaignModel>>(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCampaignsByTypeAsync_WhenBonusNotExistingTypePassed_ReturnEmptyList()
        {
            _dbMock.Setup(db => db.StringGetAsync("NotExisting", It.IsAny<CommandFlags>()))
                .ReturnsAsync(value: RedisValue.Null);

            //Act
            var result =
                await _campaignCacheService.GetCampaignsByTypeAsync("NotExisting");

            //Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<CampaignModel>>(result);
            Assert.Empty(result);
        }
        #endregion
    }
}
