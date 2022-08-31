using System;

namespace Krisp.BackEnd
{
	public class VersionInfo
	{
		public string os { get; set; }

		public string version { get; set; }

		public string package_64 { get; set; }

		public string package_86 { get; set; }

		public Resource release_notes { get; set; }

		public int resultCode { get; set; }
	}
}
