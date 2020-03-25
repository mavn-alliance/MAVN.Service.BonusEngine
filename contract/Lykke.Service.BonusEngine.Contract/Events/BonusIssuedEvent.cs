using System;
using Falcon.Numerics;
using JetBrains.Annotations;
using Lykke.Service.BonusEngine.Contract.Enums;

namespace Lykke.Service.BonusEngine.Contract.Events
{
    /// <summary>
    /// Represents an bonus issued event that will be published for notifying a customer
    /// </summary>
    [PublicAPI]
    public class BonusIssuedEvent
    {
        /// <summary>
        /// Represents OperationId that will be used to identify the operation 
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// Represents PartnerId that will be be used to identify the partner
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// Represents LocationId that will be be used to identify the partner
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Represents UnitLocationCode for RE property
        /// </summary>
        public string UnitLocationCode { get; set; }

        /// <summary>
        /// Represents the campaign's id
        /// </summary>
        public Guid CampaignId { get; set; }

        /// <summary>
        /// Represents the condition Id in case bonus was issued for condition completion, Guid.Empty otherwise
        /// </summary>
        public Guid ConditionId { get; set; }

        /// <summary>
        /// Represents Falcon's CustomerId
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Represents the type of the operation for the granted bonus
        /// </summary>
        public BonusOperationType BonusOperationType { get; set; }

        /// <summary>
        /// Represents an amount that will be granted
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Represents the timeStamp when the bonus is issued
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Represents referral id if available, Guid.Empty otherwise
        /// </summary>
        public string ReferralId { get; set; }
    }
}
