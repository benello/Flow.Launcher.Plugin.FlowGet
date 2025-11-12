using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowGet.Providers;

internal class CommandDispatcher
{
	private readonly Dictionary<string, Func<string?, CancellationToken, Task<List<Result>>>> _handlers;
	private readonly PackageOperations _operations;
	private readonly ResultFactory _resultFactory;

	public CommandDispatcher(PackageOperations operations, ResultFactory resultFactory)
	{
		_operations = operations;
		_resultFactory = resultFactory;

		_handlers =
			new Dictionary<string, Func<string?, CancellationToken, Task<List<Result>>>>(StringComparer
				.OrdinalIgnoreCase)
			{
				["find"] = _operations.SearchAsync,
				["search"] = _operations.SearchAsync,
				["install"] = _operations.InstallAsync,
				["uninstall"] = _operations.UninstallAsync,
				["update"] = _operations.UpdateAsync,
				["upgrade"] = _operations.UpdateAsync,
				["help"] = (_, _) => Task.FromResult(ResultFactory.Help)
			};
	}

	public async Task<List<Result>> DispatchAsync(string input, CancellationToken cancellation)
	{
		var args = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
		var command = args[0];
		var parameter = args.Length > 1
			? args[1]
			: null;

		if (_handlers.TryGetValue(command, out var handler))
			return await handler(parameter, cancellation).ConfigureAwait(false);

		return ResultFactory.Help;
	}
}
