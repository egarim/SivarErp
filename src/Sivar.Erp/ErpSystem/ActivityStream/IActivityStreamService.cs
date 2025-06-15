using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sivar.Erp.ErpSystem.ActivityStream
{
    /// <summary>
    /// Service for managing activity records
    /// </summary>
    public interface IActivityStreamService
    {
        /// <summary>
        /// Records an activity in the activity stream
        /// </summary>
        /// <param name="activity">The activity to record</param>
        /// <returns>The created activity record</returns>
        Task<ActivityRecord> RecordActivityAsync(ActivityRecord activity);
        
        /// <summary>
        /// Creates and records a simple activity
        /// </summary>
        /// <param name="actor">Who performed the action</param>
        /// <param name="verb">What action was performed</param>
        /// <param name="target">What was acted upon</param>
        /// <param name="timeZoneId">Timezone where the activity occurred</param>
        /// <returns>The created activity record</returns>
        Task<ActivityRecord> RecordActivityAsync(
            IStreamObject actor, 
            string verb, 
            IStreamObject target,
            string timeZoneId);
        
        /// <summary>
        /// Gets the activity stream for a specific actor
        /// </summary>
        /// <param name="actorType">Type of actor</param>
        /// <param name="actorKey">Key of actor</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        Task<IEnumerable<ActivityRecord>> GetActorActivityStreamAsync(
            string actorType, 
            string actorKey, 
            int page = 1, 
            int pageSize = 20);
        
        /// <summary>
        /// Gets the activity stream for a specific target
        /// </summary>
        /// <param name="targetType">Type of target</param>
        /// <param name="targetKey">Key of target</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        Task<IEnumerable<ActivityRecord>> GetTargetActivityStreamAsync(
            string targetType, 
            string targetKey, 
            int page = 1, 
            int pageSize = 20);
        
        /// <summary>
        /// Gets a global activity stream
        /// </summary>
        /// <param name="onlyPublic">Whether to include only public activities</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        Task<IEnumerable<ActivityRecord>> GetGlobalActivityStreamAsync(
            bool onlyPublic = true,
            int page = 1, 
            int pageSize = 20);
        
        /// <summary>
        /// Search activities by various criteria
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="tags">Tags to filter by</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <param name="timeZoneId">Timezone ID for the date range</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        Task<IEnumerable<ActivityRecord>> SearchActivitiesAsync(
            string query = null,
            IEnumerable<string> tags = null,
            DateOnly? startDate = null, 
            DateOnly? endDate = null,
            string timeZoneId = null,
            int page = 1, 
            int pageSize = 20);
    }
}