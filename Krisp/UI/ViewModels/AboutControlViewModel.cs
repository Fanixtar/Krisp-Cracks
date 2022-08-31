using System;
using System.Reflection;
using System.Windows.Input;
using Krisp.UI.Views;
using MVVMFoundation;
using Shared.Helpers;

namespace Krisp.UI.ViewModels
{
	internal class AboutControlViewModel : BindableBase
	{
		public AboutControlViewModel()
		{
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			AssemblyCopyrightAttribute customAttribute = entryAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
			AssemblyDescriptionAttribute customAttribute2 = entryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
			AssemblyCompanyAttribute customAttribute3 = entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
			this.Title = entryAssembly.GetName().Name;
			this.Version = EnvHelper.KrispVersion.ToString();
			this.Copyright = customAttribute.Copyright;
			this.Description = customAttribute2.Description;
			this.Publisher = customAttribute3.Company;
		}

		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if (this._Description != value)
				{
					this._Description = value;
					base.RaisePropertyChanged("Description");
				}
			}
		}

		public string Version
		{
			get
			{
				return this._Version;
			}
			set
			{
				if (this._Version != value)
				{
					this._Version = value;
					base.RaisePropertyChanged("Version");
				}
			}
		}

		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				if (this._Title != value)
				{
					this._Title = value;
					base.RaisePropertyChanged("Title");
				}
			}
		}

		public string Publisher
		{
			get
			{
				return this._Publisher;
			}
			set
			{
				if (this._Publisher != value)
				{
					this._Publisher = value;
					base.RaisePropertyChanged("Publisher");
				}
			}
		}

		public string Copyright
		{
			get
			{
				return this._Copyright;
			}
			set
			{
				if (this._Copyright != value)
				{
					this._Copyright = value;
					base.RaisePropertyChanged("Copyright");
				}
			}
		}

		public ICommand PrivacyPolicyCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._privacyPolicyCommand) == null)
				{
					relayCommand = (this._privacyPolicyCommand = new RelayCommand(delegate(object param)
					{
						Helpers.OpenUrl(UrlProvider.GetPrivacyPolicyUrl());
					}));
				}
				return relayCommand;
			}
		}

		public ICommand TermsOfUseCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._termsOfUseCommand) == null)
				{
					relayCommand = (this._termsOfUseCommand = new RelayCommand(delegate(object param)
					{
						Helpers.OpenUrl(UrlProvider.GetTermsOfUseUrl());
					}));
				}
				return relayCommand;
			}
		}

		private string _Description;

		private string _Title;

		private string _Version;

		private string _Copyright;

		private string _Publisher;

		private RelayCommand _privacyPolicyCommand;

		private RelayCommand _termsOfUseCommand;
	}
}
