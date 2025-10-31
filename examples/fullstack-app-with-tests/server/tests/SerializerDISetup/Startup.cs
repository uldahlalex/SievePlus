using System.Text.Json;
using api;
using api.DTOs.QueryModels;
using api.Services;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Plus.Models;

namespace tests.SerializerDISetup;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Program.ConfigureServices(services);

    }
}



public class MyTests(ITestOutputHelper outputHelper, 
    ComputerStoreController controller, 
    ISeeder seeder) : IAsyncLifetime
{
    

    [Fact]
    public async Task MySerializerDITest()
    {
        var result = await controller.GetComputers(new SievePlusRequest<ComputerQueryModel>());
        
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask InitializeAsync()
    {
        seeder.Seed();
        return ValueTask.CompletedTask;
    }
}
