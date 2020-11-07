namespace Common
{
    public class Settings : ISettings
    {
        public string MongoDbName { get; set; }

        public string MongoCollectionName { get; set; }
    }
}
