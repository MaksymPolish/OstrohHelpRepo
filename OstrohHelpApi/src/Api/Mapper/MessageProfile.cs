using Api.Dtos;
using AutoMapper;
using Domain.Messages;

namespace Api.Mapper;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<MessageAttachment, MessageAttachmentDto>();
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.FullNameSender, opt => opt.MapFrom(src => src.SenderId.ToString()))
            .ForMember(dest => dest.FullNameReceiver, opt => opt.MapFrom(src => src.ReceiverId.ToString()))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));
    }
}