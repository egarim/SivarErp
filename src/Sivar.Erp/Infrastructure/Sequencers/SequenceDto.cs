using Sivar.Erp.ErpSystem.Sequencers;
using System;

namespace Sivar.Erp.Infrastructure.Sequencers
{

    public class SequenceDto : ISequence
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int CurrentNumber { get; set; }
        public int PaddingLength { get; set; } = 4;
        public char PaddingChar { get; set; } = '0';
        public bool IsActive { get; set; }

    }
}
