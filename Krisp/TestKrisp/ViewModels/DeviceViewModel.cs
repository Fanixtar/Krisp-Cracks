using System;
using Krisp.UI.ViewModels;

namespace Krisp.TestKrisp.ViewModels
{
	public abstract class DeviceViewModel : BindableBase
	{
		public abstract void Destroy();
	}
}
