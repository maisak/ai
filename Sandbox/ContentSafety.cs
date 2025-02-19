using ContentSafety;
using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

public class ContentSafety: SandboxBase
{
	[Fact]
	public async Task Text_NoBlacklist()
	{
		var service = ServiceProvider.GetRequiredService<ContentSafetyService>();
		await service.DetectHarmfulContent("I hate you");
	}
	
	[Fact]
	public async Task Image()
	{
		var service = ServiceProvider.GetRequiredService<ContentSafetyService>();
		await service.AnalyzeImage(await File.ReadAllBytesAsync("Files/Images/virgin-killer.jpg"));
	}
}