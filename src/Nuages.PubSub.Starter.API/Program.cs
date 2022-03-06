using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.Mongo;
using Nuages.Web;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = builder.Configuration.AddJsonFile("appsettings.json", false, true).AddEnvironmentVariables();
            
var config = builder.Configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
        
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



var secretProvider = new AWSSecretProvider();
secretProvider.TransformSecret(builder.Configuration, "Nuages:PubSub:Data:ConnectionString");
secretProvider.TransformSecret(builder.Configuration, "Nuages:PubSub:Auth:Secret");

var configuration = configBuilder.Build();

builder.Services.AddSingleton(configuration);

var pubSubBuilder = builder.Services.AddPubSubService(configuration);

ConfigStorage();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    AWSXRayRecorder.InitializeInstance(configuration);
    AWSSDKHandler.RegisterXRayForAllServices();

    app.UseXRay(configuration["Nuages:PubSub:StackName"]);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/",
        async context => { await context.Response.WriteAsync("PubSubStarter"); });
});


app.Run();

void ConfigStorage()
{
    var storage = configuration["Nuages:PubSub:Data:Storage"];

    switch (storage)
    {
        case "DynamoDB":
        {
            pubSubBuilder.AddPubSubDynamoDbStorage();
            break;
        }
        case "MongoDB":
        {
            pubSubBuilder.AddPubSubMongoStorage(configMongo =>
            {
                configMongo.ConnectionString = configuration["Nuages:PubSub:Data:ConnectionString"];
            });
            break;
        }
        case "MySQL":
        {
            pubSubBuilder.AddPubSubMySqlStorage(configMySql =>
            {
                var connectionString = configuration["Nuages:PubSub:Data:ConnectionString"];
                configMySql.UseMySQL(connectionString);
            });

            break;
        }
        default:
        {
            throw new NotSupportedException("Storage not supported");
        }
    }
}
