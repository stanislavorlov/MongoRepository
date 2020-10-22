using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Repositories.Entities
{
    public class BaseEntity
    {
        [BsonId]
        public Guid Id { get; set; }
    }
}
