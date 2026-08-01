namespace RegOS.Api.Endpoints.Correspondence;

/// <param name="OwnerUserId">Null clears the assignment — unassigning is a real act.</param>
public sealed record AssignQuestionRequest(Guid? OwnerUserId);
