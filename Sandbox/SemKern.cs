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
}