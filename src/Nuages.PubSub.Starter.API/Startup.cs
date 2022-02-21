using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;

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

        services
            .AddPubSubService(_configuration).AddPubSubDynamoDbStorage();

        //===================================================================
        // To use MongoDB
        //===================================================================
        // 1. Add a refernce to nuget package Nuages.PubSub.Storage.MongoDb
        //
        // 2. replace previous line by  
        // services
        //     .AddPubSubService(_configuration).AddPubSubMongoStorage(config =>
        //      {
        //          config.ConnectionString = "";
        //          config.DatabaseName = "";
        //      });
        //
        // 3. Remove reference to Nuages.PubSub.Storage.DynamoDb
        //
        // 3. Apply the same changes to Nuages.PubSub.Starter.WebSocket (PubSubFunction)
        
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "Nuages WebSocket Service";

                document.Info.Contact = new NSwag.OpenApiContact
                {
                    Name = "Nuages.io",
                    Email = string.Empty,
                    Url = "https://github.com/nuages-io/nuages-pubsub-starter"
                };
                document.Info.License = new NSwag.OpenApiLicense
                {
                    Name = "Use under LICENCE",
                    Url = "http://www.apache.org/licenses/LICENSE-2.0"
                };
            };
        });
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

        app.UseOpenApi();
        app.UseSwaggerUi3();
    }
}