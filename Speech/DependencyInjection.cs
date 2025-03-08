using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Speech.Config;

namespace Speech;

public static class DependencyInjection
{
	public static IServiceCollection AddSpeechServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<SpeechSettings>(_ => new SpeechSettings
		{
			Region = config["Speech:Region"]!,
			Key = config["Speech:Key"]!
		});

		services.AddTransient<SpeechService>();
		services.AddTransient<SpeechConfig>(isp =>
		{
			var settings = isp.GetRequiredService<SpeechSettings>();
			return SpeechConfig.FromSubscription(settings.Key, settings.Region);
		});

		return services;
	}
}