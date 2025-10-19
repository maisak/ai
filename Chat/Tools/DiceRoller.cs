using System.Diagnostics;

namespace Chat.Tools;

public class DiceRoller : IGenericTool
{
	public byte Roll()
	{
		var score = Random.Shared.Next(1, 6);
		Debug.WriteLine($"Roll! {score}");
		return (byte) score;
	}
}