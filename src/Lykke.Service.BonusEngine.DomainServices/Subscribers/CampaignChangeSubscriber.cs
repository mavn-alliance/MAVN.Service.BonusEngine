﻿using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.Service.BonusEngine.Domain.Enums;
using Lykke.Service.BonusEngine.Domain.Services;
using Lykke.Service.Campaign.Contract.Events;

namespace Lykke.Service.BonusEngine.DomainServices.Subscribers
{
    public class CampaignChangeSubscriber : RabbitSubscriber<CampaignChangeEvent>
    {
        private readonly ICampaignService _campaignService;
        private readonly IMapper _mapper;

        public CampaignChangeSubscriber(
            string connectionString, 
            string exchangeName, 
            ILogFactory logFactory,
            ICampaignService campaignService,
            IMapper mapper)
            : base(connectionString, exchangeName, logFactory)
        {
            _campaignService = campaignService;
            _mapper = mapper;
        }

        protected override async Task<(bool isSuccessful, string errorMessage)> ProcessMessageAsync(CampaignChangeEvent message)
        {
            return await _campaignService.ProcessEventForCampaignChangeAsync(message.CampaignId,
                _mapper.Map<CampaignChangeEventStatus>(message.Status),
                _mapper.Map<ActionType>(message.Action));
        }
    }
}
