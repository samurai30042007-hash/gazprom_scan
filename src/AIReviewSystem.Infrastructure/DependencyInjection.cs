using AIReviewSystem.Application.Abstractions;
using AIReviewSystem.Application.Abstractions.Repositories;
using AIReviewSystem.Infrastructure.Persistence;
using AIReviewSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIReviewSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<ReviewDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IAnalysisSessionRepository, EfAnalysisSessionRepository>();
        services.AddScoped<IReportArtifactRepository, EfReportArtifactRepository>();
        services.AddScoped<IStaticFindingRepository, EfStaticFindingRepository>();

        return services;
    }
}