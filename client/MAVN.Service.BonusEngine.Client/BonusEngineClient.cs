using Lykke.HttpClientGenerator;

namespace MAVN.Service.BonusEngine.Client
{
    /// <inheritdoc />
    /// <summary>
    /// BonusEngine API aggregating interface.
    /// </summary>
    public class BonusEngineClient : IBonusEngineClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>C-tor</summary>
        public BonusEngineClient(IHttpClientGenerator httpClientGenerator)
        {
            Customers = httpClientGenerator.Generate<ICustomersApi>();
        }
        
        /// <inheritdoc />
        public ICustomersApi Customers { get; private set; }
    }
}
