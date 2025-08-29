using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.ErpSystem.Sequencers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.Sequencers
{
    /// <summary>
    /// XAF implementation of sequencer service using IObjectSpace and DevExpress XAF services
    /// </summary>
    public class XafSequencerService : ISequencerService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafSequencerService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafSequencerService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafSequencerService(IObjectSpace objectSpace, ILogger<XafSequencerService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Gets the next number in the sequence using XAF ObjectSpace
        /// </summary>
        /// <param name="sequenceCode">The unique code of the sequence</param>
        /// <returns>The next formatted sequence number</returns>
        public async Task<string> GetNextNumberAsync(string sequenceCode)
        {
            if (string.IsNullOrEmpty(sequenceCode))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequenceCode));

            try
            {
                _logger?.LogDebug("Getting next number for sequence: {SequenceCode}", sequenceCode);

                // Find the sequence by code using XAF criteria
                var sequence = _objectSpace.FindObject<Sequence>(CriteriaOperator.Parse("Code = ?", sequenceCode));
                if (sequence == null)
                    throw new InvalidOperationException($"Sequence with code {sequenceCode} not found");

                if (!sequence.IsActive)
                    throw new InvalidOperationException($"Sequence {sequenceCode} is not active");

                // Increment the current value
                sequence.CurrentValue += sequence.IncrementBy;
                sequence.UpdatedAt = DateTime.UtcNow;

                // Generate the formatted number
                string numberPart = sequence.CurrentValue.ToString().PadLeft(sequence.MinLength, '0');
                string formattedNumber = $"{sequence.Prefix ?? ""}{numberPart}{sequence.Suffix ?? ""}";

                // Commit changes to persist the updated sequence value
                _objectSpace.CommitChanges();

                _logger?.LogDebug("Generated sequence number: {FormattedNumber} for sequence: {SequenceCode}", 
                    formattedNumber, sequenceCode);

                return await Task.FromResult(formattedNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting next number for sequence: {SequenceCode}", sequenceCode);
                throw;
            }
        }

        /// <summary>
        /// Creates a new sequence using XAF ObjectSpace
        /// </summary>
        /// <param name="sequence">The sequence configuration DTO</param>
        /// <returns>The created sequence DTO</returns>
        public async Task<SequenceDto> CreateSequenceAsync(SequenceDto sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (string.IsNullOrEmpty(sequence.Code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequence));

            try
            {
                _logger?.LogDebug("Creating sequence with code: {SequenceCode}", sequence.Code);

                // Check if sequence already exists
                var existingSequence = _objectSpace.FindObject<Sequence>(CriteriaOperator.Parse("Code = ?", sequence.Code));
                if (existingSequence != null)
                    throw new InvalidOperationException($"Sequence with code {sequence.Code} already exists");

                // Create new XAF entity from DTO
                var xafSequence = CreateXafSequenceFromDto(sequence);

                // Commit changes to persist the new sequence
                _objectSpace.CommitChanges();

                // Convert back to DTO for return
                var resultDto = CreateDtoFromXafSequence(xafSequence);

                _logger?.LogInformation("Successfully created sequence: {SequenceCode} - {SequenceName}", 
                    resultDto.Code, resultDto.Name);

                return await Task.FromResult(resultDto);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating sequence: {SequenceCode}", sequence.Code);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing sequence using XAF ObjectSpace
        /// </summary>
        /// <param name="sequence">The sequence DTO to update</param>
        /// <returns>The updated sequence DTO</returns>
        public async Task<SequenceDto> UpdateSequenceAsync(SequenceDto sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            try
            {
                _logger?.LogDebug("Updating sequence with code: {SequenceCode}", sequence.Code);

                // Find existing sequence
                var existingSequence = _objectSpace.FindObject<Sequence>(CriteriaOperator.Parse("Code = ?", sequence.Code));
                if (existingSequence == null)
                    throw new InvalidOperationException($"Sequence with code {sequence.Code} not found");

                // Update the existing entity properties from DTO
                UpdateXafSequenceFromDto(existingSequence, sequence);

                // Commit changes to persist updates
                _objectSpace.CommitChanges();

                // Convert back to DTO for return
                var resultDto = CreateDtoFromXafSequence(existingSequence);

                _logger?.LogInformation("Successfully updated sequence: {SequenceCode} - {SequenceName}", 
                    resultDto.Code, resultDto.Name);

                return await Task.FromResult(resultDto);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating sequence: {SequenceCode}", sequence.Code);
                throw;
            }
        }

        /// <summary>
        /// Gets a sequence by its code using XAF ObjectSpace
        /// </summary>
        /// <param name="code">The sequence code</param>
        /// <returns>The sequence DTO if found, null otherwise</returns>
        public async Task<SequenceDto?> GetSequenceByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(code));

            try
            {
                _logger?.LogDebug("Getting sequence by code: {SequenceCode}", code);

                var sequence = _objectSpace.FindObject<Sequence>(CriteriaOperator.Parse("Code = ?", code));
                if (sequence == null)
                {
                    _logger?.LogDebug("Sequence not found: {SequenceCode}", code);
                    return await Task.FromResult<SequenceDto?>(null);
                }

                var resultDto = CreateDtoFromXafSequence(sequence);
                return await Task.FromResult<SequenceDto?>(resultDto);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting sequence by code: {SequenceCode}", code);
                throw;
            }
        }

        /// <summary>
        /// Gets all active sequences using XAF ObjectSpace
        /// </summary>
        /// <returns>Collection of active sequence DTOs</returns>
        public async Task<IEnumerable<SequenceDto>> GetActiveSequencesAsync()
        {
            try
            {
                _logger?.LogDebug("Getting all active sequences");

                var activeSequences = _objectSpace.GetObjects<Sequence>(CriteriaOperator.Parse("IsActive = true"));
                var resultDtos = activeSequences.Select(CreateDtoFromXafSequence).ToList();

                _logger?.LogDebug("Found {Count} active sequences", resultDtos.Count);

                return await Task.FromResult<IEnumerable<SequenceDto>>(resultDtos);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting active sequences");
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a new XAF Sequence entity from a SequenceDto
        /// </summary>
        /// <param name="dto">The SequenceDto to convert</param>
        /// <returns>New XAF Sequence entity</returns>
        private Sequence CreateXafSequenceFromDto(SequenceDto dto)
        {
            var sequence = _objectSpace.CreateObject<Sequence>();
            
            sequence.ID = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            sequence.Code = dto.Code;
            sequence.Name = dto.Name;
            sequence.CurrentValue = dto.CurrentNumber;
            sequence.Prefix = dto.Prefix;
            sequence.Suffix = dto.Suffix;
            sequence.MinLength = dto.PaddingLength;
            sequence.IsActive = dto.IsActive;
            sequence.IncrementBy = 1; // Default increment
            sequence.InsertedAt = DateTime.UtcNow;
            sequence.UpdatedAt = DateTime.UtcNow;

            return sequence;
        }

        /// <summary>
        /// Updates an existing XAF Sequence entity from a SequenceDto
        /// </summary>
        /// <param name="sequence">The XAF Sequence entity to update</param>
        /// <param name="dto">The SequenceDto with updated values</param>
        private void UpdateXafSequenceFromDto(Sequence sequence, SequenceDto dto)
        {
            sequence.Name = dto.Name;
            sequence.CurrentValue = dto.CurrentNumber;
            sequence.Prefix = dto.Prefix;
            sequence.Suffix = dto.Suffix;
            sequence.MinLength = dto.PaddingLength;
            sequence.IsActive = dto.IsActive;
            sequence.UpdatedAt = DateTime.UtcNow;
            // Note: Code is typically not updated to maintain referential integrity
        }

        /// <summary>
        /// Creates a SequenceDto from an XAF Sequence entity
        /// </summary>
        /// <param name="sequence">The XAF Sequence entity to convert</param>
        /// <returns>New SequenceDto with populated properties</returns>
        private SequenceDto CreateDtoFromXafSequence(Sequence sequence)
        {
            return new SequenceDto
            {
                Id = sequence.ID,
                Code = sequence.Code,
                Name = sequence.Name,
                CurrentNumber = (int)sequence.CurrentValue,
                Prefix = sequence.Prefix ?? string.Empty,
                Suffix = sequence.Suffix ?? string.Empty,
                PaddingLength = sequence.MinLength,
                PaddingChar = '0', // Fixed to '0' as per DTO default
                IsActive = sequence.IsActive,
                LastUsedDate = sequence.UpdatedAt
            };
        }

        #endregion
    }
}
