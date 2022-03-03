using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Cdk;
using Nuages.PubSub.Starter.WebSocket;

namespace Nuages.PubSub.Starter.Cdk;

[ExcludeFromCodeCoverage]
public class StarterPubSubStack : PubSubWebSocketCdkStack<PubSubFunction>
{
    public static void CreateStack(Construct scope, IConfiguration configuration, ApplicationSettings applicationSettings)
    {
        var stack = new StarterPubSubStack(scope, "Stack", configuration, new StackProps
        {
            StackName = applicationSettings.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        })
        {
            //Web Socket Endpoint
            //Other variable you may want to set
            WebSocketDomainName = applicationSettings.WebSocketDomainName,
            WebSocketCertificateArn = applicationSettings.WebSocketCertificateArn,
            
            //API Endpoint
            ApiDomainName = applicationSettings.ApiDomainName,
            ApiCertificateArn = applicationSettings.ApiCertificateArn,
            ApiApiKey = applicationSettings.ApiApiKey, //Leave null and it will be auto generated. See API GAteway API Key section in the AWS console to retrieve it.
            
            // Other example here https://blog.theodo.com/2020/01/internet-access-to-lambda-in-vpc/
            // More info here https://aws.amazon.com/premiumsupport/knowledge-center/internet-access-lambda-function/
            // WARNING!!!!!  Be aware of the restirction regarding Internet access when adding a Lmabda to a VPC
            //
            //VPC is required if you use a database proxy.
            VpcId = applicationSettings.VpcId,
            SecurityGroupId = applicationSettings.SecurityGroupId,
            
            //DatabaseProxy, if using MySql
            DatabaseProxyArn = applicationSettings.DatabaseProxyArn,
            DatabaseProxyEndpoint = applicationSettings.DatabaseProxyEndpoint,
            DatabaseProxyName = applicationSettings.DatabaseProxyName,
            DatabaseProxyUser = applicationSettings.DatabaseProxyUser,
        };

        stack.BuildStack();

    }
    
    // ReSharper disable once UnusedParameter.Local
    private StarterPubSubStack(Construct scope, string id, IConfiguration configuration, IStackProps? props = null) 
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