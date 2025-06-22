using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.ErpSystem.Diagnostics
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
