using Npgsql;

namespace RegOS.Api.Tests;

/// <summary>
/// Direct database access for what HTTP cannot express: expiring an invitation
/// without waiting a week, reading what was actually persisted, and cleaning up
/// users the API offers no way to delete.
/// </summary>
internal static class UserStore
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    /// <summary>
    /// Deletes every user whose email contains the marker. Invitations,
    /// credentials and refresh tokens go with them by cascade (ADR-026) -
    /// which is the whole reason this is one statement rather than four.
    /// </summary>
    public static Task DeleteUsersMatchingAsync(string marker) =>
        ExecuteAsync(
            """DELETE FROM "Users" WHERE "Email" LIKE '%' || @value || '%'""",
            marker);

    public static Task ExpireInvitationsForAsync(Guid userId) =>
        ExecuteAsync(
            """
            UPDATE "Invitations" SET "ExpiresAt" = now() - interval '1 day'
            WHERE "UserId" = @id
            """,
            userId);

    public static Task<int> StatusOfAsync(Guid userId) =>
        ScalarAsync<int>("""SELECT "Status" FROM "Users" WHERE "Id" = @id""", userId);

    public static Task<int> CredentialCountAsync(Guid userId) =>
        ScalarAsync<int>(
            """SELECT count(*)::int FROM "UserCredentials" WHERE "UserId" = @id""",
            userId);

    public static async Task<IReadOnlyList<string>> InvitationHashesForAsync(
        Guid userId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """SELECT "TokenHash" FROM "Invitations" WHERE "UserId" = @id""",
            connection);

        command.Parameters.AddWithValue("id", userId);

        var hashes = new List<string>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) hashes.Add(reader.GetString(0));

        return hashes;
    }

    private static async Task ExecuteAsync(string sql, object value)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            value is Guid ? "id" : "value", value);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(string sql, Guid id)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        return (T)(await command.ExecuteScalarAsync())!;
    }
}
