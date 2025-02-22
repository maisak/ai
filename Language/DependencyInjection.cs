using Azure;
using Azure.AI.TextAnalytics;
using Language.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Language;

public static class DependencyInjection
{
	public static IServiceCollection AddLanguageServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<AzureAiServicesSettings>(_ => new AzureAiServicesSettings
		{
			Endpoint = config["Language:Endpoint"]!,
			ApiKey = config["Language:ApiKey"]!
		});

		services.AddTransient<LanguageService>();
		services.AddTransient<TextAnalyticsClient>(isp =>
		{
			var settings = isp.GetRequiredService<AzureAiServicesSettings>();
			return new TextAnalyticsClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
		});

		return services;
	}
}