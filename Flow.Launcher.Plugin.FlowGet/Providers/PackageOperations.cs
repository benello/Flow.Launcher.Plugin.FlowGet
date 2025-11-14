using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet.Providers;

internal class PackageOperations(WinGetPackageManager packageManager, ResultFactory resultFactory)
{
	private readonly WinGetPackageManager _packageManager = packageManager;
	private readonly ResultFactory _resultFactory = resultFactory;

	public async Task<List<Result>> SearchAsync(string? term, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(term))
			return ResultFactory.EmptySearch;
		
		var packages = await _packageManager.SearchPackageAsync(term, cancellationToken: cancellation);

		if (packages.Count == 0)
			return ResultFactory.NoResults;

		List<Result> results = [];
		if (!SystemHelper.CheckAdministratorPrivileges())
			results.Add(ResultFactory.AdminWarningBanner);

		foreach (var pkg in packages)
			results.Add(_resultFactory.Installable(pkg));

		return results;
	}

	public async Task<List<Result>> InstallAsync(string? id, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(id))
			return ResultFactory.EmptyInstall;

		var pkg = (await _packageManager.SearchPackageAsync(id, true, cancellation)).FirstOrDefault();

		if (pkg is null)
			return ResultFactory.NoResults;

		var results = new List<Result>();
		if (!SystemHelper.CheckAdministratorPrivileges())
			results.Add(ResultFactory.AdminWarningBanner);

		results.Add(_resultFactory.Installable(pkg));

		return results;
	}

	public async Task<List<Result>> UninstallAsync(string? id, CancellationToken cancellation)
	{
		var packages = string.IsNullOrEmpty(id)
			? await _packageManager.GetInstalledPackagesAsync(cancellation)
			: await _packageManager.GetInstalledPackagesAsync(id, cancellationToken: cancellation);

		List<Result> results = [];

		if (!SystemHelper.CheckAdministratorPrivileges())
			results.Add(ResultFactory.AdminWarningBanner);

		foreach (var pkg in packages)
			results.Add(_resultFactory.Uninstallable(pkg));

		return results;
	}

	public async Task<List<Result>> UpdateAsync(string? param, CancellationToken cancellation)
	{
		List<Result> results = [];
		var packages = await _packageManager.GetUpgradeablePackagesAsync(cancellation);

		if (!string.IsNullOrEmpty(param))
			// A filter is active: find matching packages
			packages = packages.Where(pkg => pkg.Name.Contains(param, StringComparison.OrdinalIgnoreCase)).ToList();
		else if (packages.Count != 0)
			// otherwise add an option to upgrade all
			results.Add(_resultFactory.UpdateAll());

		if (packages.Count == 0)
			return ResultFactory.NoResults;

		if (!SystemHelper.CheckAdministratorPrivileges())
			results.Add(ResultFactory.AdminWarningBanner);

		foreach (var pkg in packages)
			results.Add(_resultFactory.Upgradable(pkg));

		return results;
	}
}
