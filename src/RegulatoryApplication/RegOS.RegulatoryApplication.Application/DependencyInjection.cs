using Microsoft.Extensions.DependencyInjection;
using RegOS.RegulatoryApplication.Application.Commands.CiteStudy;
using RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;
using RegOS.RegulatoryApplication.Application.Commands.RecordApplicationNumber;
using RegOS.RegulatoryApplication.Application.Commands.StopCitingStudy;
using RegOS.RegulatoryApplication.Application.Queries.Applications.GetApplication;
using RegOS.RegulatoryApplication.Application.Queries.Applications.ListApplicationStudies;
using RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

namespace RegOS.RegulatoryApplication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRegulatoryApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<CreateRegulatoryApplicationHandler>();
        services.AddScoped<RecordApplicationNumberHandler>();

        services.AddScoped<ListRegulatoryApplicationsHandler>();

        services.AddScoped<GetApplicationHandler>();

        services.AddScoped<CiteStudyHandler>();
        services.AddScoped<StopCitingStudyHandler>();
        services.AddScoped<ListApplicationStudiesHandler>();

        return services;
    }
}
