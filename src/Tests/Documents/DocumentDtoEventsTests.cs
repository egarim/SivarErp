using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using NUnit.Framework;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Tests.Documents
{
    [TestFixture]
    public class DocumentDtoEventsTests
    {
        private DocumentDto _document;
        private List<PropertyChangedEventArgs> _propertyChangedEvents;

        [SetUp]
        public void Setup()
        {
            _document = new DocumentDto();
            _propertyChangedEvents = new List<PropertyChangedEventArgs>();
            _document.PropertyChanged += OnPropertyChanged;
        }

        [TearDown]
        public void TearDown()
        {
            _document.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _propertyChangedEvents.Add(e);
        }

        [Test]
        public void PropertyChanged_DateChanges_RaisesPropertyChangedEvent()
        {
            // Arrange
            var newDate = new DateOnly(2025, 6, 12);
            
            // Act
            _document.Date = newDate;
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("Date"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.PropertyChanged));
            Assert.That(args.NewValue, Is.EqualTo(newDate));
            Assert.That(args.Source, Is.SameAs(_document));
        }
        
        [Test]
        public void PropertyChanged_TimeChanges_RaisesPropertyChangedEvent()
        {
            // Arrange
            var newTime = new TimeOnly(14, 30);
            
            // Act
            _document.Time = newTime;
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("Time"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.PropertyChanged));
            Assert.That(args.NewValue, Is.EqualTo(newTime));
        }
        
        [Test]
        public void PropertyChanged_BusinessEntityChanges_RaisesPropertyChangedEvent()
        {
            // Arrange
            var businessEntity = new BusinessEntityDto { Name = "Test Company" };
            
            // Act
            _document.BusinessEntity = businessEntity;
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("BusinessEntity"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.PropertyChanged));
            Assert.That(args.NewValue, Is.SameAs(businessEntity));
        }
        
        [Test]
        public void NestedPropertyChanged_BusinessEntityPropertyChanges_RaisesNestedPropertyChangedEvent()
        {
            // Arrange
            var businessEntity = new BusinessEntityDto { Name = "Initial Name" };
            _document.BusinessEntity = businessEntity;
            _propertyChangedEvents.Clear(); // Clear the initial property changed event
            
            // Act
            businessEntity.Name = "Updated Name";
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("BusinessEntity"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.NestedPropertyChanged));
            Assert.That(args.PropertyPath, Is.EqualTo("BusinessEntity.Name"));
            Assert.That(args.NewValue, Is.EqualTo("Updated Name"));
        }
        
        [Test]
        public void CollectionChanged_AddingDocumentLine_RaisesCollectionItemAddedEvent()
        {
            // Arrange
            var documentLine = new DocumentLineDto { LineItem = "Test Item" };
            
            // Act
            _document.Lines.Add(documentLine);
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("Lines"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.CollectionItemAdded));
        }
        
        [Test]
        public void CollectionChanged_RemovingDocumentLine_RaisesCollectionItemRemovedEvent()
        {
            // Arrange
            var documentLine = new DocumentLineDto { LineItem = "Test Item" };
            _document.Lines.Add(documentLine);
            _propertyChangedEvents.Clear(); // Clear the initial add event
            
            // Act
            _document.Lines.RemoveAt(0);
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("Lines"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.CollectionItemRemoved));
        }
        
        [Test]
        public void CollectionChanged_ReplacingLines_RaisesCollectionReplacedEvent()
        {
            // Arrange
            var newLines = new List<IDocumentLine> { new DocumentLineDto { LineItem = "New Item" } };
            
            // Act
            _document.Lines = newLines;
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("Lines"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.CollectionReplaced));
        }
        
        [Test]
        public void NestedPropertyChanged_DocumentLinePropertyChanges_RaisesNestedPropertyChangedEvent()
        {
            // Arrange
            var documentLine = new DocumentLineDto { LineItem = "Initial Item" };
            _document.Lines.Add(documentLine);
            _propertyChangedEvents.Clear(); // Clear the initial add event
            
            // Act
            documentLine.LineItem = "Updated Item";
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("Lines"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.NestedPropertyChanged));
            Assert.That(args.PropertyPath, Contains.Substring("Lines[0].LineItem"));
            Assert.That(args.NewValue, Is.EqualTo("Updated Item"));
        }
        
        [Test]
        public void CollectionChanged_AddingDocumentTotal_RaisesCollectionItemAddedEvent()
        {
            // Arrange
            var total = new TotalDto { Concept = "Tax", Total = 100.0m };
            
            // Act
            _document.DocumentTotals.Add(total);
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1));
            
            var args = _propertyChangedEvents[0] as DocumentPropertyChangedEventArgs;
            Assert.That(args, Is.Not.Null);
            Assert.That(args.PropertyName, Is.EqualTo("DocumentTotals"));
            Assert.That(args.ChangeType, Is.EqualTo(ChangeType.CollectionItemAdded));
        }

        [Test]
        public void Unsubscribe_RemovingBusinessEntity_StopsRaisingEvents()
        {
            // Arrange
            var businessEntity = new BusinessEntityDto { Name = "Test Company" };
            _document.BusinessEntity = businessEntity;
            _propertyChangedEvents.Clear();
            
            // Act
            businessEntity.Name = "Updated Name";
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(1)); // Verify events are raised
            
            _document.BusinessEntity = null; // Remove business entity
            _propertyChangedEvents.Clear();
            businessEntity.Name = "Final Name"; // Change again
            
            // Assert
            Assert.That(_propertyChangedEvents.Count, Is.EqualTo(0)); // No events should be raised
        }
        
        #region Test Implementation Classes
        
    
        
        #endregion
    }
}