using System;

namespace Krisp.Models
{
	public interface IAppInfo
	{
		string ExeName { get; }

		string ExePath { get; }

		string Description { get; }

		uint PID { get; }

		ProcessType Type { get; }
	}
}
