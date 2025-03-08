using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Speech;

public class SpeechService(SpeechConfig config)
{
	public async Task TextToSpeech(string text, string voice)
	{
		config.SpeechSynthesisVoiceName = voice;
		var synthesizer = new SpeechSynthesizer(config);
		var result = await synthesizer.SpeakTextAsync(text);
	}
	
	public async Task SpeechToTextFromFile(string path, string language)
	{
		config.SpeechRecognitionLanguage = language;
		var audioConfig = AudioConfig.FromWavFileInput(path);
		var recognizer = new SpeechRecognizer(config, audioConfig);
		var result = await recognizer.RecognizeOnceAsync();
	}
}