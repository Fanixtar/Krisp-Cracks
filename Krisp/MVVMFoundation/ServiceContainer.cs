using System;
using System.Collections.Generic;

namespace MVVMFoundation
{
	public class ServiceContainer
	{
		private ServiceContainer()
		{
			this._serviceMap = new Dictionary<Type, object>();
			this._serviceMapLock = new object();
		}

		public void AddService<TServiceContract>(TServiceContract implementation) where TServiceContract : class
		{
			object serviceMapLock = this._serviceMapLock;
			lock (serviceMapLock)
			{
				this._serviceMap[typeof(TServiceContract)] = implementation;
			}
		}

		public TServiceContract GetService<TServiceContract>() where TServiceContract : class
		{
			object serviceMapLock = this._serviceMapLock;
			object obj;
			lock (serviceMapLock)
			{
				this._serviceMap.TryGetValue(typeof(TServiceContract), out obj);
			}
			return obj as TServiceContract;
		}

		public static readonly ServiceContainer Instance = new ServiceContainer();

		private readonly Dictionary<Type, object> _serviceMap;

		private readonly object _serviceMapLock;
	}
}
