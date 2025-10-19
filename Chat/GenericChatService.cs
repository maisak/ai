using Microsoft.Extensions.AI;

namespace Chat;

public class GenericChatService(IChatClient client, ChatOptions options)
{
	private readonly List<ChatMessage> _history = [
		new (ChatRole.System, "You are T-800, respond like it.")
	];
	
	public async Task<string> Reply(string question)
	{
		_history.Add(new (ChatRole.User, question));

		var completion = await client.GetResponseAsync(_history, options);
		var reply = completion.Text;
		
		_history.Add(new (ChatRole.Assistant, reply));
		
		return reply;
	}
	
}