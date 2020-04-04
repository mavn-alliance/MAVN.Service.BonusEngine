using System.Collections.Generic;
using JetBrains.Annotations;

namespace MAVN.Service.BonusEngine.Client.Models.Customers
{
    /// <summary>
    /// Represents Campaign completion by a Customer
    /// </summary>
    [PublicAPI]
    public class CampaignCompletionModel
    {
        /// <summary>
        /// Id of the completion entry
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the Customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// How many times the Campaign has been completed
        /// </summary>
        public int CampaignCompletionCount { get; set; }

        /// <summary>
        /// Id of the Campaign
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// Whether or not the Customer completed the Campaign
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Condition completions for the Campaign
        /// </summary>
        public IReadOnlyList<ConditionCompletionModel> ConditionCompletions { get; set; }
    }
}