using System;

namespace Sivar.Erp.Infrastructure.Sequencers
{
    public interface ISequenceDto
    {
        string Code { get; set; }
        int CurrentNumber { get; set; }
        Guid Id { get; set; }
        bool IsActive { get; set; }
        DateTime LastUsedDate { get; set; }
        string Name { get; set; }
        char PaddingChar { get; set; }
        int PaddingLength { get; set; }
        string Prefix { get; set; }
        string Suffix { get; set; }
    }
}
