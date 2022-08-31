using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Shared.Interops;

namespace Rewrite.SuperNotifyIcon.Finder
{
	public class WindowPositioning
	{
		public static bool IsNotificationAreaActive
		{
			get
			{
				IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
				IntPtr intPtr = NativeMethods.FindWindow("Shell_TrayWnd", string.Empty);
				IntPtr intPtr2 = NativeMethods.FindWindow("NotifyIconOverflowWindow", string.Empty);
				return foregroundWindow == intPtr || foregroundWindow == intPtr2;
			}
		}

		public static Point GetWindowPosition(NotifyIcon notifyicon, double windowwidth, double windowheight, double dpi, bool pinned)
		{
			WindowPositioning.TaskBarInfo taskBarInfo = WindowPositioning.GetTaskBarInfo();
			Rect? notifyIconRectangle = WindowPositioning.GetNotifyIconRectangle(notifyicon);
			Rect rect;
			if (notifyIconRectangle == null)
			{
				switch (taskBarInfo.Alignment)
				{
				default:
					rect = new Rect(taskBarInfo.Position.Right - 1.0, taskBarInfo.Position.Bottom - 1.0, 1.0, 1.0);
					break;
				case WindowPositioning.TaskBarAlignment.Top:
					rect = new Rect(taskBarInfo.Position.Right - 1.0, taskBarInfo.Position.Top, 1.0, 1.0);
					break;
				case WindowPositioning.TaskBarAlignment.Left:
					rect = new Rect(taskBarInfo.Position.Left, taskBarInfo.Position.Bottom - 1.0, 1.0, 1.0);
					break;
				case WindowPositioning.TaskBarAlignment.Right:
					rect = new Rect(taskBarInfo.Position.Right - 1.0, taskBarInfo.Position.Bottom - 1.0, 1.0, 1.0);
					break;
				}
			}
			else
			{
				rect = notifyIconRectangle.Value;
			}
			bool flag = WindowPositioning.IsNotifyIconInFlyOut(rect, taskBarInfo.Position);
			if (flag && pinned)
			{
				rect = WindowPositioning.GetNotifyAreaButtonRectangle().Value;
			}
			Point point = new Point(rect.Left + rect.Width / 2.0, rect.Top + rect.Height / 2.0);
			double num = (double)Compatibility.WindowEdgeOffset * dpi;
			Rect workingArea = WindowPositioning.GetWorkingArea(rect);
			double num2;
			double num3;
			switch (taskBarInfo.Alignment)
			{
			default:
				num2 = point.X - windowwidth / 2.0;
				if (flag)
				{
					num3 = rect.Top - windowheight - num;
				}
				else
				{
					num3 = taskBarInfo.Position.Top - windowheight - num;
				}
				break;
			case WindowPositioning.TaskBarAlignment.Top:
				num2 = point.X - windowwidth / 2.0;
				if (flag)
				{
					num3 = rect.Bottom + num;
				}
				else
				{
					num3 = taskBarInfo.Position.Bottom + num;
				}
				break;
			case WindowPositioning.TaskBarAlignment.Left:
				if (flag && !pinned)
				{
					num2 = point.X - windowwidth / 2.0;
					num3 = rect.Top - windowheight - num;
				}
				else
				{
					num2 = taskBarInfo.Position.Right + num;
					num3 = point.Y - windowheight / 2.0;
				}
				break;
			case WindowPositioning.TaskBarAlignment.Right:
				if (flag && !pinned)
				{
					num2 = point.X - windowwidth / 2.0;
					num3 = rect.Top - windowheight - num;
				}
				else
				{
					num2 = taskBarInfo.Position.Left - windowwidth - num;
					num3 = point.Y - windowheight / 2.0;
				}
				break;
			}
			if (num2 + windowwidth + num > workingArea.Right)
			{
				num2 = workingArea.Right - windowwidth - num;
			}
			else if (num2 < workingArea.Left)
			{
				num2 = workingArea.Left + num;
			}
			if (num3 + windowheight + num > workingArea.Bottom)
			{
				num3 = workingArea.Bottom - windowheight - num;
			}
			return new Point(num2, num3);
		}

