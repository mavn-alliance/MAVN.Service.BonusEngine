using Autofac;
using Lykke.Common.MsSql;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.MsSqlRepositories.Repositories;

namespace MAVN.Service.BonusEngine.MsSqlRepositories
{
    public class AutofacModule : Module
    {
        private readonly string _connectionString;

        public AutofacModule(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _connectionString,
                connString => new BonusEngineContext(connString, false),
                dbConn => new BonusEngineContext(dbConn));

            builder.RegisterType<CampaignCompletionRepository>()
                .As<ICampaignCompletionRepository>()
                .SingleInstance();

            builder.RegisterType<ConditionCompletionRepository>()
                .As<IConditionCompletionRepository>()
                .SingleInstance();

            builder.RegisterType<ActiveCampaignRepository>()
                .As<IActiveCampaignRepository>()
                .SingleInstance();
        }
    }
}
