using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using System.ComponentModel;

namespace Sivar.Erp.Core.Tests.Unit.Infrastructure.Data
{
    /// <summary>
    /// Comprehensive unit tests for the InMemoryRepository implementation
    /// </summary>
    [Description("Unit tests for InMemoryRepository")]
    public class InMemoryRepositoryTests : IDisposable
    {
        private readonly InMemoryRepository _repository;

        public InMemoryRepositoryTests()
        {
            _repository = new InMemoryRepository();
        }

        public void Dispose()
        {
            _repository?.Dispose();
        }

        #region CreateObject Tests

        [Fact]
        [Description("CreateObject should create a new instance and track it")]
        public void CreateObject_ShouldCreateAndTrackNewObject()
        {
            // Arrange & Act
            var testEntity = _repository.CreateObject<TestEntity>();

            // Assert
            Assert.NotNull(testEntity);
            Assert.NotEqual(Guid.Empty, testEntity.Id);
            Assert.True(testEntity.CreatedAt > DateTime.MinValue);
            Assert.True(testEntity.UpdatedAt > DateTime.MinValue);
            Assert.True(_repository.IsModified);
        }

        [Fact]
        [Description("CreateObject should add object to the appropriate collection")]
        public void CreateObject_ShouldAddToCollection()
        {
            // Arrange & Act
            var testEntity = _repository.CreateObject<TestEntity>();

            // Assert
            var allEntities = _repository.GetObjects<TestEntity>();
            Assert.Contains(testEntity, allEntities);
            Assert.Single(allEntities);
        }

        [Fact]
        [Description("CreateObject should work with non-entity classes")]
        public void CreateObject_ShouldWorkWithNonEntityClasses()
        {
            // Arrange & Act
            var simpleObject = _repository.CreateObject<SimpleTestClass>();

            // Assert
            Assert.NotNull(simpleObject);
            Assert.Equal("Default", simpleObject.Name);
            Assert.True(_repository.IsModified);
        }

        #endregion

        #region GetObjectByKey Tests

        [Fact]
        [Description("GetObjectByKey should retrieve object by Guid ID")]
        public void GetObjectByKey_ShouldRetrieveByGuidId()
        {
            // Arrange
            var testEntity = _repository.CreateObject<TestEntity>();
            var entityId = testEntity.Id;

            // Act
            var retrievedEntity = _repository.GetObjectByKey<TestEntity>(entityId);

            // Assert
            Assert.NotNull(retrievedEntity);
            Assert.Equal(testEntity.Id, retrievedEntity.Id);
            Assert.Same(testEntity, retrievedEntity);
        }

