using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace SemKern.Prompts;

public class PromptRenderer : IPromptRenderer
{
	public async Task<string> RenderSystemPrompt(Kernel kernel, string name)
	{
		var template = await File.ReadAllTextAsync("./Prompts/system-template.yaml");
		var strategies = await File.ReadAllTextAsync("./Prompts/strategies.prompt");
		
		var templateFactory = new HandlebarsPromptTemplateFactory();
		var promptTemplateConfig = new PromptTemplateConfig
		{
			Template = template,
			TemplateFormat = "handlebars",
			Name = "SystemPromptTemplate",
		};
		var promptTemplate = await templateFactory
			.Create(promptTemplateConfig)
			.RenderAsync(kernel, new KernelArguments
			{
				{ "customerName", name },
				{ "responseStrategies", strategies }
			});
		
		return promptTemplate;
	}
}

public interface IPromptRenderer
{
	Task<string> RenderSystemPrompt(Kernel kernel, string name);
}