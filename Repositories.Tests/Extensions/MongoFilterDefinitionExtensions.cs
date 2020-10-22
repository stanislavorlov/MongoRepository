using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Repositories.Tests.Extensions
{
    public static class MongoFilterDefinitionExtensions
    {
        public static string RenderToJson<TDocument>(this FilterDefinition<TDocument> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<TDocument>();

            return filter.Render(documentSerializer, serializerRegistry).ToJson();
        }
    }
}
