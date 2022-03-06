using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;
using Nuages.PubSub.Starter.Shared;
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
            .AddEnvironmentVariables();
     
        
        builder.AddApplicationConfig(configManager);

        var secretProvider = new AWSSecretProvider();
        secretProvider.TransformSecret(configManager, "Nuages:PubSub:Data:ConnectionString");
        secretProvider.TransformSecret(configManager, "Nuages:PubSub:Auth:Secret");
        
        var serviceCollection = new ServiceCollection();

        var configuration = builder.Build();
        
        serviceCollection
            .AddSingleton(configuration);

        var  pubSubBuilder = serviceCollection
            .AddPubSubService(configuration).AddStorage();
        
        var pubSubRouteBuilder = pubSubBuilder.AddPubSubLambdaRoutes();

        var useExternalAuth = configuration.GetValue<bool>("Nuages:PubSub:ExternalAuth:Enabled");
        if (useExternalAuth)
            pubSubRouteBuilder.UseExternalAuthRoute();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);
        
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
    }

}