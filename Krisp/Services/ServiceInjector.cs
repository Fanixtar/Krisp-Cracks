using System;
using Krisp.Core.Internals;
using Krisp.Models;
using Krisp.UI;
using MVVMFoundation;

namespace Krisp.Services
{
	public static class ServiceInjector
	{
		public static void InjectServices()
		{
			ServiceContainer.Instance.AddService<IAccountManager>(AccountManager.Instance);
			ServiceContainer.Instance.AddService<IRelayCommandsService>(RelayCommandsService.Instance);
		}
	}
}
