using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

public class ConsultationStatusRepository(ApplicationDbContext context) : IConsultationStatusQuery, IConsultationStatusRepository
{
    
}