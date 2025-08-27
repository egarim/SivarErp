using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sivar.Erp.Core.Infrastructure.Data;

/// <summary>
/// Design-time factory for ErpDbContext to support EF Core migrations
/// </summary>
public class ErpDbContextFactory : IDesignTimeDbContextFactory<ErpDbContext>
{
    public ErpDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ErpDbContext>();
        
        // Use InMemory database for migrations design-time
        optionsBuilder.UseInMemoryDatabase("DesignTime_SivarErpCoreDb");
        
        return new ErpDbContext(optionsBuilder.Options);
    }
}