		public static Rect GetWindowClientAreaSize(IntPtr hWnd)
		{
			NativeMethods.RECT rect = default(NativeMethods.RECT);
			if (NativeMethods.GetClientRect(hWnd, out rect))
			{
				return rect;
			}
			throw new Exception(string.Format("Could not find client area bounds for specified window (handle {0:X})", hWnd));
		}

		public static Rect GetWindowSize(IntPtr hWnd)
		{
			NativeMethods.RECT rect = default(NativeMethods.RECT);
			if (NativeMethods.GetWindowRect(hWnd, out rect))
			{
				return rect;
			}
			throw new Exception(string.Format("Could not find window bounds for specified window (handle {0:X})", hWnd));
		}

		public static Rect? GetNotifyIconRectangle(NotifyIcon notifyicon)
		{
			if (Compatibility.CurrentWindowsVersion == Compatibility.WindowsVersion.Windows7Plus)
			{
				return WindowPositioning.GetNotifyIconRectWindows7(notifyicon);
			}
			return WindowPositioning.GetNotifyIconRectLegacy(notifyicon);
		}

		public static Rect? GetNotifyIconRectWindows7(NotifyIcon notifyicon)
		{
			if (Compatibility.CurrentWindowsVersion != Compatibility.WindowsVersion.Windows7Plus)
			{
				throw new PlatformNotSupportedException("This method can only be used under Windows 7 or later. Please use GetNotifyIconRectangleLegacy() if you use an earlier operating system.");
			}
			int num = (int)notifyicon.GetType().GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(notifyicon);
			IntPtr handle = ((NativeWindow)notifyicon.GetType().GetField("window", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(notifyicon)).Handle;
			if (handle == IntPtr.Zero)
			{
				return null;
			}
			NativeMethods.RECT rect = default(NativeMethods.RECT);
			NativeMethods.NOTIFYICONIDENTIFIER notifyiconidentifier = new NativeMethods.NOTIFYICONIDENTIFIER
			{
				hWnd = handle,
				uID = (uint)num
			};
			notifyiconidentifier.cbSize = (uint)Marshal.SizeOf<NativeMethods.NOTIFYICONIDENTIFIER>(notifyiconidentifier);
			int num2 = NativeMethods.Shell_NotifyIconGetRect(ref notifyiconidentifier, out rect);
			if (num2 != 0 && num2 != 1)
			{
				return null;
			}
			return new Rect?(rect);
		}

		public static Rect? GetNotifyIconRectLegacy(NotifyIcon notifyicon)
		{
			Rect? rect = null;
			int num = (int)notifyicon.GetType().GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(notifyicon);
			IntPtr handle = ((NativeWindow)notifyicon.GetType().GetField("window", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(notifyicon)).Handle;
			if (handle == IntPtr.Zero)
			{
				Rect? rect2 = null;
				return rect2;
			}
			IntPtr intPtr = NativeMethods.FindWindow("Shell_TrayWnd", null);
			if (intPtr == (IntPtr)null)
			{
				Rect? rect2 = null;
				return rect2;
			}
			IntPtr intPtr2 = NativeMethods.FindWindowEx(intPtr, IntPtr.Zero, "TrayNotifyWnd", null);
			if (intPtr2 == (IntPtr)null)
			{
				Rect? rect2 = null;
				return rect2;
			}
			List<IntPtr> childToolbarWindows = NativeMethods.GetChildToolbarWindows(intPtr2);
			bool flag = false;
			int num2 = 0;
			while (!flag && num2 < childToolbarWindows.Count)
			{
				IntPtr intPtr3 = childToolbarWindows[num2];
				int num3 = NativeMethods.SendMessage(intPtr3, 1048U, IntPtr.Zero, IntPtr.Zero).ToInt32();
				uint num4;
				NativeMethods.GetWindowThreadProcessId(intPtr3, out num4);
				IntPtr intPtr4 = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.PROCESS_ALL_ACCESS, false, num4);
				if (intPtr4 == IntPtr.Zero)
				{
					Rect? rect2 = null;
					return rect2;
				}
				IntPtr intPtr5 = NativeMethods.VirtualAllocEx(intPtr4, (IntPtr)null, (uint)Marshal.SizeOf(typeof(NativeMethods.TBBUTTON)), NativeMethods.AllocationType.Commit, NativeMethods.MemoryProtection.ReadWrite);
				if (intPtr5 == IntPtr.Zero)
				{
					Rect? rect2 = null;
					return rect2;
				}
				try
				{
					int num5 = 0;
					while (!flag && num5 < num3)
					{
						int num6 = -1;
						NativeMethods.SendMessage(intPtr3, 1047U, new IntPtr(num5), intPtr5);
						NativeMethods.TBBUTTON tbbutton = default(NativeMethods.TBBUTTON);
						NativeMethods.ReadProcessMemory(intPtr4, intPtr5, out tbbutton, Marshal.SizeOf<NativeMethods.TBBUTTON>(tbbutton), out num6);
						if (num6 != Marshal.SizeOf<NativeMethods.TBBUTTON>(tbbutton))
						{
							Rect? rect2 = null;
							return rect2;
						}
						if (tbbutton.dwData == IntPtr.Zero)
						{
							return null;
						}
						IntPtr dwData = tbbutton.dwData;
						IntPtr intPtr6;
						NativeMethods.ReadProcessMemory(intPtr4, dwData, out intPtr6, Marshal.SizeOf(typeof(IntPtr)), out num6);
						if (num6 != Marshal.SizeOf(typeof(IntPtr)))
						{
							return null;
						}
						uint num7;
						NativeMethods.ReadProcessMemory(intPtr4, new IntPtr(dwData.ToInt64() + (long)Marshal.SizeOf(typeof(IntPtr))), out num7, Marshal.SizeOf(typeof(uint)), out num6);
						if (num6 != Marshal.SizeOf(typeof(uint)))
						{
							return null;
						}
						if (intPtr6 == handle && (ulong)num7 == (ulong)((long)num))
						{
							if ((tbbutton.fsState & 8) != 0)
							{
								rect = WindowPositioning.GetNotifyAreaButtonRectangle();
							}
							else
							{
								NativeMethods.RECT rect3 = default(NativeMethods.RECT);
								NativeMethods.SendMessage(intPtr3, 1053U, new IntPtr(num5), intPtr5);
								NativeMethods.ReadProcessMemory(intPtr4, intPtr5, out rect3, Marshal.SizeOf<NativeMethods.RECT>(rect3), out num6);
								if (num6 != Marshal.SizeOf<NativeMethods.RECT>(rect3))
								{
									return null;
								}
								NativeMethods.MapWindowPoints(intPtr3, (IntPtr)null, ref rect3, 2U);
								rect = new Rect?(rect3);
							}
							flag = true;
						}
						num5++;
					}
				}
				finally
				{
					NativeMethods.VirtualFreeEx(intPtr4, intPtr5, 0, NativeMethods.FreeType.Release);
					NativeMethods.CloseHandle(intPtr4);
				}
				num2++;
			}
			return rect;
		}

		public static Rect? GetNotifyAreaButtonRectangle()
		{
			IntPtr intPtr = NativeMethods.FindWindow("Shell_TrayWnd", null);
			if (intPtr == (IntPtr)null)
			{
				return null;
			}
			IntPtr intPtr2 = NativeMethods.FindWindowEx(intPtr, IntPtr.Zero, "TrayNotifyWnd", null);
			if (intPtr2 == (IntPtr)null)
			{
				return null;
			}
			List<IntPtr> childButtonWindows = NativeMethods.GetChildButtonWindows(intPtr2);
			if (childButtonWindows.Count == 0)
			{
				return null;
			}
			NativeMethods.RECT rect;
			if (!NativeMethods.GetWindowRect(childButtonWindows[0], out rect))
			{
				return null;
			}
			return new Rect?(rect);
		}

		public static bool IsNotifyIconInFlyOut(NotifyIcon notifyicon, WindowPositioning.TaskBarInfo taskbarinfo)
		{
			if (Compatibility.CurrentWindowsVersion != Compatibility.WindowsVersion.Windows7Plus)
			{
				return false;
			}
			Rect? notifyIconRectangle = WindowPositioning.GetNotifyIconRectangle(notifyicon);
			return notifyIconRectangle != null && WindowPositioning.IsNotifyIconInFlyOut(notifyIconRectangle.Value, taskbarinfo.Position);
		}

		public static bool IsNotifyIconInFlyOut(Rect notifyiconrect, Rect taskbarrect)
		{
			return Compatibility.CurrentWindowsVersion == Compatibility.WindowsVersion.Windows7Plus && (notifyiconrect.Left > taskbarrect.Right || notifyiconrect.Right < taskbarrect.Left || notifyiconrect.Bottom < taskbarrect.Top || notifyiconrect.Top > taskbarrect.Bottom);
		}

		public static bool IsPointInNotifyIcon(Point point, NotifyIcon notifyicon)
		{
			Rect? notifyIconRectangle = WindowPositioning.GetNotifyIconRectangle(notifyicon);
			return notifyIconRectangle != null && notifyIconRectangle.Value.Contains(point);
		}

		public static Point GetCursorPosition()
		{
			NativeMethods.POINT point;
			if (NativeMethods.GetCursorPos(out point))
			{
				return point;
			}
			throw new Exception("Failed to retrieve mouse position");
		}

		public static bool IsCursorOverNotifyIcon(NotifyIcon notifyicon)
		{
			return WindowPositioning.IsPointInNotifyIcon(WindowPositioning.GetCursorPosition(), notifyicon);
		}

		public static WindowPositioning.TaskBarInfo GetTaskBarInfo()
		{
			NativeMethods.APPBARDATA appbardata = new NativeMethods.APPBARDATA
			{
				hWnd = IntPtr.Zero
			};
			appbardata.cbSize = (uint)Marshal.SizeOf<NativeMethods.APPBARDATA>(appbardata);
			if (NativeMethods.SHAppBarMessage(NativeMethods.ABMsg.ABM_GETTASKBARPOS, ref appbardata) == IntPtr.Zero)
			{
				throw new Exception("Could not retrieve taskbar information.");
			}
			Rect rect = appbardata.rc;
			WindowPositioning.TaskBarAlignment taskBarAlignment;
			switch (appbardata.uEdge)
			{
			case NativeMethods.ABEdge.ABE_LEFT:
				taskBarAlignment = WindowPositioning.TaskBarAlignment.Left;
				break;
			case NativeMethods.ABEdge.ABE_TOP:
				taskBarAlignment = WindowPositioning.TaskBarAlignment.Top;
				break;
			case NativeMethods.ABEdge.ABE_RIGHT:
				taskBarAlignment = WindowPositioning.TaskBarAlignment.Right;
				break;
			case NativeMethods.ABEdge.ABE_BOTTOM:
				taskBarAlignment = WindowPositioning.TaskBarAlignment.Bottom;
				break;
			default:
				throw new ArgumentOutOfRangeException("Couldn't retrieve location of taskbar.");
			}
			return new WindowPositioning.TaskBarInfo
			{
				Position = rect,
				Alignment = taskBarAlignment
			};
		}

		public static Rect GetWorkingArea(Rect rectangle)
		{
			NativeMethods.RECT rect = rectangle;
			IntPtr intPtr = NativeMethods.MonitorFromRect(ref rect, 2U);
			NativeMethods.MONITORINFO monitorinfo = default(NativeMethods.MONITORINFO);
			monitorinfo.cbSize = (uint)Marshal.SizeOf<NativeMethods.MONITORINFO>(monitorinfo);
			if (!NativeMethods.GetMonitorInfo(intPtr, ref monitorinfo))
			{
				throw new Exception("Failed to retrieve monitor information.");
			}
			return monitorinfo.rcWork;
		}

		public enum TaskBarAlignment
		{
			Bottom,
			Top,
			Left,
			Right
		}

		public struct TaskBarInfo
		{
			public Rect Position;

			public WindowPositioning.TaskBarAlignment Alignment;
		}
	}
}
