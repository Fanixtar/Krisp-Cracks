using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Krisp.UI.Views.Controls
{
	public partial class AudioDeviceSelector : UserControl
	{
		public AudioDeviceSelector()
		{
			this.InitializeComponent();
			base.SetBinding(AudioDeviceSelector.ActivityLevelProperty, new Binding("ActivityLevel"));
		}

		public double ActivityLevel
		{
			get
			{
				return (double)base.GetValue(AudioDeviceSelector.ActivityLevelProperty);
			}
			set
			{
				base.SetValue(AudioDeviceSelector.ActivityLevelProperty, value);
			}
		}

		private static void OnActivityLevelChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as AudioDeviceSelector).OnActivityLevelChange((double)e.OldValue, (double)e.NewValue);
		}

		private void OnActivityLevelChange(double oldLevel, double newLevel)
		{
			if (this._storyBoard != null)
			{
				this._storyBoard.Pause();
			}
			DoubleAnimation doubleAnimation = new DoubleAnimation
			{
				From = new double?(this.ActivityLevelRect.Height),
				To = new double?(newLevel),
				Duration = TimeSpan.FromMilliseconds(100.0)
			};
			Storyboard.SetTarget(doubleAnimation, this.ActivityLevelRect);
			Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(FrameworkElement.HeightProperty));
			this._storyBoard = new Storyboard();
			this._storyBoard.Children.Add(doubleAnimation);
			this._storyBoard.Begin();
		}

		private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.Toggle.IsChecked = !this.Toggle.IsChecked;
		}

		private Storyboard _storyBoard;

		public static readonly DependencyProperty ActivityLevelProperty = DependencyProperty.Register("ActivityLevel", typeof(double), typeof(AudioDeviceSelector), new PropertyMetadata(new PropertyChangedCallback(AudioDeviceSelector.OnActivityLevelChange)));
	}
}
