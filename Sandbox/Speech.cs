using Microsoft.Extensions.DependencyInjection;
using Speech;

namespace Sandbox;

public class Speech: SandboxBase
{
	[Fact]
	public async Task SpeechToText_FromFile()
	{
		var service = ServiceProvider.GetRequiredService<SpeechService>();
		await service.SpeechToTextFromFile("Files/Audio/ukr-test.wav", "uk-UA");
	}
	
	[Fact]
	public async Task TextToSpeech()
	{
		var service = ServiceProvider.GetRequiredService<SpeechService>();
		await service.TextToSpeech(
			"""
			Кругом мене одні гандони, 
			я б лучшє разгружав вагони, 
			завжди воняв, як роботяга, 
			і був щасливий, як бродяга.
			""",
			"uk-UA-OstapNeural");
	}
}