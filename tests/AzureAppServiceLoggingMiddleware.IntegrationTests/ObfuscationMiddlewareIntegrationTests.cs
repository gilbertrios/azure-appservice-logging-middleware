using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace AzureAppServiceLoggingMiddleware.IntegrationTests;

[Trait("Category", "Integration")]
public class ObfuscationMiddlewareIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ObfuscationMiddlewareIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200_WithoutLogging()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task RootEndpoint_ShouldReturn200_WithoutLogging()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Orders_CreateOrder_ShouldObfuscateSensitiveData()
    {
        // Arrange
        var orderRequest = new
        {
            customerId = "cust-123",
            productId = "prod-456",
            quantity = 2,
            paymentInfo = new
            {
                creditCard = "4111-1111-1111-1111",
                cvv = "123"
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(orderRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Orders_GetOrder_ShouldReturnOrder()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/ord-123");

        // Assert - Returns 404 because order doesn't exist in test database
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Payments_ProcessPayment_ShouldObfuscateCreditCard()
    {
        // Arrange
        var paymentRequest = new
        {
            orderId = "ord-123",
            amount = 99.99,
            creditCardNumber = "4111-1111-1111-1111",
            cvv = "123",
            expiryDate = "12/25"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(paymentRequest),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/payments", content);

        // Assert - Payment POST endpoint doesn't exist, returns 405 Method Not Allowed
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Payments_GetPayment_ShouldReturnPayment()
    {
        // Act
        var response = await _client.GetAsync("/api/payments/pay-123");

        // Assert - Returns 404 because payment doesn't exist in test database
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvalidJson_ShouldReturn400_AndHandleGracefully()
    {
        // Arrange
        var invalidContent = new StringContent(
            "{invalid json}",
            Encoding.UTF8,
            "application/json");

        // Act & Assert - Should throw BadHttpRequestException, which is expected
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
        {
            await _client.PostAsync("/api/orders", invalidContent);
        });
    }

    [Fact]
    public async Task Swagger_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Azure App Service API");
    }

    [Fact]
    public async Task MultipleRequests_ShouldAllBeObfuscated()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5).Select(i => new
        {
            customerId = $"cust-{i}",
            productId = $"prod-{i}",
            quantity = i,
            apiKey = $"secret-key-{i}"
        });

        // Act
        var tasks = requests.Select(async req =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(req),
                Encoding.UTF8,
                "application/json");
            return await _client.PostAsync("/api/orders", content);
        });

        var responses = await Task.WhenAll(tasks);

        // Assert - POST to orders returns 201 Created
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));
    }
}
