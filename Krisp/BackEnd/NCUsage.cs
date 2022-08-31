using System;

namespace Krisp.BackEnd
{
	public struct NCUsage
	{
		public uint range_id { get; set; }

		public uint next_index { get; set; }

		public uint relative_timestamp { get; set; }

		public uint used_seconds { get; set; }

		public string apps { get; set; }
	}
}
