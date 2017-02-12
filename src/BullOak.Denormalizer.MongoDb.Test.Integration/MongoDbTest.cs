namespace BullOak.Denormalizer.MongoDb.Test.Integration
{
    using System;
    using Xunit;
    using System.Threading.Tasks;
    using System.Linq;
    using FluentAssertions;
    using System.Collections.Generic;
    using EventStream;

    public class MongoDbTest
    {
        // MethodName_StateUnderTest_ExpectedBehavior

        public class PersonTest
        {
            public Guid PersonId { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }

            public Guid HomeID { get; set; }
        }

        private const string DatabaseId = "local";
        private const string CollectionId = "Persons";
        private const string NewCollectionId = "My.Nested.Namespace.Class";
        private const string ConnectionString = "mongodb://localhost:27017";

        public MongoDbTest()
        {
            var db = new MongoDb(ConnectionString, DatabaseId);

            db.DeleteCollection(NewCollectionId).Wait();
            db.DeleteCollection(CollectionId).Wait();
            db.CreateCollection(CollectionId).Wait();
        }

        private async Task<MongoDb> SetupMultiplePersons()
        {
            var db = new MongoDb(ConnectionString, DatabaseId);

            var personA = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87,
                HomeID = new Guid("F79E323E-7BCD-4865-8BE7-5F674BE650CE")
            };
            var personAEnvelope = new MongoDb.MongoDocumentWrapper<PersonTest> { VM = personA, ETag = Guid.NewGuid().ToString() };
            var personB = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Wilma Flintstone",
                Age = 88,
                HomeID = new Guid("F79E323E-7BCD-4865-8BE7-5F674BE650CE")
            };
            var personBEnvelope = new MongoDb.MongoDocumentWrapper<PersonTest> { VM = personB, ETag = Guid.NewGuid().ToString() };
            var personC = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Wilma Flintstone",
                Age = 66,
                HomeID = new Guid("F79E323E-7BCD-4865-8BE7-5F674BE650CE")
            };
            var personCEnvelope = new MongoDb.MongoDocumentWrapper<PersonTest> { VM = personC, ETag = Guid.NewGuid().ToString() };
            var personD = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Wilma Flintstone",
                Age = 66,
                HomeID = new Guid("F79E323E-7BCD-4865-8BE7-5F674BE650CE")
            };
            var personDEnvelope = new MongoDb.MongoDocumentWrapper<PersonTest> { VM = personD, ETag = Guid.NewGuid().ToString() };

            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personAEnvelope);
            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personBEnvelope);
            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personCEnvelope);
            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personDEnvelope);

            return db;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void MongoDb_ConstructorParamDbIdIsNull_ExpectException(string dbId)
        {
            // Act 
            var exception = Record.Exception(() => new MongoDb(ConnectionString, dbId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: dbId");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void MongoDb_ConstructorParamConnectionStringIsNull_ExpectException(string connectionString)
        {
            // Act 
            var exception = Record.Exception(() => new MongoDb(connectionString, DatabaseId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: connectionString");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task UpsertDocument_NullCollectionId_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person, ETag = Guid.NewGuid().ToString() };

            // Act
            var exception = await Record.ExceptionAsync(() => db.UpsertDocument(collectionId, documentId, personEnvelope));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task UpsertDocument_NullDocumentId_ThrowsException(string documentId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };

            var personEnvelope = new DocumentBase<PersonTest> { VM = person, ETag = Guid.NewGuid().ToString() };

            // Act
            var exception = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, personEnvelope));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task UpsertDocument_NullItem_ThrowsException()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            DocumentBase<PersonTest> personEnvelope = null;

            // Act
            var exception = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, personEnvelope));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task UpsertDocument_NullVM_ThrowsException()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            DocumentBase<PersonTest> personEnvelope = new DocumentBase<PersonTest>();

            // Act
            var exception = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, personEnvelope));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task UpsertDocument_NewDocument_IsStoredCorrectly()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };

            var personEnvelope = new DocumentBase<PersonTest>() { VM = person };
            // Act
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            // Assert
            var doc = await db.ReadDocument<PersonTest>(CollectionId, documentId);
            doc.VM.Age.Should().Be(87);
            doc.VM.Name.Should().Be("Barney Rubble");
            doc.VM.PersonId.Should().Be(person.PersonId);
        }

        [Fact]
        public async Task UpsertDocument_UpdateDocument_IsUpdatedCorrectly()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };

            var personEnvelope = new DocumentBase<PersonTest> { VM = person };
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            personEnvelope.VM.Name = "Fred Flintstone";
            personEnvelope.VM.Age = 88;

            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            // Assert
            var doc = await db.ReadDocument<PersonTest>(CollectionId, documentId);
            doc.VM.Age.Should().Be(88);
            doc.VM.Name.Should().Be("Fred Flintstone");
            doc.VM.PersonId.Should().Be(person.PersonId);
        }

        [Fact]
        public async Task UpsertDocument_UpdateDocumentConcurrently_ThrowsException()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };

            var personEnvelope = new DocumentBase<PersonTest> { VM = person };
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            personEnvelope = await db.ReadDocument<PersonTest>(CollectionId, documentId);

            var newChange = new DocumentBase<PersonTest> { VM = person };
            newChange.VM.Name = "Change";
            newChange.ETag = personEnvelope.ETag;

            personEnvelope.VM.Name = "Fred Flintstone";
            personEnvelope.VM.Age = 88;

            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            var exception = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, newChange));

            // Assert"
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ConcurrencyException>();
        }

        [Fact]
        public async Task UpsertDocument_UpsertTwoNewDocuments_OneFailsWithConcurrencyException()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            //The two persons are meant to be the same one updated from different sources, hence the same id.
            var personId = Guid.NewGuid();
            var personA = new PersonTest
            {
                PersonId = personId,
                Name = "Person A",
                Age = 1
            };
            var personB = new PersonTest
            {
                PersonId = personId,
                Name = "Person B",
                Age = 2
            };

            var personAEnvelope = new DocumentBase<PersonTest> { VM = personA };
            var personBEnvelope = new DocumentBase<PersonTest> { VM = personB };
            var taskForUpsertA = Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, personA.PersonId.ToString(), personAEnvelope));
            var taskForUpsertB = Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, personB.PersonId.ToString(), personBEnvelope));

            await Task.WhenAll(taskForUpsertA, taskForUpsertB);

            //The .Result below is fine because we have already awaited normally on line above
            var exceptions = new[] { taskForUpsertA.Result, taskForUpsertB.Result };

            // Assert"
            exceptions.Count(x => x != null).Should().Be(1);
            exceptions.Single(x => x != null).Should().BeOfType<ConcurrencyException>();
        }

        [Fact]
        public async Task UpsertDocument_UpsertTwoNewDocuments_OneSucceeds()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            //The two persons are meant to be the same one updated from different sources, hence the same id.
            var personId = Guid.NewGuid();
            var personA = new PersonTest
            {
                PersonId = personId,
                Name = "Person A",
                Age = 1
            };
            var personB = new PersonTest
            {
                PersonId = personId,
                Name = "Person B",
                Age = 2
            };

            var personAEnvelope = new DocumentBase<PersonTest> { VM = personA };
            var personBEnvelope = new DocumentBase<PersonTest> { VM = personB };

            // Act
            var taskForUpsertA = Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, personA.PersonId.ToString(), personAEnvelope));
            var taskForUpsertB = Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, personB.PersonId.ToString(), personBEnvelope));

            // Assert"
            await Task.WhenAll(taskForUpsertA, taskForUpsertB);

            var items = await db.Query<PersonTest>(CollectionId, new Dictionary<string, object>
            {
                {"PersonId", personId}
            });

            items.Count().Should().Be(1);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void IsDocumentInCollection_CollectionIdIsNull_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();


            // Act
            var exception = Record.Exception(() => db.DocumentExists(collectionId, documentId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void IsDocumentInCollection_DocumentIdIsNull_ThrowsException(string documentId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var exception = Record.Exception(() => db.DocumentExists(CollectionId, documentId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void IsDocumentInCollection_CollectionDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var exists = db.DocumentExists("Wibble-Database", documentId);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public void IsDocumentInCollection_DocumentDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var exists = db.DocumentExists(CollectionId, documentId);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task IsDocumentInCollection_DocumentDoesExist_ReturnsTrue()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person };
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            // Act
            var exists = db.DocumentExists(CollectionId, documentId);

            // Assert
            exists.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task ReadDocument_CollectionIdIsNull_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var exception = await Record.ExceptionAsync(() => db.ReadDocument<PersonTest>(collectionId, documentId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task ReadDocument_DocumentIdIsNull_ThrowsException(string documentId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var exception = await Record.ExceptionAsync(() => db.ReadDocument<PersonTest>(CollectionId, documentId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task ReadDocument_DocumentExists_ReturnDocumentIsCorrect()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person };
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            // Act
            var doc = await db.ReadDocument<PersonTest>(CollectionId, documentId);

            // Assert
            doc.Should().NotBeNull();
            doc.VM.Age.Should().Be(person.Age);
            doc.VM.Name.Should().Be(person.Name);
            doc.VM.PersonId.Should().Be(person.PersonId);
        }

        [Fact]
        public async Task ReadDocument_DocumentNotExists_ReturnsNull()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var doc = await db.ReadDocument<PersonTest>(CollectionId, documentId);

            // Assert
            doc.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task ReadAll_CollectionIdIsNull_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);


            // Act
            var exception = await Record.ExceptionAsync(() => db.ReadAll<PersonTest>(collectionId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task ReadAll_CollectionIdIsUnknown_ReturnsEmptyCollection()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var doc = await db.ReadAll<PersonTest>("wibble");

            // Assert
            doc.Should().BeEmpty();
        }

        [Fact]
        public async Task ReadAll_CollectionIdKnownButEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var doc = await db.ReadAll<DocumentBase<PersonTest>>(CollectionId);

            // Assert
            doc.Should().BeEmpty();
        }

        [Fact]
        public async Task ReadAll_DocumentsInCollectionExist_ReturnsValidCollection()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            // Act
            var doc = await db.ReadAll<PersonTest>(CollectionId);

            // Assert
            doc.Count().Should().Be(4);
        }

        [Fact]
        public async Task Query_ApplyNameAndAgeAndHomeIDFilter_ReturnExpectedItems()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            var filterProperties = new Dictionary<string, object>()
            {
                { "Name", "Wilma Flintstone" },
                { "Age", 66 },
                { "HomeID", new Guid("F79E323E-7BCD-4865-8BE7-5F674BE650CE") },
            };

            // Act
            var docs = await db.Query<PersonTest>(CollectionId, filterProperties);

            // Assert
            docs.Should().HaveCount(2);
        }

        [Fact]
        public async Task Query_ApplyInvalidFilter_ReturnEmptyCollection()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            var filterProperties = new Dictionary<string, object>()
            {
                { "Wibble", "Wilma Flintstone" }
            };

            // Act
            var docs = await db.Query<DocumentBase<PersonTest>>(CollectionId, filterProperties);

            // Assert
            docs.Should().BeEmpty();
        }

        [Fact]
        public async Task Query_ApplyNonMatchingFilter_ReturnEmptyCollection()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            var filterProperties = new Dictionary<string, object>()
            {
                { "Name", "Bamm-Bamm Rubble" }
            };

            // Act
            var docs = await db.Query<PersonTest>(CollectionId, filterProperties);

            // Assert
            docs.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task DeleteDocument_CollectionIdIsNull_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var exception = await Record.ExceptionAsync(() => db.DeleteDocument(collectionId, documentId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task DeleteDocument_DocumentIdIsNull_ThrowsException(string documentId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var exception = await Record.ExceptionAsync(() => db.DeleteDocument(CollectionId, documentId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task DeleteDocument_DocumentExists_DocumentIsDeleted()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest>() { VM = person };
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            // Act
            await db.DeleteDocument(CollectionId, documentId);
            var exists = db.DocumentExists(CollectionId, documentId);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteDocument_DocumentNotExists_NoExceptionThrown()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var exception = await Record.ExceptionAsync(() => db.DeleteDocument(CollectionId, documentId));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task CollectionExists_WithInvalidId_ThrowsException()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var exception = await Record.ExceptionAsync(() => db.CollectionExists(null));

            //Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        public async Task CollectionExists_CollectionExists_ReturnsTrue()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            // Collection with CollectionId gets created durint ctor setup
            var exists = await db.CollectionExists(CollectionId);

            //Assert
            exists.Should().BeTrue();
        }

        public async Task CollectionExists_CollectionDoesNotExists_ReturnsFalse()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var collectionId = Guid.NewGuid().ToString();

            // Act
            var exists = await db.CollectionExists(collectionId);

            //Assert
            exists.Should().BeFalse();
        }

        public async Task CollectionExists_CollectionDoesNotExists_DoesNotThrow()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);
            var collectionId = Guid.NewGuid().ToString();

            var exception = await Record.ExceptionAsync(() => db.CollectionExists(collectionId));

            //Assert
            exception.Should().BeNull();
        }


        [Fact]
        public async Task ClearCollection_CollectionWithItemsWhenCleared_ReturnEmptyCollection()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            // Act
            await db.ClearCollection(CollectionId);

            // Assert
            var docs = await db.ReadAll<PersonTest>(CollectionId);
            docs.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task CreateCollection_NullCollectionId_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var exception = await Record.ExceptionAsync(() => db.CreateCollection(collectionId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateCollection_NewCollectionNameProvided_NewCollectionCreated()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            await db.CreateCollection(NewCollectionId);

            // Assert
            var exists = await db.CollectionExists(NewCollectionId);
            exists.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task DeleteCollection_NullCollectionId_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            var exception = await Record.ExceptionAsync(() => db.DeleteCollection(collectionId));

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task DeleteCollection_WithExistingCollection_CollectionShouldNotExistAfterDeletion()
        {
            // Arrange
            var db = new MongoDb(ConnectionString, DatabaseId);

            // Act
            await db.DeleteCollection(CollectionId);

            // Assert
            var exists = await db.CollectionExists(CollectionId);
            exists.Should().BeFalse();
        }
    }
}