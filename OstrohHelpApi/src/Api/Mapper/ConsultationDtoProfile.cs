using Api.Dtos;
using AutoMapper;

namespace Api.Mapper;

public class ConsultationDtoProfile : Profile
{
    public ConsultationDtoProfile()
    {
        // Mapping for ConsultationDto to itself (if needed)
        CreateMap<ConsultationDto, ConsultationDto>();
    }
}
