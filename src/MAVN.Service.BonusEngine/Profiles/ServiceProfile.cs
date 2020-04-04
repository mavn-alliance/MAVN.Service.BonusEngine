using AutoMapper;
using MAVN.Service.BonusEngine.Client.Models.Customers;
using MAVN.Service.BonusEngine.Domain.Models;

namespace MAVN.Service.BonusEngine.Profiles
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<CampaignCompletion, CampaignCompletionModel>()
                .ForMember(d => d.ConditionCompletions, o => o.Ignore());
        }
    }
}