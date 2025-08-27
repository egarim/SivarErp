using System.ComponentModel.DataAnnotations;

namespace Sivar.Erp.Core.Shared.DTOs;

/// <summary>
/// Base class for all DTOs
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base class for create DTOs
/// </summary>
public abstract class CreateBaseDto
{
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Base class for update DTOs
/// </summary>
public abstract class UpdateBaseDto
{
    public string? UpdatedBy { get; set; }
}
