namespace Common
{
    public interface IMongoSettings
    {
        public string MongoDbName { get; }

        public string MongoCollectionName { get; }
    }
}
