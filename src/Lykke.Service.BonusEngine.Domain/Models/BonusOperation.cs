using System;
using Falcon.Numerics;
using Lykke.Service.BonusEngine.Domain.Enums;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class BonusOperation
    {
        public string CustomerId { get; set; }
        public Money18 Reward { get; set; }
        public string CampaignId { get; set; }
        public string ConditionId { get; set; }
        public Guid ExternalOperationId { get; set; }
        public DateTime TimeStamp { get; set; }
        public BonusOperationType BonusOperationType { get; set; }
        public string PartnerId { get; set; }
        public string LocationId { get; set; }
        public string UnitLocationCode { get; set; }
        public string ReferralId { get; set; }
    }
}
