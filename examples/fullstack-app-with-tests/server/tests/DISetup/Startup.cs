using api;
using Microsoft.Extensions.DependencyInjection;

namespace tests.DISetup;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        
        Program.ConfigureServices(services);

    }
}
