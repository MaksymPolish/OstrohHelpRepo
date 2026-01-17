using Domain.Users.Roles;
using Domain.Inventory.Statuses;
using Domain.Conferences.Statuses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public static class Seeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Roles
            modelBuilder.Entity<Role>().HasData(
                Role.Create(new Domain.Users.Roles.RoleId(Guid.Parse("00000000-0000-0000-0000-000000000001")), "Студент"),
                Role.Create(new Domain.Users.Roles.RoleId(Guid.Parse("00000000-0000-0000-0000-000000000002")), "Психолог")
            );

            // QuestionaryStatuses
            modelBuilder.Entity<QuestionaryStatuses>().HasData(
                QuestionaryStatuses.Create(new Domain.Inventory.Statuses.questionaryStatusId(Guid.Parse("00000000-0000-0000-0000-000000000011")), "Принято")
            );

            // ConsultationStatuses
            modelBuilder.Entity<ConsultationStatuses>().HasData(
                ConsultationStatuses.Create(new Domain.Conferences.Statuses.ConsultationStatusesId(Guid.Parse("00000000-0000-0000-0000-000000000021")), "Назначено")
            );
        }
    }
}