using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BonusEngine.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
