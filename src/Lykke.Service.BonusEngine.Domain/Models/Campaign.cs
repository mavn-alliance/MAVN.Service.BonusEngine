using System;
using System.Collections.Generic;
using Falcon.Numerics;
using Lykke.Service.BonusEngine.Domain.Enums;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    /// <summary>
    /// Represents an earn rule.
    /// </summary>
    public class Campaign
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Money18 Reward { get; set; }

        public Money18 AmountInTokens { get; set; }

        public decimal AmountInCurrency { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string Description { get; set; }

        public int? CompletionCount { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsEnabled { get; set; }

        public IReadOnlyList<Condition> Conditions { get; set; }

        public DateTime CreationDate { get; set; }

        public string CreatedBy { get; set; }
        
        public RewardType RewardType { get; set; }

        public CampaignStatus CampaignStatus { get; set; }

        public bool UsePartnerCurrencyRate { get; set; }
    }
}
