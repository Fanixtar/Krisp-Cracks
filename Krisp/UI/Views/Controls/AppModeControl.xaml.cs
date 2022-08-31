using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Krisp.UI.ViewModels;
using Shared.Helpers;

namespace Krisp.UI.Views.Controls
{
	public partial class AppModeControl : UserControl
	{
		public AppModeControl()
		{
			this.InitializeComponent();
		}

		private void LearnMoreClick(object sender, RoutedEventArgs e)
		{
			Helpers.OpenUrl(UrlProvider.GetLearnMoreUrl(TranslationSourceViewModel.Instance.SelectedCulture.Name));
		}
	}
}
