using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemKern.Prompts;

namespace SemKern.Services;

public class TestableKernelWrapper
{
	private readonly Kernel _kernel;
	private readonly ChatHistory _history = [];
	private readonly IChatCompletionService _completionService;

	private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = new()
	{
		FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
	};

	private TestableKernelWrapper(Kernel kernel, string systemPrompt)
	{
		_kernel = kernel;
		_completionService = kernel.GetRequiredService<IChatCompletionService>();
		_history.AddMessage(AuthorRole.System, systemPrompt);
	}

	public static async Task<TestableKernelWrapper> CreateAsync(Kernel kernel, IPromptRenderer promptRenderer)
	{
		var systemPrompt = await promptRenderer
			.RenderSystemPrompt(kernel, "Homer");

		return new TestableKernelWrapper(kernel, systemPrompt);
	}

	public async Task<string> Chat(string prompt)
	{
		// Add user input
		_history.AddUserMessage(prompt);

		// Get the response from the AI
		var result = await _completionService.GetChatMessageContentAsync(
			_history,
			executionSettings: _openAiPromptExecutionSettings,
			kernel: _kernel);

		// Print the results
		Console.WriteLine("Assistant > " + result);

		// Add the message from the agent to the chat history
		_history.AddMessage(result.Role, result.Content ?? string.Empty);

		return result.Content ?? string.Empty;
	}
}