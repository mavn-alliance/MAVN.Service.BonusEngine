using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.BonusEngine.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string MsSqlConnectionString { get; set; }
    }
}
