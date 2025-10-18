using Azure.AI.OpenAI;
using Chat.Config;
using OpenAI;
using OpenAI.Chat;

namespace Chat;

public class AzureOpenAiChatService(ChatClient client)
{
	private readonly List<ChatMessage> _history = [
		new SystemChatMessage(
			"""
			You are impersonating Samuel L. Jackson, reply as he would do, sometimes impersonating
			his famous roles.
			""")
	];
	
	public async Task ChatAsync()
	{
		ChatCompletion completion = await client.CompleteChatAsync(
		[
			new SystemChatMessage("You are a music adviser who is rude and always unhappy."),
			new UserChatMessage("I like Megadeth and electronic music. Could you advise me on some bands?")
		]);
		
		Console.WriteLine(completion.Content[0].Text);
	}
	
	public async Task ChatWithParametersAsync()
	{
		var chatHistory = new List<ChatMessage>
		{
			new SystemChatMessage("You are a music adviser who is rude and always unhappy."),
			new UserChatMessage("I like Megadeth and electronic music. Could you advise me on some bands?")
		};
		ChatCompletion completion = await client.CompleteChatAsync(chatHistory, new ChatCompletionOptions()
		{
			Temperature = 0.9f,
			MaxOutputTokenCount = 400
		});
		
		Console.WriteLine(completion.Content[0].Text);
	}
	
	public string Reply(string question)
	{
		_history.Add(new UserChatMessage(question));

		var completion = client.CompleteChat(_history);
		var reply = completion.Value.Content[0].Text;
		
		_history.Add(new AssistantChatMessage(reply));
		
		return reply;
	}
}