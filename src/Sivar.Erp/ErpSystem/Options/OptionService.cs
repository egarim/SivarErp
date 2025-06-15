using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.ErpSystem.Options
{
    /// <summary>
    /// Implementation of option service
    /// </summary>
    public class OptionService : IOptionService
    {
        // In-memory storage for options (in a real application, this would use a repository or database)
        private readonly List<OptionDto> _options = new();
        private readonly List<OptionChoiceDto> _choices = new();
        private readonly List<OptionDetailDto> _details = new();
        private readonly OptionValidator _validator;

        /// <summary>
        /// Initializes a new instance of the OptionService class
        /// </summary>
        public OptionService()
        {
            _validator = new OptionValidator();
        }

        /// <summary>
        /// Initializes a new instance of the OptionService class with a custom validator
        /// </summary>
        /// <param name="validator">Custom option validator</param>
        public OptionService(OptionValidator validator)
        {
            _validator = validator ?? new OptionValidator();
        }

        #region Option Management

        /// <inheritdoc />
        public Task<OptionDto?> GetOptionAsync(Guid id)
        {
            var option = _options.FirstOrDefault(o => o.Id == id);
            return Task.FromResult(option);
        }

        /// <inheritdoc />
        public Task<OptionDto?> GetOptionByCodeAsync(string code, string moduleName)
        {
            var option = _options.FirstOrDefault(o => 
                o.Code == code && 
                o.ModuleName == moduleName && 
                o.IsActive);
                
            return Task.FromResult(option);
        }

        /// <inheritdoc />
        public Task<IEnumerable<OptionDto>> GetOptionsByModuleAsync(string moduleName)
        {
            var options = _options
                .Where(o => o.ModuleName == moduleName && o.IsActive)
                .ToList();
                
            return Task.FromResult<IEnumerable<OptionDto>>(options);
        }

        /// <inheritdoc />
        public Task<OptionDto> CreateOptionAsync(OptionDto option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            if (!_validator.ValidateOption(option))
            {
                throw new ArgumentException("Invalid option", nameof(option));
            }

            // Check if option with same code and module already exists
            if (_options.Any(o => o.Code == option.Code && o.ModuleName == option.ModuleName))
            {
                throw new InvalidOperationException($"Option with code '{option.Code}' already exists in module '{option.ModuleName}'");
            }

            // Generate a new ID if not provided
            if (option.Id == Guid.Empty)
            {
                option.Id = Guid.NewGuid();
            }

            option.CreatedDate = DateTime.UtcNow;
            _options.Add(option);
            
            return Task.FromResult(option);
        }

        /// <inheritdoc />
        public Task<OptionDto> UpdateOptionAsync(OptionDto option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            if (!_validator.ValidateOption(option))
            {
                throw new ArgumentException("Invalid option", nameof(option));
            }

            var index = _options.FindIndex(o => o.Id == option.Id);
            if (index == -1)
            {
                throw new KeyNotFoundException($"Option with ID {option.Id} not found");
            }

            // Check if the update would create a duplicate
            var duplicate = _options.FirstOrDefault(o => 
                o.Id != option.Id && 
                o.Code == option.Code && 
                o.ModuleName == option.ModuleName);
                
            if (duplicate != null)
            {
                throw new InvalidOperationException($"Option with code '{option.Code}' already exists in module '{option.ModuleName}'");
            }

            option.ModifiedDate = DateTime.UtcNow;
            _options[index] = option;
            
            return Task.FromResult(option);
        }

        /// <inheritdoc />
        public Task<bool> DeleteOptionAsync(Guid id)
        {
            var index = _options.FindIndex(o => o.Id == id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }

            // Check if the option has any associated choices
            if (_choices.Any(c => c.OptionId == id))
            {
                throw new InvalidOperationException("Cannot delete option with associated choices");
            }

            // Check if the option has any associated details
            if (_details.Any(d => d.OptionId == id))
            {
                throw new InvalidOperationException("Cannot delete option with associated details");
            }

            _options.RemoveAt(index);
            return Task.FromResult(true);
        }

        #endregion

        #region Option Choice Management

        /// <inheritdoc />
        public Task<OptionChoiceDto?> GetOptionChoiceAsync(Guid id)
        {
            var choice = _choices.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(choice);
        }

        /// <inheritdoc />
        public Task<IEnumerable<OptionChoiceDto>> GetChoicesForOptionAsync(Guid optionId)
        {
            var choices = _choices
                .Where(c => c.OptionId == optionId && c.IsActive)
                .ToList();
                
            return Task.FromResult<IEnumerable<OptionChoiceDto>>(choices);
        }

        /// <inheritdoc />
        public Task<OptionChoiceDto> CreateOptionChoiceAsync(OptionChoiceDto choice)
        {
            if (choice == null)
            {
                throw new ArgumentNullException(nameof(choice));
            }

            if (!_validator.ValidateOptionChoice(choice))
            {
                throw new ArgumentException("Invalid option choice", nameof(choice));
            }

            // Check if option exists
            if (_options.All(o => o.Id != choice.OptionId))
            {
                throw new KeyNotFoundException($"Option with ID {choice.OptionId} not found");
            }

            // Generate a new ID if not provided
            if (choice.Id == Guid.Empty)
            {
                choice.Id = Guid.NewGuid();
            }

            _choices.Add(choice);
            
            return Task.FromResult(choice);
        }

        /// <inheritdoc />
        public Task<OptionChoiceDto> UpdateOptionChoiceAsync(OptionChoiceDto choice)
        {
            if (choice == null)
            {
                throw new ArgumentNullException(nameof(choice));
            }

            if (!_validator.ValidateOptionChoice(choice))
            {
                throw new ArgumentException("Invalid option choice", nameof(choice));
            }

            var index = _choices.FindIndex(c => c.Id == choice.Id);
            if (index == -1)
            {
                throw new KeyNotFoundException($"Option choice with ID {choice.Id} not found");
            }

            // Check if option exists
            if (_options.All(o => o.Id != choice.OptionId))
            {
                throw new KeyNotFoundException($"Option with ID {choice.OptionId} not found");
            }

            _choices[index] = choice;
            
            return Task.FromResult(choice);
        }

        /// <inheritdoc />
        public Task<bool> DeleteOptionChoiceAsync(Guid id)
        {
            var index = _choices.FindIndex(c => c.Id == id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }

            // Check if the choice has any associated details
            if (_details.Any(d => d.OptionChoiceId == id))
            {
                throw new InvalidOperationException("Cannot delete option choice with associated details");
            }

            _choices.RemoveAt(index);
            return Task.FromResult(true);
        }

        #endregion

        #region Option Detail Management

        /// <inheritdoc />
        public Task<OptionDetailDto?> GetOptionDetailAsync(Guid id)
        {
            var detail = _details.FirstOrDefault(d => d.Id == id);
            return Task.FromResult(detail);
        }

        /// <inheritdoc />
        public Task<OptionDetailDto?> GetActiveOptionDetailAsync(Guid optionId, DateTime effectiveDate)
        {
            var detail = _details
                .Where(d => d.OptionId == optionId && d.IsActiveOnDate(effectiveDate))
                .OrderByDescending(d => d.ValidFrom)
                .FirstOrDefault();
                
            return Task.FromResult(detail);
        }

        /// <inheritdoc />
        public Task<IEnumerable<OptionDetailDto>> GetOptionHistoryAsync(Guid optionId)
        {
            var details = _details
                .Where(d => d.OptionId == optionId)
                .OrderByDescending(d => d.ValidFrom)
                .ToList();
                
            return Task.FromResult<IEnumerable<OptionDetailDto>>(details);
        }

        /// <inheritdoc />
        public Task<OptionDetailDto> CreateOptionDetailAsync(OptionDetailDto detail)
        {
            if (detail == null)
            {
                throw new ArgumentNullException(nameof(detail));
            }

            if (!_validator.ValidateOptionDetail(detail))
            {
                throw new ArgumentException("Invalid option detail", nameof(detail));
            }

            // Check if option exists
            if (_options.All(o => o.Id != detail.OptionId))
            {
                throw new KeyNotFoundException($"Option with ID {detail.OptionId} not found");
            }

            // Check if option choice exists
            if (_choices.All(c => c.Id != detail.OptionChoiceId))
            {
                throw new KeyNotFoundException($"Option choice with ID {detail.OptionChoiceId} not found");
            }

            // Generate a new ID if not provided
            if (detail.Id == Guid.Empty)
            {
                detail.Id = Guid.NewGuid();
            }

            detail.CreatedDate = DateTime.UtcNow;
            _details.Add(detail);
            
            return Task.FromResult(detail);
        }

        /// <inheritdoc />
        public Task<OptionDetailDto> UpdateOptionDetailAsync(OptionDetailDto detail)
        {
            if (detail == null)
            {
                throw new ArgumentNullException(nameof(detail));
            }

            if (!_validator.ValidateOptionDetail(detail))
            {
                throw new ArgumentException("Invalid option detail", nameof(detail));
            }

            var index = _details.FindIndex(d => d.Id == detail.Id);
            if (index == -1)
            {
                throw new KeyNotFoundException($"Option detail with ID {detail.Id} not found");
            }

            // Check if option exists
            if (_options.All(o => o.Id != detail.OptionId))
            {
                throw new KeyNotFoundException($"Option with ID {detail.OptionId} not found");
            }

            // Check if option choice exists
            if (_choices.All(c => c.Id != detail.OptionChoiceId))
            {
                throw new KeyNotFoundException($"Option choice with ID {detail.OptionChoiceId} not found");
            }

            _details[index] = detail;
            
            return Task.FromResult(detail);
        }

        /// <inheritdoc />
        public Task<bool> DeleteOptionDetailAsync(Guid id)
        {
            var index = _details.FindIndex(d => d.Id == id);
            if (index == -1)
            {
                return Task.FromResult(false);
            }

            _details.RemoveAt(index);
            return Task.FromResult(true);
        }

        #endregion

        #region Utility Methods

        /// <inheritdoc />
        public async Task<string?> GetCurrentOptionValueAsync(string optionCode, string moduleName, DateTime? effectiveDate = null)
        {
            var date = effectiveDate ?? DateTime.UtcNow;
            
            var option = await GetOptionByCodeAsync(optionCode, moduleName);
            if (option == null)
            {
                return null;
            }

            var detail = await GetActiveOptionDetailAsync(option.Id, date);
            return detail?.Value;
        }

        /// <inheritdoc />
        public async Task<bool> SetOptionValueAsync(string optionCode, string moduleName, string value, DateTime validFrom, DateTime? validTo = null, string? userName = null)
        {
            var option = await GetOptionByCodeAsync(optionCode, moduleName);
            if (option == null)
            {
                return false;
            }

            // Get all choices for this option
            var choices = await GetChoicesForOptionAsync(option.Id);
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
                
                await CreateOptionChoiceAsync(choice);
            }

            // Create new option detail
            var detail = new OptionDetailDto
            {
                OptionId = option.Id,
                OptionChoiceId = choice.Id,
                Value = value,
                ValidFrom = validFrom,
                ValidTo = validTo,
                CreatedBy = userName,
                IsActive = true
            };
            
            await CreateOptionDetailAsync(detail);
            return true;
        }

        #endregion
    }
}