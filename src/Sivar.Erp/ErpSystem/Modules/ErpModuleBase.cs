using System;
using System.Threading.Tasks;
using Sivar.Erp.ErpSystem.Options;

using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.ErpSystem.Sequencers;

namespace Sivar.Erp.ErpSystem.Modules
{
    /// <summary>
    /// Base class for services that provides common functionality related to options and activity recording
    /// </summary>
    public abstract class ErpModuleBase
    {
        /// <summary>
        /// Service for managing options
        /// </summary>
        protected readonly IOptionService OptionService;

        protected ISequencerService sequencerService;

        protected readonly IDateTimeZoneService DateTimeZoneService;

    
        
        /// <summary>
        /// Default timezone identifier for the service
        /// </summary>
        protected string DefaultTimeZoneId { get; set; } = "UTC";

        /// <summary>
        /// Initializes a new instance of the service base class
        /// </summary>
        /// <param name="optionService">The option service</param>
        /// <param name="activityStreamService">The activity stream service</param>
        protected ErpModuleBase(IOptionService optionService, IDateTimeZoneService dateTimeZoneService, ISequencerService sequencerService)
        {
            OptionService = optionService ?? throw new ArgumentNullException(nameof(optionService));
          
            DateTimeZoneService= dateTimeZoneService ?? throw new ArgumentNullException(nameof(DateTimeZoneService));
            this.sequencerService = sequencerService;
        }
        
        /// <summary>
        /// Gets an option value from the specified module
        /// </summary>
        /// <param name="optionCode">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <param name="defaultValue">Default value if option not found</param>
        /// <returns>Option value or default</returns>
        protected async Task<string> GetOptionValueAsync(string optionCode, string moduleName, string defaultValue = null)
        {
            var value = await OptionService.GetCurrentOptionValueAsync(optionCode, moduleName);
            return value ?? defaultValue;
        }

        public abstract void RegisterSequence(IEnumerable<ISequence> sequenceDtos);
     

        /// <summary>
        /// Sets an option value for the specified module
        /// </summary>
        /// <param name="optionCode">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <param name="value">Value to set</param>
        /// <param name="userName">User making the change</param>
        /// <returns>True if successful</returns>
        protected async Task<bool> SetOptionValueAsync(string optionCode, string moduleName, string value, string userName = null)
        {
            return await OptionService.SetOptionValueAsync(
                optionCode, 
                moduleName, 
                value, 
                DateTime.UtcNow, 
                null, 
                userName);
        }
        
       
        
      
    }
}
