using Microsoft.Extensions.DependencyInjection;

using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Commands.PlaceSubmissionDocument;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Commands.RemoveProductDocument;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Queries.GetSubmissionContentPlan;
using RegOS.Submission.Application.Queries.GetSubmissionSnapshot;
using RegOS.Submission.Application.Queries.ListAttachableProductDocuments;
using RegOS.Submission.Application.Queries.ListProductDocumentUsage;
using RegOS.Submission.Application.Queries.ListSubmissionDocuments;
using RegOS.Submission.Application.Queries.ListSubmissions;
using RegOS.Submission.Application.Queries.ValidateSubmission;
using RegOS.Submission.Application.Validation;
using RegOS.Submission.Application.Validation.Rules;

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

        services.AddScoped<ListSubmissionsHandler>();

        services.AddScoped<GetSubmissionHandler>();

        services.AddScoped<ListSubmissionDocumentsHandler>();

        services.AddScoped<ListAttachableProductDocumentsHandler>();

        services.AddScoped<ListProductDocumentUsageHandler>();

        // Blueprint rule evaluators. Adding a rule type is one more line here
        // plus its evaluator — the orchestrator never changes.
        services.AddScoped<IBlueprintRuleEvaluator, FileFormatEvaluator>();

        services.AddScoped<BlueprintValidationEvaluator>();

        services.AddScoped<SubmissionValidator>();

        services.AddScoped<ValidateSubmissionHandler>();

        services.AddScoped<PublishSubmissionHandler>();

        services.AddScoped<GetSubmissionSnapshotHandler>();

        return services;
    }
}
