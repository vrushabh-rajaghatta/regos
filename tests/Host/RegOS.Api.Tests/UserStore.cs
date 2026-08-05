using Npgsql;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Tests;

/// <summary>
/// Direct database access for what HTTP cannot express: expiring an invitation
/// without waiting a week, reading what was actually persisted, and cleaning up
/// users the API offers no way to delete.
/// </summary>
public sealed class UserStore
{
    private readonly string _connectionString;

    public UserStore(string connectionString)
    {
        _connectionString = connectionString;
    }


    /// <summary>
    /// Deletes every user whose email contains the marker. Invitations,
    /// credentials and refresh tokens go with them by cascade (ADR-026) -
    /// which is the whole reason this is one statement rather than four.
    /// </summary>
    public Task DeleteUsersMatchingAsync(string marker) =>
        ExecuteAsync(
            """DELETE FROM "Users" WHERE "Email" LIKE '%' || @value || '%'""",
            marker);

    public Task ExpireInvitationsForAsync(Guid userId) =>
        ExecuteAsync(
            """
            UPDATE "Invitations" SET "ExpiresAt" = now() - interval '1 day'
            WHERE "UserId" = @id
            """,
            userId);

    public Task ExpirePasswordResetsForAsync(Guid userId) =>
        ExecuteAsync(
            """
            UPDATE "PasswordResets" SET "ExpiresAt" = now() - interval '1 day'
            WHERE "UserId" = @id
            """,
            userId);

    public async Task<IReadOnlyList<string>> PasswordResetHashesForAsync(
        Guid userId) =>
        await StringsAsync(
            """SELECT "TokenHash" FROM "PasswordResets" WHERE "UserId" = @id""",
            userId);

    public Task<int> StatusOfAsync(Guid userId) =>
        ScalarAsync<int>("""SELECT "Status" FROM "Users" WHERE "Id" = @id""", userId);

    public Task<int> CredentialCountAsync(Guid userId) =>
        ScalarAsync<int>(
            """SELECT count(*)::int FROM "UserCredentials" WHERE "UserId" = @id""",
            userId);

    public Task<IReadOnlyList<string>> InvitationHashesForAsync(
        Guid userId) =>
        StringsAsync(
            """SELECT "TokenHash" FROM "Invitations" WHERE "UserId" = @id""",
            userId);

    private async Task<IReadOnlyList<string>> StringsAsync(
        string sql, Guid id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        var values = new List<string>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) values.Add(reader.GetString(0));

        return values;
    }

    // Public, not private: the tenant-provisioning tests clean up Tenants,
    // which the API deliberately cannot delete.
    public async Task ExecuteAsync(string sql, object value)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            value is Guid ? "id" : "value", value);

        await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string sql, Guid id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("id", id);

        return (T)(await command.ExecuteScalarAsync())!;
    }
}
