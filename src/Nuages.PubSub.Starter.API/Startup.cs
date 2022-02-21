using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Options;
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

        var pubSubBuilder = services
            .AddPubSubService(_configuration);

        pubSubBuilder.AddPubSubDynamoDbStorage();

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
                    Url = "https://github.com/nuages-io/nuages-pubsub"
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
            var stackName = _configuration.GetSection("Nuages:PubSub:StackName").Value;

            AWSXRayRecorder.InitializeInstance(_configuration);
            AWSSDKHandler.RegisterXRayForAllServices();

            app.UseXRay(stackName);
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/",
                async context =>
                {
                    var option = app.ApplicationServices.GetService<IOptions<PubSubOptions>>();

                    var config = new
                    {
                        PubSubOptions = option,
                        Storage = _configuration.GetSection("Nuages:Data:Storage").Value
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(config, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
                });
        });

        app.UseOpenApi();
        app.UseSwaggerUi3();
    }
}