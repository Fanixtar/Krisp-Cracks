using System;
using System.ComponentModel;

namespace Krisp.UI.ViewModels
{
	public class BindableBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged = delegate(object <p0>, PropertyChangedEventArgs <p1>)
		{
		};

		protected void RaisePropertyChanged(string name)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
