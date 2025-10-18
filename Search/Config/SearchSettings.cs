using System.ComponentModel.DataAnnotations;

namespace Search.Config;

public class SearchSettings
{
	[Required] public string Endpoint { get; init; } = string.Empty;
	[Required] public string ApiKey { get; init; } = string.Empty;
	[Required] public string Index { get; init; } = string.Empty;
}