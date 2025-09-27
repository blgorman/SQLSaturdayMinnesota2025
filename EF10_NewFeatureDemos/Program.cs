using EF10_NewFeaturesDbLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System;

namespace EF10_NewFeatureDemos;

public class Program
{
    public static async Task Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
        //TODO: Toggle this to turn interceptors on or off to see them in action
        var useInterceptors = Environment.GetEnvironmentVariable("USE_INTERCEPTORS") ?? "true";
        var logToConsole = Environment.GetEnvironmentVariable("LOG_TO_CONSOLE") ?? "true"; //only works for queries when the interceptor is on
        var appSettingsFile = string.IsNullOrWhiteSpace(environment)
            ? "appsettings.json"
            : $"appsettings.{environment}.json";

        var ts = DateTime.Now.ToString("yyyyMMddHHmmss");
        var directory = @"C:\Logs";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        var logPath = $@"{directory}\logfile_{ts}.txt"; // Adjust the path as needed

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)  // 👈 no SQL commands
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", Serilog.Events.LogEventLevel.Warning)            // 👈 no query compilation
            .MinimumLevel.Override("EFCustomInterceptor", LogEventLevel.Information)
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);

        if (logToConsole.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            loggerConfig = loggerConfig.WriteTo.Console();
        }

        Log.Logger = loggerConfig.CreateLogger();

        // Banner for current config
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"Environment:     {environment}");
        Console.WriteLine($"Interceptors:    {(useInterceptors.Equals("true", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF")}");
        Console.WriteLine($"Console logging: {(logToConsole.Equals("true", StringComparison.OrdinalIgnoreCase) ? "ON" : "OFF")}");
        Console.WriteLine($"Log file:        {logPath}");
        Console.WriteLine("--------------------------------------------------");

        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables();
                config.AddUserSecrets<Program>();
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("InventoryDbConnection");
                // Register interceptors only when toggle is on
                services.AddSingleton<LoggingCommandInterceptor>();
                services.AddSingleton<SoftDeleteInterceptor>();

                if (useInterceptors.Equals("true", StringComparison.OrdinalIgnoreCase))
                {

                    services.AddDbContext<InventoryDbContext>((serviceProvider, options) =>
                    {
                        options.UseSqlServer(connectionString)
                               .AddInterceptors(
                                   serviceProvider.GetRequiredService<LoggingCommandInterceptor>(),
                                   serviceProvider.GetRequiredService<SoftDeleteInterceptor>());
                    });
                }
                else
                {
                    services.AddDbContextFactory<InventoryDbContext>(options =>
                        options.UseSqlServer(connectionString));

                }

                // Add other services here if needed
                services.AddTransient<Application>();
            }).Build();

        using var scope = host.Services.CreateScope();
        var app = scope.ServiceProvider.GetRequiredService<Application>();
        await app.DoWork();
        Log.CloseAndFlush();
    }
}
