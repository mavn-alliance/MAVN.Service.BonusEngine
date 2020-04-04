using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using MAVN.Service.BonusEngine.MsSqlRepositories;

namespace MAVN.Service.BonusEngine.Tests.PostgresRepositories.Fixtures
{
    public class BonusEngineDbContextSeed
    {
        public const string BonusTypeSignUp = "signup";
   
        public static void Seed(BonusEngineContext context)
        {
            var today = DateTime.UtcNow;
            var creationDate = DateTime.UtcNow.AddMonths(-1);

          
        }
    }
}
