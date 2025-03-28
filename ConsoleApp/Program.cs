using ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemKern;
using SemKern.Services;

var config = Configuration.InitConfiguration();
var serviceProvider = CreateServiceProvider(config);

/* experimentation zone */

var chat = serviceProvider.GetRequiredService<SemKernelWithAppInsights>();
var response = await chat.InvokePrompt("random Megadeth album to listen to?");
Console.WriteLine(response);

/* end of experimentation zone */
Console.ReadKey();
return;

IServiceProvider CreateServiceProvider(IConfiguration configuration)
{
	var serviceCollection =
		new ServiceCollection()
			.AddLogging(builder =>
				builder
					.ClearProviders()
					.AddConsole());
		
	serviceCollection.AddSingleton(configuration);
	serviceCollection.AddSemanticKernelServices(configuration);

	return serviceCollection.BuildServiceProvider();
}