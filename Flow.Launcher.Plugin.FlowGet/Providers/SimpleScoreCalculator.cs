using System;
using Flow.Launcher.Plugin.FlowGet.Contracts;
using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet.Providers;

internal class SimpleScoreCalculator
	: IScoreCalculator
{
	private static readonly Version MinimumVersion = new Version(0, 0, 0, 0);

	public int CalculateScore(WinGetPackage package)
	{
		var score = 0;

		// Add score for having a name
		if (!string.IsNullOrWhiteSpace(package.Name))
			score += 10;

		// Add score for having a valid, non-zero version
		if (package.Version > MinimumVersion)
		{
			score += 20;
			// Add bonus for major releases
			if (package.Version.Major > 0)
			{
				score += package.Version.Major * 5;
			}
		}

		return score;
	}
}
