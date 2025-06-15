using Sivar.Erp.ErpSystem.DateTimeZone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.ErpSystem.Options
{
    /// <summary>
    /// Service that extends the option service with timezone-aware capabilities
    /// </summary>
    public class OptionTimeZoneService
    {
        private readonly IOptionService _optionService;
        private readonly Erp.ErpSystem.DateTimeZone.IDateTimeZoneService _dateTimeZoneService;

        /// <summary>
        /// Initializes a new instance of the OptionTimeZoneService class
        /// </summary>
        /// <param name="optionService">The underlying option service</param>
        /// <param name="dateTimeZoneService">The datetime/timezone service</param>
        public OptionTimeZoneService(IOptionService optionService, Erp.ErpSystem.DateTimeZone.IDateTimeZoneService dateTimeZoneService)
        {
            _optionService = optionService ?? throw new ArgumentNullException(nameof(optionService));
            _dateTimeZoneService = dateTimeZoneService ?? throw new ArgumentNullException(nameof(dateTimeZoneService));
        }

        /// <summary>
        /// Creates a new timezone-aware option detail
        /// </summary>
        /// <param name="optionCode">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <param name="value">Option value</param>
        /// <param name="effectiveDateTime">Effective date and time</param>
        /// <param name="timeZoneId">Timezone ID for the effective date and time</param>
        /// <param name="validTo">Optional end date for validity</param>
        /// <param name="userName">User making the change</param>
        /// <returns>True if created successfully, false otherwise</returns>
        public async Task<bool> SetOptionValueWithTimeZoneAsync(string optionCode, 
                                                             string moduleName, 
                                                             string value, 
                                                             DateTime effectiveDateTime, 
                                                             string timeZoneId, 
                                                             DateTime? validTo = null, 
                                                             string? userName = null)
        {
            var option = await _optionService.GetOptionByCodeAsync(optionCode, moduleName);
            if (option == null)
            {
                return false;
            }

            // Get all choices for this option
            var choices = await _optionService.GetChoicesForOptionAsync(option.Id);
            var choice = choices.FirstOrDefault();
            
            if (choice == null)
            {
                // Auto-create a default choice if none exists
                choice = new OptionChoiceDto
                {
                    OptionId = option.Id,
                    Name = "Default",
                    Description = "Auto-generated default choice",
                    IsDefault = true,
                    IsActive = true
                };
                
                await _optionService.CreateOptionChoiceAsync(choice);
            }

            // Create a timezone-aware option detail
            var detail = new OptionDetailWithTimeZoneDto
            {
                OptionId = option.Id,
                OptionChoiceId = choice.Id,
                Value = value,
                ValidTo = validTo,
                CreatedBy = userName,
                IsActive = true
            };
            
            // Set the date/time/timezone using our service
            _dateTimeZoneService.SetDateTimeZone(detail, effectiveDateTime, timeZoneId);
            
            // Create the detail
            await CreateOptionDetailWithTimeZoneAsync(detail);
            return true;
        }
        
        /// <summary>
        /// Gets the current option value in a specific timezone
        /// </summary>
        /// <param name="optionCode">Option code</param>
        /// <param name="moduleName">Module name</param>
        /// <param name="effectiveDateTime">Optional effective date, defaults to current UTC time</param>
        /// <param name="targetTimeZoneId">Target timezone ID to convert the value's timestamp to</param>
        /// <returns>Option value with timezone-converted timestamp information</returns>
        public async Task<(string? Value, DateTime? Timestamp)> GetCurrentOptionValueInTimeZoneAsync(string optionCode, 
                                                                                                   string moduleName, 
                                                                                                   DateTime? effectiveDateTime = null, 
                                                                                                   string? targetTimeZoneId = null)
        {
            var date = effectiveDateTime ?? DateTime.UtcNow;
            
            var option = await _optionService.GetOptionByCodeAsync(optionCode, moduleName);
            if (option == null)
            {
                return (null, null);
            }

            // Get the current active value from the regular service
            var value = await _optionService.GetCurrentOptionValueAsync(optionCode, moduleName, date);
            if (value == null)
            {
                return (null, null);
            }
            
            // Now get the actual detail to access its timestamp information
            // In a real implementation, you would extend the OptionService to return the full detail
            // For this example, we'll simulate the retrieval
            var detail = await SimulateGetActiveOptionDetailWithTimeZoneAsync(option.Id, date);
            if (detail == null)
            {
                return (value, date);
            }
            
            // Convert the timestamp to the requested timezone
            DateTime timestamp;
            if (!string.IsNullOrEmpty(targetTimeZoneId))
            {
                timestamp = _dateTimeZoneService.ToTimeZone(detail, targetTimeZoneId);
            }
            else
            {
                timestamp = detail.Date.ToDateTime(detail.Time);
            }
            
            return (value, timestamp);
        }
        
        // In a real implementation, these methods would access the actual data repository
        private Task<OptionDetailWithTimeZoneDto> CreateOptionDetailWithTimeZoneAsync(OptionDetailWithTimeZoneDto detail)
        {
            // Simulated creation
            return Task.FromResult(detail);
        }
        
        private Task<OptionDetailWithTimeZoneDto?> SimulateGetActiveOptionDetailWithTimeZoneAsync(Guid optionId, DateTime effectiveDate)
        {
            // In a real implementation, this would retrieve from a database
            var detail = new OptionDetailWithTimeZoneDto
            {
                OptionId = optionId,
                OptionChoiceId = Guid.NewGuid(),
                Value = "Sample value",
                Date = DateOnly.FromDateTime(effectiveDate.AddDays(-7)), // Simulated week-old setting
                Time = TimeOnly.FromDateTime(effectiveDate.AddDays(-7)),
                TimeZoneId = "UTC",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "System"
            };
            
            return Task.FromResult<OptionDetailWithTimeZoneDto?>(detail);
        }
    }
}