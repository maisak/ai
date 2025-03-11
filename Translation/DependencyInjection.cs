using Azure;
using Azure.AI.Translation.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Translation.Config;

namespace Translation;

public static class DependencyInjection
{
	public static IServiceCollection AddTranslationServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<TranslationSettings>(_ => new TranslationSettings
		{
			Region = config["Translation:Region"]!,
			Key = config["Translation:Key"]!
		});

		services.AddTransient<TranslationService>();
		services.AddTransient<TextTranslationClient>(isp =>
		{
			var settings = isp.GetRequiredService<TranslationSettings>();
			return new TextTranslationClient(new AzureKeyCredential(settings.Key), settings.Region);
		});

		return services;
	}
}