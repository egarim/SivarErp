using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Shared.Interfaces;

namespace Sivar.Erp.Core.Application.Services.Purchasing;

/// <summary>
/// Purchase order service implementation
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly ErpDbContext _context;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(ErpDbContext context, ILogger<PurchaseOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CreatePurchaseOrderAsync(int productId, int quantity, decimal price)
    {
        // Placeholder implementation
        _logger.LogInformation($"Creating purchase order: ProductId={productId}, Quantity={quantity}, Price={price}");
        return await Task.FromResult(true);
    }
}
