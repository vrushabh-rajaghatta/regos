namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

public static class ProcessObjectiveErrors
{
    public const string TenantRequired =
        "An objective belongs to a tenant.";

    public const string ProductRequired =
        "An objective is about a product. Choose one.";

    public const string CountryRequired =
        "An objective is about a market. Choose a country.";

    public const string NameRequired =
        "Say what you are trying to achieve.";

    public const string NameTooLong =
        "That objective name is too long.";

    public const string RationaleTooLong =
        "That rationale is too long.";

    public const string NoteTooLong =
        "That note is too long.";

    // --- lifecycle -----------------------------------------------------------

    public const string AlreadyClosed =
        "This objective was achieved or abandoned. Record a new one rather than "
        + "reopening it.";

    public const string CannotReturnToProposed =
        "An objective that has been taken up cannot go back to proposed.";

    public const string AlreadyInThatStatus =
        "This objective is already in that state.";

    public const string HistoryOutOfOrder =
        "That date is before something already recorded on this objective.";

    // --- the confirmation seam (ADR-065 D8) ----------------------------------

    /// <summary>
    /// Raised by the <em>command handler</em>, never by the aggregate — see
    /// <see cref="ProcessObjective.ConfirmMarketRecord"/> for why the check
    /// cannot live in the domain (ADR-016).
    /// </summary>
    public const string MarketRecordIsForAnotherMarket =
        "That market record is for a different product or country than this "
        + "objective. The link confirms which record fulfils the objective; it "
        + "cannot change what the objective is about.";

    public const string MarketRecordNotFound =
        "That market record does not exist.";
}
