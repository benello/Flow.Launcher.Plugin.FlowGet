using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.FlowGet.Contracts;
using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet.Providers;

internal class ResultFactory(UiExecutor ui, WinGetPackageManager packageManager, IScoreCalculator scoreCalculator)
{
	private readonly WinGetPackageManager _packageManager = packageManager;
	private readonly UiExecutor _ui = ui;
	private readonly IScoreCalculator _scoreCalculator = scoreCalculator;

	public static readonly List<Result> Help =
	[
		ResultBuilder.Create()
			.WithTitle("Commands")
			.WithSubTitle("Commands: search <term> | install <id> | uninstall [<id>] | update [all]")
			.WithIcon("Images/help.png")
			.Build(),
	];

	public static readonly List<Result> EmptySearch =
	[
		ResultBuilder.Create()
			.WithTitle("Please enter a search term")
			.WithIcon("Images/search.png")
			.Build(),
	];

	public static readonly List<Result> EmptyInstall =
	[
		ResultBuilder.Create()
			.WithTitle("Please specify a package ID")
			.WithIcon("Images/search.png")
			.Build(),
	];

	public static readonly List<Result> NoResults =
	[
		ResultBuilder.Create()
			.WithTitle("No results found")
			.WithIcon("Images/noresults.png")
			.Build(),
	];

	public static Result AdminWarningBanner =>
		ResultBuilder.Create()
			.WithTitle("Administrator privileges may be required")
			.WithSubTitle("Some operations may require elevated permissions to execute successfully")
			.WithScore(int.MaxValue)
			.WithIcon("Images/warning.png")
			.Build();

	public Result Installable(WinGetPackage pkg)
	{
		return ResultBuilder.Create(this)
				.WithTitle(pkg.Name)
				.WithSubTitle($"ID: {pkg.Id} | Version: {pkg.Version}")
				.WithAction(ct => _packageManager.InstallPackageAsync(pkg, ct), 
					$"Installed {pkg.Name}",
					$"Failed to install {pkg.Name}")
				.WithScore(_scoreCalculator.CalculateScore(pkg))
				.WithIcon("Images/install.png")
				.Build(); 
	}

	public Result Uninstallable(WinGetPackage pkg)
	{
		return ResultBuilder.Create(this)
				.WithTitle(pkg.Name)
				.WithSubTitle($"Uninstall {pkg.Id}")
				.WithAction(ct => _packageManager.UninstallPackageAsync(pkg, ct),
					$"Uninstalled {pkg.Name}",
					$"Failed to uninstall {pkg.Name}")
				.WithScore(_scoreCalculator.CalculateScore(pkg))
				.WithIcon("Images/remove.png")
				.Build();
	}

	public Result Upgradable(WinGetPackage pkg)
	{
		return ResultBuilder.Create(this)
				.WithTitle(pkg.Name)
				.WithSubTitle($"Upgrade from {pkg.Version} → {pkg.AvailableVersion}")
				.WithAction(ct => _packageManager.UpgradePackageAsync(pkg, ct),
					$"Upgraded {pkg.Name}",
					$"Failed to upgrade {pkg.Name}")
				.WithScore(_scoreCalculator.CalculateScore(pkg))
				.WithIcon("Images/upgrade.png")
				.Build();
	}

	public Result UpdateAll()
	{
		return ResultBuilder.Create(this)
				.WithTitle("Upgrade all upgradable packages")
				.WithSubTitle("Runs winget upgrade for each package")
				.WithAction(
					async ct =>
					{
						var successful = true;

						foreach (var pkg in await _packageManager.GetUpgradeablePackagesAsync(ct))
							successful &= await _packageManager.UpgradePackageAsync(pkg, ct);

						return successful;
					},
					"All packages processed",
					"Failed to upgrade all packages")
				.WithIcon("Images/upgrade.png")
				.Build();
	}

	private class ResultBuilder(ResultFactory? factory)
	{
		private readonly ResultFactory? _factory = factory;
		private readonly Result _result = new();

		public static ResultBuilder Create(ResultFactory factory)
			=> new(factory);

		public static ResultBuilder Create()
			=> new (null);

		public ResultBuilder WithTitle(string title)
		{
			_result.Title = title;
			return this;
		}

		public ResultBuilder WithSubTitle(string subTitle)
		{
			_result.SubTitle = subTitle;
			return this;
		}

		public ResultBuilder WithScore(int score)
		{
			_result.Score = score;
			return this;
		}

		public ResultBuilder WithIcon(string iconPath)
		{
			_result.IcoPath = iconPath;
			return this;
		}

		public ResultBuilder WithAction(Func<CancellationToken, Task<bool>> operation,
			string successMessage = "Operation succeeded", string failureMessage = "Operation failed")
		{
			if (_factory is null)
				throw new InvalidOperationException("ResultFactory instance is required to set an action.");

			_result.Action = _ =>
			{
				_factory._ui.Schedule(_result.Title, operation, successMessage, failureMessage);
				return true;
			};

			return this;
		}

		public Result Build() => _result;
	}
}
