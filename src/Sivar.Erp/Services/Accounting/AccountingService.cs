using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Services;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.Accounting
{
    public class AccountingService : ServiceBase
    {
        protected IFiscalPeriodService FiscalPeriodService;
        IAccountBalanceCalculator AccountBalanceCalculator;
        public AccountingService(IOptionService optionService, IActivityStreamService activityStreamService, IDateTimeZoneService dateTimeZoneService,
            IFiscalPeriodService FiscalPeriodService, IAccountBalanceCalculator AccountBalanceCalculator) : base(optionService, activityStreamService, dateTimeZoneService)
        {
        }
    }
}
