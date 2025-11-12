using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet.Contracts;

internal interface IScoreCalculator
{
	int CalculateScore(WinGetPackage package);
}
