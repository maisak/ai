using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Search;
using Xunit.Abstractions;

namespace Sandbox;

public class Search : SandboxBase
{
	private readonly SearchService _service;
	private readonly ITestOutputHelper _output;

	public Search(ITestOutputHelper output)
	{
		_service = ServiceProvider.GetRequiredService<SearchService>();
		_output = output;
	}
	
	[Fact]
	public async Task CreateIndex() => await _service.CreateIndex();
	
	[Fact]
	public async Task DeleteAllDocuments() => await _service.DeleteAllDocuments();

	[Fact]
	public async Task Stats()
	{
		var response = await _service.GetStats();
		_output.WriteLine(response);
	}
	
	[Fact]
	public async Task IndexFiles()
	{
		var files = new List<string>
		{
			"Coffee Machine manual.pdf",
			"Whitewater magazine #2.pdf",
			"emails.txt",
			"my awesome presentation.pptx",
		};
		var chat = ServiceProvider.GetRequiredService<SearchService>();
		foreach (var file in files)
		{
			await chat.IndexAsync(file, CancellationToken.None);
		}
	}
}