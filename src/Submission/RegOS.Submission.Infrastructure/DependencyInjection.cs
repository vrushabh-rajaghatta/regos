using Microsoft.Extensions.DependencyInjection;

using RegOS.Submission.Application.Services;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;
using RegOS.Submission.Infrastructure.Services;

namespace RegOS.Submission.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSubmissionInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();

        services.AddScoped<ISubmissionSnapshotRepository, SubmissionSnapshotRepository>();

        services.AddScoped<ISubmissionNumberingPolicy, SubmissionNumberingPolicy>();

        return services;
    }
}
