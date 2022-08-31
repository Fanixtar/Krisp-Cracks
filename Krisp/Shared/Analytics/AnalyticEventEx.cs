using System;
using Shared.Helpers;

namespace Shared.Analytics
{
	public class AnalyticEventEx : AnalyticEvent
	{
		public AnalyticEventEx(string name)
			: base(name)
		{
			this.installID = InstallationID.ID;
			this.krisp_version = EnvHelper.KrispVersion.ToString();
		}

		public string installID { get; set; }

		public string krisp_version { get; set; }
	}
}
