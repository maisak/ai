using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace SemKern.Logging;

public class DumpLoggingProvider : ILoggerProvider
{
	private readonly AccumulatorLogger _logger;
	private RequestBodyLogger? _httpRequestBodyLogger;

	private static DumpLoggingProvider? _instance;

	public DumpLoggingProvider()
	{
		_logger = new AccumulatorLogger();
		_instance = this;
	}

	public IHttpClientAsyncLogger CreateHttpRequestBodyLogger(ILogger logger) => _httpRequestBodyLogger = new RequestBodyLogger(logger);

	public ILogger CreateLogger(string categoryName) => _logger;

	public void Dispose() { }

	internal IEnumerable<LlmCall> GetLlmCalls() => _logger.GetLlmCalls();

	private class AccumulatorLogger : ILogger
	{
		private readonly List<string> _logs = [];
		private readonly List<LlmCall> _llmCalls = [];

		public IEnumerable<LlmCall> GetLlmCalls() => _llmCalls;

		public void AddLlmCall(LlmCall llmCall)
		{
			_llmCalls.Add(llmCall);
		}

		internal LlmCall? CompleteLlmCall(string correlationId, string function, string arguments, string response)
		{
			for (var i = _llmCalls.Count - 1; i >= 0; i--)
			{
				var llmCall = _llmCalls[i];
				if (llmCall.CorrelationKey == correlationId)
				{
					llmCall.Response = response;
					llmCall.ResponseFunctionCall = function;
					llmCall.ResponseFunctionCallParameters = arguments;
					llmCall.CallEnd = DateTime.UtcNow;
					return llmCall;
				}
			}

			return null;
		}

		public IDisposable BeginScope<TState>(TState state) where TState : notnull
		{
			return new LogScope(state);
		}

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
			Func<TState, Exception, string> formatter)
		{
			var stateDictionary = ExtractDictionaryFromState(state);
			var interfaces = state!.GetType().GetInterfaces().Select(i => i.Name).ToList();
			_logs.Add(formatter(state, exception!));
		}

		private static Dictionary<string, object> ExtractDictionaryFromState<TState>(TState state)
		{
			Dictionary<string, object> retValue = new();
			if (state is IEnumerable en)
			{
				foreach (var element in en)
				{
					if (element is KeyValuePair<string, object> stateValue)
					{
						retValue[stateValue.Key] = stateValue.Value;
					}
				}
			}

			return retValue;
		}

