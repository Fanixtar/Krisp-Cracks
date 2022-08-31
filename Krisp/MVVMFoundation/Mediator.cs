using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MVVMFoundation
{
	public class Mediator
	{
		private Mediator()
		{
		}

		public static Mediator Instance
		{
			get
			{
				return Mediator.instance;
			}
		}

		public void Register(object view)
		{
			foreach (MethodInfo methodInfo in view.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				foreach (MediatorMessageSinkAttribute mediatorMessageSinkAttribute in methodInfo.GetCustomAttributes(typeof(MediatorMessageSinkAttribute), true))
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					Type type;
					if (parameters.Length == 1)
					{
						type = typeof(Action<>).MakeGenericType(new Type[] { parameters[0].ParameterType });
					}
					else
					{
						if (parameters.Length != 2)
						{
							throw new InvalidCastException("Cannot cast " + methodInfo.Name + " to Action<T> delegate type.");
						}
						type = typeof(Action<, >).MakeGenericType(new Type[]
						{
							typeof(object),
							parameters[1].ParameterType
						});
					}
					object obj = mediatorMessageSinkAttribute.MessageKey ?? type;
					if (methodInfo.IsStatic)
					{
						this.RegisterHandler(obj, type, Delegate.CreateDelegate(type, methodInfo));
					}
					else
					{
						this.RegisterHandler(obj, type, Delegate.CreateDelegate(type, view, methodInfo.Name));
					}
				}
			}
		}

		public void Unregister(object view)
		{
			foreach (MethodInfo methodInfo in view.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				foreach (MediatorMessageSinkAttribute mediatorMessageSinkAttribute in methodInfo.GetCustomAttributes(typeof(MediatorMessageSinkAttribute), true))
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length != 1)
					{
						throw new InvalidCastException("Cannot cast " + methodInfo.Name + " to Action<T> delegate type.");
					}
					Type type = typeof(Action<>).MakeGenericType(new Type[] { parameters[0].ParameterType });
					object obj = mediatorMessageSinkAttribute.MessageKey ?? type;
					if (methodInfo.IsStatic)
					{
						this.UnregisterHandler(obj, type, Delegate.CreateDelegate(type, methodInfo));
					}
					else
					{
						this.UnregisterHandler(obj, type, Delegate.CreateDelegate(type, view, methodInfo.Name));
					}
				}
			}
		}

		public void RegisterHandler<T>(string key, Action<T> handler)
		{
			this.RegisterHandler(key, handler.GetType(), handler);
		}

		public void RegisterHandler<T>(Action<T> handler)
		{
			this.RegisterHandler(typeof(Action<T>), handler.GetType(), handler);
		}

		public void UnregisterHandler<T>(string key, Action<T> handler)
		{
			this.UnregisterHandler(key, handler.GetType(), handler);
		}

		public void UnregisterHandler<T>(Action<T> handler)
		{
			this.UnregisterHandler(typeof(Action<T>), handler.GetType(), handler);
		}

		private void RegisterHandler(object key, Type actionType, Delegate handler)
		{
			Mediator.WeakAction weakAction = new Mediator.WeakAction(handler.Target, actionType, handler.Method);
			Dictionary<object, List<Mediator.WeakAction>> registeredHandlers = this._registeredHandlers;
			lock (registeredHandlers)
			{
				List<Mediator.WeakAction> list;
				if (this._registeredHandlers.TryGetValue(key, out list))
				{
					if (list.Count > 0)
					{
						Mediator.WeakAction weakAction2 = list[0];
						if (weakAction2.ActionType != actionType && !weakAction2.ActionType.IsAssignableFrom(actionType))
						{
							throw new ArgumentException("Invalid key passed to RegisterHandler - existing handler has incompatible parameter type");
						}
					}
					list.Add(weakAction);
				}
				else
				{
					list = new List<Mediator.WeakAction> { weakAction };
					this._registeredHandlers.Add(key, list);
				}
			}
		}

		private void UnregisterHandler(object key, Type actionType, Delegate handler)
		{
			Dictionary<object, List<Mediator.WeakAction>> registeredHandlers = this._registeredHandlers;
			lock (registeredHandlers)
			{
				List<Mediator.WeakAction> list;
				if (this._registeredHandlers.TryGetValue(key, out list))
				{
					list.RemoveAll((Mediator.WeakAction wa) => handler == wa.GetMethod() && actionType == wa.ActionType);
					if (list.Count == 0)
					{
						this._registeredHandlers.Remove(key);
					}
				}
			}
		}

		private bool NotifyColleagues(object key, WeakReference sender, object message)
		{
			Dictionary<object, List<Mediator.WeakAction>> dictionary = this._registeredHandlers;
			List<Mediator.WeakAction> list;
			lock (dictionary)
			{
				if (!this._registeredHandlers.TryGetValue(key, out list))
				{
					return false;
				}
			}
			foreach (Mediator.WeakAction weakAction in list)
			{
				Delegate method = weakAction.GetMethod();
				if (method != null)
				{
					if (sender != null && method.Method.GetParameters().Count<ParameterInfo>() == 2)
					{
						method.DynamicInvoke(new object[] { sender, message });
					}
					else
					{
						method.DynamicInvoke(new object[] { message });
					}
				}
			}
			dictionary = this._registeredHandlers;
			lock (dictionary)
			{
				list.RemoveAll((Mediator.WeakAction wa) => wa.HasBeenCollected);
			}
			return true;
		}

		public bool NotifyColleagues<T>(string key, WeakReference sender, T message)
		{
			return this.NotifyColleagues(key, sender, message);
		}

		public bool NotifyColleagues<T>(string key, T message)
		{
			return this.NotifyColleagues(key, null, message);
		}

		public bool NotifyColleagues<T>(T message)
		{
			Type actionType = typeof(Action<>).MakeGenericType(new Type[] { typeof(T) });
			IEnumerable<object> enumerable = this._registeredHandlers.Keys.Where((object key) => key is Type && ((Type)key).IsAssignableFrom(actionType));
			bool flag = false;
			foreach (object obj in enumerable)
			{
				flag |= this.NotifyColleagues(obj, null, message);
			}
			return flag;
		}

		public void NotifyColleaguesAsync<T>(string key, T message)
		{
			Func<string, T, bool> smaFunc = new Func<string, T, bool>(this.NotifyColleagues<T>);
			smaFunc.BeginInvoke(key, message, delegate(IAsyncResult ia)
			{
				try
				{
					smaFunc.EndInvoke(ia);
				}
				catch
				{
				}
			}, null);
		}

		public void NotifyColleaguesAsync<T>(T message)
		{
			Func<T, bool> smaFunc = new Func<T, bool>(this.NotifyColleagues<T>);
			smaFunc.BeginInvoke(message, delegate(IAsyncResult ia)
			{
				try
				{
					smaFunc.EndInvoke(ia);
				}
				catch
				{
				}
			}, null);
		}

		private static readonly Mediator instance = new Mediator();

		private static readonly object syncLock = new object();

		private readonly Dictionary<object, List<Mediator.WeakAction>> _registeredHandlers = new Dictionary<object, List<Mediator.WeakAction>>();

		internal class WeakAction
		{
			public WeakAction(object target, Type actionType, MethodBase mi)
			{
				if (target == null)
				{
					this._ownerType = mi.DeclaringType;
				}
				else
				{
					this._target = new WeakReference(target);
				}
				this._methodName = mi.Name;
				this._actionType = actionType;
			}

			public Type ActionType
			{
				get
				{
					return this._actionType;
				}
			}

			public bool HasBeenCollected
			{
				get
				{
					return this._ownerType == null && (this._target == null || !this._target.IsAlive);
				}
			}

			public Delegate GetMethod()
			{
				if (this._ownerType != null)
				{
					return Delegate.CreateDelegate(this._actionType, this._ownerType, this._methodName);
				}
				if (this._target != null && this._target.IsAlive)
				{
					object target = this._target.Target;
					if (target != null)
					{
						return Delegate.CreateDelegate(this._actionType, target, this._methodName);
					}
				}
				return null;
			}

			private readonly WeakReference _target;

			private readonly Type _ownerType;

			private readonly Type _actionType;

			private readonly string _methodName;
		}
	}
}
