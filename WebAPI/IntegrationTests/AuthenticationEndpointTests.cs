using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class AuthenticationEndpointTests
{
    [Fact]
    public async Task AuthenticationFlow_ShouldRegisterLoginAndRefreshToken()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        string email = $"candidate-{Guid.NewGuid():N}@example.com";
        var credentials = new { email, password = "Strong!123" };

        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/Auth/Register", credentials);

        string registerContent = await registerResponse.Content.ReadAsStringAsync();
        Assert.True(
            registerResponse.StatusCode == HttpStatusCode.Created,
            $"Expected registration to return 201, but received {(int)registerResponse.StatusCode}: {registerContent}"
        );
        AssertRefreshTokenCookie(registerResponse);
        using JsonDocument registerBody = JsonDocument.Parse(registerContent);
        Assert.False(string.IsNullOrWhiteSpace(registerBody.RootElement.GetProperty("token").GetString()));

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/Auth/Login", credentials);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        AssertRefreshTokenCookie(loginResponse);
        using JsonDocument loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        string? accessToken = loginBody.RootElement.GetProperty("accessToken").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage protectedResponse = await client.GetAsync("/api/Users?PageIndex=0&PageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, protectedResponse.StatusCode);
        using JsonDocument protectedBody = JsonDocument.Parse(await protectedResponse.Content.ReadAsStringAsync());
        Assert.Equal("You are not authorized.", protectedBody.RootElement.GetProperty("detail").GetString());

        HttpResponseMessage refreshResponse = await client.GetAsync("/api/Auth/RefreshToken");

        Assert.Equal(HttpStatusCode.Created, refreshResponse.StatusCode);
        AssertRefreshTokenCookie(refreshResponse);
        using JsonDocument refreshBody = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(refreshBody.RootElement.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task SecuredEndpoint_WithoutAccessToken_ShouldReturnUnauthorized()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        HttpResponseMessage response = await client.GetAsync("/api/Users?PageIndex=0&PageSize=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("You are not authenticated.", body.RootElement.GetProperty("detail").GetString());
    }

    private static HttpClient CreateClient(CustomWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }
        );
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        return client;
    }

    private static void AssertRefreshTokenCookie(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values));
        Assert.Contains(values, value => value.StartsWith("refreshToken=", StringComparison.OrdinalIgnoreCase));
    }
}
