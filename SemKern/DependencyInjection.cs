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
		
		services.AddSingleton<ILoggerFactory>(_ =>
		{
			// not reliable in tests, but always works in console apps, etc
			var connectionString = config.GetConnectionString("ApplicationInsights");
			var resourceBuilder = ResourceBuilder
				.CreateDefault()
				.AddService("TelemetryApplicationInsightsQuickstart");
			AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
			var traceProvider = Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddSource("Microsoft.SemanticKernel*")
				.AddSource(nameof(SemKernelWithAppInsights))
				//.AddConsoleExporter()
				.AddAzureMonitorTraceExporter(options => options.ConnectionString = connectionString)
				.Build();
			var meterProvider = Sdk.CreateMeterProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddMeter("Microsoft.SemanticKernel*")
				.AddMeter(nameof(SemKernelWithAppInsights))
				//.AddConsoleExporter()
				.AddAzureMonitorMetricExporter(options => options.ConnectionString = connectionString)
				.Build();
			var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddOpenTelemetry(options =>
				{
					options.SetResourceBuilder(resourceBuilder);
					//options.AddConsoleExporter();
					options.AddAzureMonitorLogExporter(opt => opt.ConnectionString = connectionString);
					options.IncludeFormattedMessage = true;
					options.IncludeScopes = true;
				});
				builder.SetMinimumLevel(LogLevel.Trace);
			});

			return loggerFactory;
		});
		
		services.AddTransient<SemanticKernelService>();
		services.AddSingleton<SemKernelWithAppInsights>();
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