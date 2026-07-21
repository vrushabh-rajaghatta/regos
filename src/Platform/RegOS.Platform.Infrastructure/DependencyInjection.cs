using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Domain.Aggregates.UserCredential;
using RegOS.Platform.Infrastructure.Repositories;
using RegOS.Platform.Infrastructure.Services;

namespace RegOS.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<
            IUserCredentialRepository, UserCredentialRepository>();

        services.AddScoped<IUserPolicy, UserPolicy>();

        // Stateless and thread-safe, so a singleton. The framework hasher
        // allocates no per-request state.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
