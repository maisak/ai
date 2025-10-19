using Azure.AI.OpenAI;
using Chat.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using System.ClientModel;

namespace Chat;

public static class DependencyInjection
{
	public static IServiceCollection AddAzureOpenAiServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<OpenAiSettings>(_ => new OpenAiSettings
		{
			Endpoint = config["AzureOpenAiSettings:Endpoint"]!,
			ApiKey = config["AzureOpenAiSettings:ApiKey"]!,
			Model = config["AzureOpenAiSettings:Model"]!
		});

		services.AddTransient<AzureOpenAiChatService>();
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

	public static IServiceCollection AddOpenAiServices(this IServiceCollection services, 
													   IConfiguration config, 
													   string model)
	{
		RegisterOpenAiSettings(services, config, model);
		
		services.AddTransient<OpenAiChatService>(sp =>
		{
			var settings = sp.GetRequiredKeyedService<OpenAiSettings>(model);
			var client = new ChatClient(settings.Model, settings.ApiKey);
			return new OpenAiChatService(client);
		});

		return services;
	}

	private static void RegisterOpenAiSettings(IServiceCollection services, IConfiguration config, string model)
	{
		if (services.All(sd => sd.ServiceType != typeof(OpenAiSettings) || sd.ServiceKey?.ToString() != model))
		{
			services.AddKeyedTransient<OpenAiSettings>(model,
				(_, _) => new OpenAiSettings
				{
					ApiKey = config[$"{nameof(OpenAiSettings)}:{model}:ApiKey"]!,
					Model = config[$"{nameof(OpenAiSettings)}:{model}:Model"]!	
				});
		}
	}
}