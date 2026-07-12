using RegulatoryApplicationId = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplicationId;

namespace RegOS.RegulatoryApplication.Application.Commands.RegisterApplication;

public sealed record RegisterApplicationResult(
    RegulatoryApplicationId Id);