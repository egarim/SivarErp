using System;

namespace Sivar.Erp.ErpSystem.Sequencers
{
    public class SequenceDto
    {
        public Guid Id { get; set; }
        public  string Code { get; set; }
        public  string Name { get; set; }
        public  string Prefix { get; set; }
        public  string Suffix { get; set; }
        public int CurrentNumber { get; set; }
        public int PaddingLength { get; set; } = 4;
        public char PaddingChar { get; set; } = '0';
        public bool IsActive { get; set; }
        public DateTime LastUsedDate { get; set; }
    }
}