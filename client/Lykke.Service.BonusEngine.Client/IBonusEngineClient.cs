using JetBrains.Annotations;

namespace Lykke.Service.BonusEngine.Client
{
    /// <summary>
    /// BonusEngine client interface.
    /// </summary>
    [PublicAPI]
    public interface IBonusEngineClient
    {
        /// <summary>
        /// Interface to interact with Customer specific data
        /// </summary>
        ICustomersApi Customers { get; }
    }
}
