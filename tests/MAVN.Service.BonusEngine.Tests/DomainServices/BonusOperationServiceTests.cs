using System;
using System.Threading.Tasks;
using MAVN.Service.BonusEngine.Contract.Enums;
using MAVN.Service.BonusEngine.Contract.Events;
using Moq;
using Xunit;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class BonusOperationServiceTests
    {
        /// <summary>
        /// Given that all requirements for a bonus are completed
        /// When method is called for adding bonus
        /// Then Event for Cash-in and Bonus issues should be dispatched with the correct data
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldSendCashInAndBonusIssues_WhenBonusOperationValid()
        {
            // Arrange
            var fixture = new BonusOperationServiceFixture();

            var bonusOperation = fixture.CreateBonusOperation();

            var mapper = MapperHelper.CreateAutoMapper();
            var expectedType = mapper.Map<BonusOperationType>(bonusOperation.BonusOperationType);

            // Execute
            await fixture.OperationService.AddBonusOperationAsync(bonusOperation);

            // Assert

            fixture.BonusIssuedEventPublisher
                .Verify(c => c.PublishAsync(
                    It.Is<BonusIssuedEvent>(x =>
                        x.BonusOperationType == expectedType &&
                        x.Amount == bonusOperation.Reward &&
                        x.CampaignId == Guid.Parse(bonusOperation.CampaignId) &&
                        x.CustomerId == bonusOperation.CustomerId &&
                        x.OperationId != Guid.Empty &&
                        x.TimeStamp == bonusOperation.TimeStamp)), 
                    Times.Once);
        }

        /// <summary>
        /// Given that all requirements for a bonus are completed
        ///  And the id for the campaign is invalid guid
        /// When method is called for adding bonus
        /// Then an exception should be raised without sending any events
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ShouldThrowException_WhenBonusOperationInvalid()
        {
            // Arrange
            var fixture = new BonusOperationServiceFixture();

            var bonusOperation = fixture.CreateBonusOperation();
            bonusOperation.CampaignId = "I am invalid id";

            // Execute
            await Assert.ThrowsAsync<ArgumentException>(() => fixture.OperationService.AddBonusOperationAsync(bonusOperation));

            // Assert
            fixture.BonusIssuedEventPublisher
                .Verify(c => c.PublishAsync(
                        It.IsAny<BonusIssuedEvent>()),
                    Times.Never);
        }
    }
}
