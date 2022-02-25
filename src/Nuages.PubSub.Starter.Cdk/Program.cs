using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
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
        });

        stack.DataStorage = configuration[ContextValues.DataStorage];
        
        //You SHOULD change the value for the following options
        
        stack.AuthIssuer = configuration[ContextValues.AuthIssuer];
        stack.AuthAudience = configuration[ContextValues.AuthAudience];
        stack.AuthSecret = configuration[ContextValues.AuthSecret];
        
        //Other variable you may want to set
        
        //Web Socket Endpoint
        stack.WebSocketDomainName = null;
        stack.WebSocketCertificateArn = null;
        
        //API Endpoint
        stack.ApiDomainName = null;
        stack.ApiCertificateArn = null;
        stack.ApiApiKey = null; //Leave null and it will be auto generated. See API GAteway API Key section in the AWS console to retrieve it.

        //Database options
        stack.DataPort = null; //Assign port if different from the default port from database engine
        stack.DataConnectionString = null;
        stack.DataCreateDynamoDbTables = false;
        
        //DatabaseProxy, if using MySql
        stack.DatabaseProxyArn = null;
        stack.DatabaseProxyEndpoint = null;
        stack.DatabaseProxyName = null;
        stack.DatabaseProxyUser = null;
        stack.DatabaseProxySecurityGroup = null;

        //VPC is required if you use a database proxy.
        //
        // WARNING!!!!!  Be aware of the restirction regarding Internet access when adding a Lmabda to a VPC
        // More info here https://aws.amazon.com/premiumsupport/knowledge-center/internet-access-lambda-function/
        // Other example here https://blog.theodo.com/2020/01/internet-access-to-lambda-in-vpc/
        stack.VpcId = null;
        
        stack.CreateTemplate();

        app.Synth();
    }
}