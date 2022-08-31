using System;
using Stateless;

namespace Krisp.Core
{
	public abstract class KAppStateMachine
	{
		internal StateMachine<KAppState, KAppTriger> Machine { get; }

		internal KAppStateMachine()
		{
			this.Machine = new StateMachine<KAppState, KAppTriger>(() => this._state, delegate(KAppState s)
			{
				this._state = s;
			});
			this.Machine.Configure(KAppState.Initial).OnEntry(delegate(StateMachine<KAppState, KAppTriger>.Transition t)
			{
				this.enteredInitialState();
			}, null).Ignore(KAppTriger.NotLoggedIn)
				.Permit(KAppTriger.ConnectionError, KAppState.ErrorConnection)
				.Permit(KAppTriger.UserLoggedIn, KAppState.LoggedInReady);
			this.Machine.Configure(KAppState.NotLoggedIn).OnEntry(delegate(StateMachine<KAppState, KAppTriger>.Transition t)
			{
				this.enteredNotLoggedInState();
			}, null).Permit(KAppTriger.NotLoggedIn, KAppState.Initial)
				.Permit(KAppTriger.ConnectionError, KAppState.ErrorConnection)
				.Permit(KAppTriger.UserLoggedIn, KAppState.LoggedInReady);
			this.Machine.Configure(KAppState.LoggedInReady).OnEntry(delegate(StateMachine<KAppState, KAppTriger>.Transition t)
			{
				this.enteredLogedInReadyState();
			}, null).Permit(KAppTriger.NotLoggedIn, KAppState.Initial)
				.Permit(KAppTriger.ConnectionError, KAppState.ErrorConnection);
			this.Machine.Configure(KAppState.ErrorConnection).OnEntry(delegate(StateMachine<KAppState, KAppTriger>.Transition t)
			{
				this.enteredErrorConnectionState();
			}, null).Ignore(KAppTriger.ConnectionError)
				.Permit(KAppTriger.NotLoggedIn, KAppState.Initial);
		}

		protected abstract void enteredErrorConnectionState();

		protected abstract void enteredLogedInReadyState();

		protected abstract void enteredNotLoggedInState();

		protected abstract void enteredInitialState();

		private KAppState _state;
	}
}
