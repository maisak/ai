using Azure.AI.OpenAI;
using Chat.Config;
using OpenAI;
using OpenAI.Chat;

namespace Chat;

public class ChatService(ChatClient client)
{
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
}