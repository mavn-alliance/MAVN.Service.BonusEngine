namespace Lykke.Service.BonusEngine.Contract.Events
{
    /// <summary>
    /// Represents an event that will be published once a customer participates in a campaign
    /// </summary>
    public class ParticipatedInCampaignEvent
    {
        /// <summary>
        /// Represents Falcon's CustomerId
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Represents the campaign's id
        /// </summary>
        public string CampaignId { get; set; }
    }
}
