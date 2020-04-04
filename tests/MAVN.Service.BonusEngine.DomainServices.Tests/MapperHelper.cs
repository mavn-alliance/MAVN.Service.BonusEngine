using AutoMapper;

namespace MAVN.Service.BonusEngine.DomainServices.Tests
{
    public static class MapperHelper
    {
        public static IMapper CreateAutoMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(AutoMapperProfile)));

            return config.CreateMapper();
        }
    }
}
