using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1");

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        var codespaceName = Environment.GetEnvironmentVariable("CODESPACE_NAME");
        var domain = Environment.GetEnvironmentVariable("CODESPACES_PORT_FORWARDING_DOMAIN")
                     ?? "app.github.dev";

        if (!string.IsNullOrEmpty(codespaceName))
        {
            var port = 5163;
            options.AddServer($"https://{codespaceName}-{port}.{domain}");
        }
    });
// }

var urls = new Dictionary<string, UrlEntry>();

app.MapGet("/", () => "URL SHORTENER");


app.MapPost("/urls", (CreateUrlRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Url)) {
        return Results.BadRequest(new {error = "url vacia gilipollas"});
    }
    var code = Guid.NewGuid().ToString()[..6];

    var entry = new UrlEntry(code, request.Url, DateTime.UtcNow);

    urls[code] = entry;

    return Results.Created($"/urls/{code}", entry);
}); // crear url acotada


app.MapDelete("/urls/{code}", (string code) =>
{
    return urls.Remove(code) ? Results.NoContent() : Results.NotFound() ;
}); // eliminar url


app.MapGet("/urls/{code}", (string code) =>
{
    return urls.TryGetValue(code, out var entry) ? Results.Redirect(entry.OriginalUrl) : Results.NotFound() ;
}); 

app.Run();

record UrlEntry(string Code, string OriginalUrl, DateTime CreatedAt);

record CreateUrlRequest(string Url);