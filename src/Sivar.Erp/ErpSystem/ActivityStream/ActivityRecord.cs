using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sivar.Erp.ErpSystem.ActivityStream
{
    /// <summary>
    /// Represents a single activity in the activity stream following the Actor-Verb-Target pattern
    /// </summary>
    public class ActivityRecord : IDateTimeZoneTrackable
    {
        /// <summary>
        /// Unique identifier for the activity
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The entity that performed the action (typically a user)
        /// </summary>
        public IStreamObject Actor { get; set; }
        
        /// <summary>
        /// The action that was performed (e.g., "created", "modified", "deleted", "approved")
        /// </summary>
        public string Verb { get; set; }
        
        /// <summary>
        /// The entity that was acted upon
        /// </summary>
        public IStreamObject Target { get; set; }
        
        /// <summary>
        /// Optional additional object involved in the activity (e.g., "moved [Target] to [Object]")
        /// </summary>
        public IStreamObject Object { get; set; }
        
        /// <summary>
        /// Human-readable description of the activity
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Detailed information about the activity (could be JSON or formatted text)
        /// </summary>
        public string Details { get; set; }
        
        /// <summary>
        /// The date component when the activity occurred
        /// </summary>
        public DateOnly Date { get; set; }
        
        /// <summary>
        /// The time component when the activity occurred
        /// </summary>
        public TimeOnly Time { get; set; }
        
        /// <summary>
        /// The timezone identifier where the activity occurred
        /// </summary>
        public string TimeZoneId { get; set; }
        
        /// <summary>
        /// Tags for categorizing and filtering activities
        /// </summary>
        public ICollection<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether this activity is public or private
        /// </summary>
        public bool IsPublic { get; set; } = true;
        
        /// <summary>
        /// URL or resource for additional context (e.g., link to view the target)
        /// </summary>
        public string ContextUrl { get; set; }
        
      
    }
}