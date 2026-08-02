using Microsoft.Extensions.DependencyInjection;

using RegOS.Persistence;
using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Commands.PlaceSubmissionDocument;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Commands.RemoveProductDocument;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Queries.GetSubmissionChanges;
using RegOS.Submission.Application.Queries.GetSubmissionContentPlan;
using RegOS.Submission.Application.Queries.ListAttachableProductDocuments;
using RegOS.Submission.Application.Queries.ListProductDocumentUsage;
using RegOS.Submission.Application.Queries.ListSubmissionDocuments;
using RegOS.Submission.Application.Queries.ListSubmissions;
using RegOS.Submission.Application.Queries.ValidateSubmission;
using RegOS.Submission.Application.Validation;

namespace RegOS.Submission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSubmissionApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateSubmissionHandler>();

        services.AddScoped<AttachProductDocumentHandler>();

        services.AddScoped<RemoveProductDocumentHandler>();

        services.AddScoped<PlaceSubmissionDocumentHandler>();

        services.AddScoped<GetSubmissionContentPlanHandler>();

        services.AddScoped<GetSubmissionChangesHandler>();

        services.AddScoped<ListSubmissionsHandler>();

        services.AddScoped<GetSubmissionHandler>();

        services.AddScoped<ListSubmissionDocumentsHandler>();

        services.AddScoped<ListAttachableProductDocumentsHandler>();

        services.AddScoped<ListProductDocumentUsageHandler>();

        // Composed from the evaluator registry rather than from a second list
        // of registrations kept in step by hand. Constructed explicitly so a
        // missing registration can never resolve to an engine with no
        // evaluators that silently reports every rule as unevaluated.
        services.AddScoped(sp =>
            new BlueprintValidationEvaluator(sp.GetRequiredService<RegOSDbContext>()));

        services.AddScoped<SubmissionValidator>();

        services.AddScoped<ValidateSubmissionHandler>();

        services.AddScoped<PublishSubmissionHandler>();


        return services;
    }
}
