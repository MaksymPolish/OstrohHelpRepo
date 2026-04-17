using Api.Dtos;
using AutoMapper;
using Domain.Messages;

namespace Api.Mapper;

public class MessageAttachmentDtoProfile : Profile
{
    public MessageAttachmentDtoProfile()
    {
        // Map MessageAttachment domain model to MessageAttachmentDto
        // Preserves preview URLs that were generated during upload
        CreateMap<MessageAttachment, MessageAttachmentDto>()
            .ForMember(dest => dest.ThumbnailUrl, 
                opt => opt.MapFrom(src => src.ThumbnailUrl))
            .ForMember(dest => dest.MediumPreviewUrl, 
                opt => opt.MapFrom(src => src.MediumPreviewUrl))
            .ForMember(dest => dest.VideoPosterUrl, 
                opt => opt.MapFrom(src => src.VideoPosterUrl))
            .ForMember(dest => dest.PdfPagePreviewUrl, 
                opt => opt.MapFrom(src => src.PdfPagePreviewUrl));
    }
}
