using System;
using Shared.Helpers;
using Stateless;

namespace Krisp.Core.Internals
{
	public abstract class ControllerStateMachine : DisposableBase
	{
		internal StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger> Machine { get; }

		public ControllerStateMachine(bool? forceToDef = null)
		{
			this._ForceKrispAsSystemDefault = forceToDef;
			this.Machine = new StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>(() => this._state, delegate(ControllerStateMachine.ControllerState s)
			{
				this._state = s;
			});
			this.Machine.Configure(ControllerStateMachine.ControllerState.ControllerHealtyState).OnEntry(delegate(StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>.Transition t)
			{
				this.enteredControllerHealtyState();
			}, null).IgnoreIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged, () => this.isDLStateHealty(), null)
				.IgnoreIf(ControllerStateMachine.ControllerTrigger.AudioEngineStateChanged, () => this.isAEStateHealty(), null)
				.IgnoreIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault, delegate()
				{
					bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
					bool flag = false;
					return !((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null));
				}, null)
				.Permit(ControllerStateMachine.ControllerTrigger.UnhealtyStateError, ControllerStateMachine.ControllerState.ControllerUnhealtyState)
				.PermitIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault, ControllerStateMachine.ControllerState.ControllerKrispDefault, delegate()
				{
					bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
					bool flag = false;
					return (forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null);
				}, null)
				.PermitIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged, ControllerStateMachine.ControllerState.ControllerErrorState, () => !this.isDLStateHealty(), null)
				.PermitIf(ControllerStateMachine.ControllerTrigger.AudioEngineStateChanged, ControllerStateMachine.ControllerState.ControllerErrorState, () => !this.isAEStateHealty(), null);
			this.Machine.Configure(ControllerStateMachine.ControllerState.ControllerKrispDefault).OnEntry(delegate(StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>.Transition t)
			{
				this.enteredControllerKrispDefault();
			}, null).InternalTransition(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault, delegate(StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>.Transition t)
			{
				this.onControllerInKrispDefault();
			})
				.Permit(ControllerStateMachine.ControllerTrigger.UnhealtyStateError, ControllerStateMachine.ControllerState.ControllerUnhealtyState)
				.PermitIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged, ControllerStateMachine.ControllerState.ControllerHealtyState, () => this.isDLStateHealty(), null)
				.PermitIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged, ControllerStateMachine.ControllerState.ControllerErrorState, () => !this.isDLStateHealty(), null);
			this.Machine.Configure(ControllerStateMachine.ControllerState.ControllerErrorState).OnEntry(delegate(StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>.Transition t)
			{
				this.enteredControllerErrorState();
			}, null).IgnoreIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged, () => !this.isDLStateHealty(), null)
				.IgnoreIf(ControllerStateMachine.ControllerTrigger.AudioEngineStateChanged, () => !this.isAEStateHealty(), null)
				.IgnoreIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault, delegate()
				{
					bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
					bool flag = false;
					return !((forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null));
				}, null)
				.Permit(ControllerStateMachine.ControllerTrigger.UnhealtyStateError, ControllerStateMachine.ControllerState.ControllerUnhealtyState)
				.PermitIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault, ControllerStateMachine.ControllerState.ControllerKrispDefault, delegate()
				{
					bool? forceKrispAsSystemDefault = this._ForceKrispAsSystemDefault;
					bool flag = false;
					return (forceKrispAsSystemDefault.GetValueOrDefault() == flag) & (forceKrispAsSystemDefault != null);
				}, null)
				.PermitIf(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged, ControllerStateMachine.ControllerState.ControllerHealtyState, () => this.isDLStateHealty(), null)
				.PermitIf(ControllerStateMachine.ControllerTrigger.AudioEngineStateChanged, ControllerStateMachine.ControllerState.ControllerHealtyState, () => this.isAEStateHealty(), null);
			this.Machine.Configure(ControllerStateMachine.ControllerState.ControllerUnhealtyState).OnEntry(delegate(StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>.Transition t)
			{
				this.enteredControllerUnhealtyState();
			}, null).InternalTransition(ControllerStateMachine.ControllerTrigger.UnhealtyStateError, delegate(StateMachine<ControllerStateMachine.ControllerState, ControllerStateMachine.ControllerTrigger>.Transition t)
			{
				this.onControllerInUnhealtyState();
			})
				.Ignore(ControllerStateMachine.ControllerTrigger.DeviceLoadderStateChanged)
				.Ignore(ControllerStateMachine.ControllerTrigger.AudioEngineStateChanged)
				.Ignore(ControllerStateMachine.ControllerTrigger.DeviceLoadderKrispSetsDefault);
		}

		private void enteredControllerKrispDefault()
		{
			this.onControllerInKrispDefault();
		}

		private void enteredControllerErrorState()
		{
			this.onControllerInErrorState();
		}

		private void enteredControllerUnhealtyState()
		{
			this.onControllerInUnhealtyState();
		}

		private void enteredControllerHealtyState()
		{
			this.onControllerInHealtyState();
		}

		protected abstract void onControllerInKrispDefault();

		protected abstract void onControllerInHealtyState();

		protected abstract void onControllerInErrorState();

		protected abstract void onControllerInUnhealtyState();

		protected abstract bool isAEStateHealty();

		protected abstract bool isDLStateHealty();

		private ControllerStateMachine.ControllerState _state;

		protected readonly bool? _ForceKrispAsSystemDefault;

		internal enum ControllerState
		{
			ControllerHealtyState,
			ControllerKrispDefault,
			ControllerErrorState,
			ControllerUnhealtyState
		}

		public enum ControllerTrigger
		{
			DeviceLoadderStateChanged,
			DeviceLoadderKrispSetsDefault,
			AudioEngineStateChanged,
			UnhealtyStateError
		}
	}
}
