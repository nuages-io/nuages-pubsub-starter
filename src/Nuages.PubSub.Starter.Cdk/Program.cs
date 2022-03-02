using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Cdk;

namespace Nuages.PubSub.Starter.Cdk;

//ReSharper disable once ClassNeverInstantiated.Global
//ReSharper disable once ArrangeTypeModifiers
[ExcludeFromCodeCoverage]
sealed class Program
{
    // ReSharper disable once UnusedParameter.Global
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();
        
        var configuration = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddEnvironmentVariables()
            .Build();

        var app = new App();

        var stackname = configuration["StackName"];
        
        var stack = new StarterNuagesPubSubStack(app, stackname, new StackProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        })
        {
            DataStorage = configuration[ContextValues.DataStorage],
            //You SHOULD change the value for the following options
            AuthIssuer = configuration[ContextValues.AuthIssuer],
            AuthAudience = configuration[ContextValues.AuthAudience],
            AuthSecret = configuration[ContextValues.AuthSecret],
            //Web Socket Endpoint
            //Other variable you may want to set
            WebSocketDomainName = null,
            WebSocketCertificateArn = null,
            //API Endpoint
            ApiDomainName = null,
            ApiCertificateArn = null,
            ApiApiKey = null, //Leave null and it will be auto generated. See API GAteway API Key section in the AWS console to retrieve it.
            
            DataConnectionString = null,
            //DatabaseProxy, if using MySql
            DatabaseProxyArn = null,
            DatabaseProxyEndpoint = null,
            DatabaseProxyName = null,
            DatabaseProxyUser = null,
            
            // Other example here https://blog.theodo.com/2020/01/internet-access-to-lambda-in-vpc/
            // More info here https://aws.amazon.com/premiumsupport/knowledge-center/internet-access-lambda-function/
            // WARNING!!!!!  Be aware of the restirction regarding Internet access when adding a Lmabda to a VPC
            //
            //VPC is required if you use a database proxy.
            VpcId = null,
            SecurityGroupId = null
        };

        stack.CreateTemplate();

        app.Synth();
    }
}