using Api.Dtos;
using AutoMapper;
using Domain.Users;

namespace Api.Mapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId.Value))
            .ForMember(dest => dest.GoogleId, opt => opt.MapFrom(src => src.GoogleId ?? string.Empty));
    }
}