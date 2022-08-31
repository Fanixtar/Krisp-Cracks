using System;

namespace Krisp.UI.ViewModels
{
	public class MenuItemsVisibility
	{
		public bool About { get; set; } = true;

		public bool CheckForUpdate { get; set; } = true;

		public bool ReportBug { get; set; } = true;

		public bool ContactSupport { get; set; } = true;

		public bool SetupKrisp { get; set; } = true;

		public bool Preferences { get; set; } = true;

		public bool SignOut { get; set; }

		public bool Quit { get; set; } = true;

		private bool _contactSupport = true;
	}
}
