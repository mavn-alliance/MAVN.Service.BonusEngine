using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using MAVN.Service.Campaign.Client;
using MAVN.Service.EligibilityEngine.Client;

namespace MAVN.Service.BonusEngine.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public BonusEngineSettings BonusEngineService { get; set; }

        public CampaignServiceClientSettings CampaignServiceClient { get; internal set; }

        public EligibilityEngineServiceClientSettings EligibilityEngineServiceClient { get; set; }
    }
}
