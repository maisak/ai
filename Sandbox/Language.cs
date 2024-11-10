using Language;
using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

public class Language: SandboxBase
{
	[Fact]
	public async Task Sentiment()
	{
		var lang = ServiceProvider.GetRequiredService<LanguageService>();
		await lang.AnalyzeSentiment();
	}
	
	[Fact]
	public async Task KeyPhrases()
	{
		var lang = ServiceProvider.GetRequiredService<LanguageService>();
		await lang.GetKeyPhrases();
	}
	
	[Fact]
	public async Task Entities()
	{
		var lang = ServiceProvider.GetRequiredService<LanguageService>();
		await lang.GetEntities();
	}
}