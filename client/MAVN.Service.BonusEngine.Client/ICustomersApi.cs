using System.Threading.Tasks;
using MAVN.Service.BonusEngine.Client.Models.Customers;
using Refit;

namespace MAVN.Service.BonusEngine.Client
{
    /// <summary>
    /// Interface to interact with Customer specific data
    /// </summary>
    public interface ICustomersApi
    {
        /// <summary>
        /// Used to retrieve info about campaign and condition completions
        /// </summary>
        /// <param name="customerId">Id of the Customer</param>
        /// <param name="campaignId">Id of the Campaign</param>
        /// <returns>The object representing completions</returns>
        [Get("/api/customers/campaign-completion/{customerId}/{campaignId}")]
        Task<CampaignCompletionModel> GetCampaignCompletionsByCustomerIdAsync(string customerId, string campaignId);
    }
}