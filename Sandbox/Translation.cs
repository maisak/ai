using ContentSafety;
using Microsoft.Extensions.DependencyInjection;
using Translation;

namespace Sandbox;

public class Translation : SandboxBase
{
	[Fact]
	public async Task TranslateText()
	{
		var service = ServiceProvider.GetRequiredService<TranslationService>();
		await service.TranslateText("Hello, World!", "en", "uk");
	}
	
	[Fact]
	public async Task TransliterateText()
	{
		var service = ServiceProvider.GetRequiredService<TranslationService>();
		await service.TransliterateText(
			"""
			Берег моря. Чути розбещенi крики морських птахiв, ревiння моржа, 
			а також iншi звуки, iздаваємиє різною морською сволотою.
			""",
			"uk", "Cyrl", "Latn");
	}
	
	[Fact]
	public async Task GetSupportedLanguages()
	{
		var service = ServiceProvider.GetRequiredService<TranslationService>();
		await service.GetSupportedLanguages();
	}
}