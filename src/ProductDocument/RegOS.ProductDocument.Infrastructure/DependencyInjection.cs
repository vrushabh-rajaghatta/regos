using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.ProductDocument.Application.Storage;
using RegOS.ProductDocument.Domain.Repositories;
using RegOS.ProductDocument.Infrastructure.Repositories;
using RegOS.ProductDocument.Infrastructure.Storage;

namespace RegOS.ProductDocument.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductDocumentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IProductDocumentRepository, ProductDocumentRepository>();

        var rootPath = configuration["Storage:RootPath"];
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            rootPath = "storage";
        }

        services.AddScoped<IFileStorage>(_ => new LocalFileStorage(rootPath));

        return services;
    }
}
