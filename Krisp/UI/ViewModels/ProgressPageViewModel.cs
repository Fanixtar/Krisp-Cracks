using System;

namespace Krisp.UI.ViewModels
{
	internal class ProgressPageViewModel : BindableBase, IPageViewModel
	{
		public MenuItemsVisibility MenuItemsVisibility { get; } = new MenuItemsVisibility();
	}
}
