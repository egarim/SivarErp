using System.ComponentModel;

namespace Sivar.Erp.Core.Infrastructure.Localization
{
    /// <summary>
    /// Service for managing multi-language support throughout the ERP system
    /// </summary>
    [Description("Service for managing multi-language support")]
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets localized string for the specified key
        /// </summary>
        /// <param name="key">Localization key</param>
        /// <param name="culture">Optional culture code (defaults to current culture)</param>
        /// <returns>Localized string or key if not found</returns>
        [Description("Gets localized string for the specified key")]
        string GetString(
            [Description("Localization key")] string key,
            [Description("Optional culture code")] string? culture = null);

        /// <summary>
        /// Gets localized string with format parameters
        /// </summary>
        /// <param name="key">Localization key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        [Description("Gets localized string with format parameters")]
        string GetString(
            [Description("Localization key")] string key,
            [Description("Format arguments")] params object[] args);

        /// <summary>
        /// Sets the current culture for localization
        /// </summary>
        /// <param name="culture">Culture code (e.g., 'es-SV', 'en-US')</param>
        [Description("Sets the current culture")]
        void SetCulture([Description("Culture code")] string culture);

        /// <summary>
        /// Gets the current culture code
        /// </summary>
        /// <returns>Current culture code</returns>
        [Description("Gets the current culture code")]
        string GetCurrentCulture();

        /// <summary>
        /// Gets all available cultures
        /// </summary>
        /// <returns>List of available culture codes</returns>
        [Description("Gets all available cultures")]
        IEnumerable<string> GetAvailableCultures();

        /// <summary>
        /// Checks if a localization key exists
        /// </summary>
        /// <param name="key">Localization key to check</param>
        /// <param name="culture">Optional culture code</param>
        /// <returns>True if key exists</returns>
        [Description("Checks if a localization key exists")]
        bool HasKey([Description("Localization key")] string key, [Description("Optional culture code")] string? culture = null);
    }
}
