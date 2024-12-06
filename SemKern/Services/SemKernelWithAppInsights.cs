using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemKern.Config;

namespace SemKern.Services;

/// <summary>
/// Does not work as expected yet with IChatCompletionService and Application Insights 
/// https://learn.microsoft.com/en-us/semantic-kernel/concepts/enterprise-readiness/observability/telemetry-with-app-insights?tabs=Powershell&pivots=programming-language-csharp
/// </summary>
public class SemKernelWithAppInsights
{
	private readonly Kernel _kernel;
	private readonly IChatCompletionService _chatCompletionService;
	private readonly ChatHistory _history;
	private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings = new() 
	{
		FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
	};
	
	public SemKernelWithAppInsights(OpenAiSettings settings, ILoggerFactory loggerFactory)
	{
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
		_history.AddUserMessage(prompt);
		var result = await _chatCompletionService.GetChatMessageContentAsync(
			_history,
			executionSettings: _openAiPromptExecutionSettings,
			kernel: _kernel);
		Console.WriteLine("Assistant > " + result);
		_history.AddMessage(result.Role, result.Content ?? string.Empty);

		return result.Content ?? string.Empty;
	}
}