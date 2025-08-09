using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using System;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Accounting
{
    /// <summary>
    /// XAF persistent implementation of IFiscalPeriod
    /// Represents fiscal periods for controlling transaction posting
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Accounting")]
    [DefaultProperty(nameof(Name))]
    [XafDisplayName("Fiscal Period")]
    [XafDefaultProperty(nameof(Name))]
    [Appearance("FiscalPeriod_ReadOnlyAuditFields", 
        TargetItems = "CreatedOn;CreatedBy;ModifiedOn;ModifiedBy;Version;InsertedAt;InsertedBy;UpdatedAt;UpdatedBy", 
        Enabled = false)]
    [Appearance("FiscalPeriod_ClosedPeriod", 
        TargetItems = "*", 
        Criteria = "Status = 'Closed'",
        BackColor = "LightGray",
        FontColor = "Gray")]
    [Appearance("FiscalPeriod_ClosedPeriodReadOnly", 
        TargetItems = "Code;Name;StartDate;EndDate;Description", 
        Criteria = "Status = 'Closed'",
        Enabled = false)]
    public class FiscalPeriod : ErpBaseObject, IFiscalPeriod
    {
        public FiscalPeriod(Session session) : base(session) { }

        string code = string.Empty;
        /// <summary>
        /// Unique code for the fiscal period
        /// </summary>
        [RuleRequiredField("FiscalPeriod_Code_Required", DefaultContexts.Save)]
        [RuleUniqueValue("FiscalPeriod_Code_Unique", DefaultContexts.Save)]
        [Size(20)]
        [XafDisplayName("Code")]
        [Index(0)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value?.ToUpperInvariant());
        }

        string name = string.Empty;
        /// <summary>
        /// Name of the fiscal period
        /// </summary>
        [RuleRequiredField("FiscalPeriod_Name_Required", DefaultContexts.Save)]
        [Size(100)]
        [XafDisplayName("Name")]
        [Index(1)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        DateOnly startDate;
        /// <summary>
        /// Start date of the fiscal period
        /// </summary>
        [RuleRequiredField("FiscalPeriod_StartDate_Required", DefaultContexts.Save)]
        [XafDisplayName("Start Date")]
        [Index(2)]
        public DateOnly StartDate
        {
            get => startDate;
            set => SetPropertyValue(nameof(StartDate), ref startDate, value);
        }

        DateOnly endDate;
        /// <summary>
        /// End date of the fiscal period
        /// </summary>
        [RuleRequiredField("FiscalPeriod_EndDate_Required", DefaultContexts.Save)]
        [XafDisplayName("End Date")]
        [Index(3)]
        public DateOnly EndDate
        {
            get => endDate;
            set => SetPropertyValue(nameof(EndDate), ref endDate, value);
        }

        FiscalPeriodStatus status = FiscalPeriodStatus.Open;
        /// <summary>
        /// Status of the fiscal period (Open or Closed)
        /// </summary>
        [XafDisplayName("Status")]
        [Index(4)]
        public FiscalPeriodStatus Status
        {
            get => status;
            set => SetPropertyValue(nameof(Status), ref status, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Description of the fiscal period
        /// </summary>
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(5)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        /// <summary>
        /// Interface implementation - UTC timestamp when the entity was created
        /// Maps to CreatedOn from ErpBaseObject
        /// </summary>
        [Browsable(false)]
        public DateTime InsertedAt 
        { 
            get => CreatedOn; 
            set => CreatedOn = value; 
        }

        /// <summary>
        /// Interface implementation - User who created the entity
        /// Maps to CreatedBy from ErpBaseObject
        /// </summary>
        [Browsable(false)]
        public string InsertedBy 
        { 
            get => CreatedBy; 
            set => CreatedBy = value; 
        }

        /// <summary>
        /// Interface implementation - UTC timestamp when the entity was last updated
        /// Maps to ModifiedOn from ErpBaseObject
        /// </summary>
        [Browsable(false)]
        public DateTime UpdatedAt 
        { 
            get => ModifiedOn; 
            set => ModifiedOn = value; 
        }

        /// <summary>
        /// Interface implementation - User who last updated the entity
        /// Maps to ModifiedBy from ErpBaseObject
        /// </summary>
        [Browsable(false)]
        public string UpdatedBy 
        { 
            get => ModifiedBy; 
            set => ModifiedBy = value; 
        }

        /// <summary>
        /// Opens the fiscal period for transaction posting
        /// </summary>
        public void Open()
        {
            if (Status == FiscalPeriodStatus.Open)
                return;

            Status = FiscalPeriodStatus.Open;
        }

        /// <summary>
        /// Closes the fiscal period to prevent further transaction posting
        /// </summary>
        public void Close()
        {
            if (Status == FiscalPeriodStatus.Closed)
                return;

            // TODO: Add validation to ensure all transactions in this period are posted
            // This could be implemented as a business rule or validation
            Status = FiscalPeriodStatus.Closed;
        }

        /// <summary>
        /// Checks if the given date falls within this fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if date is within the fiscal period</returns>
        public bool ContainsDate(DateOnly date)
        {
            return date >= StartDate && date <= EndDate;
        }

        /// <summary>
        /// Checks if transactions can be posted in this period
        /// </summary>
        public bool CanPostTransactions => Status == FiscalPeriodStatus.Open;

        /// <summary>
        /// Returns a string representation of the fiscal period
        /// </summary>
        public override string ToString()
        {
            return $"{Code} - {Name} ({StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}) [{Status}]";
        }

        /// <summary>
        /// Validation to ensure end date is after start date
        /// </summary>
        [RuleFromBoolProperty("FiscalPeriod_ValidDateRange", DefaultContexts.Save,
            "End date must be after start date")]
        public bool IsDateRangeValid
        {
            get => EndDate >= StartDate;
        }

        /// <summary>
        /// Validation to ensure fiscal period span is reasonable (not more than 5 years)
        /// </summary>
        [RuleFromBoolProperty("FiscalPeriod_ReasonableSpan", DefaultContexts.Save,
            "Fiscal period cannot span more than 5 years")]
        public bool IsSpanReasonable
        {
            get
            {
                if (StartDate == default || EndDate == default)
                    return true;
                
                var span = EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue);
                return span.TotalDays <= (5 * 365); // 5 years
            }
        }

        /// <summary>
        /// Validation to ensure closed periods cannot be modified
        /// </summary>
        [RuleFromBoolProperty("FiscalPeriod_CannotModifyClosed", DefaultContexts.Save,
            "Closed fiscal periods cannot be modified")]
        public bool CanModifyPeriod
        {
            get
            {
                // Allow modification if open or if we're just changing the status
                return Status == FiscalPeriodStatus.Open || IsDeleted;
            }
        }

        /// <summary>
        /// Validation to prevent overlapping fiscal periods
        /// </summary>
        [RuleFromBoolProperty("FiscalPeriod_NoOverlap", DefaultContexts.Save,
            "Fiscal period dates cannot overlap with existing periods")]
        public bool HasNoOverlap
        {
            get
            {
                if (StartDate == default || EndDate == default)
                    return true;

                // Check for overlapping periods in the database
                var overlappingPeriods = Session.Query<FiscalPeriod>()
                    .Where(fp => fp.Oid != this.Oid && // Exclude self
                                ((fp.StartDate <= EndDate && fp.EndDate >= StartDate))) // Check overlap
                    .Any();

                return !overlappingPeriods;
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Status = FiscalPeriodStatus.Open;
            
            // Set default date range to current year
            var currentYear = DateTime.Now.Year;
            StartDate = new DateOnly(currentYear, 1, 1);
            EndDate = new DateOnly(currentYear, 12, 31);
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            // Prevent modification of closed fiscal periods except for status changes
            if (Status == FiscalPeriodStatus.Closed && propertyName != nameof(Status) && !IsLoading && !IsDeleted)
            {
                SetPropertyValue(propertyName, ref oldValue, oldValue);
                throw new InvalidOperationException("Cannot modify closed fiscal period");
            }
        }
    }
}