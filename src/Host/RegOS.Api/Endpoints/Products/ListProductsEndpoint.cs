using RegOS.Product.Application.Queries.ListProducts;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public static class ListProductsEndpoint
{
    public static IEndpointRouteBuilder MapListProductsEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products", HandleAsync)
            .WithName("ListProducts")
            .WithSummary("List products")
            .WithTags("Product");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        ListProductsHandler handler,
        CancellationToken cancellationToken,
        string? search = null,
        string? type = null,
        string? status = null,
        int page = ListProductsQuery.DefaultPage,
        int pageSize = ListProductsQuery.DefaultPageSize)
    {
        // An unparseable filter is a malformed request, not an empty result.
        if (!TryParse<ProductType>(type, out var parsedType))
            return Problem(type, "product type");

        if (!TryParse<ProductStatus>(status, out var parsedStatus))
            return Problem(status, "product status");

        var result = await handler.HandleAsync(
            new ListProductsQuery(
                search, parsedType, parsedStatus, page, pageSize),
            cancellationToken);

        return Results.Ok(result);
    }

    private static bool TryParse<TEnum>(string? value, out TEnum? parsed)
        where TEnum : struct, Enum
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (!Enum.TryParse<TEnum>(value, ignoreCase: true, out var result))
            return false;

        parsed = result;
        return true;
    }

    private static IResult Problem(string? value, string label)
        => Results.Problem(
            detail: $"'{value}' is not a valid {label}.",
            statusCode: StatusCodes.Status400BadRequest);
}
