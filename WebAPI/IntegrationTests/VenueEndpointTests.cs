using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class VenueEndpointTests
{
    [Fact]
    public async Task Administrator_ShouldCreateVenueManageSeatsAndReadLayout()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthentication.CreateAdministratorAccessToken(factory, client)
        );

        HttpResponseMessage createVenueResponse = await client.PostAsJsonAsync(
            "/api/Venues",
            new { name = "  Istanbul Concert Hall  " }
        );
        Assert.Equal(HttpStatusCode.Created, createVenueResponse.StatusCode);
        using JsonDocument createdVenue = JsonDocument.Parse(await createVenueResponse.Content.ReadAsStringAsync());
        Guid venueId = createdVenue.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("Istanbul Concert Hall", createdVenue.RootElement.GetProperty("name").GetString());

        HttpResponseMessage addSeatResponse = await client.PostAsJsonAsync(
            $"/api/Venues/{venueId}/seats",
            new { label = " Main   A  12 " }
        );
        Assert.Equal(HttpStatusCode.Created, addSeatResponse.StatusCode);
        using JsonDocument addedSeat = JsonDocument.Parse(await addSeatResponse.Content.ReadAsStringAsync());
        Guid seatId = addedSeat.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("Main A 12", addedSeat.RootElement.GetProperty("label").GetString());

        HttpResponseMessage duplicateResponse = await client.PostAsJsonAsync(
            $"/api/Venues/{venueId}/seats",
            new { label = "main a 12" }
        );
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);

        HttpResponseMessage deactivateResponse = await client.DeleteAsync($"/api/Venues/{venueId}/seats/{seatId}");
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        HttpResponseMessage replacementResponse = await client.PostAsJsonAsync(
            $"/api/Venues/{venueId}/seats",
            new { label = "main a 12" }
        );
        Assert.Equal(HttpStatusCode.Created, replacementResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage getVenueResponse = await client.GetAsync($"/api/Venues/{venueId}");
        Assert.Equal(HttpStatusCode.OK, getVenueResponse.StatusCode);
        using JsonDocument venue = JsonDocument.Parse(await getVenueResponse.Content.ReadAsStringAsync());
        JsonElement.ArrayEnumerator seats = venue.RootElement.GetProperty("seats").EnumerateArray();
        JsonElement[] seatItems = seats.ToArray();
        Assert.Equal(2, seatItems.Length);
        Assert.Single(seatItems, seat => !seat.GetProperty("isActive").GetBoolean());
        Assert.Single(seatItems, seat => seat.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task CreateVenue_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Venues", new { name = "Concert Hall" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateVenue_WithWhitespaceName_ShouldReturnBadRequest()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthentication.CreateAdministratorAccessToken(factory, client)
        );

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Venues", new { name = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpClient CreateClient(CustomWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }
        );
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        return client;
    }
}
