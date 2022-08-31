using System;
using System.Windows;
using System.Windows.Interop;
using Shared.Interops;

namespace Krisp.UI
{
	public static class UIExtetnsions
	{
		public static void BringWindowToTop(this Window w)
		{
			User32.BringWindowToTop(((HwndSource)PresentationSource.FromVisual(w)).Handle);
		}
	}
}
