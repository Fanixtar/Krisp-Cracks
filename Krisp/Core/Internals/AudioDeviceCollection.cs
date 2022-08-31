using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Krisp.AppHelper;
using Krisp.Models;

namespace Krisp.Core.Internals
{
	internal class AudioDeviceCollection : IAudioDeviceCollection, IEnumerable<IAudioDevice>, IEnumerable, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void Add(IAudioDevice device)
		{
			if (device.InterfaceName.CompareTo("Krisp") == 0)
			{
				return;
			}
			if (this._devices.TryAdd(device.Id, device))
			{
				NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
				if (collectionChanged == null)
				{
					return;
				}
				collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
			}
		}

		public void Remove(IAudioDevice device)
		{
			IAudioDevice audioDevice;
			if (this._devices.TryRemove(device.Id, out audioDevice))
			{
				NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
				if (collectionChanged == null)
				{
					return;
				}
				collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, audioDevice));
			}
		}

		public void RemoveAll()
		{
			this._devices.Clear();
			NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
			if (collectionChanged == null)
			{
				return;
			}
			collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public bool TryFind(string deviceId, out IAudioDevice found)
		{
			if (deviceId == null)
			{
				found = null;
				return false;
			}
			return this._devices.TryGetValue(deviceId, out found);
		}

		public void SetDefault(IAudioDevice device)
		{
			IAudioDevice audioDevice;
			if (this.TryFind("", out audioDevice))
			{
				if (!this._devices.TryUpdate("", device, audioDevice))
				{
					this._logger.LogError("Unable to update the default device in device list.");
					return;
				}
			}
			else if (this._devices.TryAdd("", device))
			{
				NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
				if (collectionChanged == null)
				{
					return;
				}
				collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
				return;
			}
			else
			{
				this._logger.LogError("Unable to Add the default device in to device list.");
			}
		}

		public void RemoveDefault(object sender)
		{
			IAudioDevice audioDevice;
			if (this._devices.TryRemove("", out audioDevice))
			{
				NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
				if (collectionChanged == null)
				{
					return;
				}
				collectionChanged(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, audioDevice));
			}
		}

		public bool IsEmpty
		{
			get
			{
				bool flag = false;
				if (this._devices.Count == 1 && this._devices.Keys.Contains(""))
				{
					flag = true;
				}
				return flag;
			}
		}

		public IAudioDevice GetFirstOrNull()
		{
			IAudioDevice audioDevice = null;
			if (this._devices.Count > 1 && this._devices.Keys.Contains(""))
			{
				string text = this._devices.Keys.FirstOrDefault<string>();
				if (text != null && !this._devices.TryGetValue(text, out audioDevice))
				{
					audioDevice = null;
				}
			}
			return audioDevice;
		}

		public IEnumerator<IAudioDevice> GetEnumerator()
		{
			return this._devices.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private readonly ConcurrentDictionary<string, IAudioDevice> _devices = new ConcurrentDictionary<string, IAudioDevice>();

		private Logger _logger = LogWrapper.GetLogger("AudioDeviceCollection");
	}
}
