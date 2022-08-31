using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Krisp.BackEnd
{
	public class KWebProxy : IWebProxy
	{
		public bool StoreIfValidated { get; set; }

		public KWebProxy(string proxyAddress)
		{
			if (proxyAddress == null)
			{
				throw new ArgumentNullException("proxyAddress");
			}
			this.ProxyAddress = KWebProxy.CreateProxyUri(proxyAddress);
		}

		public KWebProxy(Uri proxyAddress)
		{
			if (proxyAddress == null)
			{
				throw new ArgumentNullException("proxyAddress");
			}
			this.ProxyAddress = proxyAddress;
		}

		public Uri ProxyAddress { get; private set; }

		public ICredentials Credentials { get; set; }

		public IReadOnlyList<string> BypassList
		{
			get
			{
				return this._bypassList;
			}
			set
			{
				this._bypassList = value ?? new string[0];
				this.UpdateRegExList();
			}
		}

		public Uri GetProxy(Uri destination)
		{
			return this.ProxyAddress;
		}

		public bool IsBypassed(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (this._regExBypassList != null && this._regExBypassList.Length != 0)
			{
				string normalizedUri = uri.Scheme + "://" + uri.Host + ((!uri.IsDefaultPort) ? (":" + uri.Port) : "");
				return this._regExBypassList.Any((Regex r) => r.IsMatch(normalizedUri));
			}
			return false;
		}

		private void UpdateRegExList()
		{
			IReadOnlyList<string> bypassList = this._bypassList;
			Regex[] array;
			if (bypassList == null)
			{
				array = null;
			}
			else
			{
				array = (from x in bypassList
					select KWebProxy.WildcardToRegex(x) into x
					select new Regex(x, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).ToArray<Regex>();
			}
			this._regExBypassList = array;
		}

		private static string WildcardToRegex(string pattern)
		{
			return Regex.Escape(pattern).Replace("\\*", ".*?").Replace("\\?", ".");
		}

		private static Uri CreateProxyUri(string address)
		{
			if (address == null)
			{
				return null;
			}
			if (address.IndexOf("://") == -1)
			{
				address = "http://" + address;
			}
			return new Uri(address);
		}

		private IReadOnlyList<string> _bypassList = new string[0];

		private Regex[] _regExBypassList;
	}
}
