using System;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Models
{
	internal interface IAudioDeviceSession
	{
		event EventHandler<AudioSessionDisconnectReason> SessionDisconnected;

		event EventHandler<AudioSessionState> StateChanged;

		AudioDeviceKind Kind { get; }

		string SessionID { get; }

		IAppInfo AppInfo { get; }
	}
}
