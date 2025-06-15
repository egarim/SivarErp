using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.ErpSystem.DateTimeZone;
using Sivar.Erp.Services;

namespace Sivar.Erp.ErpSystem.ActivityStream
{
    /// <summary>
    /// Implementation of the activity stream service using IObjectDb
    /// </summary>
    public class ActivityStreamService : IActivityStreamService
    {
        private readonly IDateTimeZoneService _dateTimeZoneService;
        private readonly IObjectDb _objectDb;
        
        /// <summary>
        /// Initializes a new instance of the activity stream service
        /// </summary>
        /// <param name="dateTimeZoneService">Service for handling date/time with timezone</param>
        /// <param name="objectDb">Object database for activity records</param>
        public ActivityStreamService(
            IDateTimeZoneService dateTimeZoneService,
            IObjectDb objectDb)
        {
            _dateTimeZoneService = dateTimeZoneService;
            _objectDb = objectDb;
        }
        
        /// <summary>
        /// Records an activity in the activity stream
        /// </summary>
        /// <param name="activity">The activity to record</param>
        /// <returns>The created activity record</returns>
        public Task<ActivityRecord> RecordActivityAsync(ActivityRecord activity)
        {
            // Generate ID if not provided
            if (activity.Id == Guid.Empty)
            {
                activity.Id = Guid.NewGuid();
            }
            
            // Set current date/time if not provided
            if (activity.Date == default && activity.Time == default)
            {
                DateTime now = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(activity.TimeZoneId))
                {
                    // Convert UTC to the activity's timezone
                    now = TimeZoneInfo.ConvertTimeFromUtc(now, 
                        TimeZoneInfo.FindSystemTimeZoneById(activity.TimeZoneId));
                }
                activity.Date = DateOnly.FromDateTime(now);
                activity.Time = TimeOnly.FromDateTime(now);
            }
            
            // Ensure the description is set
            if (string.IsNullOrEmpty(activity.Description))
            {
                activity.Description = GenerateDefaultDescription(activity);
            }
            
            // Store activity record
            _objectDb.ActivityRecords.Add(activity);
            
            return Task.FromResult(activity);
        }
        
        /// <summary>
        /// Creates and records a simple activity
        /// </summary>
        /// <param name="actor">Who performed the action</param>
        /// <param name="verb">What action was performed</param>
        /// <param name="target">What was acted upon</param>
        /// <param name="timeZoneId">Timezone where the activity occurred</param>
        /// <returns>The created activity record</returns>
        public async Task<ActivityRecord> RecordActivityAsync(
            IStreamObject actor, 
            string verb, 
            IStreamObject target,
            string timeZoneId)
        {
            var activity = new ActivityRecord
            {
                Actor = actor,
                Verb = verb,
                Target = target,
                TimeZoneId = timeZoneId
            };
            
            return await RecordActivityAsync(activity);
        }
        
        /// <summary>
        /// Gets the activity stream for a specific actor
        /// </summary>
        /// <param name="actorType">Type of actor</param>
        /// <param name="actorKey">Key of actor</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public Task<IEnumerable<ActivityRecord>> GetActorActivityStreamAsync(
            string actorType, 
            string actorKey, 
            int page = 1, 
            int pageSize = 20)
        {
            var result = _objectDb.ActivityRecords
                .Where(a => a.Actor.ObjectType == actorType && a.Actor.ObjectKey == actorKey)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            return Task.FromResult(result.AsEnumerable());
        }
        
        /// <summary>
        /// Gets the activity stream for a specific target
        /// </summary>
        /// <param name="targetType">Type of target</param>
        /// <param name="targetKey">Key of target</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public Task<IEnumerable<ActivityRecord>> GetTargetActivityStreamAsync(
            string targetType, 
            string targetKey, 
            int page = 1, 
            int pageSize = 20)
        {
            var result = _objectDb.ActivityRecords
                .Where(a => a.Target.ObjectType == targetType && a.Target.ObjectKey == targetKey)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            return Task.FromResult(result.AsEnumerable());
        }
        
        /// <summary>
        /// Gets a global activity stream
        /// </summary>
        /// <param name="onlyPublic">Whether to include only public activities</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public Task<IEnumerable<ActivityRecord>> GetGlobalActivityStreamAsync(
            bool onlyPublic = true,
            int page = 1, 
            int pageSize = 20)
        {
            var query = _objectDb.ActivityRecords.AsEnumerable();
            
            if (onlyPublic)
            {
                query = query.Where(a => a.IsPublic);
            }
            
            var result = query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            return Task.FromResult(result.AsEnumerable());
        }
        
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
        public Task<IEnumerable<ActivityRecord>> SearchActivitiesAsync(
            string query = null,
            IEnumerable<string> tags = null,
            DateOnly? startDate = null, 
            DateOnly? endDate = null,
            string timeZoneId = null,
            int page = 1, 
            int pageSize = 20)
        {
            var result = _objectDb.ActivityRecords.AsEnumerable();
            
            // Filter by query text
            if (!string.IsNullOrEmpty(query))
            {
                result = result.Where(a => 
                    a.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (a.Details?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                    a.Actor.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    a.Target.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (a.Object?.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                );
            }
            
            // Filter by tags
            if (tags != null && tags.Any())
            {
                result = result.Where(a => tags.Any(tag => a.Tags.Contains(tag)));
            }
            
            // Filter by start date
            if (startDate.HasValue)
            {
                result = result.Where(a => a.Date >= startDate.Value);
            }
            
            // Filter by end date
            if (endDate.HasValue)
            {
                result = result.Where(a => a.Date <= endDate.Value);
            }
            
            // Apply paging and ordering
            result = result
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
                
            return Task.FromResult(result);
        }
        
        /// <summary>
        /// Generates a default description for an activity
        /// </summary>
        private string GenerateDefaultDescription(ActivityRecord activity)
        {
            if (activity.Actor == null || activity.Target == null || string.IsNullOrEmpty(activity.Verb))
            {
                return string.Empty;
            }
            
            string description = $"{activity.Actor.DisplayName} {activity.Verb} {activity.Target.DisplayName}";
            
            if (activity.Object != null)
            {
                description += $" to {activity.Object.DisplayName}";
            }
            
            return description;
        }
    }
}