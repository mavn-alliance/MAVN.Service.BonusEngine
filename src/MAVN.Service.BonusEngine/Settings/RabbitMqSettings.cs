using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.BonusEngine.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string RabbitMqConnectionString { get; set; }
    }
}
