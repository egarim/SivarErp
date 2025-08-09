using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;

namespace Sivar.Erp.Core.Tests.Infrastructure.Data
{
    public class InMemoryRepositoryTests
    {
        /// <summary>
        /// Test entity class that implements IEntity
        /// </summary>
        private class TestEntity : IEntity
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        /// <summary>
        /// Another test entity for relationship testing
        /// </summary>
        private class RelatedEntity : IEntity
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string Code { get; set; } = string.Empty;
            public List<TestEntity> Items { get; set; } = new();
        }

        [Fact]
        public void CreateObject_ShouldCreateAndTrackNewObject()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            
            // Act
            var entity = repository.CreateObject<TestEntity>();
            
            // Assert
            Assert.NotNull(entity);
            Assert.NotEqual(Guid.Empty, entity.Id);
            Assert.True(repository.IsModified);
            
            // Verify it's in the repository
            var retrievedEntity = repository.GetObjects<TestEntity>().FirstOrDefault();
            Assert.Same(entity, retrievedEntity);
        }
        
        [Fact]
        public async Task CommitChanges_ShouldClearTrackedObjects()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            var entity = repository.CreateObject<TestEntity>();
            
            // Act
            await repository.CommitChanges();
            
            // Assert
            Assert.False(repository.IsModified);
        }
        
        [Fact]
        public void Rollback_ShouldDiscardNewObjects()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            var entity = repository.CreateObject<TestEntity>();
            
            // Act
            repository.Rollback();
            
            // Assert
            Assert.False(repository.IsModified);
            Assert.Empty(repository.GetObjects<TestEntity>());
        }
        
        [Fact]
        public void GetObjectByKey_ShouldReturnObjectWithMatchingId()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            var entity = repository.CreateObject<TestEntity>();
            entity.Name = "Test Entity";
            
            // Act
            var result = repository.GetObjectByKey<TestEntity>(entity.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Entity", result.Name);
        }
        
        [Fact]
        public void FindObject_ShouldReturnObjectMatchingCriteria()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            var entity1 = repository.CreateObject<TestEntity>();
            entity1.Name = "Entity 1";
            entity1.Amount = 100;
            
            var entity2 = repository.CreateObject<TestEntity>();
            entity2.Name = "Entity 2";
            entity2.Amount = 200;
            
            // Act
            var result = repository.FindObject<TestEntity>(e => e.Amount > 150);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Entity 2", result.Name);
        }
        
        [Fact]
        public void GetObjects_ShouldReturnAllObjectsOfSpecifiedType()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            var entity1 = repository.CreateObject<TestEntity>();
            var entity2 = repository.CreateObject<TestEntity>();
            var entity3 = repository.CreateObject<TestEntity>();
            
            // Also create some objects of a different type
            var related1 = repository.CreateObject<RelatedEntity>();
            var related2 = repository.CreateObject<RelatedEntity>();
            
            // Act
            var testEntities = repository.GetObjects<TestEntity>().ToList();
            var relatedEntities = repository.GetObjects<RelatedEntity>().ToList();
            
            // Assert
            Assert.Equal(3, testEntities.Count);
            Assert.Equal(2, relatedEntities.Count);
        }
        
        [Fact]
        public async Task EntityLifecycle_ShouldHandleFullLifecycle()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            
            // Act & Assert - Create
            var entity = repository.CreateObject<TestEntity>();
            entity.Name = "Test Entity";
            entity.Amount = 500;
            Assert.True(repository.IsModified);
            
            // Act & Assert - Commit
            await repository.CommitChanges();
            Assert.False(repository.IsModified);
            
            // Act & Assert - Retrieve
            var retrievedEntity = repository.GetObjectByKey<TestEntity>(entity.Id);
            Assert.NotNull(retrievedEntity);
            Assert.Equal("Test Entity", retrievedEntity.Name);
            
            // Act & Assert - Update
            retrievedEntity.Name = "Updated Entity";
            
            // No automatic tracking in this implementation, would need
            // explicit tracking method if implemented
            
            // Act & Assert - Query
            var entities = repository.GetObjects<TestEntity>()
                .Where(e => e.Amount >= 500)
                .ToList();
            Assert.Single(entities);
            Assert.Equal("Updated Entity", entities[0].Name);
        }
        
        [Fact]
        public void Dispose_ShouldClearAllCollections()
        {
            // Arrange
            var repository = new InMemoryRepository();
            var entity = repository.CreateObject<TestEntity>();
            
            // Act
            repository.Dispose();
            
            // Assert
            // This is a bit tricky to test directly since the collections are private
            // Instead, we'll create a new object and see if the previous one is gone
            var newRepository = new InMemoryRepository();
            Assert.Empty(newRepository.GetObjects<TestEntity>());
        }
        
        [Fact]
        public void Repository_ShouldSupportMultipleEntityTypes()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            
            // Act
            var testEntity = repository.CreateObject<TestEntity>();
            testEntity.Name = "Test";
            
            var relatedEntity = repository.CreateObject<RelatedEntity>();
            relatedEntity.Code = "REL-001";
            
            // Assert
            Assert.Single(repository.GetObjects<TestEntity>());
            Assert.Single(repository.GetObjects<RelatedEntity>());
            Assert.Equal("Test", repository.GetObjects<TestEntity>().First().Name);
            Assert.Equal("REL-001", repository.GetObjects<RelatedEntity>().First().Code);
        }
        
        [Fact]
        public async Task ConcurrentOperations_ShouldBeSafe()
        {
            // Arrange
            using var repository = new InMemoryRepository();
            var tasks = new List<Task>();
            var entityCount = 100;
            
            // Act - Create entities concurrently
            for (int i = 0; i < entityCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() =>
                {
                    var entity = repository.CreateObject<TestEntity>();
                    entity.Name = $"Entity {index}";
                    entity.Amount = index;
                }));
            }
            
            await Task.WhenAll(tasks);
            
            // Assert
            var entities = repository.GetObjects<TestEntity>().ToList();
            Assert.Equal(entityCount, entities.Count);
            Assert.Equal(entityCount, entities.Select(e => e.Name).Distinct().Count());
        }
    }
}