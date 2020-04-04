using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MAVN.Service.BonusEngine.Domain.Enums;

namespace MAVN.Service.BonusEngine.Domain.Services
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
