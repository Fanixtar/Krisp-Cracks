using System;
using Shared.Helpers;
using Stateless;
using Stateless.Graph;

namespace Krisp.Core.Internals
{
	public abstract class AudioDeviceLoader : DisposableBase
	{
		private StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger> Machine { get; }

		public AudioDeviceLoader(bool? forceToDef = null)
		{
			this.Machine = new StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>(() => this._state, delegate(AudioDeviceLoader.LoaderState s)
			{
				this._state = s;
			});
			this._ForceKrispAsSystemDefault = forceToDef;
			StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.StateConfiguration stateConfiguration = this.Machine.Configure(AudioDeviceLoader.LoaderState.Healty).OnEntry(delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.enteredHealtyState();
			}, "enteredHealtyState").Ignore(AudioDeviceLoader.LoaderTrigger.KrispEnabled)
				.Permit(AudioDeviceLoader.LoaderTrigger.KrispDisabled, AudioDeviceLoader.LoaderState.KrispDisabled)
				.Permit(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected, AudioDeviceLoader.LoaderState.NoDevice)
				.IgnoreIf(AudioDeviceLoader.LoaderTrigger.DeviceAdded, () => !this.isKrispDefault(), "!isKrispDefault()");
			bool? flag = this._ForceKrispAsSystemDefault;
			bool flag2 = false;
			if (!((flag.GetValueOrDefault() == flag2) & (flag != null)))
			{
				stateConfiguration.IgnoreIf(AudioDeviceLoader.LoaderTrigger.DeviceAdded, () => this.isKrispDefault(), "isKrispDefault()").Ignore(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged);
			}
			else
			{
				stateConfiguration.IgnoreIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, () => !this.isKrispDefault(), "!isKrispDefault()").PermitIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, AudioDeviceLoader.LoaderState.KrispDefault, () => this.isKrispDefault(), "isKrispDefault()");
			}
			stateConfiguration.Permit(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice, AudioDeviceLoader.LoaderState.MissingKrispDevice);
			this.Machine.Configure(AudioDeviceLoader.LoaderState.NoDevice).OnEntry(delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.enteredNoDeviceState();
			}, "enteredNoDeviceState").InternalTransition(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.errorNoDeviceDetected();
			})
				.Ignore(AudioDeviceLoader.LoaderTrigger.KrispEnabled)
				.Permit(AudioDeviceLoader.LoaderTrigger.KrispDisabled, AudioDeviceLoader.LoaderState.KrispDisabled)
				.Ignore(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged)
				.PermitIf(AudioDeviceLoader.LoaderTrigger.DeviceAdded, AudioDeviceLoader.LoaderState.KrispDefault, () => this.isKrispDefault(), "isKrispDefault()")
				.PermitIf(AudioDeviceLoader.LoaderTrigger.DeviceAdded, AudioDeviceLoader.LoaderState.Healty, () => !this.isKrispDefault(), "!isKrispDefault()")
				.Permit(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice, AudioDeviceLoader.LoaderState.MissingKrispDevice);
			this.Machine.Configure(AudioDeviceLoader.LoaderState.KrispDisabled).OnEntry(delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.enteredKrispDisabledState();
			}, "enteredKrispDisabledState").InternalTransition(AudioDeviceLoader.LoaderTrigger.KrispDisabled, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.errorKrispDevice_Disabled();
			})
				.InternalTransition(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
				{
					this.errorKrispDevice_Disabled();
				})
				.InternalTransition(AudioDeviceLoader.LoaderTrigger.DeviceAdded, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
				{
					this.errorKrispDevice_Disabled();
				})
				.Ignore(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected)
				.PermitIf(AudioDeviceLoader.LoaderTrigger.KrispEnabled, AudioDeviceLoader.LoaderState.KrispDefault, () => !this.devicesIsEmpty() && this.isKrispDefault(), "isKrispDefault()")
				.PermitIf(AudioDeviceLoader.LoaderTrigger.KrispEnabled, AudioDeviceLoader.LoaderState.Healty, () => !this.devicesIsEmpty() && !this.isKrispDefault(), "!devicesIsEmpty()")
				.PermitIf(AudioDeviceLoader.LoaderTrigger.KrispEnabled, AudioDeviceLoader.LoaderState.NoDevice, () => this.devicesIsEmpty(), "devicesIsEmpty()")
				.Permit(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice, AudioDeviceLoader.LoaderState.MissingKrispDevice);
			StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.StateConfiguration stateConfiguration2 = this.Machine.Configure(AudioDeviceLoader.LoaderState.KrispDefault);
			stateConfiguration2.OnEntry(delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.enteredKrispDefaultState();
			}, "enteredKrispDefaultState").Ignore(AudioDeviceLoader.LoaderTrigger.DeviceAdded).Ignore(AudioDeviceLoader.LoaderTrigger.KrispEnabled)
				.Permit(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected, AudioDeviceLoader.LoaderState.NoDevice)
				.Permit(AudioDeviceLoader.LoaderTrigger.KrispDisabled, AudioDeviceLoader.LoaderState.KrispDisabled)
				.Permit(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice, AudioDeviceLoader.LoaderState.MissingKrispDevice)
				.IgnoreIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, () => this.devicesIsEmpty() && this.isKrispDefault(), "devicesIsEmpty() && isKrispDefault()")
				.IgnoreIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, () => !this.devicesIsEmpty() && this.isKrispDefault(), "!devicesIsEmpty() && isKrispDefault()");
			flag = this._ForceKrispAsSystemDefault;
			flag2 = true;
			if ((flag.GetValueOrDefault() == flag2) & (flag != null))
			{
				stateConfiguration2.PermitIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, AudioDeviceLoader.LoaderState.Healty, () => !this.devicesIsEmpty(), "!devicesIsEmpty()").PermitIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, AudioDeviceLoader.LoaderState.NoDevice, () => this.devicesIsEmpty(), "devicesIsEmpty()");
			}
			else
			{
				stateConfiguration2.PermitIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, AudioDeviceLoader.LoaderState.Healty, () => !this.devicesIsEmpty() && !this.isKrispDefault(), "!devicesIsEmpty()  && !isKrispDefault()").PermitIf(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, AudioDeviceLoader.LoaderState.NoDevice, () => this.devicesIsEmpty() && !this.isKrispDefault(), "devicesIsEmpty()  && !isKrispDefault()");
			}
			this.Machine.Configure(AudioDeviceLoader.LoaderState.MissingKrispDevice).OnEntry(delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.enteredMissingKrispDevice();
			}, "enteredMissingKrispDevice").InternalTransition(AudioDeviceLoader.LoaderTrigger.MissingKrispDevice, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
			{
				this.errorKrispDevice_Missing();
			})
				.InternalTransition(AudioDeviceLoader.LoaderTrigger.DefaultDeviceChanged, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
				{
					this.errorKrispDevice_Missing();
				})
				.InternalTransition(AudioDeviceLoader.LoaderTrigger.DeviceAdded, delegate(StateMachine<AudioDeviceLoader.LoaderState, AudioDeviceLoader.LoaderTrigger>.Transition t)
				{
					this.errorKrispDevice_Missing();
				})
				.Ignore(AudioDeviceLoader.LoaderTrigger.NoDeviceDetected)
				.Permit(AudioDeviceLoader.LoaderTrigger.KrispDisabled, AudioDeviceLoader.LoaderState.KrispDisabled)
				.PermitIf(AudioDeviceLoader.LoaderTrigger.KrispEnabled, AudioDeviceLoader.LoaderState.KrispDefault, () => !this.devicesIsEmpty() && this.isKrispDefault(), "isKrispDefault()")
				.PermitIf(AudioDeviceLoader.LoaderTrigger.KrispEnabled, AudioDeviceLoader.LoaderState.Healty, () => !this.devicesIsEmpty() && !this.isKrispDefault(), "!devicesIsEmpty()")
				.PermitIf(AudioDeviceLoader.LoaderTrigger.KrispEnabled, AudioDeviceLoader.LoaderState.NoDevice, () => this.devicesIsEmpty(), "devicesIsEmpty()");
		}

		internal bool IsInState(AudioDeviceLoader.LoaderState loaderState)
		{
			return this.Machine.IsInState(loaderState);
		}

		internal void FireTrigger(AudioDeviceLoader.LoaderTrigger trigger)
		{
			this.Machine.Fire(trigger);
		}

		internal string StateTrace()
		{
			return this.Machine.ToString();
		}

		protected abstract void errorKrispDevice_Missing();

		protected abstract void errorKrispDevice_Disabled();

		protected abstract void errorKrispDefaultNotPermited();

		protected abstract void errorNoDeviceDetected();

		protected abstract bool devicesIsEmpty();

		protected abstract bool inHealty();

		private void enteredMissingKrispDevice()
		{
			this.errorKrispDevice_Missing();
			EventHandler<AudioDeviceLoader.LoaderState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioDeviceLoader.LoaderState.MissingKrispDevice);
		}

		private void enteredKrispDisabledState()
		{
			this.errorKrispDevice_Disabled();
			EventHandler<AudioDeviceLoader.LoaderState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioDeviceLoader.LoaderState.KrispDisabled);
		}

		private void enteredKrispDefaultState()
		{
			this.errorKrispDefaultNotPermited();
			EventHandler<AudioDeviceLoader.LoaderState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioDeviceLoader.LoaderState.KrispDefault);
		}

		private void enteredNoDeviceState()
		{
			this.errorNoDeviceDetected();
			EventHandler<AudioDeviceLoader.LoaderState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioDeviceLoader.LoaderState.NoDevice);
		}

		private void enteredHealtyState()
		{
			this.inHealty();
			EventHandler<AudioDeviceLoader.LoaderState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioDeviceLoader.LoaderState.Healty);
		}

		protected abstract bool isKrispDefault();

		public string ToDotGraph()
		{
			return UmlDotGraph.Format(this.Machine.GetInfo());
		}

		public EventHandler<AudioDeviceLoader.LoaderState> StateChanged;

		protected readonly bool? _ForceKrispAsSystemDefault;

		private AudioDeviceLoader.LoaderState _state = AudioDeviceLoader.LoaderState.NoDevice;

		public enum LoaderState
		{
			KrispDisabled,
			KrispDefault,
			NoDevice,
			Healty,
			MissingKrispDevice
		}

		public enum LoaderTrigger
		{
			DeviceAdded,
			DefaultDeviceChanged,
			NoDeviceDetected,
			KrispDisabled,
			KrispEnabled,
			MissingKrispDevice
		}
	}
}
