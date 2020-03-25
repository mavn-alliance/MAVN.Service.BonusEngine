using System;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.BonusEngine.Contract.Enums;
using Lykke.Service.BonusEngine.Contract.Events;
using Lykke.Service.BonusEngine.Domain.Extensions;
using Lykke.Service.BonusEngine.Domain.Models;
using Lykke.Service.BonusEngine.Domain.Services;

namespace Lykke.Service.BonusEngine.DomainServices
{
    public class BonusOperationService : IBonusOperationService
    {
        private readonly IRabbitPublisher<BonusIssuedEvent> _bonusIssuedPublisher;
        private readonly IMapper _mapper;
        private readonly ILog _log;

        public BonusOperationService(
            IRabbitPublisher<BonusIssuedEvent> bonusIssuedPublisher,
            IMapper mapper,
            ILogFactory logFactory)
        {
            _bonusIssuedPublisher = bonusIssuedPublisher;
            _mapper = mapper;
            _log = logFactory.CreateLog(this);
        }

        public async Task AddBonusOperationAsync(BonusOperation bonusOperation)
        {
            var campaignId = bonusOperation.CampaignId.ToGuid();

            var bonusIssuedEvent = new BonusIssuedEvent
            {
                OperationId = Guid.NewGuid(),
                Amount = bonusOperation.Reward,
                CampaignId = campaignId,
                ConditionId = bonusOperation.BonusOperationType == Domain.Enums.BonusOperationType.CampaignReward
                    ? Guid.Empty
                    : bonusOperation.ConditionId.ToGuid(),
                PartnerId = bonusOperation.PartnerId,
                LocationId = bonusOperation.LocationId,
                UnitLocationCode = bonusOperation.UnitLocationCode,
                CustomerId = bonusOperation.CustomerId,
                BonusOperationType = _mapper.Map<BonusOperationType>(bonusOperation.BonusOperationType),
                TimeStamp = bonusOperation.TimeStamp,
                ReferralId = bonusOperation.ReferralId
            };

            // Zero reward operations are not accepted by blockchain facade
            if (bonusIssuedEvent.Amount > 0)
            {
                await _bonusIssuedPublisher.PublishAsync(bonusIssuedEvent);
            
                _log.Info(
                    $"Bonus issued: {bonusIssuedEvent.ToJson()}",
                    bonusIssuedEvent.BonusOperationType,
                    process: nameof(AddBonusOperationAsync));
            }
        }
    }
}
