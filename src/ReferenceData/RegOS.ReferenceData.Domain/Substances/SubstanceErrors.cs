namespace RegOS.ReferenceData.Domain.Substances;

public static class SubstanceErrors
{
    public const string TenantRequired =
        "A proprietary substance must belong to a tenant.";

    public const string NameRequired =
        "A substance name is required.";

    public static readonly string NameTooLong =
        $"A substance name cannot exceed {Substance.NameMaxLength} characters.";

    public static readonly string InnTooLong =
        $"An INN cannot exceed {Substance.NameMaxLength} characters.";

    public const string ClassRequired =
        "A substance must have a class.";

    public const string TypeRequired =
        "A substance must have a type.";

    public static readonly string CasNumberTooLong =
        $"A CAS number cannot exceed {Substance.IdentifierMaxLength} characters.";

    public static readonly string UniiCodeTooLong =
        $"A UNII code cannot exceed {Substance.IdentifierMaxLength} characters.";

    public static readonly string MolecularFormulaTooLong =
        $"A molecular formula cannot exceed {Substance.MolecularFormulaMaxLength} characters.";

    public static readonly string DescriptionTooLong =
        $"A description cannot exceed {Substance.DescriptionMaxLength} characters.";

    /// <remarks>
    /// Names the catalogue the clash is in, because the two cases need
    /// different actions from the user: a shared row means <em>use the one
    /// that is already there</em>, their own means <em>you added this
    /// already</em>.
    /// </remarks>
    public const string NameAlreadyInSharedCatalogue =
        "That substance is already in the shared catalogue. "
        + "Use the existing one rather than adding a second.";

    public const string NameAlreadyAdded =
        "Your organisation has already added a substance with that name.";
}
