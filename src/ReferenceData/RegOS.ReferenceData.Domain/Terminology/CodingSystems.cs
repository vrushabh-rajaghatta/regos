namespace RegOS.ReferenceData.Domain.Terminology;

/// <summary>
/// The naming authorities a <see cref="CodedConcept"/> can quote.
/// </summary>
/// <remarks>
/// <b>One constant, deliberately.</b> EDQM, WHO ATC and GSRS/UNII are the
/// systems RegOS expects to quote later, and naming them here would put unheld
/// vocabularies in code as though they were held — the exact failure EPIC-019
/// hit and ADR-058 §6 is written to prevent. They arrive with the data.
/// </remarks>
public static class CodingSystems
{
    /// <summary>
    /// RegOS's own curated terminology: sufficient for demonstration and early
    /// use, and <b>not</b> a claim to be EDQM, WHO ATC, GSRS or ISO 11238
    /// (ADR-058 §6, EPIC-010a D1).
    /// </summary>
    public const string RegosInternal = "regos-internal";
}
