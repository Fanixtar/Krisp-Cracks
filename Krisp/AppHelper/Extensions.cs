using System;
using System.Runtime.InteropServices;
using Krisp.Models;
using Shared.Interops;

namespace Krisp.AppHelper
{
	public static class Extensions
	{
		public static bool Is(this Exception ex, HRESULT type)
		{
			int num = type;
			if (num <= -2147023728)
			{
				if (num != -2147483638 && num != -2147023728)
				{
					goto IL_B4;
				}
			}
			else if (num != -2004287484 && num != 143196173)
			{
				goto IL_B4;
			}
			COMException ex2 = ex as COMException;
			int? num2 = ((ex2 != null) ? new int?(ex2.HResult) : null);
			uint? num3 = ((num2 != null) ? new uint?((uint)num2.GetValueOrDefault()) : null);
			uint hashCode = (uint)type.GetHashCode();
			return (num3.GetValueOrDefault() == hashCode) & (num3 != null);
			IL_B4:
			throw new NotImplementedException();
		}

		public static string PresentationName(this AudioDeviceKind kind)
		{
			if (kind == AudioDeviceKind.Microphone)
			{
				return "Microphone";
			}
			if (kind != AudioDeviceKind.Speaker)
			{
				throw new Exception("Unknown Audio device kind.");
			}
			return "Speaker";
		}
	}
}
