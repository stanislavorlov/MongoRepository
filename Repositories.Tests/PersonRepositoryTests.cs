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

        private readonly Mock<IMongoClient> mongoClientMock;
        private readonly Mock<IMongoDatabase> mongoDatabaseMock;
        private readonly Mock<ILogger> loggerMock;

        private readonly IFixture fixture;
        private readonly IMongoSettings settings;

        public PersonRepositoryTests()
        {
            fixture = new Fixture();

            mongoClientMock = new Mock<IMongoClient>();
            mongoDatabaseMock = new Mock<IMongoDatabase>();
            loggerMock = new Mock<ILogger>();

            settings = fixture.Build<MongoSettings>().Create();

            personRepository = new PersonRepository(mongoClientMock.Object,
                settings,
                loggerMock.Object);
        }

        [Fact]
        public async Task Test_FindAdultPersons_Ok()
        {
            var random = new Random();

            var collectionMock = SetupMongoCollectionMock();

            var persons = new List<Person> 
            { 
                fixture
                    .Build<Person>()
                    .With(_ => _.Age, random.Next(18, 100))
                    .Create()
            };

            Mock<IAsyncCursor<Person>> asyncPersonCursorMock = SetupMongoCursorMock(persons);

            var filterBuilder = Builders<Person>.Filter;
            var filter = filterBuilder.Gte(_ => _.Age, 18);

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

            var person = fixture.Create<Person>();

            Person actual = null;
            collectionMock
                .Setup(_ => _.InsertOneAsync(It.IsAny<Person>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                .Callback<Person, InsertOneOptions, CancellationToken>((document, options, cancellationToken) =>
                {
                    actual = document;
                });

            _ = await personRepository.CreateAsync(person);

            Assert.Equal(person, actual);
        }

        private Mock<IMongoCollection<Person>> SetupMongoCollectionMock()
        {
            var collectionMock = new Mock<IMongoCollection<Person>>();

            mongoClientMock
                .Setup(_ => _.GetDatabase(settings.MongoDbName, null))
                .Returns(mongoDatabaseMock.Object);

            collectionMock
                .Setup(_ => _.Database)
                .Returns(mongoDatabaseMock.Object);

            mongoDatabaseMock
                .Setup(_ => _.GetCollection<Person>(settings.MongoCollectionName, null))
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
    }
}
