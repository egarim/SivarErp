using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Services.DateTimeZone;

namespace Sivar.Erp.ErpSystem.ActivityStream
{
    /// <summary>
    /// Implementation of the activity stream service
    /// </summary>
    public class ActivityStreamService : IActivityStreamService
    {
        private readonly IDateTimeZoneService _dateTimeZoneService;
        private readonly IRepository<ActivityRecord> _repository;
        
        /// <summary>
        /// Initializes a new instance of the activity stream service
        /// </summary>
        /// <param name="dateTimeZoneService">Service for handling date/time with timezone</param>
        /// <param name="repository">Repository for activity records</param>
        public ActivityStreamService(
            IDateTimeZoneService dateTimeZoneService,
            IRepository<ActivityRecord> repository)
        {
            _dateTimeZoneService = dateTimeZoneService;
            _repository = repository;
        }
        
        /// <summary>
        /// Records an activity in the activity stream
        /// </summary>
        /// <param name="activity">The activity to record</param>
        /// <returns>The created activity record</returns>
        public async Task<ActivityRecord> RecordActivityAsync(ActivityRecord activity)
        {
            // Generate ID if not provided
            if (activity.Id == Guid.Empty)
            {
                activity.Id = Guid.NewGuid();
            }
            
            // Set current date/time if not provided
            if (activity.Date == default && activity.Time == default)
            {
                var now = _dateTimeZoneService.GetCurrentDateTime(activity.TimeZoneId);
                activity.Date = DateOnly.FromDateTime(now);
                activity.Time = TimeOnly.FromDateTime(now);
            }
            
            // Ensure the description is set
            if (string.IsNullOrEmpty(activity.Description))
            {
                activity.Description = GenerateDefaultDescription(activity);
            }
            
            // Store activity record
            await _repository.AddAsync(activity);
            
            return activity;
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
        public async Task<IEnumerable<ActivityRecord>> GetActorActivityStreamAsync(
            string actorType, 
            string actorKey, 
            int page = 1, 
            int pageSize = 20)
        {
            return await _repository.QueryAsync(
                a => a.Actor.ObjectType == actorType && a.Actor.ObjectKey == actorKey,
                page,
                pageSize,
                a => a.Date.Descending().ThenBy(x => x.Time.Descending()));
        }
        
        /// <summary>
        /// Gets the activity stream for a specific target
        /// </summary>
        /// <param name="targetType">Type of target</param>
        /// <param name="targetKey">Key of target</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public async Task<IEnumerable<ActivityRecord>> GetTargetActivityStreamAsync(
            string targetType, 
            string targetKey, 
            int page = 1, 
            int pageSize = 20)
        {
            return await _repository.QueryAsync(
                a => a.Target.ObjectType == targetType && a.Target.ObjectKey == targetKey,
                page,
                pageSize,
                a => a.Date.Descending().ThenBy(x => x.Time.Descending()));
        }
        
        /// <summary>
        /// Gets a global activity stream
        /// </summary>
        /// <param name="onlyPublic">Whether to include only public activities</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public async Task<IEnumerable<ActivityRecord>> GetGlobalActivityStreamAsync(
            bool onlyPublic = true,
            int page = 1, 
            int pageSize = 20)
        {
            var query = onlyPublic ? a => a.IsPublic : null;
            return await _repository.QueryAsync(
                query,
                page,
                pageSize,
                a => a.Date.Descending().ThenBy(x => x.Time.Descending()));
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
        public async Task<IEnumerable<ActivityRecord>> SearchActivitiesAsync(
            string query = null,
            IEnumerable<string> tags = null,
            DateOnly? startDate = null, 
            DateOnly? endDate = null,
            string timeZoneId = null,
            int page = 1, 
            int pageSize = 20)
        {
            // Build query based on provided parameters
            Func<ActivityRecord, bool> predicate = a => true;
            
            if (!string.IsNullOrEmpty(query))
            {
                predicate = predicate.And(a => 
                    a.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    a.Details?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                    a.Actor.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    a.Target.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    a.Object?.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
                );
            }
            
            if (tags != null && tags.Any())
            {
                predicate = predicate.And(a => tags.Any(tag => a.Tags.Contains(tag)));
            }
            
            if (startDate.HasValue)
            {
                predicate = predicate.And(a => a.Date >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                predicate = predicate.And(a => a.Date <= endDate.Value);
            }
            
            return await _repository.QueryAsync(
                predicate,
                page,
                pageSize,
                a => a.Date.Descending().ThenBy(x => x.Time.Descending()));
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
    
    /// <summary>
    /// Extension methods for building predicates
    /// </summary>
    internal static class PredicateBuilder
    {
        public static Func<T, bool> And<T>(this Func<T, bool> first, Func<T, bool> second)
        {
            return x => first(x) && second(x);
        }
        
        public static Func<T, bool> Or<T>(this Func<T, bool> first, Func<T, bool> second)
        {
            return x => first(x) || second(x);
        }
    }
}