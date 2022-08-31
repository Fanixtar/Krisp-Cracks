using System;

namespace Krisp.UI.ViewModels
{
	public class UnlimitedModeViewModel : AppModeViewModel
	{
		public UnlimitedModeViewModel()
		{
			base.Mode = "unlimited";
			base.Disabled = false;
		}
	}
}
