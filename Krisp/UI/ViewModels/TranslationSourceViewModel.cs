using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using Krisp.Analytics;
using Krisp.Models;
using Krisp.Properties;
using Krisp.UI.Views.Windows;
using Shared.Analytics;

namespace Krisp.UI.ViewModels
{
	public class TranslationSourceViewModel : BindableBase
	{
		private TranslationSourceViewModel()
		{
			string cultureName = Settings.Default.Language;
			if (string.IsNullOrEmpty(cultureName))
			{
				cultureName = CultureInfo.CurrentCulture.Name;
			}
			CultureInfo cultureInfo2 = this.AvailableLanguages.FirstOrDefault((CultureInfo cultureInfo) => cultureInfo.Name == cultureName);
			this._selectedCulture = (this._currentCulture = cultureInfo2 ?? this.AvailableLanguages[0]);
			DefaultDeviceItem.s_AutoDescription = "Choose Automatically As System Default";
			DefaultDeviceItem.s_DefaultDescription = "Same As System Default";
			DefaultDeviceItem.s_AutoDisplayName = this["ChooseAutomatically"];
			DefaultDeviceItem.s_DefaultDisplayName = this["DefaultDevice"];
			DefaultDeviceItem.ChangeDisplayNameFn = (bool auto, string deviceName) => string.Format("{0} - {1}", auto ? this["Auto"] : this["Default"], deviceName);
		}

		public static TranslationSourceViewModel Instance { get; } = new TranslationSourceViewModel();

		public string this[string key]
		{
			get
			{
				return Resources.ResourceManager.GetString(key, this._currentCulture);
			}
		}

		public CultureInfo SelectedCulture
		{
			get
			{
				return this._selectedCulture;
			}
			set
			{
				if (this._selectedCulture != value)
				{
					this._selectedCulture = value;
					base.RaisePropertyChanged("SelectedCulture");
					AnalyticsFactory.Instance.Report(AnalyticEventComposer.PreferencesSelectorLanguage(value.Name));
					Application.Current.Dispatcher.InvokeAsync(delegate()
					{
						bool flag = new LanguageSwitchConfiramtionDialog().ShowDialog() ?? false;
						AnalyticsFactory.Instance.Report(AnalyticEventComposer.PreferencesLanguageRelaunchResponse(flag));
						if (flag)
						{
							Settings.Default.Language = value.Name;
							Settings.Default.Save();
							Process.Start(Application.ResourceAssembly.Location);
							Application.Current.Shutdown();
							return;
						}
						this._selectedCulture = this._currentCulture;
						this.RaisePropertyChanged("SelectedCulture");
					});
				}
			}
		}

		public ObservableCollection<CultureInfo> AvailableLanguages { get; } = new ObservableCollection<CultureInfo>
		{
			new CultureInfo("en-US"),
			new CultureInfo("ja-JP")
		};

		private readonly CultureInfo _currentCulture;

		private CultureInfo _selectedCulture;
	}
}
