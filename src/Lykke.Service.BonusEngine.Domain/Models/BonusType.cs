using System;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class BonusType
    {
        public string Type { get; set; }

        public string DisplayName { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreationDate { get; set; }
        
        public bool IsStakeable { get; set; }

        public bool IsHidden { get; set; }

        public int Order { get; set; }

        public bool RewardHasRatio { get; set; }
    }
}
