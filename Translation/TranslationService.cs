using Azure.AI.Translation.Text;

namespace Translation;

public class TranslationService(TextTranslationClient  client)
{
	public async Task TranslateText(string content, string from, string to)
	{
		var response = await client.TranslateAsync(to, content);
	}
	
	public async Task TransliterateText(string content, string fromLanguage, string fromScript, string toScript)
	{
		var response = await client.TransliterateAsync(fromLanguage, fromScript, toScript, content);
	}
	
	public async Task GetSupportedLanguages()
	{
		var response = await client.GetSupportedLanguagesAsync();
	}
}