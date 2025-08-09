using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using System.ComponentModel;

namespace Sivar.Erp.Core.Tests.Demo
{
    /// <summary>
    /// Demonstration of InMemoryRepository usage for Day 3 deliverables
    /// </summary>
    [Description("Demonstration of InMemoryRepository functionality")]
    public class RepositoryDemoTests
    {
        [Fact]
        [Description("Complete demo of repository functionality")]
        public async Task CompleteRepositoryDemo_ShouldShowAllFeatures()
        {
            // Create repository instance
            using var repository = new InMemoryRepository();

            // 1. Create entities and demonstrate auto-initialization
            var account1 = repository.CreateObject<SampleAccount>();
            account1.Code = "1000";
            account1.Name = "Cash";
            account1.Balance = 1000.00m;

            var account2 = repository.CreateObject<SampleAccount>();
            account2.Code = "2000";
            account2.Name = "Accounts Payable";
            account2.Balance = -500.00m;

            // Verify IEntity properties are auto-initialized
            Assert.NotEqual(Guid.Empty, account1.Id);
            Assert.NotEqual(Guid.Empty, account2.Id);
            Assert.True(account1.CreatedAt > DateTime.MinValue);
            Assert.True(account2.CreatedAt > DateTime.MinValue);

            // 2. Demonstrate querying capabilities
            var allAccounts = repository.GetObjects<SampleAccount>().ToList();
            Assert.Equal(2, allAccounts.Count);

            // LINQ queries work seamlessly
            var cashAccounts = repository.GetObjects<SampleAccount>()
                .Where(a => a.Name.Contains("Cash"))
                .ToList();
            Assert.Single(cashAccounts);

            // Find specific account
            var payableAccount = repository.FindObject<SampleAccount>(a => a.Code == "2000");
            Assert.NotNull(payableAccount);
            Assert.Equal("Accounts Payable", payableAccount.Name);

            // 3. Demonstrate key-based retrieval
            var foundAccount = repository.GetObjectByKey<SampleAccount>(account1.Id);
            Assert.NotNull(foundAccount);
            Assert.Same(account1, foundAccount);

            // 4. Demonstrate change tracking
            Assert.True(repository.IsModified); // Has new objects

            await repository.CommitChanges();
            Assert.False(repository.IsModified); // Clean state after commit

            // Modify existing entity
            repository.MarkAsModified(account1);
            account1.Balance = 1500.00m;
            Assert.True(repository.IsModified); // Has modified objects

            // 5. Demonstrate rollback functionality
            var account3 = repository.CreateObject<SampleAccount>();
            account3.Code = "3000";
            account3.Name = "Revenue";

            Assert.Equal(3, repository.GetObjects<SampleAccount>().Count());
            Assert.True(repository.IsModified);

            repository.Rollback();
            Assert.Equal(2, repository.GetObjects<SampleAccount>().Count()); // account3 removed
            Assert.False(repository.IsModified);
            Assert.Equal(1000.00m, account1.Balance); // Modification tracking cleared

            // 6. Demonstrate final commit
            repository.MarkAsModified(account2);
            account2.Balance = -750.00m;
            
            await repository.CommitChanges();
            Assert.False(repository.IsModified);
            Assert.Equal(-750.00m, account2.Balance);

            // 7. Demonstrate statistics and utility methods
            var stats = repository.GetStatistics();
            Assert.True(stats.ContainsKey("SampleAccount"));
            Assert.Equal(2, stats["SampleAccount"]);

            // Repository successfully completed all operations
            Assert.Equal(2, repository.GetObjects<SampleAccount>().Count());
        }

        [Fact]
        [Description("Performance test with realistic data volumes")]
        public async Task PerformanceDemo_ShouldHandleLargeDataSets()
        {
            using var repository = new InMemoryRepository();
            const int recordCount = 10000;

            var startTime = DateTime.UtcNow;

            // Create large number of records
            for (int i = 0; i < recordCount; i++)
            {
                var account = repository.CreateObject<SampleAccount>();
                account.Code = $"ACC{i:D6}";
                account.Name = $"Account {i}";
                account.Balance = i * 10.5m;
            }

            var createTime = DateTime.UtcNow;

            // Perform complex queries
            var expensiveAccounts = repository.GetObjects<SampleAccount>()
                .Where(a => a.Balance > 50000)
                .OrderByDescending(a => a.Balance)
                .Take(100)
                .ToList();

            var specificAccounts = repository.GetObjects<SampleAccount>()
                .Where(a => a.Code.Contains("999"))
                .ToList();

            var queryTime = DateTime.UtcNow;

            // Commit changes
            await repository.CommitChanges();

            var commitTime = DateTime.UtcNow;

            // Assert reasonable performance
            var createDuration = createTime - startTime;
            var queryDuration = queryTime - createTime;
            var commitDuration = commitTime - queryTime;

            Assert.Equal(recordCount, repository.GetObjects<SampleAccount>().Count());
            Assert.True(expensiveAccounts.Count > 0);
            Assert.True(specificAccounts.Count > 0);

            // Performance should be reasonable for in-memory operations
            Assert.True(createDuration.TotalSeconds < 10, $"Creation took {createDuration.TotalSeconds} seconds");
            Assert.True(queryDuration.TotalSeconds < 5, $"Queries took {queryDuration.TotalSeconds} seconds");
            Assert.True(commitDuration.TotalSeconds < 2, $"Commit took {commitDuration.TotalSeconds} seconds");
        }

        [Fact]
        [Description("Thread safety demonstration")]
        public async Task ThreadSafetyDemo_ShouldHandleConcurrentAccess()
        {
            using var repository = new InMemoryRepository();
            const int taskCount = 50;
            const int operationsPerTask = 100;

            var tasks = Enumerable.Range(0, taskCount).Select(async taskId =>
            {
                var accounts = new List<SampleAccount>();

                // Each task creates its own accounts
                for (int i = 0; i < operationsPerTask; i++)
                {
                    var account = repository.CreateObject<SampleAccount>();
                    account.Code = $"T{taskId:D2}A{i:D3}";
                    account.Name = $"Task {taskId} Account {i}";
                    account.Balance = taskId * 100 + i;
                    accounts.Add(account);
                }

                // Perform some queries and modifications
                await Task.Delay(1); // Simulate some work

                foreach (var account in accounts.Take(10))
                {
                    var found = repository.GetObjectByKey<SampleAccount>(account.Id);
                    Assert.NotNull(found);
                    
                    repository.MarkAsModified(account);
                    account.Balance += 1000;
                }

                return accounts.Count;
            });

            var results = await Task.WhenAll(tasks);

            // Verify all operations completed successfully
            var totalAccounts = repository.GetObjects<SampleAccount>().Count();
            var expectedTotal = taskCount * operationsPerTask;
            
            Assert.Equal(expectedTotal, totalAccounts);
            Assert.Equal(expectedTotal, results.Sum());
            
            // Verify unique IDs
            var allIds = repository.GetObjects<SampleAccount>().Select(a => a.Id).ToHashSet();
            Assert.Equal(expectedTotal, allIds.Count);

            await repository.CommitChanges();
            Assert.False(repository.IsModified);
        }
    }

    /// <summary>
    /// Sample entity for demonstration purposes
    /// </summary>
    public class SampleAccount : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}