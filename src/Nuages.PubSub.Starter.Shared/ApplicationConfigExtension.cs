using Microsoft.Extensions.Configuration;
using Nuages.Web;

namespace Nuages.PubSub.Starter.Shared;

public static class ApplicationConfigExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IConfigurationBuilder AddApplicationConfig(this IConfigurationBuilder builder, IConfiguration configuration)
    {
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

        return builder;

    }
}