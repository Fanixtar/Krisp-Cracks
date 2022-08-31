using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Krisp.Models
{
	public interface IAudioDeviceCollection : IEnumerable<IAudioDevice>, IEnumerable, INotifyCollectionChanged
	{
		bool TryFind(string deviceId, out IAudioDevice found);

		void SetDefault(IAudioDevice device);

		void RemoveDefault(object sender);
	}
}
