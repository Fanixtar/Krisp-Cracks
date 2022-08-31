using System;
using System.Runtime.InteropServices;

namespace P7
{
	public class Client
	{
		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Create")]
		private static extern IntPtr P7_Client_Create32([MarshalAs(UnmanagedType.LPWStr)] string i_sArgs);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Create")]
		private static extern IntPtr P7_Client_Create64([MarshalAs(UnmanagedType.LPWStr)] string i_sArgs);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Get_Shared")]
		private static extern IntPtr P7_Client_Get_Shared32([MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Get_Shared")]
		private static extern IntPtr P7_Client_Get_Shared64([MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Share")]
		private static extern uint P7_Client_Share32(IntPtr i_hClient, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Share")]
		private static extern uint P7_Client_Share64(IntPtr i_hClient, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Add_Ref")]
		private static extern int P7_Client_Add_Ref32(IntPtr i_hClient);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Add_Ref")]
		private static extern int P7_Client_Add_Ref64(IntPtr i_hClient);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Release")]
		private static extern int P7_Client_Release32(IntPtr i_hClient);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Client_Release")]
		private static extern int P7_Client_Release64(IntPtr i_hClient);

		public Client(string i_sArgs)
		{
			this.Initialize();
			this.m_hHandle = this.P7_Client_Create(i_sArgs);
			if (IntPtr.Zero == this.m_hHandle)
			{
				throw new ArgumentNullException("Can't create P7 client");
			}
		}

		private Client(IntPtr i_hHandle)
		{
			this.Initialize();
			this.m_hHandle = i_hHandle;
		}

		~Client()
		{
			if (IntPtr.Zero != this.m_hHandle)
			{
				this.P7_Client_Release(this.m_hHandle);
				this.m_hHandle = IntPtr.Zero;
			}
		}

		private void Initialize()
		{
			if (8 == IntPtr.Size)
			{
				this.P7_Client_Create = new Client.fnP7_Client_Create(Client.P7_Client_Create64);
				this.P7_Client_Get_Shared = new Client.fnP7_Client_Get_Shared(Client.P7_Client_Get_Shared64);
				this.P7_Client_Share = new Client.fnP7_Client_Share(Client.P7_Client_Share64);
				this.P7_Client_Add_Ref = new Client.fnP7_Client_Add_Ref(Client.P7_Client_Add_Ref64);
				this.P7_Client_Release = new Client.fnP7_Client_Release(Client.P7_Client_Release64);
				return;
			}
			this.P7_Client_Create = new Client.fnP7_Client_Create(Client.P7_Client_Create32);
			this.P7_Client_Get_Shared = new Client.fnP7_Client_Get_Shared(Client.P7_Client_Get_Shared32);
			this.P7_Client_Share = new Client.fnP7_Client_Share(Client.P7_Client_Share32);
			this.P7_Client_Add_Ref = new Client.fnP7_Client_Add_Ref(Client.P7_Client_Add_Ref32);
			this.P7_Client_Release = new Client.fnP7_Client_Release(Client.P7_Client_Release32);
		}

		public static Client Get_Shared(string i_sName)
		{
			IntPtr intPtr = IntPtr.Zero;
			if (8 == IntPtr.Size)
			{
				intPtr = Client.P7_Client_Get_Shared64(i_sName);
			}
			else
			{
				intPtr = Client.P7_Client_Get_Shared32(i_sName);
			}
			if (IntPtr.Zero != intPtr)
			{
				return new Client(intPtr);
			}
			return null;
		}

		public bool Share(string i_sName)
		{
			return this.P7_Client_Share(this.m_hHandle, i_sName) != 0U;
		}

		public IntPtr Handle
		{
			get
			{
				return this.m_hHandle;
			}
		}

		public int AddRef()
		{
			return this.P7_Client_Add_Ref(this.m_hHandle);
		}

		public int Release()
		{
			int num = this.P7_Client_Release(this.m_hHandle);
			if (num == 0)
			{
				this.m_hHandle = IntPtr.Zero;
			}
			else if (num == 0)
			{
				Console.WriteLine("ERROR: P7 Client reference counter is damaged !");
				this.m_hHandle = IntPtr.Zero;
			}
			return num;
		}

		private IntPtr m_hHandle = IntPtr.Zero;

		private Client.fnP7_Client_Create P7_Client_Create;

		private Client.fnP7_Client_Get_Shared P7_Client_Get_Shared;

		private Client.fnP7_Client_Share P7_Client_Share;

		private Client.fnP7_Client_Add_Ref P7_Client_Add_Ref;

		private Client.fnP7_Client_Release P7_Client_Release;

		private delegate IntPtr fnP7_Client_Create(string i_sArgs);

		private delegate IntPtr fnP7_Client_Get_Shared(string i_sName);

		private delegate uint fnP7_Client_Share(IntPtr i_hClient, string i_sName);

		private delegate int fnP7_Client_Add_Ref(IntPtr i_hClient);

		private delegate int fnP7_Client_Release(IntPtr i_hClient);
	}
}