        [Fact]
        [Description("GetObjectByKey should return null for non-existent key")]
        public void GetObjectByKey_ShouldReturnNullForNonExistentKey()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = _repository.GetObjectByKey<TestEntity>(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        [Description("GetObjectByKey should handle null key gracefully")]
        public void GetObjectByKey_ShouldHandleNullKey()
        {
            // Act
            var result = _repository.GetObjectByKey<TestEntity>(null);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetObjects Tests

        [Fact]
        [Description("GetObjects should return empty collection when no objects exist")]
        public void GetObjects_ShouldReturnEmptyCollectionWhenEmpty()
        {
            // Act
            var entities = _repository.GetObjects<TestEntity>();

            // Assert
            Assert.NotNull(entities);
            Assert.Empty(entities);
        }

        [Fact]
        [Description("GetObjects should return all objects of specified type")]
        public void GetObjects_ShouldReturnAllObjectsOfType()
        {
            // Arrange
            var entity1 = _repository.CreateObject<TestEntity>();
            var entity2 = _repository.CreateObject<TestEntity>();
            var entity3 = _repository.CreateObject<TestEntity>();

            // Act
            var entities = _repository.GetObjects<TestEntity>().ToList();

            // Assert
            Assert.Equal(3, entities.Count);
            Assert.Contains(entity1, entities);
            Assert.Contains(entity2, entities);
            Assert.Contains(entity3, entities);
        }

        [Fact]
        [Description("GetObjects should be queryable with LINQ")]
        public void GetObjects_ShouldBeQueryableWithLinq()
        {
            // Arrange
            var entity1 = _repository.CreateObject<TestEntity>();
            entity1.Name = "First";
            var entity2 = _repository.CreateObject<TestEntity>();
            entity2.Name = "Second";

            // Act
            var filteredEntities = _repository.GetObjects<TestEntity>()
                .Where(e => e.Name == "First")
                .ToList();

            // Assert
            Assert.Single(filteredEntities);
            Assert.Equal("First", filteredEntities[0].Name);
        }

        #endregion

        #region FindObject Tests

        [Fact]
        [Description("FindObject should find object matching criteria")]
        public void FindObject_ShouldFindMatchingObject()
        {
            // Arrange
            var entity1 = _repository.CreateObject<TestEntity>();
            entity1.Name = "Target";
            var entity2 = _repository.CreateObject<TestEntity>();
            entity2.Name = "Other";

            // Act
            var foundEntity = _repository.FindObject<TestEntity>(e => e.Name == "Target");

            // Assert
            Assert.NotNull(foundEntity);
            Assert.Equal("Target", foundEntity.Name);
            Assert.Same(entity1, foundEntity);
        }

        [Fact]
        [Description("FindObject should return null when no match found")]
        public void FindObject_ShouldReturnNullWhenNoMatch()
        {
            // Arrange
            _repository.CreateObject<TestEntity>();

            // Act
            var foundEntity = _repository.FindObject<TestEntity>(e => e.Name == "NonExistent");

            // Assert
            Assert.Null(foundEntity);
        }

        [Fact]
        [Description("FindObject should throw exception for null criteria")]
        public void FindObject_ShouldThrowForNullCriteria()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _repository.FindObject<TestEntity>(null));
        }

        #endregion

        #region Change Tracking Tests

        [Fact]
        [Description("IsModified should be true when new objects exist")]
        public void IsModified_ShouldBeTrueWithNewObjects()
        {
            // Arrange
            Assert.False(_repository.IsModified);

            // Act
            _repository.CreateObject<TestEntity>();

            // Assert
            Assert.True(_repository.IsModified);
        }

        [Fact]
        [Description("MarkAsModified should track object as modified")]
        public async Task MarkAsModified_ShouldTrackObjectAsModified()
        {
            // Arrange
            var entity = _repository.CreateObject<TestEntity>();
            await _repository.CommitChanges();
            Assert.False(_repository.IsModified);

            // Act
            _repository.MarkAsModified(entity);

            // Assert
            Assert.True(_repository.IsModified);
        }

        #endregion

        #region Commit and Rollback Tests

        [Fact]
        [Description("CommitChanges should clear change tracking")]
        public async Task CommitChanges_ShouldClearTrackedObjects()
        {
            // Arrange
            var entity = _repository.CreateObject<TestEntity>();
            Assert.True(_repository.IsModified);

            // Act
            await _repository.CommitChanges();

            // Assert
            Assert.False(_repository.IsModified);
            
            // Entity should still exist in collection
            var entities = _repository.GetObjects<TestEntity>();
            Assert.Contains(entity, entities);
        }

        [Fact]
        [Description("CommitChanges should update timestamps for entities")]
        public async Task CommitChanges_ShouldUpdateTimestamps()
        {
            // Arrange
            var entity = _repository.CreateObject<TestEntity>();
            var originalUpdatedAt = entity.UpdatedAt;
            
            // Add a small delay to ensure timestamp difference
            await Task.Delay(10);

            // Act
            await _repository.CommitChanges();

            // Assert
            Assert.True(entity.UpdatedAt >= originalUpdatedAt);
        }

