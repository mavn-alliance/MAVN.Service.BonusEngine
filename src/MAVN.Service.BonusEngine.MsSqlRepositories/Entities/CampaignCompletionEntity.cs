using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAVN.Service.BonusEngine.MsSqlRepositories.Entities
{
    [Table("campaign_completion")]
    public class CampaignCompletionEntity : BaseEntity
    {
        [Column("customer_id")]
        [Required]
        public Guid CustomerId { get; set; }

        [Column("campaign_completion_count")]
        [Required]
        public int CampaignCompletionCount { get; set; }

        [Column("campaign_id")]
        [Required]
        public Guid CampaignId { get; set; }

        [Column("is_completed")]
        [Required]
        public bool IsCompleted { get; set; }
    }
}
