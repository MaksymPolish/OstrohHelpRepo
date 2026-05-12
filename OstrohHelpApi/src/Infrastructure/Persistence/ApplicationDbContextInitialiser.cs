using Microsoft.EntityFrameworkCore;
using Domain.Users.Roles;
using Domain.Inventory.Statuses;
using Domain.Conferences.Statuses;

namespace Infrastructure.Persistence;

public class ApplicationDbContextInitialiser(ApplicationDbContext context)
{
    public async Task InitializeAsync()
    {
        await context.Database.MigrateAsync();
        await SeedDataAsync();
    }

    private async Task SeedDataAsync()
    {
        // Seed Roles if they don't exist
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                Role.Create(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Student"),
                Role.Create(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Psychologist"),
                Role.Create(Guid.Parse("00000000-0000-0000-0000-000000000003"), "HeadOfService")
            };
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
            Console.WriteLine("✓ Roles seeded successfully");
        }

        // Seed QuestionaryStatuses if they don't exist
        if (!await context.Set<QuestionaryStatuses>().AnyAsync())
        {
            var questionaryStatuses = new[]
            {
                QuestionaryStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000011"), "Принято"),
                QuestionaryStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000012"), "Відхилено"),
                QuestionaryStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000013"), "Очікує підтвердження")
            };
            context.Set<QuestionaryStatuses>().AddRange(questionaryStatuses);
            await context.SaveChangesAsync();
            Console.WriteLine("✓ Questionary statuses seeded successfully");
        }

        // Seed ConsultationStatuses if they don't exist
        if (!await context.Set<ConsultationStatuses>().AnyAsync())
        {
            var consultationStatuses = new[]
            {
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000021"), "Назначено"),
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000022"), "Відхилено"),
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000023"), "Завершено"),
                ConsultationStatuses.Create(Guid.Parse("00000000-0000-0000-0000-000000000024"), "Очікує підтвердження")
            };
            context.Set<ConsultationStatuses>().AddRange(consultationStatuses);
            await context.SaveChangesAsync();
            Console.WriteLine("✓ Consultation statuses seeded successfully");
        }
    }
}