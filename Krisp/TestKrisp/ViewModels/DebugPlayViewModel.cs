using System;
using System.Windows.Input;
using System.Windows.Threading;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.UI.ViewModels;
using MVVMFoundation;
using Shared.Interops;

namespace Krisp.TestKrisp.ViewModels
{
	public class DebugPlayViewModel : BindableBase, IDisposable
	{
		public DebugPlayViewModel()
		{
			this._Logger = LogWrapper.GetLogger("DebugPlay");
			this._spInstance = DataModelFactory.SPInstance;
			this._speakerCtrl = DataModelFactory.KrispController(AudioDeviceKind.Speaker);
			this._micCtrl = DataModelFactory.KrispController(AudioDeviceKind.Microphone);
			this._events = "Begin list";
			this._dispatcher = Dispatcher.CurrentDispatcher;
			this._spInstance.SPInitializedNotification += this.OnInit;
			this._spInstance.SPErrorNotification += this.OnError;
			this._spInstance.SPInboundNotification += this.OnInbound;
			this._spInstance.SPOutboundNotification += this.OnOutbound;
			this._spInstance.SPStreamDucked += this.OnDucked;
			this.StreamActivityLevelOut = this._spInstance.GetStreamActivityLevel(EnStreamDirection.Microphone);
			this.StreamActivityLevelIn = this._spInstance.GetStreamActivityLevel(EnStreamDirection.Speaker);
		}

		public void Dispose()
		{
			this._spInstance.SPInitializedNotification -= this.OnInit;
			this._spInstance.SPErrorNotification -= this.OnError;
			this._spInstance.SPInboundNotification -= this.OnInbound;
			this._spInstance.SPOutboundNotification -= this.OnOutbound;
			this._spInstance.SPStreamDucked -= this.OnDucked;
		}

		public bool SPIsInitialse
		{
			get
			{
				return this._spInstance.IsInitialized;
			}
		}

		public int N
		{
			get
			{
				return this._n;
			}
		}

		public int StreamActivityLevelOut
		{
			get
			{
				return this._streamActivityLevelOut;
			}
			private set
			{
				if (this._streamActivityLevelOut != value)
				{
					this._streamActivityLevelOut = value;
					base.RaisePropertyChanged("StreamActivityLevelOut");
				}
			}
		}

		public int StreamActivityLevelIn
		{
			get
			{
				return this._streamActivityLevelIn;
			}
			private set
			{
				if (this._streamActivityLevelIn != value)
				{
					this._streamActivityLevelIn = value;
					base.RaisePropertyChanged("StreamActivityLevelIn");
				}
			}
		}

		public string EventsList
		{
			get
			{
				return this._events;
			}
			private set
			{
				if (this._events != value)
				{
					this._events = value;
					base.RaisePropertyChanged("EventsList");
				}
			}
		}

		public string LastMessage
		{
			get
			{
				return this._lastMsg;
			}
			private set
			{
				this._lastMsg = value;
				base.RaisePropertyChanged("LastMessage");
			}
		}

		public string OutbountDeviceName
		{
			get
			{
				IKrispController speakerCtrl = this._speakerCtrl;
				if (speakerCtrl == null)
				{
					return null;
				}
				return speakerCtrl.WorkingDevice.DisplayName;
			}
		}

		public string OutbountDeviceBits
		{
			get
			{
				IKrispController speakerCtrl = this._speakerCtrl;
				if (speakerCtrl == null)
				{
					return null;
				}
				return speakerCtrl.WorkingDevice.DefaultWaveFormat.wBitsPerSample.ToString();
			}
		}

		public string OutbountDeviceSampleRate
		{
			get
			{
				IKrispController speakerCtrl = this._speakerCtrl;
				if (speakerCtrl == null)
				{
					return null;
				}
				return speakerCtrl.WorkingDevice.DefaultWaveFormat.nSamplesPerSec.ToString();
			}
		}

		public string InBoundDeviceName
		{
			get
			{
				IKrispController micCtrl = this._micCtrl;
				if (micCtrl == null)
				{
					return null;
				}
				return micCtrl.WorkingDevice.DisplayName;
			}
		}

		public string InbountDeviceBits
		{
			get
			{
				IKrispController micCtrl = this._micCtrl;
				if (micCtrl == null)
				{
					return null;
				}
				return micCtrl.WorkingDevice.DefaultWaveFormat.wBitsPerSample.ToString();
			}
		}

		public string InbountDeviceSampleRate
		{
			get
			{
				IKrispController micCtrl = this._micCtrl;
				if (micCtrl == null)
				{
					return null;
				}
				return micCtrl.WorkingDevice.DefaultWaveFormat.nSamplesPerSec.ToString();
			}
		}

