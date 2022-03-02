using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Cdk;
using Nuages.PubSub.Starter.WebSocket;

namespace Nuages.PubSub.Starter.Cdk;

[ExcludeFromCodeCoverage]
public class StarterNuagesPubSubStack : PubSubWebSocketCdkStack<PubSubFunction>
{
    // ReSharper disable once UnusedParameter.Local
    public StarterNuagesPubSubStack(Construct scope, string id, IConfiguration configuration, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.Starter.WebSocket/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.Starter.API/bin/Release/net6.0/linux-x64/publish";

        WebApiHandler = "Nuages.PubSub.Starter.API::Nuages.PubSub.Starter.API.LambdaEntryPoint::FunctionHandlerAsync";
    }

    protected override void AddWebApiEnvironmentVariables(Dictionary<string, string> environmentVariables)
    {   
        //Add addtional environment Variables
    }

    protected override void AddWebSocketEnvironmentVariables(Dictionary<string, string> environmentVariables)
    {
        //Add addtional environment Variables
    }
}