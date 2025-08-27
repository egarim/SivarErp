using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Shared.Services.Sales;
using Sivar.Erp.Core.Shared.Services.Inventory;
using Sivar.Erp.Core.Shared.Services.Purchasing;
using Sivar.Erp.Core.Shared.Dtos.Sales;
using Sivar.Erp.Core.Shared.Dtos.Inventory;
using Sivar.Erp.Core.Shared.Dtos.Purchasing;
using Sivar.Erp.Core.Domain.Enums;
using NUnit.Framework;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// Comprehensive tests for the complete ERP business workflow
/// </summary>
public class ApplicationServicesIntegrationTests : BaseIntegrationTest
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly IInvoiceService _invoiceService;
    private readonly IInventoryService _inventoryService;
    private readonly IStockLevelService _stockLevelService;
    private readonly IInventoryTransactionService _inventoryTransactionService;
    private readonly IPurchaseOrderService _purchaseOrderService;
    private readonly ISupplierService _supplierService;

    public ApplicationServicesIntegrationTests()
    {
        _salesOrderService = ServiceProvider.GetRequiredService<ISalesOrderService>();
        _invoiceService = ServiceProvider.GetRequiredService<IInvoiceService>();
        _inventoryService = ServiceProvider.GetRequiredService<IInventoryService>();
        _stockLevelService = ServiceProvider.GetRequiredService<IStockLevelService>();
        _inventoryTransactionService = ServiceProvider.GetRequiredService<IInventoryTransactionService>();
        _purchaseOrderService = ServiceProvider.GetRequiredService<IPurchaseOrderService>();
        _supplierService = ServiceProvider.GetRequiredService<ISupplierService>();
    }

    [Test]
    public async Task CompleteBusinessWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var companyId = TestCompanyId;
        var createdBy = "test-user";

        // 1. Create a supplier
        var supplierDto = await CreateTestSupplier(companyId, createdBy);
        
        // 2. Create products
        var product1 = await CreateTestProduct(companyId, createdBy, "PROD001", "Test Product 1");
        var product2 = await CreateTestProduct(companyId, createdBy, "PROD002", "Test Product 2");

        // 3. Create a purchase order
        var purchaseOrder = await CreateTestPurchaseOrder(companyId, createdBy, supplierDto.Id, new[]
        {
            (product1.Id, 100m, 10.00m),
            (product2.Id, 50m, 20.00m)
        });

        // 4. Approve the purchase order
        var approvedPO = await _purchaseOrderService.ApproveAsync(purchaseOrder.Id, companyId, createdBy);
        Assert.That(approvedPO.Status, Is.EqualTo("Approved"));

        // 5. Receive goods (simulate goods receipt)
        var warehouse = await GetOrCreateTestWarehouse(companyId, createdBy);
        var receiveDto = new ReceiveGoodsDto
        {
            PurchaseOrderId = purchaseOrder.Id,
            ReceiveDate = DateTime.UtcNow.Date,
            ReceiptNumber = "GR-001",
            Notes = "Complete delivery",
            Lines = purchaseOrder.Lines.Select(l => new ReceiveGoodsLineDto
            {
                PurchaseOrderLineId = l.Id,
                QuantityReceived = l.Quantity,
                Notes = "Received in good condition"
            }).ToList()
        };

        var receivedPO = await _purchaseOrderService.ReceiveGoodsAsync(receiveDto, companyId, createdBy);
        Assert.That(receivedPO.Status, Is.EqualTo("Completed"));

        // 6. Verify stock levels were updated
        var stockLevels = await _stockLevelService.GetAllStockLevelsAsync(companyId, warehouse.Id);
        Assert.That(stockLevels.Count(), Is.EqualTo(2));
        
        var product1Stock = stockLevels.FirstOrDefault(sl => sl.ProductId == product1.Id);
        var product2Stock = stockLevels.FirstOrDefault(sl => sl.ProductId == product2.Id);
        
        Assert.That(product1Stock?.QuantityOnHand, Is.EqualTo(100m));
        Assert.That(product2Stock?.QuantityOnHand, Is.EqualTo(50m));

        // 7. Create a customer and sales order
        var customer = await GetOrCreateTestCustomer(companyId, createdBy);
        var salesOrder = await CreateTestSalesOrder(companyId, createdBy, customer.Id, new[]
        {
            (product1.Id, 20m, 15.00m),
            (product2.Id, 10m, 30.00m)
        });

        // 8. Approve the sales order
        var approvedSO = await _salesOrderService.ApproveAsync(salesOrder.Id, companyId, createdBy);
        Assert.That(approvedSO.Status, Is.EqualTo("Approved"));

        // 9. Convert to invoice
        var invoice = await _salesOrderService.ConvertToInvoiceAsync(salesOrder.Id, companyId, createdBy);
        Assert.That(invoice, Is.Not.Null);
        Assert.That(invoice.Status, Is.EqualTo("Draft"));

        // 10. Post the invoice
        var postedInvoice = await _invoiceService.PostAsync(invoice.Id, companyId, createdBy);
        Assert.That(postedInvoice.Status, Is.EqualTo("Posted"));

        // 11. Verify inventory transactions were created
        var transactions = await _inventoryTransactionService.GetAllTransactionsAsync(companyId);
        Assert.That(transactions.Count(), Is.GreaterThan(0));

        // 12. Test inventory adjustments
        var adjustmentDto = new CreateStockAdjustmentDto
        {
            ProductId = product1.Id,
            WarehouseId = warehouse.Id,
            NewQuantity = 85m, // Adjust down by 15 (we sold 20, but adjust to 85 instead of 80)
            Reference = "Cycle count adjustment",
            Notes = "Inventory count discrepancy"
        };

        var adjustment = await _inventoryTransactionService.CreateAdjustmentAsync(adjustmentDto, companyId, createdBy);
        Assert.That(adjustment, Is.Not.Null);

        // 13. Test stock transfer
        var warehouse2 = await CreateTestWarehouse(companyId, createdBy, "WH002", "Warehouse 2");
        var transferDto = new CreateStockTransferDto
        {
            ProductId = product2.Id,
            FromWarehouseId = warehouse.Id,
            ToWarehouseId = warehouse2.Id,
            Quantity = 25m,
            Reference = "Inter-warehouse transfer",
            Notes = "Moving stock to new location"
        };

        var transfer = await _inventoryTransactionService.CreateTransferAsync(transferDto, companyId, createdBy);
        Assert.That(transfer, Is.Not.Null);

        // 14. Verify final stock levels
        var finalStockLevels = await _stockLevelService.GetAllStockLevelsAsync(companyId);
        var finalProduct1Stock = finalStockLevels.FirstOrDefault(sl => sl.ProductId == product1.Id && sl.WarehouseId == warehouse.Id);
        var finalProduct2MainStock = finalStockLevels.FirstOrDefault(sl => sl.ProductId == product2.Id && sl.WarehouseId == warehouse.Id);
        var finalProduct2TransferStock = finalStockLevels.FirstOrDefault(sl => sl.ProductId == product2.Id && sl.WarehouseId == warehouse2.Id);

        Assert.That(finalProduct1Stock?.QuantityOnHand, Is.EqualTo(85m)); // Adjusted quantity
        Assert.That(finalProduct2MainStock?.QuantityOnHand, Is.EqualTo(15m)); // 50 - 10 (sold) - 25 (transferred)
        Assert.That(finalProduct2TransferStock?.QuantityOnHand, Is.EqualTo(25m)); // Transferred quantity

        // 15. Test reporting and statistics
        var salesStats = await _salesOrderService.GetStatisticsAsync(companyId);
        Assert.That(salesStats.TotalOrders, Is.EqualTo(1));
        Assert.That(salesStats.InvoicedOrders, Is.EqualTo(1));

        var invoiceStats = await _invoiceService.GetStatisticsAsync(companyId);
        Assert.That(invoiceStats.TotalInvoices, Is.EqualTo(1));
        Assert.That(invoiceStats.PostedInvoices, Is.EqualTo(1));

        var purchaseStats = await _purchaseOrderService.GetStatisticsAsync(companyId);
        Assert.That(purchaseStats.TotalOrders, Is.EqualTo(1));
        Assert.That(purchaseStats.CompletelyReceivedOrders, Is.EqualTo(1));

        // 16. Test inventory valuation
        var valuation = await _stockLevelService.GetInventoryValuationAsync(companyId);
        Assert.That(valuation.TotalQuantity, Is.GreaterThan(0));
        Assert.That(valuation.TotalValue, Is.GreaterThan(0));

        // 17. Test low stock alerts
        var lowStockAlerts = await _stockLevelService.GetLowStockAlertsAsync(companyId);
        // This depends on the minimum stock settings for products

        Console.WriteLine("✅ Complete business workflow executed successfully!");
        Console.WriteLine($"📊 Sales Orders: {salesStats.TotalOrders}, Invoices: {invoiceStats.TotalInvoices}");
        Console.WriteLine($"📦 Purchase Orders: {purchaseStats.TotalOrders}, Inventory Value: ${valuation.TotalValue:N2}");
    }

    private async Task<SupplierDto> CreateTestSupplier(Guid companyId, string createdBy)
    {
        var createDto = new CreateSupplierDto
        {
            Code = "SUP001",
            Name = "Test Supplier Ltd.",
            TradeName = "Test Supplier",
            Email = "supplier@test.com",
            Phone = "+1234567890",
            ContactPerson = "John Supplier",
            Address = "123 Supplier St",
            City = "Supplier City",
            Country = "Test Country",
            Currency = "USD",
            PaymentTermsDays = 30,
            CreditLimit = 50000m
        };

        return await _supplierService.CreateAsync(createDto, companyId, createdBy);
    }

    private async Task<ProductDto> CreateTestProduct(Guid companyId, string createdBy, string code, string name)
    {
        var createDto = new CreateProductDto
        {
            Code = code,
            Name = name,
            Description = $"Test product: {name}",
            UnitOfMeasure = "EA",
            UnitPrice = 25.00m,
            StandardCost = 15.00m,
            ValuationMethod = InventoryValuationMethod.WeightedAverage,
            MinimumStock = 10m,
            ReorderPoint = 20m,
            IsActive = true,
            InitialStock = 0 // We'll add stock through purchase orders
        };

        return await _inventoryService.CreateProductAsync(createDto, companyId, createdBy);
    }

    private async Task<PurchaseOrderDto> CreateTestPurchaseOrder(Guid companyId, string createdBy, Guid supplierId, (Guid ProductId, decimal Quantity, decimal UnitPrice)[] lines)
    {
        var warehouse = await GetOrCreateTestWarehouse(companyId, createdBy);
        
        var createDto = new CreatePurchaseOrderDto
        {
            SupplierId = supplierId,
            OrderDate = DateTime.UtcNow.Date,
            RequiredDate = DateTime.UtcNow.Date.AddDays(7),
            Currency = "USD",
            ExchangeRate = 1.0m,
            Notes = "Test purchase order",
            DeliveryWarehouseId = warehouse.Id,
            Lines = lines.Select(l => new CreatePurchaseOrderLineDto
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Discount = 0,
                TaxRate = 0
            }).ToList()
        };

        return await _purchaseOrderService.CreateAsync(createDto, companyId, createdBy);
    }

    private async Task<SalesOrderDto> CreateTestSalesOrder(Guid companyId, string createdBy, Guid customerId, (Guid ProductId, decimal Quantity, decimal UnitPrice)[] lines)
    {
        var createDto = new CreateSalesOrderDto
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow.Date,
            RequiredDate = DateTime.UtcNow.Date.AddDays(3),
            Currency = "USD",
            ExchangeRate = 1.0m,
            Notes = "Test sales order",
            Lines = lines.Select((l, index) => new CreateSalesOrderLineDto
            {
                LineNumber = index + 1,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Discount = 0,
                TaxRate = 0
            }).ToList()
        };

        return await _salesOrderService.CreateAsync(createDto, companyId, createdBy);
    }

    // Helper methods for test entities (these would be implemented based on your existing test infrastructure)
    private async Task<WarehouseDto> GetOrCreateTestWarehouse(Guid companyId, string createdBy)
    {
        // This would get or create a test warehouse
        var warehouses = await _inventoryService.GetWarehousesAsync(companyId);
        var warehouse = warehouses.FirstOrDefault();
        
        if (warehouse == null)
        {
            return await CreateTestWarehouse(companyId, createdBy, "WH001", "Main Warehouse");
        }
        
        return warehouse;
    }

    private async Task<WarehouseDto> CreateTestWarehouse(Guid companyId, string createdBy, string code, string name)
    {
        // This would create a warehouse - implementation depends on your warehouse service
        return new WarehouseDto
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = $"Test warehouse: {name}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private Task<CustomerDto> GetOrCreateTestCustomer(Guid companyId, string createdBy)
    {
        // This would get or create a test customer
        return Task.FromResult(new CustomerDto
        {
            Id = Guid.NewGuid(),
            Code = "CUST001",
            Name = "Test Customer Ltd."
        });
    }

    // Placeholder CustomerDto for the test (you'd use your actual CustomerDto)
    private class CustomerDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
