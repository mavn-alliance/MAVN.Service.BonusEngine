using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using MAVN.Service.BonusEngine.Domain.Services;
using MAVN.Service.BonusEngine.DomainServices;
using MAVN.Service.BonusEngine.Managers;
using MAVN.Service.BonusEngine.MsSqlRepositories;
using MAVN.Service.BonusEngine.Settings;
using MAVN.Service.Campaign.Client;
using MAVN.Service.EligibilityEngine.Client;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace MAVN.Service.BonusEngine.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CampaignService>()
                .As<ICampaignService>()
                .SingleInstance();

            builder.RegisterType<CampaignCompletionService>()
                .As<ICampaignCompletionService>()
                .SingleInstance();

            builder.RegisterType<BonusCalculatorService>()
                .As<IBonusCalculatorService>()
                .WithParameter("baseCurrencyCode", _appSettings.CurrentValue.BonusEngineService.Constants.BaseCurrencyCode)
                .WithParameter("tokenName",
                    _appSettings.CurrentValue.BonusEngineService.Constants.TokenSymbol)
                .SingleInstance();

            builder.RegisterType<BonusOperationService>()
                .As<IBonusOperationService>()
                .SingleInstance();

            builder.RegisterType<ConditionCompletionService>()
                .As<IConditionCompletionService>()
                .SingleInstance();

            builder.RegisterModule(
                new AutofacModule(_appSettings.CurrentValue.BonusEngineService.Db.MsSqlConnectionString));

            builder.RegisterCampaignClient(_appSettings.CurrentValue.CampaignServiceClient, null);
            
            builder.RegisterEligibilityEngineClient(_appSettings.CurrentValue.EligibilityEngineServiceClient, null);

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();
            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance()
                .AutoActivate();

            //Redis
            builder.Register(context =>
            {
                var connectionMultiplexer =
                    ConnectionMultiplexer.Connect(_appSettings.CurrentValue.BonusEngineService.Redis.ConnectionString);
                connectionMultiplexer.IncludeDetailInExceptions = false;
                return connectionMultiplexer;
            }).As<IConnectionMultiplexer>().SingleInstance();

            builder.RegisterType<CampaignCacheService>()
                .As<ICampaignCacheService>()
                .WithParameter("redisInstanceName", _appSettings.CurrentValue.BonusEngineService.Redis.InstanceName)
                .WithParameter("redisConnectionString", _appSettings.CurrentValue.BonusEngineService.Redis.ConnectionString)
                .SingleInstance();
        }
    }
}
