namespace RegOS.Api.Endpoints.MedicinalProducts;

/// <param name="On">
/// The business date this record left or returned to normal work — supplied,
/// not taken from the clock.
/// </param>
/// <remarks>
/// One request shape for both directions: activation and deactivation carry the
/// same single fact, and two identical records named differently would be
/// symmetry for its own sake.
/// </remarks>
public sealed record MedicinalProductActivationRequest(DateOnly On);
