using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.MsSql;
using Lykke.Service.BonusEngine.Domain.Models;
using Lykke.Service.BonusEngine.Domain.Repositories;
using Lykke.Service.BonusEngine.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Service.BonusEngine.MsSqlRepositories.Repositories
{
    public class CampaignCompletionRepository : ICampaignCompletionRepository
    {
        private readonly MsSqlContextFactory<BonusEngineContext> _msSqlContextFactory;
        private readonly IMapper _mapper;

        public CampaignCompletionRepository(
            MsSqlContextFactory<BonusEngineContext> msSqlContextFactory,
            IMapper mapper)
        {
            _msSqlContextFactory = msSqlContextFactory;
            _mapper = mapper;
        }

        public async Task<Guid> InsertAsync(CampaignCompletion campaignCompletion)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = _mapper.Map<CampaignCompletionEntity>(campaignCompletion);

                context.Add(entity);

                await context.SaveChangesAsync();

                return entity.Id;
            }
        }

        public async Task<CampaignCompletion> GetByCampaignAsync(Guid campaignId, Guid customerId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                // Current Date is passed as parameter to avoid execution in memory
                var entities = await context.CampaignCompletionEntities
                    .Where(c => c.CampaignId == campaignId && c.CustomerId == customerId)
                    .FirstOrDefaultAsync();

                return _mapper.Map<CampaignCompletion>(entities);
            }
        }

        public async Task UpdateAsync(CampaignCompletion campaignCompletion)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = _mapper.Map<CampaignCompletionEntity>(campaignCompletion);
                context.CampaignCompletionEntities.Update(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(IEnumerable<CampaignCompletion> campaignCompletions)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entities = _mapper.Map<IEnumerable<CampaignCompletionEntity>>(campaignCompletions);
                context.CampaignCompletionEntities.RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CampaignCompletion>> GetByCampaignAsync(Guid campaignId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entities = await context.CampaignCompletionEntities
                   .Where(c => c.CampaignId == campaignId)
                   .ToListAsync();

                return _mapper.Map<IEnumerable<CampaignCompletion>>(entities);
            }
        }
    }
}
