using AzureAppServiceLoggingMiddleware.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace AzureAppServiceLoggingMiddleware.UnitTests.Middleware;

[Trait("Category", "Unit")]
public class ObfuscationMiddlewareTests
{
    private readonly Mock<ILogger<ObfuscationMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly ObfuscationOptions _options;

    public ObfuscationMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ObfuscationMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _options = new ObfuscationOptions
        {
            Enabled = true,
            ObfuscationMask = "***REDACTED***",
            SensitiveProperties = new List<string>
            {
                "password",
                "creditCard",
                "ssn",
                "apiKey"
            }
        };
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipLogging_ForHealthCheckEndpoint()
    {
        // Arrange
        var context = CreateHttpContext("/health");
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipLogging_ForRootEndpoint()
    {
        // Arrange
        var context = CreateHttpContext("/");
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldObfuscateSensitiveProperties_InRequestBody()
    {
        // Arrange
        var requestBody = @"{""username"":""john"",""password"":""secret123"",""email"":""john@example.com""}";
        var context = CreateHttpContext("/api/login", "POST", requestBody);
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("***REDACTED***")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotObfuscate_NonSensitiveProperties()
    {
        // Arrange
        var requestBody = @"{""username"":""john"",""email"":""john@example.com""}";
        var context = CreateHttpContext("/api/profile", "POST", requestBody);
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("john") && v.ToString()!.Contains("john@example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleEmptyRequestBody()
    {
        // Arrange
        var context = CreateHttpContext("/api/test", "GET");
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldHandleInvalidJson()
    {
        // Arrange
        var requestBody = @"{invalid json}";
        var context = CreateHttpContext("/api/test", "POST", requestBody);
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to parse JSON")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldObfuscateNestedProperties()
    {
        // Arrange
        var requestBody = @"{""user"":{""name"":""john"",""credentials"":{""password"":""secret123"",""apiKey"":""key123""}}}";
        var context = CreateHttpContext("/api/auth", "POST", requestBody);
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("***REDACTED***")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldObfuscatePropertiesInArrays()
    {
        // Arrange
        var requestBody = @"{""users"":[{""name"":""john"",""password"":""secret1""},{""name"":""jane"",""password"":""secret2""}]}";
        var context = CreateHttpContext("/api/users", "POST", requestBody);
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("***REDACTED***")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldBeCaseInsensitive_ForSensitiveProperties()
    {
        // Arrange
        var requestBody = @"{""PASSWORD"":""secret123"",""ApiKey"":""key123""}";
        var context = CreateHttpContext("/api/test", "POST", requestBody);
        var middleware = new ObfuscationMiddleware(_nextMock.Object, _loggerMock.Object, _options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("***REDACTED***")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private HttpContext CreateHttpContext(string path, string method = "GET", string? body = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;

        if (body != null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
            context.Request.ContentType = "application/json";
        }

        context.Response.Body = new MemoryStream();
        
        return context;
    }
}
