namespace RegOS.ReferenceData.Application.Queries.Measurement.ListMeasurementUnits;

/// <summary>
/// The units a quantity may be measured in.
/// </summary>
/// <remarks>
/// <b>Its own endpoint, not a field on the presentation vocabulary.</b> Mixing
/// them into one payload is the first step towards one picker offering both
/// <em>vial</em> and <em>mL</em> — and a strength that could name an article
/// would state what the presentation already says (EPIC-010a S003).
/// </remarks>
public sealed record ListMeasurementUnitsQuery();
