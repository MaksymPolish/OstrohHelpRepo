using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

public class QuestionnaireRepository(ApplicationDbContext context) : IQuestionnaireQuery, IQuestionnaireRepository
{
    
}