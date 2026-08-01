namespace RegOS.Interaction.Domain.Correspondence;

/// <summary>
/// Which way a piece of correspondence travelled.
/// </summary>
/// <remarks>
/// An enum rather than reference data because rules branch on it (ADR-038
/// decision 3): only an <see cref="Inbound"/> letter can carry a response due
/// date we owe, and the two directions are read as separate lists in every real
/// query — <em>"what have they asked us?"</em> is a different question from
/// <em>"what have we told them?"</em>.
/// <para>
/// RIM has <c>Correspondence Mode</c> and <c>Correspondence Action</c> but no
/// explicit direction, leaving it to be inferred from initiator and recipient
/// names. That inference is fragile and every real query starts with it, so
/// RegOS states it.
/// </para>
/// </remarks>
public enum CorrespondenceDirection
{
    /// <summary>From the authority to us.</summary>
    Inbound = 1,

    /// <summary>From us to the authority.</summary>
    Outbound = 2
}
