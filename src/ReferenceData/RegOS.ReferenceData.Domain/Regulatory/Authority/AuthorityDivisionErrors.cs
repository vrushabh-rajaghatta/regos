namespace RegOS.ReferenceData.Domain.Regulatory.Authority;

public static class AuthorityDivisionErrors
{
    public const string AuthorityRequired =
        "An authority division must belong to an authority.";

    public const string NameRequired =
        "A division name is required.";

    public static readonly string NameTooLong =
        $"A division name cannot exceed {AuthorityDivision.NameMaxLength} characters.";
}
