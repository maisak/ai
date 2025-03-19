using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Moq;
using SemKern.Config;
using SemKern.Plugins;
using SemKern.Prompts;
using SemKern.Services;
using SemKern.Services.Models;
using Shouldly;
using System.Text.Json;
using Xunit.Abstractions;

namespace Sandbox.SemanticKernel.TestedKernel;

public class SemanticKernelTests : SandboxBase
{
	private readonly ITestOutputHelper _output;
	private readonly Mock<MusicPlugin> _mockMusicPlugin;
	private readonly Kernel _kernel;
	private readonly IPromptRenderer _promptRenderer;
	
	public static TheoryData<string> TestDataGeneric => ["hi", "hello", "thanks, bye"];
	public static TheoryData<string, string> TestDataAdvice => new ()
	{
		{ "can you suggest something from punk rock?", string.Empty },
		{ "what genres you specialize on?", "blues" },
		{ "what kind of music you can help with?", "industrial" }
	};
	public static TheoryData<string, string> TestDataWarehouse => new ()
	{
		{ "you have anything of Queen available?", "Sheer" },
		{ "do you have Rammstein?", string.Empty },
	};
	
	public SemanticKernelTests(ITestOutputHelper output)
	{
		_output = output;
		// mocks
		_mockMusicPlugin = new Mock<MusicPlugin>();
		_mockMusicPlugin.Setup(x => x.GetAvailableGenres()).ReturnsAsync(["Industrial", "Blues"]);
		_mockMusicPlugin.Setup(x => x.SearchForProduct(It.IsAny<string>())).ReturnsAsync(
			new Dictionary<string, double>
			{
				{ "Queen - Sheer Heart Attack", 1.99 }
			});
		// construct kernel
		var settings = ServiceProvider.GetRequiredService<IOptions<OpenAiSettings>>().Value;
		var builder = Kernel.CreateBuilder()
			.AddAzureOpenAIChatCompletion(settings.Model, settings.Endpoint, settings.ApiKey);
		builder.Services.AddLogging(s => s.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Trace));
		builder.Plugins.AddFromObject(_mockMusicPlugin.Object);
		_kernel = builder.Build();
		_promptRenderer = ServiceProvider.GetRequiredService<IPromptRenderer>();
	}

	[Theory, MemberData(nameof(TestDataGeneric))]
	public async Task Strategies_Generic(string input)
	{
		// act
		var service = await TestableKernelWrapper.CreateAsync(_kernel, _promptRenderer);
		var r1 = await service.Chat(input);
		// json parsing
		var response = JsonSerializer.Deserialize<ChatResponse>(r1);
		// strategy is 'generic'
		response!.Strategy.ShouldBe("Generic");
		// no plugin calls, as the question is a generic one
		_mockMusicPlugin.VerifyNoOtherCalls();
		// response is not empty
		response.Text.ShouldNotBeNullOrEmpty();
		// has a name of the customer
		response.Text.ShouldContain("Homer");
		_output.WriteLine(r1);
	}

	[Theory, MemberData(nameof(TestDataAdvice))]
	public async Task Strategies_Advice(string input, string expected)
	{
		// act
		var service = await TestableKernelWrapper.CreateAsync(_kernel, _promptRenderer);
		var r1 = await service.Chat(input);
		// json parsing
		var response = JsonSerializer.Deserialize<ChatResponse>(r1);
		// strategy is 'advice'
		response!.Strategy.ShouldBe("Advice");
		// genres function is called
		_mockMusicPlugin.Verify(x => x.GetAvailableGenres(), Times.AtLeast(1));
		// response is not empty
		response.Text.ShouldNotBeNullOrEmpty();
		if (!string.IsNullOrEmpty(expected)) response.Text.ShouldContain(expected);
		_output.WriteLine(r1);
	}
	
	[Theory, MemberData(nameof(TestDataWarehouse))]
	public async Task Strategies_Warehouse(string input, string expected)
	{
		// act
		var service = await TestableKernelWrapper.CreateAsync(_kernel, _promptRenderer);
		var r1 = await service.Chat(input);
		// json parsing
		var response = JsonSerializer.Deserialize<ChatResponse>(r1);
		// strategy is 'warehouse'
		response!.Strategy.ShouldBe("Warehouse");
		// search_for_product function is called
		_mockMusicPlugin.Verify(x => x.SearchForProduct(It.IsAny<string>()), Times.AtLeast(1));
		response.Text.ShouldNotBeNullOrEmpty();
		if (!string.IsNullOrEmpty(expected)) response.Text.ShouldContain(expected);
		_output.WriteLine(r1);
	}
}