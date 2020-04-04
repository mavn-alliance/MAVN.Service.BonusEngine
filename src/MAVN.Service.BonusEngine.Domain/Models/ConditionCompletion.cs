using System.Collections.Generic;
using System.Data;

namespace MAVN.Service.BonusEngine.Domain.Models
{
    public class ConditionCompletion
    {
        public string Id { get; set; }

        public string CustomerId { get; set; }

        public string ConditionId { get; set; }

        public string CampaignId { get; set; }

        public int CurrentCount { get; set; }

        public bool IsCompleted { get; set; }
        
        public Dictionary<string, string>[] Data { get; set; }
    }
}
