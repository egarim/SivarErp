using System;

namespace Sivar.Erp.ErpSystem.Sequencers
{
    public interface ISequence
    {
        string Code { get; set; }
        int CurrentNumber { get; set; }
 
        bool IsActive { get; set; }
        string Name { get; set; }
        char PaddingChar { get; set; }
        int PaddingLength { get; set; }
        string Prefix { get; set; }
        string Suffix { get; set; }
    }
}
