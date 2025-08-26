namespace Sivar.Erp.Core.Domain.ValueObjects;

/// <summary>
/// Strongly typed Company ID value object
/// </summary>
public readonly struct CompanyId : IEquatable<CompanyId>
{
    public Guid Value { get; }

    public CompanyId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Company ID cannot be empty", nameof(value));
        
        Value = value;
    }

    public static CompanyId New() => new(Guid.NewGuid());
    
    public static CompanyId From(Guid value) => new(value);
    
    public static CompanyId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid Company ID format: {value}", nameof(value));
        
        return new CompanyId(guid);
    }

    public bool Equals(CompanyId other) => Value.Equals(other.Value);
    
    public override bool Equals(object? obj) => obj is CompanyId other && Equals(other);
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public override string ToString() => Value.ToString();

    // Implicit conversions
    public static implicit operator Guid(CompanyId companyId) => companyId.Value;
    public static implicit operator CompanyId(Guid value) => new(value);
    
    // Operators
    public static bool operator ==(CompanyId left, CompanyId right) => left.Equals(right);
    public static bool operator !=(CompanyId left, CompanyId right) => !left.Equals(right);
}

/// <summary>
/// Strongly typed Branch ID value object
/// </summary>
public readonly struct BranchId : IEquatable<BranchId>
{
    public Guid Value { get; }

    public BranchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Branch ID cannot be empty", nameof(value));
        
        Value = value;
    }

    public static BranchId New() => new(Guid.NewGuid());
    
    public static BranchId From(Guid value) => new(value);
    
    public static BranchId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid Branch ID format: {value}", nameof(value));
        
        return new BranchId(guid);
    }

    public bool Equals(BranchId other) => Value.Equals(other.Value);
    
    public override bool Equals(object? obj) => obj is BranchId other && Equals(other);
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public override string ToString() => Value.ToString();

    // Implicit conversions
    public static implicit operator Guid(BranchId branchId) => branchId.Value;
    public static implicit operator BranchId(Guid value) => new(value);
    
    // Operators
    public static bool operator ==(BranchId left, BranchId right) => left.Equals(right);
    public static bool operator !=(BranchId left, BranchId right) => !left.Equals(right);
}

/// <summary>
/// Strongly typed User ID value object (for Keycloak user IDs)
/// </summary>
public readonly struct UserId : IEquatable<UserId>
{
    public string Value { get; }

    public UserId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("User ID cannot be null or empty", nameof(value));
        
        Value = value.Trim();
    }

    public static UserId From(string value) => new(value);
    
    public bool Equals(UserId other) => Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    
    public override bool Equals(object? obj) => obj is UserId other && Equals(other);
    
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
    
    public override string ToString() => Value;

    // Implicit conversions
    public static implicit operator string(UserId userId) => userId.Value;
    public static implicit operator UserId(string value) => new(value);
    
    // Operators
    public static bool operator ==(UserId left, UserId right) => left.Equals(right);
    public static bool operator !=(UserId left, UserId right) => !left.Equals(right);
}
