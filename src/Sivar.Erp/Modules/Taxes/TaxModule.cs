using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Services;
using Sivar.Erp.ErpSystem.TimeService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.Taxes
{
    public class TaxModule : ErpModuleBase
    {
        public TaxModule(IOptionService optionService, IActivityStreamService activityStreamService, IDateTimeZoneService dateTimeZoneService, ISequencerService sequencerService) : base(optionService, activityStreamService, dateTimeZoneService, sequencerService)
        {
        }

        public override void RegisterSequence(IEnumerable<SequenceDto> sequenceDtos)
        {
            throw new NotImplementedException();
        }
    }
}
