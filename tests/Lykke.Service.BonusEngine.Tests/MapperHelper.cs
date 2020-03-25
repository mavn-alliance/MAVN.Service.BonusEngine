using AutoMapper;
using Lykke.Service.BonusEngine.MsSqlRepositories;

namespace Lykke.Service.BonusEngine.Tests
{
    public class MapperHelper
    {
        public static IMapper CreateAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddMaps(typeof(AutoMapperProfile), typeof(BonusEngine.DomainServices.AutoMapperProfile)));

            return config.CreateMapper();
        }
    }
}
