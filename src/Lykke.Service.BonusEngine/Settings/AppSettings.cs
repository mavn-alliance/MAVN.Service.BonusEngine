using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Campaign.Client;
using Lykke.Service.EligibilityEngine.Client;

namespace Lykke.Service.BonusEngine.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public BonusEngineSettings BonusEngineService { get; set; }

        public CampaignServiceClientSettings CampaignServiceClient { get; internal set; }

        public EligibilityEngineServiceClientSettings EligibilityEngineServiceClient { get; set; }
    }
}
