using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SemKern.Config;
using SemKern.Logging;
using SemKern.Services;

namespace SemKern;

public static class DependencyInjection
{
	public static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddTransient<OpenAiSettings>(_ => new OpenAiSettings
		{
			Endpoint = config["OpenAiSettings:Endpoint"]!,
			ApiKey = config["OpenAiSettings:ApiKey"]!,
			Model = config["OpenAiSettings:Model"]!
		});
		
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
					options.AddAzureMonitorLogExporter(options => options.ConnectionString = connectionString);
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
		
		return services;
	}
}