using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Lykke.Service.BonusEngine.Domain.Models;
using Lykke.Service.BonusEngine.Domain.Repositories;
using Lykke.Service.BonusEngine.Domain.Services;
using Lykke.Service.BonusEngine.DomainServices;
using Moq;

namespace Lykke.Service.BonusEngine.Tests.DomainServices
{
    public class CampaignCompletionServiceTestsFixture
    {
        private Fixture _fixture;

        public CampaignCompletionServiceTestsFixture()
        {
            _fixture = new Fixture();
            CampaignCompletion = _fixture.Create<CampaignCompletion>();
            CampaignCompletion.Id = Guid.NewGuid().ToString("D");
            CampaignCompletion.CustomerId = Guid.NewGuid().ToString("D");

            Campaign = _fixture.Create<Domain.Models.Campaign>();
            Campaign.Id = Guid.NewGuid().ToString("D");

            ConditionCompletions = new List<ConditionCompletion>
            {
                new ConditionCompletion
                {
                    ConditionId = _fixture.Create<Guid>().ToString("D"),
                    CurrentCount = 0,
                    CustomerId = _fixture.Create<Guid>().ToString("D"),
                    Id = _fixture.Create<Guid>().ToString("D"),
                    IsCompleted = false
                }
            };

            CampaignCompletionRepositoryMock = new Mock<ICampaignCompletionRepository>(MockBehavior.Strict);

            ConditionCompletionServiceMock = new Mock<IConditionCompletionService>(MockBehavior.Strict);

            Service = new CampaignCompletionService(
                CampaignCompletionRepositoryMock.Object,
                ConditionCompletionServiceMock.Object);
        }

        public CampaignCompletionServiceTestsFixture SetupGetCampaigns()
        {
            CampaignCompletionRepositoryMock.Setup(c => c.GetByCampaignAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(() => CampaignCompletion);

            return this;
        }

        public CampaignCompletionServiceTestsFixture IncreaseCompletionCount()
        {
            CampaignCompletionRepositoryMock.Setup(c => c.UpdateAsync(It.IsAny<CampaignCompletion>()))
                .Returns(Task.CompletedTask);

            ConditionCompletionServiceMock.Setup(c => c.DeleteAsync(It.IsAny<IEnumerable<ConditionCompletion>>()))
                .Returns(Task.CompletedTask);

            return this;
        }

        public Mock<ICampaignCompletionRepository> CampaignCompletionRepositoryMock { get; set; }

        public Mock<IConditionCompletionService> ConditionCompletionServiceMock { get; set; }

        public Domain.Models.Campaign Campaign { get; set; }

        public CampaignCompletion CampaignCompletion { get; set; }

        public List<ConditionCompletion> ConditionCompletions { get; set; }

        public CampaignCompletionService Service { get; set; }
    }
}
