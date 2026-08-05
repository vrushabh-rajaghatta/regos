using Npgsql;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Tests;

/// <summary>
/// Direct database access for the two things HTTP cannot express: making a
/// token expire without waiting fourteen days, and looking at what was actually
/// persisted.
/// </summary>
public sealed class RefreshTokenStore
{
    private readonly string _connectionString;

    public RefreshTokenStore(string connectionString)
    {
        _connectionString = connectionString;
    }


    public async Task ExpireAllForAsync(string email)
    {
        await ExecuteAsync(
            """
            UPDATE "RefreshTokens" SET "ExpiresAt" = now() - interval '1 day'
            WHERE "UserId" IN (SELECT "Id" FROM "Users" WHERE "Email" = @email)
            """,
            email);
    }

    public async Task DeleteAllForAsync(string email)
    {
        await ExecuteAsync(
            """
            DELETE FROM "RefreshTokens"
            WHERE "UserId" IN (SELECT "Id" FROM "Users" WHERE "Email" = @email)
            """,
            email);
    }

    public async Task<IReadOnlyList<string>> HashesForAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT "TokenHash" FROM "RefreshTokens"
            WHERE "UserId" IN (SELECT "Id" FROM "Users" WHERE "Email" = @email)
            """,
            connection);

        command.Parameters.AddWithValue("email", email);

        var hashes = new List<string>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) hashes.Add(reader.GetString(0));

        return hashes;
    }

    private async Task ExecuteAsync(string sql, string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("email", email);

        await command.ExecuteNonQueryAsync();
    }
}
