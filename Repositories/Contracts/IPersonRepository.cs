using Repositories.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    interface IPersonRepository : IRepository<Person>
    {
        public Task<List<Person>> FindAdultPersons();
    }
}
