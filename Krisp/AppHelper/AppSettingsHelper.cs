using System;
using System.Collections.Generic;
using Krisp.BackEnd;
using Newtonsoft.Json;

namespace Krisp.AppHelper
{
	public class AppSettingsHelper
	{
		public AppSettingsHelper(List<AppSettings> settings)
		{
			this.AppSettings = settings;
		}

		public List<string> IgnoreList
		{
			get
			{
				foreach (AppSettings appSettings in this.AppSettings)
				{
					if (appSettings.name == "ignore_list")
					{
						return JsonConvert.DeserializeObject<List<string>>(appSettings.value.ToString());
					}
				}
				return null;
			}
		}

		public static AppSettingsHelper Instance { get; set; }

		public List<AppSettings> AppSettings;
	}
}
