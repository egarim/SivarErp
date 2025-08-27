using System;

namespace Sivar.Erp.Infrastructure.TimeService
{
    public interface IDateTimeZoneTrackable
    {
       TimeOnly Time { get; }
        string TimeZoneId { get; }
        DateOnly Date { get; set; }
    }
}