using AutoMapper;
using Lykke.Service.BonusEngine.Client.Models.Customers;
using Lykke.Service.BonusEngine.Domain.Models;

namespace Lykke.Service.BonusEngine.Profiles
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