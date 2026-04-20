using Api.Dtos;
using AutoMapper;
using Domain.Messages;

namespace Api.Mapper;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        // Message → MessageDto mapping moved to MessageDtoProfile.cs
        // which handles encryption field conversion to Base64
        
        CreateMap<MessageAttachment, MessageAttachmentDto>();
    }
}