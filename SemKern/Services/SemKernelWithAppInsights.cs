using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemKern.Config;
using System.Diagnostics;

namespace SemKern.Services;

public class SemKernelWithAppInsights
{
	private const byte MaxHistoryLength = 4;
	private readonly Kernel _kernel;
	private readonly IChatCompletionService _chatCompletionService;
	private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = new() 
	{
		FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
	};
	private static readonly ActivitySource ActivitySource = new(nameof(SemKernelWithAppInsights));
	private readonly ChatHistorySummarizationReducer _historySummarizationReducer;
	private ChatHistory _history = new("You are a metalhead talk buddy. Behave accordingly.");
	
	public SemKernelWithAppInsights(IOptions<OpenAiSettings> options, ILoggerFactory loggerFactory)
	{
		var settings = options.Value;
		var builder = Kernel.CreateBuilder();
		builder.Services.AddSingleton(loggerFactory);
		builder.AddAzureOpenAIChatCompletion(settings.Model, settings.Endpoint, settings.ApiKey);
		_kernel = builder.Build();
		_chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
		_historySummarizationReducer = new ChatHistorySummarizationReducer(_chatCompletionService, MaxHistoryLength);
	}

	public async Task<string> InvokePrompt(string prompt)
	{
		var answer = await _kernel.InvokePromptAsync(prompt);
		return answer.ToString();
	}
	
	public async Task<string> Chat(string prompt)
	{
		using var activity = ActivitySource.StartActivity(); // 'using' makes sure the activity is stopped and disposed
		// summarize previous history
		if (_history.Count > MaxHistoryLength)
		{
			var reducedMessages = await _historySummarizationReducer.ReduceAsync(_history);
			if (reducedMessages is not null)
			{
				_history = new ChatHistory(reducedMessages);
			}
		}
		// continue the conversation
		_history.AddUserMessage(prompt);
		var result = await _chatCompletionService.GetChatMessageContentAsync(
			_history,
			executionSettings: _openAiPromptExecutionSettings,
			kernel: _kernel);
		_history.AddMessage(result.Role, result.Content ?? string.Empty);

		return result.Content ?? string.Empty;
	}
}