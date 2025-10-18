using Chat;
using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

public class Chat : SandboxBase
{
	[Fact]
	public async Task AzureOpenAi_Chat()
	{
		var chat = ServiceProvider.GetRequiredService<AzureOpenAiChatService>();
		var reply = chat.Reply("Howdy");
	}

	[Fact]
	public async Task OpenAi_Chat()
	{
		var chat = ServiceProvider.GetRequiredService<OpenAiChatService>();
		var reply = chat.Reply("Hi");
	}
}