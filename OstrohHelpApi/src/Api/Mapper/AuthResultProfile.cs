using Api.Dtos;
using AutoMapper;

namespace Api.Mapper;

public class AuthResultProfile : Profile
{
    public AuthResultProfile()
    {
        // Mapping for AuthResultDto is usually manual, but for completeness:
        CreateMap<AuthResultDto, AuthResultDto>();
    }
}
