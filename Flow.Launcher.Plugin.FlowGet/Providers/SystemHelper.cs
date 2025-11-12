using System;
using System.Security.Principal;

namespace Flow.Launcher.Plugin.FlowGet.Providers;

internal static class SystemHelper
{
	public static bool CheckAdministratorPrivileges()
	{
		if (Environment.OSVersion.Platform == PlatformID.Unix)
			return false;

		using (var current = WindowsIdentity.GetCurrent(false))
			return current != null && new WindowsPrincipal(current).IsInRole(WindowsBuiltInRole.Administrator);
	}
}
