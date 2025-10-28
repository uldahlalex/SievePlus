using api;

namespace fixture;

/// <summary>
/// A test fixture that starts a real Kestrel server for integration testing.
/// Designed to be started by TypeScript tests via a global setup script.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5555;

        // Set environment to Development so Testcontainers is used
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        Console.WriteLine($"Starting API test server on port {port}...");

        var builder = WebApplication.CreateBuilder();

        // Configure services using the same setup as the API
        api.Program.ConfigureServices(builder.Services);

        var app = builder.Build();

        // Configure middleware (same as API Program.cs)
        app.UseExceptionHandler(config => { });
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x => true));
        app.MapControllers();

        // Generate API clients
        await app.GenerateApiClientsFromOpenApi("/../../client/src/generated-client.ts");

        // Seed database
        using (var scope = app.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<ISeeder>().Seed();

        // Start the server on the specified port
        app.Urls.Clear();
        app.Urls.Add($"http://localhost:{port}");

        Console.WriteLine($"API test server ready at http://localhost:{port}");

        // Run the server (this blocks until shutdown)
        await app.RunAsync();
    }
}
