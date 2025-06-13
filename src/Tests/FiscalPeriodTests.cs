using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.FiscalPeriods;

namespace Sivar.Erp.Tests.FiscalPeriods
{
    /// <summary>
    /// Unit tests for the Fiscal Periods module
    /// </summary>
    [TestFixture]
    public class FiscalPeriodTests
    {
     
        private IFiscalPeriodService _fiscalPeriodService;
        private FiscalPeriodValidator _validator; [SetUp]
        public void Setup()
        {
           
          
            _validator = new FiscalPeriodValidator();

            // Clear any existing fiscal periods from previous tests
            _fiscalPeriodService.ClearAllFiscalPeriods();
        }

        #region FiscalPeriodDto Tests

        [Test]
        public void FiscalPeriodDto_NewFiscalPeriod_HasNonEmptyGuid()
        {
            // Arrange & Act
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Assert
            Assert.That(fiscalPeriod.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void FiscalPeriodDto_DefaultStatus_IsOpen()
        {
            // Arrange & Act
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Assert
            Assert.That(fiscalPeriod.Status, Is.EqualTo(FiscalPeriodStatus.Open));
            Assert.That(fiscalPeriod.IsOpen(), Is.True);
            Assert.That(fiscalPeriod.IsClosed(), Is.False);
        }

        [Test]
        public void FiscalPeriodDto_Close_SetsStatusToClosed()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            fiscalPeriod.Close();

            // Assert
            Assert.That(fiscalPeriod.Status, Is.EqualTo(FiscalPeriodStatus.Closed));
            Assert.That(fiscalPeriod.IsOpen(), Is.False);
            Assert.That(fiscalPeriod.IsClosed(), Is.True);
        }

        [Test]
        public void FiscalPeriodDto_Open_SetsStatusToOpen()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                Status = FiscalPeriodStatus.Closed,
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            fiscalPeriod.Open();

            // Assert
            Assert.That(fiscalPeriod.Status, Is.EqualTo(FiscalPeriodStatus.Open));
            Assert.That(fiscalPeriod.IsOpen(), Is.True);
            Assert.That(fiscalPeriod.IsClosed(), Is.False);
        }

        [Test]
        public void FiscalPeriodDto_ContainsDate_WithDateInRange_ReturnsTrue()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var testDate = new DateOnly(2025, 2, 15);

            // Act
            bool contains = fiscalPeriod.ContainsDate(testDate);

            // Assert
            Assert.That(contains, Is.True);
        }

        [Test]
        public void FiscalPeriodDto_ContainsDate_WithDateOutsideRange_ReturnsFalse()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var testDate = new DateOnly(2025, 4, 1);

            // Act
            bool contains = fiscalPeriod.ContainsDate(testDate);

