using System;
using System.ComponentModel;
using Falcon.Numerics;
using Lykke.Service.BonusEngine.Domain.Enums;
using Newtonsoft.Json;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class Condition
    {
        public string Id { get; set; }

        public string CampaignId { get; set; }

        public BonusType BonusType { get; set; }

        public Money18 ImmediateReward { get; set; }

        public int? CompletionCount { get; set; }

        public Guid[] PartnerIds { get; set; }
        
        [DefaultValue(false)]            
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool HasStaking { get; set; }

        public RewardType RewardType { get; set; }

        public Money18? AmountInTokens { get; set; }

        public decimal? AmountInCurrency { get; set; }

        public bool UsePartnerCurrencyRate { get; set; }

        public bool RewardHasRatio { get; set; }

        public decimal BurningRule { get; set; }

        public RewardRatioAttribute RewardRatio { get; set; }
    }
}
