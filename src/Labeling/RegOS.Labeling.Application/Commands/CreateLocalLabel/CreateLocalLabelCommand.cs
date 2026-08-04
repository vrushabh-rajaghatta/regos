using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Commands.CreateLocalLabel;

/// <summary>
/// Starts holding a market's own controlled labelling document.
/// </summary>
/// <param name="LabelTypeCode">
/// A code from <c>LabelVocabulary.LocalLabelTypes</c> — carton artwork is one
/// of them, not a separate thing (EPIC-018 D2).
/// </param>
/// <param name="Language">
/// Two-letter ISO 639-1. Part of what the document <em>is</em>: a French carton
/// and a Dutch carton are separately approved.
/// </param>
public sealed record CreateLocalLabelCommand(
    MedicinalProductId MedicinalProductId,
    string LabelTypeCode,
    string Language);
