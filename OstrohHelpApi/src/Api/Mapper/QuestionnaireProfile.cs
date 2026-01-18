using Api.Dtos;
using AutoMapper;
using Domain.Inventory;

namespace Api.Mapper;

public class QuestionnaireProfile : Profile
{
    public QuestionnaireProfile()
    {
        CreateMap<Questionary, QuestionnaireDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.Value))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.StatusId.Value))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsAnonymous, opt => opt.MapFrom(src => src.IsAnonymous))
            .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.SubmittedAt))
            
            // --- Поля, що заповнюються окремо ---
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.StatusName, opt => opt.Ignore());
    }
}