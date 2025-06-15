using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.System.Sequencers;

namespace Sivar.Erp.Services.Sequencers
{
    public interface ISequenceRepository
    {
        Task<SequenceDto?> GetByCodeAsync(string code);
        Task<IEnumerable<SequenceDto>> GetAllAsync();
        Task<SequenceDto> SaveAsync(SequenceDto sequence);
        Task<int> IncrementNumberAsync(string code);
    }
}