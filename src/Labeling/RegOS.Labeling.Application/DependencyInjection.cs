using Microsoft.Extensions.DependencyInjection;

using RegOS.Labeling.Application.Commands.AttachGlobalLabelContent;
using RegOS.Labeling.Application.Commands.CreateLocalLabel;
using RegOS.Labeling.Application.Commands.DiscardLocalLabelDraft;
using RegOS.Labeling.Application.Commands.PrepareLocalLabelRevision;
using RegOS.Labeling.Application.Commands.PublishLocalLabelRevision;
using RegOS.Labeling.Application.Commands.StartLocalLabelRevision;
using RegOS.Labeling.Application.Queries.ListCoreVersionsForProduct;
using RegOS.Labeling.Application.Queries.ListLocalLabelRevisions;
using RegOS.Labeling.Application.Queries.ListLocalLabels;
using RegOS.Labeling.Application.Commands.CreateGlobalLabel;
using RegOS.Labeling.Application.Commands.DiscardGlobalLabelDraft;
using RegOS.Labeling.Application.Commands.PublishGlobalLabelVersion;
using RegOS.Labeling.Application.Commands.StartGlobalLabelDraft;
using RegOS.Labeling.Application.Queries.ListGlobalLabelVersions;
using RegOS.Labeling.Application.Queries.ListGlobalLabels;

namespace RegOS.Labeling.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLabelingApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateGlobalLabelHandler>();

        services.AddScoped<StartGlobalLabelDraftHandler>();

        services.AddScoped<AttachGlobalLabelContentHandler>();

        services.AddScoped<PublishGlobalLabelVersionHandler>();

        services.AddScoped<DiscardGlobalLabelDraftHandler>();

        services.AddScoped<ListGlobalLabelsHandler>();

        services.AddScoped<ListGlobalLabelVersionsHandler>();

        services.AddScoped<CreateLocalLabelHandler>();

        services.AddScoped<StartLocalLabelRevisionHandler>();

        services.AddScoped<PrepareLocalLabelRevisionHandler>();

        services.AddScoped<PublishLocalLabelRevisionHandler>();

        services.AddScoped<DiscardLocalLabelDraftHandler>();

        services.AddScoped<ListLocalLabelsHandler>();

        services.AddScoped<ListLocalLabelRevisionsHandler>();

        services.AddScoped<ListCoreVersionsForProductHandler>();

        return services;
    }
}
