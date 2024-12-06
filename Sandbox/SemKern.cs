using Microsoft.Extensions.DependencyInjection;
using SemKern;
using SemKern.Services;

namespace Sandbox;

public class SemKern : SandboxBase
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
		/*
		 * Logging for IChatCompletionService is not really working in the current version of the SDK.
		 * Going to return to this later.
		 */
		var chat = ServiceProvider.GetRequiredService<SemKernelWithAppInsights>();
		await chat.Chat("who is the chosen one?"); // IChatCompletionService logging is not full
		//await chat.InvokePrompt("who is the chosen four?"); // Kernel's InvokePrompt seems to have more logs sent
	}
}