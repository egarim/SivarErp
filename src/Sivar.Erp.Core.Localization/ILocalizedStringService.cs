using Microsoft.Extensions.Localization;

namespace Sivar.Erp.Core.Localization;

/// <summary>
/// Interface for localized string resources
/// </summary>
public interface ILocalizedStringService
{
    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    string GetString(string key);
    
    /// <summary>
    /// Gets a localized string by key with parameters
    /// </summary>
    string GetString(string key, params object[] parameters);
    
    /// <summary>
    /// Gets a localized string from a specific resource type
    /// </summary>
    string GetString<T>(string key) where T : class;
    
    /// <summary>
    /// Gets a localized string from a specific resource type with parameters
    /// </summary>
    string GetString<T>(string key, params object[] parameters) where T : class;
    
    /// <summary>
    /// Changes the current culture
    /// </summary>
    void ChangeCulture(string culture);
    
    /// <summary>
    /// Gets the current culture
    /// </summary>
    string GetCurrentCulture();
    
    /// <summary>
    /// Gets all available cultures
    /// </summary>
    IEnumerable<string> GetAvailableCultures();
}

/// <summary>
/// Implementation of localized string service
/// </summary>
public class LocalizedStringService : ILocalizedStringService
{
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    private readonly IStringLocalizer _sharedLocalizer;
    private string _currentCulture = "es";

    public LocalizedStringService(IStringLocalizerFactory stringLocalizerFactory)
    {
        _stringLocalizerFactory = stringLocalizerFactory;
        _sharedLocalizer = _stringLocalizerFactory.Create("SharedResources", typeof(LocalizedStringService).Assembly.GetName().Name!);
    }

    public string GetString(string key)
    {
        return _sharedLocalizer[key];
    }

    public string GetString(string key, params object[] parameters)
    {
        return _sharedLocalizer[key, parameters];
    }

    public string GetString<T>(string key) where T : class
    {
        var localizer = _stringLocalizerFactory.Create(typeof(T));
        return localizer[key];
    }

    public string GetString<T>(string key, params object[] parameters) where T : class
    {
        var localizer = _stringLocalizerFactory.Create(typeof(T));
        return localizer[key, parameters];
    }

    public void ChangeCulture(string culture)
    {
        _currentCulture = culture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
        System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo(culture);
    }

    public string GetCurrentCulture()
    {
        return _currentCulture;
    }

    public IEnumerable<string> GetAvailableCultures()
    {
        return new[] { "es", "en" };
    }
}