            // Assert
            Assert.That(contains, Is.False);
        }

        [Test]
        public void FiscalPeriodDto_GetDurationInDays_ReturnsCorrectDuration()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 1, 31), // January has 31 days
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            int duration = fiscalPeriod.GetDurationInDays();

            // Assert
            Assert.That(duration, Is.EqualTo(31));
        }

        [Test]
        public void FiscalPeriodDto_Validate_WithValidData_ReturnsTrue()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            bool isValid = fiscalPeriod.Validate();

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodDto_Validate_WithEmptyName_ReturnsFalse()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            bool isValid = fiscalPeriod.Validate();

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void FiscalPeriodDto_Validate_WithEndDateBeforeStartDate_ReturnsFalse()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Invalid Period",
                StartDate = new DateOnly(2025, 3, 31),
                EndDate = new DateOnly(2025, 1, 1),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            bool isValid = fiscalPeriod.Validate();

            // Assert
            Assert.That(isValid, Is.False);
        }

        #endregion

        #region FiscalPeriodValidator Tests

        [Test]
        public void FiscalPeriodValidator_ValidateFiscalPeriod_WithValidPeriod_ReturnsTrue()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            bool isValid = _validator.ValidateFiscalPeriod(fiscalPeriod);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateDateRange_WithValidRange_ReturnsTrue()
        {
            // Arrange
            var startDate = new DateOnly(2025, 1, 1);
            var endDate = new DateOnly(2025, 3, 31);

            // Act
            bool isValid = _validator.ValidateDateRange(startDate, endDate);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateDateRange_WithInvalidRange_ReturnsFalse()
        {
            // Arrange
            var startDate = new DateOnly(2025, 3, 31);
            var endDate = new DateOnly(2025, 1, 1);

            // Act
            bool isValid = _validator.ValidateDateRange(startDate, endDate);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateName_WithValidName_ReturnsTrue()
        {
            // Act
            bool isValid = _validator.ValidateName("Q1 2025");

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateName_WithEmptyName_ReturnsFalse()
        {
            // Act
            bool isValid = _validator.ValidateName("");

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void FiscalPeriodValidator_ValidatePeriodLength_WithReasonableLength_ReturnsTrue()
        {
            // Arrange
            var startDate = new DateOnly(2025, 1, 1);
            var endDate = new DateOnly(2025, 12, 31); // One year

            // Act
            bool isValid = _validator.ValidatePeriodLength(startDate, endDate);

            // Assert
            Assert.That(isValid, Is.True);
        }
        [Test]
        public void FiscalPeriodValidator_ValidatePeriodLength_WithTooLongPeriod_ReturnsFalse()
        {
            // Arrange
            var startDate = new DateOnly(2025, 1, 1);
            var endDate = new DateOnly(2027, 12, 31); // Three years

            // Act
            bool isValid = _validator.ValidatePeriodLength(startDate, endDate);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateNoOverlap_WithNoExistingPeriods_ReturnsTrue()
        {
            // Arrange
            var startDate = new DateOnly(2025, 1, 1);
            var endDate = new DateOnly(2025, 3, 31);
            var existingPeriods = new List<IFiscalPeriod>();

            // Act
            bool isValid = _validator.ValidateNoOverlap(startDate, endDate, existingPeriods);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateNoOverlap_WithOverlappingPeriod_ReturnsFalse()
        {
            // Arrange
            var startDate = new DateOnly(2025, 2, 1);
            var endDate = new DateOnly(2025, 4, 30);
            var existingPeriods = new List<IFiscalPeriod>
            {
                new FiscalPeriodDto
                {
                    StartDate = new DateOnly(2025, 1, 1),
                    EndDate = new DateOnly(2025, 3, 31),
                    Name = "Q1 2025",
                    InsertedBy = "TestUser",
                    UpdatedBy = "TestUser"
                }
            };

            // Act
            bool isValid = _validator.ValidateNoOverlap(startDate, endDate, existingPeriods);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateNoOverlap_WithNonOverlappingPeriod_ReturnsTrue()
        {
            // Arrange
            var startDate = new DateOnly(2025, 4, 1);
            var endDate = new DateOnly(2025, 6, 30);
            var existingPeriods = new List<IFiscalPeriod>
            {
                new FiscalPeriodDto
                {
                    StartDate = new DateOnly(2025, 1, 1),
                    EndDate = new DateOnly(2025, 3, 31),
                    Name = "Q1 2025",
                    InsertedBy = "TestUser",
                    UpdatedBy = "TestUser"
                }
            };

            // Act
            bool isValid = _validator.ValidateNoOverlap(startDate, endDate, existingPeriods);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateFiscalPeriodWithOverlapCheck_WithValidPeriod_ReturnsTrue()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q2 2025",
                StartDate = new DateOnly(2025, 4, 1),
                EndDate = new DateOnly(2025, 6, 30),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var existingPeriods = new List<IFiscalPeriod>
            {
                new FiscalPeriodDto
                {
                    StartDate = new DateOnly(2025, 1, 1),
                    EndDate = new DateOnly(2025, 3, 31),
                    Name = "Q1 2025",
                    InsertedBy = "TestUser",
                    UpdatedBy = "TestUser"
                }
            };

            // Act
            bool isValid = _validator.ValidateFiscalPeriodWithOverlapCheck(fiscalPeriod, existingPeriods);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void FiscalPeriodValidator_ValidateFiscalPeriodWithOverlapCheck_WithOverlappingPeriod_ReturnsFalse()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Overlapping Period",
                StartDate = new DateOnly(2025, 2, 1),
                EndDate = new DateOnly(2025, 4, 30),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var existingPeriods = new List<IFiscalPeriod>
            {
                new FiscalPeriodDto
                {
                    StartDate = new DateOnly(2025, 1, 1),
                    EndDate = new DateOnly(2025, 3, 31),
                    Name = "Q1 2025",
                    InsertedBy = "TestUser",
                    UpdatedBy = "TestUser"
                }
            };

            // Act
            bool isValid = _validator.ValidateFiscalPeriodWithOverlapCheck(fiscalPeriod, existingPeriods);

            // Assert
            Assert.That(isValid, Is.False);
        }

        #endregion

        #region FiscalPeriodService Tests

        [Test]
        public async Task FiscalPeriodService_CreateFiscalPeriodAsync_WithValidData_CreatesSuccessfully()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                Description = "First quarter of 2025",
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            var createdPeriod = await _fiscalPeriodService.CreateFiscalPeriodAsync(fiscalPeriod, "TestUser");

            // Assert
            Assert.That(createdPeriod, Is.Not.Null);
            Assert.That(createdPeriod.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdPeriod.Name, Is.EqualTo("Q1 2025"));
            Assert.That(createdPeriod.Status, Is.EqualTo(FiscalPeriodStatus.Open));
        }

        [Test]
        public async Task FiscalPeriodService_GetFiscalPeriodForDateAsync_WithDateInPeriod_ReturnsPeriod()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            await _fiscalPeriodService.CreateFiscalPeriodAsync(fiscalPeriod, "TestUser");

            // Act
            var foundPeriod = await _fiscalPeriodService.GetFiscalPeriodForDateAsync(new DateOnly(2025, 2, 15));

            // Assert
            Assert.That(foundPeriod, Is.Not.Null);
            Assert.That(foundPeriod.Name, Is.EqualTo("Q1 2025"));
        }

        [Test]
        public async Task FiscalPeriodService_CloseFiscalPeriodAsync_ClosesPeriodSuccessfully()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var createdPeriod = await _fiscalPeriodService.CreateFiscalPeriodAsync(fiscalPeriod, "TestUser");

            // Act
            var closedPeriod = await _fiscalPeriodService.CloseFiscalPeriodAsync(createdPeriod.Id, "TestUser");

            // Assert
            Assert.That(closedPeriod.Status, Is.EqualTo(FiscalPeriodStatus.Closed));
        }

        [Test]
        public async Task FiscalPeriodService_GetFiscalPeriodsByStatusAsync_ReturnsCorrectPeriods()
        {
            // Arrange
            var openPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                Status = FiscalPeriodStatus.Open,
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var closedPeriod = new FiscalPeriodDto
            {
                Name = "Q4 2024",
                StartDate = new DateOnly(2024, 10, 1),
                EndDate = new DateOnly(2024, 12, 31),
                Status = FiscalPeriodStatus.Closed,
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            await _fiscalPeriodService.CreateFiscalPeriodAsync(openPeriod, "TestUser");
            var created = await _fiscalPeriodService.CreateFiscalPeriodAsync(closedPeriod, "TestUser");
            await _fiscalPeriodService.CloseFiscalPeriodAsync(created.Id, "TestUser");

            // Act
            var openPeriods = await _fiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Open);
            var closedPeriods = await _fiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Closed);

            // Assert
            Assert.That(openPeriods.Count(), Is.GreaterThanOrEqualTo(1));
            Assert.That(closedPeriods.Count(), Is.GreaterThanOrEqualTo(1));
        }
        [Test]
        public void FiscalPeriodService_CreateFiscalPeriodAsync_WithOverlappingPeriods_ThrowsException()
        {
            // Arrange
            var firstPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var overlappingPeriod = new FiscalPeriodDto
            {
                Name = "Overlapping Period",
                StartDate = new DateOnly(2025, 2, 1),
                EndDate = new DateOnly(2025, 4, 30),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _fiscalPeriodService.CreateFiscalPeriodAsync(firstPeriod, "TestUser"));
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _fiscalPeriodService.CreateFiscalPeriodAsync(overlappingPeriod, "TestUser"));
        }

        [Test]
        public async Task FiscalPeriodService_ValidateFiscalPeriodWithOverlapAsync_WithValidPeriod_ReturnsTrue()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            var isValid = await _fiscalPeriodService.ValidateFiscalPeriodWithOverlapAsync(fiscalPeriod);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public async Task FiscalPeriodService_ValidateFiscalPeriodWithOverlapAsync_WithOverlappingPeriod_ReturnsFalse()
        {
            // Arrange
            var firstPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            await _fiscalPeriodService.CreateFiscalPeriodAsync(firstPeriod, "TestUser");

            var overlappingPeriod = new FiscalPeriodDto
            {
                Name = "Overlapping Period",
                StartDate = new DateOnly(2025, 2, 1),
                EndDate = new DateOnly(2025, 4, 30),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };

            // Act
            var isValid = await _fiscalPeriodService.ValidateFiscalPeriodWithOverlapAsync(overlappingPeriod);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public async Task FiscalPeriodService_ValidateFiscalPeriodWithOverlapAsync_ExcludingCurrentPeriod_ReturnsTrue()
        {
            // Arrange
            var fiscalPeriod = new FiscalPeriodDto
            {
                Name = "Q1 2025",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 3, 31),
                InsertedBy = "TestUser",
                UpdatedBy = "TestUser"
            };
            var createdPeriod = await _fiscalPeriodService.CreateFiscalPeriodAsync(fiscalPeriod, "TestUser");

            // Modify the period slightly but exclude itself from overlap check
            createdPeriod.EndDate = new DateOnly(2025, 3, 30);

            // Act
            var isValid = await _fiscalPeriodService.ValidateFiscalPeriodWithOverlapAsync(createdPeriod, createdPeriod.Id);

            // Assert
            Assert.That(isValid, Is.True);
        }

        #endregion
    }
}
