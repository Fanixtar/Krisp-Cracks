using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using Krisp.AppHelper;
using Rewrite.SuperNotifyIcon.Finder;

namespace Krisp.UI.Views.Windows
{
	public partial class KrispWindow : Window
	{
		public bool AutomaticalyHide { get; set; } = true;

		public KrispWindow()
		{
			this.InitializeComponent();
			base.Deactivated += delegate(object sender, EventArgs e)
			{
				if (this.AutomaticalyHide)
				{
					base.Hide();
				}
			};
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
			if (hwndSource == null)
			{
				return;
			}
			hwndSource.AddHook(delegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				if ((long)msg == (long)((ulong)SingleInstance.WM_SHOWFIRSTINSTANCE))
				{
					base.Activate();
					base.Show();
					handled = true;
				}
				return IntPtr.Zero;
			});
		}

		public void FixFramePosition(NotifyIcon notifyIcon)
		{
			double num = 1.0;
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				num = (double)(graphics.DpiX / 96f);
			}
			try
			{
				System.Windows.Point windowPosition = WindowPositioning.GetWindowPosition(notifyIcon, num * base.Width, num * base.Height, num, true);
				base.Top = windowPosition.Y / num;
				base.Left = windowPosition.X / num;
			}
			catch
			{
			}
		}
	}
}
