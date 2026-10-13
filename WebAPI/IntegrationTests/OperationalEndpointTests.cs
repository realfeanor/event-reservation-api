using System.Net;
using System.Text.Json;
using Xunit;

namespace IntegrationTests;

public class OperationalEndpointTests
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoints_ShouldReportHealthy(string endpoint)
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ProblemResponse_ShouldPreserveCorrelationIdWithoutLeakingInternalDetails()
    {
        await using CustomWebApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        const string correlationId = "portfolio-integration-test";
        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

        HttpResponseMessage response = await client.GetAsync($"/api/Sessions/{Guid.NewGuid()}/seats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(correlationId, Assert.Single(response.Headers.GetValues("X-Correlation-ID")));
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(correlationId, body.RootElement.GetProperty("traceId").GetString());
        Assert.Equal("Session not found.", body.RootElement.GetProperty("detail").GetString());
    }
}
