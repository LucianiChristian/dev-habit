using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DevHabit.Api.Database;

public static class ServiceCollectionExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsqlOptions => 
                    npgsqlOptions.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName, 
                        Schemas.Application))
                .UseSnakeCaseNamingConvention());
    }
}
