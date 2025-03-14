using Azure.AI.TextAnalytics;

namespace Language;

public class LanguageService(TextAnalyticsClient client)
{
	private const string Text = 
		"""
		RAG with Azure OpenAI allows developers to use supported AI chat models that can reference specific 
		sources of information to ground the response. Adding this information allows the model to reference 
		both the specific data provided and its pretrained knowledge to provide more effective responses.
		""";

	public async Task AnalyzeSentiment()
	{
		var response = await client.AnalyzeSentimentAsync(Text);
		var sentiment = response.Value.Sentiment;
	}

	public async Task GetKeyPhrases()
	{
		var response = await client.ExtractKeyPhrasesAsync(Text);
		var phrases = response.Value;
	}

	public async Task GetEntities()
	{
		var response = await client.RecognizeEntitiesAsync(Text);
		var entities = response.Value;
	}
}