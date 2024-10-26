using Chat;
using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

public class Chat : SandboxBase
{
	[Fact]
	public async Task BaseChat()
	{
		var chat = ServiceProvider.GetRequiredService<ChatService>();
		await chat.ChatWithParametersAsync();
	}
}