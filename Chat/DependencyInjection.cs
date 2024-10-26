using Azure.AI.OpenAI;
using Chat.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using System.ClientModel;

namespace Chat;

public static class DependencyInjection
{
	public static IServiceCollection AddChatServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<OpenAiSettings>(_ => new OpenAiSettings
		{
			Endpoint = config["OpenAiSettings:Endpoint"]!,
			ApiKey = config["OpenAiSettings:ApiKey"]!,
			Model = config["OpenAiSettings:Model"]!
		});

		services.AddTransient<ChatService>();
		services.AddTransient<AzureOpenAIClient>(isp =>
		{
			var settings = isp.GetRequiredService<OpenAiSettings>();
			return new AzureOpenAIClient(
				new Uri(settings.Endpoint),
				new ApiKeyCredential(settings.ApiKey));
		});
		services.AddTransient<ChatClient>(isp =>
		{
			var settings = isp.GetRequiredService<OpenAiSettings>();
			var client = isp.GetRequiredService<AzureOpenAIClient>();
			return client.GetChatClient(settings.Model);
		});

		return services;
	}
}