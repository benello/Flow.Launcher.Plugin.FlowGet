using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet;

internal class PackageOperations(WinGetPackageManager packageManager, ResultFactory resultFactory)
{
	private readonly WinGetPackageManager _packageManager = packageManager;
	private readonly ResultFactory _resultFactory = resultFactory;

	public async Task<List<Result>> SearchAsync(string? term, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(term))
			return ResultFactory.EmptySearch;

		List<Result> results = [];

		foreach (var pkg in await _packageManager.SearchPackageAsync(term, cancellationToken: cancellation))
			results.Add(_resultFactory.Installable(pkg));

		return results;
	}

	public async Task<List<Result>> InstallAsync(string? id, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(id))
			return ResultFactory.EmptySearch;

		var pkg = (await _packageManager.SearchPackageAsync(id, true, cancellation)).FirstOrDefault();
		return pkg is not null
			? new List<Result> { _resultFactory.Installable(pkg) }
			: new List<Result> { new() { Title = "Package not found" } };
	}

	public async Task<List<Result>> UninstallAsync(string? id, CancellationToken cancellation)
	{
		var packages = string.IsNullOrEmpty(id)
			? await _packageManager.GetInstalledPackagesAsync(cancellation)
			: await _packageManager.GetInstalledPackagesAsync(id, cancellationToken: cancellation);

		List<Result> results = [];
		foreach (var pkg in packages)
			results.Add(_resultFactory.Uninstallable(pkg));

		return results;
	}

	public async Task<List<Result>> UpdateAsync(string? param, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(param))
		{
			return
			[
				_resultFactory.UpdateAll()
			];
		}

		var packages = await _packageManager.GetUpgradeablePackagesAsync(cancellation);
		List<Result> results = [];

		if (packages.Count != 0)
			results.Add(_resultFactory.UpdateAll());

		foreach (var pkg in packages)
			results.Add(_resultFactory.Upgradable(pkg));

		return results;
	}
}
