using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemKern.Config;
using SemKern.Logging;
using SemKern.Plugins;
using System.Text;

namespace SemKern.Services;

public class SemKernelWithCustomLogger
{
	private readonly DumpLoggingProvider _loggingProvider;
	private readonly Kernel _kernel;
	private readonly IChatCompletionService _chatCompletionService;
	private readonly ChatHistory _history;

	private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = new()
	{
		FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
	};

	public SemKernelWithCustomLogger(IOptions<OpenAiSettings> options, DumpLoggingProvider loggingProvider)
	{
		var settings = options.Value;
		_loggingProvider = loggingProvider;
		// Create a kernel with Azure OpenAI chat completion
		var builder = Kernel.CreateBuilder()
			.AddAzureOpenAIChatCompletion(settings.Model, settings.Endpoint, settings.ApiKey);

		// Add logging
		//var loggingProvider = new DumpLoggingProvider();
		builder.Services
			.AddLogging(x => x
				.SetMinimumLevel(LogLevel.Trace)
				.AddConsole()
				.AddProvider(loggingProvider))
			.ConfigureHttpClientDefaults(x => x
				.AddLogger(s => loggingProvider
					.CreateHttpRequestBodyLogger(s.GetRequiredService<ILogger<DumpLoggingProvider>>())));

		// Build the kernel
		_kernel = builder.Build();
		_chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
		_kernel.Plugins.AddFromType<LightsPlugin>("Lights");

		// Create a history store the conversation
		_history = [];
	}

	public async Task<string> Chat(string prompt)
	{
		// Add user input
		_history.AddUserMessage(prompt);

		// Get the response from the AI
		var result = await _chatCompletionService.GetChatMessageContentAsync(
			_history,
			executionSettings: _openAiPromptExecutionSettings,
			kernel: _kernel);

		// Print the results
		Console.WriteLine("Assistant > " + result);

		// Add the message from the agent to the chat history
		_history.AddMessage(result.Role, result.Content ?? string.Empty);

		return result.Content ?? string.Empty;
	}

	public async Task<string> StreamingChat(string prompt)
	{
		// Add user input
		_history.AddUserMessage(prompt);

		// Get the response from the AI
		var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(
			_history,
			executionSettings: _openAiPromptExecutionSettings,
			kernel: _kernel);

		var sb = new StringBuilder();
		// Print the results
		await foreach (var chunk in result)
		{
			sb.Append(chunk);
			Console.Write(chunk);
		}

		var logs = _loggingProvider.GetLlmCalls();

		return sb.ToString();
	}
}