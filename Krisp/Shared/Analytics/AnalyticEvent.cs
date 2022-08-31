using System;
using System.Globalization;

namespace Shared.Analytics
{
	public class AnalyticEvent
	{
		public AnalyticEvent(string nm)
		{
			this.name = nm;
			this.tm = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CreateSpecificCulture("en-US"));
		}

		public string name { get; set; }

		public string tm { get; set; }

		public string os
		{
			get
			{
				return "win";
			}
		}

		public string label1 { get; set; }

		public string label2 { get; set; }

		public string label3 { get; set; }

		public string label4 { get; set; }

		public string large_label1 { get; set; }

		public string large_label2 { get; set; }

		public uint value1 { get; set; }

		public uint value2 { get; set; }

		public uint value3 { get; set; }

		public uint value4 { get; set; }

		public uint value5 { get; set; }

		public uint value6 { get; set; }

		public uint? user_id { get; set; }

		public uint? team_id { get; set; }
	}
}
