using System;
using Krisp.BackEnd;

namespace Krisp.Models
{
	public interface IAccountManager
	{
		bool IsLoggedIn { get; }

		bool HasAppToken { get; }

		bool HasSecret { get; }

		WorkingMode WorkingMode { get; }

		AccountManagerState State { get; }

		UserProfileInfo UserProfileInfo { get; }

		VersionInfo UpdateVersionInfo { get; }

		void Stop();

		void Resume();

		void Initialize();

		void Login(Action<string> URLOpener);

		string GetLoginURL();

		string GetProfileURL();

		string GetUnlockURL();

		void CreateInstallation();

		void DeviceLogin();

		void MinutesBalance(MinutesUsage minutesUsage);

		void NoInternetConnectionPolling();

		void OneTimeLogin();

		void LogoutAsync();

		void LoginWithPollingAsync();

		void FetchUserProfileInfoAsync();

		void MinutesBalanceAsync(MinutesUsage minutesUsage = null);

		void CheckForUpdateAsync(bool latest);

		void RecoverAfterSyncError();

		event EventHandler<UserProfileInfo> UserProfileInfoChanged;

		event EventHandler<MinutesBalance> MinutesBalanceChanged;

		event EventHandler<AccountManagerStateChangedEventArgs> StateChanged;

		event EventHandler<VersionInfo> UpdateVersionInfoChanged;

		event EventHandler<bool> InternetConnectionChangeDetected;
	}
}
