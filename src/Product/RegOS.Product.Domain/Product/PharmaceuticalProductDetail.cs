using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// What a product physically <em>is</em> in one market — the administrable
/// form, the way it is given, and the article it is counted in. Screen word:
/// <b>Presentation</b>.
/// </summary>
/// <remarks>
/// <b>Its own root, not a child of <see cref="MedicinalProduct"/>, and this
/// supersedes EPIC-017's prediction.</b> That epic's change-case table said
/// strengths, dosage forms and presentations would be children of the market
/// tier; it was written before composition was designed, and EPIC-010a's Phase
/// 2 changed it with that detail in hand.
/// <para>
/// The deciding argument is the consistency boundary, not depth.
/// <b>Composition and commerce move on different clocks</b> — a formulation
/// changes through a variation, a market presence changes through launches,
/// withdrawals, trade names and licences — and that is the same argument that
/// separated <see cref="MedicinalProduct"/> from <c>Registration</c> one tier
/// up. As a child, this would also drag <c>Ingredient</c> into the market
/// aggregate, so every trade-name edit would load and re-save composition, and
/// every one of those loads would be one more <c>Include</c> that has to be
/// remembered. EPIC-019 has already paid for a forgotten one.
/// </para>
/// <para>
/// <b>A market may have several, and nothing constrains it.</b> 10 mg, 20 mg
/// and 40 mg tablets are one commercial presence, and making a tenant duplicate
/// the whole market — its trade names, its history, its licences — to record
/// the second strength would be the wrong shape. The same call the tier above
/// made on <c>(GlobalProductId, CountryId)</c>.
/// </para>
/// <para>
/// <b>No <c>Version</c>.</b> RIM carries one; nothing in RegOS writes or reads
/// it, and a persistent property with no acquisition path is the defect
/// EPIC-007a spent three findings on. Recorded as a seam.
/// </para>
/// <para>
/// <b>No strength here.</b> A product's strength is its ingredients' strengths,
/// and duplicating it at this level would create two sources of truth for one
/// fact. <c>Strength</c> arrives on <c>Ingredient</c> in S003.
/// </para>
/// </remarks>
public sealed class PharmaceuticalProductDetail
    : AggregateRoot<PharmaceuticalProductDetailId>
{
    public const int NameMaxLength = 250;
    public const int DescriptionMaxLength = 2000;

    private readonly List<CodedConcept> _routesOfAdministration = [];
    private readonly List<Ingredient> _ingredients = [];

    private PharmaceuticalProductDetail()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// The market this presentation belongs to. Immutable — moving a
    /// presentation between markets would silently rewrite what it describes.
    /// </summary>
    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>
    /// What this presentation is called — <em>"Film-coated tablet, 10 mg"</em>.
    /// A label for humans choosing between several, not an identifier.
    /// </summary>
    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    /// <summary>The administrable form.</summary>
    public CodedConcept DoseForm { get; private set; } = default!;

    /// <summary>
    /// The countable article — a vial, a tablet. Optional: an oral solution
    /// measured in mL has no natural unit to count.
    /// </summary>
    public CodedConcept? UnitOfPresentation { get; private set; }

    /// <summary>
    /// How it may be given. Several is ordinary — a solution for injection is
    /// routinely intravenous <em>and</em> intramuscular.
    /// </summary>
    /// <remarks>
    /// <b>Modelled once, as an owned collection.</b> RIM gives route of
    /// administration its own object <em>and</em> carries it as a multi-valued
    /// attribute here, and the two disagree about which is authoritative. The
    /// standalone object is a relational artifact — it exists so a spreadsheet
    /// had somewhere to put a second row (EPIC-010a D6).
    /// </remarks>
    public IReadOnlyCollection<CodedConcept> RoutesOfAdministration
        => _routesOfAdministration.AsReadOnly();

    /// <summary>
    /// What this presentation is made of. Empty is ordinary — a presentation is
    /// recorded before its formulation is stated.
    /// </summary>
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.AsReadOnly();

    /// <summary>
    /// True when the composition says what the product works by.
    /// </summary>
    /// <remarks>
    /// <b>A completeness check, deliberately not an invariant.</b> Requiring an
    /// active on every edit would dictate the order a user types a formulation
    /// in — excipient first would be refused — and RegOS settled long ago that
    /// completeness belongs at a gate rather than on every keystroke, which is
    /// what submission validation is. Exposed so a screen can say the
    /// composition is unfinished, and so the gate that eventually enforces it
    /// has something to read.
    /// </remarks>
    public bool HasAnActiveIngredient
        => _ingredients.Any(x => x.Role == IngredientRole.Active);

    public DateTime CreatedOn { get; private set; }

    public static PharmaceuticalProductDetail Create(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string name,
        string? description,
        CodedConcept doseForm,
        CodedConcept? unitOfPresentation,
        IEnumerable<CodedConcept> routesOfAdministration)
    {
        if (tenantId is null)
            throw new DomainException(
                PharmaceuticalProductDetailErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                PharmaceuticalProductDetailErrors.MarketRequired);

        var detail = new PharmaceuticalProductDetail
        {
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            CreatedOn = DateTime.UtcNow
        };

        detail.Restate(
            name, description, doseForm, unitOfPresentation, routesOfAdministration);

        detail.Id = PharmaceuticalProductDetailId.New();

        return detail;
    }

    /// <summary>
    /// Restates the whole presentation.
    /// </summary>
    /// <remarks>
    /// <b>One method, not five setters.</b> A presentation is a short
    /// descriptive record a user corrects as a whole — "no, it is a film-coated
    /// tablet given orally" — and per-field mutation would offer five ways to
    /// leave it half-corrected. It also keeps the routes collection replaced
    /// atomically rather than needing add and remove methods that no screen
    /// asks for.
    /// <para>
    /// Deliberately <em>not</em> the shape <c>MedicinalProduct</c> uses for
    /// trade names. There, each name is a fact with its own identity that other
    /// records may come to reference; here the routes are attributes of one
    /// statement about one thing.
    /// </para>
    /// </remarks>
    public void Restate(
        string name,
        string? description,
        CodedConcept doseForm,
        CodedConcept? unitOfPresentation,
        IEnumerable<CodedConcept> routesOfAdministration)
    {
        if (doseForm is null)
            throw new DomainException(
                PharmaceuticalProductDetailErrors.DoseFormRequired);

        Name = ValidatedName(name);
        Description = ValidatedDescription(description);
        DoseForm = doseForm;
        UnitOfPresentation = unitOfPresentation;

        _routesOfAdministration.Clear();

        foreach (var route in routesOfAdministration ?? [])
        {
            if (route is null)
                continue;

            // Compared by value: two CodedConcepts quoting the same code from
            // the same system are the same route even though they are two
            // objects — which they must be, because each is persisted against
            // its own owner.
            if (_routesOfAdministration.Contains(route))
                throw new BusinessRuleViolationException(
                    PharmaceuticalProductDetailErrors.RouteAlreadyRecorded);

            _routesOfAdministration.Add(route);
        }
    }

    /// <summary>
    /// Adds a substance to the composition, in the role it plays.
    /// </summary>
    /// <remarks>
    /// <b>One row per substance.</b> The same substance twice is one fact
    /// stated twice, and it would double every quantity a reader adds up.
    /// </remarks>
    public Ingredient AddIngredient(
        SubstanceId substanceId, IngredientRole role, Strength? strength)
    {
        if (substanceId is null)
            throw new DomainException(IngredientErrors.SubstanceRequired);

        if (_ingredients.Any(x => x.SubstanceId == substanceId))
            throw new BusinessRuleViolationException(
                IngredientErrors.SubstanceAlreadyInComposition);

        var ingredient = new Ingredient(
            IngredientId.New(), substanceId, role, strength);

        _ingredients.Add(ingredient);

        return ingredient;
    }

    /// <summary>
    /// Corrects an ingredient's role or its strength.
    /// </summary>
    /// <remarks>
    /// <b>The substance is not changeable, and that is what makes remove
    /// usable.</b> A different substance is a different ingredient, so swapping
    /// one is add-then-remove — and because the new active is added first, the
    /// last-active guard below never blocks it. A restate that could change the
    /// substance would leave no way to tell a correction from a replacement.
    /// </remarks>
    public void RestateIngredient(
        IngredientId ingredientId, IngredientRole role, Strength? strength)
    {
        var existing = _ingredients.FirstOrDefault(x => x.Id == ingredientId)
            ?? throw new NotFoundException(IngredientErrors.NotFound);

        RequireTheCompositionKeepsAnActive(
            losing: existing, gaining: role == IngredientRole.Active);

        // Rebuilt rather than mutated in place, so the "an active declares a
        // strength" rule is checked by the one constructor that owns it rather
        // than restated here.
        var corrected = new Ingredient(
            existing.Id, existing.SubstanceId, role, strength);

        _ingredients[_ingredients.IndexOf(existing)] = corrected;
    }

    public void RemoveIngredient(IngredientId ingredientId)
    {
        var existing = _ingredients.FirstOrDefault(x => x.Id == ingredientId)
            ?? throw new NotFoundException(IngredientErrors.NotFound);

        RequireTheCompositionKeepsAnActive(losing: existing, gaining: false);

        _ingredients.Remove(existing);
    }

    /// <summary>
    /// A composition that has an active may not be left with excipients and no
    /// active.
    /// </summary>
    /// <remarks>
    /// <b>An anti-corruption rule, not a completeness rule.</b> It stops a
    /// valid formulation being turned into an incoherent one — a list of
    /// excipients describing nothing — while leaving
    /// <see cref="HasAnActiveIngredient"/> to say whether a composition is
    /// finished.
    /// <para>
    /// <b>Emptying the composition entirely is allowed.</b> Removing the last
    /// ingredient of all is starting over, which is a different act from
    /// hollowing one out.
    /// </para>
    /// </remarks>
    private void RequireTheCompositionKeepsAnActive(
        Ingredient losing, bool gaining)
    {
        if (losing.Role != IngredientRole.Active || gaining)
            return;

        var othersRemain = _ingredients.Any(x => x.Id != losing.Id);

        var anotherActiveRemains = _ingredients.Any(
            x => x.Id != losing.Id && x.Role == IngredientRole.Active);

        if (othersRemain && !anotherActiveRemains)
            throw new BusinessRuleViolationException(
                IngredientErrors.CompositionNeedsAnActive);
    }

    private static string ValidatedName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                PharmaceuticalProductDetailErrors.NameRequired);

        var trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new DomainException(
                PharmaceuticalProductDetailErrors.NameTooLong);

        return trimmed;
    }

    private static string? ValidatedDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException(
                PharmaceuticalProductDetailErrors.DescriptionTooLong);

        return trimmed;
    }
}
