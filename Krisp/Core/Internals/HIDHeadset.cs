using System;
using HidLibrary;

namespace Krisp.Core.Internals
{
	internal class HIDHeadset : HeadsetStateMachine
	{
		public event EventHandler<bool> MuteTrigered;

		public event EventHandler<bool> HookSwitched;

		public int OffHookStatus
		{
			get
			{
				return this._offHookStatus;
			}
			set
			{
				EventHandler<bool> hookSwitched = this.HookSwitched;
				if (hookSwitched != null)
				{
					hookSwitched(this, value != 0);
				}
				this._offHookStatus = value;
			}
		}

		public int RingerStatus
		{
			get
			{
				return this._ringerStatus;
			}
			set
			{
				this._ringerStatus = value;
			}
		}

		public bool MuteStatus
		{
			get
			{
				return this._muteStatus;
			}
			set
			{
				this._muteStatus = value;
			}
		}

		public int OnHoldStatus
		{
			get
			{
				return this._onHoldStatus;
			}
			set
			{
				this._onHoldStatus = value;
			}
		}

		public int BusyLightStatus
		{
			get
			{
				return this._microphoneStatus;
			}
			set
			{
				this._microphoneStatus = value;
			}
		}

		public HIDHeadset()
		{
			this._device = null;
		}

		public void setOnHook()
		{
			this._offHookStatus = 0;
			this.writeReport();
		}

		public void setOffHook()
		{
			this._offHookStatus = 1;
			this.writeReport();
		}

		public void setMute(bool mute)
		{
			if (this.hookSwitch != 0)
			{
				this._muteStatus = mute;
				this.writeReport();
			}
		}

		public void CloseDevice()
		{
			if (this._device != null && this._device.IsOpen)
			{
				this._device.CloseDevice();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				this.MuteTrigered = null;
			}
			this._disposed = true;
		}

		protected virtual void readReports()
		{
			throw new NotImplementedException();
		}

		protected virtual void writeReport()
		{
			throw new NotImplementedException();
		}

		protected override void onUnMute()
		{
			throw new NotImplementedException();
		}

		protected override void onMute()
		{
			throw new NotImplementedException();
		}

		protected void TrigerMute()
		{
			EventHandler<bool> muteTrigered = this.MuteTrigered;
			if (muteTrigered == null)
			{
				return;
			}
			muteTrigered(this, this._muteStatus);
		}

		protected HidDevice _device;

		private bool _disposed;

		protected int _offHookStatus;

		protected bool _muteStatus;

		protected int _ringStatus;

		protected int _onHoldStatus;

		protected int _microphoneStatus;

		protected int _ringerStatus;

		protected int hookSwitch;

		protected int lineBusyTone;

		protected int phoneMute;

		protected int flash;

		protected int redial;

		protected int speedDial;

		protected int progrButton;

		protected int keypadValue;

		public delegate void ReadHandlerDelegate(HidReport report);
	}
}
