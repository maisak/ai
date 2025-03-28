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

// read user input and pass it to the chat service in a cycle
while (true)
{
	Console.Write("You: ");
	var input = Console.ReadLine();
	if (string.IsNullOrWhiteSpace(input))
		break;
	var response = await chat.Chat(input);
	Console.WriteLine("Bot: " + response);
}

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