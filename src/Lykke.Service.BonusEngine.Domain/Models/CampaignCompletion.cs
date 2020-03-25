namespace Lykke.Service.BonusEngine.Domain.Models
{
    public class CampaignCompletion
    {
        public string Id { get; set; }

        public string CustomerId { get; set; }

        public int CampaignCompletionCount { get; set; }

        public string CampaignId { get; set; }

        public bool IsCompleted { get; set; }
    }
}
