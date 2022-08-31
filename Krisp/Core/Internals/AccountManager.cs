using System;
using System.Collections.Generic;
using System.Net;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.ViewModels;
using RestSharp;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.Core.Internals
{
	public class AccountManager : IAccountManager
	{
		public string Secret { get; private set; }

		public WorkingMode WorkingMode { get; private set; } = WorkingMode.Running;

		public AccountManagerState State { get; set; } = AccountManagerState.Uninitialized;

		public UserProfileInfo UserProfileInfo { get; private set; }

		private bool FetchIngInProgress { get; set; }

		private bool NoInternetPollingInProgress { get; set; }

		private bool LoginPollingInProgress { get; set; }

		public bool HasAppToken
		{
			get
			{
				return !string.IsNullOrWhiteSpace(this._data.AppToken);
			}
		}

		public bool HasSecret
		{
			get
			{
				return !string.IsNullOrWhiteSpace(this.Secret);
			}
		}

		public bool IsLoggedIn
		{
			get
			{
				return this.HasAppToken && this.UserProfileInfo != null;
			}
		}

		private Logger Logger
		{
			get
			{
				return LogWrapper.GetLogger("AccountManager");
			}
		}

		public VersionInfo UpdateVersionInfo
		{
			get
			{
				return this._updateVersionInfo;
			}
			private set
			{
				this._updateVersionInfo = value;
				EventHandler<VersionInfo> updateVersionInfoChanged = this.UpdateVersionInfoChanged;
				if (updateVersionInfoChanged == null)
				{
					return;
				}
				updateVersionInfoChanged(this, this._updateVersionInfo);
			}
		}

		private AccountManager()
		{
			this.Logger.LogDebug("Constructing AccounManager.");
			this._data = new AccountManager.CachingData();
			this.ObtainDataFromCache();
		}

		public void Stop()
		{
			this.Logger.LogInfo("Stopping AccountManager");
			this.WorkingMode = WorkingMode.Stopped;
			TimerHelper userProfileUpdateTimer = this._userProfileUpdateTimer;
			if (userProfileUpdateTimer != null)
			{
				userProfileUpdateTimer.Stop();
			}
			TimerHelper noInternetConnectionTimer = this._noInternetConnectionTimer;
			if (noInternetConnectionTimer != null)
			{
				noInternetConnectionTimer.Stop();
			}
			TimerHelper loginPollingTimer = this._loginPollingTimer;
			if (loginPollingTimer == null)
			{
				return;
			}
			loginPollingTimer.Stop();
		}

		public void Resume()
		{
			if (this.WorkingMode != WorkingMode.Running)
			{
				this.Logger.LogInfo("Resuming AccountManager");
				this.WorkingMode = WorkingMode.Running;
				this._userProfileUpdateTimer.Start();
				if (this.IsLoggedIn)
				{
					this.FetchUserProfileInfoAsync();
					return;
				}
			}
			else
			{
				this.Logger.LogInfo("Skipping AccountManager resuming, as it is already running");
			}
		}

		public void Initialize()
		{
			this.Logger.LogInfo("Initializing AccountManager");
			if (this.State != AccountManagerState.Uninitialized)
			{
				return;
			}
			this.Logger.LogInfo("InstallationInfo async request.");
			KrispSDKFactory.Instance.DoAsyncRequest<InstallationInfo>(new InstallationRequestInfo(), delegate(KrispSDKResponse<InstallationInfo> krispResp, object err)
			{
				this.CreateInstallationResponseHandler(krispResp, err);
				this.StartFetchUserProfileTimer();
				if (!this.HasSecret)
				{
					this.Logger.LogError("CreateInstallation failed");
					return;
				}
				if (DeviceLoginHelper.DeviceMode)
				{
					this._data.AppToken = null;
					this.Logger.LogInfo("Device login mode.");
					this.DeviceLogin();
					if (!this.HasAppToken)
					{
						return;
					}
				}
				if (!this.HasAppToken)
				{
					this.Logger.LogDebug("There is no cached AppToken.");
					this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, null);
					return;
				}
				this.FetchUserProfileInfoAsync();
			});
		}

		public event EventHandler<AccountManagerStateChangedEventArgs> StateChanged;

		public event EventHandler<UserProfileInfo> UserProfileInfoChanged;

		public event EventHandler<MinutesBalance> MinutesBalanceChanged;

		public event EventHandler<VersionInfo> UpdateVersionInfoChanged;

		public event EventHandler<bool> InternetConnectionChangeDetected;

		public string ReportProblem(string description, bool hasSystemInfo, bool hasAudio)
		{
			this.Logger.LogDebug("Invoking ReportProblem function.");
			this.Logger.LogInfo("ReportProblem sync request.");
			IRestResponse<KrispSDKResponse<string>> restResponse = KrispSDKFactory.Instance.DoRequest<string>(new ReportProblemRequestInfo(this._data.AppToken, description, hasSystemInfo, hasAudio));
			KrispSDKResponse<string> data = restResponse.Data;
			if (restResponse.StatusCode.IsSuccess())
			{
				this.Logger.LogInfo("ReportProblem succeeded.");
				return data.data;
			}
			return null;
		}

		public void CreateInstallation()
		{
			this.Logger.LogInfo("InstallationInfo sync request.");
			IRestResponse<KrispSDKResponse<InstallationInfo>> restResponse = KrispSDKFactory.Instance.DoRequest<InstallationInfo>(new InstallationRequestInfo());
			this.CreateInstallationResponseHandler(restResponse.Data, restResponse.StatusCode.IsSuccess() ? null : restResponse);
		}

		public void DeviceLogin()
		{
			this.Logger.LogDebug("Invoking CreateAppToken function.");
			this.SetState(AccountManagerState.LoggingIn, AccountManagerErrorCode.UNINITIALIZED, null);
			this.Logger.LogDebug("createAppToken sync request.");
			IRestResponse<KrispSDKResponse<AppTokenInfo>> restResponse = KrispSDKFactory.Instance.DoRequest<AppTokenInfo>(new CreateAppTokenRequestInfo(this.GenerateJwt()));
			KrispSDKResponse<AppTokenInfo> data = restResponse.Data;
			if (restResponse.StatusCode.IsSuccess())
			{
				this.Logger.LogInfo("CreateAppToken succeeded.");
				this._data.AppToken = data.data.token;
				this.UpdateCache();
				this.FetchUserProfileInfoAsync();
				return;
			}
			this.Logger.LogError("DeviceLogin response: {0} ({1}), Code: {2}, ErrorCode: {3}, krispErr: {4}, req_id = {5}", new object[]
			{
				restResponse.StatusCode,
				restResponse.ErrorMessage,
				(data != null) ? new ResponseCode?(data.code) : null,
				(data != null) ? data.error_code : null,
				(data != null) ? data.message : null,
				(data != null) ? data.req_id : null
			});
			if (data == null)
			{
				this.SetState(AccountManagerState.NoInternetConnection, AccountManagerErrorCode.UNINITIALIZED, null);
				return;
			}
			if (!string.IsNullOrWhiteSpace(data.error_code))
			{
				this.SetState(AccountManagerState.GeneralError, this.ErrorCode(data.error_code), null);
				return;
			}
			this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
		}

		public void NoInternetConnectionPolling()
		{
			this.Logger.LogInfo("Invoking NoInternetConnectionPolling function.");
			try
			{
				if (!this.HasAppToken || this.NoInternetPollingInProgress)
				{
					this.Logger.LogDebug("There is no point to Fetch UserProfile wiithout AppToken. Terminating", new object[] { this._data.AppToken });
				}
				else
				{
					this.Logger.LogDebug("Starting InternetConnection polling timer with 10 seconds interval.");
					uint totalAttemptCount = 0U;
					this._noInternetConnectionTimer = new TimerHelper();
					this._noInternetConnectionTimer.Interval = TimeSpan.FromSeconds(10.0).TotalMilliseconds;
					this._noInternetConnectionTimer.Elapsed += delegate(object sender, TimerHelperElapsedEventArgs e)
					{
						if (this.State != AccountManagerState.NoInternetConnection)
						{
							this.Logger.LogInfo("Stopping InternetConnection polling timer.");
							this.NoInternetPollingInProgress = false;
							this._noInternetConnectionTimer.Stop();
						}
						else
						{
							if (totalAttemptCount == 12U)
							{
								this._noInternetConnectionTimer.Interval = TimeSpan.FromMinutes(1.0).TotalMilliseconds;
								this.Logger.LogDebug("InternetConnection polling timer interval changed to 1 minute");
							}
							else if (totalAttemptCount == 20U)
							{
								this._noInternetConnectionTimer.Interval = TimeSpan.FromHours(1.0).TotalMilliseconds;
								this.Logger.LogDebug("InternetConnection polling timer interval changed to 1 hour");
							}
							else if (totalAttemptCount == 25U)
							{
								EventHandler<bool> internetConnectionChangeDetected = this.InternetConnectionChangeDetected;
								if (internetConnectionChangeDetected != null)
								{
									internetConnectionChangeDetected(this, false);
								}
								this._noInternetConnectionTimer.Interval = TimeSpan.FromHours(6.0).TotalMilliseconds;
								this.Logger.LogDebug("InternetConnection polling timer interval changed to 6 hours");
							}
							else if (totalAttemptCount >= 50U)
							{
								this.Logger.LogDebug("InternetConnection polling timer total attempt count reached {0}, stopping the timer.", new object[] { totalAttemptCount });
								this.NoInternetPollingInProgress = false;
								this._noInternetConnectionTimer.Stop();
							}
							this.Logger.LogDebug("InternetConnection polling iteration #{0}", new object[] { totalAttemptCount });
							this.Logger.LogInfo("Polling UserProfile. SessionID = {0}", new object[] { this._data.SessionID });
							IRestResponse<KrispSDKResponse<UserProfileInfo>> restResponse = KrispSDKFactory.Instance.DoRequest<UserProfileInfo>(new UserProfileRequestInfo(this._data.AppToken));
							KrispSDKResponse<UserProfileInfo> data = restResponse.Data;
							if (restResponse.StatusCode.IsSuccess())
							{
								this.Logger.LogInfo("GetUserProfileSync response: Success.");
								this.UserProfileInfo = data.data;
								this.SetState(AccountManagerState.LoggedIn, AccountManagerErrorCode.UNINITIALIZED, null);
								EventHandler<UserProfileInfo> userProfileInfoChanged = this.UserProfileInfoChanged;
								if (userProfileInfoChanged != null)
								{
									userProfileInfoChanged(this, data.data);
								}
								EventHandler<bool> internetConnectionChangeDetected2 = this.InternetConnectionChangeDetected;
								if (internetConnectionChangeDetected2 != null)
								{
									internetConnectionChangeDetected2(this, true);
								}
								if (!AccountManager.ShowAvailabilityOfUpdate())
								{
									this.UpdateVersionInfo = null;
								}
							}
							else
							{
								this.Logger.LogInfo("InternetConnection polling error: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
								{
									restResponse.StatusCode,
									restResponse.ErrorMessage,
									(data != null) ? new ResponseCode?(data.code) : null,
									(data != null) ? data.error_code : null,
									(data != null) ? data.req_id : null,
									(data != null) ? data.message : null,
									this._data.SessionID
								});
								if (data != null)
								{
									if (data.code == ResponseCode.NotFound || data.code == ResponseCode.AuthenticationError)
									{
										this.ClearData();
										this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, false);
									}
									else if (restResponse.StatusCode != HttpStatusCode.BadGateway)
									{
										this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
									}
								}
							}
						}
						uint num = totalAttemptCount + 1U;
						totalAttemptCount = num;
					};
					this.NoInternetPollingInProgress = true;
					this._noInternetConnectionTimer.Enabled = true;
				}
			}
			catch (Exception ex)
			{
				this.Logger.LogError("Error on NoInternetConnectionP: {0}", new object[] { ex.Message });
			}
		}

		public void FetchUserProfileInfoAsync()
		{
			this.Logger.LogDebug("Invoking FetchUserProfileInfo function.");
			if (this.FetchIngInProgress)
			{
				this.Logger.LogDebug("FetchUserProfileInfo already in progress, terminating this one...");
				return;
			}
			this.FetchIngInProgress = true;
			this.Logger.LogInfo("GetUserProfile async request. SessionID = {0}", new object[] { this._data.SessionID });
			KrispSDKFactory.Instance.DoAsyncRequest<UserProfileInfo>(new UserProfileRequestInfo(this._data.AppToken), delegate(KrispSDKResponse<UserProfileInfo> krispResp, object err)
			{
				if (err == null)
				{
					this.Logger.LogInfo("UserProfile Successfully updated.");
					this.UserProfileInfo = krispResp.data;
					if (this.State == AccountManagerState.NoInternetConnection)
					{
						EventHandler<bool> internetConnectionChangeDetected = this.InternetConnectionChangeDetected;
						if (internetConnectionChangeDetected != null)
						{
							internetConnectionChangeDetected(this, true);
						}
					}
					this.SetState(AccountManagerState.LoggedIn, AccountManagerErrorCode.UNINITIALIZED, null);
					EventHandler<UserProfileInfo> userProfileInfoChanged = this.UserProfileInfoChanged;
					if (userProfileInfoChanged != null)
					{
						userProfileInfoChanged(this, krispResp.data);
					}
					if (!AccountManager.ShowAvailabilityOfUpdate())
					{
						this.UpdateVersionInfo = null;
					}
				}
				else
				{
					IRestResponse restResponse = err as IRestResponse;
					Logger logger = this.Logger;
					string text = "GetUserProfile error: {0} ({1} rHost: {2}), Code: {3}, ErrorCode: {4}, req_id: {5}, krispErr: {6}, SessionID: {7}.";
					object[] array = new object[8];
					array[0] = ((restResponse != null) ? new HttpStatusCode?(restResponse.StatusCode) : null);
					array[1] = ((restResponse != null) ? restResponse.ErrorMessage : null);
					int num = 2;
					object obj;
					if (restResponse == null)
					{
						obj = null;
					}
					else
					{
						Uri responseUri = restResponse.ResponseUri;
						obj = ((responseUri != null) ? responseUri.Host : null);
					}
					array[num] = obj;
					array[3] = ((krispResp != null) ? new ResponseCode?(krispResp.code) : null);
					array[4] = ((krispResp != null) ? krispResp.error_code : null);
					array[5] = ((krispResp != null) ? krispResp.req_id : null);
					array[6] = ((krispResp != null) ? krispResp.message : null);
					array[7] = this._data.SessionID;
					logger.LogInfo(text, array);
					if (krispResp == null)
					{
						this.SetState(AccountManagerState.NoInternetConnection, AccountManagerErrorCode.UNINITIALIZED, null);
					}
					else if (krispResp.code == ResponseCode.NotFound || krispResp.code == ResponseCode.AuthenticationError)
					{
						this.ClearData();
						this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, false);
					}
					else if (restResponse == null || restResponse.StatusCode != HttpStatusCode.BadGateway)
					{
						this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
					}
				}
				this.FetchIngInProgress = false;
			});
		}

		public void Login(Action<string> URLOpener)
		{
			if (DeviceLoginHelper.DeviceMode)
			{
				this.DeviceLogin();
				return;
			}
			if (URLOpener != null)
			{
				URLOpener(this.GetLoginURL());
			}
			this.LoginWithPollingAsync();
		}

		public void LoginWithPollingAsync()
		{
			this.Logger.LogDebug("Invoking LoginWithPolling function.");
			if (this.State != AccountManagerState.LoggedIn)
			{
				this.SetState(AccountManagerState.LoggingIn, AccountManagerErrorCode.UNINITIALIZED, null);
				if (this.LoginPollingInProgress)
				{
					this.Logger.LogDebug("LoginWithPolling already in progress, terminating this one...");
					return;
				}
				this.LoginPollingInProgress = true;
				double pollingInterval = TimeSpan.FromMinutes(3.0).TotalMilliseconds;
				this.Logger.LogInfo("Starting LoginWithPolling timer for {0} minutes.", new object[] { pollingInterval / 60000.0 });
				bool requestIsInProcess = false;
				int attemptsRemaining = Convert.ToInt32(Math.Floor(pollingInterval / 3000.0));
				this._loginPollingTimer = new TimerHelper();
				this._loginPollingTimer.Interval = TimeSpan.FromSeconds(3.0).TotalMilliseconds;
				Action<KrispSDKResponse<AppTokenInfo>, object> <>9__1;
				this._loginPollingTimer.Elapsed += delegate(object sender, TimerHelperElapsedEventArgs e)
				{
					int num = attemptsRemaining - 1;
					attemptsRemaining = num;
					if (num <= 0 || (this.State != AccountManagerState.LoggingIn && this.State != AccountManagerState.NoInternetConnection))
					{
						this.Logger.LogInfo("Stopping LoginWithPolling timer.");
						this.LoginPollingInProgress = false;
						this._loginPollingTimer.Stop();
						return;
					}
					if (!requestIsInProcess)
					{
						string text = this.GenerateJwt();
						if (string.IsNullOrWhiteSpace(text))
						{
							return;
						}
						this.Logger.LogDebug("Polling iteration #{0}: GetAppToken async request. with JWT = {1}", new object[]
						{
							pollingInterval / 3000.0 - (double)attemptsRemaining,
							text
						});
						requestIsInProcess = true;
						IKrispSDK instance = KrispSDKFactory.Instance;
						RequestInfo requestInfo = new AppTokenRequestInfo(text);
						Action<KrispSDKResponse<AppTokenInfo>, object> action;
						if ((action = <>9__1) == null)
						{
							action = (<>9__1 = delegate(KrispSDKResponse<AppTokenInfo> krispResp, object err)
							{
								if (err == null)
								{
									this.Logger.LogInfo("LoginWithPolling: AppToken received successfully. Stopping LoginWithPolling timer.");
									this.LoginPollingInProgress = false;
									this._loginPollingTimer.Stop();
									this._data.AppToken = krispResp.data.token;
									this.UpdateCache();
									this.FetchUserProfileInfoAsync();
								}
								else
								{
									IRestResponse restResponse = err as IRestResponse;
									if (krispResp != null && krispResp.code == ResponseCode.NotFound)
									{
										this.Logger.LogInfo("AppToken Polling response.: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
										{
											restResponse.StatusCode,
											restResponse.ErrorMessage,
											(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
											(krispResp != null) ? krispResp.error_code : null,
											(krispResp != null) ? krispResp.req_id : null,
											(krispResp != null) ? krispResp.message : null,
											this._data.SessionID
										});
									}
									else
									{
										this.Logger.LogWarning("AppToken Polling response.: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
										{
											restResponse.StatusCode,
											restResponse.ErrorMessage,
											(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
											(krispResp != null) ? krispResp.error_code : null,
											(krispResp != null) ? krispResp.req_id : null,
											(krispResp != null) ? krispResp.message : null,
											this._data.SessionID
										});
									}
									if (krispResp != null && krispResp.code != ResponseCode.NotFound && restResponse.StatusCode != HttpStatusCode.BadGateway && krispResp.code != ResponseCode.AuthenticationError)
									{
										this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
									}
								}
								requestIsInProcess = false;
							});
						}
						instance.DoAsyncRequest<AppTokenInfo>(requestInfo, action);
					}
				};
				this._loginPollingTimer.Enabled = true;
			}
		}

		public void OneTimeLogin()
		{
			this.Logger.LogDebug("Invoking OneTimeLogin function.");
			if (this.State != AccountManagerState.LoggedIn)
			{
				string text = this.GenerateJwt();
				if (string.IsNullOrWhiteSpace(text))
				{
					return;
				}
				this.Logger.LogInfo("GetAppToken async request. with SessionID = {0}", new object[] { this._data.SessionID });
				KrispSDKFactory.Instance.DoAsyncRequest<AppTokenInfo>(new AppTokenRequestInfo(text), delegate(KrispSDKResponse<AppTokenInfo> krispResp, object err)
				{
					if (err == null)
					{
						this.Logger.LogInfo("AppToken received successfully");
						this._data.AppToken = krispResp.data.token;
						this.UpdateCache();
						this.FetchUserProfileInfoAsync();
						return;
					}
					IRestResponse restResponse = err as IRestResponse;
					if (krispResp != null && krispResp.code == ResponseCode.NotFound)
					{
						this.Logger.LogInfo("OneTime login response: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
						{
							restResponse.StatusCode,
							restResponse.ErrorMessage,
							(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
							(krispResp != null) ? krispResp.error_code : null,
							(krispResp != null) ? krispResp.req_id : null,
							(krispResp != null) ? krispResp.message : null,
							this._data.SessionID
						});
					}
					else
					{
						this.Logger.LogWarning("OneTime login response: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
						{
							restResponse.StatusCode,
							restResponse.ErrorMessage,
							(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
							(krispResp != null) ? krispResp.error_code : null,
							(krispResp != null) ? krispResp.req_id : null,
							(krispResp != null) ? krispResp.message : null,
							this._data.SessionID
						});
					}
					if (krispResp == null)
					{
						this.SetState(AccountManagerState.NoInternetConnection, AccountManagerErrorCode.UNINITIALIZED, null);
						return;
					}
					if (krispResp.code == ResponseCode.NotFound)
					{
						this.ClearData();
						this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, false);
						return;
					}
					if (restResponse.StatusCode != HttpStatusCode.BadGateway)
					{
						if (krispResp.code == ResponseCode.AuthenticationError)
						{
							this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, "jwt_error");
							return;
						}
						this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
					}
				});
			}
		}

		public void LogoutAsync()
		{
			this.Logger.LogDebug("Invoking Logout function.");
			if (this.IsLoggedIn)
			{
				this.Logger.LogInfo("Logout async request");
				KrispSDKFactory.Instance.DoAsyncRequest<NoDataType>(new LogoutRequestInfo(this._data.AppToken), delegate(KrispSDKResponse<NoDataType> krispResp, object err)
				{
					if (err == null)
					{
						this.Logger.LogInfo("Logged out successfully.");
						return;
					}
					IRestResponse restResponse = err as IRestResponse;
					this.Logger.LogWarning("Logout failed: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
					{
						restResponse.StatusCode,
						restResponse.ErrorMessage,
						(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
						(krispResp != null) ? krispResp.error_code : null,
						(krispResp != null) ? krispResp.req_id : null,
						(krispResp != null) ? krispResp.message : null,
						this._data.SessionID
					});
				});
				this.ClearData();
				this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, true);
			}
		}

		public void CheckForUpdateAsync(bool latest)
		{
			this.Logger.LogDebug("Checking for updates.");
			VersionRequestInfo versionRequestInfo = new VersionRequestInfo(latest);
			KrispSDKFactory.Instance.DoAsyncRequest<VersionInfo>(versionRequestInfo, delegate(KrispSDKResponse<VersionInfo> krispResp, object err)
			{
				try
				{
					if (err == null && ((krispResp != null) ? krispResp.data : null) != null)
					{
						Version krispVersion = EnvHelper.KrispVersion;
						Version version = new Version(krispResp.data.version);
						if (krispVersion < version)
						{
							this.Logger.LogInfo("Update available. Current version: \"{0}\", New version: \"{1}\".", new object[] { krispVersion, version });
							VersionInfo data = krispResp.data;
							data.resultCode = 1;
							this.UpdateVersionInfo = data;
						}
						else
						{
							this.Logger.LogDebug("Krisp is up to date.");
							this.UpdateVersionInfo = new VersionInfo();
						}
					}
					else
					{
						IRestResponse restResponse = err as IRestResponse;
						Logger logger = this.Logger;
						string text = "Fail to get version info rHost: {0}, resp: {1}, rStat: {2}";
						object[] array = new object[3];
						int num = 0;
						object obj;
						if (restResponse == null)
						{
							obj = null;
						}
						else
						{
							Uri responseUri = restResponse.ResponseUri;
							obj = ((responseUri != null) ? responseUri.Host : null);
						}
						array[num] = obj;
						array[1] = ((restResponse != null) ? restResponse.ErrorMessage : null) ?? ((restResponse != null) ? restResponse.StatusCode.ToString() : null);
						array[2] = ((restResponse != null) ? new ResponseStatus?(restResponse.ResponseStatus) : null);
						logger.LogWarning(text, array);
						this.UpdateVersionInfo = new VersionInfo
						{
							resultCode = -1
						};
					}
				}
				catch (Exception ex)
				{
					this.Logger.LogWarning("Unable to get version info: {0}", new object[] { ex.Message });
					this.UpdateVersionInfo = new VersionInfo
					{
						resultCode = -1
					};
				}
			});
		}

		public void MinutesBalanceAsync(MinutesUsage minutesUsage = null)
		{
			this.Logger.LogDebug("Invoking MinutesBalanceAsync function.");
			MinutesBalanceRequestInfo minutesBalanceRequestInfo = new MinutesBalanceRequestInfo(this._data.AppToken, minutesUsage);
			KrispSDKFactory.Instance.DoAsyncRequest<MinutesBalance>(minutesBalanceRequestInfo, delegate(KrispSDKResponse<MinutesBalance> krispResp, object err)
			{
				if (err == null)
				{
					this.Logger.LogInfo("MinutesBalance async response success");
					EventHandler<MinutesBalance> minutesBalanceChanged = this.MinutesBalanceChanged;
					if (minutesBalanceChanged == null)
					{
						return;
					}
					minutesBalanceChanged(this, krispResp.data);
					return;
				}
				else
				{
					IRestResponse restResponse = err as IRestResponse;
					this.Logger.LogWarning("MinutesBalance response: {0} ({1}), Code: {2}, ErrorCode: {3}, req_id: {4}, krispErr: {5}, SessionID: {6}.", new object[]
					{
						restResponse.StatusCode,
						restResponse.ErrorMessage,
						(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
						(krispResp != null) ? krispResp.error_code : null,
						(krispResp != null) ? krispResp.req_id : null,
						(krispResp != null) ? krispResp.message : null,
						this._data.SessionID
					});
					if (krispResp == null)
					{
						this.SetState(AccountManagerState.NoInternetConnection, AccountManagerErrorCode.UNINITIALIZED, null);
						return;
					}
					if (krispResp.code == ResponseCode.AuthenticationError)
					{
						this.ClearData();
						this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, false);
						return;
					}
					if (restResponse.StatusCode != HttpStatusCode.BadGateway)
					{
						this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
					}
					return;
				}
			});
		}

		public void MinutesBalance(MinutesUsage minutesUsage)
		{
			this.Logger.LogDebug("Invoking MinutesBalance function.");
			MinutesBalanceRequestInfo minutesBalanceRequestInfo = new MinutesBalanceRequestInfo(this._data.AppToken, minutesUsage);
			IRestResponse<KrispSDKResponse<MinutesBalance>> restResponse = KrispSDKFactory.Instance.DoRequest<MinutesBalance>(minutesBalanceRequestInfo);
			KrispSDKResponse<MinutesBalance> data = restResponse.Data;
			if (restResponse.StatusCode.IsSuccess())
			{
				this.Logger.LogInfo("MinutesBalance sync response success. req_id: {0}", new object[] { data.req_id });
				EventHandler<MinutesBalance> minutesBalanceChanged = this.MinutesBalanceChanged;
				if (minutesBalanceChanged == null)
				{
					return;
				}
				minutesBalanceChanged(this, data.data);
				return;
			}
			else
			{
				this.Logger.LogError("MinutesBalance response: {0} ({1}), Code: {2}, ErrorCode: {3}, krispErr: {4}, req_id = {5}", new object[]
				{
					restResponse.StatusCode,
					restResponse.ErrorMessage,
					(data != null) ? new ResponseCode?(data.code) : null,
					(data != null) ? data.error_code : null,
					(data != null) ? data.message : null,
					(data != null) ? data.req_id : null
				});
				if (data == null)
				{
					this.SetState(AccountManagerState.NoInternetConnection, AccountManagerErrorCode.UNINITIALIZED, null);
					return;
				}
				if (data.code == ResponseCode.AuthenticationError)
				{
					this.ClearData();
					this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, false);
					return;
				}
				if (restResponse.StatusCode != HttpStatusCode.BadGateway)
				{
					this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
				}
				return;
			}
		}

		public string GetLoginURL()
		{
			this.Logger.LogDebug("Invoking GetLoginURL function.");
			if (string.IsNullOrWhiteSpace(this.Secret))
			{
				this.RecoverData();
			}
			return string.Concat(new string[]
			{
				ServerInfoLoader.Instance.FrontendInfo.url,
				"login?locale=",
				TranslationSourceViewModel.Instance.SelectedCulture.Name,
				"&app=",
				this.GenerateJwt()
			});
		}

		public string GetProfileURL()
		{
			this.Logger.LogDebug("Invoking GetProfileURL function.");
			if (string.IsNullOrWhiteSpace(this.Secret))
			{
				this.RecoverData();
			}
			return string.Concat(new string[]
			{
				ServerInfoLoader.Instance.FrontendInfo.url,
				"login?locale=",
				TranslationSourceViewModel.Instance.SelectedCulture.Name,
				"&jwt=",
				this.GenerateJwt()
			});
		}

		public string GetUnlockURL()
		{
			this.Logger.LogDebug("Invoking GetUnlockURL function.");
			if (string.IsNullOrWhiteSpace(this.Secret))
			{
				this.RecoverData();
			}
			return string.Concat(new string[]
			{
				ServerInfoLoader.Instance.FrontendInfo.url,
				"app/unlock?locale=",
				TranslationSourceViewModel.Instance.SelectedCulture.Name,
				"&jwt=",
				this.GenerateJwt()
			});
		}

		public string GetCreateTeamURL()
		{
			this.Logger.LogDebug("Invoking GetCreateTeamURL function.");
			if (string.IsNullOrWhiteSpace(this.Secret))
			{
				this.RecoverData();
			}
			return string.Concat(new string[]
			{
				ServerInfoLoader.Instance.FrontendInfo.url,
				"app/teams/create?locale=",
				TranslationSourceViewModel.Instance.SelectedCulture.Name,
				"&jwt=",
				this.GenerateJwt()
			});
		}

		private void CreateInstallationResponseHandler(KrispSDKResponse<InstallationInfo> krispResp, object err)
		{
			this.Logger.LogDebug("Invoking CreateInstallationResponseHandler function.");
			if (err == null)
			{
				this.Logger.LogInfo("CreateInstallation succeeded.");
				this.Secret = krispResp.data.secret;
				AppSettingsHelper.Instance = new AppSettingsHelper(krispResp.data.settings);
				this.UpdateCache();
				if (!this.HasSecret)
				{
					this.Logger.LogError("Secret was null.");
					this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
					return;
				}
				if (!DeviceLoginHelper.DeviceMode && !this.HasAppToken)
				{
					this.SetState(AccountManagerState.LoggedOut, AccountManagerErrorCode.UNINITIALIZED, null);
					return;
				}
			}
			else
			{
				IRestResponse restResponse = err as IRestResponse;
				this.Logger.LogError("InstallationInfo response: {0} ({1}), Code: {2}, ErrorCode: {3}, krispErr: {4}, req_id = {5}", new object[]
				{
					restResponse.StatusCode,
					restResponse.ErrorMessage,
					(krispResp != null) ? new ResponseCode?(krispResp.code) : null,
					(krispResp != null) ? krispResp.error_code : null,
					(krispResp != null) ? krispResp.message : null,
					(krispResp != null) ? krispResp.req_id : null
				});
				if (krispResp == null)
				{
					this.SetState(AccountManagerState.NoInternetConnection, AccountManagerErrorCode.UNINITIALIZED, null);
					return;
				}
				this.SetState(AccountManagerState.GeneralError, AccountManagerErrorCode.UNINITIALIZED, null);
			}
		}

		private string GenerateJwt()
		{
			if (string.IsNullOrWhiteSpace(this._data.SessionID))
			{
				this._data.SessionID = Guid.NewGuid().ToString();
				this.UpdateCache();
			}
			return JWTHelper.GenerateToken(InstallationID.ID, this._data.SessionID, this.HasAppToken ? this._data.AppToken : this.Secret, this.HasAppToken);
		}

		private void StartFetchUserProfileTimer()
		{
			double fetchUesrProfileIntervalMilisec = Settings.Default.FetchUesrProfileIntervalMilisec;
			double num = fetchUesrProfileIntervalMilisec + (double)new Random().Next(Convert.ToInt32(Math.Floor(fetchUesrProfileIntervalMilisec / 12.0)));
			this._userProfileUpdateTimer = new TimerHelper();
			this._userProfileUpdateTimer.Interval = num;
			this._userProfileUpdateTimer.Elapsed += delegate(object sender, TimerHelperElapsedEventArgs eventArgs)
			{
				if (this.IsLoggedIn)
				{
					this.Logger.LogInfo("Updating UserProfileInfo via timer.");
					this.FetchUserProfileInfoAsync();
				}
			};
			this.Logger.LogInfo("Starting FetchUserProfileInfo Timer");
			this._userProfileUpdateTimer.Start();
		}

		private void RecoverData()
		{
			this.Logger.LogWarning("Data is incomplete. Trying to recover...");
			if (!this.HasSecret)
			{
				this.CreateInstallation();
			}
		}

		private void UpdateCache()
		{
			this.Logger.LogDebug("Updating Cache.");
			AppLocalCache instance = AppLocalCache.Instance;
			instance.Set("AppToken", this._data.AppToken);
			instance.Set("SessionID", this._data.SessionID);
			string lastError = AppLocalCache.LastError;
			if (!string.IsNullOrWhiteSpace(lastError))
			{
				this.Logger.LogWarning("Updating Cache ->: {0}", new object[] { lastError });
			}
		}

		private void ObtainDataFromCache()
		{
			this.Logger.LogDebug("Obtaining data from cache");
			AppLocalCache instance = AppLocalCache.Instance;
			this._data.AppToken = instance.Get("AppToken");
			this._data.SessionID = instance.Get("SessionID");
		}

		private void ClearData()
		{
			this.Logger.LogDebug("Clearing data.");
			this.UserProfileInfo = null;
			this._data.AppToken = null;
			this._data.SessionID = null;
			this.UpdateCache();
		}

		private void SetState(AccountManagerState state, AccountManagerErrorCode errorCode = AccountManagerErrorCode.UNINITIALIZED, object analyticsData = null)
		{
			if (this.State != state)
			{
				this.Logger.LogInfo("AccounManager state changed from {0} to {1}.", new object[]
				{
					this.State.ToString(),
					state.ToString()
				});
				this.State = state;
				EventHandler<AccountManagerStateChangedEventArgs> stateChanged = this.StateChanged;
				if (stateChanged != null)
				{
					stateChanged(this, new AccountManagerStateChangedEventArgs(state, errorCode));
				}
				switch (state)
				{
				case AccountManagerState.LoggedIn:
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.SigninSuccessEvent());
					return;
				case AccountManagerState.LoggingIn:
					break;
				case AccountManagerState.LoggedOut:
					if (analyticsData != null)
					{
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.SignoutEvent((analyticsData as bool?) ?? false));
						return;
					}
					break;
				case AccountManagerState.NoInternetConnection:
				{
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.SigningProblemEvent(((analyticsData != null) ? analyticsData.ToString() : null) ?? "sync_problem"));
					EventHandler<bool> internetConnectionChangeDetected = this.InternetConnectionChangeDetected;
					if (internetConnectionChangeDetected == null)
					{
						return;
					}
					internetConnectionChangeDetected(this, false);
					return;
				}
				case AccountManagerState.GeneralError:
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.SigningProblemEvent(((analyticsData != null) ? analyticsData.ToString() : null) ?? "general"));
					return;
				default:
					return;
				}
			}
			else
			{
				this.Logger.LogDebug("AccounManager state was the same {0}. Statechanged event skipped.", new object[] { this.State.ToString() });
			}
		}

		private static Dictionary<string, AccountManagerErrorCode> ErrorCodeMap
		{
			get
			{
				Dictionary<string, AccountManagerErrorCode> dictionary;
				if ((dictionary = AccountManager._errorCodeMap) == null)
				{
					Dictionary<string, AccountManagerErrorCode> dictionary2 = new Dictionary<string, AccountManagerErrorCode>();
					dictionary2.Add("JWT_TEAM_NOT_FOUND", AccountManagerErrorCode.JWT_TEAM_NOT_FOUND);
					dictionary2.Add("NOT_EMPTY_SEAT", AccountManagerErrorCode.NOT_EMPTY_SEAT);
					dictionary = dictionary2;
					dictionary2.Add("DEVICE_BLOCKED", AccountManagerErrorCode.DEVICE_BLOCKED);
				}
				AccountManager._errorCodeMap = dictionary;
				return AccountManager._errorCodeMap;
			}
		}

		private AccountManagerErrorCode ErrorCode(string value)
		{
			AccountManagerErrorCode accountManagerErrorCode;
			AccountManager.ErrorCodeMap.TryGetValue(value, out accountManagerErrorCode);
			return accountManagerErrorCode;
		}

		public static AccountManager Instance
		{
			get
			{
				if (AccountManager._instance == null)
				{
					AccountManager._instance = new AccountManager();
				}
				return AccountManager._instance;
			}
		}

		public static bool ShowAvailabilityOfUpdate()
		{
			ProfileSettings settings = AccountManager.Instance.UserProfileInfo.settings;
			UpdateSetting updateSetting = ((settings != null) ? settings.update : null);
			return updateSetting != null && updateSetting.available && updateSetting.prevent_update != "on";
		}

		private AccountManager.CachingData _data;

		private static readonly object _stateLock = new object();

		private TimerHelper _userProfileUpdateTimer;

		private TimerHelper _noInternetConnectionTimer;

		private TimerHelper _loginPollingTimer;

		private VersionInfo _updateVersionInfo;

		private static Dictionary<string, AccountManagerErrorCode> _errorCodeMap;

		private static AccountManager _instance;

		private class CachingData
		{
			public string AppToken { get; set; }

			public string SessionID { get; set; }
		}
	}
}
