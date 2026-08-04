using Microsoft.Extensions.DependencyInjection;

using RegOS.Labeling.Application.Commands.AttachGlobalLabelContent;
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

        return services;
    }
}
