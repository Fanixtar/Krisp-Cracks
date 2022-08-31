using System;
using System.Net;
using System.Security;
using System.Windows.Controls;
using System.Windows.Input;
using Krisp.Models;
using MVVMFoundation;

namespace Krisp.UI.ViewModels
{
	public class ProxyCredentialsPromptViewModel
	{
		public ICommand SubmitCommand
		{
			get
			{
				RelayCommand relayCommand;
				if ((relayCommand = this._submitCommand) == null)
				{
					relayCommand = (this._submitCommand = new RelayCommand(delegate(object param)
					{
						PasswordBox passwordBox = param as PasswordBox;
						this._credData.Credentials.SecurePassword = ((passwordBox != null) ? passwordBox.SecurePassword : null);
					}));
				}
				return relayCommand;
			}
		}

		public string Title
		{
			get
			{
				return this._credData.Title;
			}
		}

		public string Message
		{
			get
			{
				return this._credData.Message;
			}
		}

		public bool SaveChecked
		{
			get
			{
				return this._credData.SaveChecked;
			}
			set
			{
				this._credData.SaveChecked = value;
			}
		}

		public bool GenericCredentials
		{
			get
			{
				return this._credData.GenericCredentials;
			}
		}

		public bool ShowSaveCheckBox
		{
			get
			{
				return this._credData.ShowSaveCheckBox;
			}
		}

		public int ErrorCode
		{
			get
			{
				return this._credData.ErrorCode;
			}
			set
			{
				this._credData.ErrorCode = value;
			}
		}

		public string UserName
		{
			get
			{
				NetworkCredential credentials = this._credData.Credentials;
				if (credentials == null)
				{
					return null;
				}
				return credentials.UserName;
			}
			set
			{
				this._credData.Credentials.UserName = value;
			}
		}

		public SecureString SecurePassword
		{
			get
			{
				NetworkCredential credentials = this._credData.Credentials;
				if (credentials == null)
				{
					return null;
				}
				return credentials.SecurePassword;
			}
			set
			{
				this._credData.Credentials.SecurePassword = value;
			}
		}

		public ProxyCredentialsPromptViewModel(CredentialPromptData credData)
		{
			this._credData = credData;
		}

		public CredentialPromptData _credData;

		private RelayCommand _submitCommand;
	}
}
