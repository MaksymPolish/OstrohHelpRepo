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
                Role.Create(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Student"),
                Role.Create(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Psychologist"),
                Role.Create(Guid.Parse("00000000-0000-0000-0000-000000000003"), "HeadOfService")
            );

            // QuestionaryStatuses
            modelBuilder.Entity<QuestionaryStatuses>().HasData(
                QuestionaryStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000011"), "Принято"),
                QuestionaryStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000012"), "Відхилено"),
                QuestionaryStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000013"), "Очікує підтвердження")
            );

            // ConsultationStatuses
            modelBuilder.Entity<ConsultationStatuses>().HasData(
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000021"), "Назначено"),
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000022"), "Відхилено"),
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000023"), "Завершено"),
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000024"), "Очікує підтвердження")
            );
        }
    }
}