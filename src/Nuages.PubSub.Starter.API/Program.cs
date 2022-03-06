using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;
using Nuages.PubSub.Starter.Shared;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = builder.Configuration
                    .AddJsonFile("appsettings.json", false, true)
                    .AddEnvironmentVariables();
            
configBuilder.AddApplicationConfig(builder.Configuration);

var secretProvider = new AWSSecretProvider();
secretProvider.TransformSecret(builder.Configuration, "Nuages:PubSub:Data:ConnectionString");
secretProvider.TransformSecret(builder.Configuration, "Nuages:PubSub:Auth:Secret");

var configuration = configBuilder.Build();

builder.Services.AddSingleton(configuration);

builder.Services
       .AddPubSubService(configuration)
        .AddStorage();


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


