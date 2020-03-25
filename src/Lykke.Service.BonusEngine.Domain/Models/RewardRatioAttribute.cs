using System.Collections.Generic;

namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class RewardRatioAttribute
    {
        public IReadOnlyList<RatioAttribute> Ratios { get; set; }
    }
}
