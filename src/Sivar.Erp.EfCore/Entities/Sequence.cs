using Sivar.Erp.ErpSystem.Sequencers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Sequences
/// </summary>
[Table("Sequences")]
public class Sequence : BaseEntity, ISequence
{
    public string Code { get; set; }
    public int CurrentNumber { get; set; }

    public bool IsActive { get; set; }
    public string Name { get; set; }
    public char PaddingChar { get; set; }
    public int PaddingLength { get; set; }
    public string Prefix { get; set; }
    public string Suffix { get; set; }
}
