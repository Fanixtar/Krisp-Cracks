using System;
using System.Windows.Input;

namespace Krisp.Services
{
	internal interface IRelayCommandsService
	{
		ICommand AboutCommand { get; }

		ICommand UpdateWindowCommand { get; }

		ICommand ReportBugCommand { get; }

		ICommand SetupKrispCommand { get; }

		ICommand TestNoiseCancellationCommand { get; }

		ICommand ContactSupportCommand { get; }
	}
}
