using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Rewrite.SuperNotifyIcon.Finder
{
	internal class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern bool GetClientRect(IntPtr hWnd, out NativeMethods.RECT lpRect);

		[DllImport("Shell32", SetLastError = true)]
		public static extern int Shell_NotifyIconGetRect(ref NativeMethods.NOTIFYICONIDENTIFIER identifier, out NativeMethods.RECT iconLocation);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hWnd, out NativeMethods.RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref NativeMethods.RECT lpPoints, uint cPoints);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, NativeMethods.AllocationType flAllocationType, NativeMethods.MemoryProtection flProtect);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, NativeMethods.FreeType dwFreeType);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out IntPtr lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out uint lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out NativeMethods.TBBUTTON lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out NativeMethods.RECT lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref NativeMethods.TBBUTTON lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr hWndParent, NativeMethods.EnumWindowProc lpEnumFunc, IntPtr lParam);

		public static List<IntPtr> GetChildToolbarWindows(IntPtr parent)
		{
			List<IntPtr> list = new List<IntPtr>();
			GCHandle gchandle = GCHandle.Alloc(list);
			try
			{
				NativeMethods.EnumWindowProc enumWindowProc = new NativeMethods.EnumWindowProc(NativeMethods.EnumToolbarWindow);
				NativeMethods.EnumChildWindows(parent, enumWindowProc, GCHandle.ToIntPtr(gchandle));
			}
			finally
			{
				if (gchandle.IsAllocated)
				{
					gchandle.Free();
				}
			}
			return list;
		}

		public static bool EnumToolbarWindow(IntPtr handle, IntPtr pointer)
		{
			List<IntPtr> list = GCHandle.FromIntPtr(pointer).Target as List<IntPtr>;
			if (list == null)
			{
				throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
			}
			StringBuilder stringBuilder = new StringBuilder(128);
			NativeMethods.GetClassName(handle, stringBuilder, stringBuilder.Capacity);
			if (stringBuilder.ToString() == "ToolbarWindow32")
			{
				list.Add(handle);
			}
			return true;
		}

		public static List<IntPtr> GetChildButtonWindows(IntPtr parent)
		{
			List<IntPtr> list = new List<IntPtr>();
			GCHandle gchandle = GCHandle.Alloc(list);
			try
			{
				NativeMethods.EnumWindowProc enumWindowProc = new NativeMethods.EnumWindowProc(NativeMethods.EnumButtonWindow);
				NativeMethods.EnumChildWindows(parent, enumWindowProc, GCHandle.ToIntPtr(gchandle));
			}
			finally
			{
				if (gchandle.IsAllocated)
				{
					gchandle.Free();
				}
			}
			return list;
		}

		public static bool EnumButtonWindow(IntPtr handle, IntPtr pointer)
		{
			List<IntPtr> list = GCHandle.FromIntPtr(pointer).Target as List<IntPtr>;
			if (list == null)
			{
				throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
			}
			StringBuilder stringBuilder = new StringBuilder(128);
			NativeMethods.GetClassName(handle, stringBuilder, stringBuilder.Capacity);
			if (stringBuilder.ToString() == "Button" && NativeMethods.GetWindowTextLength(handle) == 0)
			{
				list.Add(handle);
			}
			return true;
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out NativeMethods.POINT lpPoint);

		[DllImport("shell32.dll", SetLastError = true)]
		public static extern IntPtr SHAppBarMessage(NativeMethods.ABMsg dwMessage, ref NativeMethods.APPBARDATA pData);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr MonitorFromRect(ref NativeMethods.RECT lprc, uint dwFlags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, ref NativeMethods.MONITORINFO lpmi);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("DwmApi.dll", SetLastError = true)]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref NativeMethods.MARGINS pMarInset);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		public static bool IsOverClientArea(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
		{
			return NativeMethods.DefWindowProc(hWnd, 132U, wParam, lParam).ToInt32() == 1;
		}

		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(NativeMethods.SystemMetric smIndex);

		[DllImport("DwmApi.dll")]
		public static extern int DwmIsCompositionEnabled(out bool enabled);

		public const uint TB_GETBUTTON = 1047U;

		public const uint TB_BUTTONCOUNT = 1048U;

		public const uint TB_GETITEMRECT = 1053U;

		public const byte TBSTATE_HIDDEN = 8;

		public const uint MONITOR_DEFAULTTONULL = 0U;

		public const uint MONITOR_DEFAULTTOPRIMARY = 1U;

		public const uint MONITOR_DEFAULTTONEAREST = 2U;

		public const int WM_SIZE = 5;

		public const int WM_NCHITTEST = 132;

		public const int WM_SETCURSOR = 32;

		public const int WM_LBUTTONDOWN = 513;

		public const int WM_RBUTTONDOWN = 516;

		public const int WM_MBUTTONDOWN = 519;

		public const int WM_XBUTTONDOWN = 523;

		public const int WM_DWMCOMPOSITIONCHANGED = 798;

		public const int HTCLIENT = 1;

		public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

		[Flags]
		public enum AllocationType
		{
			Commit = 4096,
			Reserve = 8192,
			Decommit = 16384,
			Release = 32768,
			Reset = 524288,
			Physical = 4194304,
			TopDown = 1048576,
			LargePages = 536870912
		}

		[Flags]
		public enum MemoryProtection
		{
			Execute = 16,
			ExecuteRead = 32,
			ExecuteReadWrite = 64,
			ExecuteWriteCopy = 128,
			NoAccess = 1,
			ReadOnly = 2,
			ReadWrite = 4,
			WriteCopy = 8,
			GuardModifierflag = 256,
			NoCacheModifierflag = 512,
			WriteCombineModifierflag = 1024
		}

		[Flags]
		public enum FreeType
		{
			Decommit = 16384,
			Release = 32768
		}

		public enum ABMsg
		{
			ABM_NEW,
			ABM_REMOVE,
			ABM_QUERYPOS,
			ABM_SETPOS,
			ABM_GETSTATE,
			ABM_GETTASKBARPOS,
			ABM_ACTIVATE,
			ABM_GETAUTOHIDEBAR,
			ABM_SETAUTOHIDEBAR,
			ABM_WINDOWPOSCHANGED,
			ABM_SETSTATE
		}

		public enum ABEdge
		{
			ABE_LEFT,
			ABE_TOP,
			ABE_RIGHT,
			ABE_BOTTOM
		}

		public enum ABState
		{
			ABS_MANUAL,
			ABS_AUTOHIDE,
			ABS_ALWAYSONTOP,
			ABS_AUTOHIDEANDONTOP
		}

		public enum SystemMetric
		{
			SM_CXICON = 11,
			SM_CYICON,
			SM_CXSMICON = 49,
			SM_CYSMICON
		}

		public struct NOTIFYICONIDENTIFIER
		{
			public uint cbSize;

			public IntPtr hWnd;

			public uint uID;

			public Guid guidItem;
		}

		public struct RECT
		{
			public static implicit operator Rect(NativeMethods.RECT rect)
			{
				if (rect.right - rect.left < 0 || rect.bottom - rect.top < 0)
				{
					return new Rect((double)rect.left, (double)rect.top, 0.0, 0.0);
				}
				return new Rect((double)rect.left, (double)rect.top, (double)(rect.right - rect.left), (double)(rect.bottom - rect.top));
			}

			public static implicit operator NativeMethods.RECT(Rect rect)
			{
				return new NativeMethods.RECT
				{
					left = (int)rect.Left,
					top = (int)rect.Top,
					right = (int)rect.Right,
					bottom = (int)rect.Bottom
				};
			}

			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		public struct POINT
		{
			public static implicit operator Point(NativeMethods.POINT point)
			{
				return new Point((double)point.x, (double)point.y);
			}

			public int x;

			public int y;
		}

		public struct TBBUTTON
		{
			public byte fsState
			{
				get
				{
					if (IntPtr.Size == 8)
					{
						return BitConverter.GetBytes(this.fsStateStylePadding.ToInt64())[0];
					}
					return BitConverter.GetBytes(this.fsStateStylePadding.ToInt32())[0];
				}
			}

			public byte fsStyle
			{
				get
				{
					if (IntPtr.Size == 8)
					{
						return BitConverter.GetBytes(this.fsStateStylePadding.ToInt64())[1];
					}
					return BitConverter.GetBytes(this.fsStateStylePadding.ToInt32())[1];
				}
			}

			public int iBitmap;

			public int idCommand;

			public IntPtr fsStateStylePadding;

			public IntPtr dwData;

			public IntPtr iString;
		}

		public struct APPBARDATA
		{
			public uint cbSize;

			public IntPtr hWnd;

			public uint uCallbackMessage;

			public NativeMethods.ABEdge uEdge;

			public NativeMethods.RECT rc;

			public IntPtr lParam;
		}

		public struct MONITORINFO
		{
			public uint cbSize;

			public NativeMethods.RECT rcMonitor;

			public NativeMethods.RECT rcWork;

			public uint dwFlags;
		}

		public struct MARGINS
		{
			public int cxLeftWidth;

			public int cxRightWidth;

			public int cyTopHeight;

			public int cyBottomHeight;
		}
	}
}
