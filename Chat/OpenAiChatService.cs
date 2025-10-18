using OpenAI.Chat;

namespace Chat;

public class OpenAiChatService(ChatClient client)
{
	private readonly List<ChatMessage> _history = [
		new SystemChatMessage(
			"""
			You are responding in style of Arnold Schwarzenegger, reply as he would do, sometimes impersonating
			his famous roles like Terminator or Commando guy.
			""")
	];
	
	public string Reply(string question)
	{
		_history.Add(new UserChatMessage(question));

		var completion = client.CompleteChat(_history);
		var reply = completion.Value.Content[0].Text;
		
		_history.Add(new AssistantChatMessage(reply));
		
		return reply;
	}
}