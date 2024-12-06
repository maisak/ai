using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemKern.Config;

namespace SemKern;

public static class DependencyInjection
{
	public static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<OpenAiSettings>(_ => new OpenAiSettings
		{
			Endpoint = config["OpenAiSettings:Endpoint"]!,
			ApiKey = config["OpenAiSettings:ApiKey"]!,
			Model = config["OpenAiSettings:Model"]!
		});
		
		services.AddTransient<SemanticKernelService>();
		
		return services;
	}
}