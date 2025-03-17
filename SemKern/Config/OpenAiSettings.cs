using System.ComponentModel.DataAnnotations;

namespace SemKern.Config;

public class OpenAiSettings
{
	[Required] public string Endpoint { get; init; } = string.Empty;
	[Required] public string ApiKey { get; init; } = string.Empty;
	[Required] public string Model { get; init; } = string.Empty;
}