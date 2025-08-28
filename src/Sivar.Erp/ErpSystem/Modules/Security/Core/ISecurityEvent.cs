namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public interface ISecurityEvent
    {
        string Action { get; set; }
        Dictionary<string, object> AdditionalData { get; set; }
        string Details { get; set; }
        string Id { get; set; }
        string? IpAddress { get; set; }
        string Result { get; set; }
        DateTime Timestamp { get; set; }
        string? UserAgent { get; set; }
        string? UserId { get; set; }
        string? UserName { get; set; }
    }
}
