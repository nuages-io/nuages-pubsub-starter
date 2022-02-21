using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Starter.API;

/// <summary>
/// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage]
// ReSharper disable once ClassNeverInstantiated.Global
public class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile(
                    hostingContext.HostingEnvironment.IsDevelopment()
                        ? "appsettings.local.json"
                        : "appsettings.prod.json", false, true);
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}