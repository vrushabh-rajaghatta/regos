using Microsoft.Extensions.DependencyInjection;

using RegOS.Interaction.Application.Commands.AttachCorrespondenceContent;
using RegOS.Interaction.Application.Commands.AssignQuestion;
using RegOS.Interaction.Application.Commands.BeginInspection;
using RegOS.Interaction.Application.Commands.BeginMeeting;
using RegOS.Interaction.Application.Commands.ChangeInspectionStatus;
using RegOS.Interaction.Application.Commands.RecordInspectionFindings;
using RegOS.Interaction.Application.Commands.ChangeMeetingStatus;
using RegOS.Interaction.Application.Commands.RecordMeetingOutcome;
using RegOS.Interaction.Application.Commands.ChangeCommitmentStatus;
using RegOS.Interaction.Application.Commands.GiveCommitment;
using RegOS.Interaction.Application.Commands.RaiseQuestion;
using RegOS.Interaction.Application.Commands.RecordCorrespondence;
using RegOS.Interaction.Application.Commands.ResolveQuestion;
using RegOS.Interaction.Application.Commands.RespondToQuestion;
using RegOS.Interaction.Application.Commands.RemoveCorrespondenceContent;
using RegOS.Interaction.Application.Commands.AttachCommitmentToStep;
using RegOS.Interaction.Application.Commands.AttachHaCorrespondenceToStep;
using RegOS.Interaction.Application.Commands.AttachHaMeetingToStep;
using RegOS.Interaction.Application.Commands.AttachInspectionToStep;
using RegOS.Interaction.Application.Queries.GetCorrespondenceContent;
using RegOS.Interaction.Application.Queries.GetCorrespondence;
using RegOS.Interaction.Application.Queries.ListCommitments;
using RegOS.Interaction.Application.Queries.ListCorrespondence;
using RegOS.Interaction.Application.Queries.ListDueWork;
using RegOS.Interaction.Application.Queries.ListInspections;
using RegOS.Interaction.Application.Queries.ListMeetings;

namespace RegOS.Interaction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInteractionApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RecordCorrespondenceHandler>();

        services.AddScoped<AttachCorrespondenceContentHandler>();

        services.AddScoped<RemoveCorrespondenceContentHandler>();

        services.AddScoped<GetCorrespondenceContentHandler>();

        services.AddScoped<RaiseQuestionHandler>();

        services.AddScoped<RespondToQuestionHandler>();

        services.AddScoped<ResolveQuestionHandler>();

        services.AddScoped<AssignQuestionHandler>();

        services.AddScoped<GiveCommitmentHandler>();

        services.AddScoped<ChangeCommitmentStatusHandler>();

        services.AddScoped<ListCommitmentsHandler>();

        services.AddScoped<ListDueWorkHandler>();

        services.AddScoped<BeginMeetingHandler>();

        services.AddScoped<ChangeMeetingStatusHandler>();

        services.AddScoped<RecordMeetingOutcomeHandler>();

        services.AddScoped<ListMeetingsHandler>();

        services.AddScoped<BeginInspectionHandler>();

        services.AddScoped<ChangeInspectionStatusHandler>();

        services.AddScoped<RecordInspectionFindingsHandler>();

        services.AddScoped<ListInspectionsHandler>();

        services.AddScoped<ListCorrespondenceHandler>();

        services.AddScoped<GetCorrespondenceHandler>();

        // ADR-065 S007 — all four, following the shape S006 established.
        services.AddScoped<AttachHaCorrespondenceToStepHandler>();
        services.AddScoped<AttachHaMeetingToStepHandler>();
        services.AddScoped<AttachInspectionToStepHandler>();
        services.AddScoped<AttachCommitmentToStepHandler>();

        return services;
    }
}
