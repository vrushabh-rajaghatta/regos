using Microsoft.Extensions.DependencyInjection;

using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Queries.ListSubmissions;

namespace RegOS.Submission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSubmissionApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateSubmissionHandler>();

        services.AddScoped<ListSubmissionsHandler>();

        services.AddScoped<GetSubmissionHandler>();

        return services;
    }
}
