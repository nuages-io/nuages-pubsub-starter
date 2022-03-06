using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.Mongo;

namespace Nuages.PubSub.Starter.Shared;

public static class StorageConfigExtension
{
    public static IPubSubBuilder AddStorage(this IPubSubBuilder pubSubBuilder, IConfiguration? configuration = null)
    {
        configuration ??= pubSubBuilder.Configuration;

        if (configuration == null)
            throw new Exception("Econfiguration is required");
        
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

        return pubSubBuilder;
    }
}