using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class AuthorityDivisions
{
    /// <summary>
    /// The well-known ones, deliberately partial.
    /// </summary>
    /// <remarks>
    /// This list is <b>not</b> trying to be complete, and could not be: FDA
    /// alone has around thirty review divisions and reorganises them. It seeds
    /// the units a pharma tenant meets first, and the tenant-augmentable shape
    /// exists precisely because the rest will never be here. If this list ever
    /// looks exhaustive, something has gone wrong.
    /// </remarks>
    public static IReadOnlyList<AuthorityDivision> Data =>
    [
        // FDA — CDER's centre and the offices above the review divisions.
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.FdaCder),
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "Center for Drug Evaluation and Research"),
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.FdaCber),
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "Center for Biologics Evaluation and Research"),
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.FdaOnd),
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "Office of New Drugs"),

        // Health Canada — the directorate a drug sponsor deals with.
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.HcTpd),
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada),
            "Therapeutic Products Directorate"),
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.HcBgtd),
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada),
            "Biologic and Radiopharmaceutical Drugs Directorate"),

        // TGA.
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.TgaPrescription),
            new AuthorityId(GeographyAndRegulatoryIds.TGA),
            "Prescription Medicines Authorisation Branch"),

        // MHRA.
        AuthorityDivision.Create(
            new AuthorityDivisionId(AuthorityDivisionIds.MhraLicensing),
            new AuthorityId(GeographyAndRegulatoryIds.MHRA),
            "Licensing Division")
    ];
}
