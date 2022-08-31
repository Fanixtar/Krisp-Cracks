using System;
using System.Threading.Tasks;
using HidLibrary;
using Krisp.AppHelper;

namespace Krisp.Core.Internals
{
	internal class JHIDHeadset : HIDHeadset
	{
		public JHIDHeadset(HidDevice hidDev)
		{
			this._logger = LogWrapper.GetLogger("JHIDHeadset");
			this._device = hidDev;
		}

		protected override void writeReport()
		{
			try
			{
				if (this._device == null)
				{
					this._logger.LogWarning("The HID device is null.");
				}
				else
				{
					if (!this._device.IsOpen)
					{
						this._device.OpenDevice();
						this.readReports();
					}
					HidReport hidReport = this._device.CreateReport();
					hidReport.ReportId = 2;
					hidReport.Data[0] = (byte)(this._offHookStatus | ((this._muteStatus ? 1 : 0) << 1) | (this._ringStatus << 2) | (this._onHoldStatus << 3) | (this._microphoneStatus << 4) | (this._ringerStatus << 5));
					object obj = this.wLocker;
					lock (obj)
					{
						this._logger.LogInfo(string.Format("send report: -- {0}", hidReport.Data[0]));
						bool flag2 = this._device.WriteReport(hidReport, 100);
						this._logger.LogInfo(string.Format("Result: {0}", flag2));
					}
				}
			}
			catch (Exception ex)
			{
				this._logger.LogError("Error on write reports. {0}", new object[] { ex });
			}
		}

		protected override void readReports()
		{
			Task.Run(delegate()
			{
				try
				{
					while (this._device != null && this._device.IsConnected && this._device.IsOpen)
					{
						HidDevice device = this._device;
						HidReport hidReport = ((device != null) ? device.ReadReport() : null);
						object obj = this.wLocker;
						lock (obj)
						{
							if (hidReport != null && hidReport.ReadStatus == HidDeviceData.ReadStatus.Success)
							{
								this._logger.LogInfo(string.Format("got report {0}:  {1}", hidReport.ReportId, hidReport.Data[0]));
								if (hidReport.ReportId == 2)
								{
									this.hookSwitch = (int)(hidReport.Data[0] & 1);
									this.lineBusyTone = (int)(hidReport.Data[0] & 2);
									this.phoneMute = (int)(hidReport.Data[0] & 4);
									this.flash = (int)(hidReport.Data[0] & 8);
									this.redial = (int)(hidReport.Data[0] & 16);
									this.speedDial = (int)(hidReport.Data[0] & 32);
									this.progrButton = (int)(hidReport.Data[0] & 64);
									this.keypadValue = ((hidReport.Data[0] & 128) >> 7) | ((int)(hidReport.Data[1] & 7) << 1);
									base.OffHookStatus = this.hookSwitch;
									if (this.phoneMute != 0)
									{
										this._muteStatus = !this._muteStatus;
										base.TrigerMute();
									}
								}
								else if (hidReport.ReportId == 0 && !this._device.IsOpen)
								{
									break;
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					this._logger.LogError("Error on reading reports. {0}", new object[] { ex });
				}
			});
		}

		protected Logger _logger;

		private object wLocker = new object();
	}
}
