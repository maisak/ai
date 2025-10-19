using Chat.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace Chat.Config;

public class GenericChatBuilder(IServiceCollection services, IConfiguration config)
{
	public GenericChatBuilder WithTool<T>() where T : class, IGenericTool
	{
		services.AddTransient<IGenericTool, T>();
		return this;
	}
	
	public void Build()
	{
		
	}
}

public static class GenericChatServiceCollectionExtensions
{
	public static GenericChatBuilder AddGenericChatServices(this IServiceCollection services, 
															IConfiguration config, 
															string model)
	{
		RegisterOpenAiSettings(services, config, model);
		services.AddSingleton<ILoggerFactory>(_ 
			=> LoggerFactory.Create(lf => lf.AddConsole().AddDebug()));
		services.AddTransient<GenericChatService>();
		services.AddTransient<IChatClient>(sp =>
		{
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			var settings = sp.GetRequiredKeyedService<OpenAiSettings>(model);
			var client = new ChatClient(settings.Model, settings.ApiKey).AsIChatClient();
			return new ChatClientBuilder(client)
				.UseLogging(loggerFactory)
				.UseFunctionInvocation(loggerFactory, c => c.IncludeDetailedErrors = true)
				.Build(sp);
		});

		services.AddTransient<ChatOptions>(sp =>
		{
			var settings = sp.GetRequiredKeyedService<OpenAiSettings>(model);
			return new ChatOptions
			{
				ModelId = settings.Model,
				Temperature = 1,
				MaxOutputTokens = 5000,
				Tools = [..FunctionsRegistry.GetTools(sp)]
			};
		});
		
		return new GenericChatBuilder(services, config);
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