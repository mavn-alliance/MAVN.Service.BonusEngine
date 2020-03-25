using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.BonusEngine.Client.Models.Customers
{
    /// <summary>
    /// Represents the Condition completion
    /// </summary>
    [PublicAPI]
    public class ConditionCompletionModel
    {
        /// <summary>
        /// Id of the Condition completion entry
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the Condition
        /// </summary>
        public string ConditionId { get; set; }

        /// <summary>
        /// How many times has the Condition been met
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// Whether or not the Customer completed the Condition
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Additional metadata for Condition completions
        /// </summary>
        public Dictionary<string, string>[] Data { get; set; }
    }
}