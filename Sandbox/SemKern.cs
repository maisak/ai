using Microsoft.Extensions.DependencyInjection;
using SemKern;

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
}