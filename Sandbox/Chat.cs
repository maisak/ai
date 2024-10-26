using Chat;
using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

public class Chat : SandboxBase
{
	[Fact]
	public void BaseChat()
	{
		var chat = ServiceProvider.GetRequiredService<ChatService>();
	}
}