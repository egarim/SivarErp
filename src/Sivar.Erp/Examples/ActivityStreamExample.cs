using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.Services.DateTimeZone;

namespace Sivar.Erp.Examples
{
    /// <summary>
    /// Example of activity stream usage
    /// </summary>
    public class ActivityStreamExample
    {
        private readonly IActivityStreamService _activityService;
        private readonly IDateTimeZoneService _dateTimeZoneService;
        
        /// <summary>
        /// Constructor with dependencies
        /// </summary>
        public ActivityStreamExample(
            IActivityStreamService activityService,
            IDateTimeZoneService dateTimeZoneService)
        {
            _activityService = activityService;
            _dateTimeZoneService = dateTimeZoneService;
        }
        
        /// <summary>
        /// Example that records various types of activities
        /// </summary>
        public async Task RunExampleAsync()
        {
            // 1. Record a user login activity
            await RecordUserLoginActivity();
            
            // 2. Record document creation activity
            await RecordDocumentCreationActivity();
            
            // 3. Record approval activity with workflow
            await RecordApprovalActivity();
            
            // 4. Record system notification activity
            await RecordSystemActivity();
            
            // 5. Query activities
            await QueryActivities();
        }
        
        /// <summary>
        /// Example of recording a user login activity
        /// </summary>
        private async Task RecordUserLoginActivity()
        {
            // Define the user as actor
            var user = new StreamObject
            {
                ObjectType = "User",
                ObjectKey = "user-123",
                DisplayName = "John Smith",
                DisplayImage = "/assets/avatars/john-smith.jpg"
            };
            
            // The system is the target
            var system = new StreamObject
            {
                ObjectType = "System",
                ObjectKey = "erp",
                DisplayName = "ERP System",
                DisplayImage = "/assets/icons/system.svg"
            };
            
            // Get current timezone
            string timeZoneId = "America/El_Salvador";
            
            // Create activity using the simplified method
            await _activityService.RecordActivityAsync(
                user,
                "logged into",
                system,
                timeZoneId);
        }
        
        /// <summary>
        /// Example of recording a document creation activity
        /// </summary>
        private async Task RecordDocumentCreationActivity()
        {
            // Define the user as actor
            var user = new StreamObject
            {
                ObjectType = "User",
                ObjectKey = "user-123",
                DisplayName = "John Smith",
                DisplayImage = "/assets/avatars/john-smith.jpg"
            };
            
            // The invoice is the target
            var invoice = new StreamObject
            {
                ObjectType = "Invoice",
                ObjectKey = "invoice-456",
                DisplayName = "Invoice #456",
                DisplayImage = "/assets/icons/invoice.svg"
            };
            
            // Get current timezone
            string timeZoneId = "America/El_Salvador";
            
            // Create activity
            var activity = new ActivityRecord
            {
                Actor = user,
                Verb = "created",
                Target = invoice,
                Description = "John Smith created Invoice #456",
                Details = "Invoice created for Customer XYZ with a total of $1,500.00",
                TimeZoneId = timeZoneId,
                ContextUrl = "/invoices/456",
                Tags = new List<string> { "finance", "invoices", "creation" },
                IsPublic = true
            };
            
            await _activityService.RecordActivityAsync(activity);
        }
        
        /// <summary>
        /// Example of recording an approval activity with workflow
        /// </summary>
        private async Task RecordApprovalActivity()
        {
            // Define the approver as actor
            var approver = new StreamObject
            {
                ObjectType = "User",
                ObjectKey = "user-456",
                DisplayName = "Maria Rodriguez",
                DisplayImage = "/assets/avatars/maria-rodriguez.jpg"
            };
            
            // The purchase order is the target
            var purchaseOrder = new StreamObject
            {
                ObjectType = "PurchaseOrder",
                ObjectKey = "po-789",
                DisplayName = "PO #789",
                DisplayImage = "/assets/icons/purchase-order.svg"
            };
            
            // The workflow stage is the object
            var workflowStage = new StreamObject
            {
                ObjectType = "WorkflowStage",
                ObjectKey = "stage-3",
                DisplayName = "Final Approval",
                DisplayImage = "/assets/icons/workflow-stage.svg"
            };
            
            // Get current timezone
            string timeZoneId = "America/El_Salvador";
            
            // Create activity
            var activity = new ActivityRecord
            {
                Actor = approver,
                Verb = "approved",
                Target = purchaseOrder,
                Object = workflowStage,
                Description = "Maria Rodriguez approved PO #789 at Final Approval stage",
                TimeZoneId = timeZoneId,
                ContextUrl = "/purchase-orders/789",
                Tags = new List<string> { "procurement", "approval", "workflow" }
            };
            
            await _activityService.RecordActivityAsync(activity);
        }
        
