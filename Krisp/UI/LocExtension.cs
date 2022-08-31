using System;
using System.Windows.Data;
using Krisp.UI.ViewModels;

namespace Krisp.UI
{
	public class LocExtension : Binding
	{
		public LocExtension(string name)
			: base("[" + name + "]")
		{
			base.Mode = BindingMode.OneWay;
			base.Source = TranslationSourceViewModel.Instance;
		}
	}
}
