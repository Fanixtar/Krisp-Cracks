using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Shared.Interops
{
	public static class Kernel32
	{
		public static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
		{
			return (DeviceType << 16) | (Access << 14) | (Function << 2) | Method;
		}

		[DllImport("kernel32.dll")]
		public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

		[DllImport("kernel32.dll")]
		public static extern bool SetEvent(IntPtr hEvent);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll")]
		public static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename, [MarshalAs(UnmanagedType.U4)] FileAccess access, [MarshalAs(UnmanagedType.U4)] FileShare share, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes, IntPtr templateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(Kernel32.ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern int QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpExeName, ref uint lpdwSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, [MarshalAs(UnmanagedType.AsAny)] [In] object InBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, [In] ref NativeOverlapped Overlapped);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CancelIo(IntPtr hFile);

		public static readonly uint INFINITE = uint.MaxValue;

		public static readonly uint WAIT_ABANDONED = 128U;

		public static readonly uint WAIT_OBJECT_0 = 0U;

		public static readonly uint WAIT_TIMEOUT = 258U;

		public static readonly uint FILE_DEVICE_UNKNOWN = 34U;

		public static readonly uint FILE_DEVICE_HAL = 257U;

		public static readonly uint FILE_DEVICE_CONSOLE = 258U;

		public static readonly uint FILE_DEVICE_PSL = 259U;

		public static readonly uint METHOD_BUFFERED = 0U;

		public static readonly uint METHOD_IN_DIRECT = 1U;

		public static readonly uint METHOD_OUT_DIRECT = 2U;

		public static readonly uint METHOD_NEITHER = 3U;

		public static readonly uint FILE_ANY_ACCESS = 0U;

		public static readonly uint FILE_READ_ACCESS = 1U;

		public static readonly uint FILE_WRITE_ACCESS = 2U;

		[Flags]
		public enum ProcessAccessFlags : uint
		{
			PROCESS_TERMINATE = 1U,
			PROCESS_CREATE_THREAD = 2U,
			PROCESS_VM_OPERATION = 8U,
			PROCESS_VM_READ = 16U,
			PROCESS_VM_WRITE = 32U,
			PROCESS_DUP_HANDLE = 64U,
			PROCESS_SET_INFORMATION = 512U,
			PROCESS_QUERY_INFORMATION = 1024U,
			PROCESS_QUERY_LIMITED_INFORMATION = 4096U,
			SYNCHRONIZE = 1048576U,
			PROCESS_ALL_ACCESS = 2035711U
		}
	}
}
