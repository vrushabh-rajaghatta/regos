using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Persistence.Initialization.ReferenceData.Geography;

/// <summary>
/// The eight jurisdictions RegOS demonstrates against.
/// </summary>
/// <remarks>
/// <b>Demonstration seed data only. These records intentionally do not
/// represent an authoritative geography or membership register.</b> The same
/// statement <c>Substance</c> carries, and for the same reason: eight
/// hand-verified rows and a licensed register are different evidence levels,
/// and only one of them can be widened without going and fetching something
/// (<see href="../../../../../docs/evidence/EPIC-022/iso-3166-1.md">E36</see>,
/// <see href="../../../../../docs/evidence/EPIC-022/regional-membership.md">E37</see>).
/// <para>
/// <b>Every value below was read off a published source, not inferred.</b> The
/// alpha-3 code and the ISO name are not derivable from the alpha-2 code or the
/// common name; neither is membership. Two rows contradict what a careful guess
/// would have produced — <b>Australia and India are ICH <em>observers</em>, not
/// members</b> — which is the whole reason the lists were fetched.
/// </para>
/// </remarks>
internal static class Countries
{
    private static CodedConcept Region(string code)
        => GeographyVocabulary.RegionOf(code)!;

    /// <summary>
    /// <b>Germany and France are tagged ICH by inheritance, and that is a
    /// derived claim rather than a register row.</b> The European Commission is
    /// the ICH member, not its member states — but ICH guidelines are adopted
    /// in Germany and France through the EU, and <em>"do ICH guidelines apply
    /// here?"</em> is the question this field exists to answer. Said here so a
    /// future reader does not go looking for Germany on ICH's member list.
    /// </summary>
    private static IReadOnlyList<CodedConcept> EuMemberState =>
        [Region("EU"), Region("ICH"), Region("PIC_S")];

    public static IReadOnlyList<Country> Data =>
    [
        // FDA — ICH Founding Regulatory Member; PIC/S since 2011.
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.UnitedStates),
            "US",
            "USA",
            "United States",
            "United States of America",
            [Region("ICH"), Region("PIC_S")]),

        // Health Canada — ICH Standing Regulatory Member; PIC/S since 1999.
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Canada),
            "CA",
            "CAN",
            "Canada",
            "Canada",
            [Region("ICH"), Region("PIC_S")]),

        // MHRA — an ICH Regulatory Member in its own right, and PIC/S since
        // 1999. **Not EU**, which is the row that shows why membership is a
        // dated fact rather than geography.
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.UnitedKingdom),
            "GB",
            "GBR",
            "United Kingdom",
            "United Kingdom of Great Britain and Northern Ireland",
            [Region("ICH"), Region("PIC_S")]),

        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Germany),
            "DE",
            "DEU",
            "Germany",
            "Germany",
            EuMemberState),

        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.France),
            "FR",
            "FRA",
            "France",
            "France",
            EuMemberState),

        // MHLW/PMDA — ICH Founding Regulatory Member; PIC/S since 2014.
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Japan),
            "JP",
            "JPN",
            "Japan",
            "Japan",
            [Region("ICH"), Region("PIC_S")]),

        // TGA — PIC/S since 1995, and an ICH **Standing Observer**, so no ICH.
        // The correction a guess would not have made.
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Australia),
            "AU",
            "AUS",
            "Australia",
            "Australia",
            [Region("PIC_S")]),

        // CDSCO is an ICH **observer**, and India is **not** a PIC/S
        // participant — so India belongs to none of the five. An empty
        // collection is the recorded answer, not an unfilled field.
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.India),
            "IN",
            "IND",
            "India",
            "India",
            [])
    ];
}
