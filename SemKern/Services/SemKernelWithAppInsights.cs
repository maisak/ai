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
	private readonly Kernel _kernel;
	private readonly IChatCompletionService _chatCompletionService;
	private readonly ChatHistory _history;
	private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = new() 
	{
		FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
	};
	private static readonly ActivitySource ActivitySource = new(nameof(SemKernelWithAppInsights));
	
	public SemKernelWithAppInsights(IOptions<OpenAiSettings> options, ILoggerFactory loggerFactory)
	{
		var settings = options.Value;
		var builder = Kernel.CreateBuilder();
		builder.Services.AddSingleton(loggerFactory);
		builder.AddAzureOpenAIChatCompletion(settings.Model, settings.Endpoint, settings.ApiKey);
		_kernel = builder.Build();
		_chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
		_history = [];
	}

	public async Task<string> InvokePrompt(string prompt)
	{
		var answer = await _kernel.InvokePromptAsync(prompt);
		return answer.ToString();
	}
	
	public async Task<string> Chat(string prompt)
	{
		using var activity = ActivitySource.StartActivity(); // 'using' makes sure the activity is stopped and disposed
		_history.AddUserMessage(prompt);
		var result = await _chatCompletionService.GetChatMessageContentAsync(
			_history,
			executionSettings: _openAiPromptExecutionSettings,
			kernel: _kernel);
		_history.AddMessage(result.Role, result.Content ?? string.Empty);

		return result.Content ?? string.Empty;
	}
}