using System;
using Shared.Helpers;
using Stateless;

namespace Krisp.Core.Internals
{
	internal abstract class HeadsetStateMachine : DisposableBase
	{
		public HeadsetStateMachine()
		{
			this._machine = new StateMachine<TelephonyState, TelephonyTrigger>(() => this._state, delegate(TelephonyState s)
			{
				this._state = s;
			});
			this.initMachine();
		}

		private void initMachine()
		{
			this._machine.Configure(TelephonyState.NotConnected).Ignore(TelephonyTrigger.Mute).Ignore(TelephonyTrigger.UnMute)
				.Permit(TelephonyTrigger.OnHook, TelephonyState.OnHook)
				.Permit(TelephonyTrigger.OffHook, TelephonyState.OffHook);
			this._machine.Configure(TelephonyState.OffHook).Ignore(TelephonyTrigger.OffHook).InternalTransition(TelephonyTrigger.Mute, delegate(StateMachine<TelephonyState, TelephonyTrigger>.Transition t)
			{
				this.onMute();
			})
				.InternalTransition(TelephonyTrigger.UnMute, delegate(StateMachine<TelephonyState, TelephonyTrigger>.Transition t)
				{
					this.onUnMute();
				})
				.Permit(TelephonyTrigger.OnHook, TelephonyState.OnHook);
			this._machine.Configure(TelephonyState.OnHook).Ignore(TelephonyTrigger.OnHook).Ignore(TelephonyTrigger.Mute)
				.Ignore(TelephonyTrigger.UnMute)
				.Permit(TelephonyTrigger.OffHook, TelephonyState.OffHook);
		}

		protected abstract void onUnMute();

		protected abstract void onMute();

		public void OnHook()
		{
			this._machine.Fire(TelephonyTrigger.OnHook);
		}

		public void OffHook()
		{
			this._machine.Fire(TelephonyTrigger.OffHook);
		}

		public void Mute(bool mute)
		{
			if (mute)
			{
				this._machine.Fire(TelephonyTrigger.Mute);
				return;
			}
			this._machine.Fire(TelephonyTrigger.UnMute);
		}

		public void Print()
		{
			Console.WriteLine(this._machine);
		}

		protected StateMachine<TelephonyState, TelephonyTrigger> _machine;

		private TelephonyState _state;
	}
}
