using System;
using AutoMapper;
using MAVN.Service.BonusEngine.Domain.Enums;
using MAVN.Service.BonusEngine.Domain.Models;
using MAVN.Service.Campaign.Client.Models.BonusType;
using MAVN.Service.Campaign.Client.Models.Campaign.Responses;
using MAVN.Service.Campaign.Client.Models.Condition;
using BonusOperationTypeContract = MAVN.Service.BonusEngine.Contract.Enums.BonusOperationType;
using BonusOperationTypeModel = MAVN.Service.BonusEngine.Domain.Enums.BonusOperationType;
using CampaignModel = MAVN.Service.BonusEngine.Domain.Models.Campaign;
using RewardRatioAttribute = MAVN.Service.BonusEngine.Domain.Models.RewardRatioAttribute;

namespace MAVN.Service.BonusEngine.DomainServices
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<BonusOperationTypeContract, BonusOperationTypeModel>();
            CreateMap<BonusOperationTypeModel, BonusOperationTypeContract>();

            CreateMap<CampaignDetailResponseModel, CampaignModel>(MemberList.Destination)
                .ForMember(desc => desc.IsDeleted, opt => opt.Ignore());

            CreateMap<CampaignResponse, CampaignModel>()
                .ForMember(desc => desc.IsDeleted, opt => opt.Ignore());

            CreateMap<BonusType, BonusTypeModel>()
                .ForMember(dest => dest.ErrorCode, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
                .ForMember(dest => dest.Vertical, opt => opt.Ignore())
                .ForMember(dest => dest.AllowInfinite, opt => opt.Ignore())
                .ForMember(dest => dest.AllowPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.AllowConversionRate, opt => opt.Ignore());
            CreateMap<BonusTypeModel, BonusType>()
                .ForMember(dest => dest.CreationDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore());

            CreateMap<ConditionModel, Condition>()
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(c => c.CampaignId.ToString()))
                .ForMember(dest => dest.BonusType,
                    opt => opt.MapFrom(c => new BonusType() {DisplayName = c.TypeDisplayName, Type = c.Type}));

            CreateMap<RewardRatioAttributeDetailsResponseModel, RewardRatioAttribute>();
            CreateMap<RatioAttributeDetailsModel, Domain.Models.RatioAttribute>();

            CreateMap<MAVN.Service.Campaign.Contract.Enums.CampaignStatus, CampaignChangeEventStatus>();
            CreateMap<MAVN.Service.Campaign.Contract.Enums.ActionType, ActionType>();
        }
    }
}
