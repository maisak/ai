using Microsoft.Extensions.Configuration;

namespace ConsoleApp;

public static class Configuration
{
	public static IConfiguration InitConfiguration()
	{
		var config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.dev.json")
			.Build();
		return config;
	}
}
