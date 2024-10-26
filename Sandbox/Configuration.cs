using Microsoft.Extensions.Configuration;

namespace Sandbox;

public static class Configuration
{
	public static IConfiguration InitConfiguration()
	{
		var config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.sandbox.json")
			.Build();
		return config;
	}
}