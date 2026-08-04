namespace RegOS.ReferenceData.Application.Queries.Measurement.ListMeasurementUnits;

public sealed record MeasurementUnitDto(
    string System,
    string Code,
    string Display);
