using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Krisp.Models;
using Shared.Interops;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	internal class AppInfo : IAppInfo
	{
		public string ExeName { get; protected set; }

		public string Description { get; protected set; }

		public string ExePath { get; protected set; }

		public uint PID { get; private set; }

		public ProcessType Type { get; private set; }

		public AppInfo(uint pid)
		{
			this.PID = pid;
			if (pid == 0U)
			{
				this.Type = ProcessType.System;
			}
			else if (AppInfo.IsImmersiveProcess(pid))
			{
				this.Type = ProcessType.Immersive;
			}
			else
			{
				this.Type = ProcessType.Desktop;
			}
			this.ExePath = AppInfo.GetExecutablePathByPid(pid);
			if (!string.IsNullOrWhiteSpace(this.ExePath))
			{
				try
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(this.ExePath);
					this.Description = versionInfo.FileDescription;
				}
				catch
				{
				}
				this.ExeName = Path.GetFileNameWithoutExtension(this.ExePath);
			}
		}

		public static AppInfo CreateAppInfo(IAudioSessionControl sessCtl)
		{
			uint num = 0U;
			((IAudioSessionControl2)sessCtl).GetProcessId(out num);
			return new AppInfo(num);
		}

		private static bool IsImmersiveProcess(uint processId)
		{
			IntPtr intPtr = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
			if (intPtr == IntPtr.Zero)
			{
				return false;
			}
			bool flag;
			try
			{
				flag = User32.IsImmersiveProcess(intPtr) != 0;
			}
			finally
			{
				Kernel32.CloseHandle(intPtr);
			}
			return flag;
		}

		private static string GetExecutablePathByPid(uint pid)
		{
			IntPtr intPtr = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION | Kernel32.ProcessAccessFlags.SYNCHRONIZE, false, pid);
			if (intPtr != IntPtr.Zero)
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder(260);
					uint capacity = (uint)stringBuilder.Capacity;
					if (Kernel32.QueryFullProcessImageName(intPtr, 0U, stringBuilder, ref capacity) != 0 && stringBuilder.Length > 0)
					{
						return stringBuilder.ToString();
					}
				}
				finally
				{
					Kernel32.CloseHandle(intPtr);
				}
			}
			return string.Empty;
		}
	}
}
