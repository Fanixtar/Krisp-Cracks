using System;
using Krisp.Models;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	public class KrispAudioDevice : AudioDevice, IStreamActivityHolder
	{
		public event EventHandler<StreamActivityState> StreamActivityChanged;

		public KrispAudioDevice(AudioDeviceKind kind, IMMDevice device)
			: base(kind, device)
		{
			this.RegisterActivityNotifications();
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			this.UnRegisterActivityNotifications();
			this._disposed = true;
		}

		public void RegisterActivityNotifications()
		{
			try
			{
				if (this._notificationClient == null)
				{
					this._notificationClient = DataModelFactory.CreateKrispActivityNotificationClient();
				}
				if (!this._notificationClientRegistered || !this._notificationClient.IsClientValidated())
				{
					KrispActivityNotificationClient notificationClient = this._notificationClient;
					lock (notificationClient)
					{
						if (base.Kind == AudioDeviceKind.Speaker)
						{
							this._notificationClient.RendererStreamActivityChanged += this.OnStreamActivityChanged;
						}
						else
						{
							this._notificationClient.CapturerStreamActivityChanged += this.OnStreamActivityChanged;
						}
						this._notificationClient.StartActivityNotificationClient();
						this._notificationClientRegistered = true;
					}
				}
				if (!this._notificationClient.IsClientValidated())
				{
					this.UnRegisterActivityNotifications();
					this._logger.LogError("Error on RegisterActivityNotifications.");
				}
			}
			catch (Exception ex)
			{
				this.UnRegisterActivityNotifications();
				this._logger.LogError("Error on RegisterActivityNotifications. {0}", new object[] { ex });
			}
		}

		private void OnStreamActivityChanged(object sender, StreamActivityState state)
		{
			StreamActivityState streamActivityState = state;
			if (StreamActivityState.StreamOpened == state)
			{
				if (this._streamActivityState == state)
				{
					return;
				}
				this._streamActivityState = state;
			}
			if (state.HasFlag(StreamActivityState.StreamClosed))
			{
				streamActivityState = (this._streamActivityState = StreamActivityState.StreamClosed);
			}
			else if (state.HasFlag(StreamActivityState.StreamStarted))
			{
				if (this._streamActivityState == StreamActivityState.StreamStarted || this._streamActivityState != StreamActivityState.StreamOpened)
				{
					this._logger.LogWarning("OnCapturerStreamActivityChanged:  _streamActivityState: {0}, newState: {1}.", new object[] { this._streamActivityState, state });
				}
				this._streamActivityState = state;
			}
			else if (state.HasFlag(StreamActivityState.StreamOpened))
			{
				this._streamActivityState = state;
			}
			EventHandler<StreamActivityState> streamActivityChanged = this.StreamActivityChanged;
			if (streamActivityChanged == null)
			{
				return;
			}
			streamActivityChanged(this, streamActivityState);
		}

		public void UnRegisterActivityNotifications()
		{
			if (this._notificationClient != null)
			{
				KrispActivityNotificationClient notificationClient = this._notificationClient;
				lock (notificationClient)
				{
					if (base.Kind == AudioDeviceKind.Speaker)
					{
						this._notificationClient.RendererStreamActivityChanged -= this.OnStreamActivityChanged;
					}
					else
					{
						this._notificationClient.CapturerStreamActivityChanged -= this.OnStreamActivityChanged;
					}
					this._notificationClient = null;
					this._notificationClientRegistered = false;
				}
			}
		}

		private KrispActivityNotificationClient _notificationClient;

		private bool _notificationClientRegistered;

		private bool _disposed;

		private StreamActivityState _streamActivityState = StreamActivityState.StreamClosed;
	}
}
