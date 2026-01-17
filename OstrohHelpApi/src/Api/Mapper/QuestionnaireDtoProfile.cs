using Api.Dtos;
using AutoMapper;

namespace Api.Mapper;

public class QuestionnaireDtoProfile : Profile
{
    public QuestionnaireDtoProfile()
    {
        // Mapping for QuestionnaireDto to itself (if needed)
        CreateMap<QuestionnaireDto, QuestionnaireDto>();
    }
}
