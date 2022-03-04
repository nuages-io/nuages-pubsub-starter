using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SecretsManager;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.AWS.Secrets;
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
    class SecretValue
    {
        public string Value { get; set; } = string.Empty;
    }
    
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
        
        //Here we are going to read the connection string secret...if this is a scret
        var connectionString = configuration["Nuages:PubSub:Data:ConnectionString"];
        Console.WriteLine($"Initial connection string = {connectionString}");
        if (!string.IsNullOrEmpty(connectionString))
        {
            if (connectionString.StartsWith("arn:aws:secretsmanager"))
            {
                //Here we are going to read the connection string secret...if this is a scret
                var secretProvider = new AWSSecretProvider(new AmazonSecretsManagerClient());

                var secret = secretProvider.GetSecretAsync<SecretValue>(connectionString).Result;
                if (secret != null)
                {
                    Console.WriteLine($"Real connection string = { secret.Value}");
                    builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new ("Nuages:PubSub:Data:ConnectionString",  secret.Value)
                    });
                }
            }
        }

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);

        var  pubSubBuilder = serviceCollection
            .AddPubSubService(configuration);
        
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
            case "DynamoDB":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDB":
            {
                pubSubBuilder.AddPubSubMongoStorage(configOptions =>
                {
                    configOptions.ConnectionString = configuration["Nuages:PubSub:Data:ConnectionString"];
                });
                break;
            }
            case "MySQL":
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