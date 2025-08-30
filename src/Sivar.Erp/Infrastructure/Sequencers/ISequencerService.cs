using Sivar.Erp.ErpSystem.Sequencers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.Infrastructure.Sequencers
{
    public interface ISequencerService
    {
        /// <summary>
        /// Gets the next number in the sequence
        /// </summary>
        /// <param name="sequenceCode">The unique code of the sequence</param>
        /// <returns>The next formatted sequence number</returns>
        Task<string> GetNextNumberAsync(string sequenceCode);

        /// <summary>
        /// Creates a new sequence
        /// </summary>
        /// <param name="sequence">The sequence configuration</param>
        /// <returns>The created sequence</returns>
        Task<ISequence> CreateSequenceAsync(ISequence sequence);

        /// <summary>
        /// Updates an existing sequence
        /// </summary>
        /// <param name="sequence">The sequence to update</param>
        /// <returns>The updated sequence</returns>
        Task<ISequence> UpdateSequenceAsync(ISequence sequence);

        /// <summary>
        /// Gets a sequence by its code
        /// </summary>
        /// <param name="code">The sequence code</param>
        /// <returns>The sequence if found, null otherwise</returns>
        Task<ISequence?> GetSequenceByCodeAsync(string code);

        /// <summary>
        /// Gets all active sequences
        /// </summary>
        /// <returns>List of active sequences</returns>
        Task<IEnumerable<ISequence>> GetActiveSequencesAsync();
    }
}
