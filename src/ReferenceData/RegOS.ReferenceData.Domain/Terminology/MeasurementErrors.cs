namespace RegOS.ReferenceData.Domain.Terminology;

public static class MeasurementErrors
{
    /// <remarks>
    /// Lists the accepted codes, as every vocabulary refusal in RegOS does — a
    /// caller who sent the wrong one cannot guess the right one.
    /// </remarks>
    public static string UnknownUnit(string? code)
        => $"\"{code}\" is not a unit RegOS knows. "
            + $"Accepted: {string.Join(", ", MeasurementVocabulary.Units.Select(x => x.Code))}.";
}
