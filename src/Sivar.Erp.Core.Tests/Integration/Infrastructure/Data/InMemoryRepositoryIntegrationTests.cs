using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using System.ComponentModel;

namespace Sivar.Erp.Core.Tests.Integration.Infrastructure.Data
{
    /// <summary>
    /// Integration tests for InMemoryRepository focusing on real-world scenarios
    /// </summary>
    [Description("Integration tests for InMemoryRepository")]
    public class InMemoryRepositoryIntegrationTests : IDisposable
    {
        private readonly InMemoryRepository _repository;

        public InMemoryRepositoryIntegrationTests()
        {
            _repository = new InMemoryRepository();
        }

        public void Dispose()
        {
            _repository?.Dispose();
        }

        [Fact]
        [Description("Should handle complex entity relationships and queries")]
        public async Task ComplexScenario_ShouldHandleEntityRelationshipsAndQueries()
        {
            // Arrange - Create a mini ERP scenario
            var customer = _repository.CreateObject<Customer>();
            customer.Name = "Acme Corp";
            customer.Email = "contact@acme.com";

            var product1 = _repository.CreateObject<Product>();
            product1.Name = "Widget A";
            product1.Price = 10.00m;

            var product2 = _repository.CreateObject<Product>();
            product2.Name = "Widget B";
            product2.Price = 15.00m;

            var order = _repository.CreateObject<Order>();
            order.CustomerId = customer.Id;
            order.OrderDate = DateTime.Today;
            order.TotalAmount = 25.00m;

            await _repository.CommitChanges();

            // Act - Perform complex queries
            var orders = _repository.GetObjects<Order>()
                .Where(o => o.OrderDate >= DateTime.Today.AddDays(-30))
                .ToList();

            var expensiveProducts = _repository.GetObjects<Product>()
                .Where(p => p.Price > 12.00m)
                .OrderBy(p => p.Name)
                .ToList();

            var specificCustomer = _repository.FindObject<Customer>(c => c.Email.Contains("acme"));

            // Assert
            Assert.Single(orders);
            Assert.Equal(order.Id, orders[0].Id);

            Assert.Single(expensiveProducts);
            Assert.Equal("Widget B", expensiveProducts[0].Name);

            Assert.NotNull(specificCustomer);
            Assert.Equal("Acme Corp", specificCustomer.Name);
        }

        [Fact]
        [Description("Should handle batch operations efficiently")]
        public async Task BatchOperations_ShouldBeEfficient()
        {
            // Arrange
            const int batchSize = 1000;
            var startTime = DateTime.UtcNow;

            // Act - Create batch of entities
            var entities = new List<Product>();
            for (int i = 0; i < batchSize; i++)
            {
                var product = _repository.CreateObject<Product>();
                product.Name = $"Product {i}";
                product.Price = i * 1.5m;
                entities.Add(product);
            }

            await _repository.CommitChanges();
            var commitTime = DateTime.UtcNow;

            // Perform batch queries
            var allProducts = _repository.GetObjects<Product>().ToList();
            var expensiveProducts = _repository.GetObjects<Product>()
                .Where(p => p.Price > 500)
                .ToList();
            
            var endTime = DateTime.UtcNow;

            // Assert
            Assert.Equal(batchSize, allProducts.Count);
            Assert.True(expensiveProducts.Count > 0);
            
            // Performance assertions (should be fast for in-memory operations)
            var commitDuration = commitTime - startTime;
            var queryDuration = endTime - commitTime;
            
            Assert.True(commitDuration.TotalSeconds < 5, $"Commit took too long: {commitDuration.TotalSeconds} seconds");
            Assert.True(queryDuration.TotalSeconds < 1, $"Queries took too long: {queryDuration.TotalSeconds} seconds");
        }

        [Fact]
        [Description("Should maintain data integrity during concurrent modifications")]
        public async Task ConcurrentModifications_ShouldMaintainDataIntegrity()
        {
            // Arrange
            const int concurrentTasks = 20;
            const int entitiesPerTask = 50;

            // Act - Run concurrent operations
            var tasks = Enumerable.Range(0, concurrentTasks).Select(async taskId =>
            {
                var taskEntities = new List<Customer>();
                
                // Create entities
                for (int i = 0; i < entitiesPerTask; i++)
                {
                    var customer = _repository.CreateObject<Customer>();
                    customer.Name = $"Customer {taskId}-{i}";
                    customer.Email = $"customer{taskId}_{i}@test.com";
                    taskEntities.Add(customer);
                }

                // Simulate some business logic
                await Task.Delay(10);

                // Query and modify
                foreach (var customer in taskEntities)
                {
                    var found = _repository.GetObjectByKey<Customer>(customer.Id);
                    Assert.NotNull(found);
                    Assert.Equal(customer.Name, found.Name);
                    
                    // Mark as modified
                    _repository.MarkAsModified(customer);
                    customer.Name += " Modified";
                }

                return taskEntities;
            }).ToArray();

            var results = await Task.WhenAll(tasks);

            // Commit all changes
            await _repository.CommitChanges();

            // Assert
            var totalCustomers = _repository.GetObjects<Customer>().Count();
            var expectedTotal = concurrentTasks * entitiesPerTask;
            
            Assert.Equal(expectedTotal, totalCustomers);

            // Verify all entities have unique IDs
            var allIds = _repository.GetObjects<Customer>().Select(c => c.Id).ToList();
            var uniqueIds = allIds.Distinct().ToList();
            Assert.Equal(allIds.Count, uniqueIds.Count);

            // Verify modifications were applied
            var modifiedCustomers = _repository.GetObjects<Customer>()
                .Where(c => c.Name.Contains("Modified"))
                .Count();
            Assert.Equal(expectedTotal, modifiedCustomers);
        }

