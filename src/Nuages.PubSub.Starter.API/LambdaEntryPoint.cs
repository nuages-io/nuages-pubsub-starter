using System.Diagnostics.CodeAnalysis;
using Amazon.SecretsManager;
using NLog.Web;
using Nuages.AWS.Secrets;
using Nuages.Web;

namespace Nuages.PubSub.Starter.API;

[ExcludeFromCodeCoverage]
// ReSharper disable once UnusedType.Global
public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    class SecretValue
    {
        public string Value { get; set; } = string.Empty;
    }
    
    protected override void Init(IWebHostBuilder builder)
    {
        builder.UseStartup<Startup>();
    }

    protected override void Init(IHostBuilder builder)
    {
        // ReSharper disable once UnusedParameter.Local
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json", false, true);
            configBuilder.AddEnvironmentVariables();
            
            var configuration = configBuilder.Build();

            var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
        
            if (config.ParameterStore.Enabled)
            {
                configBuilder.AddSystemsManager(configureSource =>
                {
                    configureSource.Path = config.ParameterStore.Path;
                    configureSource.Optional = true;
                });
            }

            if (config.AppConfig.Enabled)
            {
                configBuilder.AddAppConfig(config.AppConfig.ApplicationId,  
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
                    var secretProvider = new AWSSecretProvider(new AmazonSecretsManagerClient());

                    var secret = secretProvider.GetSecretAsync<SecretValue>(connectionString).Result;
                    if (secret != null)
                    {
                        Console.WriteLine($"Real connection string = { secret.Value}");
                        configBuilder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                        {
                            new ("Nuages:PubSub:Data:ConnectionString",  secret.Value)
                        });
                    }
                }
            }
          
            
        }).UseNLog();


    }
}