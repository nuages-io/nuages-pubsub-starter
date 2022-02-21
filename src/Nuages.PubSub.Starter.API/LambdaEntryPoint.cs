using System.Diagnostics.CodeAnalysis;
using NLog.Web;

namespace Nuages.PubSub.Starter.API;

[ExcludeFromCodeCoverage]
// ReSharper disable once UnusedType.Global
public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        
        builder.UseStartup<Startup>();
    }

    protected override void Init(IHostBuilder builder)
    {
        // ReSharper disable once UnusedParameter.Local
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json", false, true);
            configBuilder.AddJsonFile("appsettings.prod.json", true, true);
            configBuilder.AddEnvironmentVariables();
            
            var name = Environment.GetEnvironmentVariable("Nuages__PubSub__StackName");

            if (name != null)
            {
                configBuilder.AddSystemsManager(configureSource =>
                {
                    configureSource.Path = $"/{name}/API";
                    configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                    configureSource.Optional = true;
                });
            }
        }).UseNLog();


    }
}