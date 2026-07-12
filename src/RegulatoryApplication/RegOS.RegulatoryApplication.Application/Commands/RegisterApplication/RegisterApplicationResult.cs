using ApplicationId = RegOS.RegulatoryApplication.Domain.Aggregates.Application.ApplicationId;

namespace RegOS.RegulatoryApplication.Application.Commands.RegisterApplication;

public sealed record RegisterApplicationResult(
    ApplicationId Id);