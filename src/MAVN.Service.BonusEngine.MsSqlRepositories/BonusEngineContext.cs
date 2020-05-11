using System.Data.Common;
using MAVN.Common.MsSql;
using MAVN.Service.BonusEngine.MsSqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace MAVN.Service.BonusEngine.MsSqlRepositories
{
    public class BonusEngineContext : MsSqlContext
    {
        private const string Schema = "bonus_engine";

        public DbSet<CampaignCompletionEntity> CampaignCompletionEntities { get; set; }
        public DbSet<ConditionCompletionEntity> ConditionCompletionEntities { get; set; }
        public DbSet<ActiveCampaign> ActiveCampaigns { get; set; }

        public BonusEngineContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        // empty constructor needed for EF migrations
        public BonusEngineContext() : base(Schema)
        {
        }

        //Needed constructor for using InMemoryDatabase for tests
        public BonusEngineContext(DbContextOptions options)
            : base(Schema, options)
        {
        }

        public BonusEngineContext(DbConnection dbConnection)
            : base(Schema, dbConnection)
        {
        }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CampaignCompletionEntity>()
                .HasIndex(c => new
                {
                    c.CampaignId,
                    c.CustomerId
                }).IsUnique(false);
            var conditionCompletion = modelBuilder.Entity<ConditionCompletionEntity>();
            conditionCompletion.HasIndex(c => new
            {
                c.CustomerId,
                c.CampaignId
            }).IsUnique(false);

            conditionCompletion.HasIndex(c => new
            {
                c.CustomerId,
                c.ConditionEntityId
            }).IsUnique(false);
        }
    }
}
