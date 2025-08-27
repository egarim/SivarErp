using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Shared.Interfaces;

namespace Sivar.Erp.Core.Application.Services.Sales;

/// <summary>
/// Invoice service implementation
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly ErpDbContext _context;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ErpDbContext context, ILogger<InvoiceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CreateInvoiceAsync(int productId, int quantity, decimal price)
    {
        // Placeholder implementation
        _logger.LogInformation($"Creating invoice: ProductId={productId}, Quantity={quantity}, Price={price}");
        return await Task.FromResult(true);
    }
}
