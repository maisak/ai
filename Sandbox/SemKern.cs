using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SemKern.Prompts;
using SemKern.Services;
using Xunit.Abstractions;

namespace Sandbox;

public class SemKern(ITestOutputHelper output) : SandboxBase
{
	[Fact]
	public async Task BaseChat()
	{
		var chat = ServiceProvider.GetRequiredService<SemanticKernelService>();
		await chat.Chat("turn on the lights");
	}
	
	[Fact]
	public async Task StreamingChat()
	{
		var chat = ServiceProvider.GetRequiredService<SemanticKernelService>();
		await chat.SteamingChat("give me a short description of Megadeth music");
	}
	
	[Fact]
	public async Task ImageDescription()
	{
		var chat = ServiceProvider.GetRequiredService<SemanticKernelService>();
		var bytes = await File.ReadAllBytesAsync("Files/Images/megadeth.png");
		await chat.ImageDescription(bytes);
	}
	
	[Fact]
	public async Task ChatLoggedToApplicationInsights()
	{
		var chat = ServiceProvider.GetRequiredService<SemKernelWithAppInsights>();
		var result = await chat.Chat("who is the chosen one?");
		output.WriteLine(result);
	}
	
	[Fact]
	public async Task ChatCustomLogging()
	{
		var chat = ServiceProvider.GetRequiredService<SemKernelWithCustomLogger>();
		await chat.StreamingChat("turn off the lights");
	}
	
	[Fact]
	public async Task BasicChatWithTestableKernelWrapper()
	{
		var kernel = ServiceProvider.GetRequiredService<Kernel>();
		var promptRenderer = ServiceProvider.GetRequiredService<IPromptRenderer>();
		var service = await TestableKernelWrapper.CreateAsync(kernel, promptRenderer);
		var r1 = await service.Chat("hi");
		var r2 = await service.Chat("what are you?");
		var r3 = await service.Chat("what music do you specialize on?");
		var r4 = await service.Chat("your opinion on doom metal?");
		var r5 = await service.Chat("do you have any Pantera albums?");
		var r6 = await service.Chat("I would like to buy Spice Girls cd");
	}
}