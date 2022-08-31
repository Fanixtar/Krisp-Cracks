using System;
using Shared.Interops;

namespace Krisp.Models
{
	public interface IStreamProcessor
	{
		event EventHandler<bool> SPInitializedNotification;

		event EventHandler<SPMessage> SPErrorNotification;

		event EventHandler<SPMessage> SPInboundNotification;

		event EventHandler<SPMessage> SPOutboundNotification;

		event EventHandler<bool> SPStreamDucked;

		bool IsInitialized { get; }

		bool IsInitializationFailed { get; }

		bool SPInitialize();

		int SPRelease();

		HRESULT SessionSetDevice(EnStreamDirection kind, uint sesID, string modelName, string devID);

		HRESULT SessionRelease(EnStreamDirection kind, uint sesID);

		HRESULT SetActivityState(EnStreamDirection kind, bool state);

		HRESULT SetFeatureState(EnStreamDirection kind, SPFeature feature, bool state);

		HRESULT RecordSession(EnStreamDirection dir, uint sesID, SPFeature feature, uint nSamplesPerSec, string deviceID, string sourceFilePath, string beforeNCFilePath, string afterNCFilePath);

		uint GenerateRecordingSessionId();

		int GetStreamActivityLevel(EnStreamDirection kind);

		bool IsSPFeatureAvailable(EnStreamDirection kind, SPFeature feature, uint samplingrate);
	}
}
