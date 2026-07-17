using Microsoft.Extensions.DependencyInjection;

using RegOS.ProductDocument.Application.Commands.ActivateProductDocument;
using RegOS.ProductDocument.Application.Commands.ArchiveProductDocument;
using RegOS.ProductDocument.Application.Commands.UploadProductDocument;
using RegOS.ProductDocument.Application.Queries.GetProductDocument;
using RegOS.ProductDocument.Application.Queries.ListProductDocuments;

namespace RegOS.ProductDocument.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductDocumentApplication(
        this IServiceCollection services)
    {
        services.AddScoped<UploadProductDocumentHandler>();

        services.AddScoped<ActivateProductDocumentHandler>();

        services.AddScoped<ArchiveProductDocumentHandler>();

        services.AddScoped<ListProductDocumentsHandler>();

        services.AddScoped<GetProductDocumentHandler>();

        return services;
    }
}
