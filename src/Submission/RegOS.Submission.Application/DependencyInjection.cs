using Microsoft.Extensions.DependencyInjection;

using RegOS.Persistence;
using RegOS.Submission.Application.Commands.AssignSubmissionRole;
using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Commands.ChangeSubmissionFormat;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Commands.PlaceSubmissionDocument;
using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Commands.RemoveProductDocument;
using RegOS.Submission.Application.Commands.RemoveSubmissionRole;
using RegOS.Submission.Application.Commands.ReportStudyOnPlacement;
using RegOS.Submission.Application.Queries.GetApplicationContacts;
using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Application.Queries.GetSubmissionChanges;
using RegOS.Submission.Application.Queries.GetSubmissionContentPlan;
using RegOS.Submission.Application.Queries.ListAttachableProductDocuments;
using RegOS.Submission.Application.Queries.ListProductDocumentUsage;
using RegOS.Submission.Application.Queries.ListSubmissionDocuments;
using RegOS.Submission.Application.Queries.ListSubmissionRoles;
using RegOS.Submission.Application.Queries.ListSubmissions;
using RegOS.Submission.Application.Queries.ListContinuableSubmissions;
using RegOS.Submission.Application.Generation;
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

        services.AddScoped<ReportStudyOnPlacementHandler>();

        services.AddScoped<ChangeSubmissionFormatHandler>();

        services.AddScoped<AssignSubmissionRoleHandler>();

        services.AddScoped<RemoveSubmissionRoleHandler>();

        services.AddScoped<ListSubmissionRolesHandler>();

        services.AddScoped<GetApplicationContactsHandler>();

        services.AddScoped<GetSubmissionContentPlanHandler>();

        services.AddScoped<GetSubmissionChangesHandler>();

        services.AddScoped<ListSubmissionsHandler>();
        services.AddScoped<ListContinuableSubmissionsHandler>();

        // The first RegOS code that produces part of an eCTD package (S004).
        services.AddScoped<SequenceFolderGenerator>();
        services.AddScoped<SequencePackageAssembler>();

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
