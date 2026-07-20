namespace RegOS.Product.Domain.Product;

/// <summary>
/// A product's lifecycle. There are exactly two states, and that is the whole
/// state machine:
/// </summary>
/// <remarks>
/// <code>
///        Register()
///            │
///            ▼
///       Registered
///            │
///        Archive()          (one-way; archiving again is a conflict)
///            ▼
///        Archived
/// </code>
/// <para>
/// Two states is not an omission. An <c>Active</c> member existed here until
/// PROD-007 and was unreachable: nothing ever transitioned a product into it,
/// so no product was ever active and no code could branch on it. It described a
/// domain we had imagined rather than the one we built.
/// </para>
/// <para>
/// If a regulatory review workflow later makes products active only once
/// approved, reintroduce the state together with the transition that reaches
/// it. A state with no transition into it is speculation, not modelling.
/// </para>
/// </remarks>
public enum ProductStatus
{
    /// <summary>Registered and usable for new regulatory work.</summary>
    Registered = 1,

    /// <summary>
    /// Retired from new work. The product stays readable and every existing
    /// application, submission and document that references it is unaffected -
    /// archiving is not a delete.
    /// </summary>
    Archived = 3
}
