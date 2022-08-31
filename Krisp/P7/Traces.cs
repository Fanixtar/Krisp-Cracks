using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace P7
{
	public class Traces
	{
		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Create")]
		private static extern IntPtr P7_Trace_Create32(IntPtr i_hClient, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, IntPtr i_pOpt);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Create")]
		private static extern IntPtr P7_Trace_Create64(IntPtr i_hClient, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, IntPtr i_pOpt);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Get_Shared")]
		private static extern IntPtr P7_Trace_Get_Shared32([MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Get_Shared")]
		private static extern IntPtr P7_Trace_Get_Shared64([MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Share")]
		private static extern uint P7_Trace_Share32(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Share")]
		private static extern uint P7_Trace_Share64(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Set_Verbosity")]
		private static extern void P7_Trace_Set_Verbosity32(IntPtr i_hTrace, IntPtr i_hModule, uint i_dwVerbosity);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Set_Verbosity")]
		private static extern void P7_Trace_Set_Verbosity64(IntPtr i_hTrace, IntPtr i_hModule, uint i_dwVerbosity);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Register_Thread")]
		private static extern uint P7_Trace_Register_Thread32(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, uint i_dwThreadId);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Register_Thread")]
		private static extern uint P7_Trace_Register_Thread64(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, uint i_dwThreadId);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Unregister_Thread")]
		private static extern uint P7_Trace_Unregister_Thread32(IntPtr i_hTrace, uint i_dwThreadId);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Unregister_Thread")]
		private static extern uint P7_Trace_Unregister_Thread64(IntPtr i_hTrace, uint i_dwThreadId);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Register_Module")]
		private static extern IntPtr P7_Trace_Register_Module32(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Register_Module")]
		private static extern IntPtr P7_Trace_Register_Module64(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Managed")]
		private static extern uint P7_Trace_Managed32(IntPtr i_hTrace, ushort i_wTrace_ID, uint i_dwLevel, IntPtr i_hModule, ushort i_wLine, [MarshalAs(UnmanagedType.LPWStr)] string i_sFile, [MarshalAs(UnmanagedType.LPWStr)] string i_sFunction, [MarshalAs(UnmanagedType.LPWStr)] string i_sMessage);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Managed")]
		private static extern uint P7_Trace_Managed64(IntPtr i_hTrace, ushort i_wTrace_ID, uint i_dwLevel, IntPtr i_hModule, ushort i_wLine, [MarshalAs(UnmanagedType.LPWStr)] string i_sFile, [MarshalAs(UnmanagedType.LPWStr)] string i_sFunction, [MarshalAs(UnmanagedType.LPWStr)] string i_sMessage);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Add_Ref")]
		private static extern int P7_Trace_Add_Ref32(IntPtr i_hTrace);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Add_Ref")]
		private static extern int P7_Trace_Add_Ref64(IntPtr i_hTrace);

		[DllImport("P7x32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Release")]
		private static extern int P7_Trace_Release32(IntPtr i_hTrace);

		[DllImport("P7x64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "P7_Trace_Release")]
		private static extern int P7_Trace_Release64(IntPtr i_hTrace);

		public Traces(Client i_pClient, string i_sName)
		{
			this.Initialize();
			if (i_pClient == null && !(IntPtr.Zero != i_pClient.Handle))
			{
				throw new ArgumentException("Can't create P7 trace, input parameter (i_pClient) is wrong");
			}
			this.m_hHandle = this.P7_Trace_Create(i_pClient.Handle, i_sName, IntPtr.Zero);
			if (IntPtr.Zero == this.m_hHandle)
			{
				throw new ArgumentNullException("Can't create P7 trace, more than 32 streams are used ?");
			}
		}

		private Traces(IntPtr i_hHandle)
		{
			this.Initialize();
			this.m_hHandle = i_hHandle;
		}

		~Traces()
		{
			if (IntPtr.Zero != this.m_hHandle)
			{
				this.P7_Trace_Release(this.m_hHandle);
				this.m_hHandle = IntPtr.Zero;
			}
		}

		private void Initialize()
		{
			if (8 == IntPtr.Size)
			{
				this.P7_Trace_Create = new Traces.fnP7_Trace_Create(Traces.P7_Trace_Create64);
				this.P7_Trace_Get_Shared = new Traces.fnP7_Trace_Get_Shared(Traces.P7_Trace_Get_Shared64);
				this.P7_Trace_Share = new Traces.fnP7_Trace_Share(Traces.P7_Trace_Share64);
				this.P7_Trace_Set_Verbosity = new Traces.fnP7_Trace_Set_Verbosity(Traces.P7_Trace_Set_Verbosity64);
				this.P7_Trace_Register_Thread = new Traces.fnP7_Trace_Register_Thread(Traces.P7_Trace_Register_Thread64);
				this.P7_Trace_Unregister_Thread = new Traces.fnP7_Trace_Unregister_Thread(Traces.P7_Trace_Unregister_Thread64);
				this.P7_Trace_Register_Module = new Traces.fnP7_Trace_Register_Module(Traces.P7_Trace_Register_Module64);
				this.P7_Trace_Managed = new Traces.fnP7_Trace_Managed(Traces.P7_Trace_Managed64);
				this.P7_Trace_Add_Ref = new Traces.fnP7_Trace_Add_Ref(Traces.P7_Trace_Add_Ref64);
				this.P7_Trace_Release = new Traces.fnP7_Trace_Release(Traces.P7_Trace_Release64);
				return;
			}
			this.P7_Trace_Create = new Traces.fnP7_Trace_Create(Traces.P7_Trace_Create32);
			this.P7_Trace_Get_Shared = new Traces.fnP7_Trace_Get_Shared(Traces.P7_Trace_Get_Shared32);
			this.P7_Trace_Share = new Traces.fnP7_Trace_Share(Traces.P7_Trace_Share32);
			this.P7_Trace_Set_Verbosity = new Traces.fnP7_Trace_Set_Verbosity(Traces.P7_Trace_Set_Verbosity32);
			this.P7_Trace_Register_Thread = new Traces.fnP7_Trace_Register_Thread(Traces.P7_Trace_Register_Thread32);
			this.P7_Trace_Unregister_Thread = new Traces.fnP7_Trace_Unregister_Thread(Traces.P7_Trace_Unregister_Thread32);
			this.P7_Trace_Register_Module = new Traces.fnP7_Trace_Register_Module(Traces.P7_Trace_Register_Module32);
			this.P7_Trace_Managed = new Traces.fnP7_Trace_Managed(Traces.P7_Trace_Managed32);
			this.P7_Trace_Add_Ref = new Traces.fnP7_Trace_Add_Ref(Traces.P7_Trace_Add_Ref32);
			this.P7_Trace_Release = new Traces.fnP7_Trace_Release(Traces.P7_Trace_Release32);
		}

		public static Traces Get_Shared(string i_sName)
		{
			IntPtr intPtr = IntPtr.Zero;
			if (8 == IntPtr.Size)
			{
				intPtr = Traces.P7_Trace_Get_Shared64(i_sName);
			}
			else
			{
				intPtr = Traces.P7_Trace_Get_Shared32(i_sName);
			}
			if (IntPtr.Zero != intPtr)
			{
				return new Traces(intPtr);
			}
			return null;
		}

		public bool Share(string i_sName)
		{
			return this.P7_Trace_Share(this.m_hHandle, i_sName) != 0U;
		}

		public void Set_Verbosity(IntPtr i_hModule, Traces.Level i_eLevel)
		{
			this.P7_Trace_Set_Verbosity(this.m_hHandle, i_hModule, (uint)i_eLevel);
		}

		public bool Register_Thread(string i_sName, uint i_dwThreadID = 0U)
		{
			return this.P7_Trace_Register_Thread(this.m_hHandle, i_sName, i_dwThreadID) != 0U;
		}

		public bool Unregister_Thread(uint i_dwThreadID = 0U)
		{
			return this.P7_Trace_Unregister_Thread(this.m_hHandle, i_dwThreadID) != 0U;
		}

		public IntPtr Register_Module(string i_sName)
		{
			return this.P7_Trace_Register_Module(this.m_hHandle, i_sName);
		}

		public bool Add(ushort i_wTraceId, Traces.Level i_eLevel, IntPtr i_hModule, int i_dwStackFrame, string i_sMessage)
		{
			StackFrame frame = new StackTrace(true).GetFrame(i_dwStackFrame);
			int num = 0;
			string text = null;
			string text2 = null;
			if (frame != null)
			{
				MethodBase method = frame.GetMethod();
				if (null != method)
				{
					text = method.Name;
				}
				text2 = frame.GetFileName();
				num = frame.GetFileLineNumber();
			}
			return this.P7_Trace_Managed(this.m_hHandle, i_wTraceId, (uint)i_eLevel, i_hModule, (ushort)num, (text2 != null) ? text2 : "<optimized>", (text != null) ? text : "<optimized>", i_sMessage) != 0U;
		}

		public bool Trace(IntPtr i_hModule, string i_sMessage)
		{
			return this.Add(0, Traces.Level.TRACE, i_hModule, 3, i_sMessage);
		}

		public bool Debug(IntPtr i_hModule, string i_sMessage)
		{
			return this.Add(0, Traces.Level.DEBUG, i_hModule, 3, i_sMessage);
		}

		public bool Info(IntPtr i_hModule, string i_sMessage)
		{
			return this.Add(0, Traces.Level.INFO, i_hModule, 3, i_sMessage);
		}

		public bool Warning(IntPtr i_hModule, string i_sMessage)
		{
			return this.Add(0, Traces.Level.WARNING, i_hModule, 3, i_sMessage);
		}

		public bool Error(IntPtr i_hModule, string i_sMessage)
		{
			return this.Add(0, Traces.Level.ERROR, i_hModule, 3, i_sMessage);
		}

		public bool Critical(IntPtr i_hModule, string i_sMessage)
		{
			return this.Add(0, Traces.Level.CRITICAL, i_hModule, 3, i_sMessage);
		}

		public int AddRef()
		{
			return this.P7_Trace_Add_Ref(this.m_hHandle);
		}

		public int Release()
		{
			int num = this.P7_Trace_Release(this.m_hHandle);
			if (num == 0)
			{
				this.m_hHandle = IntPtr.Zero;
			}
			else if (num == 0)
			{
				Console.WriteLine("ERROR: P7 trace reference counter is damaged !");
				this.m_hHandle = IntPtr.Zero;
			}
			return num;
		}

		private IntPtr m_hHandle = IntPtr.Zero;

		private Traces.fnP7_Trace_Create P7_Trace_Create;

		private Traces.fnP7_Trace_Get_Shared P7_Trace_Get_Shared;

		private Traces.fnP7_Trace_Share P7_Trace_Share;

		private Traces.fnP7_Trace_Set_Verbosity P7_Trace_Set_Verbosity;

		private Traces.fnP7_Trace_Register_Thread P7_Trace_Register_Thread;

		private Traces.fnP7_Trace_Unregister_Thread P7_Trace_Unregister_Thread;

		private Traces.fnP7_Trace_Register_Module P7_Trace_Register_Module;

		private Traces.fnP7_Trace_Managed P7_Trace_Managed;

		private Traces.fnP7_Trace_Add_Ref P7_Trace_Add_Ref;

		private Traces.fnP7_Trace_Release P7_Trace_Release;

		private const int m_iStackFrame = 3;

		private delegate IntPtr fnP7_Trace_Create(IntPtr i_hClient, string i_sName, IntPtr i_pOpt);

		private delegate IntPtr fnP7_Trace_Get_Shared(string i_sName);

		private delegate uint fnP7_Trace_Share(IntPtr i_hTrace, string i_sName);

		private delegate void fnP7_Trace_Set_Verbosity(IntPtr i_hTrace, IntPtr i_hModule, uint i_dwVerbosity);

		private delegate uint fnP7_Trace_Register_Thread(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName, uint i_dwThreadId);

		private delegate uint fnP7_Trace_Unregister_Thread(IntPtr i_hTrace, uint i_dwThreadId);

		private delegate IntPtr fnP7_Trace_Register_Module(IntPtr i_hTrace, [MarshalAs(UnmanagedType.LPWStr)] string i_sName);

		private delegate uint fnP7_Trace_Managed(IntPtr i_hTrace, ushort i_wTrace_ID, uint i_dwLevel, IntPtr i_hModule, ushort i_wLine, string i_sFile, string i_sFunction, string i_sMessage);

		private delegate int fnP7_Trace_Add_Ref(IntPtr i_hTrace);

		private delegate int fnP7_Trace_Release(IntPtr i_hTrace);

		public enum Level
		{
			TRACE,
			DEBUG,
			INFO,
			WARNING,
			ERROR,
			CRITICAL,
			COUNT
		}
	}
}
