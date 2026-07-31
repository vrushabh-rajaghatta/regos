using System.Text.RegularExpressions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// Finds handler classes in source text.
/// </summary>
internal static class Handlers
{
    private static readonly Regex Declaration = new(
        @"\bclass\s+(?<name>\w+Handler)\b",
        RegexOptions.Compiled);

    /// <summary>The names of every handler class declared in a file's code.</summary>
    internal static IEnumerable<string> DeclaredIn(string code) =>
        Declaration.Matches(code).Select(m => m.Groups["name"].Value).Distinct();
}
