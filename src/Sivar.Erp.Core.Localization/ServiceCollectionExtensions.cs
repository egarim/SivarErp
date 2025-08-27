using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Sivar.Erp.Core.Localization;

/// <summary>
/// Extension methods for service collection to register localization services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds localization services to the service collection
    /// </summary>
    public static IServiceCollection AddErpLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        services.AddSingleton<ILocalizedStringService, LocalizedStringService>();

        return services;
    }

    /// <summary>
    /// Configures request localization for web applications
    /// </summary>
    public static IServiceCollection AddErpRequestLocalization(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "es", "en" };
            options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("es");
            options.SupportedCultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
        });

        return services;
    }
}
