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

		List<Result> results = [];

		if (!SystemHelper.CheckAdministratorPrivileges())
			results.Add(ResultFactory.AdminWarningBanner);

		foreach (var pkg in await _packageManager.SearchPackageAsync(term, cancellationToken: cancellation))
			results.Add(_resultFactory.Installable(pkg));

		return results;
	}

	public async Task<List<Result>> InstallAsync(string? id, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(id))
			return ResultFactory.EmptyInstall;

		var pkg = (await _packageManager.SearchPackageAsync(id, true, cancellation)).FirstOrDefault();
		var results = new List<Result>();

		if (!SystemHelper.CheckAdministratorPrivileges())
			results.Add(ResultFactory.AdminWarningBanner);

		if (pkg is not null)
			results.Add(_resultFactory.Installable(pkg));
		else
			results.Add(new Result { Title = "Package not found" });

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
