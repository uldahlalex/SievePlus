using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using api.Services;
using dataccess;
using Microsoft.EntityFrameworkCore;
using NSwag.Generation.Processors.Collections;
using Sieve.Plus.Models;
using Sieve.Plus.Services;
using Testcontainers.PostgreSql;

namespace api;

public class Program
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AppOptions>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var appOptions = new AppOptions();
            configuration.GetSection(nameof(AppOptions)).Bind(appOptions);
            return appOptions;
        });
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment!="Production")
        {
                 var postgreSqlContainer = new PostgreSqlBuilder().Build();
                            postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
                            var connectionString = postgreSqlContainer.GetConnectionString();
                            services.AddDbContext<MyDbContext>((services, options) =>
                            {
                                options.UseNpgsql(connectionString);
                                // options.UseQueryTrackingBehavior(QueryTrackingBehavior
                                //     .NoTrackingWithIdentityResolution);
                            });
        }
        else
        {
            services.AddDbContext<MyDbContext>((services, options) =>
            {
                options.UseNpgsql(services.GetRequiredService<AppOptions>().Db);
            }, ServiceLifetime.Transient);
        }
        services.AddControllers().AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        });
        services.AddOpenApiDocument(config =>
        {
            // Add string constants to schemas - generates TypeScript constants
            config.AddStringConstants(typeof(SieveConstants));
        });
        services.AddCors();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<ISeeder, SieveTestSeeder>();
        services.AddExceptionHandler<MyGlobalExceptionHandler>();
        services.Configure<SievePlusOptions>(options =>
        {
            options.CaseSensitive = false;
            options.DefaultPageSize = 10;
            options.MaxPageSize = 100;
        });
     
        services.AddScoped<ISievePlusProcessor, ApplicationSievePlusProcessor>();
    }

    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        
        ConfigureServices(builder.Services);
        var app = builder.Build();


        var appOptions = app.Services.GetRequiredService<AppOptions>();
        //Here im just checking that I can get the "Db" connection string - it throws exception if not minimum 1 length
        Validator.ValidateObject(appOptions, new ValidationContext(appOptions), true);
        app.UseExceptionHandler(config => { });
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x => true));
        app.MapControllers();
        app.GenerateApiClientsFromOpenApi("/../../client/src/generated-client.ts").GetAwaiter().GetResult();
         if (app.Environment.IsDevelopment())
            using (var scope = app.Services.CreateScope())
                scope.ServiceProvider.GetRequiredService<ISeeder>().Seed().GetAwaiter().GetResult();
        
        
         app.Run();
    }
}