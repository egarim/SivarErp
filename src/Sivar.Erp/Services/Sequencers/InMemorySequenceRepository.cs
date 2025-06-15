using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Sequencers;

namespace Sivar.Erp.Services.Sequencers
{
    public class InMemorySequenceRepository : ISequenceRepository
    {
        private readonly ConcurrentDictionary<string, SequenceDto> _sequences = new();

        public Task<SequenceDto?> GetByCodeAsync(string code)
        {
            return Task.FromResult(_sequences.TryGetValue(code, out var sequence) ? sequence : null);
        }

        public Task<IEnumerable<SequenceDto>> GetAllAsync()
        {
            return Task.FromResult(_sequences.Values.AsEnumerable());
        }

        public Task<SequenceDto> SaveAsync(SequenceDto sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (string.IsNullOrEmpty(sequence.Code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequence));

            sequence.LastUsedDate = DateTime.UtcNow;
            _sequences[sequence.Code] = sequence;
            
            return Task.FromResult(sequence);
        }

        public Task<int> IncrementNumberAsync(string code)
        {
            if (!_sequences.TryGetValue(code, out var sequence))
                throw new InvalidOperationException($"Sequence with code {code} not found");

            sequence.CurrentNumber++;
            sequence.LastUsedDate = DateTime.UtcNow;
            
            return Task.FromResult(sequence.CurrentNumber);
        }
    }
}