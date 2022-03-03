using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.Mongo;

namespace Nuages.PubSub.Starter.API;

[ExcludeFromCodeCoverage]
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static IConfiguration _configuration = null!;

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_configuration);

        var pubSubBuilder = services
            .AddPubSubService(_configuration);

        ConfigStorage(pubSubBuilder);

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    private static void ConfigStorage(IPubSubBuilder pubSubBuilder)
    {
        var storage = _configuration["Nuages:PubSub:Data:Storage"];

        switch (storage)
        {
            case "DynamoDb":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDb":
            {
                pubSubBuilder.AddPubSubMongoStorage(config =>
                {
                    config.ConnectionString = _configuration["Nuages:PubSub:Data:ConnectionString"];
                });
                break;
            }
            case "MySql":
            {
                pubSubBuilder.AddPubSubMySqlStorage(config =>
                {
                    var connectionString = _configuration["Nuages:PubSub:Data:ConnectionString"];
                    config.UseMySQL(connectionString);
                });

                break;
            }
            default:
            {
                throw new NotSupportedException("Storage not supported");
            }
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            AWSXRayRecorder.InitializeInstance(_configuration);
            AWSSDKHandler.RegisterXRayForAllServices();

            app.UseXRay(_configuration["Nuages:PubSub:StackName"]);
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    }
}