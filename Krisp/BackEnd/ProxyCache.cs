using System;
using System.Collections.Concurrent;
using System.Net;
using CredentialManagement;

namespace Krisp.BackEnd
{
	public class ProxyCache : ICredentials
	{
		private static ProxyCache FromDefaultSettings()
		{
			return new ProxyCache();
		}

		public static ProxyCache Instance
		{
			get
			{
				return ProxyCache._instance.Value;
			}
		}

		public Guid Version { get; private set; } = Guid.NewGuid();

		private ProxyCache()
		{
		}

		public KWebProxy GetProxy(Uri sourceUri)
		{
			if (ProxyCache.IsSystemProxySet(sourceUri))
			{
				KWebProxy systemProxy = ProxyCache.GetSystemProxy(sourceUri);
				this.TryAddProxyCredentialsToCache(systemProxy);
				systemProxy.Credentials = this;
				return systemProxy;
			}
			return null;
		}

		private bool TryAddProxyCredentialsToCache(KWebProxy configuredProxy)
		{
			ICredentials credentials = configuredProxy.Credentials ?? CredentialCache.DefaultCredentials;
			return this._cachedCredentials.TryAdd(configuredProxy.ProxyAddress, credentials);
		}

		public void UpdateCredential(Uri proxyAddress, NetworkCredential credentials)
		{
			if (credentials == null)
			{
				throw new ArgumentNullException("credentials");
			}
			NetworkCredential networkCredential = (NetworkCredential)this._cachedCredentials.AddOrUpdate(proxyAddress, delegate(Uri _)
			{
				this.Version = Guid.NewGuid();
				return credentials;
			}, delegate(Uri _, ICredentials __)
			{
				this.Version = Guid.NewGuid();
				return credentials;
			});
		}

		public void UpdateAndStoreCredential(KWebProxy proxy, NetworkCredential credentials)
		{
			if (proxy == null || credentials == null)
			{
				throw new ArgumentNullException("credentials");
			}
			this.UpdateCredential(proxy.ProxyAddress, credentials);
			if (proxy.StoreIfValidated)
			{
				new Credential
				{
					Username = credentials.UserName,
					Target = proxy.ProxyAddress.Host,
					Type = 1,
					SecurePassword = credentials.SecurePassword,
					PersistanceType = 2,
					Type = 1
				}.Save();
			}
		}

		public NetworkCredential GetCredential(Uri proxyAddress, string authType)
		{
			ICredentials credentials;
			if (this._cachedCredentials.TryGetValue(proxyAddress, out credentials))
			{
				NetworkCredential networkCredential = credentials.GetCredential(proxyAddress, authType);
				if (!(credentials is ProxyCache) && string.Compare(authType, "basic", true) == 0)
				{
					CredentialSet credentialSet = new CredentialSet();
					credentialSet.Load();
					Credential credential = credentialSet.Find((Credential f) => f.Target == proxyAddress.Host && f.Type == 1);
					if (credential != null)
					{
						networkCredential = new NetworkCredential(credential.Username, credential.SecurePassword);
					}
					else
					{
						credential = credentialSet.Find((Credential f) => f.Target == proxyAddress.Host && f.Type == 2);
						if (credential != null)
						{
							networkCredential = new NetworkCredential(credential.Username, credential.SecurePassword);
						}
					}
					credentialSet.Dispose();
				}
				return networkCredential;
			}
			return null;
		}

		private static KWebProxy GetSystemProxy(Uri uri)
		{
			return new KWebProxy(ProxyCache._originalSystemProxy.GetProxy(uri));
		}

		private static bool IsSystemProxySet(Uri uri)
		{
			IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
			if (defaultWebProxy != null)
			{
				Uri proxy = defaultWebProxy.GetProxy(uri);
				if (proxy != null)
				{
					return !string.Equals(new Uri(proxy.AbsoluteUri).AbsoluteUri, uri.AbsoluteUri) && !defaultWebProxy.IsBypassed(uri);
				}
			}
			return false;
		}

		private static readonly IWebProxy _originalSystemProxy = WebRequest.GetSystemWebProxy();

		private readonly ConcurrentDictionary<Uri, ICredentials> _cachedCredentials = new ConcurrentDictionary<Uri, ICredentials>();

		private static readonly Lazy<ProxyCache> _instance = new Lazy<ProxyCache>(() => ProxyCache.FromDefaultSettings());
	}
}
