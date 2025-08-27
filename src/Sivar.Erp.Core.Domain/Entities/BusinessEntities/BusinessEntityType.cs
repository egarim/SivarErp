namespace Sivar.Erp.Core.Domain.Entities.BusinessEntities;

/// <summary>
/// Types of business entities in the ERP system
/// </summary>
public enum BusinessEntityType
{
    /// <summary>
    /// Customer who purchases products/services
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Supplier who provides products/services
    /// </summary>
    Supplier = 2,

    /// <summary>
    /// Vendor for procurement
    /// </summary>
    Vendor = 3,

    /// <summary>
    /// Employee of the company
    /// </summary>
    Employee = 4,

    /// <summary>
    /// Partner or joint venture entity
    /// </summary>
    Partner = 5,

    /// <summary>
    /// Contractor or freelancer
    /// </summary>
    Contractor = 6,

    /// <summary>
    /// Government agency or institution
    /// </summary>
    GovernmentAgency = 7,

    /// <summary>
    /// Financial institution (bank, credit union, etc.)
    /// </summary>
    FinancialInstitution = 8,

    /// <summary>
    /// Insurance company
    /// </summary>
    InsuranceCompany = 9,

    /// <summary>
    /// Other type of business entity
    /// </summary>
    Other = 10
}
