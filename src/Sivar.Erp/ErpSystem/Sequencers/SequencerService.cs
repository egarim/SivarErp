using Sivar.Erp.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.ErpSystem.Sequencers
{
    public class SequencerService : ISequencerService
    {
        private readonly IObjectDb objectDb;
        
        public SequencerService(IObjectDb objectDb)
        {
            this.objectDb = objectDb;
        }

        public async Task<string> GetNextNumberAsync(string sequenceCode)
        {
            if (string.IsNullOrEmpty(sequenceCode))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequenceCode));

            var sequence = objectDb.Sequences.FirstOrDefault(s => s.Code == sequenceCode);
            if (sequence == null)
                throw new InvalidOperationException($"Sequence with code {sequenceCode} not found");

            if (!sequence.IsActive)
                throw new InvalidOperationException($"Sequence {sequenceCode} is not active");

            sequence.CurrentNumber++;
     
            int nextNumber = sequence.CurrentNumber;
            
            string numberPart = nextNumber.ToString().PadLeft(sequence.PaddingLength, sequence.PaddingChar);
            
            return await Task.FromResult($"{sequence.Prefix}{numberPart}{sequence.Suffix}");
        }

        public async Task<ISequence> CreateSequenceAsync(ISequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (string.IsNullOrEmpty(sequence.Code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(sequence));

            var existing = objectDb.Sequences.FirstOrDefault(s => s.Code == sequence.Code);
            if (existing != null)
                throw new InvalidOperationException($"Sequence with code {sequence.Code} already exists");

           
            sequence.CurrentNumber = 0;
            sequence.IsActive = true;
         

            objectDb.Sequences.Add(sequence);
            return await Task.FromResult(sequence);
        }

        public async Task<ISequence> UpdateSequenceAsync(ISequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var existing = objectDb.Sequences.FirstOrDefault(s => s.Code == sequence.Code);
            if (existing == null)
                throw new InvalidOperationException($"Sequence with code {sequence.Code} not found");

            // Remove the existing sequence
            var existingIndex = objectDb.Sequences.IndexOf(existing);
            if (existingIndex >= 0)
            {
                objectDb.Sequences.RemoveAt(existingIndex);
            }
            
            // Add the updated sequence
            objectDb.Sequences.Add(sequence);
            
            return await Task.FromResult(sequence);
        }

        public async Task<ISequence?> GetSequenceByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Sequence code cannot be empty", nameof(code));

            var sequence = objectDb.Sequences.FirstOrDefault(s => s.Code == code);
            return await Task.FromResult(sequence);
        }

        public async Task<IEnumerable<ISequence>> GetActiveSequencesAsync()
        {
            return await Task.FromResult(objectDb.Sequences.Where(s => s.IsActive));
        }
    }
}
