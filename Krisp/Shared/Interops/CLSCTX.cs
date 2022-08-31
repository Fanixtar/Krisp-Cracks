using System;

namespace Shared.Interops
{
	[Flags]
	internal enum CLSCTX
	{
		CLSCTX_INPROC_SERVER = 1,
		CLSCTX_INPROC_HANDLER = 2
	}
}
