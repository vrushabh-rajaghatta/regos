using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RegOS.Api.Tests;

/// <summary>
/// The cookies a browser would be holding, carried by hand.
/// </summary>
/// <remarks>
/// Explicit rather than a <c>CookieContainer</c> for two reasons. The cookies
/// are marked <c>Secure</c>, and a cookie jar would refuse to return them over
/// the test host's plain HTTP — hiding the very behaviour under test. And doing
/// it manually means each test states exactly which cookie it is presenting,
/// which matters when the point is that a *stale* one is rejected.
/// </remarks>
public sealed class Session
{
    public const string DevEmail = "dev@regos.local";
    public const string DevPassword = "development-password";

    private readonly Dictionary<string, string> _cookies = new();

    public string? Access => _cookies.GetValueOrDefault(
        Api.Authentication.SessionCookies.AccessToken);

    public string? Refresh => _cookies.GetValueOrDefault(
        Api.Authentication.SessionCookies.RefreshToken);

    public static async Task<(Session Session, HttpResponseMessage Response)>
        LoginAsync(
            HttpClient client,
            string email = DevEmail,
            string password = DevPassword)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login", new { email, password });

        var session = new Session();
        session.Absorb(response);

        return (session, response);
    }

    /// <summary>Reads every Set-Cookie the response carries.</summary>
    public void Absorb(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
            return;

        foreach (var header in headers)
        {
            var pair = header.Split(';')[0];
            var separator = pair.IndexOf('=');

            if (separator <= 0) continue;

            var name = pair[..separator];
            var value = pair[(separator + 1)..];

            // An empty value with an expiry in the past is a deletion.
            if (string.IsNullOrEmpty(value)) _cookies.Remove(name);
            else _cookies[name] = value;
        }
    }

    /// <summary>Sends a request carrying the cookies this session holds.</summary>
    public async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        params string[] cookieNames)
    {
        var request = new HttpRequestMessage(method, path);

        var names = cookieNames.Length > 0
            ? cookieNames
            : _cookies.Keys.ToArray();

        var header = string.Join(
            "; ",
            names.Where(_cookies.ContainsKey)
                .Select(name => $"{name}={_cookies[name]}"));

        if (header.Length > 0)
            request.Headers.Add("Cookie", header);

        var response = await client.SendAsync(request);

        Absorb(response);

        return response;
    }

    /// <summary>Sends a request carrying one explicit cookie value.</summary>
    public static async Task<HttpResponseMessage> SendWithCookieAsync(
        HttpClient client,
        HttpMethod method,
        string path,
        string cookieName,
        string cookieValue)
    {
        var request = new HttpRequestMessage(method, path);

        request.Headers.Add("Cookie", $"{cookieName}={cookieValue}");

        return await client.SendAsync(request);
    }

    /// <summary>Sends a request authenticated by bearer header instead.</summary>
    public static async Task<HttpResponseMessage> SendWithBearerAsync(
        HttpClient client,
        string path,
        string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await client.SendAsync(request);
    }
}
