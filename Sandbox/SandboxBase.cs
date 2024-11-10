using Chat;
using Language;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sandbox;

public class SandboxBase
{
	protected IServiceProvider ServiceProvider;
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
		serviceCollection.AddLanguageServices(_config);

		return serviceCollection.BuildServiceProvider();
	}
}