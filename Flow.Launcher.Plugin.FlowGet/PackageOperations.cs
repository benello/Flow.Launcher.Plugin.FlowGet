using System;
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

	/// <summary>
	/// Locates a package by id and returns the install options for it.
	/// </summary>
	/// <param name="id">Package identifier to search for; if null or empty, the EmptyInstall sentinel is returned.</param>
	/// <returns>A list of Result containing an installable entry for the found package; a single Result with Title "Package not found" if no package matches; or ResultFactory.EmptyInstall when <paramref name="id"/> is null or empty.</returns>
	public async Task<List<Result>> InstallAsync(string? id, CancellationToken cancellation)
	{
		if (string.IsNullOrEmpty(id))
			return ResultFactory.EmptyInstall;

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

	/// <summary>
	/// Produces results describing available package updates, optionally filtered by name.
	/// </summary>
	/// <param name="param">An optional case-insensitive substring to filter package names. If null or empty, the results include an UpdateAll entry.</param>
	/// <param name="cancellation">A token to cancel the operation.</param>
	/// <returns>A list of Result entries for upgradeable packages. When no packages match, returns a list containing a single Result with Title = "No packages found".</returns>
	public async Task<List<Result>> UpdateAsync(string? param, CancellationToken cancellation)
	{
		List<Result> results = [];

		var packages = await _packageManager.GetUpgradeablePackagesAsync(cancellation);
		
		if (!string.IsNullOrEmpty(param))
			// A filter is active: find matching packages
			packages = packages.Where(pkg => pkg.Name.Contains(param, StringComparison.OrdinalIgnoreCase)).ToList();
		else 
			// otherwise add an option to upgrade all
			results.Add(_resultFactory.UpdateAll());

		if (packages.Count != 0)
		{
			foreach (var pkg in packages)
				results.Add(_resultFactory.Upgradable(pkg));
		}
		else
		{
			results.Add(new Result
			{
				Title = "No packages found",
			});
		}

		return results;
	}
}