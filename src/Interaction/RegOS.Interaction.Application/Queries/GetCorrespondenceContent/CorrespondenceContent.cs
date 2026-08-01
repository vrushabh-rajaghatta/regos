namespace RegOS.Interaction.Application.Queries.GetCorrespondenceContent;

/// <summary>
/// The bytes, plus the name they arrived under. The original file name travels
/// with the download because forwarding <c>a1b2c3.pdf</c> to a colleague is not
/// the same as forwarding <c>FDA-IR-2019-06-14.pdf</c>.
/// </summary>
public sealed record CorrespondenceContent(
    Stream Content,
    string ContentType,
    string OriginalFileName);
