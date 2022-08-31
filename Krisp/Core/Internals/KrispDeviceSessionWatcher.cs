using System;
using System.Collections.Generic;
using Krisp.AppHelper;
using Krisp.Models;
using Shared.Helpers;
using Shared.Interops.IMMDeviceAPI;

namespace Krisp.Core.Internals
{
	public class KrispDeviceSessionWatcher : DisposableBase, IAudioSessionNotification
	{
		public KrispDeviceSessionWatcher(AudioDeviceKind kind, IMMDevice device)
		{
			try
			{
				this._kind = kind;
				this._logger = LogWrapper.GetLogger(string.Format("KrispDeviceWatcher ({0})", this._kind));
				device.Activate(out this._sessionManager);
				this._sessionManager.RegisterSessionNotification(this);
				IAudioSessionEnumerator sessionEnumerator = this._sessionManager.GetSessionEnumerator();
				int count = sessionEnumerator.GetCount();
				for (int i = 0; i < count; i++)
				{
					this.AddNewSession(sessionEnumerator.GetSession(i));
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("KrispDeviceWatcher failed: {0}", new object[] { ex.Message });
			}
		}

		~KrispDeviceSessionWatcher()
		{
			this._logger.LogDebug("~KrispDeviceWatcher");
			this.Dispose(false);
		}

		public void SetWatcherHolder(IWatcherHandlerHolder wh)
		{
			this._watcherHolder = wh;
		}

		protected override void Dispose(bool disposing)
		{
			this._logger.LogDebug("Dispose # {0}", new object[] { disposing });
			if (this._disposed)
			{
				return;
			}
			if (this._sessionManager != null)
			{
				this._sessionManager.UnregisterSessionNotification(this);
				this._sessionManager = null;
			}
			if (disposing)
			{
				try
				{
					IAudioDeviceSession[] array = this._sessions.ToArray();
					this._sessions.Clear();
					IAudioDeviceSession[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						((AudioDeviceSession)array2[i]).Dispose();
					}
				}
				catch (Exception ex)
				{
					this._logger.LogError("AudioDeviceSession.Dispose failed: {0}", new object[] { ex.Message });
				}
			}
			this._disposed = true;
			base.Dispose(disposing);
		}

		private bool IsWatchableApp(AppInfo appInfo)
		{
			return appInfo != null && appInfo.Type != ProcessType.System && !string.IsNullOrWhiteSpace(appInfo.ExeName) && !this._ignoreAppList.Exists((string item) => appInfo.ExeName.IndexOf(item, StringComparison.CurrentCultureIgnoreCase) != -1);
		}

		private void AddNewSession(IAudioSessionControl session)
		{
			try
			{
				if (session != null)
				{
					AppInfo appInfo = AppInfo.CreateAppInfo(session);
					if (this.IsWatchableApp(appInfo))
					{
						IAudioDeviceSession audioDeviceSession = new AudioDeviceSession(this._kind, session, appInfo);
						this._sessions.Add(audioDeviceSession);
						audioDeviceSession.SessionDisconnected += this.OnSessionDisconnected;
						audioDeviceSession.StateChanged += this.AudioDeviceSessionStateChanged;
						this._logger.LogDebug("Added new session # {0} # App: {1} ({2} - {3}) # {4}", new object[]
						{
							this._sessions.Count,
							appInfo.ExeName,
							appInfo.PID,
							appInfo.Type.ToString(),
							audioDeviceSession.SessionID
						});
					}
					else
					{
						this._logger.LogDebug("Session ignored # App: {0} ({1} - {2})  {3}", new object[]
						{
							appInfo.ExeName,
							appInfo.PID,
							appInfo.Type.ToString(),
							appInfo.ExePath
						});
					}
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("AddNewSession failed: {0}", new object[] { ex.Message });
			}
		}

		private void AudioDeviceSessionStateChanged(object sender, AudioSessionState e)
		{
			IAudioDeviceSession audioDeviceSession = (IAudioDeviceSession)sender;
			if (audioDeviceSession != null)
			{
				switch (e)
				{
				case AudioSessionState.Inactive:
				case AudioSessionState.Expired:
				{
					IWatcherHandlerHolder watcherHolder = this._watcherHolder;
					if (watcherHolder == null)
					{
						return;
					}
					watcherHolder.DisconnectSession(audioDeviceSession.AppInfo);
					return;
				}
				case AudioSessionState.Active:
				{
					IWatcherHandlerHolder watcherHolder2 = this._watcherHolder;
					if (watcherHolder2 == null)
					{
						return;
					}
					watcherHolder2.ConnectSession(audioDeviceSession.AppInfo);
					break;
				}
				default:
					return;
				}
			}
		}

		void IAudioSessionNotification.OnSessionCreated(IAudioSessionControl NewSession)
		{
			try
			{
				this._logger.LogDebug("OnSessionCreated");
				this.AddNewSession(NewSession);
			}
			catch (Exception ex)
			{
				this._logger.LogError("OnSessionCreated failed: {0}", new object[] { ex.Message });
			}
		}

		private void OnSessionDisconnected(object sender, AudioSessionDisconnectReason disconnectReason)
		{
			try
			{
				IAudioDeviceSession audioDeviceSession = (IAudioDeviceSession)sender;
				if (audioDeviceSession != null)
				{
					this._sessions.Remove(audioDeviceSession);
					audioDeviceSession.SessionDisconnected -= this.OnSessionDisconnected;
					audioDeviceSession.StateChanged -= this.AudioDeviceSessionStateChanged;
					((AudioDeviceSession)audioDeviceSession).UnregisterAudioSessionNotification();
					IWatcherHandlerHolder watcherHolder = this._watcherHolder;
					if (watcherHolder != null)
					{
						watcherHolder.DisconnectSession(audioDeviceSession.AppInfo);
					}
					this._logger.LogDebug("Removed session # {0} # App: {1} ({2} - {3}) # {4}", new object[]
					{
						this._sessions.Count,
						audioDeviceSession.AppInfo.ExeName,
						audioDeviceSession.AppInfo.PID,
						audioDeviceSession.AppInfo.Type.ToString(),
						audioDeviceSession.SessionID
					});
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("OnSessionDisconnected failed: {0}", new object[] { ex.Message });
			}
		}

		private Logger _logger;

		private AudioDeviceKind _kind;

		private IAudioSessionManager2 _sessionManager;

		private List<IAudioDeviceSession> _sessions = new List<IAudioDeviceSession>();

		private List<string> _ignoreAppList = new List<string>();

		private bool _disposed;

		private IWatcherHandlerHolder _watcherHolder;
	}
}
