using Api.Dtos;
using AutoMapper;
using Domain.Conferences;

namespace Api.Mapper;

public class ConsultationProfile : Profile
{
    public ConsultationProfile()
    {
        CreateMap<Consultations, ConsultationDto>()
            .ForMember(dest => dest.Id, opt => opt.ConvertUsing(new GuidToStringConverter(), src => src.Id))
            .ForMember(dest => dest.StudentId, opt => opt.ConvertUsing(new GuidToStringConverter(), src => src.StudentId))
            .ForMember(dest => dest.PsychologistId, opt => opt.ConvertUsing(new GuidToStringConverter(), src => src.PsychologistId))
            .ForMember(dest => dest.ScheduledTime, opt => opt.MapFrom(src => src.ScheduledTime))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))

            // поля, що заповнюються окремо
            .ForMember(dest => dest.StatusName, opt => opt.Ignore())
            .ForMember(dest => dest.StudentName, opt => opt.Ignore())
            .ForMember(dest => dest.PsychologistName, opt => opt.Ignore());
    }
}