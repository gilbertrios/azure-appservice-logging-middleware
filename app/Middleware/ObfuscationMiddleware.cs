namespace AzureAppServiceLoggingMiddleware.Middleware;

/// <summary>
/// Middleware that captures request and response bodies, obfuscates sensitive properties,
/// and logs them as custom properties to Application Insights telemetry.
/// </summary>
public class ObfuscationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ObfuscationMiddleware> _logger;
    private readonly ObfuscationOptions _options;

    public ObfuscationMiddleware(
        RequestDelegate next,
        ILogger<ObfuscationMiddleware> logger,
        ObfuscationOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check endpoints
        if (IsHealthCheckEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        string? requestBody = null;
        string? responseBody = null;

        try
        {
            // Capture request body
            requestBody = await CaptureRequestBodyAsync(context.Request);

            // Capture response body by replacing the response stream
            var originalResponseBody = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Call the next middleware
            await _next(context);

            // Capture response body
            responseBody = await CaptureResponseBodyAsync(responseBodyStream);

            // Copy response back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            // Process and log obfuscated bodies
            await LogObfuscatedBodiesAsync(context, requestBody, responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ObfuscationMiddleware");
            throw;
        }
    }

    private async Task<string?> CaptureRequestBodyAsync(HttpRequest request)
    {
        if (!request.ContentLength.HasValue || request.ContentLength == 0)
            return null;

        request.EnableBuffering();
        
        using var reader = new StreamReader(
            request.Body,
            encoding: System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return body;
    }

    private async Task<string> CaptureResponseBodyAsync(MemoryStream responseStream)
    {
        responseStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(
            responseStream,
            encoding: System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        
        var body = await reader.ReadToEndAsync();
        responseStream.Seek(0, SeekOrigin.Begin);
        return body;
    }

    private async Task LogObfuscatedBodiesAsync(HttpContext context, string? requestBody, string? responseBody)
    {
        var obfuscatedRequest = ObfuscateJson(requestBody);
        var obfuscatedResponse = ObfuscateJson(responseBody);

        // Log as custom properties (will flow to Application Insights)
        var customProperties = new Dictionary<string, object>
        {
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["StatusCode"] = context.Response.StatusCode,
            ["ObfuscatedRequest"] = obfuscatedRequest ?? "null",
            ["ObfuscatedResponse"] = obfuscatedResponse ?? "null"
        };

        using (_logger.BeginScope(customProperties))
        {
            _logger.LogInformation(
                "Request processed: {Method} {Path} - Status: {StatusCode}\n" +
                "Obfuscated Request: {ObfuscatedRequest}\n" +
                "Obfuscated Response: {ObfuscatedResponse}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                obfuscatedRequest ?? "null",
                obfuscatedResponse ?? "null");
        }

        await Task.CompletedTask;
    }

    private string? ObfuscateJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var jsonDocument = System.Text.Json.JsonDocument.Parse(json);
            var obfuscated = ObfuscateJsonElement(jsonDocument.RootElement);
            return System.Text.Json.JsonSerializer.Serialize(obfuscated);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON for obfuscation");
            return "[Invalid JSON]";
        }
    }

    private object ObfuscateJsonElement(System.Text.Json.JsonElement element)
    {
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Object:
                return ObfuscateJsonObject(element);

            case System.Text.Json.JsonValueKind.Array:
                return ObfuscateJsonArray(element);

            case System.Text.Json.JsonValueKind.String:
                return element.GetString() ?? string.Empty;

            case System.Text.Json.JsonValueKind.Number:
                return element.GetRawText();

            case System.Text.Json.JsonValueKind.True:
                return true;

            case System.Text.Json.JsonValueKind.False:
                return false;

            case System.Text.Json.JsonValueKind.Null:
                return null!;

            default:
                return element.GetRawText();
        }
    }

    private Dictionary<string, object> ObfuscateJsonObject(System.Text.Json.JsonElement element)
    {
        var result = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            // Check if property name matches any sensitive property in configuration
            if (ShouldObfuscateProperty(property.Name))
            {
                result[property.Name] = ObfuscateValue(property.Value);
            }
            else
            {
                // Recursively process nested objects and arrays
                result[property.Name] = ObfuscateJsonElement(property.Value);
            }
        }

        return result;
    }

    private List<object> ObfuscateJsonArray(System.Text.Json.JsonElement element)
    {
        var result = new List<object>();

        foreach (var item in element.EnumerateArray())
        {
            result.Add(ObfuscateJsonElement(item));
        }

        return result;
    }

    private bool ShouldObfuscateProperty(string propertyName)
    {
        // Case-insensitive comparison with configured sensitive properties
        return _options.SensitiveProperties.Any(
            p => string.Equals(p, propertyName, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsHealthCheckEndpoint(PathString path)
    {
        // Skip logging for health check and root endpoints
        return path.Value == "/" || path.Value == "/health";
    }

    private string ObfuscateValue(System.Text.Json.JsonElement value)
    {
        if (value.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var stringValue = value.GetString();
            return string.IsNullOrEmpty(stringValue) 
                ? stringValue ?? string.Empty
                : _options.ObfuscationMask;
        }

        // For non-string values (numbers, booleans), mask them too
        return _options.ObfuscationMask;
    }
}
