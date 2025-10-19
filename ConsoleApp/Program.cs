using Chat;
using Chat.Config;
using Chat.Tools;
using ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemKern;

var config = Configuration.InitConfiguration();
var serviceProvider = CreateServiceProvider(config);

/* experimentation zone */

var chat = serviceProvider.GetRequiredService<GenericChatService>();

// read user input and pass it to the chat service in a cycle
while (true)
{
	Console.Write("You: ");
	var input = Console.ReadLine();
	if (string.IsNullOrWhiteSpace(input))
		break;
	Console.ForegroundColor = ConsoleColor.Yellow;
	var response = await chat.Reply(input);
	Console.WriteLine("Bot: " + response);
	Console.ResetColor();
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
	
	serviceCollection
		.AddGenericChatServices(configuration, "Gpt5Mini")
		.WithTool<DiceRoller>()
		.Build();
	
	serviceCollection.AddSingleton(configuration);
	serviceCollection.AddSemanticKernelServices(configuration);

	return serviceCollection.BuildServiceProvider();
}