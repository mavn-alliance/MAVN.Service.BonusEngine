using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BonusEngine.Domain.Enums;

namespace Lykke.Service.BonusEngine.Domain.Services
{
    public interface ICampaignService
    {
        Task ProcessEventForCustomerAsync(
            string customerId,
            string partnerId,
            string locationId,
            IReadOnlyDictionary<string, string> data,
            string conditionType);

        Task<(bool isSuccessful, string errorMessage)> ProcessEventForCampaignChangeAsync(Guid messageCampaignId,
            CampaignChangeEventStatus messageStatus, ActionType action);
    }
}
