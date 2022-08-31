using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Krisp.UI.Views.Controls
{
	public partial class CircularProgressBar : UserControl
	{
		public Color DotColor
		{
			get
			{
				return (Color)base.GetValue(CircularProgressBar.DotColorProperty);
			}
			set
			{
				base.SetValue(CircularProgressBar.DotColorProperty, value);
			}
		}

		public CircularProgressBar()
		{
			this.InitializeComponent();
			base.IsVisibleChanged += this.OnVisibleChanged;
			this._animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, base.Dispatcher)
			{
				Interval = new TimeSpan(0, 0, 0, 0, 75)
			};
		}

		private void Start()
		{
			this._animationTimer.Tick += this.OnAnimationTick;
			this._animationTimer.Start();
		}

		private void Stop()
		{
			this._animationTimer.Stop();
			this._animationTimer.Tick -= this.OnAnimationTick;
		}

		private void OnAnimationTick(object sender, EventArgs e)
		{
			this._spinnerRotate.Angle = (this._spinnerRotate.Angle + 45.0) % 360.0;
		}

		private void OnCanvasUnloaded(object sender, RoutedEventArgs e)
		{
			this.Stop();
			base.IsVisibleChanged -= this.OnVisibleChanged;
		}

		private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				this.Start();
				return;
			}
			this.Stop();
		}

		private readonly DispatcherTimer _animationTimer;

		[Bindable(true)]
		[Category("Appearance")]
		public static readonly DependencyProperty DotColorProperty = DependencyProperty.Register("DotColor", typeof(Color), typeof(CircularProgressBar), new PropertyMetadata(Colors.White));
	}
}
