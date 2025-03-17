using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;

namespace SemKern.Plugins;

public class MusicPlugin
{
	[KernelFunction("get_available_genres")]
	[Description("Gets a list of available music genres")]
	[return: Description("An array of genres")]
	public async Task<List<string>> GetAvailableGenres()
	{
		Debug.WriteLine("Function call: return available genres");
		
		return ["Rock", "Metal", "Synthwave"];
	}
	
	[KernelFunction("search_for_product")]
	[Description("Checks if a product is available")]
	[return: Description("A dictionary of products and their prices")]
	public async Task<Dictionary<string, double>> SearchForProduct(string product)
	{
		Debug.WriteLine("Function call: finding out if product is available");
		
		if (product.Contains("megadeth"))
		{
			return new Dictionary<string, double>
			{
				{ "Megadeth - Rust in Peace", 1.99 },
				{ "Megadeth - Countdown to Extinction", 0.99 }
			};
		}

		if (product.Contains("pantera"))
		{
			return new Dictionary<string, double>
				{
					{ "Pantera - Vulgar Display of Power", 3.99 },
					{ "Pantera - Cowboys from Hell", 5.99 }
				};
		}

		return [];
	}
	
	[KernelFunction("get_similar_products")]
	[Description("Suggestions for similar products")]
	[return: Description("A dictionary of products and their prices")]
	public async Task<Dictionary<string, double>> GetSimilarProducts(string product)
	{
		Debug.WriteLine("Function call: finding out if product is available");
		
		return new Dictionary<string, double>
		{
			{"Metallica - Master of Puppets", 2.99},
			{"Slayer - Reign in Blood", 3.99}
		};
	}
	
}