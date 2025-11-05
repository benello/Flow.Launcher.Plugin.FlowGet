using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.FlowGet;

internal class UiExecutor(IPublicAPI? api, string pluginName)
{
	private readonly IPublicAPI? _api = api;
	private readonly string _pluginName = pluginName;

	public void Schedule(string opName, Func<CancellationToken, Task<bool>> operation, string success, string failure)
	{
		_ = Task.Run(() => ExecuteAsync(opName, operation, success, failure));
	}

	private async Task ExecuteAsync(string opName, Func<CancellationToken, Task<bool>> operation, string success,
		string failure)
	{
		try
		{
			_api?.ShowMsg(_pluginName, $"{opName} started...");
			using var cts = new CancellationTokenSource();

			var ok = await operation(cts.Token).ConfigureAwait(false);
			_api?.ShowMsg(_pluginName, ok ? success : failure);
		}
		catch (OperationCanceledException)
		{
			_api?.ShowMsg(_pluginName, $"{opName} canceled");
		}
		catch (Exception ex)
		{
			_api?.LogException(_pluginName, opName, ex);
			_api?.ShowMsg(_pluginName, $"{failure}: {ex.Message}");
		}
	}
}
