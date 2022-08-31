using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib
{
	[CompilerGenerated]
	[Guid("ADE87BF7-7B56-4275-8FAB-B9B0E591844B")]
	[TypeIdentifier]
	[ComImport]
	public interface ISystemInformation
	{
		void _VtblGap1_1();

		[DispId(1610743810)]
		bool RebootRequired
		{
			[DispId(1610743810)]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
	}
}