		public ICommand SP_Init
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._SP_Init) == null)
				{
					relayCommand = (this._SP_Init = new RelayCommand(delegate(object p)
					{
						this._Logger.LogDebug("Init command start");
						this._spInstance.SPInitialize();
						this._Logger.LogDebug("Init command end");
					}));
				}
				return relayCommand;
			}
		}

		public ICommand SP_Release
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._SP_Release) == null)
				{
					relayCommand = (this._SP_Release = new RelayCommand(delegate(object p)
					{
						int num = this._spInstance.SPRelease();
						base.RaisePropertyChanged("SPIsInitialse");
						this._n = num;
						base.RaisePropertyChanged("N");
					}));
				}
				return relayCommand;
			}
		}

		public ICommand SP_EchoOn
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._SP_EchoOn) == null)
				{
					relayCommand = (this._SP_EchoOn = new RelayCommand(delegate(object p)
					{
						HRESULT hresult = this._spInstance.SetFeatureState(EnStreamDirection.Speaker, SPFeature.Feature_Echo, true);
						this.LastMessage = string.Format("EchoOn:{0}", hresult);
					}));
				}
				return relayCommand;
			}
		}

		public ICommand SP_EchoOff
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._SP_EchoOff) == null)
				{
					relayCommand = (this._SP_EchoOff = new RelayCommand(delegate(object p)
					{
						HRESULT hresult = this._spInstance.SetFeatureState(EnStreamDirection.Speaker, SPFeature.Feature_Echo, false);
						this.LastMessage = string.Format("EchoOff:{0}", hresult);
					}));
				}
				return relayCommand;
			}
		}

		public ICommand SP_CleanOn
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._SP_CleanOn) == null)
				{
					relayCommand = (this._SP_CleanOn = new RelayCommand(delegate(object p)
					{
						HRESULT hresult = this._spInstance.SetFeatureState(EnStreamDirection.Speaker, SPFeature.Feature_NoiseClean, true);
						this.LastMessage = string.Format("_SP_CleanOn:{0}", hresult);
					}));
				}
				return relayCommand;
			}
		}

		public ICommand SP_CleanOff
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._SP_CleanOff) == null)
				{
					relayCommand = (this._SP_CleanOff = new RelayCommand(delegate(object p)
					{
						HRESULT hresult = this._spInstance.SetFeatureState(EnStreamDirection.Speaker, SPFeature.Feature_NoiseClean, false);
						this.LastMessage = string.Format("_SP_CleanOff:{0}", hresult);
					}));
				}
				return relayCommand;
			}
		}

		private void OnInit(object sender, bool initState)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				this.RaisePropertyChanged("SPIsInitialse");
			});
			this._dispatcher.InvokeAsync(delegate()
			{
				this.EventHappened(string.Format("Init:{0}", initState));
			});
			this._Logger.LogDebug(string.Format("SPInitializedNotification:{0}", initState));
		}

		private void OnDucked(object sender, bool e)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				this.EventHappened(string.Format("Duck:{0}", e));
			});
		}

		private void OnOutbound(object sender, SPMessage e)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				this.EventHappened("Out", e);
			});
		}

		private void OnInbound(object sender, SPMessage e)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				this.EventHappened("In", e);
			});
		}

		private void OnError(object sender, SPMessage e)
		{
			this._dispatcher.InvokeAsync(delegate()
			{
				this.EventHappened("Err", e);
			});
		}

		private void EventHappened(string s)
		{
			this.EventsList = this.EventsList + Environment.NewLine + s;
		}

		private void EventHappened(string s, SPMessage m)
		{
			if (m.hr == 0)
			{
				this.EventsList = this.EventsList + Environment.NewLine + string.Format("{0}=>sess:{1}, {2}, {3}", new object[] { s, m.sesId, m.notifyCode, m.Message });
				return;
			}
			this.EventsList = this.EventsList + Environment.NewLine + string.Format("{0}=>sess:{1}, h:{2}, code:{3}, {4}", new object[] { s, m.sesId, m.hr, m.notifyCode, m.Message });
		}

		private IStreamProcessor _spInstance;

		private int _n = -1;

		private Logger _Logger;

		private int _streamActivityLevelOut = -1;

		private int _streamActivityLevelIn = -1;

		private string _events;

		private string _lastMsg;

		private Dispatcher _dispatcher;

		private IKrispController _speakerCtrl;

		private IKrispController _micCtrl;

		private RelayCommand _SP_Init;

		private RelayCommand _SP_Release;

		private RelayCommand _SP_EchoOn;

		private RelayCommand _SP_EchoOff;

		private RelayCommand _SP_CleanOn;

		private RelayCommand _SP_CleanOff;
	}
}
