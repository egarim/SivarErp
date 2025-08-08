using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.BusinessEntities;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects
{
    /// <summary>
    /// XAF persistent implementation of IBusinessEntity
    /// Represents business entities like customers, vendors, employees, etc.
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Master Data")]
    [DefaultProperty(nameof(Name))]
    [XafDisplayName("Business Entity")]
    [XafDefaultProperty(nameof(Name))]
    public class BusinessEntity : ErpBaseObject, IBusinessEntity
    {
        public BusinessEntity(Session session) : base(session) { }

        string code = string.Empty;
        /// <summary>
        /// Unique business entity code (e.g., customer code, vendor code)
        /// </summary>
        [RuleRequiredField("BusinessEntity_Code_Required", DefaultContexts.Save)]
        [RuleUniqueValue("BusinessEntity_Code_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Code")]
        [Index(0)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }

        string name = string.Empty;
        /// <summary>
        /// Business entity name or company name
        /// </summary>
        [RuleRequiredField("BusinessEntity_Name_Required", DefaultContexts.Save)]
        [Size(200)]
        [XafDisplayName("Name")]
        [Index(1)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string address = string.Empty;
        /// <summary>
        /// Street address
        /// </summary>
        [Size(500)]
        [XafDisplayName("Address")]
        [Index(2)]
        public string Address
        {
            get => address;
            set => SetPropertyValue(nameof(Address), ref address, value);
        }

        string city = string.Empty;
        /// <summary>
        /// City
        /// </summary>
        [Size(100)]
        [XafDisplayName("City")]
        [Index(3)]
        public string City
        {
            get => city;
            set => SetPropertyValue(nameof(City), ref city, value);
        }

        string state = string.Empty;
        /// <summary>
        /// State or province
        /// </summary>
        [Size(100)]
        [XafDisplayName("State/Province")]
        [Index(4)]
        public string State
        {
            get => state;
            set => SetPropertyValue(nameof(State), ref state, value);
        }

        string zipCode = string.Empty;
        /// <summary>
        /// ZIP or postal code
        /// </summary>
        [Size(20)]
        [XafDisplayName("ZIP/Postal Code")]
        [Index(5)]
        public string ZipCode
        {
            get => zipCode;
            set => SetPropertyValue(nameof(ZipCode), ref zipCode, value);
        }

        string country = string.Empty;
        /// <summary>
        /// Country
        /// </summary>
        [Size(100)]
        [XafDisplayName("Country")]
        [Index(6)]
        public string Country
        {
            get => country;
            set => SetPropertyValue(nameof(Country), ref country, value);
        }

        string phoneNumber = string.Empty;
        /// <summary>
        /// Primary phone number
        /// </summary>
        [Size(50)]
        [XafDisplayName("Phone Number")]
        [Index(7)]
        public string PhoneNumber
        {
            get => phoneNumber;
            set => SetPropertyValue(nameof(PhoneNumber), ref phoneNumber, value);
        }

        string email = string.Empty;
        /// <summary>
        /// Primary email address
        /// </summary>
        [RuleRegularExpression("BusinessEntity_Email_Valid", DefaultContexts.Save,
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            "Please enter a valid email address")]
        [Size(200)]
        [XafDisplayName("Email")]
        [Index(8)]
        public string Email
        {
            get => email;
            set => SetPropertyValue(nameof(Email), ref email, value);
        }

        bool isActive = true;
        /// <summary>
        /// Indicates whether this business entity is active
        /// </summary>
        [XafDisplayName("Active")]
        [Index(9)]
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        /// <summary>
        /// Returns a string representation of the business entity
        /// </summary>
        public override string ToString()
        {
            return $"{Code} - {Name}";
        }

        /// <summary>
        /// Validation to ensure either email or phone number is provided
        /// </summary>
        [RuleFromBoolProperty("BusinessEntity_ContactInfo_Required", DefaultContexts.Save,
            "Either Email or Phone Number must be provided")]
        public bool IsContactInfoProvided
        {
            get => !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(PhoneNumber);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsActive = true;
        }
    }
}