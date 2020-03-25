using Autofac;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.BonusEngine.Contract.Events;
using Lykke.Service.BonusEngine.DomainServices.Subscribers;
using Lykke.Service.BonusEngine.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.BonusEngine.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private readonly string _connString;

        private const string BonusIssuedExchangeName = "lykke.bonus.bonusissued";
        private const string BonusTriggerExchangeName = "lykke.bonus.trigger";
        private const string ParticipatedInCampaignEventExchangeName = "lykke.bonus.participatedincampaign";
        private const string CampaignChangeExchangeName = "lykke.campaign.campaignchange";

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _connString = appSettings.CurrentValue.BonusEngineService.RabbitMq.RabbitMqConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterJsonRabbitPublisher<BonusIssuedEvent>(
                _connString,
                BonusIssuedExchangeName);

            builder.RegisterJsonRabbitPublisher<ParticipatedInCampaignEvent>(
                _connString,
                ParticipatedInCampaignEventExchangeName);

            //Subscribers
            builder.RegisterType<BonusTriggerSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _connString)
                .WithParameter("exchangeName", BonusTriggerExchangeName);

            builder.RegisterType<CampaignChangeSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _connString)
                .WithParameter("exchangeName", CampaignChangeExchangeName);
        }
    }
}
