using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepository<T>
    {
        Task<bool> CreateAsync(T model, CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(T model, CancellationToken cancellationToken = default);

        Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default);

        Task<T> FindAsync(Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default);

        Task<List<T>> FindAsync(FilterDefinition<T> filter,
            SortDefinition<T> sort,
            int? skip, int? limit,
            CancellationToken cancellationToken = default);
    }
}
