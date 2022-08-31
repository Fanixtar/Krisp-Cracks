using System;
using System.Windows;
using System.Windows.Controls;

namespace Krisp.UI.Views
{
	public static class TextBlockHelper
	{
		public static bool GetAutoTooltip(DependencyObject obj)
		{
			return (bool)obj.GetValue(TextBlockHelper.AutoTooltipProperty);
		}

		public static void SetAutoTooltip(DependencyObject obj, bool value)
		{
			obj.SetValue(TextBlockHelper.AutoTooltipProperty, value);
		}

		private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TextBlock textBlock = d as TextBlock;
			if (textBlock == null)
			{
				return;
			}
			if (e.NewValue.Equals(true))
			{
				textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
				TextBlockHelper.ComputeAutoTooltip(textBlock);
				textBlock.SizeChanged += TextBlockHelper.TextBlock_SizeChanged;
				return;
			}
			textBlock.SizeChanged -= TextBlockHelper.TextBlock_SizeChanged;
		}

		private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			TextBlockHelper.ComputeAutoTooltip(sender as TextBlock);
		}

		private static void ComputeAutoTooltip(TextBlock textBlock)
		{
			textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			ToolTipService.SetToolTip(textBlock, (textBlock.ActualWidth < textBlock.DesiredSize.Width) ? textBlock.Text : null);
		}

		public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached("AutoTooltip", typeof(bool), typeof(TextBlockHelper), new PropertyMetadata(false, new PropertyChangedCallback(TextBlockHelper.OnAutoTooltipPropertyChanged)));
	}
}
