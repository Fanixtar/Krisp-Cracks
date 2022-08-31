using System;
using System.Collections.Generic;

namespace Krisp.BackEnd
{
	public class InstallationInfo
	{
		public string installation_id { get; set; }

		public string app_version { get; set; }

		public string app_id { get; set; }

		public string timezone { get; set; }

		public string locale { get; set; }

		public string push_token { get; set; }

		public string os { get; set; }

		public string os_version { get; set; }

		public string type { get; set; }

		public string cpu { get; set; }

		public string gpu { get; set; }

		public string secret { get; set; }

		public string device_name { get; set; }

		public List<AppSettings> settings { get; set; }
	}
}
