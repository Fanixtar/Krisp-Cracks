using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Models;
using MVVMFoundation;
using Newtonsoft.Json;
using RestSharp;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.Analytics
{
	public class AnalyticsManager : RestClient, IKrispAnalytics
	{
		public AnalyticsManager(AnalyticsManager.AnalyticsConfig cfg)
			: base(cfg.rest_url)
		{
			this.aCfg = cfg;
			this.InitevEntDicardFilters();
			this.aCfg.interval = TimeSpan.FromMinutes(Math.Max(Math.Min(this.aCfg.interval, 1440.0), 1.0)).TotalMilliseconds;
			this.aCfg.batchCount = Math.Max(Math.Min(this.aCfg.batchCount, 200U), 1U);
			this.batchSendTimer = new TimerHelper(1000.0);
			this.batchSendTimer.AutoReset = false;
			this.batchSendTimer.Elapsed += delegate(object sender, TimerHelperElapsedEventArgs eventArgs)
			{
				double interval = this.failResendInterval;
				try
				{
					if (this.SubmitCachedEvents())
					{
						interval = this.aCfg.interval;
					}
				}
				finally
				{
					this.batchSendTimer.Interval = interval;
					this._logger.LogDebug("Analytics batch submit rescheduled for {0} seconds", new object[] { interval / 1000.0 });
				}
			};
			this.batchSendTimer.Start();
			base.Timeout = BackEndHelper.ANALYTIC_REQUEST_TIMEOUT;
			RestClientExtensions.AddDefaultHeader(this, "Authorization", cfg.stoken);
			RestClientExtensions.AddDefaultHeader(this, "User-Agent", BackEndHelper.GetHTTPUserAgent());
			this._accManager = ServiceContainer.Instance.GetService<IAccountManager>();
		}

		public void Report(AnalyticEvent aEvent)
		{
			try
			{
				if (this._accManager.IsLoggedIn)
				{
					UserProfileInfo userProfileInfo = this._accManager.UserProfileInfo;
					aEvent.user_id = ((userProfileInfo != null) ? new uint?(userProfileInfo.id) : null);
					UserProfileInfo userProfileInfo2 = this._accManager.UserProfileInfo;
					uint? num;
					if (userProfileInfo2 == null)
					{
						num = null;
					}
					else
					{
						Team team = userProfileInfo2.team;
						num = ((team != null) ? new uint?(team.id) : null);
					}
					aEvent.team_id = num;
				}
				JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
				{
					Formatting = 0,
					NullValueHandling = 1,
					DefaultValueHandling = 1
				};
				string text = JsonConvert.SerializeObject(aEvent, jsonSerializerSettings);
				object obj = this.lockobj;
				lock (obj)
				{
					File.AppendAllText(this.aCfg.cache_file, text + Environment.NewLine);
				}
				this._logger.LogDebug("Analytic event cached: {0}", new object[] { text });
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Unable to cache analytic event {0}, ex: {1}", new object[] { aEvent.name, ex.Message });
			}
		}

		private void DiscardEntriesFromCache(int dropCount)
		{
			object obj = this.lockobj;
			lock (obj)
			{
				File.WriteAllLines(this.aCfg.cache_file, File.ReadAllLines(this.aCfg.cache_file).Skip(dropCount));
			}
			this._logger.LogDebug("Dropped {0} analytic events", new object[] { dropCount });
		}

		private void InitevEntDicardFilters()
		{
			this._eventDiscardFilters.Clear();
			this._eventDiscardFilters.Add(AnalyticEventComposer.ANALYTIC_EVENT_MIC_STREAM_STATS, 30);
			this._eventDiscardFilters.Add(AnalyticEventComposer.ANALYTIC_EVENT_MIC_CALL_END, 30);
			this._eventDiscardFilters.Add(AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_STREAM_STATS, 30);
			this._eventDiscardFilters.Add(AnalyticEventComposer.ANALYTIC_EVENT_SPEAKER_CALL_END, 30);
		}

		private bool BatchSubmit(IEnumerable<AnalyticEventEx> entries)
		{
			try
			{
				RestRequest restRequest = new RestRequest(this.store_endpoint, 1);
				restRequest.AddJsonBody(entries);
				IRestResponse restResponse = this.Execute(restRequest);
				if (!restResponse.IsSuccessful)
				{
					Logger logger = this._logger;
					string text = "Unable to submit analytic events, http error: {0} {1}, ex: {2}";
					object[] array = new object[3];
					array[0] = restResponse.StatusCode;
					array[1] = restResponse.StatusDescription;
					int num = 2;
					Exception errorException = restResponse.ErrorException;
					array[num] = ((errorException != null) ? errorException.Message : null);
					logger.LogWarning(text, array);
					return restResponse.ErrorException == null;
				}
				return true;
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Unable to submit analytic events, exception: {0}", new object[] { ex.Message });
			}
			return false;
		}

		private bool SubmitCachedEvents()
		{
			try
			{
				int num = 0;
				if (File.Exists(this.aCfg.cache_file))
				{
					List<string> list = new List<string>();
					object obj = this.lockobj;
					lock (obj)
					{
						list.AddRange(File.ReadAllLines(this.aCfg.cache_file));
					}
					if ((long)list.Count > (long)((ulong)this.aCfg.maxCacheCount))
					{
						num = list.Count - (int)this.aCfg.maxCacheCount;
						list.RemoveRange(0, num);
						this.DiscardEntriesFromCache(num);
					}
					this._logger.LogDebug("Start processing {0} analytic entries", new object[] { list.Count });
					Dictionary<string, int> cleanCounters = this.getCleanCounters();
					while (list.Count > 0)
					{
						int num2 = Math.Min((int)this.aCfg.batchCount, list.Count);
						List<AnalyticEventEx> list2 = new List<AnalyticEventEx>(num2);
						int i = 0;
						while (i < num2)
						{
							AnalyticEventEx analyticEventEx = JsonConvert.DeserializeObject<AnalyticEventEx>(list[i]);
							if (num <= 0 || !cleanCounters.ContainsKey(analyticEventEx.name))
							{
								goto IL_125;
							}
							Dictionary<string, int> dictionary = cleanCounters;
							string name = analyticEventEx.name;
							int num3 = dictionary[name];
							dictionary[name] = num3 - 1;
							if (num3 >= 0)
							{
								goto IL_125;
							}
							IL_12E:
							i++;
							continue;
							IL_125:
							list2.Add(analyticEventEx);
							goto IL_12E;
						}
						if (num > 0)
						{
							foreach (string text in cleanCounters.Keys)
							{
								if (cleanCounters[text] < 0)
								{
									list2.Add(AnalyticEventComposer.DroppedEvent(text, (uint)(-(uint)cleanCounters[text])));
								}
							}
						}
						if (!this.BatchSubmit(list2))
						{
							this._logger.LogDebug("Unable to submit {0} ({1}) analytic events", new object[] { num2, list.Count });
							break;
						}
						list.RemoveRange(0, num2);
						this.DiscardEntriesFromCache(num2);
						this._logger.LogDebug("Processed events count {0} ({1})", new object[] { num2, list.Count });
					}
					return list.Count == 0;
				}
			}
			catch (Exception ex)
			{
				this._logger.LogWarning("Unable to handle cached events, exception: {0}", new object[] { ex.Message });
			}
			return false;
		}

		private Dictionary<string, int> getCleanCounters()
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (string text in this._eventDiscardFilters.Keys)
			{
				dictionary.Add(text, this._eventDiscardFilters[text]);
			}
			return dictionary;
		}

		private readonly double failResendInterval = TimeSpan.FromMinutes(10.0).TotalMilliseconds;

		private readonly object lockobj = new object();

		private readonly string store_endpoint = "/store";

		private AnalyticsManager.AnalyticsConfig aCfg;

		private TimerHelper batchSendTimer;

		private Dictionary<string, int> _eventDiscardFilters = new Dictionary<string, int>();

		private Logger _logger = LogWrapper.GetLogger("Analytics");

		private IAccountManager _accManager;

		public struct AnalyticsConfig
		{
			public string rest_url { get; set; }

			public string cache_file { get; set; }

			public string stoken { get; set; }

			public double interval { get; set; }

			public uint batchCount { get; set; }

			public uint maxCacheCount { get; set; }
		}
	}
}
