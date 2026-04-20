using Api.Dtos;
using AutoMapper;
using Domain.Messages;

namespace Api.Mapper;

public class MessageDtoProfile : Profile
{
    public MessageDtoProfile()
    {
        // MessageAttachment mapping
        CreateMap<MessageAttachment, MessageAttachmentDto>();
        
        // Map Message domain model to MessageDto
        // Converts encrypted byte arrays to base64-encoded strings for transmission
        CreateMap<Message, MessageDto>()
            // String IDs
            .ForMember(dest => dest.ConsultationId, 
                opt => opt.MapFrom(src => src.ConsultationId.ToString()))
            .ForMember(dest => dest.SenderId, 
                opt => opt.MapFrom(src => src.SenderId.ToString()))
            .ForMember(dest => dest.ReceiverId, 
                opt => opt.MapFrom(src => src.ReceiverId.ToString()))
            
            // Encrypted fields: byte[] → Base64 string
            .ForMember(dest => dest.EncryptedContent,
                opt => opt.MapFrom(src => 
                    src.EncryptedContent == null || src.EncryptedContent.Length == 0
                        ? null
                        : Convert.ToBase64String(src.EncryptedContent)))
            .ForMember(dest => dest.Iv,
                opt => opt.MapFrom(src => 
                    src.Iv == null || src.Iv.Length == 0
                        ? null
                        : Convert.ToBase64String(src.Iv)))
            .ForMember(dest => dest.AuthTag,
                opt => opt.MapFrom(src => 
                    src.AuthTag == null || src.AuthTag.Length == 0
                        ? null
                        : Convert.ToBase64String(src.AuthTag)))
            
            // Attachments collection
            .ForMember(dest => dest.Attachments,
                opt => opt.MapFrom(src => src.Attachments))
            
            // Ignore these properties - they're set manually in controller/hub
            .ForMember(dest => dest.FullNameSender, opt => opt.Ignore())
            .ForMember(dest => dest.FullNameReceiver, opt => opt.Ignore());
    }
}

