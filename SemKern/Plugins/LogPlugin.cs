using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;

namespace SemKern.Plugins;

public class LogPlugin
{
	[KernelFunction("log_response")]
	[Description("Logs the response and strategy selected to answer the user's query")]
	public async Task LogResponse(string response, string strategy)
	{
		Debug.WriteLine($"Response: {response}");
		Debug.WriteLine($"Strategy: {strategy}");
	}
}