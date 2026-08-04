using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.ReferenceData.Application.Queries.Measurement.ListMeasurementUnits;

public sealed class ListMeasurementUnitsHandler
{
    public Task<IReadOnlyList<MeasurementUnitDto>> HandleAsync(
        ListMeasurementUnitsQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MeasurementUnitDto> units =
        [
            .. MeasurementVocabulary.Units.Select(
                unit => new MeasurementUnitDto(unit.System, unit.Code, unit.Display))
        ];

        return Task.FromResult(units);
    }
}
