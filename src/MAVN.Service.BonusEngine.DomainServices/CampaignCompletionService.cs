using System;
using MAVN.Service.BonusEngine.Domain.Extensions;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.BonusEngine.DomainServices
{
    public class CampaignCompletionService : ICampaignCompletionService
    {
        private readonly ICampaignCompletionRepository _campaignCompletionRepository;
        private readonly IConditionCompletionService _conditionCompletion;

        public CampaignCompletionService(
            ICampaignCompletionRepository campaignCompletionRepository,
            IConditionCompletionService conditionCompletion)
        {
            _campaignCompletionRepository = campaignCompletionRepository;
            _conditionCompletion = conditionCompletion;
        }

        public async Task IncreaseCompletionCountAsync(CampaignCompletion campaignCompletion, Domain.Models.Campaign campaign, IEnumerable<ConditionCompletion> conditionCompletions)
        {
            campaignCompletion.CampaignCompletionCount++;

            if (campaignCompletion.CampaignCompletionCount >= campaign.CompletionCount)
            {
                campaignCompletion.IsCompleted = true;
            }

            // Delete CONDITION tracking because they needs to be reset before next campaign completion
            await _conditionCompletion.DeleteAsync(conditionCompletions);

            // Keep track of CAMPAIGN completion
            await _campaignCompletionRepository.UpdateAsync(campaignCompletion);
        }

        public async Task<CampaignCompletion> GetByCampaignAsync(string campaignId, string customerId)
        {
            return await _campaignCompletionRepository.GetByCampaignAsync(campaignId.ToGuid(), customerId.ToGuid());
        }

        public async Task<string> InsertAsync(CampaignCompletion campaignCompletion)
        {
            var id = (await _campaignCompletionRepository.InsertAsync(campaignCompletion)).ToString("D");

            campaignCompletion.Id = id;

            return id;
        }

        public async Task<IEnumerable<CampaignCompletion>> GetByCampaignAsync(Guid campaignId)
        {
            return await _campaignCompletionRepository.GetByCampaignAsync(campaignId);
        }

        public async Task DeleteAsync(IEnumerable<CampaignCompletion> campaignCompletionsToDelete)
        {
            await _campaignCompletionRepository.DeleteAsync(campaignCompletionsToDelete);
        }
    }
}
