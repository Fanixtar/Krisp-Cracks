using System;
using Krisp.BackEnd;

namespace Krisp.UI.Models
{
	public class UpdateInfo
	{
		public UpdateInfo()
		{
		}

		public UpdateInfo(VersionInfo versionInfo)
		{
			if (versionInfo != null)
			{
				this.OS = versionInfo.os;
				this.ReleaseNotes = versionInfo.release_notes.url;
				this.Version = versionInfo.version;
				this.Package = (Environment.Is64BitOperatingSystem ? versionInfo.package_64 : versionInfo.package_86);
				this.RetrivingResult = versionInfo.resultCode;
			}
		}

		public string OS { get; set; }

		public string ReleaseNotes { get; set; }

		public string Package { get; set; }

		public string Version { get; set; }

		public int RetrivingResult { get; set; }
	}
}
