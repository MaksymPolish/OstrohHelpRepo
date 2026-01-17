using Api.Dtos;
using AutoMapper;

namespace Api.Mapper;

public class MessageDtoProfile : Profile
{
    public MessageDtoProfile()
    {
        // Mapping for MessageDto to itself (if needed)
        CreateMap<MessageDto, MessageDto>();
    }
}
