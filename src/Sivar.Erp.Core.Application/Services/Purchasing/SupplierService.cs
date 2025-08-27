using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Shared.Interfaces;

namespace Sivar.Erp.Core.Application.Services.Purchasing;

/// <summary>
/// Supplier service implementation
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly ErpDbContext _context;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(ErpDbContext context, ILogger<SupplierService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<object>> GetAllSuppliersAsync()
    {
        // Placeholder implementation
        _logger.LogInformation("Getting all suppliers");
        return await Task.FromResult(new List<object>());
    }
}
