using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Domain.Entities.Accounting;
using Sivar.Erp.Core.Domain.Interfaces.Repositories.Accounting;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Infrastructure.Repositories;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Infrastructure.Repositories.Accounting;

/// <summary>
/// Repository implementation for Journal Entry entities
/// </summary>
public class JournalEntryRepository : TenantRepository<JournalEntry>, IJournalEntryRepository
{
    private readonly ErpDbContext _erpContext;

    public JournalEntryRepository(ErpDbContext context) : base(context)
    {
        _erpContext = context;
    }

    public async Task<JournalEntry?> GetByNumberAsync(string number, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(je => je.Number == number, cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(je => je.Status == status)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByDateRangeAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(je => je.TransactionDate >= fromDate && je.TransactionDate <= toDate)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByPeriodAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(je => je.AccountingPeriodId == periodId)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(je => je.Lines.Any(l => l.AccountId == accountId))
            .Include(je => je.Lines)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetByApprovalStatusAsync(ApprovalStatus approvalStatus, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(je => je.ApprovalStatus == approvalStatus)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetBySourceAsync(string sourceType, Guid sourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(je => je.SourceType == sourceType && je.SourceId == sourceId)
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<JournalEntry?> GetWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(je => je.Lines)
                .ThenInclude(l => l.Account)
            .Include(je => je.AccountingPeriod)
            .FirstOrDefaultAsync(je => je.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<JournalEntry>> GetWithLinesAsync(
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        JournalEntryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(je => je.Lines)
                .ThenInclude(l => l.Account)
            .Include(je => je.AccountingPeriod)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(je => je.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(je => je.TransactionDate <= toDate.Value);

        if (status.HasValue)
            query = query.Where(je => je.Status == status.Value);

        return await query
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NumberExistsAsync(string number, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(je => je.Number == number);

        if (excludeId.HasValue)
        {
            query = query.Where(je => je.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.Now.Year;
        var prefix = $"JE{year:0000}-";
        
        var lastNumber = await _dbSet
            .Where(je => je.Number.StartsWith(prefix))
            .Select(je => je.Number)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastNumber == null)
        {
            return $"{prefix}0001";
        }

        var numberPart = lastNumber.Substring(prefix.Length);
        if (int.TryParse(numberPart, out var lastNum))
        {
            return $"{prefix}{(lastNum + 1):0000}";
        }

        return $"{prefix}0001";
    }

    public async Task PostAsync(Guid id, string postedBy, CancellationToken cancellationToken = default)
    {
        var journalEntry = await GetByIdAsync(id, cancellationToken);
        if (journalEntry != null)
        {
            journalEntry.Status = JournalEntryStatus.Posted;
            journalEntry.PostedBy = postedBy;
            journalEntry.PostedAt = DateTime.UtcNow;
            journalEntry.PostingDate = DateOnly.FromDateTime(DateTime.UtcNow);
            
            await _erpContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ReverseAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var journalEntry = await GetByIdAsync(id, cancellationToken);
        if (journalEntry != null)
        {
            journalEntry.Status = JournalEntryStatus.Reversed;
            journalEntry.Description = $"{journalEntry.Description} - REVERSED: {reason}";
            
            await _erpContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ApproveAsync(Guid id, string approvedBy, string? notes = null, CancellationToken cancellationToken = default)
    {
        var journalEntry = await GetByIdAsync(id, cancellationToken);
        if (journalEntry != null)
        {
            journalEntry.ApprovalStatus = ApprovalStatus.Approved;
            journalEntry.ApprovedBy = approvedBy;
            journalEntry.ApprovedAt = DateTime.UtcNow;
            journalEntry.ApprovalNotes = notes;
            
            await _erpContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RejectAsync(Guid id, string rejectedBy, string reason, CancellationToken cancellationToken = default)
    {
        var journalEntry = await GetByIdAsync(id, cancellationToken);
        if (journalEntry != null)
        {
            journalEntry.ApprovalStatus = ApprovalStatus.Rejected;
            journalEntry.ApprovedBy = rejectedBy;
            journalEntry.ApprovedAt = DateTime.UtcNow;
            journalEntry.ApprovalNotes = reason;
            
            await _erpContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<JournalEntry>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(je => je.Description.ToLower().Contains(normalizedSearchTerm) ||
                        (je.Reference != null && je.Reference.ToLower().Contains(normalizedSearchTerm)) ||
                        je.Number.ToLower().Contains(normalizedSearchTerm))
            .OrderByDescending(je => je.TransactionDate)
            .ThenByDescending(je => je.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<JournalEntryStatus, int>> GetStatusStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .GroupBy(je => je.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<(decimal TotalDebit, decimal TotalCredit)> GetPeriodTotalsAsync(DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var totals = await _dbSet
            .Where(je => je.TransactionDate >= fromDate && je.TransactionDate <= toDate && je.Status == JournalEntryStatus.Posted)
            .GroupBy(je => 1)
            .Select(g => new { TotalDebit = g.Sum(je => je.TotalDebit), TotalCredit = g.Sum(je => je.TotalCredit) })
            .FirstOrDefaultAsync(cancellationToken);

        return (totals?.TotalDebit ?? 0, totals?.TotalCredit ?? 0);
    }
}
