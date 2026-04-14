using Api.Dtos;
using AutoMapper;
using Domain.Messages;

namespace Api.Mapper;

public class MessageDtoProfile : Profile
{
    public MessageDtoProfile()
    {
        // Map Message domain model to MessageDto
        // Converts encrypted byte arrays to base64-encoded strings for transmission
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.ConsultationId, 
                opt => opt.MapFrom(src => src.ConsultationId.Value.ToString()))
            .ForMember(dest => dest.SenderId, 
                opt => opt.MapFrom(src => src.SenderId.Value.ToString()))
            .ForMember(dest => dest.ReceiverId, 
                opt => opt.MapFrom(src => src.ReceiverId.Value.ToString()))
            .ForMember(dest => dest.EncryptedContent,
                opt => opt.MapFrom(src => src.EncryptedContent != null && src.EncryptedContent.Length > 0
                    ? Convert.ToBase64String(src.EncryptedContent)
                    : null))
            .ForMember(dest => dest.Iv,
                opt => opt.MapFrom(src => src.Iv != null && src.Iv.Length > 0
                    ? Convert.ToBase64String(src.Iv)
                    : null))
            .ForMember(dest => dest.AuthTag,
                opt => opt.MapFrom(src => src.AuthTag != null && src.AuthTag.Length > 0
                    ? Convert.ToBase64String(src.AuthTag)
                    : null));
    }
}
