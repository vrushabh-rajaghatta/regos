using Microsoft.Extensions.DependencyInjection;

using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;

namespace RegOS.Submission.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSubmissionInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        services.AddScoped<ISubmissionSnapshotRepository, SubmissionSnapshotRepository>();

        return services;
    }
}
