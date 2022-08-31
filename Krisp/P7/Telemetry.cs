using System;
using System.Runtime.InteropServices;

namespace P7
{
	public class Telemetry
	{
		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Create")]
		private static extern IntPtr P7_Telemetry_Create32(IntPtr i_hClient, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, IntPtr i_pConf);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Create")]
		private static extern IntPtr P7_Telemetry_Create64(IntPtr i_hClient, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, IntPtr i_pConf);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Get_Shared")]
		private static extern IntPtr P7_Telemetry_Get_Shared32([MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Get_Shared")]
		private static extern IntPtr P7_Telemetry_Get_Shared64([MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Share")]
		private static extern uint P7_Telemetry_Share32(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Share")]
		private static extern uint P7_Telemetry_Share64(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Create_Counter")]
		private static extern uint P7_Telemetry_Create_Counter32(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, double i_dbMin, double i_dbAlarmMin, double i_dbMax, double i_dbAlarmMax, int i_bOn, ref ushort o_rCounter_ID);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Create_Counter")]
		private static extern uint P7_Telemetry_Create_Counter64(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, double i_dbMin, double i_dbAlarmMin, double i_dbMax, double i_dbAlarmMax, int i_bOn, ref ushort o_rCounter_ID);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Put_Value")]
		private static extern uint P7_Telemetry_Put_Value32(IntPtr i_hTelemetry, ushort i_wCounter_ID, double i_dbValue);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Put_Value")]
		private static extern uint P7_Telemetry_Put_Value64(IntPtr i_hTelemetry, ushort i_wCounter_ID, double i_dbValue);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Put_Value")]
		private static extern uint P7_Telemetry_Find_Counter32(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, ref ushort o_rCounter_ID);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Put_Value")]
		private static extern uint P7_Telemetry_Find_Counter64(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, ref ushort o_rCounter_ID);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Add_Ref")]
		private static extern int P7_Telemetry_Add_Ref32(IntPtr i_hTelemetry);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Add_Ref")]
		private static extern int P7_Telemetry_Add_Ref64(IntPtr i_hTelemetry);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Release")]
		private static extern int P7_Telemetry_Release32(IntPtr i_hTelemetry);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Telemetry_Release")]
		private static extern int P7_Telemetry_Release64(IntPtr i_hTelemetry);

		public Telemetry(Client i_pClient, string i_sName)
		{
			this.Initialize();
			if (i_pClient == null && !(IntPtr.Zero != i_pClient.Handle))
			{
				throw new ArgumentException("Can't create P7 telemetry, input parameter (i_pClient) is wrong");
			}
			this.m_hHandle = this.P7_Telemetry_Create(i_pClient.Handle, i_sName, IntPtr.Zero);
			if (IntPtr.Zero == this.m_hHandle)
			{
				throw new ArgumentNullException("Can't create P7 telemetry, more than 32 streams are used ?");
			}
		}

		private Telemetry(IntPtr i_hHandle)
		{
			this.Initialize();
			this.m_hHandle = i_hHandle;
		}

		~Telemetry()
		{
			if (IntPtr.Zero != this.m_hHandle)
			{
				this.P7_Telemetry_Release(this.m_hHandle);
				this.m_hHandle = IntPtr.Zero;
			}
		}

		private void Initialize()
		{
			if (8 == IntPtr.Size)
			{
				this.P7_Telemetry_Create = new Telemetry.fnP7_Telemetry_Create(Telemetry.P7_Telemetry_Create64);
				this.P7_Telemetry_Get_Shared = new Telemetry.fnP7_Telemetry_Get_Shared(Telemetry.P7_Telemetry_Get_Shared64);
				this.P7_Telemetry_Share = new Telemetry.fnP7_Telemetry_Share(Telemetry.P7_Telemetry_Share64);
				this.P7_Telemetry_Create_Counter = new Telemetry.fnP7_Telemetry_Create_Counter(Telemetry.P7_Telemetry_Create_Counter64);
				this.P7_Telemetry_Put_Value = new Telemetry.fnP7_Telemetry_Put_Value(Telemetry.P7_Telemetry_Put_Value64);
				this.P7_Telemetry_Find_Counter = new Telemetry.fnP7_Telemetry_Find_Counter(Telemetry.P7_Telemetry_Find_Counter64);
				this.P7_Telemetry_Add_Ref = new Telemetry.fnP7_Telemetry_Add_Ref(Telemetry.P7_Telemetry_Add_Ref64);
				this.P7_Telemetry_Release = new Telemetry.fnP7_Telemetry_Release(Telemetry.P7_Telemetry_Release64);
				return;
			}
			this.P7_Telemetry_Create = new Telemetry.fnP7_Telemetry_Create(Telemetry.P7_Telemetry_Create32);
			this.P7_Telemetry_Get_Shared = new Telemetry.fnP7_Telemetry_Get_Shared(Telemetry.P7_Telemetry_Get_Shared32);
			this.P7_Telemetry_Share = new Telemetry.fnP7_Telemetry_Share(Telemetry.P7_Telemetry_Share32);
			this.P7_Telemetry_Create_Counter = new Telemetry.fnP7_Telemetry_Create_Counter(Telemetry.P7_Telemetry_Create_Counter32);
			this.P7_Telemetry_Put_Value = new Telemetry.fnP7_Telemetry_Put_Value(Telemetry.P7_Telemetry_Put_Value32);
			this.P7_Telemetry_Find_Counter = new Telemetry.fnP7_Telemetry_Find_Counter(Telemetry.P7_Telemetry_Find_Counter32);
			this.P7_Telemetry_Add_Ref = new Telemetry.fnP7_Telemetry_Add_Ref(Telemetry.P7_Telemetry_Add_Ref32);
			this.P7_Telemetry_Release = new Telemetry.fnP7_Telemetry_Release(Telemetry.P7_Telemetry_Release32);
		}

		public static Telemetry Get_Shared(string i_sName)
		{
			IntPtr intPtr = IntPtr.Zero;
			if (8 == IntPtr.Size)
			{
				intPtr = Telemetry.P7_Telemetry_Get_Shared64(i_sName);
			}
			else
			{
				intPtr = Telemetry.P7_Telemetry_Get_Shared32(i_sName);
			}
			if (IntPtr.Zero != intPtr)
			{
				return new Telemetry(intPtr);
			}
			return null;
		}

		public bool Share(string i_sName)
		{
			return this.P7_Telemetry_Share(this.m_hHandle, i_sName) != 0U;
		}

		public bool Create(string i_sName, double i_dbMin, double i_dbAlarmMin, double i_dbMax, double i_dbAlarmMax, int i_bOn, ref ushort o_rCounter_ID)
		{
			return this.P7_Telemetry_Create_Counter(this.m_hHandle, i_sName, i_dbMin, i_dbAlarmMin, i_dbMax, i_dbAlarmMax, i_bOn, ref o_rCounter_ID) != 0U;
		}

		public bool Add(ushort i_wCounter_ID, double i_dbValue)
		{
			return this.P7_Telemetry_Put_Value(this.m_hHandle, i_wCounter_ID, i_dbValue) != 0U;
		}

		public bool Find_Counter(string i_sName, ref ushort o_rCounter_ID)
		{
			return this.P7_Telemetry_Find_Counter(this.m_hHandle, i_sName, ref o_rCounter_ID) != 0U;
		}

		public int AddRef()
		{
			return this.P7_Telemetry_Add_Ref(this.m_hHandle);
		}

		public int Release()
		{
			int num = this.P7_Telemetry_Release(this.m_hHandle);
			if (num == 0)
			{
				this.m_hHandle = IntPtr.Zero;
			}
			else if (num == 0)
			{
				Console.WriteLine("ERROR: P7 telemetry reference counter is damaged !");
				this.m_hHandle = IntPtr.Zero;
			}
			return num;
		}

		private IntPtr m_hHandle = IntPtr.Zero;

		private Telemetry.fnP7_Telemetry_Create P7_Telemetry_Create;

		private Telemetry.fnP7_Telemetry_Get_Shared P7_Telemetry_Get_Shared;

		private Telemetry.fnP7_Telemetry_Share P7_Telemetry_Share;

		private Telemetry.fnP7_Telemetry_Create_Counter P7_Telemetry_Create_Counter;

		private Telemetry.fnP7_Telemetry_Put_Value P7_Telemetry_Put_Value;

		private Telemetry.fnP7_Telemetry_Find_Counter P7_Telemetry_Find_Counter;

		private Telemetry.fnP7_Telemetry_Add_Ref P7_Telemetry_Add_Ref;

		private Telemetry.fnP7_Telemetry_Release P7_Telemetry_Release;

		private delegate IntPtr fnP7_Telemetry_Create(IntPtr i_hClient, string i_sName, IntPtr i_pConf);

		private delegate IntPtr fnP7_Telemetry_Get_Shared(string i_sName);

		private delegate uint fnP7_Telemetry_Share(IntPtr i_hTelemetry, string i_sName);

		private delegate uint fnP7_Telemetry_Create_Counter(IntPtr i_hTelemetry, string i_sName, double i_dbMin, double i_dbAlarmMin, double i_dbMax, double i_dbAlarmMax, int i_bOn, ref ushort o_rCounter_ID);

		private delegate uint fnP7_Telemetry_Put_Value(IntPtr i_hTelemetry, ushort i_wCounter_ID, double i_dbValue);

		private delegate uint fnP7_Telemetry_Find_Counter(IntPtr i_hTelemetry, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, ref ushort o_rCounter_ID);

		private delegate int fnP7_Telemetry_Add_Ref(IntPtr i_hTelemetry);

		private delegate int fnP7_Telemetry_Release(IntPtr i_hTelemetry);
	}
}
