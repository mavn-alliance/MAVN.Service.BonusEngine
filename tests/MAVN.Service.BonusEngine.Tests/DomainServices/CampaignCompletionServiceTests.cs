using MAVN.Service.BonusEngine.Domain.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class CampaignCompletionServiceTests
    {
        [Fact]
        public async Task ShouldReturnCampaignCompletion_WhenSuchIsPresentedInTheDatabase()
        {
            // Arrange
            var fixture = new CampaignCompletionServiceTestsFixture().SetupGetCampaigns();

            // Execute
            var result = await fixture.Service.GetByCampaignAsync(fixture.CampaignCompletion.Id, fixture.CampaignCompletion.CustomerId);

            // Assert
            Assert.Equal(fixture.CampaignCompletion, result);
        }

        [Fact]
        public async Task ShouldReturnNull_WhenNoCampaignCompletionIsPresentedInTheDatabase()
        {
            // Arrange
            var fixture = new CampaignCompletionServiceTestsFixture
            {
                CampaignCompletion = null
            }.SetupGetCampaigns();

            // Execute
            var result = await fixture.Service.GetByCampaignAsync(Guid.NewGuid().ToString("D"), Guid.NewGuid().ToString("D"));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldDeleteConditionCompletions_WhenCampaignCompleted()
        {
            // Arrange
            var fixture = new CampaignCompletionServiceTestsFixture().IncreaseCompletionCount();

            fixture.Campaign.CompletionCount = 3;

            fixture.ConditionCompletions.AddRange(new List<ConditionCompletion>
            {
                new ConditionCompletion(),
                new ConditionCompletion(),
            });

            // Execute
            await fixture.Service.IncreaseCompletionCountAsync(
                fixture.CampaignCompletion,
                fixture.Campaign,
                fixture.ConditionCompletions);

            // Assert
            fixture.ConditionCompletionServiceMock
                .Verify(c => c.DeleteAsync(It.IsAny<IEnumerable<ConditionCompletion>>()), Times.Once);

            fixture.CampaignCompletionRepositoryMock
                .Verify(c => c.UpdateAsync(It.IsAny<CampaignCompletion>()), Times.Once);
        }

        [Fact]
        public async Task ShouldIncreaseCompletionCountAndMarkIsCompletedTrue_WhenMaxCompletionCountIsMetYet()
        {
            // Arrange
            var fixture = new CampaignCompletionServiceTestsFixture().IncreaseCompletionCount();

            fixture.Campaign.CompletionCount = 3;
            fixture.CampaignCompletion.CampaignCompletionCount = 2;

            // Execute
            await fixture.Service.IncreaseCompletionCountAsync(
                fixture.CampaignCompletion,
                fixture.Campaign,
                fixture.ConditionCompletions);

            // Assert
            fixture.ConditionCompletionServiceMock
                .Verify(c => c.UpdateAsync(It.IsAny<ConditionCompletion>()), Times.Never);

            fixture.CampaignCompletionRepositoryMock
                .Verify(c => c.UpdateAsync(It.IsAny<CampaignCompletion>()), Times.Once);
        }
    }
}