        [Fact]
        [Description("Should handle rollback scenarios correctly")]
        public void RollbackScenarios_ShouldMaintainConsistency()
        {
            // Arrange - Create some committed data
            var existingCustomer = _repository.CreateObject<Customer>();
            existingCustomer.Name = "Existing Customer";
            _repository.CommitChanges().Wait();

            var originalCount = _repository.GetObjects<Customer>().Count();

            // Act - Create new data and then rollback
            var newCustomer1 = _repository.CreateObject<Customer>();
            newCustomer1.Name = "New Customer 1";
            
            var newCustomer2 = _repository.CreateObject<Customer>();
            newCustomer2.Name = "New Customer 2";

            // Modify existing customer
            _repository.MarkAsModified(existingCustomer);
            existingCustomer.Name = "Modified Existing Customer";

            Assert.True(_repository.IsModified);
            Assert.Equal(originalCount + 2, _repository.GetObjects<Customer>().Count());

            // Rollback
            _repository.Rollback();

            // Assert
            Assert.False(_repository.IsModified);
            Assert.Equal(originalCount, _repository.GetObjects<Customer>().Count());
            
            // Original customer should still exist
            var customers = _repository.GetObjects<Customer>().ToList();
            Assert.Contains(existingCustomer, customers);
            
            // New customers should be gone
            Assert.DoesNotContain(newCustomer1, customers);
            Assert.DoesNotContain(newCustomer2, customers);
        }

        [Fact]
        [Description("Should work with inheritance hierarchies")]
        public void InheritanceHierarchies_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var baseEntity = _repository.CreateObject<BaseEntity>();
            baseEntity.Name = "Base";

            var derivedEntity = _repository.CreateObject<DerivedEntity>();
            derivedEntity.Name = "Derived";
            derivedEntity.SpecialProperty = "Special";

            // Assert
            // Should be able to query by base type
            var allBaseEntities = _repository.GetObjects<BaseEntity>().ToList();
            Assert.Equal(2, allBaseEntities.Count);
            Assert.Contains(baseEntity, allBaseEntities);
            Assert.Contains(derivedEntity, allBaseEntities);

            // Should be able to query by derived type
            var allDerivedEntities = _repository.GetObjects<DerivedEntity>().ToList();
            Assert.Single(allDerivedEntities);
            Assert.Equal("Special", allDerivedEntities[0].SpecialProperty);

            // Should be able to find by specific criteria
            var foundDerived = _repository.FindObject<DerivedEntity>(d => d.SpecialProperty == "Special");
            Assert.NotNull(foundDerived);
            Assert.Equal(derivedEntity.Id, foundDerived.Id);
        }

        [Fact]
        [Description("Should handle edge cases gracefully")]
        public void EdgeCases_ShouldBeHandledGracefully()
        {
            // Test empty queries
            var emptyResults = _repository.GetObjects<Customer>().Where(c => c.Name == "NonExistent").ToList();
            Assert.Empty(emptyResults);

            // Test null handling
            var nullResult = _repository.GetObjectByKey<Customer>(null);
            Assert.Null(nullResult);

            // Test with empty Guid
            var emptyGuidResult = _repository.GetObjectByKey<Customer>(Guid.Empty);
            Assert.Null(emptyGuidResult);

            // Test multiple commits
            var customer = _repository.CreateObject<Customer>();
            _repository.CommitChanges().Wait();
            _repository.CommitChanges().Wait(); // Should not throw
            
            Assert.False(_repository.IsModified);
            Assert.Single(_repository.GetObjects<Customer>());

            // Test multiple rollbacks
            _repository.CreateObject<Customer>();
            _repository.Rollback();
            _repository.Rollback(); // Should not throw
            
            Assert.False(_repository.IsModified);
            Assert.Single(_repository.GetObjects<Customer>()); // Original customer still there
        }
    }

    #region Test Entity Classes for Integration Tests

    public class Customer : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Product : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class Order : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class BaseEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class DerivedEntity : BaseEntity
    {
        public string SpecialProperty { get; set; } = string.Empty;
    }

    #endregion
}