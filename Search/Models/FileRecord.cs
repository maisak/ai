using System.Security.Cryptography;
using System.Text;

namespace Search.Models;

public class FileRecord(string filename)
{
	public string Id { get; set; } = CreateSha512(filename);
	public string Filename { get; set; } = filename;
	public string Summary { get; set; } = string.Empty;

	private static string CreateSha512(string strData)
	{
		var message = Encoding.UTF8.GetBytes(strData);
		var hashValue = SHA512.HashData(message);
		return hashValue.Aggregate("", (current, x) => current + $"{x:x2}");
	}
}