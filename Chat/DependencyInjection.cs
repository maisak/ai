using Chat.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat;

public static class DependencyInjection
{
	public static IServiceCollection AddChatServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<OpenAiSettings>(_ => new OpenAiSettings
		{
			Endpoint = config["OpenAiSettings:Endpoint"]!,
			ApiKey = config["OpenAiSettings:ApiKey"]!,
		});

		services.AddTransient<ChatService>();

		return services;
	}
}