using Microsoft.Extensions.DependencyInjection;

using RegOS.Labeling.Application.Commands.AttachGlobalLabelContent;
using RegOS.Labeling.Application.Commands.CreateLocalLabel;
using RegOS.Labeling.Application.Commands.DiscardLocalLabelDraft;
using RegOS.Labeling.Application.Commands.PrepareLocalLabelRevision;
using RegOS.Labeling.Application.Commands.PublishLocalLabelRevision;
using RegOS.Labeling.Application.Commands.StartLocalLabelRevision;
using RegOS.Labeling.Application.Commands.RecordIndication;
using RegOS.Labeling.Application.Commands.RestateIndicationText;
using RegOS.Labeling.Application.Commands.RecordIndicationDecision;
using RegOS.Labeling.Application.Commands.AddIndicationPopulation;
using RegOS.Labeling.Application.Commands.AmendIndicationPopulation;
using RegOS.Labeling.Application.Commands.RemoveIndicationPopulation;
using RegOS.Labeling.Application.Commands.AddIndicationTherapy;
using RegOS.Labeling.Application.Commands.RemoveIndicationTherapy;
using RegOS.Labeling.Application.Queries.ListCoreVersionsForProduct;
using RegOS.Labeling.Application.Commands.RecordContraindication;
using RegOS.Labeling.Application.Commands.RestateContraindicationText;
using RegOS.Labeling.Application.Commands.AddContraindicationPopulation;
using RegOS.Labeling.Application.Commands.AmendContraindicationPopulation;
using RegOS.Labeling.Application.Commands.RemoveContraindicationPopulation;
using RegOS.Labeling.Application.Commands.RecordUndesirableEffect;
using RegOS.Labeling.Application.Commands.RestateUndesirableEffectText;
using RegOS.Labeling.Application.Commands.AddUndesirableEffectPopulation;
using RegOS.Labeling.Application.Commands.AmendUndesirableEffectPopulation;
using RegOS.Labeling.Application.Commands.RemoveUndesirableEffectPopulation;
using RegOS.Labeling.Application.Queries.ListContraindications;
using RegOS.Labeling.Application.Queries.ListIndications;
using RegOS.Labeling.Application.Queries.ListUndesirableEffects;
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

        services.AddScoped<RecordIndicationHandler>();

        services.AddScoped<RestateIndicationTextHandler>();

        services.AddScoped<RecordIndicationDecisionHandler>();

        services.AddScoped<AddIndicationPopulationHandler>();

        services.AddScoped<AmendIndicationPopulationHandler>();

        services.AddScoped<RemoveIndicationPopulationHandler>();

        services.AddScoped<AddIndicationTherapyHandler>();

        services.AddScoped<RemoveIndicationTherapyHandler>();

        services.AddScoped<ListIndicationsHandler>();

        services.AddScoped<RecordContraindicationHandler>();

        services.AddScoped<RestateContraindicationTextHandler>();

        services.AddScoped<AddContraindicationPopulationHandler>();

        services.AddScoped<AmendContraindicationPopulationHandler>();

        services.AddScoped<RemoveContraindicationPopulationHandler>();

        services.AddScoped<RecordUndesirableEffectHandler>();

        services.AddScoped<RestateUndesirableEffectTextHandler>();

        services.AddScoped<AddUndesirableEffectPopulationHandler>();

        services.AddScoped<AmendUndesirableEffectPopulationHandler>();

        services.AddScoped<RemoveUndesirableEffectPopulationHandler>();

        services.AddScoped<ListContraindicationsHandler>();

        services.AddScoped<ListUndesirableEffectsHandler>();

        return services;
    }
}
