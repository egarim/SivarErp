using Sivar.Erp.Services;
using Sivar.Erp.EfCore.Context;
using Sivar.Erp.EfCore.UnitOfWork;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Modules.Payments.Models;
using Sivar.Erp.Modules.Inventory;

namespace Sivar.Erp.EfCore.Adapters
{
    /// <summary>
    /// Adapter that makes DbContext implement IObjectDb interface for backward compatibility
    /// </summary>
    public class DbContextObjectDbAdapter : IObjectDb
    {
        private readonly ErpDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;

        public DbContextObjectDbAdapter(ErpDbContext dbContext, IUnitOfWork unitOfWork)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
        }

        // Core entities
        public IList<IAccount> Accounts
        {
            get => new EntityFrameworkCollection<IAccount>(
                _dbContext.Accounts.Cast<IAccount>().AsQueryable(),
                account =>
                {
                    var efAccount = ConvertToEfCoreAccount(account);
                    if (efAccount != null) _dbContext.Accounts.Add(efAccount);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<IBusinessEntity> BusinessEntities
        {
            get => new EntityFrameworkCollection<IBusinessEntity>(
                _dbContext.BusinessEntities.Cast<IBusinessEntity>().AsQueryable(),
                entity =>
                {
                    var efEntity = ConvertToEfCoreBusinessEntity(entity);
                    if (efEntity != null) _dbContext.BusinessEntities.Add(efEntity);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<IDocumentType> DocumentTypes
        {
            get => new EntityFrameworkCollection<IDocumentType>(
                _dbContext.DocumentTypes.Cast<IDocumentType>().AsQueryable(),
                docType =>
                {
                    var efDocType = ConvertToEfCoreDocumentType(docType);
                    if (efDocType != null) _dbContext.DocumentTypes.Add(efDocType);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<IItem> Items
        {
            get => new EntityFrameworkCollection<IItem>(
                _dbContext.Items.Cast<IItem>().AsQueryable(),
                item =>
                {
                    var efItem = ConvertToEfCoreItem(item);
                    if (efItem != null) _dbContext.Items.Add(efItem);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        // Accounting entities
        public IList<IFiscalPeriod> fiscalPeriods
        {
            get => new EntityFrameworkCollection<IFiscalPeriod>(
                _dbContext.FiscalPeriods.Cast<IFiscalPeriod>().AsQueryable(),
                item =>
                {
                    var efFiscalPeriod = ConvertToEfCoreFiscalPeriod(item);
                    if (efFiscalPeriod != null) _dbContext.FiscalPeriods.Add(efFiscalPeriod);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<ITransaction> Transactions
        {
            get => new EntityFrameworkCollection<ITransaction>(_dbContext.Transactions.Cast<ITransaction>().AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<ILedgerEntry> LedgerEntries
        {
            get => new EntityFrameworkCollection<ILedgerEntry>(_dbContext.LedgerEntries.Cast<ILedgerEntry>().AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<ITransactionBatch> TransactionBatches { get; set; } = new List<ITransactionBatch>();

        public IList<IDocumentAccountingProfile> DocumentAccountingProfiles { get; set; } = new List<IDocumentAccountingProfile>();

        // Tax entities
        public IList<ITax> Taxes
        {
            get => new EntityFrameworkCollection<ITax>(
                _dbContext.Taxes.Cast<ITax>().AsQueryable(),
                tax =>
                {
                    var efTax = ConvertToEfCoreTax(tax);
                    if (efTax != null) _dbContext.Taxes.Add(efTax);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<ITaxGroup> TaxGroups
        {
            get => new EntityFrameworkCollection<ITaxGroup>(
                _dbContext.TaxGroups.Cast<ITaxGroup>().AsQueryable(),
                taxGroup =>
                {
                    var efTaxGroup = ConvertToEfCoreTaxGroup(taxGroup);
                    if (efTaxGroup != null) _dbContext.TaxGroups.Add(efTaxGroup);
                });
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<ITaxRule> TaxRules
        {
            get => new EntityFrameworkCollection<ITaxRule>(_dbContext.TaxRules.Cast<ITaxRule>().AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<GroupMembershipDto> GroupMemberships
        {
            get => new EntityFrameworkCollection<GroupMembershipDto>(
                _dbContext.GroupMemberships.AsQueryable(),
                membership => _dbContext.GroupMemberships.Add(membership));
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        // System entities
        public IList<ActivityRecord> ActivityRecords
        {
            get => new EntityFrameworkCollection<ActivityRecord>(_dbContext.ActivityRecords.Cast<Sivar.Erp.ErpSystem.ActivityStream.ActivityRecord>().AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<PerformanceLog> PerformanceLogs
        {
            get => new EntityFrameworkCollection<PerformanceLog>(_dbContext.PerformanceLogs
                .Select(p => new Sivar.Erp.ErpSystem.Diagnostics.PerformanceLog
                {
                    Id = (int)p.Id.GetHashCode(), // Convert Guid to int
                    Timestamp = p.Timestamp,
                    Method = p.Method,
                    ExecutionTimeMs = p.ExecutionTimeMs,
                    MemoryDeltaBytes = p.MemoryDeltaBytes,
                    IsSlow = p.IsSlow,
                    IsMemoryIntensive = p.IsMemoryIntensive,
                    UserId = null, // Not available in EF Core entity yet
                    UserName = p.UserName,
                    InstanceId = p.InstanceId,
                    SessionId = null, // Not available in EF Core entity yet
                    Context = p.Context
                })
                .AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<SequenceDto> Sequences
        {
            get => new EntityFrameworkCollection<SequenceDto>(_dbContext.Sequences.AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        // Security entities
        public IList<User> Users
        {
            get => new EntityFrameworkCollection<User>(_dbContext.Users.AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<Role> Roles
        {
            get => new EntityFrameworkCollection<Role>(_dbContext.Roles.AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<SecurityEvent> SecurityEvents
        {
            get => new EntityFrameworkCollection<SecurityEvent>(_dbContext.SecurityEvents.AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        // Payment entities
        public IList<PaymentMethodDto> PaymentMethods
        {
            get => new EntityFrameworkCollection<PaymentMethodDto>(_dbContext.PaymentMethods.AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        public IList<PaymentDto> Payments
        {
            get => new EntityFrameworkCollection<PaymentDto>(_dbContext.Payments.AsQueryable());
            set => throw new NotSupportedException("Setting collections is not supported in EF Core adapter");
        }

        // Inventory entities (placeholder implementations)
        public IList<IInventoryItem> InventoryItems { get; set; } = new List<IInventoryItem>();
        public IList<IStockLevel> StockLevels { get; set; } = new List<IStockLevel>();
        public IList<IInventoryTransaction> InventoryTransactions { get; set; } = new List<IInventoryTransaction>();
        public IList<IInventoryReservation> InventoryReservations { get; set; } = new List<IInventoryReservation>();
        public IList<InventoryLayerDto> InventoryLayers { get; set; } = new List<InventoryLayerDto>();

        // Conversion methods to convert DTOs to EF Core entities
        private Sivar.Erp.EfCore.Entities.Core.Account? ConvertToEfCoreAccount(IAccount account)
        {
            // If it's already an EF Core account, return it
            if (account is Sivar.Erp.EfCore.Entities.Core.Account efAccount)
                return efAccount;

            // Convert from DTO to EF Core entity
            if (account is Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountDto accountDto)
            {
                return new Sivar.Erp.EfCore.Entities.Core.Account
                {
                    Oid = Guid.NewGuid(), // Generate new ID since AccountDto doesn't have Oid
                    AccountName = accountDto.AccountName,
                    AccountType = accountDto.AccountType,
                    OfficialCode = accountDto.OfficialCode,
                    ParentOfficialCode = accountDto.ParentOfficialCode,
                    BalanceAndIncomeLineId = accountDto.BalanceAndIncomeLineId,
                    InsertedBy = accountDto.InsertedBy,
                    UpdatedBy = accountDto.UpdatedBy,
                    InsertedAt = accountDto.InsertedAt,
                    UpdatedAt = accountDto.UpdatedAt
                };
            }

            return null;
        }

        private Sivar.Erp.EfCore.Entities.Core.BusinessEntity? ConvertToEfCoreBusinessEntity(IBusinessEntity businessEntity)
        {
            // If it's already an EF Core entity, return it
            if (businessEntity is Sivar.Erp.EfCore.Entities.Core.BusinessEntity efEntity)
                return efEntity;

            // Convert from DTO to EF Core entity
            if (businessEntity is Sivar.Erp.BusinessEntities.BusinessEntityDto entityDto)
            {
                return new Sivar.Erp.EfCore.Entities.Core.BusinessEntity
                {
                    Oid = entityDto.Oid,
                    Code = entityDto.Code,
                    Name = entityDto.Name,
                    Address = entityDto.Address,
                    City = entityDto.City,
                    State = entityDto.State,
                    ZipCode = entityDto.ZipCode,
                    Country = entityDto.Country,
                    PhoneNumber = entityDto.PhoneNumber,
                    Email = entityDto.Email
                };
            }

            return null;
        }

        private Sivar.Erp.EfCore.Entities.Core.DocumentType? ConvertToEfCoreDocumentType(IDocumentType documentType)
        {
            // If it's already an EF Core entity, return it
            if (documentType is Sivar.Erp.EfCore.Entities.Core.DocumentType efDocType)
                return efDocType;

            // Convert from DTO to EF Core entity
            if (documentType is Sivar.Erp.Documents.DocumentTypeDto docTypeDto)
            {
                return new Sivar.Erp.EfCore.Entities.Core.DocumentType
                {
                    Oid = docTypeDto.Oid,
                    Code = docTypeDto.Code,
                    Name = docTypeDto.Name,
                    IsEnabled = docTypeDto.IsEnabled,
                    DocumentOperation = docTypeDto.DocumentOperation
                };
            }

            return null;
        }

        private Sivar.Erp.EfCore.Entities.Core.Item? ConvertToEfCoreItem(IItem item)
        {
            // If it's already an EF Core entity, return it
            if (item is Sivar.Erp.EfCore.Entities.Core.Item efItem)
                return efItem;

            // Convert from DTO to EF Core entity
            if (item is Sivar.Erp.Documents.ItemDto itemDto)
            {
                return new Sivar.Erp.EfCore.Entities.Core.Item
                {
                    Oid = itemDto.Oid,
                    Code = itemDto.Code,
                    Type = itemDto.Type,
                    Description = itemDto.Description,
                    BasePrice = itemDto.BasePrice
                };
            }

            return null;
        }

        private Sivar.Erp.EfCore.Entities.Tax.Tax? ConvertToEfCoreTax(ITax tax)
        {
            // If it's already an EF Core tax, return it
            if (tax is Sivar.Erp.EfCore.Entities.Tax.Tax efTax)
                return efTax;

            // Convert from DTO to EF Core entity
            if (tax is Sivar.Erp.Services.Taxes.TaxDto taxDto)
            {
                return new Sivar.Erp.EfCore.Entities.Tax.Tax
                {
                    Oid = taxDto.Oid,
                    Name = taxDto.Name,
                    Code = taxDto.Code,
                    Amount = taxDto.Amount,
                    Percentage = taxDto.Percentage,
                    ApplicationLevel = taxDto.ApplicationLevel,
                    IsEnabled = taxDto.IsEnabled,
                    IsIncludedInPrice = taxDto.IsIncludedInPrice
                };
            }

            return null;
        }

        private Sivar.Erp.EfCore.Entities.Tax.TaxGroup? ConvertToEfCoreTaxGroup(ITaxGroup taxGroup)
        {
            // If it's already an EF Core tax group, return it
            if (taxGroup is Sivar.Erp.EfCore.Entities.Tax.TaxGroup efTaxGroup)
                return efTaxGroup;

            // Convert from DTO to EF Core entity
            if (taxGroup is Sivar.Erp.Services.Taxes.TaxGroup.TaxGroupDto taxGroupDto)
            {
                return new Sivar.Erp.EfCore.Entities.Tax.TaxGroup
                {
                    Oid = taxGroupDto.Oid,
                    Code = taxGroupDto.Code,
                    Name = taxGroupDto.Name,
                    Description = taxGroupDto.Description,
                    IsEnabled = taxGroupDto.IsEnabled
                };
            }

            return null;
        }

        private Sivar.Erp.EfCore.Entities.Accounting.FiscalPeriod? ConvertToEfCoreFiscalPeriod(IFiscalPeriod fiscalPeriod)
        {
            // If it's already an EF Core entity, return it
            if (fiscalPeriod is Sivar.Erp.EfCore.Entities.Accounting.FiscalPeriod efFiscalPeriod)
                return efFiscalPeriod;

            // Convert from DTO to EF Core entity - use interface properties
            try
            {
                return new Sivar.Erp.EfCore.Entities.Accounting.FiscalPeriod
                {
                    Code = fiscalPeriod.Code,
                    Name = fiscalPeriod.Name,
                    StartDate = fiscalPeriod.StartDate,
                    EndDate = fiscalPeriod.EndDate,
                    Status = fiscalPeriod.Status,
                    Description = fiscalPeriod.Description,
                    InsertedAt = DateTime.UtcNow,
                    InsertedBy = "system",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "system"
                };
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Helper class to make IQueryable behave like IList for backward compatibility
    /// </summary>
    public class EntityFrameworkCollection<T> : IList<T>
    {
        private readonly IQueryable<T> _queryable;
        private readonly Action<T>? _addAction;
        private List<T>? _cachedList;

        public EntityFrameworkCollection(IQueryable<T> queryable, Action<T>? addAction = null)
        {
            _queryable = queryable;
            _addAction = addAction;
        }

        private List<T> GetList()
        {
            return _cachedList ??= _queryable.ToList();
        }

        public T this[int index]
        {
            get => GetList()[index];
            set => GetList()[index] = value;
        }

        public int Count => GetList().Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            // If we have an add action (for adding to DbSet), use it
            if (_addAction != null)
            {
                _addAction(item);
            }
            else
            {
                // Fallback to in-memory list for collections without DbSet backing
                GetList().Add(item);
            }
            _cachedList = null; // Invalidate cache
        }

        public void Clear()
        {
            GetList().Clear();
            _cachedList = null;
        }

        public bool Contains(T item) => GetList().Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => GetList().CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => GetList().GetEnumerator();
        public int IndexOf(T item) => GetList().IndexOf(item);
        public void Insert(int index, T item) => GetList().Insert(index, item);
        public bool Remove(T item) => GetList().Remove(item);
        public void RemoveAt(int index) => GetList().RemoveAt(index);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
