using System;
using System.Threading.Tasks;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.ActivityStream;

namespace Sivar.Erp.ErpSystem.Services
{
    /// <summary>
    /// Base class for services that provides common functionality related to options and activity recording
    /// </summary>
    public abstract class ServiceBase
    {
        /// <summary>
        /// Service for managing options
        /// </summary>
        protected readonly IOptionService OptionService;
        
        /// <summary>
        /// Service for recording activities
        /// </summary>
        protected readonly IActivityStreamService ActivityStreamService;
        
        /// <summary>
        /// Default timezone identifier for the service
        /// </summary>
        protected string DefaultTimeZoneId { get; set; } = "UTC";

        /// <summary>
        /// Initializes a new instance of the service base class
        /// </summary>
        /// <param name="optionService">The option service</param>
        /// <param name="activityStreamService">The activity stream service</param>
        protected ServiceBase(IOptionService optionService, IActivityStreamService activityStreamService)
        {
            OptionService = optionService ?? throw new ArgumentNullException(nameof(optionService));
            ActivityStreamService = activityStreamService ?? throw new ArgumentNullException(nameof(activityStreamService));
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
        
        /// <summary>
        /// Records an activity in the system activity stream
        /// </summary>
        /// <param name="actor">Who performed the action</param>
        /// <param name="verb">What action was performed</param>
        /// <param name="target">What was acted upon</param>
        /// <param name="timeZoneId">Optional timezone ID, defaults to service default</param>
        /// <returns>The recorded activity</returns>
        protected async Task<ActivityRecord> RecordActivityAsync(
            IStreamObject actor,
            string verb,
            IStreamObject target,
            string timeZoneId = null)
        {
            return await ActivityStreamService.RecordActivityAsync(
                actor,
                verb,
                target,
                timeZoneId ?? DefaultTimeZoneId);
        }
        
        /// <summary>
        /// Records a detailed activity in the system activity stream
        /// </summary>
        /// <param name="activity">Activity record with detailed information</param>
        /// <returns>The recorded activity</returns>
        protected async Task<ActivityRecord> RecordDetailedActivityAsync(ActivityRecord activity)
        {
            // Ensure the timezone is set
            if (string.IsNullOrEmpty(activity.TimeZoneId))
            {
                activity.TimeZoneId = DefaultTimeZoneId;
            }
            
            return await ActivityStreamService.RecordActivityAsync(activity);
        }
        
        /// <summary>
        /// Creates a new stream object for use in activity records
        /// </summary>
        /// <param name="objectType">Type of object</param>
        /// <param name="objectKey">Object key/identifier</param>
        /// <param name="displayName">Human-friendly name</param>
        /// <param name="displayImage">Optional URL to image</param>
        /// <returns>Stream object for activity recording</returns>
        protected IStreamObject CreateStreamObject(
            string objectType,
            string objectKey,
            string displayName,
            string displayImage = null)
        {
            return new StreamObject(objectType, objectKey, displayName, displayImage);
        }
        
        /// <summary>
        /// Creates a stream object representing a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="displayName">User's display name</param>
        /// <param name="avatarUrl">Optional URL to user's avatar</param>
        /// <returns>Stream object representing the user</returns>
        protected IStreamObject CreateUserStreamObject(string userId, string displayName, string avatarUrl = null)
        {
            return CreateStreamObject("User", userId, displayName, avatarUrl);
        }
        
        /// <summary>
        /// Creates a stream object representing the system
        /// </summary>
        /// <returns>Stream object representing the system</returns>
        protected IStreamObject CreateSystemStreamObject()
        {
            return CreateStreamObject("System", "system", "System", "/assets/icons/system.svg");
        }
    }
}