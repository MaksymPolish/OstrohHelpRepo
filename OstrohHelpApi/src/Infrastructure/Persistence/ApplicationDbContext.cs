using System.Reflection;
using Domain.Consultations;
using Domain.Consultations.Statuses;
using Domain.Messages;
using Domain.Questionnaires;
using Domain.Questionnaires.Statuses;
using Domain.Users;
using Domain.Users.Roles;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<QuestionnaireStatuses> QuestionnaireStatuses { get; set; }
    public DbSet<Questionnaire> Questionnaires { get; set; }
    public DbSet<ConsultationStatuses> ConsultationStatuses { get; set; }
    public DbSet<Consultations> Consultations { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}