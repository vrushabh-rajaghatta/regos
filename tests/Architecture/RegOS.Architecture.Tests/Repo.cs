using System.Text.RegularExpressions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// Locates the repository on disk and reads source files out of it.
///
/// These tests scan text rather than reflecting over assemblies, for one
/// reason: a reflection test only sees the contexts this project references,
/// so every new bounded context would have to be wired in by hand — and the
/// context nobody wired in is exactly the one that drifts. Reading src/ means
/// a convention applies to code written after this file, by someone who never
/// read it.
/// </summary>
internal static class Repo
{
    /// <summary>The repository root, found by walking up to RegOS.slnx.</summary>
    internal static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "RegOS.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate RegOS.slnx above " + AppContext.BaseDirectory
            + ". The architecture tests read source from the repository and "
            + "cannot run outside it.");
    }

    /// <summary>
    /// Every .cs file under a repo-relative path, excluding build output.
    /// obj/ holds generated AssemblyInfo and GlobalUsings files that would
    /// otherwise be scanned as if a human had written them.
    /// </summary>
    internal static IEnumerable<string> SourceFiles(string relativePath)
    {
        var root = Path.Combine(Root, relativePath);

        if (!Directory.Exists(root)) return [];

        return Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(NotBuildOutput)
            .OrderBy(path => path, StringComparer.Ordinal);
    }

    private static bool NotBuildOutput(string path)
    {
        var relative = Relative(path);

        return !relative.Contains("/obj/", StringComparison.Ordinal)
            && !relative.Contains("/bin/", StringComparison.Ordinal);
    }

    /// <summary>
    /// A path relative to the repo root, with forward slashes on every OS, so
    /// failure messages read the same on macOS and CI and can be pasted
    /// straight into an exemption list.
    /// </summary>
    internal static string Relative(string absolutePath) =>
        Path.GetRelativePath(Root, absolutePath).Replace('\\', '/');

    /// <summary>
    /// Directories under a repo-relative path, excluding build output.
    /// </summary>
    internal static IEnumerable<string> Directories(string relativePath)
    {
        var root = Path.Combine(Root, relativePath);

        if (!Directory.Exists(root)) return [];

        return Directory
            .EnumerateDirectories(root, "*", SearchOption.AllDirectories)
            .Where(NotBuildOutput)
            .OrderBy(path => path, StringComparer.Ordinal);
    }

    /// <summary>
    /// Strips line and block comments before pattern matching, so a route or
    /// a lambda quoted inside an explanatory comment is not read as code.
    /// </summary>
    internal static string CodeOf(string path)
    {
        var text = File.ReadAllText(path);

        text = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        text = Regex.Replace(text, @"//[^\n]*", string.Empty);

        return text;
    }
}
