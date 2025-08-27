namespace Sivar.Erp.Core.Interfaces
{
    /// <summary>
    /// Interface for business entities (customers, suppliers, etc.)
    /// </summary>
    public interface IBusinessEntity
    {
        string Code { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string State { get; set; }
        string ZipCode { get; set; }
        string Country { get; set; }
        string PhoneNumber { get; set; }
        string Email { get; set; }
    }
}
