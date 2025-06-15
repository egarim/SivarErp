using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.System.Sequencers;

namespace Sivar.Erp.Services.Sequencers
{
    public class SequencerService : ISequencerService
    {
        private readonly ISequenceRepository _repository;

        public SequencerService(ISequenceRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<string> GetNextNumberAsync(string sequenceCode)
        {
            if (string.IsNullOrEmpty(sequenceCode))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequenceCode));

            var sequence = await _repository.GetByCodeAsync(sequenceCode);
            if (sequence == null)
                throw new InvalidOperationException($"Sequence with code {sequenceCode} not found");

            if (!sequence.IsActive)
                throw new InvalidOperationException($"Sequence {sequenceCode} is not active");

            int nextNumber = await _repository.IncrementNumberAsync(sequenceCode);
            string numberPart = nextNumber.ToString().PadLeft(sequence.PaddingLength, sequence.PaddingChar);
            
            return $"{sequence.Prefix}{numberPart}{sequence.Suffix}";
        }

        public async Task<SequenceDto> CreateSequenceAsync(SequenceDto sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (string.IsNullOrEmpty(sequence.Code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequence));

            var existing = await _repository.GetByCodeAsync(sequence.Code);
            if (existing != null)
                throw new InvalidOperationException($"Sequence with code {sequence.Code} already exists");

            sequence.Id = Guid.NewGuid();
            sequence.CurrentNumber = 0;
            sequence.IsActive = true;
            sequence.LastUsedDate = DateTime.UtcNow;

            return await _repository.SaveAsync(sequence);
        }

        public async Task<SequenceDto> UpdateSequenceAsync(SequenceDto sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var existing = await _repository.GetByCodeAsync(sequence.Code);
            if (existing == null)
                throw new InvalidOperationException($"Sequence with code {sequence.Code} not found");

            return await _repository.SaveAsync(sequence);
        }

        public async Task<SequenceDto?> GetSequenceByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(code));

            return await _repository.GetByCodeAsync(code);
        }

        public async Task<IEnumerable<SequenceDto>> GetActiveSequencesAsync()
        {
            var sequences = await _repository.GetAllAsync();
            return sequences.Where(s => s.IsActive);
        }
    }
}