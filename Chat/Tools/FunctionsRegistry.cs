using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Reflection;

namespace Chat.Tools;

public static class FunctionsRegistry
{
	public static IEnumerable<AITool> GetTools(IServiceProvider serviceProvider)
	{
		var genericTools = serviceProvider.GetServices<IGenericTool>();
		foreach (var tool in genericTools)
		{
			var toolFunctions = tool.GetType()
				.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach (var function in toolFunctions)
			{
				var description = function.CustomAttributes
									  .FirstOrDefault(x => x.AttributeType == typeof(DescriptionAttribute))?
									  .ConstructorArguments.FirstOrDefault().Value as string
								  ?? function.Name;
				yield return AIFunctionFactory.Create(
					function,
					tool,
					new AIFunctionFactoryOptions
					{
						Name = function.Name,
						Description = description
					});
			}
		}
	}
}