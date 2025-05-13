using System;

namespace Sivar.Erp.FinancialStatements.BalanceAndIncome
{
    /// <summary>
    /// Abstract base implementation of balance and income line service
    /// </summary>
    public abstract class BalanceAndIncomeLineServiceBase : IBalanceAndIncomeLineService
    {
        protected readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the service
        /// </summary>
        /// <param name="auditService">Audit service</param>
        protected BalanceAndIncomeLineServiceBase(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Creates a new line
        /// </summary>
        /// <param name="line">Line to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created line with ID</returns>
        public virtual async Task<IBalanceAndIncomeLine> CreateLineAsync(IBalanceAndIncomeLine line, string userName)
        {
            // Validate the line
            var validationResult = await ValidateLineAsync(line);
            if (!validationResult.IsValid)
            {
                throw new Exception($"Line validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Generate ID if not provided
            if (line.Id == Guid.Empty)
            {
                line.Id = Guid.NewGuid();
            }

            // Set audit information
            _auditService.SetCreationAudit(line, userName);

            // Derived classes implement actual persistence
            return await CreateLineInternalAsync(line);
        }

        /// <summary>
        /// Updates an existing line
        /// </summary>
        /// <param name="line">Line with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated line</returns>
        public virtual async Task<IBalanceAndIncomeLine> UpdateLineAsync(IBalanceAndIncomeLine line, string userName)
        {
            if (line.Id == Guid.Empty)
            {
                throw new ArgumentException("Line ID must be provided for update", nameof(line));
            }

            // Validate the line
            var validationResult = await ValidateLineAsync(line);
            if (!validationResult.IsValid)
            {
                throw new Exception($"Line validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Set audit information
            _auditService.SetUpdateAudit(line, userName);

            // Derived classes implement actual persistence
            return await UpdateLineInternalAsync(line);
        }

        /// <summary>
        /// Validates a line before create/update
        /// </summary>
        /// <param name="line">Line to validate</param>
        /// <returns>Validation result</returns>
        public virtual async Task<BalanceLineValidationResult> ValidateLineAsync(IBalanceAndIncomeLine line)
        {
            var result = new BalanceLineValidationResult();

            // Basic validation
            if (!((BalanceAndIncomeLineDto)line).Validate())
            {
                result.AddError("Basic validation failed");
            }

            // Check for unique visible index within same parent
            var siblings = await GetSiblingsAsync(line);
            if (siblings.Any(s => s.Id != line.Id && s.VisibleIndex == line.VisibleIndex))
            {
                result.AddWarning($"Another line with visible index {line.VisibleIndex} exists at the same level");
            }

            // Validate line type consistency
            if (!await ValidateLineTypeConsistencyAsync(line))
            {
                result.AddError("Line type is inconsistent with its parent");
            }

            return result;
        }

        #region Abstract Methods

        /// <summary>
        /// Creates a line in the underlying storage
        /// </summary>
        /// <param name="line">Line to create</param>
        /// <returns>Created line</returns>
        protected abstract Task<IBalanceAndIncomeLine> CreateLineInternalAsync(IBalanceAndIncomeLine line);

        /// <summary>
        /// Updates a line in the underlying storage
        /// </summary>
        /// <param name="line">Line to update</param>
        /// <returns>Updated line</returns>
        protected abstract Task<IBalanceAndIncomeLine> UpdateLineInternalAsync(IBalanceAndIncomeLine line);

        /// <summary>
        /// Deletes a line from the underlying storage
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <returns>True if successful</returns>
        public abstract Task<bool> DeleteLineAsync(Guid id);

        /// <summary>
        /// Moves a line in the underlying storage
        /// </summary>
        /// <param name="lineId">Line ID</param>
        /// <param name="parentId">New parent ID</param>
        /// <param name="position">Position within parent</param>
        /// <returns>Updated line</returns>
        public abstract Task<IBalanceAndIncomeLine> MoveLineAsync(Guid lineId, Guid? parentId, int position);

        /// <summary>
        /// Gets lines by type from underlying storage
        /// </summary>
        /// <param name="lineType">Line type</param>
        /// <returns>Lines of specified type</returns>
        public abstract Task<IEnumerable<IBalanceAndIncomeLine>> GetLinesByTypeAsync(BalanceIncomeLineType lineType);

        /// <summary>
        /// Gets child lines from underlying storage
        /// </summary>
        /// <param name="parentId">Parent ID</param>
        /// <returns>Child lines</returns>
        public abstract Task<IEnumerable<IBalanceAndIncomeLine>> GetChildLinesAsync(Guid parentId);

        /// <summary>
        /// Gets all lines in tree order from underlying storage
        /// </summary>
        /// <returns>All lines ordered by left index</returns>
        public abstract Task<IEnumerable<IBalanceAndIncomeLine>> GetAllLinesTreeOrderAsync();

        /// <summary>
        /// Gets count of accounts assigned to a line from underlying storage
        /// </summary>
        /// <param name="lineId">Line ID</param>
        /// <returns>Number of accounts</returns>
        public abstract Task<int> GetAssignedAccountsCountAsync(Guid lineId);

        /// <summary>
        /// Gets sibling lines from underlying storage
        /// </summary>
        /// <param name="line">Line to find siblings for</param>
        /// <returns>Sibling lines</returns>
        protected abstract Task<IEnumerable<IBalanceAndIncomeLine>> GetSiblingsAsync(IBalanceAndIncomeLine line);

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates that a line type is consistent with its parent
        /// </summary>
        /// <param name="line">Line to validate</param>
        /// <returns>True if consistent</returns>
        private async Task<bool> ValidateLineTypeConsistencyAsync(IBalanceAndIncomeLine line)
        {
            // Get parent line
            var allLines = await GetAllLinesTreeOrderAsync();
            var parent = allLines.FirstOrDefault(l => l.IsParentOf(line));

            if (parent == null)
            {
                // Root level lines must be headers
                return line.LineType == BalanceIncomeLineType.BaseHeader ||
                       line.LineType == BalanceIncomeLineType.BalanceHeader ||
                       line.LineType == BalanceIncomeLineType.IncomeHeader;
            }

            // Check consistency rules
            switch (parent.LineType)
            {
                case BalanceIncomeLineType.BaseHeader:
                    // Base header can only have balance or income headers as children
                    return line.LineType == BalanceIncomeLineType.BalanceHeader ||
                           line.LineType == BalanceIncomeLineType.IncomeHeader;

                case BalanceIncomeLineType.BalanceHeader:
                    // Balance header can only have balance lines as children
                    return line.LineType == BalanceIncomeLineType.BalanceLine;

                case BalanceIncomeLineType.IncomeHeader:
                    // Income header can only have income lines as children
                    return line.LineType == BalanceIncomeLineType.IncomeLine;

                case BalanceIncomeLineType.BalanceLine:
                    // Balance line can have other balance lines as children (for grouping)
                    return line.LineType == BalanceIncomeLineType.BalanceLine;

                case BalanceIncomeLineType.IncomeLine:
                    // Income line can have other income lines as children (for grouping)
                    return line.LineType == BalanceIncomeLineType.IncomeLine;

                default:
                    return false;
            }
        }

        #endregion
    }
}