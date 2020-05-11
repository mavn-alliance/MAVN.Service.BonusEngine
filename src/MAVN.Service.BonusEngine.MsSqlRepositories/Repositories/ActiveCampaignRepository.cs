using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVN.Common.MsSql;
using MAVN.Service.BonusEngine.Domain.Repositories;
using MAVN.Service.BonusEngine.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.BonusEngine.MsSqlRepositories.Repositories
{
    public class ActiveCampaignRepository : IActiveCampaignRepository
    {
        private readonly MsSqlContextFactory<BonusEngineContext> _msSqlContextFactory;

        public ActiveCampaignRepository(MsSqlContextFactory<BonusEngineContext> msSqlContextFactory)
        {
            _msSqlContextFactory = msSqlContextFactory;
        }

        public async Task<ICollection<Guid>> GetAll()
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                return await context.ActiveCampaigns.Select(c => c.Id).ToListAsync();
            }
        }

        public async Task<Guid> InsertAsync(Guid campaignId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var existing = await context.ActiveCampaigns.FirstOrDefaultAsync(c => c.Id == campaignId);
                if (existing != null)
                {
                    return existing.Id;
                }

                var entity = new ActiveCampaign { Id = campaignId };

                context.Add(entity);

                await context.SaveChangesAsync();

                return entity.Id;
            }
        }

        public async Task DeleteAsync(Guid campaignId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = context.ActiveCampaigns.FirstOrDefault(c => c.Id == campaignId);

                if (entity != null)
                {
                    context.ActiveCampaigns.Remove(entity);

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
