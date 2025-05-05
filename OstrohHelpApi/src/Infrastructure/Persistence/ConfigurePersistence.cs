using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.Persistence;

public static class ConfigurePersistence
{
    public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSourceBuild = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Default"));
        dataSourceBuild.EnableDynamicJson();
        var dataSource = dataSourceBuild.Build();

        services.AddDbContext<ApplicationDbContext>(
            options => options
                .UseNpgsql(
                    dataSource,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserQuery, UserRepository>();
        
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
        services.AddScoped<IQuestionnaireQuery, QuestionnaireRepository>();
        
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IConsultationQuery, ConsultationRepository>();
        
        services.AddScoped<IQuestionnaireStatusQuery, QuestionnaireStatusRepository>();
        services.AddScoped<IQuestionnaireStatusQuery, QuestionnaireStatusRepository>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleQuery, RoleRepository>();
    }
}