﻿using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BonusEngine.Client 
{
    /// <summary>
    /// BonusEngine client settings.
    /// </summary>
    public class BonusEngineServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
