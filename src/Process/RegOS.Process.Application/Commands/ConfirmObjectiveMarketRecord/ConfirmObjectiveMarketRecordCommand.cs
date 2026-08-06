using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Product.Domain.Product;

namespace RegOS.Process.Application.Commands.ConfirmObjectiveMarketRecord;

/// <summary>
/// Records which market-local product fulfils an objective — or, with null,
/// clears the link.
/// </summary>
public sealed record ConfirmObjectiveMarketRecordCommand(
    ProcessObjectiveId Id,
    MedicinalProductId? MedicinalProductId);
