using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Reservations.Commands.Expire;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Xunit;

namespace IntegrationTests;

public class ReservationLifecycleEndpointTests
{
    [Fact]
    public async Task Customer_ShouldConfirmAndCancelReservationIdempotently()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = SeatHoldEndpointTests.CreateClient(factory);
        Guid sessionId = await SeatHoldEndpointTests.CreateOpenSessionWithThreeSeats(factory, client);
        Guid[] seatIds = await SeatHoldEndpointTests.GetSeatIds(client, sessionId);
        string ownerToken = await TestAuthentication.CreateCustomerAccessToken(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        Guid reservationId = await HoldSeats(client, sessionId, [seatIds[0], seatIds[1]]);

        string otherCustomerToken = await TestAuthentication.CreateCustomerAccessToken(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherCustomerToken);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsync($"/api/Reservations/{reservationId}/confirmation", null)).StatusCode
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        HttpResponseMessage firstConfirmation = await client.PutAsync(
            $"/api/Reservations/{reservationId}/confirmation",
            null
        );
        HttpResponseMessage repeatedConfirmation = await client.PutAsync(
            $"/api/Reservations/{reservationId}/confirmation",
            null
        );

        Assert.Equal(HttpStatusCode.OK, firstConfirmation.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeatedConfirmation.StatusCode);
        Assert.Equal(ReservationStatus.Confirmed, await ReadStatus(repeatedConfirmation));
        HttpResponseMessage mineResponse = await client.GetAsync("/api/Reservations");
        using (JsonDocument mine = JsonDocument.Parse(await mineResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal(HttpStatusCode.OK, mineResponse.StatusCode);
            Assert.Equal(reservationId, mine.RootElement[0].GetProperty("id").GetGuid());
        }

        HttpResponseMessage firstCancellation = await client.DeleteAsync($"/api/Reservations/{reservationId}");
        HttpResponseMessage repeatedCancellation = await client.DeleteAsync($"/api/Reservations/{reservationId}");

        Assert.Equal(HttpStatusCode.OK, firstCancellation.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeatedCancellation.StatusCode);
        Assert.Equal(ReservationStatus.Cancelled, await ReadStatus(repeatedCancellation));

        client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage layoutResponse = await client.GetAsync($"/api/Sessions/{sessionId}/seats");
        using JsonDocument layout = JsonDocument.Parse(await layoutResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            3,
            layout.RootElement.GetProperty("seats").EnumerateArray()
                .Count(seat => seat.GetProperty("status").GetInt32() == (int)SessionSeatStatus.Available)
        );
    }

    [Fact]
    public async Task ExpirationProcessing_ShouldReleaseSeatsAndIgnoreRepeatedProcessing()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = SeatHoldEndpointTests.CreateClient(factory);
        Guid sessionId = await SeatHoldEndpointTests.CreateOpenSessionWithThreeSeats(factory, client);
        Guid[] seatIds = await SeatHoldEndpointTests.GetSeatIds(client, sessionId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await TestAuthentication.CreateCustomerAccessToken(client)
        );
        Guid reservationId = await HoldSeats(client, sessionId, [seatIds[0], seatIds[1]]);
        factory.Clock.Advance(TimeSpan.FromMinutes(11));

        bool firstResult = await Expire(factory, reservationId);
        bool repeatedResult = await Expire(factory, reservationId);

        Assert.True(firstResult);
        Assert.False(repeatedResult);
        using IServiceScope scope = factory.Services.CreateScope();
        BaseDbContext context = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
        Assert.Equal(
            ReservationStatus.Expired,
            await context.Reservations.Where(reservation => reservation.Id == reservationId)
                .Select(reservation => reservation.Status)
                .SingleAsync()
        );
        Assert.Equal(
            2,
            await context.SessionSeats.CountAsync(
                seat => seatIds.Take(2).Contains(seat.Id) && seat.Status == SessionSeatStatus.Available
            )
        );
    }

    private static async Task<Guid> HoldSeats(HttpClient client, Guid sessionId, Guid[] seatIds)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/Sessions/{sessionId}/seat-holds",
            new { sessionSeatIds = seatIds }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<ReservationStatus> ReadStatus(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (ReservationStatus)body.RootElement.GetProperty("status").GetInt32();
    }

    private static async Task<bool> Expire(CustomWebApplicationFactory factory, Guid reservationId)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(new ExpireReservationCommand(reservationId));
    }
}