		public List<string> GetLogs()
		{
			return _logs;
		}
	}

	private class LogScope(object state) : IDisposable
	{
		private object _state = state;

		public void Dispose()
		{
		}
	}

	private sealed class RequestBodyLogger(ILogger logger) : IHttpClientAsyncLogger
	{
		public async ValueTask<object?> LogRequestStartAsync(HttpRequestMessage request,
			CancellationToken cancellationToken = default)
		{
			if (request.Content == null)
			{
				//nothing to do.
				return default;
			}

			var requestContent = await request.Content!.ReadAsStringAsync(cancellationToken);
			StringBuilder sb = new();

			if (request.RequestUri!.Host.Contains("openai"))
			{
				// I need to pase the request content as json object to extract some informations.
				var jsonObject = JsonDocument.Parse(requestContent).RootElement;
				var messages = jsonObject.GetProperty("messages");

				sb.AppendLine($"Call LLM: {request.RequestUri}");

				foreach (var message in messages.EnumerateArray())
				{
					var content = message.GetProperty("content").GetString();
					sb.AppendLine($"{message.GetProperty("role").GetString()}: {content}");
				}

				if (jsonObject.TryGetProperty("tools", out var tools))
				{
					sb.AppendLine("Functions:");
					foreach (JsonElement tool in tools.EnumerateArray())
					{
						// Extracting function object
						JsonElement function = tool.GetProperty("function");

						// Extracting function name and description
						string functionName = function.GetProperty("name").GetString() ?? string.Empty;
						string functionDescription = function.GetProperty("description").GetString() ?? string.Empty;

						sb.AppendLine($"Function Name: {functionName}");
						sb.AppendLine($"Description: {functionDescription}");

						// Extracting parameters
						JsonElement parameters = function.GetProperty("parameters");
						foreach (JsonProperty parameter in parameters.EnumerateObject())
						{
							sb.AppendLine($"Parameter name {parameter.Name} Value; {parameter.Value}");
						}

						sb.AppendLine();
					}
				}

				foreach (var header in request.Headers)
				{
					if (!header.Key.Contains("key", StringComparison.OrdinalIgnoreCase))
					{
						sb.AppendLine($"{header.Key}: {header.Value.First()}");
					}
				}

				LlmCall lLMCall = new LlmCall()
				{
					Url = request.RequestUri.ToString(),
					CorrelationKey = request.Headers.GetValues("x-ms-client-request-id").First(),
					Prompt = jsonObject.GetProperty("messages").ToString(),
					FullRequest = jsonObject.ToString(),
					PromptFunctions = tools.ToString(),
					CallStart = DateTime.UtcNow
				};
				DumpLoggingProvider._instance!._logger.AddLlmCall(lLMCall);
			}
			else
			{
				sb.AppendLine($"Call HTTP: {request.RequestUri}");
				sb.AppendLine("CONTENT:");
				sb.AppendLine(requestContent);
			}

			logger.LogTrace(sb.ToString());
			return default;
		}

		public object? LogRequestStart(HttpRequestMessage request)
		{
			var requestContent = request.Content!.ReadAsStringAsync().Result;

			logger.LogTrace("Request: {Request}", request);
			logger.LogTrace("Request content: {Content}", requestContent);
			return default;
		}

		public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response,
			TimeSpan elapsed)
		{
			var responseContent = response.Content.ReadAsStringAsync().Result;
			logger.LogTrace("Response: {Response}", response);
			logger.LogTrace("Response content: {Content}", responseContent);
		}

		public ValueTask LogRequestStopAsync(object? context, HttpRequestMessage request, HttpResponseMessage response,
			TimeSpan elapsed, CancellationToken cancellationToken = default)
		{
			var responseContent = response.Content.ReadAsStringAsync().Result;

			// Rewind the response content stream
			var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
			response.Content = new StreamContent(contentStream);
			contentStream.Position = 0;
			
			var sb = new StringBuilder();
			var functions = GetFunctionInformation(responseContent);
			if (functions.Function != null)
			{
				sb.AppendLine($"Call function {functions.Function} with arguments {functions.Arguments}");
			}

			sb.AppendLine($"Response: {response}");
			sb.AppendLine($"Response content: {responseContent}");

			if (request.RequestUri!.Host.Contains("openai"))
			{
				var correlationId = response.Headers.GetValues("x-ms-client-request-id").First();
				var llmCall = DumpLoggingProvider._instance!._logger.CompleteLlmCall(correlationId, functions.Function!,
					functions.Arguments!, responseContent);
				if (llmCall != null)
				{
					logger.LogTrace(llmCall.Dump());
					return ValueTask.CompletedTask;
				}
			}

			logger.LogTrace(sb.ToString());
			return ValueTask.CompletedTask;
		}

		private static (string? Function, string? Arguments) GetFunctionInformation(string responseContent)
		{
			try
			{
				var root = JsonDocument.Parse(responseContent);
				var functionInfo = root.RootElement
					.GetProperty("choices")[0]
					.GetProperty("message")
					.GetProperty("tool_calls")[0]
					.GetProperty("function");

				var functionName = functionInfo.GetProperty("name").GetString() ?? string.Empty;
				var arguments = functionInfo.GetProperty("arguments").GetString() ?? string.Empty;

				return (functionName, arguments);
			}
			catch (Exception)
			{
				return (null, null);
			}
		}

		public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response,
			Exception exception, TimeSpan elapsed)
		{
		}

		public ValueTask LogRequestFailedAsync(object? context, HttpRequestMessage request,
			HttpResponseMessage? response, Exception exception, TimeSpan elapsed,
			CancellationToken cancellationToken = default) => default;
	}
}

public class LlmCall
{
	public  string? Url { get; set; }

	public string? CorrelationKey { get; set; }

	public string? Prompt { get; set; }

	/// <summary>
	/// Full RAW request made by semantic kernel.
	/// </summary>
	public string? FullRequest { get; set; }

	public string? PromptFunctions { get; set; }

	public string? Response { get; set; }

	public string? ResponseFunctionCall { get; set; }

	public string? ResponseFunctionCallParameters { get; set; }

	public DateTime CallStart { get; set; }

	public DateTime CallEnd { get; set; }

	public TimeSpan CallDuration => CallEnd - CallStart;    

	public string Dump()
	{
		if (string.IsNullOrEmpty(PromptFunctions))
			return
				$"Prompt: {Prompt}\n" +
				$"Response: {Response}\n" +
				$"ResponseFunctionCall: {ResponseFunctionCall}\n";

		return $"Ask to LLM: {Prompt} -> Call function {ResponseFunctionCall} with arguments {ResponseFunctionCallParameters}";
	}
}