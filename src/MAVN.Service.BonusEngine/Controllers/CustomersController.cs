using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MAVN.Service.BonusEngine.Client;
using MAVN.Service.BonusEngine.Client.Models.Customers;
using MAVN.Service.BonusEngine.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace MAVN.Service.BonusEngine.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersController : Controller, ICustomersApi
    {
        private readonly ICampaignCompletionService _campaignCompletionService;
        private readonly IConditionCompletionService _conditionCompletionService;
        private readonly IMapper _mapper;

        public CustomersController(
            ICampaignCompletionService campaignCompletionService,
            IConditionCompletionService conditionCompletionService,
            IMapper mapper)
        {
            _campaignCompletionService = campaignCompletionService;
            _conditionCompletionService = conditionCompletionService;
            _mapper = mapper;
        }

        /// <inheritdoc />
        [HttpGet("campaign-completion/{customerId}/{campaignId}")]
        [ProducesResponseType(typeof(CampaignCompletionModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        public async Task<CampaignCompletionModel> GetCampaignCompletionsByCustomerIdAsync(string customerId, string campaignId)
        {
            if (!Guid.TryParse(customerId, out _) || !Guid.TryParse(campaignId, out _))
                return null;

            var campaignCompletion =
                await _campaignCompletionService.GetByCampaignAsync(campaignId, customerId);
            var conditionCompletions =
                await _conditionCompletionService.GetConditionCompletionsAsync(customerId, campaignId);

            if (campaignCompletion == null)
                return null;

            var model = _mapper.Map<CampaignCompletionModel>(campaignCompletion);

            model.ConditionCompletions = _mapper.Map<IReadOnlyList<ConditionCompletionModel>>(conditionCompletions);

            return model;
        }
    }
}
