using Azure.AI.ContentSafety;
using System.Diagnostics;

namespace ContentSafety;

public class ContentSafetyService(ContentSafetyClient client, BlocklistClient blocklistClient)
{
	public async Task DetectHarmfulContent(string content)
	{
		var request = new AnalyzeTextOptions(content);
		var response = await client.AnalyzeTextAsync(request);
	}
	
	public async Task AnalyzeImage(byte[] bytes)
	{
		var request = new AnalyzeImageOptions(new ContentSafetyImageData(BinaryData.FromBytes(bytes)));
		var response = await client.AnalyzeImageAsync(request);
	}
	
	public void ReadBlocklist()
	{
		foreach (var blocklist in blocklistClient.GetTextBlocklists())
		{
			Debug.WriteLine(blocklist.Name);
		}
	}
}