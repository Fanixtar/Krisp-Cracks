using System;

namespace Krisp.Models
{
	public interface IAppCore
	{
		IKrispController SpeakerController { get; }

		IKrispController MicController { get; }

		void Dispose();

		bool Initialize();

		string GetDiagnosticInfo();
	}
}
