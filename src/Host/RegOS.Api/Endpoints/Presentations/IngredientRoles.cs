using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Presentations;

/// <summary>
/// Reads the role off the wire.
/// </summary>
/// <remarks>
/// Parsed at the edge rather than bound by the framework so an unrecognised
/// word is a business refusal naming what was expected, not a 400 with a
/// serializer's wording.
/// </remarks>
internal static class IngredientRoles
{
    public static IngredientRole Parse(string? value)
        => Enum.TryParse<IngredientRole>(value, ignoreCase: true, out var role)
            ? role
            : throw new DomainException(
                $"\"{value}\" is not an ingredient role. "
                + $"Accepted: {string.Join(", ", Enum.GetNames<IngredientRole>())}.");
}
