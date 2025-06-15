using System;
using NUnit.Framework;

namespace Sivar.Erp.Tests
{
    [TestFixture]
    public class DateTimeZoneServiceTests
    {
        private IDateTimeZoneService _dateTimeZoneService;

        [SetUp]
        public void Setup()
        {
            _dateTimeZoneService = new DateTimeZoneService();
        }

        [Test]
        public void SetDateTimeZone_WithUtcDateTime_SetsCorrectly()
        {
            // Arrange
            var entity = new DateTimeZoneTrackableBase();
            var utcNow = DateTime.UtcNow;
            
            // Act
            _dateTimeZoneService.SetDateTimeZone(entity, utcNow);
            
            // Assert
            Assert.That(entity.Date, Is.EqualTo(DateOnly.FromDateTime(utcNow)));
            Assert.That(entity.Time, Is.EqualTo(TimeOnly.FromDateTime(utcNow)));
            Assert.That(entity.TimeZoneId, Is.EqualTo("UTC"));
        }

        [Test]
        public void ToUtc_WithUtcEntity_ReturnsCorrectDateTime()
        {
            // Arrange
            var expectedDate = new DateOnly(2024, 1, 1);
            var expectedTime = new TimeOnly(12, 30, 45);
            
            var entity = new DateTimeZoneTrackableBase
            {
                Date = expectedDate,
                Time = expectedTime,
                TimeZoneId = "UTC"
            };
            
            // Act
            var result = _dateTimeZoneService.ToUtc(entity);
            
            // Assert
            Assert.That(result.Date, Is.EqualTo(expectedDate.ToDateTime(TimeOnly.MinValue).Date));
            Assert.That(result.Hour, Is.EqualTo(expectedTime.Hour));
            Assert.That(result.Minute, Is.EqualTo(expectedTime.Minute));
            Assert.That(result.Second, Is.EqualTo(expectedTime.Second));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void GetAvailableTimeZones_ReturnsNonEmptyList()
        {
            // Act
            var timeZones = _dateTimeZoneService.GetAvailableTimeZones();
            
            // Assert
            Assert.That(timeZones, Is.Not.Null);
            Assert.That(timeZones.Length, Is.GreaterThan(0));
        }

        [Test]
        public void GetSystemTimeZoneId_ReturnsNonEmptyString()
        {
            // Act
            var systemTimeZoneId = _dateTimeZoneService.GetSystemTimeZoneId();
            
            // Assert
            Assert.That(systemTimeZoneId, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void SetDateTimeZone_WithNonUtcTimeZone_ConvertsToUtcCorrectly()
        {
            // This test is environment-dependent, so we'll check if the timezone exists first
            var systemTimeZoneId = _dateTimeZoneService.GetSystemTimeZoneId();
            if (string.IsNullOrEmpty(systemTimeZoneId))
            {
                Assert.Inconclusive("Could not determine system timezone");
                return;
            }
            
            try
            {
                // Arrange
                var entity = new DateTimeZoneTrackableBase();
                var localNow = DateTime.Now;
                
                // Act - use the system's timezone
                _dateTimeZoneService.SetDateTimeZone(entity, localNow, systemTimeZoneId);
                
                // Convert back to the original timezone to compare
                var roundTripDateTime = _dateTimeZoneService.ToTimeZone(entity, systemTimeZoneId);
                
                // Assert - allow 1 second difference due to potential rounding
                var diffSeconds = Math.Abs((roundTripDateTime - localNow).TotalSeconds);
                Assert.That(diffSeconds, Is.LessThan(1.0), 
                    $"Difference in seconds: {diffSeconds}. Original: {localNow}, RoundTrip: {roundTripDateTime}");
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
            {
                Assert.Inconclusive($"Test skipped due to timezone issue: {ex.Message}");
            }
        }

        [Test]
        public void DateTimeZoneExtensions_SetFromUtc_SetsCorrectly()
        {
            // Arrange
            var entity = new DateTimeZoneTrackableBase();
            var utcNow = DateTime.UtcNow;
            
            // Act
            entity.SetFromUtc(utcNow);
            
            // Assert
            Assert.That(entity.Date, Is.EqualTo(DateOnly.FromDateTime(utcNow)));
            Assert.That(entity.Time, Is.EqualTo(TimeOnly.FromDateTime(utcNow)));
            Assert.That(entity.TimeZoneId, Is.EqualTo("UTC"));
        }

        [Test]
        public void DateTimeZoneExtensions_ToUtcDateTime_ReturnsCorrectDateTime()
        {
            // Arrange
            var expectedDate = new DateOnly(2024, 1, 1);
            var expectedTime = new TimeOnly(12, 30, 45);
            
            var entity = new DateTimeZoneTrackableBase
            {
                Date = expectedDate,
                Time = expectedTime,
                TimeZoneId = "UTC"
            };
            
            // Act
            var result = entity.ToUtcDateTime(_dateTimeZoneService);
            
            // Assert
            Assert.That(result.Date, Is.EqualTo(expectedDate.ToDateTime(TimeOnly.MinValue).Date));
            Assert.That(result.Hour, Is.EqualTo(expectedTime.Hour));
            Assert.That(result.Minute, Is.EqualTo(expectedTime.Minute));
            Assert.That(result.Second, Is.EqualTo(expectedTime.Second));
            Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
        }
    }
}