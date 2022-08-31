using System;
using Krisp.Analytics;
using Krisp.AppHelper;
using Krisp.Models;
using Krisp.UI.ViewModels;
using MVVMFoundation;
using Shared.Analytics;
using Shared.Helpers;

namespace Krisp.UI.Models
{
	public static class ModelFactory
	{
		public static GenericPageModel NoInternetConnectionModel()
		{
			TimerHelper timer = new TimerHelper();
			timer.Interval = 3000.0;
			timer.AutoReset = false;
			timer.Elapsed += delegate(object sender, TimerHelperElapsedEventArgs eventArgs)
			{
				if (ModelFactory._acmService.State == AccountManagerState.NoInternetConnection)
				{
					Mediator.Instance.NotifyColleagues<PageViews>("SelectPageViewModel", PageViews.GenericPage);
				}
			};
			return new GenericPageModel
			{
				PageText = TranslationSourceViewModel.Instance["NoInternetMessage"],
				ButtonText = TranslationSourceViewModel.Instance["TryAgain"],
				ButtonAction = delegate
				{
					timer.Start();
					Mediator.Instance.NotifyColleagues<PageViews>("SelectPageViewModel", PageViews.ProgressPage);
					ModelFactory._acmService.RecoverAfterSyncError();
				}
			};
		}

		public static GenericPageModel LoggingInModel()
		{
			GenericPageModel genericPageModel = new GenericPageModel();
			genericPageModel.PageText = TranslationSourceViewModel.Instance["LoadingKrispPleaseWait"];
			genericPageModel.ButtonText = TranslationSourceViewModel.Instance["TryAgain"];
			genericPageModel.ButtonAction = delegate()
			{
				AnalyticsFactory.Instance.Report(AnalyticEventComposer.TryAgainEvent());
				ServiceContainer.Instance.GetService<IAccountManager>().OneTimeLogin();
			};
			return genericPageModel;
		}

		private static void TeamKeyReloadAction()
		{
			DeviceLoginHelper.ReloadKey();
			ModelFactory._acmService.DeviceLogin();
		}

		private static void DeviceKeyConfigReload()
		{
			DeviceLoginHelper.ReloadConfig();
			ModelFactory._acmService.DeviceLogin();
		}

		public static GenericPageModel GenericPageModel(AccountManagerStateChangedEventArgs args)
		{
			GenericPageModel genericPageModel = new GenericPageModel();
			switch (args.ReasonCode)
			{
			case AccountManagerErrorCode.JWT_TEAM_NOT_FOUND:
			case AccountManagerErrorCode.TEAM_NOT_FOUND:
				genericPageModel.PageText = TranslationSourceViewModel.Instance["TeamNotFound"];
				genericPageModel.ButtonText = TranslationSourceViewModel.Instance["TryAgain"];
				genericPageModel.ButtonAction = new Action(ModelFactory.TeamKeyReloadAction);
				break;
			case AccountManagerErrorCode.NOT_EMPTY_SEAT:
				genericPageModel.PageText = TranslationSourceViewModel.Instance["NoEmptySeats"];
				genericPageModel.ButtonText = TranslationSourceViewModel.Instance["TryAgain"];
				genericPageModel.ButtonAction = new Action(ModelFactory.TeamKeyReloadAction);
				break;
			case AccountManagerErrorCode.DEVICE_BLOCKED:
				genericPageModel.PageText = TranslationSourceViewModel.Instance["DeviceBlocked"];
				genericPageModel.ButtonText = TranslationSourceViewModel.Instance["TryAgain"];
				genericPageModel.ButtonAction = new Action(ModelFactory.TeamKeyReloadAction);
				break;
			case AccountManagerErrorCode.INSTALL_ID_MECHANISM_NOT_ENABLED:
				genericPageModel.PageText = TranslationSourceViewModel.Instance["LicenseNotValid"];
				genericPageModel.ButtonText = TranslationSourceViewModel.Instance["TryAgain"];
				genericPageModel.ButtonAction = new Action(ModelFactory.DeviceKeyConfigReload);
				break;
			}
			return genericPageModel;
		}

		private static IAccountManager _acmService = ServiceContainer.Instance.GetService<IAccountManager>();
	}
}
