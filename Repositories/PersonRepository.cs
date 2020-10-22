using Common;
using MongoDB.Driver;
using Repositories.Base;
using Repositories.Contracts;
using Repositories.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonRepository : BaseRepositoryMongo<Person>, IPersonRepository
    {
        public PersonRepository(IMongoClient mongoClient, ISettings settings, ILogger logger)
            : base(mongoClient, settings, logger)
        { }

        public async Task<List<Person>> FindAdultPersons()
        {
            var builder = Builders<Person>.Filter;
            var filter = builder.Gte(p => p.Age, 18);

            var collection = GetMongoCollection();
            var asyncCursor = await collection.FindAsync(filter);

            return asyncCursor.ToList();
        }
    }
}
