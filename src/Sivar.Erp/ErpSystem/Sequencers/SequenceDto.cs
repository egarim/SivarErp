using System;

namespace Sivar.Erp.System.Sequencers
{
    public class SequenceDto
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public required string Name { get; set; }
        public required string Prefix { get; set; }
        public required string Suffix { get; set; }
        public int CurrentNumber { get; set; }
        public int PaddingLength { get; set; } = 4;
        public char PaddingChar { get; set; } = '0';
        public bool IsActive { get; set; }
        public DateTime LastUsedDate { get; set; }
    }
}