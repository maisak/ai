using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Search.Config;

namespace Search;

public static class DependencyInjection
{
	public static IServiceCollection AddSearchServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddOptionsWithValidateOnStart<SearchSettings>()
			.BindConfiguration(nameof(SearchSettings))
			.ValidateDataAnnotations();

		services.AddTransient<SearchIndexClient>(isp =>
		{
			var options = isp.GetRequiredService<IOptions<SearchSettings>>().Value;
			var endpoint = new Uri(options.Endpoint);
			var credential = new AzureKeyCredential(options.ApiKey);

			return new SearchIndexClient(endpoint, credential);
		});
		
		services.AddTransient<SearchClient>(isp =>
		{
			var options = isp.GetRequiredService<IOptions<SearchSettings>>().Value;
			var endpoint = new Uri(options.Endpoint);
			var credential = new AzureKeyCredential(options.ApiKey);

			return new SearchClient(endpoint, options.Index, credential);
		});
		
		services.AddTransient<SearchService>();
		
		return services;
	}
}