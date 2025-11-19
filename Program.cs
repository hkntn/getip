using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(x =>
{
    x.AddServerHeader = false;
    x.Listen(IPAddress.Any, 8080);
});

var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
};

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(s =>
    {
        var httpContext = (HttpContext)s;
        var keys = httpContext.Response.Headers.Keys.Where(x=>!string.Equals(x, "Cache-Control", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keys)
        {
            httpContext.Response.Headers.Remove(key);
        }
        return Task.CompletedTask;
    }, context);
    await next();
});

app.MapGet("/favicon.ico", (HttpContext ctx) =>
{
    ctx.Response.Headers["Cache-Control"] = "public, max-age=31536000";
    return Results.NoContent();
});


app.MapFallback((HttpContext context) =>
{
    var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
             ?? context.Connection.RemoteIpAddress?.ToString() 
             ?? "unknown";
    
    return Results.Text(ip);
});

app.MapGet("/detailed/json", (HttpContext context) =>
{
    var details = new
    {
        Method = context.Request.Method,
        Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
        Query = context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
        RemoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
    };

    return Results.Json(details, serializerOptions);
});

app.MapGet("/detailed", (HttpContext context) =>
{
    var method = context.Request.Method;
    var headers = context.Request.Headers
        .Select(h => $"{h.Key}: {h.Value}")
        .ToList();
    var queryParams = context.Request.Query
        .Select(q => $"{q.Key}: {q.Value}")
        .ToList();
    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    var details = $"Method: {method}\n" +
                  $"Remote IP: {remoteIp}\n" +
                  "Headers:\n" +
                  string.Join("\n", headers) + "\n" +
                  "Query Parameters:\n" +
                  string.Join("\n", queryParams);

    return Results.Text(details);
});



app.Run();
