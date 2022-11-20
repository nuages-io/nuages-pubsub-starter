using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using Nuages.Web;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();

        if (config == null)
            throw new Exception("ApplicationConfig not found");
        
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
        
        var applicationSettings = configuration.GetSection("Settings").Get<ApplicationSettings>();
        
        if (applicationSettings == null)
            throw new Exception("Settings not found");
        
        Console.WriteLine("====================================================================================================");
        Console.WriteLine(JsonSerializer.Serialize(applicationSettings));
        Console.WriteLine("====================================================================================================");
        
        var app = new App();

        if (args.Contains("--pipeline"))
        {
            StarterPubSubStackWithPipeline.Create(app, configuration, applicationSettings);
        }
        else
        {
            StarterPubSubStack.CreateStack(app, configuration, applicationSettings);
        }
        app.Synth();
    }
}