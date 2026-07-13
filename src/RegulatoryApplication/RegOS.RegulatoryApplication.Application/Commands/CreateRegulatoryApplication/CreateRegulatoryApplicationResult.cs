using RegulatoryApplicationId = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplicationId;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed record CreateRegulatoryApplicationResult(
    RegulatoryApplicationId Id);
