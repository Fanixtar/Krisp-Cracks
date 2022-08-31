using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MVVMFoundation
{
	public class RelayCommand<T> : ICommand
	{
		public RelayCommand(Action<T> execute)
			: this(execute, null)
		{
		}

		public RelayCommand(Action<T> execute, Predicate<T> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException("execute");
			}
			this._execute = execute;
			this._canExecute = canExecute;
		}

		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return this._canExecute == null || this._canExecute((T)((object)parameter));
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (this._canExecute != null)
				{
					CommandManager.RequerySuggested += value;
				}
			}
			remove
			{
				if (this._canExecute != null)
				{
					CommandManager.RequerySuggested -= value;
				}
			}
		}

		public void Execute(object parameter)
		{
			this._execute((T)((object)parameter));
		}

		private readonly Action<T> _execute;

		private readonly Predicate<T> _canExecute;
	}
}
