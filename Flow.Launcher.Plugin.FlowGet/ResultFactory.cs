using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet;

internal class ResultFactory(UiExecutor ui, WinGetPackageManager packageManager)
{
	public static readonly List<Result> Help =
	[
		new()
		{
			Title = "Commands",
			SubTitle = "Commands: search <term> | install <id> | uninstall [<id>] | update [all]"
		}
	];

	public static readonly List<Result> EmptySearch =
	[
		new()
		{
			Title = "Please enter a search term"
		}
	];

	public static readonly List<Result> EmptyInstall =
	[
		new()
		{
			Title = "Please specify a package ID"
		}
	];

	public static readonly List<Result> NoResults =
	[
		new()
		{
			Title = "No results found"
		}
	];

	private readonly WinGetPackageManager _packageManager = packageManager;
	private readonly UiExecutor _ui = ui;

	public Result Installable(WinGetPackage pkg)
	{
		return ActionResult(pkg.Name, $"ID: {pkg.Id} | Version: {pkg.Version}",
			$"Install {pkg.Id}",
			ct => _packageManager.InstallPackageAsync(pkg, ct),
			$"Installed {pkg.Name}",
			$"Failed to install {pkg.Name}");
	}

	public Result Uninstallable(WinGetPackage pkg)
	{
		return ActionResult(pkg.Name, $"Uninstall {pkg.Id}",
			$"Uninstall {pkg.Id}",
			ct => _packageManager.UninstallPackageAsync(pkg, ct),
			$"Uninstalled {pkg.Name}",
			$"Failed to uninstall {pkg.Name}");
	}

	public Result Upgradable(WinGetPackage pkg)
	{
		return ActionResult(pkg.Name, $"Upgrade from {pkg.Version} → {pkg.AvailableVersion}",
			$"Upgrade {pkg.Id}",
			ct => _packageManager.UpgradePackageAsync(pkg, ct),
			$"Upgraded {pkg.Name}",
			$"Failed to upgrade {pkg.Name}");
	}

	public Result UpdateAll()
	{
		return ActionResult("Upgrade all upgradable packages", "Runs winget upgrade for each package",
			"Upgrade all",
			ct => _packageManager.GetUpgradeablePackagesAsync(ct)
				.ContinueWith(async listTask =>
				{
					foreach (var pkg in await listTask)
						await _packageManager.UpgradePackageAsync(pkg, ct);
					return true;
				}, ct, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current).Unwrap(),
			"All packages processed",
			"Failed to upgrade all packages");
	}

	private Result ActionResult(string title, string subTitle, string opName,
		Func<CancellationToken, Task<bool>> operation, string success, string failure)
	{
		return new Result
		{
			Title = title,
			SubTitle = subTitle,
			Action = _ =>
			{
				_ui.Schedule(opName, operation, success, failure);
				return true;
			}
		};
	}
}
