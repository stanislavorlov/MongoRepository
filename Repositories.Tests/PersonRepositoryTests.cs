using AutoFixture;
using Common;
using MongoDB.Driver;
using Moq;
using Repositories.Entities;
using Repositories.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Repositories.Tests
{
    public class PersonRepositoryTests
    {
        private readonly PersonRepository personRepository;

        private readonly string DbName;
        private readonly string CollectionName;

        private readonly Mock<IMongoClient> mongoClientMock;
        private readonly Mock<IMongoDatabase> mongoDatabaseMock;
        private readonly Mock<ISettings> settingsMock;
        private readonly Mock<ILogger> loggerMock;

        private readonly IFixture fixture;

        public PersonRepositoryTests()
        {
            fixture = new Fixture();

            mongoClientMock = new Mock<IMongoClient>();
            mongoDatabaseMock = new Mock<IMongoDatabase>();
            settingsMock = new Mock<ISettings>();
            loggerMock = new Mock<ILogger>();

            DbName = fixture.Build<string>().Create();
            CollectionName = fixture.Build<string>().Create();

            SetupSettings();

            personRepository = new PersonRepository(mongoClientMock.Object,
                settingsMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public async Task Test_FindAdultPersons_Ok()
        {
            var collectionMock = SetupMongoCollectionMock();

            var persons = new List<Person> { new Person { Id = Guid.NewGuid(), Age = 25 } };

            Mock<IAsyncCursor<Person>> asyncPersonCursorMock = SetupMongoCursorMock(persons);

            var filterBuilder = Builders<Person>.Filter;
            var filter = filterBuilder.Gte(p => p.Age, 18);

            var expectedFilterAsBsonDoc = filter.RenderToJson();

            collectionMock
                .Setup(_ => _.FindAsync(
                    It.Is<FilterDefinition<Person>>(_ => _.RenderToJson().Equals(expectedFilterAsBsonDoc)),
                    It.IsAny<FindOptions<Person, Person>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(asyncPersonCursorMock.Object);

            var actual = await personRepository.FindAdultPersons();

            Assert.True(persons.SequenceEqual(actual));
        }

        [Fact]
        public async Task Test_CreateAsync()
        {
            var collectionMock = SetupMongoCollectionMock();

            var person = new Person { };

            collectionMock
                .Setup(_ => _.InsertOneAsync(person, null, default))
                .Callback<Person, InsertOneOptions, CancellationToken>((document, options, cancellationToken) =>
                {
                    Assert.Equal(person, document);
                });

            _ = await personRepository.CreateAsync(person);
        }

        private Mock<IMongoCollection<Person>> SetupMongoCollectionMock()
        {
            var collectionMock = new Mock<IMongoCollection<Person>>();

            mongoClientMock
                .Setup(_ => _.GetDatabase(DbName, null))
                .Returns(mongoDatabaseMock.Object);

            collectionMock
                .Setup(_ => _.Database)
                .Returns(mongoDatabaseMock.Object);

            mongoDatabaseMock
                .Setup(_ => _.GetCollection<Person>(CollectionName, null))
                .Returns(collectionMock.Object);

            return collectionMock;
        }

        private Mock<IAsyncCursor<Person>> SetupMongoCursorMock(List<Person> persons)
        {
            Mock<IAsyncCursor<Person>> asyncPersonCursorMock = new Mock<IAsyncCursor<Person>>();

            asyncPersonCursorMock
                .Setup(_ => _.Current)
                .Returns(persons);

            asyncPersonCursorMock
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            asyncPersonCursorMock
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            return asyncPersonCursorMock;
        }

        private void SetupSettings()
        {
            settingsMock
                .SetupGet(_ => _.MongoDbName)
                .Returns(DbName);
            settingsMock
                .SetupGet(_ => _.MongoCollectionName)
                .Returns(CollectionName);
        }
    }
}
