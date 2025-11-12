using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.FlowGet.Providers;
using WGetNET;

namespace Flow.Launcher.Plugin.FlowGet;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class FlowGet
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	: IAsyncPlugin
{
	private const string PluginName = "FlowGet";

	private PluginInitContext _context = null!;
	private CommandDispatcher _dispatcher = null!;

	/// <inheritdoc />
	public Task InitAsync(PluginInitContext context)
	{
		_context = context;
		var packageManager = new WinGetPackageManager();

		if (!packageManager.IsInstalled)
			throw new InvalidOperationException("Winget is not installed");

		var ui = new UiExecutor(_context.API, PluginName);
		var scoreCalculator = new SimpleScoreCalculator();
		var resultFactory = new ResultFactory(ui, packageManager, scoreCalculator);
		_dispatcher = new CommandDispatcher(new PackageOperations(packageManager, resultFactory), resultFactory);

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public async Task<List<Result>> QueryAsync(Query query, CancellationToken cancellationToken)
	{
		try
		{
			var input = (query.Search ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(input))
				return ResultFactory.Help;

			return await _dispatcher.DispatchAsync(input, cancellationToken);
		}
		catch (Exception ex)
		{
			_context.API.LogException(PluginName, "Query", ex);
			return new List<Result>
			{
				new()
				{
					Title = "Error"
				}
			};
		}
	}
}
