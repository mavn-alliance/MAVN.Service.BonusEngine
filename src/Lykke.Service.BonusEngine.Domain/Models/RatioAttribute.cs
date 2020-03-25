namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class RatioAttribute
    {
        public int Order { get; set; }

        public decimal RewardRatio { get; set; }

        public decimal PaymentRatio { get; set; }

        public decimal Threshold { get; set; }
    }
}
