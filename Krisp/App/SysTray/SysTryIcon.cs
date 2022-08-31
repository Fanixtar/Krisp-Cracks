using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.Views.Windows;
using Rewrite.SuperNotifyIcon.Finder;

namespace Krisp.App.SysTray
{
	public class SysTryIcon : FrameworkElement
	{
		[Description("Gets or sets SysTry icon.")]
		public Icon Icon
		{
			get
			{
				return (Icon)base.GetValue(SysTryIcon.IconProperty);
			}
			set
			{
				base.SetValue(SysTryIcon.IconProperty, value);
			}
		}

		private static void IconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((SysTryIcon)d).OnIconChange(e);
		}

		private void OnIconChange(DependencyPropertyChangedEventArgs e)
		{
			this._notifyIcon.Icon = (Icon)e.NewValue;
		}

		public SysTryIcon(KrispWindow krispWindow)
		{
			base.SetBinding(SysTryIcon.IconProperty, new System.Windows.Data.Binding("Icon"));
			this._krispWindow = krispWindow;
			this._notifyIcon = new NotifyIcon(new Container())
			{
				Icon = this.Icon,
				Text = "Krisp",
				Visible = true
			};
			this._notifyIcon.MouseClick += this.MouseClick;
			this._notifyIcon.BalloonTipClicked += this.BalloonTipClicked;
			this._krispWindow.SizeChanged += this.WindowSizeChanged;
		}

		public void PushNotification(INotification notification)
		{
			this._notification = notification;
			this._notifyIcon.ShowBalloonTip(3000, this._notification.Title, this._notification.Text, ToolTipIcon.None);
		}

		public void UnregisterEvents()
		{
			this._notifyIcon.MouseClick -= this.MouseClick;
			this._notifyIcon.BalloonTipClicked -= this.BalloonTipClicked;
			this._krispWindow.SizeChanged -= this.WindowSizeChanged;
		}

		public void Hide()
		{
			this._notifyIcon.Visible = false;
		}

		private void MouseClick(object sender, MouseEventArgs e)
		{
			Dispatcher.CurrentDispatcher.InvokeAsync(delegate()
			{
				this._krispWindow.FixFramePosition(this._notifyIcon);
				this._krispWindow.Show();
				this._krispWindow.Activate();
			});
		}

		private void BalloonTipClicked(object sender, EventArgs e)
		{
			Action handler = this._notification.Handler;
			if (handler == null)
			{
				return;
			}
			handler();
		}

		private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
		{
			double num = 1.0;
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				num = (double)(graphics.DpiX / 96f);
			}
			try
			{
				System.Windows.Point windowPosition = WindowPositioning.GetWindowPosition(this._notifyIcon, num * e.NewSize.Width, num * e.NewSize.Height, num, true);
				this._krispWindow.Top = windowPosition.Y / num;
				this._krispWindow.Left = windowPosition.X / num;
			}
			catch
			{
			}
		}

		private void CreateContextMenu()
		{
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("Exit");
			toolStripMenuItem.Click += delegate(object sender, EventArgs e)
			{
				System.Windows.Application.Current.Shutdown();
			};
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("Show Always");
			toolStripMenuItem2.Click += delegate(object sender, EventArgs e)
			{
				ToolStripMenuItem toolStripMenuItem5;
				if ((toolStripMenuItem5 = sender as ToolStripMenuItem) != null)
				{
					string text = toolStripMenuItem5.Text;
					if (text == "Show Always")
					{
						toolStripMenuItem5.Text = "Hide Automatically";
						this._krispWindow.AutomaticalyHide = false;
						this._krispWindow.Show();
						this._krispWindow.Activate();
						return;
					}
					if (!(text == "Hide Automatically"))
					{
						return;
					}
					toolStripMenuItem5.Text = "Show Always";
					this._krispWindow.AutomaticalyHide = true;
					this._krispWindow.Hide();
				}
			};
			ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem("Unload Core");
			toolStripMenuItem3.Click += delegate(object sender, EventArgs e)
			{
				((KrispApp)System.Windows.Application.Current).UnLoadKrispInternals("CreateContextMenu");
			};
			ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem("Load Core");
			toolStripMenuItem4.Click += delegate(object sender, EventArgs e)
			{
				((KrispApp)System.Windows.Application.Current).LoadKrispInternals("CreateContextMenu");
			};
			this._notifyIcon.ContextMenuStrip = new ContextMenuStrip();
			this._notifyIcon.ContextMenuStrip.Items.Add(toolStripMenuItem2);
			this._notifyIcon.ContextMenuStrip.Items.Add(toolStripMenuItem4);
			this._notifyIcon.ContextMenuStrip.Items.Add(toolStripMenuItem3);
			this._notifyIcon.ContextMenuStrip.Items.Add(toolStripMenuItem);
		}

		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Icon), typeof(SysTryIcon), new PropertyMetadata(Krisp.Properties.Resources.Krisp, new PropertyChangedCallback(SysTryIcon.IconPropertyChanged)));

		private readonly KrispWindow _krispWindow;

		private readonly NotifyIcon _notifyIcon;

		private INotification _notification;
	}
}
