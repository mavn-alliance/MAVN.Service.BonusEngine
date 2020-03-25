using JetBrains.Annotations;

namespace Lykke.Service.BonusEngine.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BonusEngineSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public RedisSettings Redis { get; set; }

        public Constants Constants { get; set; }
    }
}
