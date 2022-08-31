using System;

namespace Krisp.BackEnd
{
	public class ProfileSettings
	{
		public NCOutSetting nc_out { get; set; }

		public BaseProfileSetting nc_in { get; set; }

		public UpdateSetting update { get; set; }
	}
}
