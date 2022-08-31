using System;
using Krisp.AppHelper;
using Krisp.BackEnd;
using Krisp.Models;
using Krisp.UI.Models;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	public class UpdateInfoViewModel : BindableBase
	{
		public UpdateInfo UpdateInfo
		{
			get
			{
				return this._updateInfo;
			}
			private set
			{
				if (this._updateInfo != value)
				{
					this._updateInfo = value;
					base.RaisePropertyChanged("UpdateInfo");
					Mediator.Instance.NotifyColleagues<UpdateInfo>("UpdateInfoChanged", this._updateInfo);
				}
			}
		}

		public UpdateInfoViewModel()
		{
			IAccountManager service = ServiceContainer.Instance.GetService<IAccountManager>();
			this.UpdateInfo = ((service.UpdateVersionInfo != null && service.UpdateVersionInfo.resultCode > 0) ? new UpdateInfo(service.UpdateVersionInfo) : null);
			service.UpdateVersionInfoChanged += this.AccountManager_UpdateVersionInfoChanged;
		}

		private void AccountManager_UpdateVersionInfoChanged(object sender, VersionInfo e)
		{
			if (e != null && e.resultCode > 0)
			{
				this.UpdateInfo = new UpdateInfo(e);
				return;
			}
			this.UpdateInfo = null;
		}

		private UpdateInfo _updateInfo;

		private readonly Logger _logger = LogWrapper.GetLogger("Update");
	}
}
