using System;

namespace Sivar.Erp.Infrastructure.Diagnostics
{
    [Flags]
    public enum PerformanceLogMode
    {
        None = 0,
        Narrative = 1,
        ExecutionTime = 2,
        Memory = 4,
        All = Narrative | ExecutionTime | Memory
    }
}