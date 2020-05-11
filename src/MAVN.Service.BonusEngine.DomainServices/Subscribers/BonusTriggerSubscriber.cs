using System;
using System.Threading.Tasks;
using Common;
using Lykke.Common.Log;
using MAVN.Service.BonusEngine.Domain.Services;
using MAVN.Service.BonusTriggerAgent.Contract.Events;

namespace MAVN.Service.BonusEngine.DomainServices.Subscribers
{
    public class BonusTriggerSubscriber : RabbitSubscriber<BonusTriggerEvent>
    {
        private readonly ICampaignService _campaignManagementService;

        public BonusTriggerSubscriber(
            string connectionString,
            string exchangeName,
            ILogFactory logFactory,
            ICampaignService campaignManagementService)
        : base(connectionString, exchangeName, logFactory)
        {
            _campaignManagementService = campaignManagementService ??
                                         throw new ArgumentNullException(nameof(campaignManagementService));
        }

        protected override async Task<(bool isSuccessful, string errorMessage)> ProcessMessageAsync(BonusTriggerEvent msg)
        {
            Log.Info($"Bonus Trigger event: {msg.ToJson()}", msg.Type, process: nameof(ProcessMessageAsync));

            await _campaignManagementService.ProcessEventForCustomerAsync(
                msg.CustomerId,
                msg.PartnerId,
                msg.LocationId,
                msg.Data,
                msg.Type);

            return (true, null);
        }
    }
}
