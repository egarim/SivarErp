using System.ComponentModel;
using System.Globalization;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.Core.Infrastructure.Localization
{
    /// <summary>
    /// In-memory implementation of localization service for ERP system
    /// Supports El Salvador (Spanish) and US (English) locales by default
    /// </summary>
    [Description("In-memory implementation of localization service")]
    public class InMemoryLocalizationService : ILocalizationService
    {
        private readonly ILogger<InMemoryLocalizationService> _logger;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _localizations;
        private string _currentCulture;

        /// <summary>
        /// Initializes the localization service with default localizations
        /// </summary>
        /// <param name="logger">Logger for the service</param>
        public InMemoryLocalizationService(ILogger<InMemoryLocalizationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizations = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
            _currentCulture = "es-SV"; // Default to El Salvador Spanish
            
            InitializeDefaultLocalizations();
        }

        /// <summary>
        /// Gets localized string for the specified key
        /// </summary>
        [Description("Gets localized string for the specified key")]
        public string GetString(string key, string? culture = null)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            var targetCulture = culture ?? _currentCulture;
            
            if (_localizations.TryGetValue(targetCulture, out var cultureStrings) &&
                cultureStrings.TryGetValue(key, out var localizedString))
            {
                return localizedString;
            }

            // Fallback to English if not found in target culture
            if (targetCulture != "en-US" && 
                _localizations.TryGetValue("en-US", out var englishStrings) &&
                englishStrings.TryGetValue(key, out var englishString))
            {
                _logger.LogWarning("Localization key '{Key}' not found for culture '{Culture}', using English fallback", key, targetCulture);
                return englishString;
            }

            _logger.LogWarning("Localization key '{Key}' not found for any culture", key);
            return key; // Return key as fallback
        }

        /// <summary>
        /// Gets localized string with format parameters
        /// </summary>
        [Description("Gets localized string with format parameters")]
        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Failed to format localized string for key '{Key}'", key);
                return format;
            }
        }

        /// <summary>
        /// Sets the current culture for localization
        /// </summary>
        [Description("Sets the current culture")]
        public void SetCulture(string culture)
        {
            if (string.IsNullOrEmpty(culture))
                throw new ArgumentException("Culture cannot be null or empty", nameof(culture));

            if (!_localizations.ContainsKey(culture))
            {
                _logger.LogWarning("Culture '{Culture}' not available, keeping current culture '{CurrentCulture}'", culture, _currentCulture);
                return;
            }

            _currentCulture = culture;
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            
            _logger.LogInformation("Culture changed to '{Culture}'", culture);
        }

        /// <summary>
        /// Gets the current culture code
        /// </summary>
        [Description("Gets the current culture code")]
        public string GetCurrentCulture()
        {
            return _currentCulture;
        }

        /// <summary>
        /// Gets all available cultures
        /// </summary>
        [Description("Gets all available cultures")]
        public IEnumerable<string> GetAvailableCultures()
        {
            return _localizations.Keys.ToList();
        }

        /// <summary>
        /// Checks if a localization key exists
        /// </summary>
        [Description("Checks if a localization key exists")]
        public bool HasKey(string key, string? culture = null)
        {
            var targetCulture = culture ?? _currentCulture;
            return _localizations.TryGetValue(targetCulture, out var cultureStrings) &&
                   cultureStrings.ContainsKey(key);
        }

        /// <summary>
        /// Initializes default localizations for El Salvador and US
        /// </summary>
        private void InitializeDefaultLocalizations()
        {
            // El Salvador Spanish (es-SV)
            var esSV = new ConcurrentDictionary<string, string>();
            
            // Common UI strings
            esSV["Common.Save"] = "Guardar";
            esSV["Common.Cancel"] = "Cancelar";
            esSV["Common.Delete"] = "Eliminar";
            esSV["Common.Edit"] = "Editar";
            esSV["Common.Create"] = "Crear";
            esSV["Common.Search"] = "Buscar";
            esSV["Common.Loading"] = "Cargando...";
            esSV["Common.Error"] = "Error";
            esSV["Common.Success"] = "Éxito";
            esSV["Common.Warning"] = "Advertencia";
            esSV["Common.Information"] = "Información";
            
            // Accounting terms
            esSV["Accounting.Transaction"] = "Transacción";
            esSV["Accounting.Account"] = "Cuenta";
            esSV["Accounting.DebitAmount"] = "Debe";
            esSV["Accounting.CreditAmount"] = "Haber";
            esSV["Accounting.Balance"] = "Saldo";
            esSV["Accounting.JournalEntry"] = "Asiento Contable";
            esSV["Accounting.GeneralLedger"] = "Libro Mayor";
            esSV["Accounting.ChartOfAccounts"] = "Plan de Cuentas";
            
            // Tax terms
            esSV["Tax.IVA"] = "IVA";
            esSV["Tax.TaxRate"] = "Tasa de Impuesto";
            esSV["Tax.TaxableAmount"] = "Monto Gravable";
            esSV["Tax.TaxAmount"] = "Monto del Impuesto";
            esSV["Tax.ExemptAmount"] = "Monto Exento";
            
            // Business entities
            esSV["BusinessEntity.Customer"] = "Cliente";
            esSV["BusinessEntity.Supplier"] = "Proveedor";
            esSV["BusinessEntity.Company"] = "Empresa";
            
            // Documents
            esSV["Document.Invoice"] = "Factura";
            esSV["Document.Receipt"] = "Recibo";
            esSV["Document.CreditNote"] = "Nota de Crédito";
            esSV["Document.DebitNote"] = "Nota de Débito";
            esSV["Document.PurchaseOrder"] = "Orden de Compra";
            esSV["Document.SalesOrder"] = "Orden de Venta";
            
            _localizations["es-SV"] = esSV;

            // US English (en-US)
            var enUS = new ConcurrentDictionary<string, string>();
            
            // Common UI strings
            enUS["Common.Save"] = "Save";
            enUS["Common.Cancel"] = "Cancel";
            enUS["Common.Delete"] = "Delete";
            enUS["Common.Edit"] = "Edit";
            enUS["Common.Create"] = "Create";
            enUS["Common.Search"] = "Search";
            enUS["Common.Loading"] = "Loading...";
            enUS["Common.Error"] = "Error";
            enUS["Common.Success"] = "Success";
            enUS["Common.Warning"] = "Warning";
            enUS["Common.Information"] = "Information";
            
            // Accounting terms
            enUS["Accounting.Transaction"] = "Transaction";
            enUS["Accounting.Account"] = "Account";
            enUS["Accounting.DebitAmount"] = "Debit";
            enUS["Accounting.CreditAmount"] = "Credit";
            enUS["Accounting.Balance"] = "Balance";
            enUS["Accounting.JournalEntry"] = "Journal Entry";
            enUS["Accounting.GeneralLedger"] = "General Ledger";
            enUS["Accounting.ChartOfAccounts"] = "Chart of Accounts";
            
            // Tax terms
            enUS["Tax.IVA"] = "VAT";
            enUS["Tax.TaxRate"] = "Tax Rate";
            enUS["Tax.TaxableAmount"] = "Taxable Amount";
            enUS["Tax.TaxAmount"] = "Tax Amount";
            enUS["Tax.ExemptAmount"] = "Exempt Amount";
            
            // Business entities
            enUS["BusinessEntity.Customer"] = "Customer";
            enUS["BusinessEntity.Supplier"] = "Supplier";
            enUS["BusinessEntity.Company"] = "Company";
            
            // Documents
            enUS["Document.Invoice"] = "Invoice";
            enUS["Document.Receipt"] = "Receipt";
            enUS["Document.CreditNote"] = "Credit Note";
            enUS["Document.DebitNote"] = "Debit Note";
            enUS["Document.PurchaseOrder"] = "Purchase Order";
            enUS["Document.SalesOrder"] = "Sales Order";
            
            _localizations["en-US"] = enUS;

            _logger.LogInformation("Initialized localization for cultures: {Cultures}", 
                string.Join(", ", _localizations.Keys));
        }

        /// <summary>
        /// Adds or updates a localization entry
        /// </summary>
        /// <param name="culture">Culture code</param>
        /// <param name="key">Localization key</param>
        /// <param name="value">Localized value</param>
        [Description("Adds or updates a localization entry")]
        public void SetLocalization(
            [Description("Culture code")] string culture,
            [Description("Localization key")] string key,
            [Description("Localized value")] string value)
        {
            var cultureStrings = _localizations.GetOrAdd(culture, _ => new ConcurrentDictionary<string, string>());
            cultureStrings.AddOrUpdate(key, value, (_, _) => value);
            
            _logger.LogDebug("Updated localization for culture '{Culture}', key '{Key}'", culture, key);
        }
    }
}
