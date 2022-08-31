using System;
using Shared.Helpers;
using Stateless;

namespace Krisp.Core.Internals
{
	public abstract class AudioEngineStateMachine : DisposableBase
	{
		public event EventHandler<AudioEngineStateMachine.EngineState> StateChanged;

		internal StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger> Machine { get; }

		public AudioEngineStateMachine()
		{
			this.Machine = new StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>(() => this._state, delegate(AudioEngineStateMachine.EngineState s)
			{
				this._state = s;
			});
			this._engineEnabledTrigger = this.Machine.SetTriggerParameters<bool>(AudioEngineStateMachine.EngineTrigger.EnableEngine);
			this.Machine.Configure(AudioEngineStateMachine.EngineState.SPSessionStarting).OnEntry(delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition t)
			{
				this.enteredSPSessionStartingState();
			}, null).Ignore(AudioEngineStateMachine.EngineTrigger.SPNotificationStarting)
				.Ignore(AudioEngineStateMachine.EngineTrigger.SPRequestedStart)
				.InternalTransition(AudioEngineStateMachine.EngineTrigger.SPNotificationResetting, delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition started)
				{
					this.resetting(AudioEngineStateMachine.EngineState.SPSessionStarting);
				})
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationStarted, AudioEngineStateMachine.EngineState.SPSessionStarted)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationStartFailed, AudioEngineStateMachine.EngineState.SPSessionError)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationDisconnected, AudioEngineStateMachine.EngineState.SPSessionError)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationStoped, AudioEngineStateMachine.EngineState.SPSessionStoped);
			this.Machine.Configure(AudioEngineStateMachine.EngineState.SPSessionStarted).OnEntry(delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition t)
			{
				this.enteredSPSessionStartedState();
			}, null).Ignore(AudioEngineStateMachine.EngineTrigger.SPRequestedStart)
				.Ignore(AudioEngineStateMachine.EngineTrigger.SPNotificationStarting)
				.InternalTransition(AudioEngineStateMachine.EngineTrigger.SPNotificationStarted, delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition st)
				{
					this.started();
				})
				.InternalTransition(AudioEngineStateMachine.EngineTrigger.SPNotificationResetting, delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition started)
				{
					this.resetting(AudioEngineStateMachine.EngineState.SPSessionStarted);
				})
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationStartFailed, AudioEngineStateMachine.EngineState.SPSessionError)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationDisconnected, AudioEngineStateMachine.EngineState.SPSessionError)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationStoped, AudioEngineStateMachine.EngineState.SPSessionStoped);
			this.Machine.Configure(AudioEngineStateMachine.EngineState.SPSessionStoped).OnEntry(delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition t)
			{
				this.enteredSPSessionStopedState();
			}, null).Ignore(AudioEngineStateMachine.EngineTrigger.SPNotificationStoped)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPRequestedStart, AudioEngineStateMachine.EngineState.SPSessionStarting)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationStarted, AudioEngineStateMachine.EngineState.SPSessionStarted)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationGeneralError, AudioEngineStateMachine.EngineState.SPSessionError)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPNotificationDisconnected, AudioEngineStateMachine.EngineState.SPSessionError);
			this.Machine.Configure(AudioEngineStateMachine.EngineState.SPSessionError).OnEntry(delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition t)
			{
				this.enteredSPSessionErrorState();
			}, null).Ignore(AudioEngineStateMachine.EngineTrigger.SPNotificationStarting)
				.InternalTransition(AudioEngineStateMachine.EngineTrigger.SPNotificationStartFailed, delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition st)
				{
					this.errorStartFailed();
				})
				.InternalTransition(AudioEngineStateMachine.EngineTrigger.SPNotificationDisconnected, delegate(StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.Transition st)
				{
					this.errorStartFailed();
				})
				.Permit(AudioEngineStateMachine.EngineTrigger.SPRequestedStop, AudioEngineStateMachine.EngineState.SPSessionStoped)
				.Permit(AudioEngineStateMachine.EngineTrigger.SPRequestedStart, AudioEngineStateMachine.EngineState.SPSessionStarting);
			this.ToDotGraph();
		}

		private void enteredSPSessionStartingState()
		{
			EventHandler<AudioEngineStateMachine.EngineState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioEngineStateMachine.EngineState.SPSessionStarting);
		}

		private void enteredSPSessionStopedState()
		{
			this.stoped();
			EventHandler<AudioEngineStateMachine.EngineState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioEngineStateMachine.EngineState.SPSessionStoped);
		}

		private void enteredSPSessionErrorState()
		{
			EventHandler<AudioEngineStateMachine.EngineState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioEngineStateMachine.EngineState.SPSessionError);
		}

		private void enteredSPSessionStartedState()
		{
			this.started();
			EventHandler<AudioEngineStateMachine.EngineState> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this, AudioEngineStateMachine.EngineState.SPSessionStarted);
		}

		protected abstract void started();

		protected abstract void stoped();

		protected abstract void errorStartFailed();

		protected abstract void resetting(AudioEngineStateMachine.EngineState st);

		private void ToDotGraph()
		{
		}

		private AudioEngineStateMachine.EngineState _state = AudioEngineStateMachine.EngineState.SPSessionStoped;

		private StateMachine<AudioEngineStateMachine.EngineState, AudioEngineStateMachine.EngineTrigger>.TriggerWithParameters<bool> _engineEnabledTrigger;

		public enum EngineState
		{
			EngineDisabled,
			SPSessionStarted,
			SPSessionStarting,
			SPSessionStoped,
			SPSessionError
		}

		internal enum EngineTrigger
		{
			EnableEngine,
			SPNotificationGeneralError,
			SPNotificationStarted,
			SPNotificationStarting,
			SPNotificationResetting,
			SPNotificationStoped,
			SPNotificationStartFailed,
			SPNotificationDisconnected,
			SPRequestedStart,
			SPRequestedStop
		}
	}
}