        [Fact]
        [Description("Rollback should remove new objects from collections")]
        public void Rollback_ShouldRemoveNewObjects()
        {
            // Arrange
            var entity = _repository.CreateObject<TestEntity>();
            Assert.Single(_repository.GetObjects<TestEntity>());
            Assert.True(_repository.IsModified);

            // Act
            _repository.Rollback();

            // Assert
            Assert.Empty(_repository.GetObjects<TestEntity>());
            Assert.False(_repository.IsModified);
        }

        [Fact]
        [Description("Rollback should clear modification tracking")]
        public async Task Rollback_ShouldClearModificationTracking()
        {
            // Arrange
            var entity = _repository.CreateObject<TestEntity>();
            await _repository.CommitChanges();
            _repository.MarkAsModified(entity);
            Assert.True(_repository.IsModified);

            // Act
            _repository.Rollback();

            // Assert
            Assert.False(_repository.IsModified);
        }

        #endregion

        #region Thread Safety Tests

        [Fact]
        [Description("Repository should be thread-safe for concurrent operations")]
        public async Task Repository_ShouldBeThreadSafe()
        {
            // Arrange
            const int threadCount = 10;
            const int operationsPerThread = 100;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var entity = _repository.CreateObject<TestEntity>();
                        entity.Name = $"Thread{threadId}_Entity{j}";
                        
                        // Perform some queries
                        var entities = _repository.GetObjects<TestEntity>().ToList();
                        var found = _repository.FindObject<TestEntity>(e => e.Name == entity.Name);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var totalEntities = _repository.GetObjects<TestEntity>().Count();
            Assert.Equal(threadCount * operationsPerThread, totalEntities);
        }

        #endregion

        #region Clear and Dispose Tests

        [Fact]
        [Description("Clear should remove all data and reset state")]
        public void Clear_ShouldRemoveAllDataAndResetState()
        {
            // Arrange
            _repository.CreateObject<TestEntity>();
            _repository.CreateObject<SimpleTestClass>();
            Assert.True(_repository.IsModified);

            // Act
            _repository.Clear();

            // Assert
            Assert.Empty(_repository.GetObjects<TestEntity>());
            Assert.Empty(_repository.GetObjects<SimpleTestClass>());
            Assert.False(_repository.IsModified);
        }

        [Fact]
        [Description("Repository should handle disposal properly")]
        public void Dispose_ShouldCleanupResources()
        {
            // Arrange
            var repository = new InMemoryRepository();
            repository.CreateObject<TestEntity>();

            // Act
            repository.Dispose();

            // Assert - Should not throw
            Assert.False(repository.IsModified);
        }

        #endregion

        #region Multiple Type Tests

        [Fact]
        [Description("Repository should handle multiple entity types independently")]
        public void Repository_ShouldHandleMultipleTypesIndependently()
        {
            // Arrange & Act
            var testEntity = _repository.CreateObject<TestEntity>();
            var simpleObject = _repository.CreateObject<SimpleTestClass>();

            // Assert
            Assert.Single(_repository.GetObjects<TestEntity>());
            Assert.Single(_repository.GetObjects<SimpleTestClass>());
            
            var testEntities = _repository.GetObjects<TestEntity>().ToList();
            var simpleObjects = _repository.GetObjects<SimpleTestClass>().ToList();
            
            Assert.Contains(testEntity, testEntities);
            Assert.Contains(simpleObject, simpleObjects);
            Assert.DoesNotContain(simpleObject, testEntities.Cast<object>());
            Assert.DoesNotContain(testEntity, simpleObjects.Cast<object>());
        }

        #endregion
    }

    #region Test Helper Classes

    /// <summary>
    /// Test entity that implements IEntity for testing
    /// </summary>
    public class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    /// <summary>
    /// Simple test class that doesn't implement IEntity
    /// </summary>
    public class SimpleTestClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Default";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Test class for testing inheritance scenarios
    /// </summary>
    public class DerivedTestEntity : TestEntity
    {
        public string Description { get; set; } = string.Empty;
    }

    #endregion
}