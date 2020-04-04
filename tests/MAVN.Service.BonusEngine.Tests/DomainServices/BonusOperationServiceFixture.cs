using System;
using System.Threading.Tasks;
using AutoFixture;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.BonusEngine.Contract.Events;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.DomainServices;
using MAVN.Service.BonusEngine.Domain.Enums;
using Moq;

namespace MAVN.Service.BonusEngine.Tests.DomainServices
{
    public class BonusOperationServiceFixture
    {
        public BonusOperationServiceFixture()
        {
            var mapper = MapperHelper.CreateAutoMapper();

            BonusIssuedEventPublisher = new Mock<IRabbitPublisher<BonusIssuedEvent>>(MockBehavior.Strict);
            BonusIssuedEventPublisher.Setup(c => c.PublishAsync(It.IsAny<BonusIssuedEvent>()))
                .Returns(Task.CompletedTask);

            OperationService = new BonusOperationService(
                BonusIssuedEventPublisher.Object,
                mapper,
                EmptyLogFactory.Instance);
        }

        public Mock<IRabbitPublisher<BonusIssuedEvent>> BonusIssuedEventPublisher { get; set; }

        public BonusOperationService OperationService { get; set; }

        public BonusOperation CreateBonusOperation()
        {
            var fixture = new Fixture();
            var campaignId = fixture.Create<Guid>().ToString("D");
            var customerId = fixture.Create<Guid>().ToString("D");
            var externalOperationId = fixture.Create<Guid>();
            var timeStamp = DateTime.UtcNow;
            var reward = fixture.Create<int>();
            const BonusOperationType bonusOperationType = BonusOperationType.CampaignReward;

            return new BonusOperation
            {
                BonusOperationType = bonusOperationType,
                CampaignId = campaignId,
                CustomerId = customerId,
                ExternalOperationId = externalOperationId,
                Reward = reward,
                TimeStamp = timeStamp
            };
        }
    }
}
