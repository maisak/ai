using Chat;
using ContentSafety;
using Language;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemKern;
using Speech;

namespace Sandbox;

public class SandboxBase
{
	protected readonly IServiceProvider ServiceProvider;
	private readonly IConfiguration _config;

	protected SandboxBase()
	{
		_config = Configuration.InitConfiguration();
		ServiceProvider = CreateServiceProvider();
	}
    
	private IServiceProvider CreateServiceProvider()
	{
		var serviceCollection =
			new ServiceCollection()
				.AddLogging(builder =>
					builder
						.ClearProviders()
						.AddConsole()
						.AddDebug());
		
		serviceCollection.AddChatServices(_config);
		serviceCollection.AddSpeechServices(_config);
		serviceCollection.AddLanguageServices(_config);
		serviceCollection.AddContentSafetyServices(_config);
		serviceCollection.AddSemanticKernelServices(_config);

		return serviceCollection.BuildServiceProvider();
	}
}