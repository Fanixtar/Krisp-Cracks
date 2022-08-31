using System;

namespace Krisp.Models
{
	public class StatusMessage
	{
		public string Message { get; set; }

		public StatusMessageType StatusType { get; set; }

		public StatusMessageFlags Flags { get; set; }
	}
}
