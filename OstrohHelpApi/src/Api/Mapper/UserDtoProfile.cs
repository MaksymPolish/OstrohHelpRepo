using Api.Dtos;
using AutoMapper;

namespace Api.Mapper;

public class UserDtoProfile : Profile
{
    public UserDtoProfile()
    {
        // Mapping for UserDto to itself (if needed)
        CreateMap<UserDto, UserDto>();
    }
}
