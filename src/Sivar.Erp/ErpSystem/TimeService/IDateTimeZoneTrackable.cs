using System;

namespace Sivar.Erp.ErpSystem.TimeService
{
    public interface IDateTimeZoneTrackable
    {
       TimeOnly Time { get; }
        string TimeZoneId { get; }
        DateOnly Date { get; set; }
    }
}