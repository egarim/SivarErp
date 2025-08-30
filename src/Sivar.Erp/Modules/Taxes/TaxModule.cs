
using Sivar.Erp.ErpSystem.Modules;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.TimeService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Modules.Taxes
{
    public class TaxModule : ErpModuleBase
    {
        public TaxModule(IOptionService optionService, IDateTimeZoneService dateTimeZoneService, ISequencerService sequencerService) : base(optionService, dateTimeZoneService, sequencerService)
        {
        }

        public override void RegisterSequence(IEnumerable<ISequence> sequenceDtos)
        {
            throw new NotImplementedException();
        }
    }
}
