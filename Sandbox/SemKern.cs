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
	
	[Fact]
	public async Task ImageDescription()
	{
		var chat = ServiceProvider.GetRequiredService<SemanticKernelService>();
		var bytes = await File.ReadAllBytesAsync("Files/Images/megadeth.png");
		await chat.ImageDescription(bytes);
	}
}