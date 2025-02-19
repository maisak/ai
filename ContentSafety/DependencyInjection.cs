using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContentSafety;

public static class DependencyInjection
{
	public static IServiceCollection AddContentSafetyServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<ContentSafetyService>();
		services.AddTransient<ContentSafetyClient>(_ => new ContentSafetyClient(
			new Uri(config["ContentSafety:Endpoint"]!), 
			new AzureKeyCredential(config["ContentSafety:ApiKey"]!)));
		services.AddTransient<BlocklistClient>(_ => new BlocklistClient(
			new Uri(config["ContentSafety:Endpoint"]!), 
			new AzureKeyCredential(config["ContentSafety:ApiKey"]!)));

		return services;
	}
}