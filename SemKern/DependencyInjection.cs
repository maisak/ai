using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SemKern.Config;
using SemKern.Logging;
using SemKern.Plugins;
using SemKern.Prompts;
using SemKern.Services;

namespace SemKern;

public static class DependencyInjection
{
	public static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddOptionsWithValidateOnStart<OpenAiSettings>()
			.BindConfiguration(nameof(OpenAiSettings))
			.ValidateDataAnnotations();
		
		services.AddTransient<ILoggerFactory>(_ =>
		{
			var connectionString = config.GetConnectionString("ApplicationInsights");

			var resourceBuilder = ResourceBuilder
				.CreateDefault()
				.AddService("TelemetryApplicationInsightsQuickstart");
		
			// Enable model diagnostics with sensitive data.
			AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
		
			using var traceProvider = Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddSource("Microsoft.SemanticKernel*")
				.AddAzureMonitorTraceExporter(options => options.ConnectionString = connectionString)
				.Build();

			using var meterProvider = Sdk.CreateMeterProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddMeter("Microsoft.SemanticKernel*")
				.AddAzureMonitorMetricExporter(options => options.ConnectionString = connectionString)
				.Build();
		
			var loggerFactory = LoggerFactory.Create(builder =>
			{
				// Add OpenTelemetry as a logging provider
				builder.AddOpenTelemetry(options =>
				{
					options.SetResourceBuilder(resourceBuilder);
					options.AddAzureMonitorLogExporter(opt => opt.ConnectionString = connectionString);
					// Format log messages. This is default to false.
					options.IncludeFormattedMessage = true;
					options.IncludeScopes = true;
				});
				builder.SetMinimumLevel(LogLevel.Trace);
			});

			return loggerFactory;
		});
		
		services.AddTransient<SemanticKernelService>();
		services.AddTransient<SemKernelWithAppInsights>();
		services.AddTransient<SemKernelWithCustomLogger>();
		services.AddTransient<DumpLoggingProvider>();
		services.AddTransient<IPromptRenderer, PromptRenderer>();

		services.AddTransient<Kernel>(isp =>
		{
			var settings = isp.GetRequiredService<IOptions<OpenAiSettings>>().Value;
			var builder = Kernel.CreateBuilder()
				.AddAzureOpenAIChatCompletion(settings.Model, settings.Endpoint, settings.ApiKey);
			builder.Plugins.AddFromType<LogPlugin>();
			builder.Plugins.AddFromType<MusicPlugin>();
			var kernel = builder.Build();
			return kernel;
		});
		
		return services;
	}
}