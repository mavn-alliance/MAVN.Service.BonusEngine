using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BonusEngine.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string MsSqlConnectionString { get; set; }
    }
}
