using System;

namespace Krisp.Core.Internals
{
	[Flags]
	public enum StreamActivityState
	{
		StreamClosed = 1,
		StreamOpened = 2,
		StreamStarted = 4,
		StreamStoped = 8
	}
}
