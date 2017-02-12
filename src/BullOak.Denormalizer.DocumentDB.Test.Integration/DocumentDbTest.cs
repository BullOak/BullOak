namespace BullOak.Denormalizer.DocumentDb.Test.Integration
{
    using System;
    using Xunit;
    using System.Threading.Tasks;
    using System.Linq;
    using FluentAssertions;
    using System.Collections.Generic;
    using BullOak.EventStream;
    using System.Configuration;

    public class DocumentDbTest
    {
        public class PersonTest
        {
            public Guid Id { get; set; }
            public Guid PersonId { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private readonly string location;
        private readonly string key;
        private readonly string database;
        private const string CollectionId = "Persons";
        private const string NewCollectionId = "My.Nested.Namespace.Class";

        public DocumentDbTest()
        {
            location = ConfigurationManager.AppSettings["DocumentDbUrl"];
            key = ConfigurationManager.AppSettings["DocumentDbAuthKey"];
            database = ConfigurationManager.AppSettings["DocumentDbDatabaseId"];

            var db = new DocumentDb(location, key, database);

            db.ClearCollection(NewCollectionId).Wait();
            db.ClearCollection(CollectionId).Wait();
            db.CreateCollection(CollectionId).Wait();
        }

        private async Task<DocumentDb> SetupMultiplePersons()
        {
            var db = new DocumentDb(location, key, database);

            var personA = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87,
                Id = new Guid("C9FCD0EB-E5F3-439D-BA4D-093E692046C0")
            };
            var personAEnvelope = new DocumentBase<PersonTest> { VM = personA, ETag = Guid.NewGuid().ToString() };
            var personB = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Wilma Flintstone",
                Age = 88,
                Id = Guid.NewGuid()
            };
            var personBEnvelope = new DocumentBase<PersonTest> { VM = personB, ETag = Guid.NewGuid().ToString() };
            var personC = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Wilma Flintstone",
                Age = 66,
                Id = Guid.Empty
            };
            var personCEnvelope = new DocumentBase<PersonTest> { VM = personC, ETag = Guid.NewGuid().ToString() };
            var personD = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Wilma Flintstone",
                Age = 66,
                Id = Guid.NewGuid()
            };
            var personDEnvelope = new DocumentBase<PersonTest> { VM = personD, ETag = Guid.NewGuid().ToString() };

            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personAEnvelope);
            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personBEnvelope);
            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personCEnvelope);
            await db.UpsertDocument(CollectionId, Guid.NewGuid().ToString(), personDEnvelope);

            return db;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void DocumentDb_ConstructorParamLocationIsNull_ExpectException(string parameter)
        {
            // Arrange
            // Act
            var ex = Record.Exception(() => new DocumentDb(parameter, key, database));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
            ex.Message.Should().Be("Value cannot be null.\r\nParameter name: location");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void DocumentDb_ConstructorParamKeyIsNull_ExpectException(string parameter)
        {
            // Arrange
            // Act
            var ex = Record.Exception(() => new DocumentDb(location, parameter, database));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
            ex.Message.Should().Be("Value cannot be null.\r\nParameter name: key");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void DocumentDb_ConstructorParamDatabaseIsNull_ExpectException(string parameter)
        {
            // Arrange
            // Act
            var ex = Record.Exception(() => new DocumentDb(location, key, parameter));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
            ex.Message.Should().Be("Value cannot be null.\r\nParameter name: dbId");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task UpsertDocument_NullCollectionId_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person, ETag = Guid.NewGuid().ToString() };

            // Act
            var ex = await Record.ExceptionAsync(() => db.UpsertDocument(collectionId, documentId, personEnvelope));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task UpsertDocument_NullDocumentId_ThrowsException(string documentId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person, ETag = Guid.NewGuid().ToString() };

            // Act
            var ex = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, personEnvelope));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task UpsertDocument_NullItem_ThrowsException()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();
            DocumentBase<PersonTest> personEnvelope = null;

            // Act
            var ex = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, personEnvelope));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task UpsertDocument_NullVM_ThrowsException()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();
            DocumentBase<PersonTest> personEnvelope = new DocumentBase<PersonTest> { VM = null };

            // Act
            var ex = await Record.ExceptionAsync(() => db.UpsertDocument(CollectionId, documentId, personEnvelope));

            // Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
         }


        [Fact]
        public async Task UpsertDocument_TwoNewDocuments_OneIsStoredCorrectly()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var id = Guid.NewGuid();
            var personA = new PersonTest()
            {
                Id = id,
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personB = new PersonTest()
            {
                Id = id,
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personAEnvelope = new DocumentBase<PersonTest> { VM = personA, ETag = id.ToString() };
            var personBEnvelope = new DocumentBase<PersonTest> { VM = personB, ETag = id.ToString() };

            // Act
            var taskForPersonA = db.UpsertDocument(CollectionId, id.ToString(), personAEnvelope);
            var taskForPersonB = db.UpsertDocument(CollectionId, id.ToString(), personBEnvelope);

            // Assert
            //We don't care about exception now we just waiting for tasks to finish
            await Record.ExceptionAsync(() => Task.WhenAll(taskForPersonA, taskForPersonB));

            var persons = await db.Query<PersonTest>(CollectionId, new Dictionary<string, object>()
                {{"Id", personA.Id}});

            persons.Count().Should().Be(1);
        }

        [Fact]
        public async Task UpsertDocument_TwoNewDocumentsSameIdDifferentEtag_OnlyOneIsStored()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var id = Guid.NewGuid();
            var personA = new PersonTest()
            {
                Id = id,
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personB = new PersonTest()
            {
                Id = id,
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personAEnvelope = new DocumentBase<PersonTest> { VM = personA };
            var personBEnvelope = new DocumentBase<PersonTest> { VM = personB };

            // Act
            var taskForPersonA = db.UpsertDocument(CollectionId, id.ToString(), personAEnvelope);
            var taskForPersonB = db.UpsertDocument(CollectionId, id.ToString(), personBEnvelope);

            // Assert
            //We don't care about exception now we just waiting for tasks to finish
            await Record.ExceptionAsync(() => Task.WhenAll(taskForPersonA, taskForPersonB));

            var persons = await db.Query<PersonTest>(CollectionId, new Dictionary<string, object>()
                {{"Id", personA.Id}});

            persons.Count().Should().Be(1);
        }

        [Fact]
        public async Task UpsertDocument_TwoNewDocuments_OneThrowsConcurrencyException()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var id = Guid.NewGuid();
            var personA = new PersonTest()
            {
                Id = id,
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personB = new PersonTest()
            {
                Id = id,
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personAEnvelope = new DocumentBase<PersonTest> { VM = personA, ETag = id.ToString() };
            var personBEnvelope = new DocumentBase<PersonTest> { VM = personB, ETag = id.ToString() };

            // Act
            var taskForPersonA = db.UpsertDocument(CollectionId, id.ToString(), personAEnvelope);
            var taskForPersonB = db.UpsertDocument(CollectionId, id.ToString(), personBEnvelope);

            // Assert
            var exception = await Record.ExceptionAsync(() => Task.WhenAll(taskForPersonA, taskForPersonB));

            exception.Should().NotBeNull();
            exception.Should().BeOfType<ConcurrencyException>();
        }

        [Fact]
        public async Task UpsertDocument_NewDocument_IsStoredCorrectly()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var id = Guid.NewGuid();
            var person = new PersonTest()
            {
                PersonId = id,
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person, ETag = Guid.NewGuid().ToString() };

            // Act
            await db.UpsertDocument(CollectionId, id.ToString(), personEnvelope);

            // Assert
            var doc = await db.ReadDocument<PersonTest>(CollectionId, id.ToString());
            doc.VM.Age.Should().Be(87);
            doc.VM.Name.Should().Be("Barney Rubble");
            doc.VM.PersonId.Should().Be(person.PersonId);
        }

        [Fact]
        public async Task UpsertDocument_UpdateDocument_IsUpdatedCorrectly()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                Id = Guid.Parse(documentId),
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87
            };
            var personEnvelope = new DocumentBase<PersonTest> { VM = person };
            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            personEnvelope = await db.ReadDocument<PersonTest>(CollectionId, documentId);
            personEnvelope.VM.Name = "Fred Flintstone";
            personEnvelope.VM.Age = 88;

            await db.UpsertDocument(CollectionId, documentId, personEnvelope);

            // Assert
            var doc = await db.ReadDocument<PersonTest>(CollectionId, documentId);
            doc.VM.Age.Should().Be(88);
            doc.VM.Name.Should().Be("Fred Flintstone");
            doc.VM.PersonId.Should().Be(person.PersonId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void IsDocumentInCollection_CollectionIdIsNull_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var ex = Record.Exception(() => db.DocumentExists(collectionId, documentId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task UpsertDocument_UpdateDocumentConncurrently_ThrowsException()

        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();
            var person = new PersonTest
            {
                PersonId = Guid.NewGuid(),
                Name = "Barney Rubble",
                Age = 87,
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

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<ConcurrencyException>();
            exception.Message.Should().Contain("One of the specified pre-condition is not met");
        }


        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public void IsDocumentInCollection_DocumentIdIsNull_ThrowsException(string documentId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);

            // Act
            var ex = Record.Exception(() => db.DocumentExists(CollectionId, documentId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void IsDocumentInCollection_CollectionDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
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
            var db = new DocumentDb(location, key, database);
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
            var db = new DocumentDb(location, key, database);
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
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var ex = await Record.ExceptionAsync(() => db.ReadDocument<PersonTest>(collectionId, documentId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task ReadDocument_DocumentIdIsNull_ThrowsException(string documentId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);

            // Act
            var ex = await Record.ExceptionAsync(() => db.ReadDocument<PersonTest>(CollectionId, documentId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task ReadDocument_DocumentExists_ReturnDocumentIsCorrect()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
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
            doc.VM.Age.Should().Be(person.Age);
            doc.VM.Name.Should().Be(person.Name);
            doc.VM.PersonId.Should().Be(person.PersonId);
            doc.ETag.Should().NotBe(string.Empty);
        }

        [Fact]
        public async Task ReadDocument_DocumentNotExists_ReturnsNull()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
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
            var db = new DocumentDb(location, key, database);

            // Act
            var ex = await Record.ExceptionAsync(() => db.ReadAll<PersonTest>(collectionId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task ReadAll_CollectionIdIsUnknown_ReturnsEmptyCollection()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);

            // Act
            var doc = await db.ReadAll<PersonTest>("wibble");

            // Assert
            doc.Should().BeEmpty();
        }

        [Fact]
        public async Task ReadAll_CollectionIdKnownButEmpty_ReturnsEmptyCollection()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);

            // Act
            var doc = await db.ReadAll<PersonTest>(CollectionId);

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
        public async Task Query_ApplyNameAndAgeFilter_ReturnExpectedItems()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            var filterProperties = new Dictionary<string, object>()
            {
                { "Name", "Wilma Flintstone" },
                { "Age", 66 }
            };
                
            // Act
            var docs = await db.Query<PersonTest>(CollectionId, filterProperties);

            // Assert
            docs.Should().HaveCount(2);
        }
        [Fact]
        public async Task Query_ApplyGuidFilter_ReturnExpectedItem()
        {
            // Arrange
            var db = await SetupMultiplePersons();

            var filterProperties = new Dictionary<string, object>()
            {
                { "Id", new Guid("C9FCD0EB-E5F3-439D-BA4D-093E692046C0") }
            };

            // Act
            var docs = await db.Query<PersonTest>(CollectionId, filterProperties);

            // Assert
            var items = docs.ToArray();
            items.Should().HaveCount(1);
            items.First().Name.Should().Be("Barney Rubble");
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
            var docs = await db.Query<DocumentBase<PersonTest>>(CollectionId, filterProperties);

            // Assert
            docs.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task DeleteDocument_CollectionIdIsNull_ThrowsException(string collectionId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var ex = await Record.ExceptionAsync(() => db.DeleteDocument(collectionId, documentId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task DeleteDocument_DocumentIdIsNull_ThrowsException(string documentId)
        {
            // Arrange
            var db = new DocumentDb(location, key, database);

            // Act
            var ex = await Record.ExceptionAsync(() => db.DeleteDocument(CollectionId, documentId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task DeleteDocument_DocumentExists_DocumentIsDeleted()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
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
            await db.DeleteDocument(CollectionId, documentId);
            var exists = db.DocumentExists(CollectionId, documentId);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteDocument_DocumentNotExists_NoExceptionThrown()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            var documentId = Guid.NewGuid().ToString();

            // Act
            var ex = await Record.ExceptionAsync(() => db.DeleteDocument(CollectionId, documentId));

            // Assert
            ex.Should().BeNull();
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
            var db = new DocumentDb(location, key, database);

            // Act
            var ex = await Record.ExceptionAsync(() => db.CreateCollection(collectionId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateCollection_NewCollectionNameProvided_NewCollectionCreated()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);

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
            var db = new DocumentDb(location, key, database);

            // Act
            var ex = await Record.ExceptionAsync(() => db.DeleteCollection(collectionId));

            //Assert
            ex.Should().NotBeNull();
            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task DeleteCollection_NewCollectionNameProvided_NewCollectionCreated()
        {
            // Arrange
            var db = new DocumentDb(location, key, database);
            await db.CreateCollection(NewCollectionId);

            // Act
            await db.DeleteCollection(NewCollectionId);

            // Assert
            var exists = await db.CollectionExists(NewCollectionId);
            exists.Should().BeFalse();
        }
    }
}
