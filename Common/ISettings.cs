namespace Common
{
    public interface ISettings
    {
        public string MongoDbName { get; }

        public string MongoCollectionName { get; }
    }
}
