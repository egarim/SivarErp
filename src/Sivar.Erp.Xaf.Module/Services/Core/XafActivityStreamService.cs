using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.ActivityStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.Core
{
    /// <summary>
    /// XAF implementation of IActivityStreamService using DevExpress ObjectSpace
    /// </summary>
    public class XafActivityStreamService : IActivityStreamService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafActivityStreamService>? _logger;

        /// <summary>
        /// Initializes a new instance of XafActivityStreamService
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger</param>
        public XafActivityStreamService(IObjectSpace objectSpace, ILogger<XafActivityStreamService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Records an activity in the activity stream
        /// </summary>
        /// <param name="activity">The activity to record</param>
        /// <returns>The created activity record</returns>
        public Task<ActivityRecord> RecordActivityAsync(ActivityRecord activity)
        {
            try
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
                        try
                        {
                            // Convert UTC to the activity's timezone
                            now = TimeZoneInfo.ConvertTimeFromUtc(now, 
                                TimeZoneInfo.FindSystemTimeZoneById(activity.TimeZoneId));
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Invalid timezone {TimeZone}, using UTC", activity.TimeZoneId);
                            activity.TimeZoneId = "UTC";
                        }
                    }
                    else
                    {
                        activity.TimeZoneId = "UTC";
                    }

                    activity.Date = DateOnly.FromDateTime(now);
                    activity.Time = TimeOnly.FromDateTime(now);
                }

                // Ensure the description is set
                if (string.IsNullOrEmpty(activity.Description))
                {
                    activity.Description = GenerateDefaultDescription(activity);
                }

                // Create the activity record in XAF
                var activityRecord = _objectSpace.CreateObject<ActivityRecord>();
                CopyActivityProperties(activity, activityRecord);

                _objectSpace.CommitChanges();
                
                _logger?.LogInformation("Recorded activity: {Description}", activity.Description);
                return Task.FromResult(activityRecord);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording activity: {Description}", activity.Description);
                throw;
            }
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
            try
            {
                var skip = (page - 1) * pageSize;
                
                var activities = _objectSpace.GetObjects<ActivityRecord>()
                    .Where(a => a.Actor != null && 
                               a.Actor.ObjectType == actorType && 
                               a.Actor.ObjectKey == actorKey)
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult<IEnumerable<ActivityRecord>>(activities);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting actor activity stream: {ActorType}/{ActorKey}", actorType, actorKey);
                return Task.FromResult<IEnumerable<ActivityRecord>>(new List<ActivityRecord>());
            }
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
            try
            {
                var skip = (page - 1) * pageSize;
                
                var activities = _objectSpace.GetObjects<ActivityRecord>()
                    .Where(a => a.Target != null && 
                               a.Target.ObjectType == targetType && 
                               a.Target.ObjectKey == targetKey)
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult<IEnumerable<ActivityRecord>>(activities);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting target activity stream: {TargetType}/{TargetKey}", targetType, targetKey);
                return Task.FromResult<IEnumerable<ActivityRecord>>(new List<ActivityRecord>());
            }
        }

        /// <summary>
        /// Gets the global activity stream
        /// </summary>
        /// <param name="publicOnly">Whether to include only public activities</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public Task<IEnumerable<ActivityRecord>> GetGlobalActivityStreamAsync(
            bool publicOnly = true, 
            int page = 1, 
            int pageSize = 20)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                
                var query = _objectSpace.GetObjects<ActivityRecord>().AsQueryable();
                
                if (publicOnly)
                {
                    query = query.Where(a => a.IsPublic);
                }

                var activities = query
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult<IEnumerable<ActivityRecord>>(activities);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting global activity stream");
                return Task.FromResult<IEnumerable<ActivityRecord>>(new List<ActivityRecord>());
            }
        }

        /// <summary>
        /// Searches for activities based on various criteria
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="tags">Tags to filter by</param>
        /// <param name="startDate">Start date range</param>
        /// <param name="endDate">End date range</param>
        /// <param name="timeZoneId">Timezone ID for the date range</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of activity records</returns>
        public Task<IEnumerable<ActivityRecord>> SearchActivitiesAsync(
            string? query = null,
            IEnumerable<string>? tags = null,
            DateOnly? startDate = null, 
            DateOnly? endDate = null,
            string? timeZoneId = null,
            int page = 1, 
            int pageSize = 20)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var queryable = _objectSpace.GetObjects<ActivityRecord>().AsQueryable();

                // Apply search query filter
                if (!string.IsNullOrEmpty(query))
                {
                    queryable = queryable.Where(a => 
                        a.Description.Contains(query) || 
                        a.Verb.Contains(query) ||
                        (a.Details != null && a.Details.Contains(query)));
                }

                // Apply date range filter
                if (startDate.HasValue)
                {
                    queryable = queryable.Where(a => a.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    queryable = queryable.Where(a => a.Date <= endDate.Value);
                }

                // Apply timezone filter
                if (!string.IsNullOrEmpty(timeZoneId))
                {
                    queryable = queryable.Where(a => a.TimeZoneId == timeZoneId);
                }

                // Apply tags filter
                if (tags != null && tags.Any())
                {
                    var tagList = tags.ToList();
                    queryable = queryable.Where(a => a.Tags.Any(t => tagList.Contains(t)));
                }

                var activities = queryable
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult<IEnumerable<ActivityRecord>>(activities);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching activities");
                return Task.FromResult<IEnumerable<ActivityRecord>>(new List<ActivityRecord>());
            }
        }

        /// <summary>
        /// Generates a default description for an activity
        /// </summary>
        private string GenerateDefaultDescription(ActivityRecord activity)
        {
            var actorName = activity.Actor?.DisplayName ?? "Unknown";
            var targetName = activity.Target?.DisplayName ?? "Unknown";
            var objectName = activity.Object?.DisplayName;

            if (!string.IsNullOrEmpty(objectName))
            {
                return $"{actorName} {activity.Verb} {targetName} with {objectName}";
            }
            else
            {
                return $"{actorName} {activity.Verb} {targetName}";
            }
        }

        /// <summary>
        /// Copies properties from source activity to target activity
        /// </summary>
        private static void CopyActivityProperties(ActivityRecord source, ActivityRecord target)
        {
            target.Id = source.Id;
            target.Actor = source.Actor;
            target.Verb = source.Verb;
            target.Target = source.Target;
            target.Object = source.Object;
            target.Description = source.Description;
            target.Details = source.Details;
            target.Date = source.Date;
            target.Time = source.Time;
            target.TimeZoneId = source.TimeZoneId;
            target.IsPublic = source.IsPublic;
            target.ContextUrl = source.ContextUrl;
            
            // Copy tags
            target.Tags.Clear();
            foreach (var tag in source.Tags)
            {
                target.Tags.Add(tag);
            }
        }
    }
}
