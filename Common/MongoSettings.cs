namespace Common
{
    public class MongoSettings : IMongoSettings
    {
        public string MongoDbName { get; set; }

        public string MongoCollectionName { get; set; }
    }
}
