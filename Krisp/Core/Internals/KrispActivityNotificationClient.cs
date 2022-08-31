using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Krisp.AppHelper;
using Shared.Interops;

namespace Krisp.Core.Internals
{
	public class KrispActivityNotificationClient : KrispFDevice
	{
		public event EventHandler<StreamActivityState> CapturerStreamActivityChanged;

		public event EventHandler<StreamActivityState> RendererStreamActivityChanged;

		public KrispActivityNotificationClient()
		{
			this._dispatcher = Application.Current.Dispatcher;
		}

		public bool IsClientValidated()
		{
			return this.krispDeviceHandle != null && !this.krispDeviceHandle.IsInvalid;
		}

		public bool StartActivityNotificationClient()
		{
			if (this.krispDeviceHandle == null || this.krispDeviceHandle.IsInvalid)
			{
				throw new Exception("Fail to open krisp device");
			}
			if (this.worker == null || !this.worker.IsAlive)
			{
				this.stillRunning = true;
				this.worker = new Thread(delegate()
				{
					this._logger.LogDebug("KrispActivityNotifier thread started");
					try
					{
						uint num = (uint)Marshal.SizeOf(typeof(uint));
						uint num2 = (uint)Marshal.SizeOf(typeof(WaveFormatExtensible));
						for (int i = 0; i < this.ioArr.Length; i++)
						{
							int num3 = i % 2;
							IntPtr intPtr = Kernel32.CreateEvent(IntPtr.Zero, false, false, null);
							IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WaveFormatExtensible)));
							NativeOverlapped nativeOverlapped = new NativeOverlapped
							{
								EventHandle = intPtr
							};
							uint num4;
							Kernel32.DeviceIoControl(this.krispDeviceHandle.DangerousGetHandle(), KrispDevicePublic.IOCTL_KRISP_NOTIFY, num3, num, intPtr2, num2, out num4, ref nativeOverlapped);
							this.ioArr[i].dir = num3;
							this.ioArr[i].evn = intPtr;
							this.ioArr[i].wfe = intPtr2;
							this.ioArr[i].nol = nativeOverlapped;
						}
						IntPtr[] array = this.ioArr.Select((KrispActivityNotificationClient.DevIOItem e) => e.evn).ToArray<IntPtr>();
						while (this.stillRunning)
						{
							uint num5 = Kernel32.WaitForMultipleObjects((uint)array.Length, array, false, uint.MaxValue);
							if (!this.stillRunning)
							{
								break;
							}
							if (num5 < Kernel32.WAIT_TIMEOUT)
							{
								uint num6 = num5 - Kernel32.WAIT_OBJECT_0;
								EDataFlow dir = (EDataFlow)this.ioArr[(int)num6].dir;
								WaveFormatExtensible waveFormatExtensible = Marshal.PtrToStructure<WaveFormatExtensible>(this.ioArr[(int)num6].wfe);
								if ((uint)waveFormatExtensible.wFormatTag == KrispDevicePublic.WAVE_FORMAT_EVENT)
								{
									if ((uint)waveFormatExtensible.nChannels == KrispDevicePublic.NOTIFY_START)
									{
										this.NotifyStateChanged(StreamActivityState.StreamStarted, dir);
									}
									else if ((uint)waveFormatExtensible.nChannels == KrispDevicePublic.NOTIFY_DISCONNECT)
									{
										this.NotifyStateChanged(StreamActivityState.StreamClosed, dir);
									}
									else
									{
										this._logger.LogWarning("Got invalid activity state # ft: {0}, ch: {1}", new object[] { waveFormatExtensible.wFormatTag, waveFormatExtensible.nChannels });
									}
								}
								else
								{
									this.NotifyStateChanged(StreamActivityState.StreamOpened, dir);
								}
								NativeOverlapped nativeOverlapped2 = new NativeOverlapped
								{
									EventHandle = this.ioArr[(int)num6].evn
								};
								uint num4;
								Kernel32.DeviceIoControl(this.krispDeviceHandle.DangerousGetHandle(), KrispDevicePublic.IOCTL_KRISP_NOTIFY, this.ioArr[(int)num6].dir, num, this.ioArr[(int)num6].wfe, num2, out num4, ref nativeOverlapped2);
								this.ioArr[(int)num6].nol = nativeOverlapped2;
							}
						}
					}
					catch (Exception ex)
					{
						this._logger.LogError("Krisp activity watcher crashed: {0}", new object[] { ex.Message });
					}
					finally
					{
						this._logger.LogDebug("KrispActivityNotifier thread stopped");
					}
				});
				this.worker.Name = "kanclnt";
				this.worker.Start();
				return true;
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				this.SuspendWorkerThread();
			}
			if (this.ioArr != null)
			{
				foreach (KrispActivityNotificationClient.DevIOItem devIOItem in this.ioArr)
				{
					if (devIOItem.evn != IntPtr.Zero)
					{
						Kernel32.CloseHandle(devIOItem.evn);
					}
					if (devIOItem.wfe != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(devIOItem.wfe);
					}
				}
				this.ioArr = null;
			}
			this._disposed = true;
			base.Dispose(disposing);
		}

		private void SuspendWorkerThread()
		{
			this.stillRunning = false;
			KrispActivityNotificationClient.DevIOItem[] array = this.ioArr;
			if (array != null && array.Length != 0)
			{
				Kernel32.SetEvent(this.ioArr[0].evn);
			}
			if (this.worker != null && this.worker.IsAlive)
			{
				this.worker.Join(1000);
			}
			this.NotifyStateChanged(StreamActivityState.StreamClosed | StreamActivityState.StreamStoped, EDataFlow.eRender);
			this.NotifyStateChanged(StreamActivityState.StreamClosed | StreamActivityState.StreamStoped, EDataFlow.eCapture);
		}

		private void NotifyStateChanged(StreamActivityState state, EDataFlow dataFlow)
		{
			try
			{
				this._dispatcher.InvokeAsync(delegate()
				{
					try
					{
						if (dataFlow == EDataFlow.eCapture)
						{
							EventHandler<StreamActivityState> capturerStreamActivityChanged = this.CapturerStreamActivityChanged;
							if (capturerStreamActivityChanged != null)
							{
								capturerStreamActivityChanged(this, state);
							}
						}
						else
						{
							EventHandler<StreamActivityState> rendererStreamActivityChanged = this.RendererStreamActivityChanged;
							if (rendererStreamActivityChanged != null)
							{
								rendererStreamActivityChanged(this, state);
							}
						}
					}
					catch (Exception ex2)
					{
						this._logger.LogError("NotifyStateChangedAsync: Unable to notify Krisp {0} activity change ({1}): {2}", new object[] { dataFlow, state, ex2.Message });
					}
				}, DispatcherPriority.Send);
			}
			catch (Exception ex)
			{
				this._logger.LogError("NotifyChange: Unable to notify Krisp {0} activity change ({1}): {2}", new object[] { dataFlow, state, ex.Message });
			}
		}

		private static readonly int PENDING_COUNT = 16;

		private bool _disposed;

		private Thread worker;

		private bool stillRunning;

		private KrispActivityNotificationClient.DevIOItem[] ioArr = new KrispActivityNotificationClient.DevIOItem[KrispActivityNotificationClient.PENDING_COUNT];

		private Logger _logger = LogWrapper.GetLogger("KrispActivityNotifier");

		private Dispatcher _dispatcher;

		private struct DevIOItem
		{
			public NativeOverlapped nol { get; set; }

			public IntPtr evn { get; set; }

			public IntPtr wfe { get; set; }

			public int dir { get; set; }
		}
	}
}
