using System.Collections.Generic;
using Lykke.Sdk;
using System.Threading.Tasks;
using Lykke.Common;
using MAVN.Service.BonusEngine.Domain.Services;

namespace MAVN.Service.BonusEngine.Managers
{
    public class StartupManager : IStartupManager
    {
        private readonly IEnumerable<IStartStop> _startables;
        private readonly ICampaignCacheService _campaignCacheService;

        public StartupManager(
            IEnumerable<IStartStop> startables,
            ICampaignCacheService campaignCacheService)
        {
            _startables = startables;
            _campaignCacheService = campaignCacheService;
        }

        public Task StartAsync()
        {
            foreach (var startable in _startables)
            {
                startable.Start();
            }

            _campaignCacheService.Start();

            return Task.CompletedTask;
        }
    }
}
