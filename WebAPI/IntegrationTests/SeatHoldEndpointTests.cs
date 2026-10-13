using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Xunit;

namespace IntegrationTests;

public class SeatHoldEndpointTests
{
    [Fact]
    public async Task HoldSeats_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/Sessions/{Guid.NewGuid()}/seat-holds",
            new { sessionSeatIds = new[] { Guid.NewGuid() } }
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CompetingRequests_ShouldAllowExactlyOneAtomicMultiSeatHold()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = CreateClient(factory);
        Guid sessionId = await CreateOpenSessionWithThreeSeats(factory, client);
        Guid[] seatIds = await GetSeatIds(client, sessionId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthentication.CreateCustomerAccessToken(client)
        );

        Task<HttpResponseMessage> firstRequest = client.PostAsJsonAsync(
            $"/api/Sessions/{sessionId}/seat-holds",
            new { sessionSeatIds = new[] { seatIds[0], seatIds[1] } }
        );
        Task<HttpResponseMessage> secondRequest = client.PostAsJsonAsync(
            $"/api/Sessions/{sessionId}/seat-holds",
            new { sessionSeatIds = new[] { seatIds[1], seatIds[2] } }
        );
        HttpResponseMessage[] responses = await Task.WhenAll(firstRequest, secondRequest);

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        HttpResponseMessage conflict = Assert.Single(
            responses,
            response => response.StatusCode == HttpStatusCode.Conflict
        );
        using (JsonDocument body = JsonDocument.Parse(await conflict.Content.ReadAsStringAsync()))
        {
            Assert.Equal("Conflict", body.RootElement.GetProperty("title").GetString());
            Assert.Equal(409, body.RootElement.GetProperty("status").GetInt32());
            Assert.Contains("no longer available", body.RootElement.GetProperty("detail").GetString());
        }

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage layoutResponse = await client.GetAsync($"/api/Sessions/{sessionId}/seats");
        Assert.Equal(HttpStatusCode.OK, layoutResponse.StatusCode);
        using (JsonDocument layout = JsonDocument.Parse(await layoutResponse.Content.ReadAsStringAsync()))
        {
            JsonElement.ArrayEnumerator seats = layout.RootElement.GetProperty("seats").EnumerateArray();
            Assert.Equal(2, seats.Count(seat => seat.GetProperty("status").GetInt32() == (int)SessionSeatStatus.Held));
        }

        using IServiceScope scope = factory.Services.CreateScope();
        BaseDbContext context = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
        Assert.Equal(1, await context.Reservations.CountAsync());
        Assert.Equal(2, await context.ReservationSeats.CountAsync());
    }

    internal static async Task<Guid> CreateOpenSessionWithThreeSeats(
        CustomWebApplicationFactory factory,
        HttpClient client
    )
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthentication.CreateAdministratorAccessToken(factory, client)
        );

        HttpResponseMessage venueResponse = await client.PostAsJsonAsync("/api/Venues", new { name = "Hold Hall" });
        Assert.Equal(HttpStatusCode.Created, venueResponse.StatusCode);
        using JsonDocument venueBody = JsonDocument.Parse(await venueResponse.Content.ReadAsStringAsync());
        Guid venueId = venueBody.RootElement.GetProperty("id").GetGuid();
        foreach (string label in new[] { "A 1", "A 2", "A 3" })
        {
            HttpResponseMessage seatResponse = await client.PostAsJsonAsync(
                $"/api/Venues/{venueId}/seats",
                new { label }
            );
            Assert.Equal(HttpStatusCode.Created, seatResponse.StatusCode);
        }

        HttpResponseMessage eventResponse = await client.PostAsJsonAsync(
            "/api/Events",
            new { title = "Concurrency Workshop", description = "Seat hold test" }
        );
        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);
        using JsonDocument eventBody = JsonDocument.Parse(await eventResponse.Content.ReadAsStringAsync());
        Guid eventId = eventBody.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync($"/api/Events/{eventId}/publish", null)).StatusCode);

        DateTime startUtc = DateTime.UtcNow.AddDays(10);
        HttpResponseMessage sessionResponse = await client.PostAsJsonAsync(
            "/api/Sessions",
            new { eventId, venueId, startUtc, endUtc = startUtc.AddHours(2) }
        );
        Assert.Equal(HttpStatusCode.Created, sessionResponse.StatusCode);
        using JsonDocument sessionBody = JsonDocument.Parse(await sessionResponse.Content.ReadAsStringAsync());
        Guid sessionId = sessionBody.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsync($"/api/Sessions/{sessionId}/reservations/open", null)).StatusCode
        );
        return sessionId;
    }

    internal static async Task<Guid[]> GetSeatIds(HttpClient client, Guid sessionId)
    {
        HttpResponseMessage response = await client.GetAsync($"/api/Sessions/{sessionId}/seats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("seats").EnumerateArray().Select(seat => seat.GetProperty("id").GetGuid()).ToArray();
    }

    internal static HttpClient CreateClient(CustomWebApplicationFactory factory)
    {
        HttpClient client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }
        );
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        return client;
    }
}
