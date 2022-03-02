using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;

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

        var applicationSettings = configuration.Get<ApplicationSettings>();

        var config = applicationSettings.ApplicationConfig;
        
        if (config.ParameterStore.Enabled)
        {
            configManager.AddSystemsManager(configureSource =>
            {
                configureSource.Path = config.ParameterStore.Path;
                configureSource.Optional = true;
            });
        }

        if (config.AppConfig.Enabled)
        {
            configManager.AddAppConfig(config.AppConfig.ApplicationId,  
                config.AppConfig.EnvironmentId, 
                config.AppConfig.ConfigProfileId,true);
        }

        var app = new App();

        var stackname = applicationSettings.StackName;
        
        var stack = new StarterNuagesPubSubStack(app, stackname, configuration, new StackProps
        {
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

        app.Synth();
    }
}