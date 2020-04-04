namespace MAVN.Service.BonusEngine.Domain.Enums
{
    /// <summary>
    /// Specifies an earn rule reward type.
    /// </summary>
    public enum RewardType
    {
        /// <summary>
        /// Fixed amount in the base currency.
        /// </summary>
        Fixed,

        /// <summary>
        /// Percentage of given amount.
        /// </summary>
        Percentage,

        /// <summary>
        /// Conversion rate of given amount.
        /// </summary>
        ConversionRate
    }
}
