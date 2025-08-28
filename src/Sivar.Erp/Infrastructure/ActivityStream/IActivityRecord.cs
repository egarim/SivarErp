using System;

namespace Sivar.Erp.Infrastructure.ActivityStream
{
    public interface IActivityRecord
    {
        string TimeZoneId { get; set; }
        /// <summary>
        /// The entity that performed the action (typically a user)
        /// </summary>
        IStreamObject Actor { get; set; }
        /// <summary>
        /// URL or resource for additional context (e.g., link to view the target)
        /// </summary>
        string? ContextUrl { get; set; }
        /// <summary>
        /// Human-readable description of the activity
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Detailed information about the activity (could be JSON or formatted text)
        /// </summary>
        string? Details { get; set; }
        /// <summary>
        /// Unique identifier for the activity
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// Whether this activity is public or private
        /// </summary>
        bool IsPublic { get; set; }
        /// <summary>
        /// Optional additional object involved in the activity (e.g., "moved [Target] to [Object]")
        /// </summary>
        IStreamObject? Object { get; set; }
        /// <summary>
        /// Tags for categorizing and filtering activities
        /// </summary>
        ICollection<string> Tags { get; set; }
        /// <summary>
        /// The entity that was acted upon
        /// </summary>
        IStreamObject Target { get; set; }
        /// <summary>
        /// The action that was performed (e.g., "created", "modified", "deleted", "approved")
        /// </summary>
        string Verb { get; set; }
        DateOnly Date { get; set; }

        TimeOnly Time { get; set; }
    }
}
