using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Sentry;
using Shared.Helpers;
using Shared.Interops;

namespace Krisp.AppHelper
{
	public static class SingleInstance
	{
		private static string Guid
		{
			get
			{
				object[] customAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
				if (customAttributes.Length == 0)
				{
					return "67F3CFDC-C5DD-49EF-8660-D7460FB530DE";
				}
				return ((GuidAttribute)customAttributes[0]).Value;
			}
		}

		public static bool Start()
		{
			string text = string.Format("Local\\{0}", SingleInstance.Guid);
			bool flag;
			SingleInstance.mutex = new Mutex(true, text, out flag);
			if (!flag)
			{
				try
				{
					Process currentProcess = Process.GetCurrentProcess();
					Process[] array = (from p in Process.GetProcessesByName("Krisp")
						where p.SessionId == currentProcess.SessionId && p.Id != currentProcess.Id && p.MainModule.FileName == EnvHelper.KrispExeFullPath
						select p).ToArray<Process>();
					if (array.Length > 1)
					{
						return false;
					}
					if (array.Length != 1)
					{
						SentrySdk.CaptureMessage("Krisp process was detected which quited gracefully.", 1);
						SingleInstance.mutex.Dispose();
						bool flag2;
						SingleInstance.mutex = new Mutex(true, text, out flag2);
						return flag2;
					}
					Process process = array[0];
					if (User32.FindWindow(null, "Krisp") != IntPtr.Zero && !process.HasExited)
					{
						return false;
					}
					process.WaitForExit(5000);
					if (!process.HasExited)
					{
						if (User32.FindWindow(null, "Krisp") != IntPtr.Zero)
						{
							return false;
						}
						SentrySdk.CaptureMessage("Found another instance of Krisp which was not responding. Killing that process and moving on.", 2);
						SingleInstance.KillProcess(process);
					}
					SingleInstance.mutex.Dispose();
					bool flag3;
					SingleInstance.mutex = new Mutex(true, text, out flag3);
					return flag3;
				}
				catch (Exception)
				{
				}
				return flag;
			}
			return flag;
		}

		private static bool KillProcess(Process process)
		{
			try
			{
				process.Kill();
				process.WaitForExit(5000);
				return process.HasExited;
			}
			catch (Win32Exception ex)
			{
				SentrySdk.CaptureMessage("Win32Exception thrown during process Kill. Error: " + ex.Message, 2);
				return false;
			}
			catch (InvalidOperationException)
			{
			}
			catch (NotSupportedException ex2)
			{
				SentrySdk.CaptureMessage("NotSupportedException thrown during process Kill. Error: " + ex2.Message, 3);
			}
			return true;
		}

		public static void ShowFirstInstance()
		{
			User32.SendMessage(User32.FindWindow(null, "Krisp"), (int)SingleInstance.WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);
		}

		public static void Stop()
		{
			SingleInstance.mutex.ReleaseMutex();
		}

		public static readonly uint WM_SHOWFIRSTINSTANCE = User32.RegisterWindowMessage(string.Format("WM_SHOWFIRSTINSTANCE|{0}", SingleInstance.Guid));

		private static Mutex mutex;
	}
}
