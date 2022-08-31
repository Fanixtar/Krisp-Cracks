using System;
using Shared.Helpers;

namespace Krisp.BackEnd
{
	public class UserProfileInfo
	{
		public string ref_string { get; set; }

		public uint id { get; set; }

		public string email
		{
			get
			{
				if (!DeviceLoginHelper.DeviceMode)
				{
					return this._email;
				}
				if (!DeviceLoginHelper.FQDNBasedInstallID)
				{
					return SystemInfo.HostName;
				}
				return SystemInfo.FQDN;
			}
			set
			{
				this._email = value;
			}
		}

		public Mode mode { get; set; }

		public Team team { get; set; }

		public ProfileSettings settings { get; set; }

		private string _email;
	}
}
