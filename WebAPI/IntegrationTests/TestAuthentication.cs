using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Core.Security.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Xunit;

namespace IntegrationTests;

internal static class TestAuthentication
{
    public static async Task<string> CreateCustomerAccessToken(HttpClient client)
    {
        string email = $"test-customer-{Guid.NewGuid():N}@example.com";
        var credentials = new { email, password = "Strong!123" };
        HttpResponseMessage registrationResponse = await client.PostAsJsonAsync("/api/Auth/Register", credentials);
        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/Auth/Login", credentials);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetProperty("token").GetString()!;
    }

    public static async Task<string> CreateAdministratorAccessToken(
        CustomWebApplicationFactory factory,
        HttpClient client
    )
    {
        string email = $"test-admin-{Guid.NewGuid():N}@example.com";
        var credentials = new { email, password = "Strong!123" };
        HttpResponseMessage registrationResponse = await client.PostAsJsonAsync("/api/Auth/Register", credentials);
        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);

        using (IServiceScope scope = factory.Services.CreateScope())
        {
            BaseDbContext context = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
            User user = await context.Users.SingleAsync(user => user.Email == email);
            OperationClaim adminClaim = await context.OperationClaims.SingleAsync(
                claim => claim.Name == GeneralOperationClaims.Admin
            );
            context.UserOperationClaims.Add(
                new UserOperationClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    OperationClaimId = adminClaim.Id
                }
            );
            await context.SaveChangesAsync();
        }

        HttpResponseMessage loginResponse = await client.PostAsJsonAsync("/api/Auth/Login", credentials);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetProperty("token").GetString()!;
    }
}
