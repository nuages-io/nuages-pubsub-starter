using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.WebSocket.Endpoints;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.Starter.WebSocket;

// ReSharper disable once ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage]
public class PubSubFunction : Nuages.PubSub.WebSocket.Endpoints.PubSubFunction
{
    public PubSubFunction() 
    {
        
        var configManager = new ConfigurationManager();

        var builder = configManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  true, true)
            .AddEnvironmentVariables();
     
        
        var name = Environment.GetEnvironmentVariable("Nuages__PubSub__StackName");

        if (name != null)
        {
            builder.AddSystemsManager(configureSource =>
            {
                // Parameter Store prefix to pull configuration data from.
                configureSource.Path = $"/{name}/WebSocket";

                // Reload configuration data every 5 minutes.
                configureSource.ReloadAfter = TimeSpan.FromMinutes(15);

                // Configure if the configuration data is optional.
                configureSource.Optional = true;

                configureSource.OnLoadException += _ =>
                {
                    // Add custom error handling. For example, look at the exceptionContext.Exception and decide
                    // whether to ignore the error or tell the provider to attempt to reload.
                };
            });
        }
        
        IConfiguration configuration = builder.Build();
            
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);
            
        var pubSubBuilder = 
            serviceCollection.AddPubSubLambdaRoutes(configuration)
            .AddPubSubService();

        pubSubBuilder.AddPubSubDynamoDbStorage();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);
    }
}