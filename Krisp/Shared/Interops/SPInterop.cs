using System;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	public static class SPInterop
	{
		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT Initialize([In] SPSDKModel[] models, int modelcnt, [MarshalAs(UnmanagedType.LPWStr)] string modelsFolder, MulticastDelegate callback);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT Destroy();

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT SessionSetDevice(EnStreamDirection dir, uint sID, [MarshalAs(UnmanagedType.LPWStr)] string modelName, [MarshalAs(UnmanagedType.LPWStr)] string deviceID);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT RecordSession(EnStreamDirection dir, uint sID, [MarshalAs(UnmanagedType.LPWStr)] string modelName, [MarshalAs(UnmanagedType.LPWStr)] string deviceID, [MarshalAs(UnmanagedType.LPWStr)] string sourceFilePath, [MarshalAs(UnmanagedType.LPWStr)] string beforeNCFilePath, [MarshalAs(UnmanagedType.LPWStr)] string afterNCFilePath);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT SessionRelease(EnStreamDirection dir, uint sID);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT SetActivityState(EnStreamDirection dir, bool state);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern HRESULT SetFeatureState(EnStreamDirection dir, SPFeature feature, bool state);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern int GetStreamActivityLevel(EnStreamDirection dir);

		[DllImport("StreamsProcessor", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool IsRunningOnVM();
	}
}
