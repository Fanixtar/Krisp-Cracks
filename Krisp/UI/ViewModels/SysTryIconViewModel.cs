using System;
using System.Collections.Generic;
using System.Drawing;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.Models;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	internal class SysTryIconViewModel : BindableBase
	{
		public Icon Icon
		{
			get
			{
				return this._icon;
			}
			private set
			{
				if (this._icon != value)
				{
					this._icon = value;
					base.RaisePropertyChanged("Icon");
				}
			}
		}

		public SysTryIconViewModel.IconState State
		{
			set
			{
				if (value != this._state)
				{
					this._state = value;
					this.Icon = this._state2IconDict[this._state];
				}
			}
		}

		public bool UpToDate
		{
			set
			{
				this.State = ((!value) ? (this._state | SysTryIconViewModel.IconState.Notified) : (this._state & ~SysTryIconViewModel.IconState.Notified));
			}
		}

		public bool HasActiveStream
		{
			set
			{
				this._hasActiveStream = value;
				this.State = (((this._micHasActiveStream && this._micNcOn) || (this._speakerHasActiveStream && this._speakerNcOn)) ? (this._state | SysTryIconViewModel.IconState.Colored) : (this._state & ~SysTryIconViewModel.IconState.Colored));
			}
		}

		public SysTryIconViewModel()
		{
			Mediator.Instance.Register(this);
			this.Icon = Resources.Krisp;
			this._state2IconDict = new Dictionary<SysTryIconViewModel.IconState, Icon>
			{
				{
					(SysTryIconViewModel.IconState)0,
					Resources.Krisp
				},
				{
					SysTryIconViewModel.IconState.Colored,
					Resources.KrispColored
				},
				{
					SysTryIconViewModel.IconState.Notified,
					Resources.KrispNotified
				},
				{
					SysTryIconViewModel.IconState.Colored | SysTryIconViewModel.IconState.Notified,
					Resources.KrispColoredNotified
				}
			};
		}

		[MediatorMessageSink("ActiveStreamChanged")]
		private void HandleActiveStreamChanged(DeviceKindValueArg arg)
		{
			if (arg.kind == AudioDeviceKind.Speaker)
			{
				this._speakerHasActiveStream = arg.Value;
			}
			else
			{
				this._micHasActiveStream = arg.Value;
			}
			this.State = (((this._micHasActiveStream && this._micNcOn) || (this._speakerHasActiveStream && this._speakerNcOn)) ? (this._state | SysTryIconViewModel.IconState.Colored) : (this._state & ~SysTryIconViewModel.IconState.Colored));
		}

		[MediatorMessageSink("NCSwitched")]
		private void HandleNCSwitched(DeviceKindValueArg arg)
		{
			if (arg.kind == AudioDeviceKind.Speaker)
			{
				this._speakerNcOn = arg.Value;
			}
			else
			{
				this._micNcOn = arg.Value;
			}
			this.State = (((this._micHasActiveStream && this._micNcOn) || (this._speakerHasActiveStream && this._speakerNcOn)) ? (this._state | SysTryIconViewModel.IconState.Colored) : (this._state & ~SysTryIconViewModel.IconState.Colored));
		}

		[MediatorMessageSink("UpdateInfoChanged")]
		private void HandleUpdateApeared(UpdateInfo updateInfo)
		{
			this.UpToDate = updateInfo == null || updateInfo.RetrivingResult <= 0;
		}

		private Icon _icon;

		private SysTryIconViewModel.IconState _state;

		private Dictionary<SysTryIconViewModel.IconState, Icon> _state2IconDict;

		private bool _micNcOn;

		private bool _speakerNcOn;

		private bool _hasActiveStream;

		private bool _micHasActiveStream;

		private bool _speakerHasActiveStream;

		[Flags]
		public enum IconState
		{
			Colored = 1,
			Notified = 2
		}
	}
}
