using System.Collections.Generic;

namespace MAVN.Service.BonusEngine.Domain.Models
{
    public class RewardRatioAttribute
    {
        public IReadOnlyList<RatioAttribute> Ratios { get; set; }
    }
}