        /// <summary>
        /// Example of recording a system notification activity
        /// </summary>
        private async Task RecordSystemActivity()
        {
            // System is the actor
            var system = new StreamObject
            {
                ObjectType = "System",
                ObjectKey = "backup-service",
                DisplayName = "Backup Service",
                DisplayImage = "/assets/icons/backup.svg"
            };
            
            // The database is the target
            var database = new StreamObject
            {
                ObjectType = "Database",
                ObjectKey = "erp-db",
                DisplayName = "ERP Database",
                DisplayImage = "/assets/icons/database.svg"
            };
            
            // Get current timezone
            string timeZoneId = "America/El_Salvador";
            
            // Create activity
            var activity = new ActivityRecord
            {
                Actor = system,
                Verb = "completed backup of",
                Target = database,
                Description = "Backup Service completed backup of ERP Database",
                Details = "Full backup completed in 15 minutes. Backup size: 2.3 GB.",
                TimeZoneId = timeZoneId,
                Tags = new List<string> { "system", "maintenance", "backup" },
                IsPublic = false // Not shown in public feeds
            };
            
            await _activityService.RecordActivityAsync(activity);
        }
        
        /// <summary>
        /// Example of querying activities
        /// </summary>
        private async Task QueryActivities()
        {
            // 1. Get all activities for a specific user
            var userActivities = await _activityService.GetActorActivityStreamAsync(
                actorType: "User",
                actorKey: "user-123",
                page: 1,
                pageSize: 10);
            
            Console.WriteLine("User Activities:");
            foreach (var activity in userActivities)
            {
                Console.WriteLine($"- {activity.Description} ({activity.LocalDateTime})");
            }
            
            // 2. Get all activities related to a specific invoice
            var invoiceActivities = await _activityService.GetTargetActivityStreamAsync(
                targetType: "Invoice",
                targetKey: "invoice-456",
                page: 1,
                pageSize: 10);
            
            Console.WriteLine("\nInvoice Activities:");
            foreach (var activity in invoiceActivities)
            {
                Console.WriteLine($"- {activity.Actor.DisplayName} {activity.Verb} {activity.Target.DisplayName} ({activity.LocalDateTime})");
            }
            
            // 3. Get activities with specific tags
            var financialActivities = await _activityService.SearchActivitiesAsync(
                tags: new[] { "finance" },
                page: 1,
                pageSize: 10);
            
            Console.WriteLine("\nFinancial Activities:");
            foreach (var activity in financialActivities)
            {
                Console.WriteLine($"- {activity.Description} ({activity.LocalDateTime})");
            }
            
            // 4. Get activities within a date range
            var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.Now);
            
            var recentActivities = await _activityService.SearchActivitiesAsync(
                startDate: startDate,
                endDate: endDate,
                timeZoneId: "America/El_Salvador",
                page: 1,
                pageSize: 20);
            
            Console.WriteLine("\nRecent Activities:");
            foreach (var activity in recentActivities)
            {
                Console.WriteLine($"- {activity.Description} ({activity.LocalDateTime})");
            }
            
            // 5. Get global activity stream (public activities only)
            var globalActivities = await _activityService.GetGlobalActivityStreamAsync(
                onlyPublic: true,
                page: 1,
                pageSize: 20);
            
            Console.WriteLine("\nGlobal Activity Stream:");
            foreach (var activity in globalActivities)
            {
                Console.WriteLine($"- {activity.Description} ({activity.LocalDateTime})");
            }
        }
    }
}