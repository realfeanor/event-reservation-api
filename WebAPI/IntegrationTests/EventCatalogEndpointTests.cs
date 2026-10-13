using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class EventCatalogEndpointTests
{
    [Fact]
    public async Task Administrator_ShouldPublishEventScheduleSessionAndPreserveSeatSnapshot()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthentication.CreateAdministratorAccessToken(factory, client)
        );

        Guid venueId = await CreateVenue(client);
        Guid firstSeatId = await AddSeat(client, venueId, "Main A 1");
        await AddSeat(client, venueId, "Main A 2");
        Guid eventId = await CreateEvent(client);
        DateTime startUtc = DateTime.UtcNow.AddDays(10);
        DateTime endUtc = startUtc.AddHours(2);

        HttpResponseMessage draftScheduleResponse = await ScheduleSession(
            client,
            eventId,
            venueId,
            startUtc,
            endUtc
        );
        Assert.Equal(HttpStatusCode.BadRequest, draftScheduleResponse.StatusCode);

        HttpResponseMessage publishResponse = await client.PutAsync($"/api/Events/{eventId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        HttpResponseMessage scheduleResponse = await ScheduleSession(client, eventId, venueId, startUtc, endUtc);
        Assert.Equal(HttpStatusCode.Created, scheduleResponse.StatusCode);
        using JsonDocument scheduled = JsonDocument.Parse(await scheduleResponse.Content.ReadAsStringAsync());
        Guid sessionId = scheduled.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(2, scheduled.RootElement.GetProperty("seatCount").GetInt32());

        HttpResponseMessage deactivateResponse = await client.DeleteAsync(
            $"/api/Venues/{venueId}/seats/{firstSeatId}"
        );
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        HttpResponseMessage openResponse = await client.PutAsync(
            $"/api/Sessions/{sessionId}/reservations/open",
            null
        );
        Assert.Equal(HttpStatusCode.OK, openResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage layoutResponse = await client.GetAsync($"/api/Sessions/{sessionId}/seats");
        Assert.Equal(HttpStatusCode.OK, layoutResponse.StatusCode);
        using JsonDocument layout = JsonDocument.Parse(await layoutResponse.Content.ReadAsStringAsync());
        Assert.True(layout.RootElement.GetProperty("reservationsOpen").GetBoolean());
        Assert.Equal(2, layout.RootElement.GetProperty("seats").GetArrayLength());
    }

    private static async Task<Guid> CreateVenue(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Venues", new { name = "Snapshot Hall" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> AddSeat(HttpClient client, Guid venueId, string label)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/Venues/{venueId}/seats", new { label });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateEvent(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Events",
            new { title = "Architecture Conference", description = "Backend engineering" }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }

    private static Task<HttpResponseMessage> ScheduleSession(
        HttpClient client,
        Guid eventId,
        Guid venueId,
        DateTime startUtc,
        DateTime endUtc
    ) => client.PostAsJsonAsync("/api/Sessions", new { eventId, venueId, startUtc, endUtc });

    private static HttpClient CreateClient(CustomWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }
        );
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        return client;
    }
}
