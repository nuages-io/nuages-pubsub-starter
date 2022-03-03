using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.Mongo;
using Nuages.PubSub.WebSocket.Endpoints;
using Nuages.Web;

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
            .AddEnvironmentVariables();
     
        var configuration = builder.Build();

        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
        
        if (config.ParameterStore.Enabled)
        {
            builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = config.ParameterStore.Path;
                configureSource.Optional = true;
            });
        }

        if (config.AppConfig.Enabled)
        {
            builder.AddAppConfig(config.AppConfig.ApplicationId,  
                config.AppConfig.EnvironmentId, 
                config.AppConfig.ConfigProfileId,true);
        }
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);

        var  pubSubBuilder = serviceCollection
            .AddPubSubService(configuration, _ =>
            {
                
            });
        
        var pubSubRouteBuilder = pubSubBuilder.AddPubSubLambdaRoutes();

        var useExternalAuth = configuration.GetValue<bool>("Nuages:PubSub:ExternalAuth:Enabled");
        if (useExternalAuth)
            pubSubRouteBuilder.UseExternalAuthRoute();
        
        ConfigStorage(pubSubBuilder, configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);
        
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
    }

    private static void ConfigStorage(IPubSubBuilder pubSubBuilder, IConfiguration configuration)
    {
        var storage = configuration["Nuages:PubSub:Data:Storage"];

        switch (storage)
        {
            case "DynamoDb":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDb":
            {
                pubSubBuilder.AddPubSubMongoStorage(configOptions =>
                {
                    configOptions.ConnectionString = configuration["Nuages:PubSub:Data:ConnectionString"];
                });
                break;
            }
            case "MySql":
            {
                pubSubBuilder.AddPubSubMySqlStorage(configOptions =>
                {
                    var connectionString = configuration["Nuages:PubSub:Data:ConnectionString"];
                    configOptions.UseMySQL(connectionString);
                });

                break;
            }
            default:
            {
                throw new NotSupportedException("Storage not supported");
            }
        }
    }
}