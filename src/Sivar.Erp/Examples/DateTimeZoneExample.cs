using System;
using System.Threading.Tasks;
using Sivar.Erp.ErpSystem.DateTimeZone;

using Sivar.Erp.System.Options;

namespace Sivar.Erp.Examples
{
    /// <summary>
    /// Example demonstrating the use of timezone-aware date/time storage
    /// </summary>
    public class DateTimeZoneExample
    {
        private readonly IOptionService _optionService;
        private readonly Erp.ErpSystem.DateTimeZone.IDateTimeZoneService _dateTimeZoneService;
        private readonly OptionTimeZoneService _optionTimeZoneService;

        /// <summary>
        /// Initializes a new instance of the DateTimeZoneExample class
        /// </summary>
        /// <param name="optionService">Option service</param>
        /// <param name="dateTimeZoneService">DateTimeZone service</param>
        public DateTimeZoneExample(IOptionService optionService, Erp.ErpSystem.DateTimeZone.IDateTimeZoneService dateTimeZoneService)
        {
            _optionService = optionService ?? throw new ArgumentNullException(nameof(optionService));
            _dateTimeZoneService = dateTimeZoneService ?? throw new ArgumentNullException(nameof(dateTimeZoneService));
            _optionTimeZoneService = new OptionTimeZoneService(_optionService, _dateTimeZoneService);
        }

        /// <summary>
        /// Demonstrates setting and retrieving timezone-aware values
        /// </summary>
        public async Task RunDemoAsync()
        {
            // Create a sample option if it doesn't exist
            var option = await _optionService.GetOptionByCodeAsync("TIMEZONE_TEST", "System");
            if (option == null)
            {
                option = new OptionDto
                {
                    Code = "TIMEZONE_TEST",
                    Name = "Timezone Test Option",
                    Description = "Option used to demonstrate timezone handling",
                    ModuleName = "System"
                };
                await _optionService.CreateOptionAsync(option);
            }
            
            // Get the local timezone ID
            var localTimeZoneId = _dateTimeZoneService.GetSystemTimeZoneId();
            Console.WriteLine($"Local system timezone: {localTimeZoneId}");
            
            // Set an option value using the local timezone
            var localNow = DateTime.Now;
            Console.WriteLine($"Setting option value at local time: {localNow}");
            
            await _optionTimeZoneService.SetOptionValueWithTimeZoneAsync(
                "TIMEZONE_TEST", 
                "System", 
                $"Value set at {localNow}", 
                localNow, 
                localTimeZoneId, 
                null, 
                "TestUser");
                
            // Wait a moment
            await Task.Delay(1000);
            
            // Get the value displaying the timestamp in different timezones
            var availableTimeZones = _dateTimeZoneService.GetAvailableTimeZones();
            
            // Get in local timezone
            var (localValue, localTimestamp) = await _optionTimeZoneService.GetCurrentOptionValueInTimeZoneAsync(
                "TIMEZONE_TEST", 
                "System", 
                null,
                localTimeZoneId);
                
            Console.WriteLine($"Value in local timezone ({localTimeZoneId}): {localValue}");
            Console.WriteLine($"Timestamp in local timezone: {localTimestamp}");
            
            // Get in UTC
            var (utcValue, utcTimestamp) = await _optionTimeZoneService.GetCurrentOptionValueInTimeZoneAsync(
                "TIMEZONE_TEST", 
                "System", 
                null,
                "UTC");
                
            Console.WriteLine($"Value in UTC: {utcValue}");
            Console.WriteLine($"Timestamp in UTC: {utcTimestamp}");
            
            // Try with a different timezone if available
            var differentTimeZone = availableTimeZones.FirstOrDefault(tz => 
                tz != "UTC" && 
                tz != localTimeZoneId && 
                (tz.Contains("Pacific") || tz.Contains("Eastern") || tz.Contains("Central")));
                
            if (!string.IsNullOrEmpty(differentTimeZone))
            {
                var (diffValue, diffTimestamp) = await _optionTimeZoneService.GetCurrentOptionValueInTimeZoneAsync(
                    "TIMEZONE_TEST", 
                    "System", 
                    null,
                    differentTimeZone);
                    
                Console.WriteLine($"Value in {differentTimeZone}: {diffValue}");
                Console.WriteLine($"Timestamp in {differentTimeZone}: {diffTimestamp}");
            }
            
            // Demonstrate direct use of IDateTimeZoneTrackable
            Console.WriteLine("\nDirect use of IDateTimeZoneTrackable:");
            var entity = new Erp.ErpSystem.DateTimeZone.DateTimeZoneTrackableBase();
            
            // Set with current local time
            entity.SetFromLocal(DateTime.Now);
            Console.WriteLine($"Entity date: {entity.Date}, time: {entity.Time}, timezone: {entity.TimeZoneId}");
            
            // Convert to various timezones
            var entityUtc = entity.ToUtcDateTime(_dateTimeZoneService);
            Console.WriteLine($"Entity in UTC: {entityUtc}");
            
            if (!string.IsNullOrEmpty(differentTimeZone))
            {
                var entityInDifferentTz = _dateTimeZoneService.ToTimeZone(entity, differentTimeZone);
                Console.WriteLine($"Entity in {differentTimeZone}: {entityInDifferentTz}");
            }
        }
    }
}