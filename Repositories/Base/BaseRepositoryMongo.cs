﻿using Common;
using MongoDB.Driver;
using Repositories.Contracts;
using Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Repositories.Base
{
    public abstract class BaseRepositoryMongo<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly IMongoClient MongoClient;
        protected readonly ISettings Settings;
        protected readonly ILogger Logger;

        protected BaseRepositoryMongo() { }

        protected BaseRepositoryMongo(IMongoClient mongoClient, ISettings settings, ILogger logger)
            => (MongoClient, Settings, Logger) = (mongoClient, settings, logger);

        public virtual async Task<bool> CreateAsync(T model, CancellationToken cancellationToken = default)
        {
            var collection = GetMongoCollection();

            try
            {
                await collection.InsertOneAsync(model, new InsertOneOptions 
                { 
                    BypassDocumentValidation = true 
                }, cancellationToken);

                return true;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception);

                return false;
            }
        }

        public virtual async Task<bool> UpdateAsync(T model, CancellationToken cancellationToken = default)
        {
            var collection = GetMongoCollection();

            try
            {
                var updateResult = await collection.ReplaceOneAsync(x => x.Id == model.Id,
                    replacement: model, new ReplaceOptions
                    {
                        BypassDocumentValidation = true
                    },
                    cancellationToken: cancellationToken);

                return updateResult.IsAcknowledged;
            }
            catch (Exception exception)
            {
                Logger.LogError(exception);

                return false;
            }
        }

        public virtual async Task<T> FindAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var collection = GetMongoCollection();

            var result = await collection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            var collection = GetMongoCollection();

            var result = await collection
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public virtual async Task<List<T>> FindAsync(FilterDefinition<T> filter, 
            SortDefinition<T> sort, 
            int? skip, int? limit,
            CancellationToken cancellationToken = default)
        {
            var collection = GetMongoCollection();

            return await collection
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync(cancellationToken);
        }

        protected IMongoCollection<T> GetMongoCollection()
        {
            var database = MongoClient.GetDatabase(Settings.MongoDbName);

            return database.GetCollection<T>(Settings.MongoCollectionName);
        }
    }
}
