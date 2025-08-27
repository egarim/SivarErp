using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Inventory;
using Sivar.Erp.Core.Domain.Entities.Inventory;
using Sivar.Erp.Core.Domain.Enums;
using Sivar.Erp.Core.Infrastructure.Data;

namespace Sivar.Erp.Core.Infrastructure.Repositories.Inventory;

/// <summary>
/// Implementation of product repository
/// </summary>
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ErpDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Product>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByTypeAsync(ProductType type, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Product>()
            .Where(p => p.Type == type && p.IsActive)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Product>()
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Product>().Where(p => p.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}

/// <summary>
/// Implementation of warehouse repository
/// </summary>
public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ErpDbContext context) : base(context)
    {
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Warehouse>()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Warehouse>()
            .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Warehouse>().Where(w => w.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
