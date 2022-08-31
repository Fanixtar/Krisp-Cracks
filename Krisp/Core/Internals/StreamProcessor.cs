using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.Properties;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class StreamProcessor : IStreamProcessor
	{
		public event EventHandler<bool> SPInitializedNotification;

		public event EventHandler<SPMessage> SPErrorNotification;

		public event EventHandler<SPMessage> SPInboundNotification;

		public event EventHandler<SPMessage> SPOutboundNotification;

		public event EventHandler<bool> SPStreamDucked;

		public bool IsInitialized
		{
			get
			{
				return StreamProcessor._spInitialized;
			}
		}

		public SDKModelManager ModelManager
		{
			get
			{
				return this._modelManager;
			}
		}

		public bool IsInitializationFailed
		{
			get
			{
				return StreamProcessor._spInitializationFailed;
			}
		}

		public static StreamProcessor Instance
		{
			get
			{
				if (StreamProcessor._instance == null)
				{
					StreamProcessor._instance = new StreamProcessor();
				}
				return StreamProcessor._instance;
			}
		}

		private StreamProcessor()
		{
			this._logger = LogWrapper.GetLogger("SP");
		}

		public int SPRelease()
		{
			if (StreamProcessor._initCalled > 0)
			{
				if (Interlocked.Decrement(ref StreamProcessor._initCalled) == 0)
				{
					SPInterop.Destroy();
					StreamProcessor._spInitialized = false;
					StreamProcessor._StreamProcessorCB = null;
				}
			}
			else
			{
				StreamProcessor._initCalled = 0;
			}
			return StreamProcessor._initCalled;
		}

		public bool SPInitialize()
		{
			if (Interlocked.Increment(ref StreamProcessor._initCalled) > 1)
			{
				if (StreamProcessor._spInitializationFailed)
				{
					EventHandler<SPMessage> sperrorNotification = this.SPErrorNotification;
					if (sperrorNotification != null)
					{
						sperrorNotification(this, new SPMessage(0U, SPNotificationType.SP_NOTIFICATION_SP_INIT_ERROR, -2147418113, "Error on SPInitialize."));
					}
				}
				return false;
			}
			HRESULT hresult = -2147467259;
			try
			{
				string text = ConfigurationManager.AppSettings["Krisp.Models.ConfigFile"];
				string text2 = ((text != null) ? text.ToString() : null);
				string text3 = ConfigurationManager.AppSettings["Krisp.Models.Location"];
				string text4 = ((text3 != null) ? text3.ToString() : null);
				if (string.IsNullOrWhiteSpace(text2) || string.IsNullOrWhiteSpace(text4))
				{
					this._logger.LogError("Incorrect models configuration");
					throw new FileNotFoundException("Incorrect models configuration or file not found.");
				}
				text2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, text2);
				text4 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, text4);
				if (!File.Exists(text2) || !Directory.Exists(text4))
				{
					this._logger.LogError("Unavailable models configuration");
					throw new FileNotFoundException("Unavailable models configuration.");
				}
				this._modelManager = new SDKModelManager(text2, text4);
				List<SPSDKModel> list = new List<SPSDKModel>();
				List<SDKModel> list2;
				this._modelManager.GetModelsList(out list2);
				foreach (SDKModel sdkmodel in list2)
				{
					list.Add(new SPSDKModel
					{
						Name = sdkmodel.modelName,
						FilePath = sdkmodel.modelConf
					});
				}
				StreamProcessor._StreamProcessorCB = new DelegateNotify(StreamProcessor._instance.OnSPNotify);
				hresult = SPInterop.Initialize(list.ToArray(), list.Count, text4, StreamProcessor._StreamProcessorCB);
				hresult.LogOnHerror(this._logger, "StreamsProcessor.Initialize:");
				if (!hresult)
				{
					throw new Exception("On initialization of StreamsProecssor.");
				}
				try
				{
					bool flag = (bool)Settings.Default["EchoCancellationState"];
					this.SetFeatureState(EnStreamDirection.Microphone, SPFeature.Feature_Echo, flag);
				}
				catch
				{
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError(ex);
				throw;
			}
			return hresult;
		}

		private void OnSPNotify(uint sesId, SPNotificationType notifyCode, HRESULT hr, [MarshalAs(UnmanagedType.LPWStr)] string Message)
		{
			this._logger.LogDebug(string.Format("Got Notify from StreamsProcessor. sesId: {0}, notifyCode: {1}, HRESULT: 0x{2:X}, Message: {3}", new object[]
			{
				sesId,
				notifyCode,
				hr.GetHashCode(),
				Message
			}));
			try
			{
				if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_DUCKED)
				{
					if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_STARTED)
					{
						if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_START_FAILED)
						{
							switch (notifyCode)
							{
							case SPNotificationType.SP_NOTIFICATION_GENERAL_ERROR:
							{
								this._logger.LogError(string.Format("Got Notify from StreamsProcessor. sesId: {0}, notifyCode: {1}, HRESULT: 0x{2:X}, Message: {3}", new object[]
								{
									sesId,
									notifyCode,
									hr.GetHashCode(),
									Message
								}));
								EventHandler<SPMessage> sperrorNotification = this.SPErrorNotification;
								if (sperrorNotification == null)
								{
									goto IL_301;
								}
								sperrorNotification(this, new SPMessage(sesId, notifyCode, hr, Message));
								goto IL_301;
							}
							case SPNotificationType.SP_NOTIFICATION_SP_INITIALIZED:
							{
								StreamProcessor._spInitialized = true;
								EventHandler<bool> spinitializedNotification = this.SPInitializedNotification;
								if (spinitializedNotification == null)
								{
									goto IL_301;
								}
								spinitializedNotification(this, this.IsInitialized);
								goto IL_301;
							}
							case (SPNotificationType)3U:
								goto IL_2DE;
							case SPNotificationType.SP_NOTIFICATION_SP_INIT_ERROR:
							{
								StreamProcessor._spInitialized = false;
								StreamProcessor._spInitializationFailed = true;
								this._logger.LogError(string.Format("Got Notify from StreamsProcessor. sesId: {0}, notifyCode: {1}, HRESULT: 0x{2:X}, Message: {3}", new object[]
								{
									sesId,
									notifyCode,
									hr.GetHashCode(),
									Message
								}));
								EventHandler<SPMessage> sperrorNotification2 = this.SPErrorNotification;
								if (sperrorNotification2 == null)
								{
									goto IL_301;
								}
								sperrorNotification2(this, new SPMessage(sesId, notifyCode, hr, Message));
								goto IL_301;
							}
							default:
								if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_START_FAILED)
								{
									goto IL_2DE;
								}
								break;
							}
						}
						else if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_DISCONNECTED && notifyCode != SPNotificationType.SP_NOTIFICATION_IN_STARTED)
						{
							goto IL_2DE;
						}
					}
					else if (notifyCode <= SPNotificationType.SP_NOTIFICATION_IN_MISSING)
					{
						if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_STOPPED && notifyCode != SPNotificationType.SP_NOTIFICATION_IN_MISSING)
						{
							goto IL_2DE;
						}
					}
					else if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_STARTING && notifyCode != SPNotificationType.SP_NOTIFICATION_IN_RESETTING)
					{
						if (notifyCode != SPNotificationType.SP_NOTIFICATION_IN_DUCKED)
						{
							goto IL_2DE;
						}
						goto IL_2C9;
					}
				}
				else
				{
					if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_STOPPED)
					{
						if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_START_FAILED)
						{
							if (notifyCode == SPNotificationType.SP_NOTIFICATION_IN_STATS)
							{
								goto IL_28D;
							}
							if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_START_FAILED)
							{
								goto IL_2DE;
							}
						}
						else if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_DISCONNECTED && notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STARTED && notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STOPPED)
						{
							goto IL_2DE;
						}
					}
					else if (notifyCode <= SPNotificationType.SP_NOTIFICATION_OUT_STARTING)
					{
						if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_MISSING && notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STARTING)
						{
							goto IL_2DE;
						}
					}
					else if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_RESETTING)
					{
						if (notifyCode == SPNotificationType.SP_NOTIFICATION_OUT_DUCKED)
						{
							goto IL_2C9;
						}
						if (notifyCode != SPNotificationType.SP_NOTIFICATION_OUT_STATS)
						{
							goto IL_2DE;
						}
					}
					EventHandler<SPMessage> spoutboundNotification = this.SPOutboundNotification;
					if (spoutboundNotification == null)
					{
						goto IL_301;
					}
					spoutboundNotification(this, new SPMessage(sesId, notifyCode, hr, Message));
					goto IL_301;
				}
				IL_28D:
				EventHandler<SPMessage> spinboundNotification = this.SPInboundNotification;
				if (spinboundNotification == null)
				{
					goto IL_301;
				}
				spinboundNotification(this, new SPMessage(sesId, notifyCode, hr, Message));
				goto IL_301;
				IL_2C9:
				EventHandler<bool> spstreamDucked = this.SPStreamDucked;
				if (spstreamDucked == null)
				{
					goto IL_301;
				}
				spstreamDucked(this, true);
				goto IL_301;
				IL_2DE:
				this._logger.LogDebug(string.Format("OnNotify got Unknown Notification. sesId: {0}, notifyCode: {1},  Message: {2}", sesId, notifyCode, Message));
				IL_301:;
			}
			catch (Exception ex)
			{
				this._logger.LogError(ex);
			}
		}

		public HRESULT SessionSetDevice(EnStreamDirection kind, uint sessionId, string modelName, string devID)
		{
			if (StreamProcessor._initCalled > 0)
			{
				return SPInterop.SessionSetDevice(kind, sessionId, modelName, devID);
			}
			return 1;
		}

		public HRESULT SessionRelease(EnStreamDirection kind, uint sesID)
		{
			if (StreamProcessor._initCalled > 0)
			{
				return SPInterop.SessionRelease(kind, sesID);
			}
			return 1;
		}

		public HRESULT SetActivityState(EnStreamDirection kind, bool state)
		{
			if (StreamProcessor._initCalled > 0)
			{
				return SPInterop.SetActivityState(kind, state);
			}
			return 1;
		}

		public HRESULT SetFeatureState(EnStreamDirection kind, SPFeature feature, bool state)
		{
			if (StreamProcessor._initCalled > 0)
			{
				return SPInterop.SetFeatureState(kind, feature, state);
			}
			return 1;
		}

		public HRESULT RecordSession(EnStreamDirection dir, uint sID, SPFeature feature, uint nSamplesPerSec, string deviceID, string sourceFilePath, string beforeNCFilePath, string afterNCFilePath)
		{
			if (StreamProcessor._initCalled > 0)
			{
				SDKModel sdkmodel = this._modelManager.FindModel(dir, feature, nSamplesPerSec);
				string text = ((sdkmodel != null) ? sdkmodel.modelName : null);
				if (!string.IsNullOrEmpty(text))
				{
					return SPInterop.RecordSession(dir, sID, text, deviceID, sourceFilePath, beforeNCFilePath, afterNCFilePath);
				}
			}
			return 1;
		}

		public bool IsSPFeatureAvailable(EnStreamDirection kind, SPFeature feature, uint samplingrate)
		{
			bool flag = false;
			SDKModel sdkmodel = this._modelManager.FindModel(kind, feature, samplingrate);
			if (feature == SPFeature.Feature_Dereverb)
			{
				flag = sdkmodel.dereverb;
			}
			return flag;
		}

		public uint GenerateRecordingSessionId()
		{
			return StreamProcessor._nextRecordingSessionID++;
		}

		public int GetStreamActivityLevel(EnStreamDirection kind)
		{
			if (this.IsInitialized)
			{
				return SPInterop.GetStreamActivityLevel(kind);
			}
			return 0;
		}

		private static StreamProcessor _instance = null;

		private static DelegateNotify _StreamProcessorCB = null;

		private static int _initCalled = 0;

		private static bool _spInitialized = false;

		private static bool _spInitializationFailed = false;

		private Logger _logger;

		private SDKModelManager _modelManager;

		private static uint _nextRecordingSessionID = 300U;
	}
}
