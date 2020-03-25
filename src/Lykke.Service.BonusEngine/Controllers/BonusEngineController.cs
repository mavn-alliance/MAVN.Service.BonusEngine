using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.BonusEngine.Domain.Enums;
using Lykke.Service.BonusEngine.Domain.Services;
using Lykke.Service.Campaign.Contract.Events;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.BonusEngine.Controllers
{
    [Route("api/campaigns")]
    [ApiController]
    public class BonusEngineController : Controller
    {
        private readonly ICampaignService _campaignManagementService;
        private readonly ICampaignService _campaignService;
        private readonly ICampaignCacheService _campaignCacheService;
        private readonly IMapper _mapper;

        public BonusEngineController(
            ICampaignService campaignManagementService,
            ICampaignService campaignService,
            ICampaignCacheService campaignCacheService,
            IMapper mapper)
        {
            _campaignManagementService = campaignManagementService;
            _campaignService = campaignService;
            _campaignCacheService = campaignCacheService;
            _mapper = mapper;
        }

        [Route("/simulate-trigger"), HttpPost]
        public async Task SimulateTrigger(
            string customerId,
            string partnerId,
            string locationId,
            string conditionType,
            [FromBody]Dictionary<string, string> data)
        {
            await _campaignManagementService.ProcessEventForCustomerAsync(
                customerId,
                partnerId,
                locationId,
                data,
                conditionType);
        }

        [Route("/simulate-event-change"), HttpPost]
        public async Task SimulateEventChange(CampaignChangeEvent message)
        {
            await _campaignService.ProcessEventForCampaignChangeAsync(message.CampaignId,
                _mapper.Map<CampaignChangeEventStatus>(message.Status),
                _mapper.Map<ActionType>(message.Action));
        }

        [Route("/update-active-campaigns"), HttpPost]
        public async Task<bool> UpdateCache()
        {
            return await _campaignCacheService.UpdateActiveCampaigns(true);
        }
    }
}
