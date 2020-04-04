using System;
using AutoMapper;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.BonusEngine.MsSqlRepositories.Entities;

namespace MAVN.Service.BonusEngine.MsSqlRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CampaignCompletion, CampaignCompletionEntity>(MemberList.Source)
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

            CreateMap<CampaignCompletionEntity, CampaignCompletion>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("D")));

            CreateMap<ConditionCompletion, ConditionCompletionEntity>(MemberList.Source)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.ConditionEntityId, opt => opt.MapFrom(src => Guid.Parse(src.ConditionId)))
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => Guid.Parse(src.CampaignId)));

            CreateMap<ConditionCompletionEntity, ConditionCompletion>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString("D")))
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId.ToString("D")))
                .ForMember(dest => dest.ConditionId, opt => opt.MapFrom(src => src.ConditionEntityId.ToString("D")));
        }
    }
}
