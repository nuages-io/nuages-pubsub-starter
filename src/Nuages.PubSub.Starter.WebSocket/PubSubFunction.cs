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
                configureSource.Path = $"/{name}/WebSocket";
                configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                configureSource.Optional = true;
            });
        }
        
        IConfiguration configuration = builder.Build();
            
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);
            
        serviceCollection
            .AddPubSubLambdaRoutes(configuration)
            .AddPubSubService()
            .AddPubSubDynamoDbStorage();

        //===================================================================
        // To use MongoDB
        //===================================================================
        // 1. Add a refernce to nuget package Nuages.PubSub.Storage.MongoDb
        //
        // 2. replace previous line by  
        // serviceCollection
        //     .AddPubSubLambdaRoutes(configuration)
        //     .AddPubSubService()
        //     .AddPubSubMongoStorage(config =>
        //      {
        //          config.ConnectionString = "";
        //          config.DatabaseName = "";
        //      });
        //
        // 3. Remove reference to Nuages.PubSub.Storage.DynamoDb
        //
        // 3. Apply the same changes to Nuages.PubSub.Starter.API (Startup)

        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);
    }
}