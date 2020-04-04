using AutoMapper;
using MAVN.Service.BonusEngine.Profiles;
using Xunit;

namespace MAVN.Service.BonusEngine.Tests
{
    public class AutoMapperProfileTests
    {
        [Fact]
        public void Mapping_Configuration_Is_Correct()
        {
            // arrange

            var mockMapper = new MapperConfiguration(cfg => { cfg.AddProfile(new ServiceProfile()); });
            var mapper = mockMapper.CreateMapper();

            // act

            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // assert

            Assert.True(true);
        }
    }
}
