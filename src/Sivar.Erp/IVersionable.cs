namespace Sivar.Erp
{
    /// <summary>
    /// Interface for entities that implement versioning
    /// </summary>
    public interface IVersionable : IEntity
    {
        /// <summary>
        /// The date from which this version is effective
        /// </summary>
        DateOnly EffectiveDate { get; set; }
    }
}
