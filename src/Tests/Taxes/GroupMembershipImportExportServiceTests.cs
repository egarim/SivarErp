using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Services.Taxes.TaxGroup;

namespace Tests.Taxes
{
    /// <summary>
    /// Tests for the group membership import/export service
    /// </summary>
    [TestFixture]
    public class GroupMembershipImportExportServiceTests
    {
        private IGroupMembershipImportExportService _importExportService;

        [SetUp]
        public void Setup()
        {
            _importExportService = new GroupMembershipImportExportService();
        }

        #region Import Tests

        [Test]
        public async Task ImportFromCsvAsync_EmptyContent_ReturnsError()
        {
            // Arrange
            string csvContent = string.Empty;

            // Act
            var (importedMemberships, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedMemberships, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("empty"));
        }

        [Test]
        public async Task ImportFromCsvAsync_HeaderOnly_ReturnsError()
        {
            // Arrange
            string csvContent = "GroupId,EntityId,GroupType";

            // Act
            var (importedMemberships, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedMemberships, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("no data"));
        }

        [Test]
        public async Task ImportFromCsvAsync_MissingRequiredHeader_ReturnsError()
        {
            // Arrange
            string csvContent = "GroupId,EntityId\nGROUP1,ENTITY1";

            // Act
            var (importedMemberships, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedMemberships, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("GroupType"));
        }

        [Test]
        public async Task ImportFromCsvAsync_ValidContent_ImportsMemberships()
        {
            // Arrange
            string csvContent = "GroupId,EntityId,GroupType\nGROUP1,ENTITY1,BusinessEntity\nGROUP2,ITEM1,Item";

            // Act
            var (importedMemberships, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedMemberships.Count(), Is.EqualTo(2));
            
            var memberships = importedMemberships.ToList();
            Assert.That(memberships[0].GroupId, Is.EqualTo("GROUP1"));
            Assert.That(memberships[0].EntityId, Is.EqualTo("ENTITY1"));
            Assert.That(memberships[0].GroupType, Is.EqualTo(GroupType.BusinessEntity));
            
            Assert.That(memberships[1].GroupId, Is.EqualTo("GROUP2"));
            Assert.That(memberships[1].EntityId, Is.EqualTo("ITEM1"));
            Assert.That(memberships[1].GroupType, Is.EqualTo(GroupType.Item));
        }

        [Test]
        public async Task ImportFromCsvAsync_InvalidGroupType_ValidatesAndReturnsError()
        {
            // Arrange
            string csvContent = "GroupId,EntityId,GroupType\nGROUP1,ENTITY1,InvalidType";

            // Act
            var (importedMemberships, errors) = await _importExportService.ImportFromCsvAsync(csvContent, "TestUser");

            // Assert
            Assert.That(importedMemberships, Is.Empty);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(errors.First(), Does.Contain("validation failed"));
        }

        #endregion

        #region Export Tests

        [Test]
        public async Task ExportToCsvAsync_EmptyCollection_ReturnsOnlyHeader()
        {
            // Act
            var csv = await _importExportService.ExportToCsvAsync(new GroupMembershipDto[0]);

            // Assert
            Assert.That(csv, Is.Not.Null);
            Assert.That(csv.Trim(), Is.EqualTo("Oid,GroupId,EntityId,GroupType"));
        }

        [Test]
        public async Task ExportToCsvAsync_WithData_ReturnsHeaderAndData()
        {
            // Arrange
            var memberships = new[]
            {
                new GroupMembershipDto 
                { 
                    Oid = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                    GroupId = "GROUP1", 
                    EntityId = "ENTITY1", 
                    GroupType = GroupType.BusinessEntity 
                },
                new GroupMembershipDto 
                { 
                    Oid = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                    GroupId = "GROUP2", 
                    EntityId = "ITEM1", 
                    GroupType = GroupType.Item 
                }
            };

            // Act
            var csv = await _importExportService.ExportToCsvAsync(memberships);
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Assert
            Assert.That(lines.Length, Is.GreaterThanOrEqualTo(3)); // Header + 2 data rows + possible empty line
            Assert.That(lines[0], Is.EqualTo("Oid,GroupId,EntityId,GroupType"));
            Assert.That(lines[1], Does.Contain("11111111-1111-1111-1111-111111111111"));
            Assert.That(lines[1], Does.Contain("GROUP1"));
            Assert.That(lines[1], Does.Contain("ENTITY1"));
            Assert.That(lines[1], Does.Contain("BusinessEntity"));
            Assert.That(lines[2], Does.Contain("22222222-2222-2222-2222-222222222222"));
            Assert.That(lines[2], Does.Contain("GROUP2"));
            Assert.That(lines[2], Does.Contain("ITEM1"));
            Assert.That(lines[2], Does.Contain("Item"));
        }

        #endregion

        #region Round Trip Tests

        [Test]
        public async Task RoundTrip_ExportThenImport_PreservesData()
        {
            // Arrange
            var originalMemberships = new[]
            {
                new GroupMembershipDto 
                { 
                    Oid = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                    GroupId = "GROUP1", 
                    EntityId = "ENTITY1", 
                    GroupType = GroupType.BusinessEntity 
                },
                new GroupMembershipDto 
                { 
                    Oid = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                    GroupId = "GROUP2", 
                    EntityId = "ITEM1", 
                    GroupType = GroupType.Item 
                }
            };

            // Act - Export
            var csv = await _importExportService.ExportToCsvAsync(originalMemberships);
            
            // Act - Import
            var (importedMemberships, errors) = await _importExportService.ImportFromCsvAsync(csv, "TestUser");

            // Assert
            Assert.That(errors, Is.Empty);
            Assert.That(importedMemberships.Count(), Is.EqualTo(2));
            
            var memberships = importedMemberships.ToList();
            Assert.That(memberships[0].Oid, Is.EqualTo(Guid.Parse("11111111-1111-1111-1111-111111111111")));
            Assert.That(memberships[0].GroupId, Is.EqualTo("GROUP1"));
            Assert.That(memberships[0].EntityId, Is.EqualTo("ENTITY1"));
            Assert.That(memberships[0].GroupType, Is.EqualTo(GroupType.BusinessEntity));
            
            Assert.That(memberships[1].Oid, Is.EqualTo(Guid.Parse("22222222-2222-2222-2222-222222222222")));
            Assert.That(memberships[1].GroupId, Is.EqualTo("GROUP2"));
            Assert.That(memberships[1].EntityId, Is.EqualTo("ITEM1"));
            Assert.That(memberships[1].GroupType, Is.EqualTo(GroupType.Item));
        }

        #endregion
    }
}