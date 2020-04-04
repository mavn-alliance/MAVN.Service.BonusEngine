using Microsoft.EntityFrameworkCore;
using System;
using MAVN.Service.BonusEngine.MsSqlRepositories;

namespace MAVN.Service.BonusEngine.Tests.PostgresRepositories.Fixtures
{
    public class BonusEngineContextFixture : IDisposable
    {
        public BonusEngineContext BonusEngineContext => GetInMemoryContextWithSeededData();

        private BonusEngineContext GetInMemoryContextWithSeededData()
        {
            var context = CreateDataContext();
            BonusEngineDbContextSeed.Seed(context);
            return context;
        }

        private BonusEngineContext CreateDataContext()
        {
            var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(nameof(BonusEngineContext))
             .Options;

            var context = new BonusEngineContext(options);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        public void Dispose()
        {
            BonusEngineContext?.Dispose();
        }
    }
}
