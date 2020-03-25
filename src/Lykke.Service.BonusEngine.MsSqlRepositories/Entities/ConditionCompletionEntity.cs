using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Lykke.Service.BonusEngine.MsSqlRepositories.Entities
{
    [Table("condition_completion")]
    public class ConditionCompletionEntity : BaseEntity
    {
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Column("current_count")]
        public int CurrentCount { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }

        [Column("condition_id")]
        public Guid ConditionEntityId { get; set; }
        
        [Column("data")]
        public string _data { get; set; }

        [Column("campaign_id")]
        public Guid CampaignId { get; set; }
        
        [NotMapped]
        public Dictionary<string, string>[] Data
        {
            get =>
                _data == null
                    ? new Dictionary<string, string>[0]
                    : JsonConvert.DeserializeObject<Dictionary<string, string>[]>(_data);

            set => _data = JsonConvert.SerializeObject(value);
        }
    }
}
