using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>Raw SQL written by hand inside a migration.</b> EF generates most of a
/// migration; the statements a person writes are the ones nothing checks, and
/// this is where rules about them live.
/// </summary>
/// <remarks>
/// <b>One rule today, and the file is named for the class rather than the
/// rule</b> — so that a second constraint on hand-written migration SQL has an
/// obvious home instead of arriving as a new file. No second rule is written
/// here: one demonstrated need is one, and
/// <see href="../../../docs/adr/ADR-018-rule-of-three.md">ADR-018</see> forbids
/// the speculative version as firmly as it forbids speculative deletion.
/// </remarks>
public class MigrationRawSqlTests
{
    /// <summary>
    /// <b>Every hand-written statement ends in a semicolon.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Migrate()</c> hands EF each migration command separately, so a missing
    /// terminator changes nothing on the path everybody runs — which is exactly
    /// why this went unnoticed through 85 migrations.
    /// <c>dotnet ef migrations script --idempotent</c> wraps every command in
    /// <c>DO $EF$ … END $EF$</c> and concatenates them, and there a missing
    /// terminator is a syntax error that stops the whole script.
    /// </para>
    /// <para>
    /// <b>Found by generating that script for the first time</b>, while planning
    /// EPIC-023 — not by review, and not by any test. Two of 28 hand-written
    /// statements were unterminated
    /// (<c>LinkUserCredentialToUser</c>, <c>AddSessions</c>), and the supported
    /// idempotent deployment artifact had therefore never run.
    /// </para>
    /// </remarks>
    [Fact]
    public void Every_hand_written_migration_statement_is_terminated()
    {
        var offenders = RawSqlCalls()
            .Where(call => !call.Sql.TrimEnd().EndsWith(';'))
            .Select(call => $"{call.File}: …{Tail(call.Sql)}")
            .ToList();

        offenders.Should().BeEmpty(
            "an unterminated statement is invisible to Migrate(), which sends "
            + "each command on its own, and a syntax error in the idempotent "
            + "script, which concatenates them inside DO $EF$ blocks");
    }

    /// <summary>
    /// The negative control. Without it the assertion above passes by reading
    /// nothing — the failure mode this repository has already been bitten by
    /// when counting test suites.
    /// </summary>
    [Fact]
    public void The_migrations_are_actually_being_read()
    {
        RawSqlCalls().Should().HaveCountGreaterThan(20,
            "RegOS had 28 hand-written migration statements when this was "
            + "written; a much smaller number means the scan stopped matching "
            + "rather than that the SQL was removed");
    }

    private static readonly Regex Call = new(
        @"migrationBuilder\.Sql\(\s*", RegexOptions.Compiled);

    private static List<(string File, string Sql)> RawSqlCalls()
    {
        var calls = new List<(string, string)>();

        foreach (var path in Repo.SourceFiles("src/Persistence/RegOS.Persistence/Migrations")
                     .Where(p => !p.EndsWith("Designer.cs", StringComparison.Ordinal)
                              && !p.EndsWith("ModelSnapshot.cs", StringComparison.Ordinal)))
        {
            // Read raw rather than through Repo.CodeOf: that strips `//` runs
            // anywhere, including inside a string, and a migration's SQL is
            // exactly the place a `//` would be mangled into a false pass.
            // A commented-out call is skipped below instead.
            var text = File.ReadAllText(path);

            foreach (Match match in Call.Matches(text))
            {
                if (IsCommentedOut(text, match.Index)) continue;

                var sql = LiteralAt(text, match.Index + match.Length);

                if (sql is not null)
                    calls.Add((Repo.Relative(path), sql));
            }
        }

        return calls;
    }

    private static bool IsCommentedOut(string text, int index)
    {
        var lineStart = text.LastIndexOf('\n', index) + 1;

        return text[lineStart..index].Contains("//", StringComparison.Ordinal);
    }

    /// <summary>
    /// The string literal starting at <paramref name="start"/>, in each of the
    /// three shapes these migrations use: raw (<c>"""…"""</c>), verbatim
    /// (<c>@"…"</c>) and regular. Returns null for a call whose argument is not
    /// a literal — none exist today, and one would deserve reading rather than
    /// a silent pass.
    /// </summary>
    private static string? LiteralAt(string text, int start)
    {
        if (text.AsSpan(start).StartsWith("\"\"\""))
        {
            var open = start + 3;
            var close = text.IndexOf("\"\"\"", open, StringComparison.Ordinal);

            return close < 0 ? null : text[open..close];
        }

        if (text.AsSpan(start).StartsWith("@\"") || text.AsSpan(start).StartsWith("\""))
        {
            var verbatim = text[start] == '@';
            var open = start + (verbatim ? 2 : 1);

            for (var i = open; i < text.Length; i++)
            {
                if (text[i] != '"') continue;

                // In a verbatim string a doubled quote is an escaped quote; in a
                // regular one the escape is a backslash.
                if (verbatim && i + 1 < text.Length && text[i + 1] == '"')
                {
                    i++;
                    continue;
                }

                if (!verbatim && text[i - 1] == '\\') continue;

                return text[open..i];
            }
        }

        return null;
    }

    private static string Tail(string sql)
    {
        var trimmed = sql.TrimEnd();

        return trimmed.Length <= 60 ? trimmed : trimmed[^60..];
    }
}
