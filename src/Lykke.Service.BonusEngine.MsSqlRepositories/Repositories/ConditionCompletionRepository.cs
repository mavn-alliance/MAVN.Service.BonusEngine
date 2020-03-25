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
    public class ConditionCompletionRepository : IConditionCompletionRepository
    {
        private readonly MsSqlContextFactory<BonusEngineContext> _msSqlContextFactory;
        private readonly IMapper _mapper;

        public ConditionCompletionRepository(
            MsSqlContextFactory<BonusEngineContext> msSqlContextFactory,
            IMapper mapper)
        {
            _msSqlContextFactory = msSqlContextFactory;
            _mapper = mapper;
        }

        public async Task<Guid> InsertAsync(ConditionCompletion conditionCompletion)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = _mapper.Map<ConditionCompletionEntity>(conditionCompletion);

                context.Add(entity);

                await context.SaveChangesAsync();

                return entity.Id;
            }
        }

        public async Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync()
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entities = await context.ConditionCompletionEntities.ToListAsync();

                return _mapper.Map<List<ConditionCompletion>>(entities);
            }
        }

        public async Task<ConditionCompletion> GetConditionCompletion(Guid conditionCompletionGuid)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.ConditionCompletionEntities
                    .FirstOrDefaultAsync(x => x.Id == conditionCompletionGuid);

                return _mapper.Map<ConditionCompletion>(entity);
            }
        }

        public async Task<ConditionCompletion> GetConditionCompletion(Guid customerId, Guid conditionId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.ConditionCompletionEntities
                    .FirstOrDefaultAsync(x => x.ConditionEntityId == conditionId && x.CustomerId == customerId);

                return _mapper.Map<ConditionCompletion>(entity);
            }
        }

        public async Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(Guid customerId, Guid campaignId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entities = await context.ConditionCompletionEntities
                     .Where(x => x.CustomerId == customerId && x.CampaignId == campaignId)
                    .ToListAsync();

                return _mapper.Map<IReadOnlyCollection<ConditionCompletion>>(entities);
            }
        }

        public async Task IncreaseCompletionCountAsync(Guid conditionCompletionId, Dictionary<string, string> data, int count)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.ConditionCompletionEntities
                    .FirstOrDefaultAsync(x => x.Id == conditionCompletionId);

                if (entity == null)
                {
                    throw new ArgumentException($"Condition completion with id '{conditionCompletionId}' not found.");
                }

                entity.CurrentCount += count;
                entity.Data = entity.Data.Append(data).ToArray();

                context.ConditionCompletionEntities.Update(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task SetConditionCompletedAsync(Guid conditionCompletionId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = await context.ConditionCompletionEntities
                    .FirstOrDefaultAsync(x => x.Id == conditionCompletionId);

                if (entity == null)
                {
                    throw new ArgumentException($"Condition completion with id '{conditionCompletionId}' not found.");
                }

                entity.IsCompleted = true;

                context.ConditionCompletionEntities.Update(entity);

                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(ConditionCompletion conditionCompletion)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entity = _mapper.Map<ConditionCompletionEntity>(conditionCompletion);
                context.ConditionCompletionEntities.Update(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(IEnumerable<ConditionCompletion> conditionCompletions)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entities = _mapper.Map<IEnumerable<ConditionCompletionEntity>>(conditionCompletions);
                context.ConditionCompletionEntities.RemoveRange(entities);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IReadOnlyCollection<ConditionCompletion>> GetConditionCompletionsAsync(Guid campaignId)
        {
            using (var context = _msSqlContextFactory.CreateDataContext())
            {
                var entities = await context.ConditionCompletionEntities
                    .Where(x => x.CampaignId == campaignId)
                    .ToListAsync();

                return _mapper.Map<IReadOnlyCollection<ConditionCompletion>>(entities);
            }
        }
    }
}
