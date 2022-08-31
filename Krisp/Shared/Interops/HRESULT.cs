using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Shared.Interops
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct HRESULT : IComparable
	{
		public static Exception GetExceptionForHR(int hr)
		{
			return new HRESULT.HResultException(hr);
		}

		public HRESULT(int value)
		{
			this.m_value = value;
		}

		public static implicit operator int(HRESULT This)
		{
			return This.m_value;
		}

		public static implicit operator HRESULT(int This)
		{
			return new HRESULT(This);
		}

		public static implicit operator bool(HRESULT This)
		{
			if (This.m_value == 0)
			{
				return true;
			}
			if (This.m_value > 0)
			{
				return false;
			}
			throw HRESULT.GetExceptionForHR(This.m_value);
		}

		public static bool operator true(HRESULT This)
		{
			return This;
		}

		public static bool operator false(HRESULT This)
		{
			return !This;
		}

		public bool Equals(HRESULT that)
		{
			return this.m_value == that.m_value;
		}

		public bool Equals(int that)
		{
			return this.m_value == that;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is HRESULT)
			{
				return this.Equals((HRESULT)obj);
			}
			return obj is int && this.Equals((int)obj);
		}

		public override int GetHashCode()
		{
			return this.m_value;
		}

		public override string ToString()
		{
			FieldInfo fieldInfo;
			if (!HRESULT.dirCodes.TryGetValue(this.m_value, out fieldInfo))
			{
				return this.m_value.ToString();
			}
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(HRESULT.DescriptionAttribute), false);
			HRESULT.DescriptionAttribute descriptionAttribute = null;
			if (customAttributes != null && customAttributes.Length != 0)
			{
				descriptionAttribute = (HRESULT.DescriptionAttribute)customAttributes[0];
			}
			if (descriptionAttribute == null)
			{
				return fieldInfo.Name;
			}
			return fieldInfo.Name + ": " + descriptionAttribute.description;
		}

		public bool Failed
		{
			get
			{
				return this.m_value < 0;
			}
		}

		public bool Succeeded
		{
			get
			{
				return this.m_value >= 0;
			}
		}

		public static bool FAILED(int hr)
		{
			return hr < 0;
		}

		public static bool SUCCEEDED(int hr)
		{
			return hr >= 0;
		}

		public int CompareTo(HRESULT that)
		{
			if (this.m_value < that.m_value)
			{
				return -1;
			}
			if (this.m_value <= that.m_value)
			{
				return 0;
			}
			return 1;
		}

		public int CompareTo(int that)
		{
			if (this.m_value < that)
			{
				return -1;
			}
			if (this.m_value <= that)
			{
				return 0;
			}
			return 1;
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}
			if (obj is HRESULT)
			{
				return this.CompareTo((HRESULT)obj);
			}
			if (obj is int)
			{
				return this.CompareTo((int)obj);
			}
			throw new ArgumentException("Arg_MustBeHRESULT");
		}

		static HRESULT()
		{
			foreach (FieldInfo fieldInfo in typeof(HRESULT).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				if (fieldInfo.GetValue(null).GetType() == typeof(int))
				{
					int num = (int)fieldInfo.GetValue(null);
					if (!HRESULT.dirCodes.ContainsKey(num))
					{
						HRESULT.dirCodes[num] = fieldInfo;
					}
				}
			}
		}

		private int m_value;

		[HRESULT.DescriptionAttribute("Success code")]
		public const int S_OK = 0;

		[HRESULT.DescriptionAttribute("Success code")]
		public const int NO_ERROR = 0;

		[HRESULT.DescriptionAttribute("Success code")]
		public const int NOERROR = 0;

		[HRESULT.DescriptionAttribute("Success code false")]
		public const int S_FALSE = 1;

		[HRESULT.DescriptionAttribute("AudClient. The Audio device invalidated")]
		public const int AUDCLNT_E_DEVICE_INVALIDATED = -2004287484;

		[HRESULT.DescriptionAttribute("AudClient. No single process")]
		public const int AUDCLNT_S_NO_SINGLE_PROCESS = 143196173;

		[HRESULT.DescriptionAttribute("Error, not found")]
		public const int ERROR_NOT_FOUND = -2147023728;

		[HRESULT.DescriptionAttribute("Catastrophic failure")]
		public const int E_UNEXPECTED = -2147418113;

		[HRESULT.DescriptionAttribute("Not implemented")]
		public const int E_NOTIMPL = -2147467263;

		[HRESULT.DescriptionAttribute("Ran out of memory")]
		public const int E_OUTOFMEMORY = -2147024882;

		[HRESULT.DescriptionAttribute("One or more arguments are invalid")]
		public const int E_INVALIDARG = -2147024809;

		[HRESULT.DescriptionAttribute("No such interface supported")]
		public const int E_NOINTERFACE = -2147467262;

		[HRESULT.DescriptionAttribute("Invalid pointer")]
		public const int E_POINTER = -2147467261;

		[HRESULT.DescriptionAttribute("Invalid handle")]
		public const int E_HANDLE = -2147024890;

		[HRESULT.DescriptionAttribute("Operation aborted")]
		public const int E_ABORT = -2147467260;

		[HRESULT.DescriptionAttribute("Unspecified error")]
		public const int E_FAIL = -2147467259;

		[HRESULT.DescriptionAttribute("General access denied error")]
		public const int E_ACCESSDENIED = -2147024891;

		[HRESULT.DescriptionAttribute("The data necessary to complete this operation is not yet available.")]
		public const int E_PENDING = -2147483638;

		[HRESULT.DescriptionAttribute("Thread local storage failure")]
		public const int CO_E_INIT_TLS = -2147467258;

		[HRESULT.DescriptionAttribute("Get shared memory allocator failure")]
		public const int CO_E_INIT_SHARED_ALLOCATOR = -2147467257;

		[HRESULT.DescriptionAttribute("Get memory allocator failure")]
		public const int CO_E_INIT_MEMORY_ALLOCATOR = -2147467256;

		[HRESULT.DescriptionAttribute("Unable to initialize class cache")]
		public const int CO_E_INIT_CLASS_CACHE = -2147467255;

		[HRESULT.DescriptionAttribute("Unable to initialize RPC services")]
		public const int CO_E_INIT_RPC_CHANNEL = -2147467254;

		[HRESULT.DescriptionAttribute("Cannot set thread local storage channel control")]
		public const int CO_E_INIT_TLS_SET_CHANNEL_CONTROL = -2147467253;

		[HRESULT.DescriptionAttribute("Could not allocate thread local storage channel control")]
		public const int CO_E_INIT_TLS_CHANNEL_CONTROL = -2147467252;

		[HRESULT.DescriptionAttribute("The user supplied memory allocator is unacceptable")]
		public const int CO_E_INIT_UNACCEPTED_USER_ALLOCATOR = -2147467251;

		[HRESULT.DescriptionAttribute("The OLE service mutex already exists")]
		public const int CO_E_INIT_SCM_MUTEX_EXISTS = -2147467250;

		[HRESULT.DescriptionAttribute("The OLE service file mapping already exists")]
		public const int CO_E_INIT_SCM_FILE_MAPPING_EXISTS = -2147467249;

		[HRESULT.DescriptionAttribute("Unable to map view of file for OLE service")]
		public const int CO_E_INIT_SCM_MAP_VIEW_OF_FILE = -2147467248;

		[HRESULT.DescriptionAttribute("Failure attempting to launch OLE service")]
		public const int CO_E_INIT_SCM_EXEC_FAILURE = -2147467247;

		[HRESULT.DescriptionAttribute("There was an attempt to call CoInitialize a second time while single threaded")]
		public const int CO_E_INIT_ONLY_SINGLE_THREADED = -2147467246;

		[HRESULT.DescriptionAttribute("A Remote activation was necessary but was not allowed")]
		public const int CO_E_CANT_REMOTE = -2147467245;

		[HRESULT.DescriptionAttribute("A Remote activation was necessary but the server name provided was invalid")]
		public const int CO_E_BAD_SERVER_NAME = -2147467244;

		[HRESULT.DescriptionAttribute("The class is configured to run as a security id different from the caller")]
		public const int CO_E_WRONG_SERVER_IDENTITY = -2147467243;

		[HRESULT.DescriptionAttribute("Use of Ole1 services requiring DDE windows is disabled")]
		public const int CO_E_OLE1DDE_DISABLED = -2147467242;

		[HRESULT.DescriptionAttribute("A RunAs specification must be <domain name>\\<user name> or simply <user name>")]
		public const int CO_E_RUNAS_SYNTAX = -2147467241;

		[HRESULT.DescriptionAttribute("The server process could not be started.  The pathname may be incorrect.")]
		public const int CO_E_CREATEPROCESS_FAILURE = -2147467240;

		[HRESULT.DescriptionAttribute("The server process could not be started as the configured identity.\nThe pathname may be incorrect or unavailable.")]
		public const int CO_E_RUNAS_CREATEPROCESS_FAILURE = -2147467239;

		[HRESULT.DescriptionAttribute("The server process could not be started because the configured identity is incorrect.\nCheck the username and password.")]
		public const int CO_E_RUNAS_LOGON_FAILURE = -2147467238;

		[HRESULT.DescriptionAttribute("The client is not allowed to launch this server.")]
		public const int CO_E_LAUNCH_PERMSSION_DENIED = -2147467237;

		[HRESULT.DescriptionAttribute("The service providing this server could not be started.")]
		public const int CO_E_START_SERVICE_FAILURE = -2147467236;

		[HRESULT.DescriptionAttribute("This computer was unable to communicate with the computer providing the server.")]
		public const int CO_E_REMOTE_COMMUNICATION_FAILURE = -2147467235;

		[HRESULT.DescriptionAttribute("The server did not respond after being launched.")]
		public const int CO_E_SERVER_START_TIMEOUT = -2147467234;

		[HRESULT.DescriptionAttribute("The registration information for this server is inconsistent or incomplete.")]
		public const int CO_E_CLSREG_INCONSISTENT = -2147467233;

		[HRESULT.DescriptionAttribute("The registration information for this interface is inconsistent or incomplete.")]
		public const int CO_E_IIDREG_INCONSISTENT = -2147467232;

		[HRESULT.DescriptionAttribute("The operation attempted is not supported.")]
		public const int CO_E_NOT_SUPPORTED = -2147467231;

		[HRESULT.DescriptionAttribute("A dll must be loaded.")]
		public const int CO_E_RELOAD_DLL = -2147467230;

		[HRESULT.DescriptionAttribute("A Microsoft Software Installer error was encountered.")]
		public const int CO_E_MSI_ERROR = -2147467229;

		[HRESULT.DescriptionAttribute("The specified activation could not occur in the client context as specified.")]
		public const int CO_E_ATTEMPT_TO_CREATE_OUTSIDE_CLIENT_CONTEXT = -2147467228;

		[HRESULT.DescriptionAttribute("Activations on the server are paused.")]
		public const int CO_E_SERVER_PAUSED = -2147467227;

		[HRESULT.DescriptionAttribute("Activations on the server are not paused.")]
		public const int CO_E_SERVER_NOT_PAUSED = -2147467226;

		[HRESULT.DescriptionAttribute("The component or application containing the component has been disabled.")]
		public const int CO_E_CLASS_DISABLED = -2147467225;

		[HRESULT.DescriptionAttribute("The common language runtime is not available")]
		public const int CO_E_CLRNOTAVAILABLE = -2147467224;

		[HRESULT.DescriptionAttribute("The thread-pool rejected the submitted asynchronous work.")]
		public const int CO_E_ASYNC_WORK_REJECTED = -2147467223;

		[HRESULT.DescriptionAttribute("The server started, but did not finish initializing in a timely fashion.")]
		public const int CO_E_SERVER_INIT_TIMEOUT = -2147467222;

		[HRESULT.DescriptionAttribute("Unable to complete the call since there is no COM+ security context inside IObjectControl.Activate.")]
		public const int CO_E_NO_SECCTX_IN_ACTIVATE = -2147467221;

		[HRESULT.DescriptionAttribute("The provided tracker configuration is invalid")]
		public const int CO_E_TRACKER_CONFIG = -2147467216;

		[HRESULT.DescriptionAttribute("The provided thread pool configuration is invalid")]
		public const int CO_E_THREADPOOL_CONFIG = -2147467215;

		[HRESULT.DescriptionAttribute("The provided side-by-side configuration is invalid")]
		public const int CO_E_SXS_CONFIG = -2147467214;

		[HRESULT.DescriptionAttribute("The server principal name (SPN) obtained during security negotiation is malformed.")]
		public const int CO_E_MALFORMED_SPN = -2147467213;

		[HRESULT.DescriptionAttribute("Call was rejected by callee.")]
		public const int RPC_E_CALL_REJECTED = -2147418111;

		[HRESULT.DescriptionAttribute("Call was canceled by the message filter.")]
		public const int RPC_E_CALL_CANCELED = -2147418110;

		[HRESULT.DescriptionAttribute("The caller is dispatching an intertask SendMessage call and cannot call out via PostMessage.")]
		public const int RPC_E_CANTPOST_INSENDCALL = -2147418109;

		[HRESULT.DescriptionAttribute("The caller is dispatching an asynchronous call and cannot make an outgoing call on behalf of this call.")]
		public const int RPC_E_CANTCALLOUT_INASYNCCALL = -2147418108;

		[HRESULT.DescriptionAttribute("It is illegal to call out while inside message filter.")]
		public const int RPC_E_CANTCALLOUT_INEXTERNALCALL = -2147418107;

		[HRESULT.DescriptionAttribute("The connection terminated or is in a bogus state and cannot be used any more. Other connections are still valid.")]
		public const int RPC_E_CONNECTION_TERMINATED = -2147418106;

		[HRESULT.DescriptionAttribute("The callee (server [not server application]) is not available and disappeared; all connections are invalid. The call may have executed.")]
		public const int RPC_E_SERVER_DIED = -2147418105;

		[HRESULT.DescriptionAttribute("The caller (client) disappeared while the callee (server) was processing a call.")]
		public const int RPC_E_CLIENT_DIED = -2147418104;

		[HRESULT.DescriptionAttribute("The data packet with the marshalled parameter data is incorrect.")]
		public const int RPC_E_INVALID_DATAPACKET = -2147418103;

		[HRESULT.DescriptionAttribute("The call was not transmitted properly; the message queue was full and was not emptied after yielding.")]
		public const int RPC_E_CANTTRANSMIT_CALL = -2147418102;

		[HRESULT.DescriptionAttribute("The client (caller) cannot marshall the parameter data - low memory, etc.")]
		public const int RPC_E_CLIENT_CANTMARSHAL_DATA = -2147418101;

		[HRESULT.DescriptionAttribute("The client (caller) cannot unmarshall the return data - low memory, etc.")]
		public const int RPC_E_CLIENT_CANTUNMARSHAL_DATA = -2147418100;

		[HRESULT.DescriptionAttribute("The server (callee) cannot marshall the return data - low memory, etc.")]
		public const int RPC_E_SERVER_CANTMARSHAL_DATA = -2147418099;

		[HRESULT.DescriptionAttribute("The server (callee) cannot unmarshall the parameter data - low memory, etc.")]
		public const int RPC_E_SERVER_CANTUNMARSHAL_DATA = -2147418098;

		[HRESULT.DescriptionAttribute("Received data is invalid; could be server or client data.")]
		public const int RPC_E_INVALID_DATA = -2147418097;

		[HRESULT.DescriptionAttribute("A particular parameter is invalid and cannot be (un)marshalled.")]
		public const int RPC_E_INVALID_PARAMETER = -2147418096;

		[HRESULT.DescriptionAttribute("There is no second outgoing call on same channel in DDE conversation.")]
		public const int RPC_E_CANTCALLOUT_AGAIN = -2147418095;

		[HRESULT.DescriptionAttribute("The callee (server [not server application]) is not available and disappeared; all connections are invalid. The call did not execute.")]
		public const int RPC_E_SERVER_DIED_DNE = -2147418094;

		[HRESULT.DescriptionAttribute("System call failed.")]
		public const int RPC_E_SYS_CALL_FAILED = -2147417856;

		[HRESULT.DescriptionAttribute("Could not allocate some required resource (memory, events, ...)")]
		public const int RPC_E_OUT_OF_RESOURCES = -2147417855;

		[HRESULT.DescriptionAttribute("Attempted to make calls on more than one thread in single threaded mode.")]
		public const int RPC_E_ATTEMPTED_MULTITHREAD = -2147417854;

		[HRESULT.DescriptionAttribute("The requested interface is not registered on the server object.")]
		public const int RPC_E_NOT_REGISTERED = -2147417853;

		[HRESULT.DescriptionAttribute("RPC could not call the server or could not return the results of calling the server.")]
		public const int RPC_E_FAULT = -2147417852;

		[HRESULT.DescriptionAttribute("The server threw an exception.")]
		public const int RPC_E_SERVERFAULT = -2147417851;

		[HRESULT.DescriptionAttribute("Cannot change thread mode after it is set.")]
		public const int RPC_E_CHANGED_MODE = -2147417850;

		[HRESULT.DescriptionAttribute("The method called does not exist on the server.")]
		public const int RPC_E_INVALIDMETHOD = -2147417849;

		[HRESULT.DescriptionAttribute("The object invoked has disconnected from its clients.")]
		public const int RPC_E_DISCONNECTED = -2147417848;

		[HRESULT.DescriptionAttribute("The object invoked chose not to process the call now.  Try again later.")]
		public const int RPC_E_RETRY = -2147417847;

		[HRESULT.DescriptionAttribute("The message filter indicated that the application is busy.")]
		public const int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

		[HRESULT.DescriptionAttribute("The message filter rejected the call.")]
		public const int RPC_E_SERVERCALL_REJECTED = -2147417845;

		[HRESULT.DescriptionAttribute("A call control interfaces was called with invalid data.")]
		public const int RPC_E_INVALID_CALLDATA = -2147417844;

		[HRESULT.DescriptionAttribute("An outgoing call cannot be made since the application is dispatching an input-synchronous call.")]
		public const int RPC_E_CANTCALLOUT_ININPUTSYNCCALL = -2147417843;

		[HRESULT.DescriptionAttribute("The application called an interface that was marshalled for a different thread.")]
		public const int RPC_E_WRONG_THREAD = -2147417842;

		[HRESULT.DescriptionAttribute("CoInitialize has not been called on the current thread.")]
		public const int RPC_E_THREAD_NOT_INIT = -2147417841;

		[HRESULT.DescriptionAttribute("The version of OLE on the client and server machines does not match.")]
		public const int RPC_E_VERSION_MISMATCH = -2147417840;

		[HRESULT.DescriptionAttribute("OLE received a packet with an invalid header.")]
		public const int RPC_E_INVALID_HEADER = -2147417839;

		[HRESULT.DescriptionAttribute("OLE received a packet with an invalid extension.")]
		public const int RPC_E_INVALID_EXTENSION = -2147417838;

		[HRESULT.DescriptionAttribute("The requested object or interface does not exist.")]
		public const int RPC_E_INVALID_IPID = -2147417837;

		[HRESULT.DescriptionAttribute("The requested object does not exist.")]
		public const int RPC_E_INVALID_OBJECT = -2147417836;

		[HRESULT.DescriptionAttribute("OLE has sent a request and is waiting for a reply.")]
		public const int RPC_S_CALLPENDING = -2147417835;

		[HRESULT.DescriptionAttribute("OLE is waiting before retrying a request.")]
		public const int RPC_S_WAITONTIMER = -2147417834;

		[HRESULT.DescriptionAttribute("Call context cannot be accessed after call completed.")]
		public const int RPC_E_CALL_COMPLETE = -2147417833;

		[HRESULT.DescriptionAttribute("Impersonate on unsecure calls is not supported.")]
		public const int RPC_E_UNSECURE_CALL = -2147417832;

		[HRESULT.DescriptionAttribute("Security must be initialized before any interfaces are marshalled or unmarshalled. It cannot be changed once initialized.")]
		public const int RPC_E_TOO_LATE = -2147417831;

		[HRESULT.DescriptionAttribute("No security packages are installed on this machine or the user is not logged on or there are no compatible security packages between the client and server.")]
		public const int RPC_E_NO_GOOD_SECURITY_PACKAGES = -2147417830;

		[HRESULT.DescriptionAttribute("Access is denied.")]
		public const int RPC_E_ACCESS_DENIED = -2147417829;

		[HRESULT.DescriptionAttribute("Remote calls are not allowed for this process.")]
		public const int RPC_E_REMOTE_DISABLED = -2147417828;

		[HRESULT.DescriptionAttribute("The marshaled interface data packet (OBJREF) has an invalid or unknown format.")]
		public const int RPC_E_INVALID_OBJREF = -2147417827;

		[HRESULT.DescriptionAttribute("No context is associated with this call. This happens for some custom marshalled calls and on the client side of the call.")]
		public const int RPC_E_NO_CONTEXT = -2147417826;

		[HRESULT.DescriptionAttribute("This operation returned because the timeout period expired.")]
		public const int RPC_E_TIMEOUT = -2147417825;

		[HRESULT.DescriptionAttribute("There are no synchronize objects to wait on.")]
		public const int RPC_E_NO_SYNC = -2147417824;

		[HRESULT.DescriptionAttribute("Full subject issuer chain SSL principal name expected from the server.")]
		public const int RPC_E_FULLSIC_REQUIRED = -2147417823;

		[HRESULT.DescriptionAttribute("Principal name is not a valid MSSTD name.")]
		public const int RPC_E_INVALID_STD_NAME = -2147417822;

		[HRESULT.DescriptionAttribute("Unable to impersonate DCOM client")]
		public const int CO_E_FAILEDTOIMPERSONATE = -2147417821;

		[HRESULT.DescriptionAttribute("Unable to obtain server's security context")]
		public const int CO_E_FAILEDTOGETSECCTX = -2147417820;

		[HRESULT.DescriptionAttribute("Unable to open the access token of the current thread")]
		public const int CO_E_FAILEDTOOPENTHREADTOKEN = -2147417819;

		[HRESULT.DescriptionAttribute("Unable to obtain user info from an access token")]
		public const int CO_E_FAILEDTOGETTOKENINFO = -2147417818;

		[HRESULT.DescriptionAttribute("The client who called IAccessControl::IsAccessPermitted was not the trustee provided to the method")]
		public const int CO_E_TRUSTEEDOESNTMATCHCLIENT = -2147417817;

		[HRESULT.DescriptionAttribute("Unable to obtain the client's security blanket")]
		public const int CO_E_FAILEDTOQUERYCLIENTBLANKET = -2147417816;

		[HRESULT.DescriptionAttribute("Unable to set a discretionary ACL into a security descriptor")]
		public const int CO_E_FAILEDTOSETDACL = -2147417815;

		[HRESULT.DescriptionAttribute("The system function, AccessCheck, returned false")]
		public const int CO_E_ACCESSCHECKFAILED = -2147417814;

		[HRESULT.DescriptionAttribute("Either NetAccessDel or NetAccessAdd returned an error code.")]
		public const int CO_E_NETACCESSAPIFAILED = -2147417813;

		[HRESULT.DescriptionAttribute("One of the trustee strings provided by the user did not conform to the <Domain>\\<Name> syntax and it was not the \"*\" string")]
		public const int CO_E_WRONGTRUSTEENAMESYNTAX = -2147417812;

		[HRESULT.DescriptionAttribute("One of the security identifiers provided by the user was invalid")]
		public const int CO_E_INVALIDSID = -2147417811;

		[HRESULT.DescriptionAttribute("Unable to convert a wide character trustee string to a multibyte trustee string")]
		public const int CO_E_CONVERSIONFAILED = -2147417810;

		[HRESULT.DescriptionAttribute("Unable to find a security identifier that corresponds to a trustee string provided by the user")]
		public const int CO_E_NOMATCHINGSIDFOUND = -2147417809;

		[HRESULT.DescriptionAttribute("The system function, LookupAccountSID, failed")]
		public const int CO_E_LOOKUPACCSIDFAILED = -2147417808;

		[HRESULT.DescriptionAttribute("Unable to find a trustee name that corresponds to a security identifier provided by the user")]
		public const int CO_E_NOMATCHINGNAMEFOUND = -2147417807;

		[HRESULT.DescriptionAttribute("The system function, LookupAccountName, failed")]
		public const int CO_E_LOOKUPACCNAMEFAILED = -2147417806;

		[HRESULT.DescriptionAttribute("Unable to set or reset a serialization handle")]
		public const int CO_E_SETSERLHNDLFAILED = -2147417805;

		[HRESULT.DescriptionAttribute("Unable to obtain the Windows directory")]
		public const int CO_E_FAILEDTOGETWINDIR = -2147417804;

		[HRESULT.DescriptionAttribute("Path too long")]
		public const int CO_E_PATHTOOLONG = -2147417803;

		[HRESULT.DescriptionAttribute("Unable to generate a uuid.")]
		public const int CO_E_FAILEDTOGENUUID = -2147417802;

		[HRESULT.DescriptionAttribute("Unable to create file")]
		public const int CO_E_FAILEDTOCREATEFILE = -2147417801;

		[HRESULT.DescriptionAttribute("Unable to close a serialization handle or a file handle.")]
		public const int CO_E_FAILEDTOCLOSEHANDLE = -2147417800;

		[HRESULT.DescriptionAttribute("The number of ACEs in an ACL exceeds the system limit.")]
		public const int CO_E_EXCEEDSYSACLLIMIT = -2147417799;

		[HRESULT.DescriptionAttribute("Not all the DENY_ACCESS ACEs are arranged in front of the GRANT_ACCESS ACEs in the stream.")]
		public const int CO_E_ACESINWRONGORDER = -2147417798;

		[HRESULT.DescriptionAttribute("The version of ACL format in the stream is not supported by this implementation of IAccessControl")]
		public const int CO_E_INCOMPATIBLESTREAMVERSION = -2147417797;

		[HRESULT.DescriptionAttribute("Unable to open the access token of the server process")]
		public const int CO_E_FAILEDTOOPENPROCESSTOKEN = -2147417796;

		[HRESULT.DescriptionAttribute("Unable to decode the ACL in the stream provided by the user")]
		public const int CO_E_DECODEFAILED = -2147417795;

		[HRESULT.DescriptionAttribute("The COM IAccessControl object is not initialized")]
		public const int CO_E_ACNOTINITIALIZED = -2147417793;

		[HRESULT.DescriptionAttribute("Call Cancellation is disabled")]
		public const int CO_E_CANCEL_DISABLED = -2147417792;

		[HRESULT.DescriptionAttribute("An internal error occurred.")]
		public const int RPC_E_UNEXPECTED = -2147352577;

		[HRESULT.DescriptionAttribute("Unknown interface.")]
		public const int DISP_E_UNKNOWNINTERFACE = -2147352575;

		[HRESULT.DescriptionAttribute("Member not found.")]
		public const int DISP_E_MEMBERNOTFOUND = -2147352573;

		[HRESULT.DescriptionAttribute("Parameter not found.")]
		public const int DISP_E_PARAMNOTFOUND = -2147352572;

		[HRESULT.DescriptionAttribute("Type mismatch.")]
		public const int DISP_E_TYPEMISMATCH = -2147352571;

		[HRESULT.DescriptionAttribute("Unknown name.")]
		public const int DISP_E_UNKNOWNNAME = -2147352570;

		[HRESULT.DescriptionAttribute("No named arguments.")]
		public const int DISP_E_NONAMEDARGS = -2147352569;

		[HRESULT.DescriptionAttribute("Bad variable type.")]
		public const int DISP_E_BADVARTYPE = -2147352568;

		[HRESULT.DescriptionAttribute("Exception occurred.")]
		public const int DISP_E_EXCEPTION = -2147352567;

		[HRESULT.DescriptionAttribute("Out of present range.")]
		public const int DISP_E_OVERFLOW = -2147352566;

		[HRESULT.DescriptionAttribute("Invalid index.")]
		public const int DISP_E_BADINDEX = -2147352565;

		[HRESULT.DescriptionAttribute("Unknown language.")]
		public const int DISP_E_UNKNOWNLCID = -2147352564;

		[HRESULT.DescriptionAttribute("Memory is locked.")]
		public const int DISP_E_ARRAYISLOCKED = -2147352563;

		[HRESULT.DescriptionAttribute("Invalid number of parameters.")]
		public const int DISP_E_BADPARAMCOUNT = -2147352562;

		[HRESULT.DescriptionAttribute("Parameter not optional.")]
		public const int DISP_E_PARAMNOTOPTIONAL = -2147352561;

		[HRESULT.DescriptionAttribute("Invalid callee.")]
		public const int DISP_E_BADCALLEE = -2147352560;

		[HRESULT.DescriptionAttribute("Does not support a collection.")]
		public const int DISP_E_NOTACOLLECTION = -2147352559;

		[HRESULT.DescriptionAttribute("Division by zero.")]
		public const int DISP_E_DIVBYZERO = -2147352558;

		[HRESULT.DescriptionAttribute("Buffer too small")]
		public const int DISP_E_BUFFERTOOSMALL = -2147352557;

		[HRESULT.DescriptionAttribute("Buffer too small.")]
		public const int TYPE_E_BUFFERTOOSMALL = -2147319786;

		[HRESULT.DescriptionAttribute("Field name not defined in the record.")]
		public const int TYPE_E_FIELDNOTFOUND = -2147319785;

		[HRESULT.DescriptionAttribute("Old format or invalid type library.")]
		public const int TYPE_E_INVDATAREAD = -2147319784;

		[HRESULT.DescriptionAttribute("Old format or invalid type library.")]
		public const int TYPE_E_UNSUPFORMAT = -2147319783;

		[HRESULT.DescriptionAttribute("Error accessing the OLE registry.")]
		public const int TYPE_E_REGISTRYACCESS = -2147319780;

		[HRESULT.DescriptionAttribute("Library not registered.")]
		public const int TYPE_E_LIBNOTREGISTERED = -2147319779;

		[HRESULT.DescriptionAttribute("Bound to unknown type.")]
		public const int TYPE_E_UNDEFINEDTYPE = -2147319769;

		[HRESULT.DescriptionAttribute("Qualified name disallowed.")]
		public const int TYPE_E_QUALIFIEDNAMEDISALLOWED = -2147319768;

		[HRESULT.DescriptionAttribute("Invalid forward reference, or reference to uncompiled type.")]
		public const int TYPE_E_INVALIDSTATE = -2147319767;

		[HRESULT.DescriptionAttribute("Type mismatch.")]
		public const int TYPE_E_WRONGTYPEKIND = -2147319766;

		[HRESULT.DescriptionAttribute("Element not found.")]
		public const int TYPE_E_ELEMENTNOTFOUND = -2147319765;

		[HRESULT.DescriptionAttribute("Ambiguous name.")]
		public const int TYPE_E_AMBIGUOUSNAME = -2147319764;

		[HRESULT.DescriptionAttribute("Name already exists in the library.")]
		public const int TYPE_E_NAMECONFLICT = -2147319763;

		[HRESULT.DescriptionAttribute("Unknown LCID.")]
		public const int TYPE_E_UNKNOWNLCID = -2147319762;

		[HRESULT.DescriptionAttribute("Function not defined in specified DLL.")]
		public const int TYPE_E_DLLFUNCTIONNOTFOUND = -2147319761;

		[HRESULT.DescriptionAttribute("Wrong module kind for the operation.")]
		public const int TYPE_E_BADMODULEKIND = -2147317571;

		[HRESULT.DescriptionAttribute("Size may not exceed 64K.")]
		public const int TYPE_E_SIZETOOBIG = -2147317563;

		[HRESULT.DescriptionAttribute("Duplicate ID in inheritance hierarchy.")]
		public const int TYPE_E_DUPLICATEID = -2147317562;

		[HRESULT.DescriptionAttribute("Incorrect inheritance depth in standard OLE hmember.")]
		public const int TYPE_E_INVALIDID = -2147317553;

		[HRESULT.DescriptionAttribute("Type mismatch.")]
		public const int TYPE_E_TYPEMISMATCH = -2147316576;

		[HRESULT.DescriptionAttribute("Invalid number of arguments.")]
		public const int TYPE_E_OUTOFBOUNDS = -2147316575;

		[HRESULT.DescriptionAttribute("I/O Error.")]
		public const int TYPE_E_IOERROR = -2147316574;

		[HRESULT.DescriptionAttribute("Error creating unique tmp file.")]
		public const int TYPE_E_CANTCREATETMPFILE = -2147316573;

		[HRESULT.DescriptionAttribute("Error loading type library/DLL.")]
		public const int TYPE_E_CANTLOADLIBRARY = -2147312566;

		[HRESULT.DescriptionAttribute("Inconsistent property functions.")]
		public const int TYPE_E_INCONSISTENTPROPFUNCS = -2147312509;

		[HRESULT.DescriptionAttribute("Circular dependency between types/modules.")]
		public const int TYPE_E_CIRCULARTYPE = -2147312508;

		[HRESULT.DescriptionAttribute("Unable to perform requested operation.")]
		public const int STG_E_INVALIDFUNCTION = -2147287039;

		[HRESULT.DescriptionAttribute("%1 could not be found.")]
		public const int STG_E_FILENOTFOUND = -2147287038;

		[HRESULT.DescriptionAttribute("The path %1 could not be found.")]
		public const int STG_E_PATHNOTFOUND = -2147287037;

		[HRESULT.DescriptionAttribute("There are insufficient resources to open another file.")]
		public const int STG_E_TOOMANYOPENFILES = -2147287036;

		[HRESULT.DescriptionAttribute("Access Denied.")]
		public const int STG_E_ACCESSDENIED = -2147287035;

		[HRESULT.DescriptionAttribute("Attempted an operation on an invalid object.")]
		public const int STG_E_INVALIDHANDLE = -2147287034;

		[HRESULT.DescriptionAttribute("There is insufficient memory available to complete operation.")]
		public const int STG_E_INSUFFICIENTMEMORY = -2147287032;

		[HRESULT.DescriptionAttribute("Invalid pointer error.")]
		public const int STG_E_INVALIDPOINTER = -2147287031;

		[HRESULT.DescriptionAttribute("There are no more entries to return.")]
		public const int STG_E_NOMOREFILES = -2147287022;

		[HRESULT.DescriptionAttribute("Disk is write-protected.")]
		public const int STG_E_DISKISWRITEPROTECTED = -2147287021;

		[HRESULT.DescriptionAttribute("An error occurred during a seek operation.")]
		public const int STG_E_SEEKERROR = -2147287015;

		[HRESULT.DescriptionAttribute("A disk error occurred during a write operation.")]
		public const int STG_E_WRITEFAULT = -2147287011;

		[HRESULT.DescriptionAttribute("A disk error occurred during a read operation.")]
		public const int STG_E_READFAULT = -2147287010;

		[HRESULT.DescriptionAttribute("A share violation has occurred.")]
		public const int STG_E_SHAREVIOLATION = -2147287008;

		[HRESULT.DescriptionAttribute("A lock violation has occurred.")]
		public const int STG_E_LOCKVIOLATION = -2147287007;

		[HRESULT.DescriptionAttribute("%1 already exists.")]
		public const int STG_E_FILEALREADYEXISTS = -2147286960;

		[HRESULT.DescriptionAttribute("Invalid parameter error.")]
		public const int STG_E_INVALIDPARAMETER = -2147286953;

		[HRESULT.DescriptionAttribute("There is insufficient disk space to complete operation.")]
		public const int STG_E_MEDIUMFULL = -2147286928;

		[HRESULT.DescriptionAttribute("Illegal write of non-simple property to simple property set.")]
		public const int STG_E_PROPSETMISMATCHED = -2147286800;

		[HRESULT.DescriptionAttribute("An API call exited abnormally.")]
		public const int STG_E_ABNORMALAPIEXIT = -2147286790;

		[HRESULT.DescriptionAttribute("The file %1 is not a valid compound file.")]
		public const int STG_E_INVALIDHEADER = -2147286789;

		[HRESULT.DescriptionAttribute("The name %1 is not valid.")]
		public const int STG_E_INVALIDNAME = -2147286788;

		[HRESULT.DescriptionAttribute("An unexpected error occurred.")]
		public const int STG_E_UNKNOWN = -2147286787;

		[HRESULT.DescriptionAttribute("That function is not implemented.")]
		public const int STG_E_UNIMPLEMENTEDFUNCTION = -2147286786;

		[HRESULT.DescriptionAttribute("Invalid flag error.")]
		public const int STG_E_INVALIDFLAG = -2147286785;

		[HRESULT.DescriptionAttribute("Attempted to use an object that is busy.")]
		public const int STG_E_INUSE = -2147286784;

		[HRESULT.DescriptionAttribute("The storage has been changed since the last commit.")]
		public const int STG_E_NOTCURRENT = -2147286783;

		[HRESULT.DescriptionAttribute("Attempted to use an object that has ceased to exist.")]
		public const int STG_E_REVERTED = -2147286782;

		[HRESULT.DescriptionAttribute("Can't save.")]
		public const int STG_E_CANTSAVE = -2147286781;

		[HRESULT.DescriptionAttribute("The compound file %1 was produced with an incompatible version of storage.")]
		public const int STG_E_OLDFORMAT = -2147286780;

		[HRESULT.DescriptionAttribute("The compound file %1 was produced with a newer version of storage.")]
		public const int STG_E_OLDDLL = -2147286779;

		[HRESULT.DescriptionAttribute("Share.exe or equivalent is required for operation.")]
		public const int STG_E_SHAREREQUIRED = -2147286778;

		[HRESULT.DescriptionAttribute("Illegal operation called on non-file based storage.")]
		public const int STG_E_NOTFILEBASEDSTORAGE = -2147286777;

		[HRESULT.DescriptionAttribute("Illegal operation called on object with extant marshallings.")]
		public const int STG_E_EXTANTMARSHALLINGS = -2147286776;

		[HRESULT.DescriptionAttribute("The docfile has been corrupted.")]
		public const int STG_E_DOCFILECORRUPT = -2147286775;

		[HRESULT.DescriptionAttribute("OLE32.DLL has been loaded at the wrong address.")]
		public const int STG_E_BADBASEADDRESS = -2147286768;

		[HRESULT.DescriptionAttribute("The compound file is too large for the current implementation")]
		public const int STG_E_DOCFILETOOLARGE = -2147286767;

		[HRESULT.DescriptionAttribute("The compound file was not created with the STGM_SIMPLE flag")]
		public const int STG_E_NOTSIMPLEFORMAT = -2147286766;

		[HRESULT.DescriptionAttribute("The file download was aborted abnormally.  The file is incomplete.")]
		public const int STG_E_INCOMPLETE = -2147286527;

		[HRESULT.DescriptionAttribute("The file download has been terminated.")]
		public const int STG_E_TERMINATED = -2147286526;

		[HRESULT.DescriptionAttribute("The underlying file was converted to compound file format.")]
		public const int STG_S_CONVERTED = 197120;

		[HRESULT.DescriptionAttribute("The storage operation should block until more data is available.")]
		public const int STG_S_BLOCK = 197121;

		[HRESULT.DescriptionAttribute("The storage operation should retry immediately.")]
		public const int STG_S_RETRYNOW = 197122;

		[HRESULT.DescriptionAttribute("The notified event sink will not influence the storage operation.")]
		public const int STG_S_MONITORING = 197123;

		[HRESULT.DescriptionAttribute("Multiple opens prevent consolidated. (commit succeeded).")]
		public const int STG_S_MULTIPLEOPENS = 197124;

		[HRESULT.DescriptionAttribute("Consolidation of the storage file failed. (commit succeeded).")]
		public const int STG_S_CONSOLIDATIONFAILED = 197125;

		[HRESULT.DescriptionAttribute("Consolidation of the storage file is inappropriate. (commit succeeded).")]
		public const int STG_S_CANNOTCONSOLIDATE = 197126;

		[HRESULT.DescriptionAttribute("Generic Copy Protection Error.")]
		public const int STG_E_STATUS_COPY_PROTECTION_FAILURE = -2147286267;

		[HRESULT.DescriptionAttribute("Copy Protection Error - DVD CSS Authentication failed.")]
		public const int STG_E_CSS_AUTHENTICATION_FAILURE = -2147286266;

		[HRESULT.DescriptionAttribute("Copy Protection Error - The given sector does not have a valid CSS key.")]
		public const int STG_E_CSS_KEY_NOT_PRESENT = -2147286265;

		[HRESULT.DescriptionAttribute("Copy Protection Error - DVD session key not established.")]
		public const int STG_E_CSS_KEY_NOT_ESTABLISHED = -2147286264;

		[HRESULT.DescriptionAttribute("Copy Protection Error - The read failed because the sector is encrypted.")]
		public const int STG_E_CSS_SCRAMBLED_SECTOR = -2147286263;

		[HRESULT.DescriptionAttribute("Copy Protection Error - The current DVD's region does not correspond to the region setting of the drive.")]
		public const int STG_E_CSS_REGION_MISMATCH = -2147286262;

		[HRESULT.DescriptionAttribute("Copy Protection Error - The drive's region setting may be permanent or the number of user resets has been exhausted.")]
		public const int STG_E_RESETS_EXHAUSTED = -2147286261;

		[HRESULT.DescriptionAttribute("Generic OLE errors that may be returned by many inerfaces")]
		public const int OLE_E_FIRST = -2147221504;

		public const int OLE_E_LAST = -2147221249;

		public const int OLE_S_FIRST = 262144;

		public const int OLE_S_LAST = 262399;

		[HRESULT.DescriptionAttribute("Invalid OLEVERB structure")]
		public const int OLE_E_OLEVERB = -2147221504;

		[HRESULT.DescriptionAttribute("Invalid advise flags")]
		public const int OLE_E_ADVF = -2147221503;

		[HRESULT.DescriptionAttribute("Can't enumerate any more, because the associated data is missing")]
		public const int OLE_E_ENUM_NOMORE = -2147221502;

		[HRESULT.DescriptionAttribute("This implementation doesn't take advises")]
		public const int OLE_E_ADVISENOTSUPPORTED = -2147221501;

		[HRESULT.DescriptionAttribute("There is no connection for this connection ID")]
		public const int OLE_E_NOCONNECTION = -2147221500;

		[HRESULT.DescriptionAttribute("Need to run the object to perform this operation")]
		public const int OLE_E_NOTRUNNING = -2147221499;

		[HRESULT.DescriptionAttribute("There is no cache to operate on")]
		public const int OLE_E_NOCACHE = -2147221498;

		[HRESULT.DescriptionAttribute("Uninitialized object")]
		public const int OLE_E_BLANK = -2147221497;

		[HRESULT.DescriptionAttribute("Linked object's source class has changed")]
		public const int OLE_E_CLASSDIFF = -2147221496;

		[HRESULT.DescriptionAttribute("Not able to get the moniker of the object")]
		public const int OLE_E_CANT_GETMONIKER = -2147221495;

		[HRESULT.DescriptionAttribute("Not able to bind to the source")]
		public const int OLE_E_CANT_BINDTOSOURCE = -2147221494;

		[HRESULT.DescriptionAttribute("Object is static; operation not allowed")]
		public const int OLE_E_STATIC = -2147221493;

		[HRESULT.DescriptionAttribute("User canceled out of save dialog")]
		public const int OLE_E_PROMPTSAVECANCELLED = -2147221492;

		[HRESULT.DescriptionAttribute("Invalid rectangle")]
		public const int OLE_E_INVALIDRECT = -2147221491;

		[HRESULT.DescriptionAttribute("compobj.dll is too old for the ole2.dll initialized")]
		public const int OLE_E_WRONGCOMPOBJ = -2147221490;

		[HRESULT.DescriptionAttribute("Invalid window handle")]
		public const int OLE_E_INVALIDHWND = -2147221489;

		[HRESULT.DescriptionAttribute("Object is not in any of the inplace active states")]
		public const int OLE_E_NOT_INPLACEACTIVE = -2147221488;

		[HRESULT.DescriptionAttribute("Not able to convert object")]
		public const int OLE_E_CANTCONVERT = -2147221487;

		[HRESULT.DescriptionAttribute("Not able to perform the operation because object is not given storage yet")]
		public const int OLE_E_NOSTORAGE = -2147221486;

		[HRESULT.DescriptionAttribute("Invalid FORMATETC structure")]
		public const int DV_E_FORMATETC = -2147221404;

		[HRESULT.DescriptionAttribute("Invalid DVTARGETDEVICE structure")]
		public const int DV_E_DVTARGETDEVICE = -2147221403;

		[HRESULT.DescriptionAttribute("Invalid STDGMEDIUM structure")]
		public const int DV_E_STGMEDIUM = -2147221402;

		[HRESULT.DescriptionAttribute("Invalid STATDATA structure")]
		public const int DV_E_STATDATA = -2147221401;

		[HRESULT.DescriptionAttribute("Invalid lindex")]
		public const int DV_E_LINDEX = -2147221400;

		[HRESULT.DescriptionAttribute("Invalid tymed")]
		public const int DV_E_TYMED = -2147221399;

		[HRESULT.DescriptionAttribute("Invalid clipboard format")]
		public const int DV_E_CLIPFORMAT = -2147221398;

		[HRESULT.DescriptionAttribute("Invalid aspect(s)")]
		public const int DV_E_DVASPECT = -2147221397;

		[HRESULT.DescriptionAttribute("tdSize parameter of the DVTARGETDEVICE structure is invalid")]
		public const int DV_E_DVTARGETDEVICE_SIZE = -2147221396;

		[HRESULT.DescriptionAttribute("Object doesn't support IViewObject interface")]
		public const int DV_E_NOIVIEWOBJECT = -2147221395;

		public const int DRAGDROP_E_FIRST = -2147221248;

		public const int DRAGDROP_E_LAST = -2147221233;

		public const int DRAGDROP_S_FIRST = 262400;

		public const int DRAGDROP_S_LAST = 262415;

		[HRESULT.DescriptionAttribute("Trying to revoke a drop target that has not been registered")]
		public const int DRAGDROP_E_NOTREGISTERED = -2147221248;

		[HRESULT.DescriptionAttribute("This window has already been registered as a drop target")]
		public const int DRAGDROP_E_ALREADYREGISTERED = -2147221247;

		[HRESULT.DescriptionAttribute("Invalid window handle")]
		public const int DRAGDROP_E_INVALIDHWND = -2147221246;

		public const int CLASSFACTORY_E_FIRST = -2147221232;

		public const int CLASSFACTORY_E_LAST = -2147221217;

		public const int CLASSFACTORY_S_FIRST = 262416;

		public const int CLASSFACTORY_S_LAST = 262431;

		[HRESULT.DescriptionAttribute("Class does not support aggregation (or class object is remote)")]
		public const int CLASS_E_NOAGGREGATION = -2147221232;

		[HRESULT.DescriptionAttribute("ClassFactory cannot supply requested class")]
		public const int CLASS_E_CLASSNOTAVAILABLE = -2147221231;

		[HRESULT.DescriptionAttribute("Class is not licensed for use")]
		public const int CLASS_E_NOTLICENSED = -2147221230;

		public const int MARSHAL_E_FIRST = -2147221216;

		public const int MARSHAL_E_LAST = -2147221201;

		public const int MARSHAL_S_FIRST = 262432;

		public const int MARSHAL_S_LAST = 262447;

		public const int DATA_E_FIRST = -2147221200;

		public const int DATA_E_LAST = -2147221185;

		public const int DATA_S_FIRST = 262448;

		public const int DATA_S_LAST = 262463;

		public const int VIEW_E_FIRST = -2147221184;

		public const int VIEW_E_LAST = -2147221169;

		public const int VIEW_S_FIRST = 262464;

		public const int VIEW_S_LAST = 262479;

		[HRESULT.DescriptionAttribute("Error drawing view")]
		public const int VIEW_E_DRAW = -2147221184;

		public const int REGDB_E_FIRST = -2147221168;

		public const int REGDB_E_LAST = -2147221153;

		public const int REGDB_S_FIRST = 262480;

		public const int REGDB_S_LAST = 262495;

		[HRESULT.DescriptionAttribute("Could not read key from registry")]
		public const int REGDB_E_READREGDB = -2147221168;

		[HRESULT.DescriptionAttribute("Could not write key to registry")]
		public const int REGDB_E_WRITEREGDB = -2147221167;

		[HRESULT.DescriptionAttribute("Could not find the key in the registry")]
		public const int REGDB_E_KEYMISSING = -2147221166;

		[HRESULT.DescriptionAttribute("Invalid value for registry")]
		public const int REGDB_E_INVALIDVALUE = -2147221165;

		[HRESULT.DescriptionAttribute("Class not registered")]
		public const int REGDB_E_CLASSNOTREG = -2147221164;

		[HRESULT.DescriptionAttribute("Interface not registered")]
		public const int REGDB_E_IIDNOTREG = -2147221163;

		[HRESULT.DescriptionAttribute("Threading model entry is not valid")]
		public const int REGDB_E_BADTHREADINGMODEL = -2147221162;

		public const int CAT_E_FIRST = -2147221152;

		public const int CAT_E_LAST = -2147221151;

		[HRESULT.DescriptionAttribute("CATID does not exist")]
		public const int CAT_E_CATIDNOEXIST = -2147221152;

		[HRESULT.DescriptionAttribute("Description not found")]
		public const int CAT_E_NODESCRIPTION = -2147221151;

		public const int CS_E_FIRST = -2147221148;

		public const int CS_E_LAST = -2147221137;

		[HRESULT.DescriptionAttribute("No package in the software installation data in the Active Directory meets this criteria.")]
		public const int CS_E_PACKAGE_NOTFOUND = -2147221148;

		[HRESULT.DescriptionAttribute("Deleting this will break the referential integrity of the software installation data in the Active Directory.")]
		public const int CS_E_NOT_DELETABLE = -2147221147;

		[HRESULT.DescriptionAttribute("The CLSID was not found in the software installation data in the Active Directory.")]
		public const int CS_E_CLASS_NOTFOUND = -2147221146;

		[HRESULT.DescriptionAttribute("The software installation data in the Active Directory is corrupt.")]
		public const int CS_E_INVALID_VERSION = -2147221145;

		[HRESULT.DescriptionAttribute("There is no software installation data in the Active Directory.")]
		public const int CS_E_NO_CLASSSTORE = -2147221144;

		[HRESULT.DescriptionAttribute("There is no software installation data object in the Active Directory.")]
		public const int CS_E_OBJECT_NOTFOUND = -2147221143;

		[HRESULT.DescriptionAttribute("The software installation data object in the Active Directory already exists.")]
		public const int CS_E_OBJECT_ALREADY_EXISTS = -2147221142;

		[HRESULT.DescriptionAttribute("The path to the software installation data in the Active Directory is not correct.")]
		public const int CS_E_INVALID_PATH = -2147221141;

		[HRESULT.DescriptionAttribute("A network error interrupted the operation.")]
		public const int CS_E_NETWORK_ERROR = -2147221140;

		[HRESULT.DescriptionAttribute("The size of this object exceeds the maximum size set by the Administrator.")]
		public const int CS_E_ADMIN_LIMIT_EXCEEDED = -2147221139;

		[HRESULT.DescriptionAttribute("The schema for the software installation data in the Active Directory does not match the required schema.")]
		public const int CS_E_SCHEMA_MISMATCH = -2147221138;

		[HRESULT.DescriptionAttribute("An error occurred in the software installation data in the Active Directory.")]
		public const int CS_E_INTERNAL_ERROR = -2147221137;

		public const int CACHE_E_FIRST = -2147221136;

		public const int CACHE_E_LAST = -2147221121;

		public const int CACHE_S_FIRST = 262512;

		public const int CACHE_S_LAST = 262527;

		[HRESULT.DescriptionAttribute("Cache not updated")]
		public const int CACHE_E_NOCACHE_UPDATED = -2147221136;

		public const int OLEOBJ_E_FIRST = -2147221120;

		public const int OLEOBJ_E_LAST = -2147221105;

		public const int OLEOBJ_S_FIRST = 262528;

		public const int OLEOBJ_S_LAST = 262543;

		[HRESULT.DescriptionAttribute("No verbs for OLE object")]
		public const int OLEOBJ_E_NOVERBS = -2147221120;

		[HRESULT.DescriptionAttribute("Invalid verb for OLE object")]
		public const int OLEOBJ_E_INVALIDVERB = -2147221119;

		public const int CLIENTSITE_E_FIRST = -2147221104;

		public const int CLIENTSITE_E_LAST = -2147221089;

		public const int CLIENTSITE_S_FIRST = 262544;

		public const int CLIENTSITE_S_LAST = 262559;

		public const int INPLACE_E_FIRST = -2147221088;

		public const int INPLACE_E_LAST = -2147221073;

		public const int INPLACE_S_FIRST = 262560;

		public const int INPLACE_S_LAST = 262575;

		[HRESULT.DescriptionAttribute("Undo is not available")]
		public const int INPLACE_E_NOTUNDOABLE = -2147221088;

		[HRESULT.DescriptionAttribute("Space for tools is not available")]
		public const int INPLACE_E_NOTOOLSPACE = -2147221087;

		public const int ENUM_E_FIRST = -2147221072;

		public const int ENUM_E_LAST = -2147221057;

		public const int ENUM_S_FIRST = 262576;

		public const int ENUM_S_LAST = 262591;

		public const int CONVERT10_E_FIRST = -2147221056;

		public const int CONVERT10_E_LAST = -2147221041;

		public const int CONVERT10_S_FIRST = 262592;

		public const int CONVERT10_S_LAST = 262607;

		[HRESULT.DescriptionAttribute("OLESTREAM Get method failed")]
		public const int CONVERT10_E_OLESTREAM_GET = -2147221056;

		[HRESULT.DescriptionAttribute("OLESTREAM Put method failed")]
		public const int CONVERT10_E_OLESTREAM_PUT = -2147221055;

		[HRESULT.DescriptionAttribute("Contents of the OLESTREAM not in correct format")]
		public const int CONVERT10_E_OLESTREAM_FMT = -2147221054;

		[HRESULT.DescriptionAttribute("There was an error in a Windows GDI call while converting the bitmap to a DIB")]
		public const int CONVERT10_E_OLESTREAM_BITMAP_TO_DIB = -2147221053;

		[HRESULT.DescriptionAttribute("Contents of the IStorage not in correct format")]
		public const int CONVERT10_E_STG_FMT = -2147221052;

		[HRESULT.DescriptionAttribute("Contents of IStorage is missing one of the standard streams")]
		public const int CONVERT10_E_STG_NO_STD_STREAM = -2147221051;

		[HRESULT.DescriptionAttribute("There was an error in a Windows GDI call while converting the DIB to a bitmap.")]
		public const int CONVERT10_E_STG_DIB_TO_BITMAP = -2147221050;

		public const int CLIPBRD_E_FIRST = -2147221040;

		public const int CLIPBRD_E_LAST = -2147221025;

		public const int CLIPBRD_S_FIRST = 262608;

		public const int CLIPBRD_S_LAST = 262623;

		[HRESULT.DescriptionAttribute("OpenClipboard Failed")]
		public const int CLIPBRD_E_CANT_OPEN = -2147221040;

		[HRESULT.DescriptionAttribute("EmptyClipboard Failed")]
		public const int CLIPBRD_E_CANT_EMPTY = -2147221039;

		[HRESULT.DescriptionAttribute("SetClipboard Failed")]
		public const int CLIPBRD_E_CANT_SET = -2147221038;

		[HRESULT.DescriptionAttribute("Data on clipboard is invalid")]
		public const int CLIPBRD_E_BAD_DATA = -2147221037;

		[HRESULT.DescriptionAttribute("CloseClipboard Failed")]
		public const int CLIPBRD_E_CANT_CLOSE = -2147221036;

		public const int MK_E_FIRST = -2147221024;

		public const int MK_E_LAST = -2147221009;

		public const int MK_S_FIRST = 262624;

		public const int MK_S_LAST = 262639;

		[HRESULT.DescriptionAttribute("Moniker needs to be connected manually")]
		public const int MK_E_CONNECTMANUALLY = -2147221024;

		[HRESULT.DescriptionAttribute("Operation exceeded deadline")]
		public const int MK_E_EXCEEDEDDEADLINE = -2147221023;

		[HRESULT.DescriptionAttribute("Moniker needs to be generic")]
		public const int MK_E_NEEDGENERIC = -2147221022;

		[HRESULT.DescriptionAttribute("Operation unavailable")]
		public const int MK_E_UNAVAILABLE = -2147221021;

		[HRESULT.DescriptionAttribute("Invalid syntax")]
		public const int MK_E_SYNTAX = -2147221020;

		[HRESULT.DescriptionAttribute("No object for moniker")]
		public const int MK_E_NOOBJECT = -2147221019;

		[HRESULT.DescriptionAttribute("Bad extension for file")]
		public const int MK_E_INVALIDEXTENSION = -2147221018;

		[HRESULT.DescriptionAttribute("Intermediate operation failed")]
		public const int MK_E_INTERMEDIATEINTERFACENOTSUPPORTED = -2147221017;

		[HRESULT.DescriptionAttribute("Moniker is not bindable")]
		public const int MK_E_NOTBINDABLE = -2147221016;

		[HRESULT.DescriptionAttribute("Moniker is not bound")]
		public const int MK_E_NOTBOUND = -2147221015;

		[HRESULT.DescriptionAttribute("Moniker cannot open file")]
		public const int MK_E_CANTOPENFILE = -2147221014;

		[HRESULT.DescriptionAttribute("User input required for operation to succeed")]
		public const int MK_E_MUSTBOTHERUSER = -2147221013;

		[HRESULT.DescriptionAttribute("Moniker class has no inverse")]
		public const int MK_E_NOINVERSE = -2147221012;

		[HRESULT.DescriptionAttribute("Moniker does not refer to storage")]
		public const int MK_E_NOSTORAGE = -2147221011;

		[HRESULT.DescriptionAttribute("No common prefix")]
		public const int MK_E_NOPREFIX = -2147221010;

		[HRESULT.DescriptionAttribute("Moniker could not be enumerated")]
		public const int MK_E_ENUMERATION_FAILED = -2147221009;

		public const int CO_E_FIRST = -2147221008;

		public const int CO_E_LAST = -2147220993;

		public const int CO_S_FIRST = 262640;

		public const int CO_S_LAST = 262655;

		[HRESULT.DescriptionAttribute("CoInitialize has not been called.")]
		public const int CO_E_NOTINITIALIZED = -2147221008;

		[HRESULT.DescriptionAttribute("CoInitialize has already been called.")]
		public const int CO_E_ALREADYINITIALIZED = -2147221007;

		[HRESULT.DescriptionAttribute("Class of object cannot be determined")]
		public const int CO_E_CANTDETERMINECLASS = -2147221006;

		[HRESULT.DescriptionAttribute("Invalid class string")]
		public const int CO_E_CLASSSTRING = -2147221005;

		[HRESULT.DescriptionAttribute("Invalid interface string")]
		public const int CO_E_IIDSTRING = -2147221004;

		[HRESULT.DescriptionAttribute("Application not found")]
		public const int CO_E_APPNOTFOUND = -2147221003;

		[HRESULT.DescriptionAttribute("Application cannot be run more than once")]
		public const int CO_E_APPSINGLEUSE = -2147221002;

		[HRESULT.DescriptionAttribute("Some error in application program")]
		public const int CO_E_ERRORINAPP = -2147221001;

		[HRESULT.DescriptionAttribute("DLL for class not found")]
		public const int CO_E_DLLNOTFOUND = -2147221000;

		[HRESULT.DescriptionAttribute("Error in the DLL")]
		public const int CO_E_ERRORINDLL = -2147220999;

		[HRESULT.DescriptionAttribute("Wrong OS or OS version for application")]
		public const int CO_E_WRONGOSFORAPP = -2147220998;

		[HRESULT.DescriptionAttribute("Object is not registered")]
		public const int CO_E_OBJNOTREG = -2147220997;

		[HRESULT.DescriptionAttribute("Object is already registered")]
		public const int CO_E_OBJISREG = -2147220996;

		[HRESULT.DescriptionAttribute("Object is not connected to server")]
		public const int CO_E_OBJNOTCONNECTED = -2147220995;

		[HRESULT.DescriptionAttribute("Application was launched but it didn't register a class factory")]
		public const int CO_E_APPDIDNTREG = -2147220994;

		[HRESULT.DescriptionAttribute("Object has been released")]
		public const int CO_E_RELEASED = -2147220993;

		public const int EVENT_E_FIRST = -2147220992;

		public const int EVENT_E_LAST = -2147220961;

		public const int EVENT_S_FIRST = 262656;

		public const int EVENT_S_LAST = 262687;

		[HRESULT.DescriptionAttribute("An event was able to invoke some but not all of the subscribers")]
		public const int EVENT_S_SOME_SUBSCRIBERS_FAILED = 262656;

		[HRESULT.DescriptionAttribute("An event was unable to invoke any of the subscribers")]
		public const int EVENT_E_ALL_SUBSCRIBERS_FAILED = -2147220991;

		[HRESULT.DescriptionAttribute("An event was delivered but there were no subscribers")]
		public const int EVENT_S_NOSUBSCRIBERS = 262658;

		[HRESULT.DescriptionAttribute("A syntax error occurred trying to evaluate a query string")]
		public const int EVENT_E_QUERYSYNTAX = -2147220989;

		[HRESULT.DescriptionAttribute("An invalid field name was used in a query string")]
		public const int EVENT_E_QUERYFIELD = -2147220988;

		[HRESULT.DescriptionAttribute("An unexpected exception was raised")]
		public const int EVENT_E_INTERNALEXCEPTION = -2147220987;

		[HRESULT.DescriptionAttribute("An unexpected internal error was detected")]
		public const int EVENT_E_INTERNALERROR = -2147220986;

		[HRESULT.DescriptionAttribute("The owner SID on a per-user subscription doesn't exist")]
		public const int EVENT_E_INVALID_PER_USER_SID = -2147220985;

		[HRESULT.DescriptionAttribute("A user-supplied component or subscriber raised an exception")]
		public const int EVENT_E_USER_EXCEPTION = -2147220984;

		[HRESULT.DescriptionAttribute("An interface has too many methods to fire events from")]
		public const int EVENT_E_TOO_MANY_METHODS = -2147220983;

		[HRESULT.DescriptionAttribute("A subscription cannot be stored unless its event class already exists")]
		public const int EVENT_E_MISSING_EVENTCLASS = -2147220982;

		[HRESULT.DescriptionAttribute("Not all the objects requested could be removed")]
		public const int EVENT_E_NOT_ALL_REMOVED = -2147220981;

		[HRESULT.DescriptionAttribute("COM+ is required for this operation, but is not installed")]
		public const int EVENT_E_COMPLUS_NOT_INSTALLED = -2147220980;

		[HRESULT.DescriptionAttribute("Cannot modify or delete an object that was not added using the COM+ Admin SDK")]
		public const int EVENT_E_CANT_MODIFY_OR_DELETE_UNCONFIGURED_OBJECT = -2147220979;

		[HRESULT.DescriptionAttribute("Cannot modify or delete an object that was added using the COM+ Admin SDK")]
		public const int EVENT_E_CANT_MODIFY_OR_DELETE_CONFIGURED_OBJECT = -2147220978;

		[HRESULT.DescriptionAttribute("The event class for this subscription is in an invalid partition")]
		public const int EVENT_E_INVALID_EVENT_CLASS_PARTITION = -2147220977;

		[HRESULT.DescriptionAttribute("The owner of the PerUser subscription is not logged on to the system specified")]
		public const int EVENT_E_PER_USER_SID_NOT_LOGGED_ON = -2147220976;

		public const int XACT_E_FIRST = -2147168256;

		public const int XACT_E_LAST = -2147168215;

		public const int XACT_S_FIRST = 315392;

		public const int XACT_S_LAST = 315408;

		[HRESULT.DescriptionAttribute("Another single phase resource manager has already been enlisted in this transaction.")]
		public const int XACT_E_ALREADYOTHERSINGLEPHASE = -2147168256;

		[HRESULT.DescriptionAttribute("A retaining commit or abort is not supported")]
		public const int XACT_E_CANTRETAIN = -2147168255;

		[HRESULT.DescriptionAttribute("The transaction failed to commit for an unknown reason. The transaction was aborted.")]
		public const int XACT_E_COMMITFAILED = -2147168254;

		[HRESULT.DescriptionAttribute("Cannot call commit on this transaction object because the calling application did not initiate the transaction.")]
		public const int XACT_E_COMMITPREVENTED = -2147168253;

		[HRESULT.DescriptionAttribute("Instead of committing, the resource heuristically aborted.")]
		public const int XACT_E_HEURISTICABORT = -2147168252;

		[HRESULT.DescriptionAttribute("Instead of aborting, the resource heuristically committed.")]
		public const int XACT_E_HEURISTICCOMMIT = -2147168251;

		[HRESULT.DescriptionAttribute("Some of the states of the resource were committed while others were aborted, likely because of heuristic decisions.")]
		public const int XACT_E_HEURISTICDAMAGE = -2147168250;

		[HRESULT.DescriptionAttribute("Some of the states of the resource may have been committed while others may have been aborted, likely because of heuristic decisions.")]
		public const int XACT_E_HEURISTICDANGER = -2147168249;

		[HRESULT.DescriptionAttribute("The requested isolation level is not valid or supported.")]
		public const int XACT_E_ISOLATIONLEVEL = -2147168248;

		[HRESULT.DescriptionAttribute("The transaction manager doesn't support an asynchronous operation for this method.")]
		public const int XACT_E_NOASYNC = -2147168247;

		[HRESULT.DescriptionAttribute("Unable to enlist in the transaction.")]
		public const int XACT_E_NOENLIST = -2147168246;

		[HRESULT.DescriptionAttribute("The requested semantics of retention of isolation across retaining commit and abort boundaries cannot be supported by this transaction implementation, or isoFlags was not equal to zero.")]
		public const int XACT_E_NOISORETAIN = -2147168245;

		[HRESULT.DescriptionAttribute("There is no resource presently associated with this enlistment")]
		public const int XACT_E_NORESOURCE = -2147168244;

		[HRESULT.DescriptionAttribute("The transaction failed to commit due to the failure of optimistic concurrency control in at least one of the resource managers.")]
		public const int XACT_E_NOTCURRENT = -2147168243;

		[HRESULT.DescriptionAttribute("The transaction has already been implicitly or explicitly committed or aborted")]
		public const int XACT_E_NOTRANSACTION = -2147168242;

		[HRESULT.DescriptionAttribute("An invalid combination of flags was specified")]
		public const int XACT_E_NOTSUPPORTED = -2147168241;

		[HRESULT.DescriptionAttribute("The resource manager id is not associated with this transaction or the transaction manager.")]
		public const int XACT_E_UNKNOWNRMGRID = -2147168240;

		[HRESULT.DescriptionAttribute("This method was called in the wrong state")]
		public const int XACT_E_WRONGSTATE = -2147168239;

		[HRESULT.DescriptionAttribute("The indicated unit of work does not match the unit of work expected by the resource manager.")]
		public const int XACT_E_WRONGUOW = -2147168238;

		[HRESULT.DescriptionAttribute("An enlistment in a transaction already exists.")]
		public const int XACT_E_XTIONEXISTS = -2147168237;

		[HRESULT.DescriptionAttribute("An import object for the transaction could not be found.")]
		public const int XACT_E_NOIMPORTOBJECT = -2147168236;

		[HRESULT.DescriptionAttribute("The transaction cookie is invalid.")]
		public const int XACT_E_INVALIDCOOKIE = -2147168235;

		[HRESULT.DescriptionAttribute("The transaction status is in doubt. A communication failure occurred, or a transaction manager or resource manager has failed")]
		public const int XACT_E_INDOUBT = -2147168234;

		[HRESULT.DescriptionAttribute("A time-out was specified, but time-outs are not supported.")]
		public const int XACT_E_NOTIMEOUT = -2147168233;

		[HRESULT.DescriptionAttribute("The requested operation is already in progress for the transaction.")]
		public const int XACT_E_ALREADYINPROGRESS = -2147168232;

		[HRESULT.DescriptionAttribute("The transaction has already been aborted.")]
		public const int XACT_E_ABORTED = -2147168231;

		[HRESULT.DescriptionAttribute("The Transaction Manager returned a log full error.")]
		public const int XACT_E_LOGFULL = -2147168230;

		[HRESULT.DescriptionAttribute("The Transaction Manager is not available.")]
		public const int XACT_E_TMNOTAVAILABLE = -2147168229;

		[HRESULT.DescriptionAttribute("A connection with the transaction manager was lost.")]
		public const int XACT_E_CONNECTION_DOWN = -2147168228;

		[HRESULT.DescriptionAttribute("A request to establish a connection with the transaction manager was denied.")]
		public const int XACT_E_CONNECTION_DENIED = -2147168227;

		[HRESULT.DescriptionAttribute("Resource manager reenlistment to determine transaction status timed out.")]
		public const int XACT_E_REENLISTTIMEOUT = -2147168226;

		[HRESULT.DescriptionAttribute("This transaction manager failed to establish a connection with another TIP transaction manager.")]
		public const int XACT_E_TIP_CONNECT_FAILED = -2147168225;

		[HRESULT.DescriptionAttribute("This transaction manager encountered a protocol error with another TIP transaction manager.")]
		public const int XACT_E_TIP_PROTOCOL_ERROR = -2147168224;

		[HRESULT.DescriptionAttribute("This transaction manager could not propagate a transaction from another TIP transaction manager.")]
		public const int XACT_E_TIP_PULL_FAILED = -2147168223;

		[HRESULT.DescriptionAttribute("The Transaction Manager on the destination machine is not available.")]
		public const int XACT_E_DEST_TMNOTAVAILABLE = -2147168222;

		[HRESULT.DescriptionAttribute("The Transaction Manager has disabled its support for TIP.")]
		public const int XACT_E_TIP_DISABLED = -2147168221;

		[HRESULT.DescriptionAttribute("The transaction manager has disabled its support for remote/network transactions.")]
		public const int XACT_E_NETWORK_TX_DISABLED = -2147168220;

		[HRESULT.DescriptionAttribute("The partner transaction manager has disabled its support for remote/network transactions.")]
		public const int XACT_E_PARTNER_NETWORK_TX_DISABLED = -2147168219;

		[HRESULT.DescriptionAttribute("The transaction manager has disabled its support for XA transactions.")]
		public const int XACT_E_XA_TX_DISABLED = -2147168218;

		[HRESULT.DescriptionAttribute("MSDTC was unable to read its configuration information.")]
		public const int XACT_E_UNABLE_TO_READ_DTC_CONFIG = -2147168217;

		[HRESULT.DescriptionAttribute("MSDTC was unable to load the dtc proxy dll.")]
		public const int XACT_E_UNABLE_TO_LOAD_DTC_PROXY = -2147168216;

		[HRESULT.DescriptionAttribute("The local transaction has aborted.")]
		public const int XACT_E_ABORTING = -2147168215;

		[HRESULT.DescriptionAttribute("XACT_E_CLERKNOTFOUND")]
		public const int XACT_E_CLERKNOTFOUND = -2147168128;

		[HRESULT.DescriptionAttribute("XACT_E_CLERKEXISTS")]
		public const int XACT_E_CLERKEXISTS = -2147168127;

		[HRESULT.DescriptionAttribute("XACT_E_RECOVERYINPROGRESS")]
		public const int XACT_E_RECOVERYINPROGRESS = -2147168126;

		[HRESULT.DescriptionAttribute("XACT_E_TRANSACTIONCLOSED")]
		public const int XACT_E_TRANSACTIONCLOSED = -2147168125;

		[HRESULT.DescriptionAttribute("XACT_E_INVALIDLSN")]
		public const int XACT_E_INVALIDLSN = -2147168124;

		[HRESULT.DescriptionAttribute("XACT_E_REPLAYREQUEST")]
		public const int XACT_E_REPLAYREQUEST = -2147168123;

		[HRESULT.DescriptionAttribute("An asynchronous operation was specified. The operation has begun, but its outcome is not known yet.")]
		public const int XACT_S_ASYNC = 315392;

		[HRESULT.DescriptionAttribute("XACT_S_DEFECT")]
		public const int XACT_S_DEFECT = 315393;

		[HRESULT.DescriptionAttribute("The method call succeeded because the transaction was read-only.")]
		public const int XACT_S_READONLY = 315394;

		[HRESULT.DescriptionAttribute("The transaction was successfully aborted. However, this is a coordinated transaction, and some number of enlisted resources were aborted outright because they could not support abort-retaining semantics")]
		public const int XACT_S_SOMENORETAIN = 315395;

		[HRESULT.DescriptionAttribute("No changes were made during this call, but the sink wants another chance to look if any other sinks make further changes.")]
		public const int XACT_S_OKINFORM = 315396;

		[HRESULT.DescriptionAttribute("The sink is content and wishes the transaction to proceed. Changes were made to one or more resources during this call.")]
		public const int XACT_S_MADECHANGESCONTENT = 315397;

		[HRESULT.DescriptionAttribute("The sink is for the moment and wishes the transaction to proceed, but if other changes are made following this return by other event sinks then this sink wants another chance to look")]
		public const int XACT_S_MADECHANGESINFORM = 315398;

		[HRESULT.DescriptionAttribute("The transaction was successfully aborted. However, the abort was non-retaining.")]
		public const int XACT_S_ALLNORETAIN = 315399;

		[HRESULT.DescriptionAttribute("An abort operation was already in progress.")]
		public const int XACT_S_ABORTING = 315400;

		[HRESULT.DescriptionAttribute("The resource manager has performed a single-phase commit of the transaction.")]
		public const int XACT_S_SINGLEPHASE = 315401;

		[HRESULT.DescriptionAttribute("The local transaction has not aborted.")]
		public const int XACT_S_LOCALLY_OK = 315402;

		[HRESULT.DescriptionAttribute("The resource manager has requested to be the coordinator (last resource manager) for the transaction.")]
		public const int XACT_S_LASTRESOURCEMANAGER = 315408;

		public const int CONTEXT_E_FIRST = -2147164160;

		public const int CONTEXT_E_LAST = -2147164113;

		public const int CONTEXT_S_FIRST = 319488;

		public const int CONTEXT_S_LAST = 319535;

		[HRESULT.DescriptionAttribute("The root transaction wanted to commit, but transaction aborted")]
		public const int CONTEXT_E_ABORTED = -2147164158;

		[HRESULT.DescriptionAttribute("You made a method call on a COM+ component that has a transaction that has already aborted or in the process of aborting.")]
		public const int CONTEXT_E_ABORTING = -2147164157;

		[HRESULT.DescriptionAttribute("There is no MTS object context")]
		public const int CONTEXT_E_NOCONTEXT = -2147164156;

		[HRESULT.DescriptionAttribute("The component is configured to use synchronization and this method call would cause a deadlock to occur.")]
		public const int CONTEXT_E_WOULD_DEADLOCK = -2147164155;

		[HRESULT.DescriptionAttribute("The component is configured to use synchronization and a thread has timed out waiting to enter the context.")]
		public const int CONTEXT_E_SYNCH_TIMEOUT = -2147164154;

		[HRESULT.DescriptionAttribute("You made a method call on a COM+ component that has a transaction that has already committed or aborted.")]
		public const int CONTEXT_E_OLDREF = -2147164153;

		[HRESULT.DescriptionAttribute("The specified role was not configured for the application")]
		public const int CONTEXT_E_ROLENOTFOUND = -2147164148;

		[HRESULT.DescriptionAttribute("COM+ was unable to talk to the Microsoft Distributed Transaction Coordinator")]
		public const int CONTEXT_E_TMNOTAVAILABLE = -2147164145;

		[HRESULT.DescriptionAttribute("An unexpected error occurred during COM+ Activation.")]
		public const int CO_E_ACTIVATIONFAILED = -2147164127;

		[HRESULT.DescriptionAttribute("COM+ Activation failed. Check the event log for more information")]
		public const int CO_E_ACTIVATIONFAILED_EVENTLOGGED = -2147164126;

		[HRESULT.DescriptionAttribute("COM+ Activation failed due to a catalog or configuration error.")]
		public const int CO_E_ACTIVATIONFAILED_CATALOGERROR = -2147164125;

		[HRESULT.DescriptionAttribute("COM+ activation failed because the activation could not be completed in the specified amount of time.")]
		public const int CO_E_ACTIVATIONFAILED_TIMEOUT = -2147164124;

		[HRESULT.DescriptionAttribute("COM+ Activation failed because an initialization function failed.  Check the event log for more information.")]
		public const int CO_E_INITIALIZATIONFAILED = -2147164123;

		[HRESULT.DescriptionAttribute("The requested operation requires that JIT be in the current context and it is not")]
		public const int CONTEXT_E_NOJIT = -2147164122;

		[HRESULT.DescriptionAttribute("The requested operation requires that the current context have a Transaction, and it does not")]
		public const int CONTEXT_E_NOTRANSACTION = -2147164121;

		[HRESULT.DescriptionAttribute("The components threading model has changed after install into a COM+ Application.  Please re-install component.")]
		public const int CO_E_THREADINGMODEL_CHANGED = -2147164120;

		[HRESULT.DescriptionAttribute("IIS intrinsics not available.  Start your work with IIS.")]
		public const int CO_E_NOIISINTRINSICS = -2147164119;

		[HRESULT.DescriptionAttribute("An attempt to write a cookie failed.")]
		public const int CO_E_NOCOOKIES = -2147164118;

		[HRESULT.DescriptionAttribute("An attempt to use a database generated a database specific error.")]
		public const int CO_E_DBERROR = -2147164117;

		[HRESULT.DescriptionAttribute("The COM+ component you created must use object pooling to work.")]
		public const int CO_E_NOTPOOLED = -2147164116;

		[HRESULT.DescriptionAttribute("The COM+ component you created must use object construction to work correctly.")]
		public const int CO_E_NOTCONSTRUCTED = -2147164115;

		[HRESULT.DescriptionAttribute("The COM+ component requires synchronization, and it is not configured for it.")]
		public const int CO_E_NOSYNCHRONIZATION = -2147164114;

		[HRESULT.DescriptionAttribute("The TxIsolation Level property for the COM+ component being created is stronger than the TxIsolationLevel for the \"root\" component for the transaction.  The creation failed.")]
		public const int CO_E_ISOLEVELMISMATCH = -2147164113;

		[HRESULT.DescriptionAttribute("Use the registry database to provide the requested information")]
		public const int OLE_S_USEREG = 262144;

		[HRESULT.DescriptionAttribute("Success, but static")]
		public const int OLE_S_STATIC = 262145;

		[HRESULT.DescriptionAttribute("Macintosh clipboard format")]
		public const int OLE_S_MAC_CLIPFORMAT = 262146;

		[HRESULT.DescriptionAttribute("Successful drop took place")]
		public const int DRAGDROP_S_DROP = 262400;

		[HRESULT.DescriptionAttribute("Drag-drop operation canceled")]
		public const int DRAGDROP_S_CANCEL = 262401;

		[HRESULT.DescriptionAttribute("Use the default cursor")]
		public const int DRAGDROP_S_USEDEFAULTCURSORS = 262402;

		[HRESULT.DescriptionAttribute("Data has same FORMATETC")]
		public const int DATA_S_SAMEFORMATETC = 262448;

		[HRESULT.DescriptionAttribute("View is already frozen")]
		public const int VIEW_S_ALREADY_FROZEN = 262464;

		[HRESULT.DescriptionAttribute("FORMATETC not supported")]
		public const int CACHE_S_FORMATETC_NOTSUPPORTED = 262512;

		[HRESULT.DescriptionAttribute("Same cache")]
		public const int CACHE_S_SAMECACHE = 262513;

		[HRESULT.DescriptionAttribute("Some cache(s) not updated")]
		public const int CACHE_S_SOMECACHES_NOTUPDATED = 262514;

		[HRESULT.DescriptionAttribute("Invalid verb for OLE object")]
		public const int OLEOBJ_S_INVALIDVERB = 262528;

		[HRESULT.DescriptionAttribute("Verb number is valid but verb cannot be done now")]
		public const int OLEOBJ_S_CANNOT_DOVERB_NOW = 262529;

		[HRESULT.DescriptionAttribute("Invalid window handle passed")]
		public const int OLEOBJ_S_INVALIDHWND = 262530;

		[HRESULT.DescriptionAttribute("Message is too long; some of it had to be truncated before displaying")]
		public const int INPLACE_S_TRUNCATED = 262560;

		[HRESULT.DescriptionAttribute("Unable to convert OLESTREAM to IStorage")]
		public const int CONVERT10_S_NO_PRESENTATION = 262592;

		[HRESULT.DescriptionAttribute("Moniker reduced to itself")]
		public const int MK_S_REDUCED_TO_SELF = 262626;

		[HRESULT.DescriptionAttribute("Common prefix is this moniker")]
		public const int MK_S_ME = 262628;

		[HRESULT.DescriptionAttribute("Common prefix is input moniker")]
		public const int MK_S_HIM = 262629;

		[HRESULT.DescriptionAttribute("Common prefix is both monikers")]
		public const int MK_S_US = 262630;

		[HRESULT.DescriptionAttribute("Moniker is already registered in running object table")]
		public const int MK_S_MONIKERALREADYREGISTERED = 262631;

		[HRESULT.DescriptionAttribute("The task is ready to run at its next scheduled time.")]
		public const int SCHED_S_TASK_READY = 267008;

		[HRESULT.DescriptionAttribute("The task is currently running.")]
		public const int SCHED_S_TASK_RUNNING = 267009;

		[HRESULT.DescriptionAttribute("The task will not run at the scheduled times because it has been disabled.")]
		public const int SCHED_S_TASK_DISABLED = 267010;

		[HRESULT.DescriptionAttribute("The task has not yet run.")]
		public const int SCHED_S_TASK_HAS_NOT_RUN = 267011;

		[HRESULT.DescriptionAttribute("There are no more runs scheduled for this task.")]
		public const int SCHED_S_TASK_NO_MORE_RUNS = 267012;

		[HRESULT.DescriptionAttribute("One or more of the properties that are needed to run this task on a schedule have not been set.")]
		public const int SCHED_S_TASK_NOT_SCHEDULED = 267013;

		[HRESULT.DescriptionAttribute("The last run of the task was terminated by the user.")]
		public const int SCHED_S_TASK_TERMINATED = 267014;

		[HRESULT.DescriptionAttribute("Either the task has no triggers or the existing triggers are disabled or not set.")]
		public const int SCHED_S_TASK_NO_VALID_TRIGGERS = 267015;

		[HRESULT.DescriptionAttribute("Event triggers don't have set run times.")]
		public const int SCHED_S_EVENT_TRIGGER = 267016;

		[HRESULT.DescriptionAttribute("Trigger not found.")]
		public const int SCHED_E_TRIGGER_NOT_FOUND = -2147216631;

		[HRESULT.DescriptionAttribute("One or more of the properties that are needed to run this task have not been set.")]
		public const int SCHED_E_TASK_NOT_READY = -2147216630;

		[HRESULT.DescriptionAttribute("There is no running instance of the task to terminate.")]
		public const int SCHED_E_TASK_NOT_RUNNING = -2147216629;

		[HRESULT.DescriptionAttribute("The Task Scheduler Service is not installed on this computer.")]
		public const int SCHED_E_SERVICE_NOT_INSTALLED = -2147216628;

		[HRESULT.DescriptionAttribute("The task object could not be opened.")]
		public const int SCHED_E_CANNOT_OPEN_TASK = -2147216627;

		[HRESULT.DescriptionAttribute("The object is either an invalid task object or is not a task object.")]
		public const int SCHED_E_INVALID_TASK = -2147216626;

		[HRESULT.DescriptionAttribute("No account information could be found in the Task Scheduler security database for the task indicated.")]
		public const int SCHED_E_ACCOUNT_INFORMATION_NOT_SET = -2147216625;

		[HRESULT.DescriptionAttribute("Unable to establish existence of the account specified.")]
		public const int SCHED_E_ACCOUNT_NAME_NOT_FOUND = -2147216624;

		[HRESULT.DescriptionAttribute("Corruption was detected in the Task Scheduler security database; the database has been reset.")]
		public const int SCHED_E_ACCOUNT_DBASE_CORRUPT = -2147216623;

		[HRESULT.DescriptionAttribute("Task Scheduler security services are available only on Windows NT.")]
		public const int SCHED_E_NO_SECURITY_SERVICES = -2147216622;

		[HRESULT.DescriptionAttribute("The task object version is either unsupported or invalid.")]
		public const int SCHED_E_UNKNOWN_OBJECT_VERSION = -2147216621;

		[HRESULT.DescriptionAttribute("The task has been configured with an unsupported combination of account settings and run time options.")]
		public const int SCHED_E_UNSUPPORTED_ACCOUNT_OPTION = -2147216620;

		[HRESULT.DescriptionAttribute("The Task Scheduler Service is not running.")]
		public const int SCHED_E_SERVICE_NOT_RUNNING = -2147216619;

		[HRESULT.DescriptionAttribute("Attempt to create a class object failed")]
		public const int CO_E_CLASS_CREATE_FAILED = -2146959359;

		[HRESULT.DescriptionAttribute("OLE service could not bind object")]
		public const int CO_E_SCM_ERROR = -2146959358;

		[HRESULT.DescriptionAttribute("RPC communication failed with OLE service")]
		public const int CO_E_SCM_RPC_FAILURE = -2146959357;

		[HRESULT.DescriptionAttribute("Bad path to object")]
		public const int CO_E_BAD_PATH = -2146959356;

		[HRESULT.DescriptionAttribute("Server execution failed")]
		public const int CO_E_SERVER_EXEC_FAILURE = -2146959355;

		[HRESULT.DescriptionAttribute("OLE service could not communicate with the object server")]
		public const int CO_E_OBJSRV_RPC_FAILURE = -2146959354;

		[HRESULT.DescriptionAttribute("Moniker path could not be normalized")]
		public const int MK_E_NO_NORMALIZED = -2146959353;

		[HRESULT.DescriptionAttribute("Object server is stopping when OLE service contacts it")]
		public const int CO_E_SERVER_STOPPING = -2146959352;

		[HRESULT.DescriptionAttribute("An invalid root block pointer was specified")]
		public const int MEM_E_INVALID_ROOT = -2146959351;

		[HRESULT.DescriptionAttribute("An allocation chain contained an invalid link pointer")]
		public const int MEM_E_INVALID_LINK = -2146959344;

		[HRESULT.DescriptionAttribute("The requested allocation size was too large")]
		public const int MEM_E_INVALID_SIZE = -2146959343;

		[HRESULT.DescriptionAttribute("Not all the requested interfaces were available")]
		public const int CO_S_NOTALLINTERFACES = 524306;

		[HRESULT.DescriptionAttribute("The specified machine name was not found in the cache.")]
		public const int CO_S_MACHINENAMENOTFOUND = 524307;

		public const int NTE_OP_OK = 0;

		[HRESULT.DescriptionAttribute("The specified event is currently not being audited.")]
		public const int ERROR_AUDITING_DISABLED = -1073151999;

		[HRESULT.DescriptionAttribute("The SID filtering operation removed all SIDs.")]
		public const int ERROR_ALL_SIDS_FILTERED = -1073151998;

		[HRESULT.DescriptionAttribute("Bad UID.")]
		public const int NTE_BAD_UID = -2146893823;

		[HRESULT.DescriptionAttribute("Bad Hash.")]
		public const int NTE_BAD_HASH = -2146893822;

		[HRESULT.DescriptionAttribute("Bad Key.")]
		public const int NTE_BAD_KEY = -2146893821;

		[HRESULT.DescriptionAttribute("Bad Length.")]
		public const int NTE_BAD_LEN = -2146893820;

		[HRESULT.DescriptionAttribute("Bad Data.")]
		public const int NTE_BAD_DATA = -2146893819;

		[HRESULT.DescriptionAttribute("Invalid Signature.")]
		public const int NTE_BAD_SIGNATURE = -2146893818;

		[HRESULT.DescriptionAttribute("Bad Version of provider.")]
		public const int NTE_BAD_VER = -2146893817;

		[HRESULT.DescriptionAttribute("Invalid algorithm specified.")]
		public const int NTE_BAD_ALGID = -2146893816;

		[HRESULT.DescriptionAttribute("Invalid flags specified.")]
		public const int NTE_BAD_FLAGS = -2146893815;

		[HRESULT.DescriptionAttribute("Invalid type specified.")]
		public const int NTE_BAD_TYPE = -2146893814;

		[HRESULT.DescriptionAttribute("Key not valid for use in specified state.")]
		public const int NTE_BAD_KEY_STATE = -2146893813;

		[HRESULT.DescriptionAttribute("Hash not valid for use in specified state.")]
		public const int NTE_BAD_HASH_STATE = -2146893812;

		[HRESULT.DescriptionAttribute("Key does not exist.")]
		public const int NTE_NO_KEY = -2146893811;

		[HRESULT.DescriptionAttribute("Insufficient memory available for the operation.")]
		public const int NTE_NO_MEMORY = -2146893810;

		[HRESULT.DescriptionAttribute("Object already exists.")]
		public const int NTE_EXISTS = -2146893809;

		[HRESULT.DescriptionAttribute("Access denied.")]
		public const int NTE_PERM = -2146893808;

		[HRESULT.DescriptionAttribute("Object was not found.")]
		public const int NTE_NOT_FOUND = -2146893807;

		[HRESULT.DescriptionAttribute("Data already encrypted.")]
		public const int NTE_DOUBLE_ENCRYPT = -2146893806;

		[HRESULT.DescriptionAttribute("Invalid provider specified.")]
		public const int NTE_BAD_PROVIDER = -2146893805;

		[HRESULT.DescriptionAttribute("Invalid provider type specified.")]
		public const int NTE_BAD_PROV_TYPE = -2146893804;

		[HRESULT.DescriptionAttribute("Provider's public key is invalid.")]
		public const int NTE_BAD_PUBLIC_KEY = -2146893803;

		[HRESULT.DescriptionAttribute("Keyset does not exist")]
		public const int NTE_BAD_KEYSET = -2146893802;

		[HRESULT.DescriptionAttribute("Provider type not defined.")]
		public const int NTE_PROV_TYPE_NOT_DEF = -2146893801;

		[HRESULT.DescriptionAttribute("Provider type as registered is invalid.")]
		public const int NTE_PROV_TYPE_ENTRY_BAD = -2146893800;

		[HRESULT.DescriptionAttribute("The keyset is not defined.")]
		public const int NTE_KEYSET_NOT_DEF = -2146893799;

		[HRESULT.DescriptionAttribute("Keyset as registered is invalid.")]
		public const int NTE_KEYSET_ENTRY_BAD = -2146893798;

		[HRESULT.DescriptionAttribute("Provider type does not match registered value.")]
		public const int NTE_PROV_TYPE_NO_MATCH = -2146893797;

		[HRESULT.DescriptionAttribute("The digital signature file is corrupt.")]
		public const int NTE_SIGNATURE_FILE_BAD = -2146893796;

		[HRESULT.DescriptionAttribute("Provider DLL failed to initialize correctly.")]
		public const int NTE_PROVIDER_DLL_FAIL = -2146893795;

		[HRESULT.DescriptionAttribute("Provider DLL could not be found.")]
		public const int NTE_PROV_DLL_NOT_FOUND = -2146893794;

		[HRESULT.DescriptionAttribute("The Keyset parameter is invalid.")]
		public const int NTE_BAD_KEYSET_PARAM = -2146893793;

		[HRESULT.DescriptionAttribute("An internal error occurred.")]
		public const int NTE_FAIL = -2146893792;

		[HRESULT.DescriptionAttribute("A base error occurred.")]
		public const int NTE_SYS_ERR = -2146893791;

		[HRESULT.DescriptionAttribute("Provider could not perform the action since the context was acquired as silent.")]
		public const int NTE_SILENT_CONTEXT = -2146893790;

		[HRESULT.DescriptionAttribute("The security token does not have storage space available for an additional container.")]
		public const int NTE_TOKEN_KEYSET_STORAGE_FULL = -2146893789;

		[HRESULT.DescriptionAttribute("The profile for the user is a temporary profile.")]
		public const int NTE_TEMPORARY_PROFILE = -2146893788;

		[HRESULT.DescriptionAttribute("The key parameters could not be set because the CSP uses fixed parameters.")]
		public const int NTE_FIXEDPARAMETER = -2146893787;

		[HRESULT.DescriptionAttribute("Not enough memory is available to complete this request")]
		public const int SEC_E_INSUFFICIENT_MEMORY = -2146893056;

		[HRESULT.DescriptionAttribute("The handle specified is invalid")]
		public const int SEC_E_INVALID_HANDLE = -2146893055;

		[HRESULT.DescriptionAttribute("The function requested is not supported")]
		public const int SEC_E_UNSUPPORTED_FUNCTION = -2146893054;

		[HRESULT.DescriptionAttribute("The specified target is unknown or unreachable")]
		public const int SEC_E_TARGET_UNKNOWN = -2146893053;

		[HRESULT.DescriptionAttribute("The Local Security Authority cannot be contacted")]
		public const int SEC_E_INTERNAL_ERROR = -2146893052;

		[HRESULT.DescriptionAttribute("The requested security package does not exist")]
		public const int SEC_E_SECPKG_NOT_FOUND = -2146893051;

		[HRESULT.DescriptionAttribute("The caller is not the owner of the desired credentials")]
		public const int SEC_E_NOT_OWNER = -2146893050;

		[HRESULT.DescriptionAttribute("The security package failed to initialize, and cannot be installed")]
		public const int SEC_E_CANNOT_INSTALL = -2146893049;

		[HRESULT.DescriptionAttribute("The token supplied to the function is invalid")]
		public const int SEC_E_INVALID_TOKEN = -2146893048;

		[HRESULT.DescriptionAttribute("The security package is not able to marshall the logon buffer, so the logon attempt has failed")]
		public const int SEC_E_CANNOT_PACK = -2146893047;

		[HRESULT.DescriptionAttribute("The per-message Quality of Protection is not supported by the security package")]
		public const int SEC_E_QOP_NOT_SUPPORTED = -2146893046;

		[HRESULT.DescriptionAttribute("The security context does not allow impersonation of the client")]
		public const int SEC_E_NO_IMPERSONATION = -2146893045;

		[HRESULT.DescriptionAttribute("The logon attempt failed")]
		public const int SEC_E_LOGON_DENIED = -2146893044;

		[HRESULT.DescriptionAttribute("The credentials supplied to the package were not recognized")]
		public const int SEC_E_UNKNOWN_CREDENTIALS = -2146893043;

		[HRESULT.DescriptionAttribute("No credentials are available in the security package")]
		public const int SEC_E_NO_CREDENTIALS = -2146893042;

		[HRESULT.DescriptionAttribute("The message or signature supplied for verification has been altered")]
		public const int SEC_E_MESSAGE_ALTERED = -2146893041;

		[HRESULT.DescriptionAttribute("The message supplied for verification is out of sequence")]
		public const int SEC_E_OUT_OF_SEQUENCE = -2146893040;

		[HRESULT.DescriptionAttribute("No authority could be contacted for authentication.")]
		public const int SEC_E_NO_AUTHENTICATING_AUTHORITY = -2146893039;

		[HRESULT.DescriptionAttribute("The function completed successfully, but must be called again to complete the context")]
		public const int SEC_I_CONTINUE_NEEDED = 590610;

		[HRESULT.DescriptionAttribute("The function completed successfully, but CompleteToken must be called")]
		public const int SEC_I_COMPLETE_NEEDED = 590611;

		[HRESULT.DescriptionAttribute("The function completed successfully, but both CompleteToken and this function must be called to complete the context")]
		public const int SEC_I_COMPLETE_AND_CONTINUE = 590612;

		[HRESULT.DescriptionAttribute("The logon was completed, but no network authority was available. The logon was made using locally known information")]
		public const int SEC_I_LOCAL_LOGON = 590613;

		[HRESULT.DescriptionAttribute("The requested security package does not exist")]
		public const int SEC_E_BAD_PKGID = -2146893034;

		[HRESULT.DescriptionAttribute("The context has expired and can no longer be used.")]
		public const int SEC_E_CONTEXT_EXPIRED = -2146893033;

		[HRESULT.DescriptionAttribute("The context has expired and can no longer be used.")]
		public const int SEC_I_CONTEXT_EXPIRED = 590615;

		[HRESULT.DescriptionAttribute("The supplied message is incomplete.  The signature was not verified.")]
		public const int SEC_E_INCOMPLETE_MESSAGE = -2146893032;

		[HRESULT.DescriptionAttribute("The credentials supplied were not complete, and could not be verified. The context could not be initialized.")]
		public const int SEC_E_INCOMPLETE_CREDENTIALS = -2146893024;

		[HRESULT.DescriptionAttribute("The buffers supplied to a function was too small.")]
		public const int SEC_E_BUFFER_TOO_SMALL = -2146893023;

		[HRESULT.DescriptionAttribute("The credentials supplied were not complete, and could not be verified. Additional information can be returned from the context.")]
		public const int SEC_I_INCOMPLETE_CREDENTIALS = 590624;

		[HRESULT.DescriptionAttribute("The context data must be renegotiated with the peer.")]
		public const int SEC_I_RENEGOTIATE = 590625;

		[HRESULT.DescriptionAttribute("The target principal name is incorrect.")]
		public const int SEC_E_WRONG_PRINCIPAL = -2146893022;

		[HRESULT.DescriptionAttribute("There is no LSA mode context associated with this context.")]
		public const int SEC_I_NO_LSA_CONTEXT = 590627;

		[HRESULT.DescriptionAttribute("The clocks on the client and server machines are skewed.")]
		public const int SEC_E_TIME_SKEW = -2146893020;

		[HRESULT.DescriptionAttribute("The certificate chain was issued by an authority that is not trusted.")]
		public const int SEC_E_UNTRUSTED_ROOT = -2146893019;

		[HRESULT.DescriptionAttribute("The message received was unexpected or badly formatted.")]
		public const int SEC_E_ILLEGAL_MESSAGE = -2146893018;

		[HRESULT.DescriptionAttribute("An unknown error occurred while processing the certificate.")]
		public const int SEC_E_CERT_UNKNOWN = -2146893017;

		[HRESULT.DescriptionAttribute("The received certificate has expired.")]
		public const int SEC_E_CERT_EXPIRED = -2146893016;

		[HRESULT.DescriptionAttribute("The specified data could not be encrypted.")]
		public const int SEC_E_ENCRYPT_FAILURE = -2146893015;

		[HRESULT.DescriptionAttribute("The specified data could not be decrypted.")]
		public const int SEC_E_DECRYPT_FAILURE = -2146893008;

		[HRESULT.DescriptionAttribute("The client and server cannot communicate, because they do not possess a common algorithm.")]
		public const int SEC_E_ALGORITHM_MISMATCH = -2146893007;

		[HRESULT.DescriptionAttribute("The security context could not be established due to a failure in the requested quality of service (e.g. mutual authentication or delegation).")]
		public const int SEC_E_SECURITY_QOS_FAILED = -2146893006;

		[HRESULT.DescriptionAttribute("A security context was deleted before the context was completed.  This is considered a logon failure.")]
		public const int SEC_E_UNFINISHED_CONTEXT_DELETED = -2146893005;

		[HRESULT.DescriptionAttribute("The client is trying to negotiate a context and the server requires user-to-user but didn't send a TGT reply.")]
		public const int SEC_E_NO_TGT_REPLY = -2146893004;

		[HRESULT.DescriptionAttribute("Unable to accomplish the requested task because the local machine does not have any IP addresses.")]
		public const int SEC_E_NO_IP_ADDRESSES = -2146893003;

		[HRESULT.DescriptionAttribute("The supplied credential handle does not match the credential associated with the security context.")]
		public const int SEC_E_WRONG_CREDENTIAL_HANDLE = -2146893002;

		[HRESULT.DescriptionAttribute("The crypto system or checksum function is invalid because a required function is unavailable.")]
		public const int SEC_E_CRYPTO_SYSTEM_INVALID = -2146893001;

		[HRESULT.DescriptionAttribute("The number of maximum ticket referrals has been exceeded.")]
		public const int SEC_E_MAX_REFERRALS_EXCEEDED = -2146893000;

		[HRESULT.DescriptionAttribute("The local machine must be a Kerberos KDC (domain controller) and it is not.")]
		public const int SEC_E_MUST_BE_KDC = -2146892999;

		[HRESULT.DescriptionAttribute("The other end of the security negotiation is requires strong crypto but it is not supported on the local machine.")]
		public const int SEC_E_STRONG_CRYPTO_NOT_SUPPORTED = -2146892998;

		[HRESULT.DescriptionAttribute("The KDC reply contained more than one principal name.")]
		public const int SEC_E_TOO_MANY_PRINCIPALS = -2146892997;

		[HRESULT.DescriptionAttribute("Expected to find PA data for a hint of what etype to use, but it was not found.")]
		public const int SEC_E_NO_PA_DATA = -2146892996;

		[HRESULT.DescriptionAttribute("The client certificate does not contain a valid UPN, or does not match the client name in the logon request.  Please contact your administrator.")]
		public const int SEC_E_PKINIT_NAME_MISMATCH = -2146892995;

		[HRESULT.DescriptionAttribute("Smartcard logon is required and was not used.")]
		public const int SEC_E_SMARTCARD_LOGON_REQUIRED = -2146892994;

		[HRESULT.DescriptionAttribute("A system shutdown is in progress.")]
		public const int SEC_E_SHUTDOWN_IN_PROGRESS = -2146892993;

		[HRESULT.DescriptionAttribute("An invalid request was sent to the KDC.")]
		public const int SEC_E_KDC_INVALID_REQUEST = -2146892992;

		[HRESULT.DescriptionAttribute("The KDC was unable to generate a referral for the service requested.")]
		public const int SEC_E_KDC_UNABLE_TO_REFER = -2146892991;

		[HRESULT.DescriptionAttribute("The encryption type requested is not supported by the KDC.")]
		public const int SEC_E_KDC_UNKNOWN_ETYPE = -2146892990;

		[HRESULT.DescriptionAttribute("An unsupported preauthentication mechanism was presented to the kerberos package.")]
		public const int SEC_E_UNSUPPORTED_PREAUTH = -2146892989;

		[HRESULT.DescriptionAttribute("The requested operation cannot be completed.  The computer must be trusted for delegation and the current user account must be configured to allow delegation.")]
		public const int SEC_E_DELEGATION_REQUIRED = -2146892987;

		[HRESULT.DescriptionAttribute("Client's supplied SSPI channel bindings were incorrect.")]
		public const int SEC_E_BAD_BINDINGS = -2146892986;

		[HRESULT.DescriptionAttribute("The received certificate was mapped to multiple accounts.")]
		public const int SEC_E_MULTIPLE_ACCOUNTS = -2146892985;

		[HRESULT.DescriptionAttribute("SEC_E_NO_KERB_KEY")]
		public const int SEC_E_NO_KERB_KEY = -2146892984;

		[HRESULT.DescriptionAttribute("The certificate is not valid for the requested usage.")]
		public const int SEC_E_CERT_WRONG_USAGE = -2146892983;

		[HRESULT.DescriptionAttribute("The system detected a possible attempt to compromise security.  Please ensure that you can contact the server that authenticated you.")]
		public const int SEC_E_DOWNGRADE_DETECTED = -2146892976;

		[HRESULT.DescriptionAttribute("The smartcard certificate used for authentication has been revoked. Please contact your system administrator.  There may be additional information in the event log.")]
		public const int SEC_E_SMARTCARD_CERT_REVOKED = -2146892975;

		[HRESULT.DescriptionAttribute("An untrusted certificate authority was detected While processing the smartcard certificate used for authentication.  Please contact your system administrator.")]
		public const int SEC_E_ISSUING_CA_UNTRUSTED = -2146892974;

		[HRESULT.DescriptionAttribute("The revocation status of the smartcard certificate used for authentication could not be determined. Please contact your system administrator.")]
		public const int SEC_E_REVOCATION_OFFLINE_C = -2146892973;

		[HRESULT.DescriptionAttribute("The smartcard certificate used for authentication was not trusted.  Please contact your system administrator.")]
		public const int SEC_E_PKINIT_CLIENT_FAILURE = -2146892972;

		[HRESULT.DescriptionAttribute("The smartcard certificate used for authentication has expired.  Please contact your system administrator.")]
		public const int SEC_E_SMARTCARD_CERT_EXPIRED = -2146892971;

		[HRESULT.DescriptionAttribute("The Kerberos subsystem encountered an error.  A service for user protocol request was made against a domain controller which does not support service for user.")]
		public const int SEC_E_NO_S4U_PROT_SUPPORT = -2146892970;

		[HRESULT.DescriptionAttribute("An attempt was made by this server to make a Kerberos constrained delegation request for a target outside of the server's realm.  This is not supported, and indicates a misconfiguration on this server's allowed to delegate to list.  Please contact your administrator.")]
		public const int SEC_E_CROSSREALM_DELEGATION_FAILURE = -2146892969;

		public const int SEC_E_NO_SPM = -2146893052;

		public const int SEC_E_NOT_SUPPORTED = -2146893054;

		[HRESULT.DescriptionAttribute("An error occurred while performing an operation on a cryptographic message.")]
		public const int CRYPT_E_MSG_ERROR = -2146889727;

		[HRESULT.DescriptionAttribute("Unknown cryptographic algorithm.")]
		public const int CRYPT_E_UNKNOWN_ALGO = -2146889726;

		[HRESULT.DescriptionAttribute("The object identifier is poorly formatted.")]
		public const int CRYPT_E_OID_FORMAT = -2146889725;

		[HRESULT.DescriptionAttribute("Invalid cryptographic message type.")]
		public const int CRYPT_E_INVALID_MSG_TYPE = -2146889724;

		[HRESULT.DescriptionAttribute("Unexpected cryptographic message encoding.")]
		public const int CRYPT_E_UNEXPECTED_ENCODING = -2146889723;

		[HRESULT.DescriptionAttribute("The cryptographic message does not contain an expected authenticated attribute.")]
		public const int CRYPT_E_AUTH_ATTR_MISSING = -2146889722;

		[HRESULT.DescriptionAttribute("The hash value is not correct.")]
		public const int CRYPT_E_HASH_VALUE = -2146889721;

		[HRESULT.DescriptionAttribute("The index value is not valid.")]
		public const int CRYPT_E_INVALID_INDEX = -2146889720;

		[HRESULT.DescriptionAttribute("The content of the cryptographic message has already been decrypted.")]
		public const int CRYPT_E_ALREADY_DECRYPTED = -2146889719;

		[HRESULT.DescriptionAttribute("The content of the cryptographic message has not been decrypted yet.")]
		public const int CRYPT_E_NOT_DECRYPTED = -2146889718;

		[HRESULT.DescriptionAttribute("The enveloped-data message does not contain the specified recipient.")]
		public const int CRYPT_E_RECIPIENT_NOT_FOUND = -2146889717;

		[HRESULT.DescriptionAttribute("Invalid control type.")]
		public const int CRYPT_E_CONTROL_TYPE = -2146889716;

		[HRESULT.DescriptionAttribute("Invalid issuer and/or serial number.")]
		public const int CRYPT_E_ISSUER_SERIALNUMBER = -2146889715;

		[HRESULT.DescriptionAttribute("Cannot find the original signer.")]
		public const int CRYPT_E_SIGNER_NOT_FOUND = -2146889714;

		[HRESULT.DescriptionAttribute("The cryptographic message does not contain all of the requested attributes.")]
		public const int CRYPT_E_ATTRIBUTES_MISSING = -2146889713;

		[HRESULT.DescriptionAttribute("The streamed cryptographic message is not ready to return data.")]
		public const int CRYPT_E_STREAM_MSG_NOT_READY = -2146889712;

		[HRESULT.DescriptionAttribute("The streamed cryptographic message requires more data to complete the decode operation.")]
		public const int CRYPT_E_STREAM_INSUFFICIENT_DATA = -2146889711;

		[HRESULT.DescriptionAttribute("The protected data needs to be re-protected.")]
		public const int CRYPT_I_NEW_PROTECTION_REQUIRED = 593938;

		[HRESULT.DescriptionAttribute("The length specified for the output data was insufficient.")]
		public const int CRYPT_E_BAD_LEN = -2146885631;

		[HRESULT.DescriptionAttribute("An error occurred during encode or decode operation.")]
		public const int CRYPT_E_BAD_ENCODE = -2146885630;

		[HRESULT.DescriptionAttribute("An error occurred while reading or writing to a file.")]
		public const int CRYPT_E_FILE_ERROR = -2146885629;

		[HRESULT.DescriptionAttribute("Cannot find object or property.")]
		public const int CRYPT_E_NOT_FOUND = -2146885628;

		[HRESULT.DescriptionAttribute("The object or property already exists.")]
		public const int CRYPT_E_EXISTS = -2146885627;

		[HRESULT.DescriptionAttribute("No provider was specified for the store or object.")]
		public const int CRYPT_E_NO_PROVIDER = -2146885626;

		[HRESULT.DescriptionAttribute("The specified certificate is self signed.")]
		public const int CRYPT_E_SELF_SIGNED = -2146885625;

		[HRESULT.DescriptionAttribute("The previous certificate or CRL context was deleted.")]
		public const int CRYPT_E_DELETED_PREV = -2146885624;

		[HRESULT.DescriptionAttribute("Cannot find the requested object.")]
		public const int CRYPT_E_NO_MATCH = -2146885623;

		[HRESULT.DescriptionAttribute("The certificate does not have a property that references a private key.")]
		public const int CRYPT_E_UNEXPECTED_MSG_TYPE = -2146885622;

		[HRESULT.DescriptionAttribute("Cannot find the certificate and private key for decryption.")]
		public const int CRYPT_E_NO_KEY_PROPERTY = -2146885621;

		[HRESULT.DescriptionAttribute("Cannot find the certificate and private key to use for decryption.")]
		public const int CRYPT_E_NO_DECRYPT_CERT = -2146885620;

		[HRESULT.DescriptionAttribute("Not a cryptographic message or the cryptographic message is not formatted correctly.")]
		public const int CRYPT_E_BAD_MSG = -2146885619;

		[HRESULT.DescriptionAttribute("The signed cryptographic message does not have a signer for the specified signer index.")]
		public const int CRYPT_E_NO_SIGNER = -2146885618;

		[HRESULT.DescriptionAttribute("Final closure is pending until additional frees or closes.")]
		public const int CRYPT_E_PENDING_CLOSE = -2146885617;

		[HRESULT.DescriptionAttribute("The certificate is revoked.")]
		public const int CRYPT_E_REVOKED = -2146885616;

		[HRESULT.DescriptionAttribute("No Dll or exported function was found to verify revocation.")]
		public const int CRYPT_E_NO_REVOCATION_DLL = -2146885615;

		[HRESULT.DescriptionAttribute("The revocation function was unable to check revocation for the certificate.")]
		public const int CRYPT_E_NO_REVOCATION_CHECK = -2146885614;

		[HRESULT.DescriptionAttribute("The revocation function was unable to check revocation because the revocation server was offline.")]
		public const int CRYPT_E_REVOCATION_OFFLINE = -2146885613;

		[HRESULT.DescriptionAttribute("The certificate is not in the revocation server's database.")]
		public const int CRYPT_E_NOT_IN_REVOCATION_DATABASE = -2146885612;

		[HRESULT.DescriptionAttribute("The string contains a non-numeric character.")]
		public const int CRYPT_E_INVALID_NUMERIC_STRING = -2146885600;

		[HRESULT.DescriptionAttribute("The string contains a non-printable character.")]
		public const int CRYPT_E_INVALID_PRINTABLE_STRING = -2146885599;

		[HRESULT.DescriptionAttribute("The string contains a character not in the 7 bit ASCII character set.")]
		public const int CRYPT_E_INVALID_IA5_STRING = -2146885598;

		[HRESULT.DescriptionAttribute("The string contains an invalid X500 name attribute key, oid, value or delimiter.")]
		public const int CRYPT_E_INVALID_X500_STRING = -2146885597;

		[HRESULT.DescriptionAttribute("The dwValueType for the CERT_NAME_VALUE is not one of the character strings.  Most likely it is either a CERT_RDN_ENCODED_BLOB or CERT_TDN_OCTED_STRING.")]
		public const int CRYPT_E_NOT_CHAR_STRING = -2146885596;

		[HRESULT.DescriptionAttribute("The Put operation can not continue.  The file needs to be resized.  However, there is already a signature present.  A complete signing operation must be done.")]
		public const int CRYPT_E_FILERESIZED = -2146885595;

		[HRESULT.DescriptionAttribute("The cryptographic operation failed due to a local security option setting.")]
		public const int CRYPT_E_SECURITY_SETTINGS = -2146885594;

		[HRESULT.DescriptionAttribute("No DLL or exported function was found to verify subject usage.")]
		public const int CRYPT_E_NO_VERIFY_USAGE_DLL = -2146885593;

		[HRESULT.DescriptionAttribute("The called function was unable to do a usage check on the subject.")]
		public const int CRYPT_E_NO_VERIFY_USAGE_CHECK = -2146885592;

		[HRESULT.DescriptionAttribute("Since the server was offline, the called function was unable to complete the usage check.")]
		public const int CRYPT_E_VERIFY_USAGE_OFFLINE = -2146885591;

		[HRESULT.DescriptionAttribute("The subject was not found in a Certificate Trust List (CTL).")]
		public const int CRYPT_E_NOT_IN_CTL = -2146885590;

		[HRESULT.DescriptionAttribute("None of the signers of the cryptographic message or certificate trust list is trusted.")]
		public const int CRYPT_E_NO_TRUSTED_SIGNER = -2146885589;

		[HRESULT.DescriptionAttribute("The public key's algorithm parameters are missing.")]
		public const int CRYPT_E_MISSING_PUBKEY_PARA = -2146885588;

		[HRESULT.DescriptionAttribute("OSS Certificate encode/decode error code base")]
		public const int CRYPT_E_OSS_ERROR = -2146881536;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Output Buffer is too small.")]
		public const int OSS_MORE_BUF = -2146881535;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Signed integer is encoded as a unsigned integer.")]
		public const int OSS_NEGATIVE_UINTEGER = -2146881534;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Unknown ASN.1 data type.")]
		public const int OSS_PDU_RANGE = -2146881533;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Output buffer is too small, the decoded data has been truncated.")]
		public const int OSS_MORE_INPUT = -2146881532;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_DATA_ERROR = -2146881531;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid argument.")]
		public const int OSS_BAD_ARG = -2146881530;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Encode/Decode version mismatch.")]
		public const int OSS_BAD_VERSION = -2146881529;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Out of memory.")]
		public const int OSS_OUT_MEMORY = -2146881528;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Encode/Decode Error.")]
		public const int OSS_PDU_MISMATCH = -2146881527;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Internal Error.")]
		public const int OSS_LIMITED = -2146881526;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_BAD_PTR = -2146881525;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_BAD_TIME = -2146881524;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Unsupported BER indefinite-length encoding.")]
		public const int OSS_INDEFINITE_NOT_SUPPORTED = -2146881523;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Access violation.")]
		public const int OSS_MEM_ERROR = -2146881522;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_BAD_TABLE = -2146881521;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_TOO_LONG = -2146881520;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_CONSTRAINT_VIOLATED = -2146881519;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Internal Error.")]
		public const int OSS_FATAL_ERROR = -2146881518;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Multi-threading conflict.")]
		public const int OSS_ACCESS_SERIALIZATION_ERROR = -2146881517;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_NULL_TBL = -2146881516;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_NULL_FCN = -2146881515;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_BAD_ENCRULES = -2146881514;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Encode/Decode function not implemented.")]
		public const int OSS_UNAVAIL_ENCRULES = -2146881513;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Trace file error.")]
		public const int OSS_CANT_OPEN_TRACE_WINDOW = -2146881512;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Function not implemented.")]
		public const int OSS_UNIMPLEMENTED = -2146881511;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_OID_DLL_NOT_LINKED = -2146881510;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Trace file error.")]
		public const int OSS_CANT_OPEN_TRACE_FILE = -2146881509;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Trace file error.")]
		public const int OSS_TRACE_FILE_ALREADY_OPEN = -2146881508;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_TABLE_MISMATCH = -2146881507;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Invalid data.")]
		public const int OSS_TYPE_NOT_SUPPORTED = -2146881506;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_REAL_DLL_NOT_LINKED = -2146881505;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_REAL_CODE_NOT_LINKED = -2146881504;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_OUT_OF_RANGE = -2146881503;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_COPIER_DLL_NOT_LINKED = -2146881502;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_CONSTRAINT_DLL_NOT_LINKED = -2146881501;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_COMPARATOR_DLL_NOT_LINKED = -2146881500;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_COMPARATOR_CODE_NOT_LINKED = -2146881499;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_MEM_MGR_DLL_NOT_LINKED = -2146881498;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_PDV_DLL_NOT_LINKED = -2146881497;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_PDV_CODE_NOT_LINKED = -2146881496;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_API_DLL_NOT_LINKED = -2146881495;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_BERDER_DLL_NOT_LINKED = -2146881494;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_PER_DLL_NOT_LINKED = -2146881493;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Program link error.")]
		public const int OSS_OPEN_TYPE_ERROR = -2146881492;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: System resource error.")]
		public const int OSS_MUTEX_NOT_CREATED = -2146881491;

		[HRESULT.DescriptionAttribute("OSS ASN.1 Error: Trace file error.")]
		public const int OSS_CANT_CLOSE_TRACE_FILE = -2146881490;

		[HRESULT.DescriptionAttribute("ASN1 Certificate encode/decode error code base.")]
		public const int CRYPT_E_ASN1_ERROR = -2146881280;

		[HRESULT.DescriptionAttribute("ASN1 internal encode or decode error.")]
		public const int CRYPT_E_ASN1_INTERNAL = -2146881279;

		[HRESULT.DescriptionAttribute("ASN1 unexpected end of data.")]
		public const int CRYPT_E_ASN1_EOD = -2146881278;

		[HRESULT.DescriptionAttribute("ASN1 corrupted data.")]
		public const int CRYPT_E_ASN1_CORRUPT = -2146881277;

		[HRESULT.DescriptionAttribute("ASN1 value too large.")]
		public const int CRYPT_E_ASN1_LARGE = -2146881276;

		[HRESULT.DescriptionAttribute("ASN1 constraint violated.")]
		public const int CRYPT_E_ASN1_CONSTRAINT = -2146881275;

		[HRESULT.DescriptionAttribute("ASN1 out of memory.")]
		public const int CRYPT_E_ASN1_MEMORY = -2146881274;

		[HRESULT.DescriptionAttribute("ASN1 buffer overflow.")]
		public const int CRYPT_E_ASN1_OVERFLOW = -2146881273;

		[HRESULT.DescriptionAttribute("ASN1 function not supported for this PDU.")]
		public const int CRYPT_E_ASN1_BADPDU = -2146881272;

		[HRESULT.DescriptionAttribute("ASN1 bad arguments to function call.")]
		public const int CRYPT_E_ASN1_BADARGS = -2146881271;

		[HRESULT.DescriptionAttribute("ASN1 bad real value.")]
		public const int CRYPT_E_ASN1_BADREAL = -2146881270;

		[HRESULT.DescriptionAttribute("ASN1 bad tag value met.")]
		public const int CRYPT_E_ASN1_BADTAG = -2146881269;

		[HRESULT.DescriptionAttribute("ASN1 bad choice value.")]
		public const int CRYPT_E_ASN1_CHOICE = -2146881268;

		[HRESULT.DescriptionAttribute("ASN1 bad encoding rule.")]
		public const int CRYPT_E_ASN1_RULE = -2146881267;

		[HRESULT.DescriptionAttribute("ASN1 bad unicode (UTF8).")]
		public const int CRYPT_E_ASN1_UTF8 = -2146881266;

		[HRESULT.DescriptionAttribute("ASN1 bad PDU type.")]
		public const int CRYPT_E_ASN1_PDU_TYPE = -2146881229;

		[HRESULT.DescriptionAttribute("ASN1 not yet implemented.")]
		public const int CRYPT_E_ASN1_NYI = -2146881228;

		[HRESULT.DescriptionAttribute("ASN1 skipped unknown extension(s).")]
		public const int CRYPT_E_ASN1_EXTENDED = -2146881023;

		[HRESULT.DescriptionAttribute("ASN1 end of data expected")]
		public const int CRYPT_E_ASN1_NOEOD = -2146881022;

		[HRESULT.DescriptionAttribute("The request subject name is invalid or too long.")]
		public const int CERTSRV_E_BAD_REQUESTSUBJECT = -2146877439;

		[HRESULT.DescriptionAttribute("The request does not exist.")]
		public const int CERTSRV_E_NO_REQUEST = -2146877438;

		[HRESULT.DescriptionAttribute("The request's current status does not allow this operation.")]
		public const int CERTSRV_E_BAD_REQUESTSTATUS = -2146877437;

		[HRESULT.DescriptionAttribute("The requested property value is empty.")]
		public const int CERTSRV_E_PROPERTY_EMPTY = -2146877436;

		[HRESULT.DescriptionAttribute("The certification authority's certificate contains invalid data.")]
		public const int CERTSRV_E_INVALID_CA_CERTIFICATE = -2146877435;

		[HRESULT.DescriptionAttribute("Certificate service has been suspended for a database restore operation.")]
		public const int CERTSRV_E_SERVER_SUSPENDED = -2146877434;

		[HRESULT.DescriptionAttribute("The certificate contains an encoded length that is potentially incompatible with older enrollment software.")]
		public const int CERTSRV_E_ENCODING_LENGTH = -2146877433;

		[HRESULT.DescriptionAttribute("The operation is denied. The user has multiple roles assigned and the certification authority is configured to enforce role separation.")]
		public const int CERTSRV_E_ROLECONFLICT = -2146877432;

		[HRESULT.DescriptionAttribute("The operation is denied. It can only be performed by a certificate manager that is allowed to manage certificates for the current requester.")]
		public const int CERTSRV_E_RESTRICTEDOFFICER = -2146877431;

		[HRESULT.DescriptionAttribute("Cannot archive private key.  The certification authority is not configured for key archival.")]
		public const int CERTSRV_E_KEY_ARCHIVAL_NOT_CONFIGURED = -2146877430;

		[HRESULT.DescriptionAttribute("Cannot archive private key.  The certification authority could not verify one or more key recovery certificates.")]
		public const int CERTSRV_E_NO_VALID_KRA = -2146877429;

		[HRESULT.DescriptionAttribute("The request is incorrectly formatted.  The encrypted private key must be in an unauthenticated attribute in an outermost signature.")]
		public const int CERTSRV_E_BAD_REQUEST_KEY_ARCHIVAL = -2146877428;

		[HRESULT.DescriptionAttribute("At least one security principal must have the permission to manage this CA.")]
		public const int CERTSRV_E_NO_CAADMIN_DEFINED = -2146877427;

		[HRESULT.DescriptionAttribute("The request contains an invalid renewal certificate attribute.")]
		public const int CERTSRV_E_BAD_RENEWAL_CERT_ATTRIBUTE = -2146877426;

		[HRESULT.DescriptionAttribute("An attempt was made to open a Certification Authority database session, but there are already too many active sessions.  The server may need to be configured to allow additional sessions.")]
		public const int CERTSRV_E_NO_DB_SESSIONS = -2146877425;

		[HRESULT.DescriptionAttribute("A memory reference caused a data alignment fault.")]
		public const int CERTSRV_E_ALIGNMENT_FAULT = -2146877424;

		[HRESULT.DescriptionAttribute("The permissions on this certification authority do not allow the current user to enroll for certificates.")]
		public const int CERTSRV_E_ENROLL_DENIED = -2146877423;

		[HRESULT.DescriptionAttribute("The permissions on the certificate template do not allow the current user to enroll for this type of certificate.")]
		public const int CERTSRV_E_TEMPLATE_DENIED = -2146877422;

		[HRESULT.DescriptionAttribute("The contacted domain controller cannot support signed LDAP traffic.  Update the domain controller or configure Certificate Services to use SSL for Active Directory access.")]
		public const int CERTSRV_E_DOWNLEVEL_DC_SSL_OR_UPGRADE = -2146877421;

		[HRESULT.DescriptionAttribute("The requested certificate template is not supported by this CA.")]
		public const int CERTSRV_E_UNSUPPORTED_CERT_TYPE = -2146875392;

		[HRESULT.DescriptionAttribute("The request contains no certificate template information.")]
		public const int CERTSRV_E_NO_CERT_TYPE = -2146875391;

		[HRESULT.DescriptionAttribute("The request contains conflicting template information.")]
		public const int CERTSRV_E_TEMPLATE_CONFLICT = -2146875390;

		[HRESULT.DescriptionAttribute("The request is missing a required Subject Alternate name extension.")]
		public const int CERTSRV_E_SUBJECT_ALT_NAME_REQUIRED = -2146875389;

		[HRESULT.DescriptionAttribute("The request is missing a required private key for archival by the server.")]
		public const int CERTSRV_E_ARCHIVED_KEY_REQUIRED = -2146875388;

		[HRESULT.DescriptionAttribute("The request is missing a required SMIME capabilities extension.")]
		public const int CERTSRV_E_SMIME_REQUIRED = -2146875387;

		[HRESULT.DescriptionAttribute("The request was made on behalf of a subject other than the caller.  The certificate template must be configured to require at least one signature to authorize the request.")]
		public const int CERTSRV_E_BAD_RENEWAL_SUBJECT = -2146875386;

		[HRESULT.DescriptionAttribute("The request template version is newer than the supported template version.")]
		public const int CERTSRV_E_BAD_TEMPLATE_VERSION = -2146875385;

		[HRESULT.DescriptionAttribute("The template is missing a required signature policy attribute.")]
		public const int CERTSRV_E_TEMPLATE_POLICY_REQUIRED = -2146875384;

		[HRESULT.DescriptionAttribute("The request is missing required signature policy information.")]
		public const int CERTSRV_E_SIGNATURE_POLICY_REQUIRED = -2146875383;

		[HRESULT.DescriptionAttribute("The request is missing one or more required signatures.")]
		public const int CERTSRV_E_SIGNATURE_COUNT = -2146875382;

		[HRESULT.DescriptionAttribute("One or more signatures did not include the required application or issuance policies.  The request is missing one or more required valid signatures.")]
		public const int CERTSRV_E_SIGNATURE_REJECTED = -2146875381;

		[HRESULT.DescriptionAttribute("The request is missing one or more required signature issuance policies.")]
		public const int CERTSRV_E_ISSUANCE_POLICY_REQUIRED = -2146875380;

		[HRESULT.DescriptionAttribute("The UPN is unavailable and cannot be added to the Subject Alternate name.")]
		public const int CERTSRV_E_SUBJECT_UPN_REQUIRED = -2146875379;

		[HRESULT.DescriptionAttribute("The Active Directory GUID is unavailable and cannot be added to the Subject Alternate name.")]
		public const int CERTSRV_E_SUBJECT_DIRECTORY_GUID_REQUIRED = -2146875378;

		[HRESULT.DescriptionAttribute("The DNS name is unavailable and cannot be added to the Subject Alternate name.")]
		public const int CERTSRV_E_SUBJECT_DNS_REQUIRED = -2146875377;

		[HRESULT.DescriptionAttribute("The request includes a private key for archival by the server, but key archival is not enabled for the specified certificate template.")]
		public const int CERTSRV_E_ARCHIVED_KEY_UNEXPECTED = -2146875376;

		[HRESULT.DescriptionAttribute("The public key does not meet the minimum size required by the specified certificate template.")]
		public const int CERTSRV_E_KEY_LENGTH = -2146875375;

		[HRESULT.DescriptionAttribute("The EMail name is unavailable and cannot be added to the Subject or Subject Alternate name.")]
		public const int CERTSRV_E_SUBJECT_EMAIL_REQUIRED = -2146875374;

		[HRESULT.DescriptionAttribute("One or more certificate templates to be enabled on this certification authority could not be found.")]
		public const int CERTSRV_E_UNKNOWN_CERT_TYPE = -2146875373;

		[HRESULT.DescriptionAttribute("The certificate template renewal period is longer than the certificate validity period.  The template should be reconfigured or the CA certificate renewed.")]
		public const int CERTSRV_E_CERT_TYPE_OVERLAP = -2146875372;

		[HRESULT.DescriptionAttribute("The key is not exportable.")]
		public const int XENROLL_E_KEY_NOT_EXPORTABLE = -2146873344;

		[HRESULT.DescriptionAttribute("You cannot add the root CA certificate into your local store.")]
		public const int XENROLL_E_CANNOT_ADD_ROOT_CERT = -2146873343;

		[HRESULT.DescriptionAttribute("The key archival hash attribute was not found in the response.")]
		public const int XENROLL_E_RESPONSE_KA_HASH_NOT_FOUND = -2146873342;

		[HRESULT.DescriptionAttribute("An unexpected key archival hash attribute was found in the response.")]
		public const int XENROLL_E_RESPONSE_UNEXPECTED_KA_HASH = -2146873341;

		[HRESULT.DescriptionAttribute("There is a key archival hash mismatch between the request and the response.")]
		public const int XENROLL_E_RESPONSE_KA_HASH_MISMATCH = -2146873340;

		[HRESULT.DescriptionAttribute("Signing certificate cannot include SMIME extension.")]
		public const int XENROLL_E_KEYSPEC_SMIME_MISMATCH = -2146873339;

		[HRESULT.DescriptionAttribute("A system-level error occurred while verifying trust.")]
		public const int TRUST_E_SYSTEM_ERROR = -2146869247;

		[HRESULT.DescriptionAttribute("The certificate for the signer of the message is invalid or not found.")]
		public const int TRUST_E_NO_SIGNER_CERT = -2146869246;

		[HRESULT.DescriptionAttribute("One of the counter signatures was invalid.")]
		public const int TRUST_E_COUNTER_SIGNER = -2146869245;

		[HRESULT.DescriptionAttribute("The signature of the certificate can not be verified.")]
		public const int TRUST_E_CERT_SIGNATURE = -2146869244;

		[HRESULT.DescriptionAttribute("The timestamp signature and/or certificate could not be verified or is malformed.")]
		public const int TRUST_E_TIME_STAMP = -2146869243;

		[HRESULT.DescriptionAttribute("The digital signature of the object did not verify.")]
		public const int TRUST_E_BAD_DIGEST = -2146869232;

		[HRESULT.DescriptionAttribute("A certificate's basic constraint extension has not been observed.")]
		public const int TRUST_E_BASIC_CONSTRAINTS = -2146869223;

		[HRESULT.DescriptionAttribute("The certificate does not meet or contain the Authenticode financial extensions.")]
		public const int TRUST_E_FINANCIAL_CRITERIA = -2146869218;

		[HRESULT.DescriptionAttribute("Tried to reference a part of the file outside the proper range.")]
		public const int MSSIPOTF_E_OUTOFMEMRANGE = -2146865151;

		[HRESULT.DescriptionAttribute("Could not retrieve an object from the file.")]
		public const int MSSIPOTF_E_CANTGETOBJECT = -2146865150;

		[HRESULT.DescriptionAttribute("Could not find the head table in the file.")]
		public const int MSSIPOTF_E_NOHEADTABLE = -2146865149;

		[HRESULT.DescriptionAttribute("The magic number in the head table is incorrect.")]
		public const int MSSIPOTF_E_BAD_MAGICNUMBER = -2146865148;

		[HRESULT.DescriptionAttribute("The offset table has incorrect values.")]
		public const int MSSIPOTF_E_BAD_OFFSET_TABLE = -2146865147;

		[HRESULT.DescriptionAttribute("Duplicate table tags or tags out of alphabetical order.")]
		public const int MSSIPOTF_E_TABLE_TAGORDER = -2146865146;

		[HRESULT.DescriptionAttribute("A table does not start on a long word boundary.")]
		public const int MSSIPOTF_E_TABLE_LONGWORD = -2146865145;

		[HRESULT.DescriptionAttribute("First table does not appear after header information.")]
		public const int MSSIPOTF_E_BAD_FIRST_TABLE_PLACEMENT = -2146865144;

		[HRESULT.DescriptionAttribute("Two or more tables overlap.")]
		public const int MSSIPOTF_E_TABLES_OVERLAP = -2146865143;

		[HRESULT.DescriptionAttribute("Too many pad bytes between tables or pad bytes are not 0.")]
		public const int MSSIPOTF_E_TABLE_PADBYTES = -2146865142;

		[HRESULT.DescriptionAttribute("File is too small to contain the last table.")]
		public const int MSSIPOTF_E_FILETOOSMALL = -2146865141;

		[HRESULT.DescriptionAttribute("A table checksum is incorrect.")]
		public const int MSSIPOTF_E_TABLE_CHECKSUM = -2146865140;

		[HRESULT.DescriptionAttribute("The file checksum is incorrect.")]
		public const int MSSIPOTF_E_FILE_CHECKSUM = -2146865139;

		[HRESULT.DescriptionAttribute("The signature does not have the correct attributes for the policy.")]
		public const int MSSIPOTF_E_FAILED_POLICY = -2146865136;

		[HRESULT.DescriptionAttribute("The file did not pass the hints check.")]
		public const int MSSIPOTF_E_FAILED_HINTS_CHECK = -2146865135;

		[HRESULT.DescriptionAttribute("The file is not an OpenType file.")]
		public const int MSSIPOTF_E_NOT_OPENTYPE = -2146865134;

		[HRESULT.DescriptionAttribute("Failed on a file operation (open, map, read, write).")]
		public const int MSSIPOTF_E_FILE = -2146865133;

		[HRESULT.DescriptionAttribute("A call to a CryptoAPI function failed.")]
		public const int MSSIPOTF_E_CRYPT = -2146865132;

		[HRESULT.DescriptionAttribute("There is a bad version number in the file.")]
		public const int MSSIPOTF_E_BADVERSION = -2146865131;

		[HRESULT.DescriptionAttribute("The structure of the DSIG table is incorrect.")]
		public const int MSSIPOTF_E_DSIG_STRUCTURE = -2146865130;

		[HRESULT.DescriptionAttribute("A check failed in a partially constant table.")]
		public const int MSSIPOTF_E_PCONST_CHECK = -2146865129;

		[HRESULT.DescriptionAttribute("Some kind of structural error.")]
		public const int MSSIPOTF_E_STRUCTURE = -2146865128;

		[HRESULT.DescriptionAttribute("Unknown trust provider.")]
		public const int TRUST_E_PROVIDER_UNKNOWN = -2146762751;

		[HRESULT.DescriptionAttribute("The trust verification action specified is not supported by the specified trust provider.")]
		public const int TRUST_E_ACTION_UNKNOWN = -2146762750;

		[HRESULT.DescriptionAttribute("The form specified for the subject is not one supported or known by the specified trust provider.")]
		public const int TRUST_E_SUBJECT_FORM_UNKNOWN = -2146762749;

		[HRESULT.DescriptionAttribute("The subject is not trusted for the specified action.")]
		public const int TRUST_E_SUBJECT_NOT_TRUSTED = -2146762748;

		[HRESULT.DescriptionAttribute("Error due to problem in ASN.1 encoding process.")]
		public const int DIGSIG_E_ENCODE = -2146762747;

		[HRESULT.DescriptionAttribute("Error due to problem in ASN.1 decoding process.")]
		public const int DIGSIG_E_DECODE = -2146762746;

		[HRESULT.DescriptionAttribute("Reading / writing Extensions where Attributes are appropriate, and visa versa.")]
		public const int DIGSIG_E_EXTENSIBILITY = -2146762745;

		[HRESULT.DescriptionAttribute("Unspecified cryptographic failure.")]
		public const int DIGSIG_E_CRYPTO = -2146762744;

		[HRESULT.DescriptionAttribute("The size of the data could not be determined.")]
		public const int PERSIST_E_SIZEDEFINITE = -2146762743;

		[HRESULT.DescriptionAttribute("The size of the indefinite-sized data could not be determined.")]
		public const int PERSIST_E_SIZEINDEFINITE = -2146762742;

		[HRESULT.DescriptionAttribute("This object does not read and write self-sizing data.")]
		public const int PERSIST_E_NOTSELFSIZING = -2146762741;

		[HRESULT.DescriptionAttribute("No signature was present in the subject.")]
		public const int TRUST_E_NOSIGNATURE = -2146762496;

		[HRESULT.DescriptionAttribute("A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.")]
		public const int CERT_E_EXPIRED = -2146762495;

		[HRESULT.DescriptionAttribute("The validity periods of the certification chain do not nest correctly.")]
		public const int CERT_E_VALIDITYPERIODNESTING = -2146762494;

		[HRESULT.DescriptionAttribute("A certificate that can only be used as an end-entity is being used as a CA or visa versa.")]
		public const int CERT_E_ROLE = -2146762493;

		[HRESULT.DescriptionAttribute("A path length constraint in the certification chain has been violated.")]
		public const int CERT_E_PATHLENCONST = -2146762492;

		[HRESULT.DescriptionAttribute("A certificate contains an unknown extension that is marked 'critical'.")]
		public const int CERT_E_CRITICAL = -2146762491;

		[HRESULT.DescriptionAttribute("A certificate being used for a purpose other than the ones specified by its CA.")]
		public const int CERT_E_PURPOSE = -2146762490;

		[HRESULT.DescriptionAttribute("A parent of a given certificate in fact did not issue that child certificate.")]
		public const int CERT_E_ISSUERCHAINING = -2146762489;

		[HRESULT.DescriptionAttribute("A certificate is missing or has an empty value for an important field, such as a subject or issuer name.")]
		public const int CERT_E_MALFORMED = -2146762488;

		[HRESULT.DescriptionAttribute("A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider.")]
		public const int CERT_E_UNTRUSTEDROOT = -2146762487;

		[HRESULT.DescriptionAttribute("A certificate chain could not be built to a trusted root authority.")]
		public const int CERT_E_CHAINING = -2146762486;

		[HRESULT.DescriptionAttribute("Generic trust failure.")]
		public const int TRUST_E_FAIL = -2146762485;

		[HRESULT.DescriptionAttribute("A certificate was explicitly revoked by its issuer.")]
		public const int CERT_E_REVOKED = -2146762484;

		[HRESULT.DescriptionAttribute("The certification path terminates with the test root which is not trusted with the current policy settings.")]
		public const int CERT_E_UNTRUSTEDTESTROOT = -2146762483;

		[HRESULT.DescriptionAttribute("The revocation process could not continue - the certificate(s) could not be checked.")]
		public const int CERT_E_REVOCATION_FAILURE = -2146762482;

		[HRESULT.DescriptionAttribute("The certificate's CN name does not match the passed value.")]
		public const int CERT_E_CN_NO_MATCH = -2146762481;

		[HRESULT.DescriptionAttribute("The certificate is not valid for the requested usage.")]
		public const int CERT_E_WRONG_USAGE = -2146762480;

		[HRESULT.DescriptionAttribute("The certificate was explicitly marked as untrusted by the user.")]
		public const int TRUST_E_EXPLICIT_DISTRUST = -2146762479;

		[HRESULT.DescriptionAttribute("A certification chain processed correctly, but one of the CA certificates is not trusted by the policy provider.")]
		public const int CERT_E_UNTRUSTEDCA = -2146762478;

		[HRESULT.DescriptionAttribute("The certificate has invalid policy.")]
		public const int CERT_E_INVALID_POLICY = -2146762477;

		[HRESULT.DescriptionAttribute("The certificate has an invalid name. The name is not included in the permitted list or is explicitly excluded.")]
		public const int CERT_E_INVALID_NAME = -2146762476;

		[HRESULT.DescriptionAttribute("A non-empty line was encountered in the INF before the start of a section.")]
		public const int SPAPI_E_EXPECTED_SECTION_NAME = -2146500608;

		[HRESULT.DescriptionAttribute("A section name marker in the INF is not complete, or does not exist on a line by itself.")]
		public const int SPAPI_E_BAD_SECTION_NAME_LINE = -2146500607;

		[HRESULT.DescriptionAttribute("An INF section was encountered whose name exceeds the maximum section name length.")]
		public const int SPAPI_E_SECTION_NAME_TOO_LONG = -2146500606;

		[HRESULT.DescriptionAttribute("The syntax of the INF is invalid.")]
		public const int SPAPI_E_GENERAL_SYNTAX = -2146500605;

		[HRESULT.DescriptionAttribute("The style of the INF is different than what was requested.")]
		public const int SPAPI_E_WRONG_INF_STYLE = -2146500352;

		[HRESULT.DescriptionAttribute("The required section was not found in the INF.")]
		public const int SPAPI_E_SECTION_NOT_FOUND = -2146500351;

		[HRESULT.DescriptionAttribute("The required line was not found in the INF.")]
		public const int SPAPI_E_LINE_NOT_FOUND = -2146500350;

		[HRESULT.DescriptionAttribute("The files affected by the installation of this file queue have not been backed up for uninstall.")]
		public const int SPAPI_E_NO_BACKUP = -2146500349;

		[HRESULT.DescriptionAttribute("The INF or the device information set or element does not have an associated install class.")]
		public const int SPAPI_E_NO_ASSOCIATED_CLASS = -2146500096;

		[HRESULT.DescriptionAttribute("The INF or the device information set or element does not match the specified install class.")]
		public const int SPAPI_E_CLASS_MISMATCH = -2146500095;

		[HRESULT.DescriptionAttribute("An existing device was found that is a duplicate of the device being manually installed.")]
		public const int SPAPI_E_DUPLICATE_FOUND = -2146500094;

		[HRESULT.DescriptionAttribute("There is no driver selected for the device information set or element.")]
		public const int SPAPI_E_NO_DRIVER_SELECTED = -2146500093;

		[HRESULT.DescriptionAttribute("The requested device registry key does not exist.")]
		public const int SPAPI_E_KEY_DOES_NOT_EXIST = -2146500092;

		[HRESULT.DescriptionAttribute("The device instance name is invalid.")]
		public const int SPAPI_E_INVALID_DEVINST_NAME = -2146500091;

		[HRESULT.DescriptionAttribute("The install class is not present or is invalid.")]
		public const int SPAPI_E_INVALID_CLASS = -2146500090;

		[HRESULT.DescriptionAttribute("The device instance cannot be created because it already exists.")]
		public const int SPAPI_E_DEVINST_ALREADY_EXISTS = -2146500089;

		[HRESULT.DescriptionAttribute("The operation cannot be performed on a device information element that has not been registered.")]
		public const int SPAPI_E_DEVINFO_NOT_REGISTERED = -2146500088;

		[HRESULT.DescriptionAttribute("The device property code is invalid.")]
		public const int SPAPI_E_INVALID_REG_PROPERTY = -2146500087;

		[HRESULT.DescriptionAttribute("The INF from which a driver list is to be built does not exist.")]
		public const int SPAPI_E_NO_INF = -2146500086;

		[HRESULT.DescriptionAttribute("The device instance does not exist in the hardware tree.")]
		public const int SPAPI_E_NO_SUCH_DEVINST = -2146500085;

		[HRESULT.DescriptionAttribute("The icon representing this install class cannot be loaded.")]
		public const int SPAPI_E_CANT_LOAD_CLASS_ICON = -2146500084;

		[HRESULT.DescriptionAttribute("The class installer registry entry is invalid.")]
		public const int SPAPI_E_INVALID_CLASS_INSTALLER = -2146500083;

		[HRESULT.DescriptionAttribute("The class installer has indicated that the default action should be performed for this installation request.")]
		public const int SPAPI_E_DI_DO_DEFAULT = -2146500082;

		[HRESULT.DescriptionAttribute("The operation does not require any files to be copied.")]
		public const int SPAPI_E_DI_NOFILECOPY = -2146500081;

		[HRESULT.DescriptionAttribute("The specified hardware profile does not exist.")]
		public const int SPAPI_E_INVALID_HWPROFILE = -2146500080;

		[HRESULT.DescriptionAttribute("There is no device information element currently selected for this device information set.")]
		public const int SPAPI_E_NO_DEVICE_SELECTED = -2146500079;

		[HRESULT.DescriptionAttribute("The operation cannot be performed because the device information set is locked.")]
		public const int SPAPI_E_DEVINFO_LIST_LOCKED = -2146500078;

		[HRESULT.DescriptionAttribute("The operation cannot be performed because the device information element is locked.")]
		public const int SPAPI_E_DEVINFO_DATA_LOCKED = -2146500077;

		[HRESULT.DescriptionAttribute("The specified path does not contain any applicable device INFs.")]
		public const int SPAPI_E_DI_BAD_PATH = -2146500076;

		[HRESULT.DescriptionAttribute("No class installer parameters have been set for the device information set or element.")]
		public const int SPAPI_E_NO_CLASSINSTALL_PARAMS = -2146500075;

		[HRESULT.DescriptionAttribute("The operation cannot be performed because the file queue is locked.")]
		public const int SPAPI_E_FILEQUEUE_LOCKED = -2146500074;

		[HRESULT.DescriptionAttribute("A service installation section in this INF is invalid.")]
		public const int SPAPI_E_BAD_SERVICE_INSTALLSECT = -2146500073;

		[HRESULT.DescriptionAttribute("There is no class driver list for the device information element.")]
		public const int SPAPI_E_NO_CLASS_DRIVER_LIST = -2146500072;

		[HRESULT.DescriptionAttribute("The installation failed because a function driver was not specified for this device instance.")]
		public const int SPAPI_E_NO_ASSOCIATED_SERVICE = -2146500071;

		[HRESULT.DescriptionAttribute("There is presently no default device interface designated for this interface class.")]
		public const int SPAPI_E_NO_DEFAULT_DEVICE_INTERFACE = -2146500070;

		[HRESULT.DescriptionAttribute("The operation cannot be performed because the device interface is currently active.")]
		public const int SPAPI_E_DEVICE_INTERFACE_ACTIVE = -2146500069;

		[HRESULT.DescriptionAttribute("The operation cannot be performed because the device interface has been removed from the system.")]
		public const int SPAPI_E_DEVICE_INTERFACE_REMOVED = -2146500068;

		[HRESULT.DescriptionAttribute("An interface installation section in this INF is invalid.")]
		public const int SPAPI_E_BAD_INTERFACE_INSTALLSECT = -2146500067;

		[HRESULT.DescriptionAttribute("This interface class does not exist in the system.")]
		public const int SPAPI_E_NO_SUCH_INTERFACE_CLASS = -2146500066;

		[HRESULT.DescriptionAttribute("The reference string supplied for this interface device is invalid.")]
		public const int SPAPI_E_INVALID_REFERENCE_STRING = -2146500065;

		[HRESULT.DescriptionAttribute("The specified machine name does not conform to UNC naming conventions.")]
		public const int SPAPI_E_INVALID_MACHINENAME = -2146500064;

		[HRESULT.DescriptionAttribute("A general remote communication error occurred.")]
		public const int SPAPI_E_REMOTE_COMM_FAILURE = -2146500063;

		[HRESULT.DescriptionAttribute("The machine selected for remote communication is not available at this time.")]
		public const int SPAPI_E_MACHINE_UNAVAILABLE = -2146500062;

		[HRESULT.DescriptionAttribute("The Plug and Play service is not available on the remote machine.")]
		public const int SPAPI_E_NO_CONFIGMGR_SERVICES = -2146500061;

		[HRESULT.DescriptionAttribute("The property page provider registry entry is invalid.")]
		public const int SPAPI_E_INVALID_PROPPAGE_PROVIDER = -2146500060;

		[HRESULT.DescriptionAttribute("The requested device interface is not present in the system.")]
		public const int SPAPI_E_NO_SUCH_DEVICE_INTERFACE = -2146500059;

		[HRESULT.DescriptionAttribute("The device's co-installer has additional work to perform after installation is complete.")]
		public const int SPAPI_E_DI_POSTPROCESSING_REQUIRED = -2146500058;

		[HRESULT.DescriptionAttribute("The device's co-installer is invalid.")]
		public const int SPAPI_E_INVALID_COINSTALLER = -2146500057;

		[HRESULT.DescriptionAttribute("There are no compatible drivers for this device.")]
		public const int SPAPI_E_NO_COMPAT_DRIVERS = -2146500056;

		[HRESULT.DescriptionAttribute("There is no icon that represents this device or device type.")]
		public const int SPAPI_E_NO_DEVICE_ICON = -2146500055;

		[HRESULT.DescriptionAttribute("A logical configuration specified in this INF is invalid.")]
		public const int SPAPI_E_INVALID_INF_LOGCONFIG = -2146500054;

		[HRESULT.DescriptionAttribute("The class installer has denied the request to install or upgrade this device.")]
		public const int SPAPI_E_DI_DONT_INSTALL = -2146500053;

		[HRESULT.DescriptionAttribute("One of the filter drivers installed for this device is invalid.")]
		public const int SPAPI_E_INVALID_FILTER_DRIVER = -2146500052;

		[HRESULT.DescriptionAttribute("The driver selected for this device does not support Windows XP.")]
		public const int SPAPI_E_NON_WINDOWS_NT_DRIVER = -2146500051;

		[HRESULT.DescriptionAttribute("The driver selected for this device does not support Windows.")]
		public const int SPAPI_E_NON_WINDOWS_DRIVER = -2146500050;

		[HRESULT.DescriptionAttribute("The third-party INF does not contain digital signature information.")]
		public const int SPAPI_E_NO_CATALOG_FOR_OEM_INF = -2146500049;

		[HRESULT.DescriptionAttribute("An invalid attempt was made to use a device installation file queue for verification of digital signatures relative to other platforms.")]
		public const int SPAPI_E_DEVINSTALL_QUEUE_NONNATIVE = -2146500048;

		[HRESULT.DescriptionAttribute("The device cannot be disabled.")]
		public const int SPAPI_E_NOT_DISABLEABLE = -2146500047;

		[HRESULT.DescriptionAttribute("The device could not be dynamically removed.")]
		public const int SPAPI_E_CANT_REMOVE_DEVINST = -2146500046;

		[HRESULT.DescriptionAttribute("Cannot copy to specified target.")]
		public const int SPAPI_E_INVALID_TARGET = -2146500045;

		[HRESULT.DescriptionAttribute("Driver is not intended for this platform.")]
		public const int SPAPI_E_DRIVER_NONNATIVE = -2146500044;

		[HRESULT.DescriptionAttribute("Operation not allowed in WOW64.")]
		public const int SPAPI_E_IN_WOW64 = -2146500043;

		[HRESULT.DescriptionAttribute("The operation involving unsigned file copying was rolled back, so that a system restore point could be set.")]
		public const int SPAPI_E_SET_SYSTEM_RESTORE_POINT = -2146500042;

		[HRESULT.DescriptionAttribute("An INF was copied into the Windows INF directory in an improper manner.")]
		public const int SPAPI_E_INCORRECTLY_COPIED_INF = -2146500041;

		[HRESULT.DescriptionAttribute("The Security Configuration Editor (SCE) APIs have been disabled on this Embedded product.")]
		public const int SPAPI_E_SCE_DISABLED = -2146500040;

		[HRESULT.DescriptionAttribute("No installed components were detected.")]
		public const int SPAPI_E_ERROR_NOT_INSTALLED = -2146496512;

		public const int SCARD_S_SUCCESS = 0;

		[HRESULT.DescriptionAttribute("An internal consistency check failed.")]
		public const int SCARD_F_INTERNAL_ERROR = -2146435071;

		[HRESULT.DescriptionAttribute("The action was cancelled by an SCardCancel request.")]
		public const int SCARD_E_CANCELLED = -2146435070;

		[HRESULT.DescriptionAttribute("The supplied handle was invalid.")]
		public const int SCARD_E_INVALID_HANDLE = -2146435069;

		[HRESULT.DescriptionAttribute("One or more of the supplied parameters could not be properly interpreted.")]
		public const int SCARD_E_INVALID_PARAMETER = -2146435068;

		[HRESULT.DescriptionAttribute("Registry startup information is missing or invalid.")]
		public const int SCARD_E_INVALID_TARGET = -2146435067;

		[HRESULT.DescriptionAttribute("Not enough memory available to complete this command.")]
		public const int SCARD_E_NO_MEMORY = -2146435066;

		[HRESULT.DescriptionAttribute("An internal consistency timer has expired.")]
		public const int SCARD_F_WAITED_TOO_LONG = -2146435065;

		[HRESULT.DescriptionAttribute("The data buffer to receive returned data is too small for the returned data.")]
		public const int SCARD_E_INSUFFICIENT_BUFFER = -2146435064;

		[HRESULT.DescriptionAttribute("The specified reader name is not recognized.")]
		public const int SCARD_E_UNKNOWN_READER = -2146435063;

		[HRESULT.DescriptionAttribute("The user-specified timeout value has expired.")]
		public const int SCARD_E_TIMEOUT = -2146435062;

		[HRESULT.DescriptionAttribute("The smart card cannot be accessed because of other connections outstanding.")]
		public const int SCARD_E_SHARING_VIOLATION = -2146435061;

		[HRESULT.DescriptionAttribute("The operation requires a Smart Card, but no Smart Card is currently in the device.")]
		public const int SCARD_E_NO_SMARTCARD = -2146435060;

		[HRESULT.DescriptionAttribute("The specified smart card name is not recognized.")]
		public const int SCARD_E_UNKNOWN_CARD = -2146435059;

		[HRESULT.DescriptionAttribute("The system could not dispose of the media in the requested manner.")]
		public const int SCARD_E_CANT_DISPOSE = -2146435058;

		[HRESULT.DescriptionAttribute("The requested protocols are incompatible with the protocol currently in use with the smart card.")]
		public const int SCARD_E_PROTO_MISMATCH = -2146435057;

		[HRESULT.DescriptionAttribute("The reader or smart card is not ready to accept commands.")]
		public const int SCARD_E_NOT_READY = -2146435056;

		[HRESULT.DescriptionAttribute("One or more of the supplied parameters values could not be properly interpreted.")]
		public const int SCARD_E_INVALID_VALUE = -2146435055;

		[HRESULT.DescriptionAttribute("The action was cancelled by the system, presumably to log off or shut down.")]
		public const int SCARD_E_SYSTEM_CANCELLED = -2146435054;

		[HRESULT.DescriptionAttribute("An internal communications error has been detected.")]
		public const int SCARD_F_COMM_ERROR = -2146435053;

		[HRESULT.DescriptionAttribute("An internal error has been detected, but the source is unknown.")]
		public const int SCARD_F_UNKNOWN_ERROR = -2146435052;

		[HRESULT.DescriptionAttribute("An ATR obtained from the registry is not a valid ATR string.")]
		public const int SCARD_E_INVALID_ATR = -2146435051;

		[HRESULT.DescriptionAttribute("An attempt was made to end a non-existent transaction.")]
		public const int SCARD_E_NOT_TRANSACTED = -2146435050;

		[HRESULT.DescriptionAttribute("The specified reader is not currently available for use.")]
		public const int SCARD_E_READER_UNAVAILABLE = -2146435049;

		[HRESULT.DescriptionAttribute("The operation has been aborted to allow the server application to exit.")]
		public const int SCARD_P_SHUTDOWN = -2146435048;

		[HRESULT.DescriptionAttribute("The PCI Receive buffer was too small.")]
		public const int SCARD_E_PCI_TOO_SMALL = -2146435047;

		[HRESULT.DescriptionAttribute("The reader driver does not meet minimal requirements for support.")]
		public const int SCARD_E_READER_UNSUPPORTED = -2146435046;

		[HRESULT.DescriptionAttribute("The reader driver did not produce a unique reader name.")]
		public const int SCARD_E_DUPLICATE_READER = -2146435045;

		[HRESULT.DescriptionAttribute("The smart card does not meet minimal requirements for support.")]
		public const int SCARD_E_CARD_UNSUPPORTED = -2146435044;

		[HRESULT.DescriptionAttribute("The Smart card resource manager is not running.")]
		public const int SCARD_E_NO_SERVICE = -2146435043;

		[HRESULT.DescriptionAttribute("The Smart card resource manager has shut down.")]
		public const int SCARD_E_SERVICE_STOPPED = -2146435042;

		[HRESULT.DescriptionAttribute("An unexpected card error has occurred.")]
		public const int SCARD_E_UNEXPECTED = -2146435041;

		[HRESULT.DescriptionAttribute("No Primary Provider can be found for the smart card.")]
		public const int SCARD_E_ICC_INSTALLATION = -2146435040;

		[HRESULT.DescriptionAttribute("The requested order of object creation is not supported.")]
		public const int SCARD_E_ICC_CREATEORDER = -2146435039;

		[HRESULT.DescriptionAttribute("This smart card does not support the requested feature.")]
		public const int SCARD_E_UNSUPPORTED_FEATURE = -2146435038;

		[HRESULT.DescriptionAttribute("The identified directory does not exist in the smart card.")]
		public const int SCARD_E_DIR_NOT_FOUND = -2146435037;

		[HRESULT.DescriptionAttribute("The identified file does not exist in the smart card.")]
		public const int SCARD_E_FILE_NOT_FOUND = -2146435036;

		[HRESULT.DescriptionAttribute("The supplied path does not represent a smart card directory.")]
		public const int SCARD_E_NO_DIR = -2146435035;

		[HRESULT.DescriptionAttribute("The supplied path does not represent a smart card file.")]
		public const int SCARD_E_NO_FILE = -2146435034;

		[HRESULT.DescriptionAttribute("Access is denied to this file.")]
		public const int SCARD_E_NO_ACCESS = -2146435033;

		[HRESULT.DescriptionAttribute("The smartcard does not have enough memory to store the information.")]
		public const int SCARD_E_WRITE_TOO_MANY = -2146435032;

		[HRESULT.DescriptionAttribute("There was an error trying to set the smart card file object pointer.")]
		public const int SCARD_E_BAD_SEEK = -2146435031;

		[HRESULT.DescriptionAttribute("The supplied PIN is incorrect.")]
		public const int SCARD_E_INVALID_CHV = -2146435030;

		[HRESULT.DescriptionAttribute("An unrecognized error code was returned from a layered component.")]
		public const int SCARD_E_UNKNOWN_RES_MNG = -2146435029;

		[HRESULT.DescriptionAttribute("The requested certificate does not exist.")]
		public const int SCARD_E_NO_SUCH_CERTIFICATE = -2146435028;

		[HRESULT.DescriptionAttribute("The requested certificate could not be obtained.")]
		public const int SCARD_E_CERTIFICATE_UNAVAILABLE = -2146435027;

		[HRESULT.DescriptionAttribute("Cannot find a smart card reader.")]
		public const int SCARD_E_NO_READERS_AVAILABLE = -2146435026;

		[HRESULT.DescriptionAttribute("A communications error with the smart card has been detected.  Retry the operation.")]
		public const int SCARD_E_COMM_DATA_LOST = -2146435025;

		[HRESULT.DescriptionAttribute("The requested key container does not exist on the smart card.")]
		public const int SCARD_E_NO_KEY_CONTAINER = -2146435024;

		[HRESULT.DescriptionAttribute("The Smart card resource manager is too busy to complete this operation.")]
		public const int SCARD_E_SERVER_TOO_BUSY = -2146435023;

		[HRESULT.DescriptionAttribute("The reader cannot communicate with the smart card, due to ATR configuration conflicts.")]
		public const int SCARD_W_UNSUPPORTED_CARD = -2146434971;

		[HRESULT.DescriptionAttribute("The smart card is not responding to a reset.")]
		public const int SCARD_W_UNRESPONSIVE_CARD = -2146434970;

		[HRESULT.DescriptionAttribute("Power has been removed from the smart card, so that further communication is not possible.")]
		public const int SCARD_W_UNPOWERED_CARD = -2146434969;

		[HRESULT.DescriptionAttribute("The smart card has been reset, so any shared state information is invalid.")]
		public const int SCARD_W_RESET_CARD = -2146434968;

		[HRESULT.DescriptionAttribute("The smart card has been removed, so that further communication is not possible.")]
		public const int SCARD_W_REMOVED_CARD = -2146434967;

		[HRESULT.DescriptionAttribute("Access was denied because of a security violation.")]
		public const int SCARD_W_SECURITY_VIOLATION = -2146434966;

		[HRESULT.DescriptionAttribute("The card cannot be accessed because the wrong PIN was presented.")]
		public const int SCARD_W_WRONG_CHV = -2146434965;

		[HRESULT.DescriptionAttribute("The card cannot be accessed because the maximum number of PIN entry attempts has been reached.")]
		public const int SCARD_W_CHV_BLOCKED = -2146434964;

		[HRESULT.DescriptionAttribute("The end of the smart card file has been reached.")]
		public const int SCARD_W_EOF = -2146434963;

		[HRESULT.DescriptionAttribute("The action was cancelled by the user.")]
		public const int SCARD_W_CANCELLED_BY_USER = -2146434962;

		[HRESULT.DescriptionAttribute("No PIN was presented to the smart card.")]
		public const int SCARD_W_CARD_NOT_AUTHENTICATED = -2146434961;

		[HRESULT.DescriptionAttribute("Errors occurred accessing one or more objects - the ErrorInfo collection may have more detail")]
		public const int COMADMIN_E_OBJECTERRORS = -2146368511;

		[HRESULT.DescriptionAttribute("One or more of the object's properties are missing or invalid")]
		public const int COMADMIN_E_OBJECTINVALID = -2146368510;

		[HRESULT.DescriptionAttribute("The object was not found in the catalog")]
		public const int COMADMIN_E_KEYMISSING = -2146368509;

		[HRESULT.DescriptionAttribute("The object is already registered")]
		public const int COMADMIN_E_ALREADYINSTALLED = -2146368508;

		[HRESULT.DescriptionAttribute("Error occurred writing to the application file")]
		public const int COMADMIN_E_APP_FILE_WRITEFAIL = -2146368505;

		[HRESULT.DescriptionAttribute("Error occurred reading the application file")]
		public const int COMADMIN_E_APP_FILE_READFAIL = -2146368504;

		[HRESULT.DescriptionAttribute("Invalid version number in application file")]
		public const int COMADMIN_E_APP_FILE_VERSION = -2146368503;

		[HRESULT.DescriptionAttribute("The file path is invalid")]
		public const int COMADMIN_E_BADPATH = -2146368502;

		[HRESULT.DescriptionAttribute("The application is already installed")]
		public const int COMADMIN_E_APPLICATIONEXISTS = -2146368501;

		[HRESULT.DescriptionAttribute("The role already exists")]
		public const int COMADMIN_E_ROLEEXISTS = -2146368500;

		[HRESULT.DescriptionAttribute("An error occurred copying the file")]
		public const int COMADMIN_E_CANTCOPYFILE = -2146368499;

		[HRESULT.DescriptionAttribute("One or more users are not valid")]
		public const int COMADMIN_E_NOUSER = -2146368497;

		[HRESULT.DescriptionAttribute("One or more users in the application file are not valid")]
		public const int COMADMIN_E_INVALIDUSERIDS = -2146368496;

		[HRESULT.DescriptionAttribute("The component's CLSID is missing or corrupt")]
		public const int COMADMIN_E_NOREGISTRYCLSID = -2146368495;

		[HRESULT.DescriptionAttribute("The component's progID is missing or corrupt")]
		public const int COMADMIN_E_BADREGISTRYPROGID = -2146368494;

		[HRESULT.DescriptionAttribute("Unable to set required authentication level for update request")]
		public const int COMADMIN_E_AUTHENTICATIONLEVEL = -2146368493;

		[HRESULT.DescriptionAttribute("The identity or password set on the application is not valid")]
		public const int COMADMIN_E_USERPASSWDNOTVALID = -2146368492;

		[HRESULT.DescriptionAttribute("Application file CLSIDs or IIDs do not match corresponding DLLs")]
		public const int COMADMIN_E_CLSIDORIIDMISMATCH = -2146368488;

		[HRESULT.DescriptionAttribute("Interface information is either missing or changed")]
		public const int COMADMIN_E_REMOTEINTERFACE = -2146368487;

		[HRESULT.DescriptionAttribute("DllRegisterServer failed on component install")]
		public const int COMADMIN_E_DLLREGISTERSERVER = -2146368486;

		[HRESULT.DescriptionAttribute("No server file share available")]
		public const int COMADMIN_E_NOSERVERSHARE = -2146368485;

		[HRESULT.DescriptionAttribute("DLL could not be loaded")]
		public const int COMADMIN_E_DLLLOADFAILED = -2146368483;

		[HRESULT.DescriptionAttribute("The registered TypeLib ID is not valid")]
		public const int COMADMIN_E_BADREGISTRYLIBID = -2146368482;

		[HRESULT.DescriptionAttribute("Application install directory not found")]
		public const int COMADMIN_E_APPDIRNOTFOUND = -2146368481;

		[HRESULT.DescriptionAttribute("Errors occurred while in the component registrar")]
		public const int COMADMIN_E_REGISTRARFAILED = -2146368477;

		[HRESULT.DescriptionAttribute("The file does not exist")]
		public const int COMADMIN_E_COMPFILE_DOESNOTEXIST = -2146368476;

		[HRESULT.DescriptionAttribute("The DLL could not be loaded")]
		public const int COMADMIN_E_COMPFILE_LOADDLLFAIL = -2146368475;

		[HRESULT.DescriptionAttribute("GetClassObject failed in the DLL")]
		public const int COMADMIN_E_COMPFILE_GETCLASSOBJ = -2146368474;

		[HRESULT.DescriptionAttribute("The DLL does not support the components listed in the TypeLib")]
		public const int COMADMIN_E_COMPFILE_CLASSNOTAVAIL = -2146368473;

		[HRESULT.DescriptionAttribute("The TypeLib could not be loaded")]
		public const int COMADMIN_E_COMPFILE_BADTLB = -2146368472;

		[HRESULT.DescriptionAttribute("The file does not contain components or component information")]
		public const int COMADMIN_E_COMPFILE_NOTINSTALLABLE = -2146368471;

		[HRESULT.DescriptionAttribute("Changes to this object and its sub-objects have been disabled")]
		public const int COMADMIN_E_NOTCHANGEABLE = -2146368470;

		[HRESULT.DescriptionAttribute("The delete function has been disabled for this object")]
		public const int COMADMIN_E_NOTDELETEABLE = -2146368469;

		[HRESULT.DescriptionAttribute("The server catalog version is not supported")]
		public const int COMADMIN_E_SESSION = -2146368468;

		[HRESULT.DescriptionAttribute("The component move was disallowed, because the source or destination application is either a system application or currently locked against changes")]
		public const int COMADMIN_E_COMP_MOVE_LOCKED = -2146368467;

		[HRESULT.DescriptionAttribute("The component move failed because the destination application no longer exists")]
		public const int COMADMIN_E_COMP_MOVE_BAD_DEST = -2146368466;

		[HRESULT.DescriptionAttribute("The system was unable to register the TypeLib")]
		public const int COMADMIN_E_REGISTERTLB = -2146368464;

		[HRESULT.DescriptionAttribute("This operation can not be performed on the system application")]
		public const int COMADMIN_E_SYSTEMAPP = -2146368461;

		[HRESULT.DescriptionAttribute("The component registrar referenced in this file is not available")]
		public const int COMADMIN_E_COMPFILE_NOREGISTRAR = -2146368460;

		[HRESULT.DescriptionAttribute("A component in the same DLL is already installed")]
		public const int COMADMIN_E_COREQCOMPINSTALLED = -2146368459;

		[HRESULT.DescriptionAttribute("The service is not installed")]
		public const int COMADMIN_E_SERVICENOTINSTALLED = -2146368458;

		[HRESULT.DescriptionAttribute("One or more property settings are either invalid or in conflict with each other")]
		public const int COMADMIN_E_PROPERTYSAVEFAILED = -2146368457;

		[HRESULT.DescriptionAttribute("The object you are attempting to add or rename already exists")]
		public const int COMADMIN_E_OBJECTEXISTS = -2146368456;

		[HRESULT.DescriptionAttribute("The component already exists")]
		public const int COMADMIN_E_COMPONENTEXISTS = -2146368455;

		[HRESULT.DescriptionAttribute("The registration file is corrupt")]
		public const int COMADMIN_E_REGFILE_CORRUPT = -2146368453;

		[HRESULT.DescriptionAttribute("The property value is too large")]
		public const int COMADMIN_E_PROPERTY_OVERFLOW = -2146368452;

		[HRESULT.DescriptionAttribute("Object was not found in registry")]
		public const int COMADMIN_E_NOTINREGISTRY = -2146368450;

		[HRESULT.DescriptionAttribute("This object is not poolable")]
		public const int COMADMIN_E_OBJECTNOTPOOLABLE = -2146368449;

		[HRESULT.DescriptionAttribute("A CLSID with the same GUID as the new application ID is already installed on this machine")]
		public const int COMADMIN_E_APPLID_MATCHES_CLSID = -2146368442;

		[HRESULT.DescriptionAttribute("A role assigned to a component, interface, or method did not exist in the application")]
		public const int COMADMIN_E_ROLE_DOES_NOT_EXIST = -2146368441;

		[HRESULT.DescriptionAttribute("You must have components in an application in order to start the application")]
		public const int COMADMIN_E_START_APP_NEEDS_COMPONENTS = -2146368440;

		[HRESULT.DescriptionAttribute("This operation is not enabled on this platform")]
		public const int COMADMIN_E_REQUIRES_DIFFERENT_PLATFORM = -2146368439;

		[HRESULT.DescriptionAttribute("Application Proxy is not exportable")]
		public const int COMADMIN_E_CAN_NOT_EXPORT_APP_PROXY = -2146368438;

		[HRESULT.DescriptionAttribute("Failed to start application because it is either a library application or an application proxy")]
		public const int COMADMIN_E_CAN_NOT_START_APP = -2146368437;

		[HRESULT.DescriptionAttribute("System application is not exportable")]
		public const int COMADMIN_E_CAN_NOT_EXPORT_SYS_APP = -2146368436;

		[HRESULT.DescriptionAttribute("Can not subscribe to this component (the component may have been imported)")]
		public const int COMADMIN_E_CANT_SUBSCRIBE_TO_COMPONENT = -2146368435;

		[HRESULT.DescriptionAttribute("An event class cannot also be a subscriber component")]
		public const int COMADMIN_E_EVENTCLASS_CANT_BE_SUBSCRIBER = -2146368434;

		[HRESULT.DescriptionAttribute("Library applications and application proxies are incompatible")]
		public const int COMADMIN_E_LIB_APP_PROXY_INCOMPATIBLE = -2146368433;

		[HRESULT.DescriptionAttribute("This function is valid for the base partition only")]
		public const int COMADMIN_E_BASE_PARTITION_ONLY = -2146368432;

		[HRESULT.DescriptionAttribute("You cannot start an application that has been disabled")]
		public const int COMADMIN_E_START_APP_DISABLED = -2146368431;

		[HRESULT.DescriptionAttribute("The specified partition name is already in use on this computer")]
		public const int COMADMIN_E_CAT_DUPLICATE_PARTITION_NAME = -2146368425;

		[HRESULT.DescriptionAttribute("The specified partition name is invalid. Check that the name contains at least one visible character")]
		public const int COMADMIN_E_CAT_INVALID_PARTITION_NAME = -2146368424;

		[HRESULT.DescriptionAttribute("The partition cannot be deleted because it is the default partition for one or more users")]
		public const int COMADMIN_E_CAT_PARTITION_IN_USE = -2146368423;

		[HRESULT.DescriptionAttribute("The partition cannot be exported, because one or more components in the partition have the same file name")]
		public const int COMADMIN_E_FILE_PARTITION_DUPLICATE_FILES = -2146368422;

		[HRESULT.DescriptionAttribute("Applications that contain one or more imported components cannot be installed into a non-base partition")]
		public const int COMADMIN_E_CAT_IMPORTED_COMPONENTS_NOT_ALLOWED = -2146368421;

		[HRESULT.DescriptionAttribute("The application name is not unique and cannot be resolved to an application id")]
		public const int COMADMIN_E_AMBIGUOUS_APPLICATION_NAME = -2146368420;

		[HRESULT.DescriptionAttribute("The partition name is not unique and cannot be resolved to a partition id")]
		public const int COMADMIN_E_AMBIGUOUS_PARTITION_NAME = -2146368419;

		[HRESULT.DescriptionAttribute("The COM+ registry database has not been initialized")]
		public const int COMADMIN_E_REGDB_NOTINITIALIZED = -2146368398;

		[HRESULT.DescriptionAttribute("The COM+ registry database is not open")]
		public const int COMADMIN_E_REGDB_NOTOPEN = -2146368397;

		[HRESULT.DescriptionAttribute("The COM+ registry database detected a system error")]
		public const int COMADMIN_E_REGDB_SYSTEMERR = -2146368396;

		[HRESULT.DescriptionAttribute("The COM+ registry database is already running")]
		public const int COMADMIN_E_REGDB_ALREADYRUNNING = -2146368395;

		[HRESULT.DescriptionAttribute("This version of the COM+ registry database cannot be migrated")]
		public const int COMADMIN_E_MIG_VERSIONNOTSUPPORTED = -2146368384;

		[HRESULT.DescriptionAttribute("The schema version to be migrated could not be found in the COM+ registry database")]
		public const int COMADMIN_E_MIG_SCHEMANOTFOUND = -2146368383;

		[HRESULT.DescriptionAttribute("There was a type mismatch between binaries")]
		public const int COMADMIN_E_CAT_BITNESSMISMATCH = -2146368382;

		[HRESULT.DescriptionAttribute("A binary of unknown or invalid type was provided")]
		public const int COMADMIN_E_CAT_UNACCEPTABLEBITNESS = -2146368381;

		[HRESULT.DescriptionAttribute("There was a type mismatch between a binary and an application")]
		public const int COMADMIN_E_CAT_WRONGAPPBITNESS = -2146368380;

		[HRESULT.DescriptionAttribute("The application cannot be paused or resumed")]
		public const int COMADMIN_E_CAT_PAUSE_RESUME_NOT_SUPPORTED = -2146368379;

		[HRESULT.DescriptionAttribute("The COM+ Catalog Server threw an exception during execution")]
		public const int COMADMIN_E_CAT_SERVERFAULT = -2146368378;

		[HRESULT.DescriptionAttribute("Only COM+ Applications marked \"queued\" can be invoked using the \"queue\" moniker")]
		public const int COMQC_E_APPLICATION_NOT_QUEUED = -2146368000;

		[HRESULT.DescriptionAttribute("At least one interface must be marked \"queued\" in order to create a queued component instance with the \"queue\" moniker")]
		public const int COMQC_E_NO_QUEUEABLE_INTERFACES = -2146367999;

		[HRESULT.DescriptionAttribute("MSMQ is required for the requested operation and is not installed")]
		public const int COMQC_E_QUEUING_SERVICE_NOT_AVAILABLE = -2146367998;

		[HRESULT.DescriptionAttribute("Unable to marshal an interface that does not support IPersistStream")]
		public const int COMQC_E_NO_IPERSISTSTREAM = -2146367997;

		[HRESULT.DescriptionAttribute("The message is improperly formatted or was damaged in transit")]
		public const int COMQC_E_BAD_MESSAGE = -2146367996;

		[HRESULT.DescriptionAttribute("An unauthenticated message was received by an application that accepts only authenticated messages")]
		public const int COMQC_E_UNAUTHENTICATED = -2146367995;

		[HRESULT.DescriptionAttribute("The message was requeued or moved by a user not in the \"QC Trusted User\" role")]
		public const int COMQC_E_UNTRUSTED_ENQUEUER = -2146367994;

		[HRESULT.DescriptionAttribute("Cannot create a duplicate resource of type Distributed Transaction Coordinator")]
		public const int MSDTC_E_DUPLICATE_RESOURCE = -2146367743;

		[HRESULT.DescriptionAttribute("One of the objects being inserted or updated does not belong to a valid parent collection")]
		public const int COMADMIN_E_OBJECT_PARENT_MISSING = -2146367480;

		[HRESULT.DescriptionAttribute("One of the specified objects cannot be found")]
		public const int COMADMIN_E_OBJECT_DOES_NOT_EXIST = -2146367479;

		[HRESULT.DescriptionAttribute("The specified application is not currently running")]
		public const int COMADMIN_E_APP_NOT_RUNNING = -2146367478;

		[HRESULT.DescriptionAttribute("The partition(s) specified are not valid.")]
		public const int COMADMIN_E_INVALID_PARTITION = -2146367477;

		[HRESULT.DescriptionAttribute("COM+ applications that run as NT service may not be pooled or recycled")]
		public const int COMADMIN_E_SVCAPP_NOT_POOLABLE_OR_RECYCLABLE = -2146367475;

		[HRESULT.DescriptionAttribute("One or more users are already assigned to a local partition set.")]
		public const int COMADMIN_E_USER_IN_SET = -2146367474;

		[HRESULT.DescriptionAttribute("Library applications may not be recycled.")]
		public const int COMADMIN_E_CANTRECYCLELIBRARYAPPS = -2146367473;

		[HRESULT.DescriptionAttribute("Applications running as NT services may not be recycled.")]
		public const int COMADMIN_E_CANTRECYCLESERVICEAPPS = -2146367471;

		[HRESULT.DescriptionAttribute("The process has already been recycled.")]
		public const int COMADMIN_E_PROCESSALREADYRECYCLED = -2146367470;

		[HRESULT.DescriptionAttribute("A paused process may not be recycled.")]
		public const int COMADMIN_E_PAUSEDPROCESSMAYNOTBERECYCLED = -2146367469;

		[HRESULT.DescriptionAttribute("Library applications may not be NT services.")]
		public const int COMADMIN_E_CANTMAKEINPROCSERVICE = -2146367468;

		[HRESULT.DescriptionAttribute("The ProgID provided to the copy operation is invalid. The ProgID is in use by another registered CLSID.")]
		public const int COMADMIN_E_PROGIDINUSEBYCLSID = -2146367467;

		[HRESULT.DescriptionAttribute("The partition specified as default is not a member of the partition set.")]
		public const int COMADMIN_E_DEFAULT_PARTITION_NOT_IN_SET = -2146367466;

		[HRESULT.DescriptionAttribute("A recycled process may not be paused.")]
		public const int COMADMIN_E_RECYCLEDPROCESSMAYNOTBEPAUSED = -2146367465;

		[HRESULT.DescriptionAttribute("Access to the specified partition is denied.")]
		public const int COMADMIN_E_PARTITION_ACCESSDENIED = -2146367464;

		[HRESULT.DescriptionAttribute("Only Application Files (*.MSI files) can be installed into partitions.")]
		public const int COMADMIN_E_PARTITION_MSI_ONLY = -2146367463;

		[HRESULT.DescriptionAttribute("Applications containing one or more legacy components may not be exported to 1.0 format.")]
		public const int COMADMIN_E_LEGACYCOMPS_NOT_ALLOWED_IN_1_0_FORMAT = -2146367462;

		[HRESULT.DescriptionAttribute("Legacy components may not exist in non-base partitions.")]
		public const int COMADMIN_E_LEGACYCOMPS_NOT_ALLOWED_IN_NONBASE_PARTITIONS = -2146367461;

		[HRESULT.DescriptionAttribute("A component cannot be moved (or copied) from the System Application, an application proxy or a non-changeable application")]
		public const int COMADMIN_E_COMP_MOVE_SOURCE = -2146367460;

		[HRESULT.DescriptionAttribute("A component cannot be moved (or copied) to the System Application, an application proxy or a non-changeable application")]
		public const int COMADMIN_E_COMP_MOVE_DEST = -2146367459;

		[HRESULT.DescriptionAttribute("A private component cannot be moved (or copied) to a library application or to the base partition")]
		public const int COMADMIN_E_COMP_MOVE_PRIVATE = -2146367458;

		[HRESULT.DescriptionAttribute("The Base Application Partition exists in all partition sets and cannot be removed.")]
		public const int COMADMIN_E_BASEPARTITION_REQUIRED_IN_SET = -2146367457;

		[HRESULT.DescriptionAttribute("Alas, Event Class components cannot be aliased.")]
		public const int COMADMIN_E_CANNOT_ALIAS_EVENTCLASS = -2146367456;

		[HRESULT.DescriptionAttribute("Access is denied because the component is private.")]
		public const int COMADMIN_E_PRIVATE_ACCESSDENIED = -2146367455;

		[HRESULT.DescriptionAttribute("The specified SAFER level is invalid.")]
		public const int COMADMIN_E_SAFERINVALID = -2146367454;

		[HRESULT.DescriptionAttribute("The specified user cannot write to the system registry")]
		public const int COMADMIN_E_REGISTRY_ACCESSDENIED = -2146367453;

		[HRESULT.DescriptionAttribute("COM+ partitions are currently disabled.")]
		public const int COMADMIN_E_PARTITIONS_DISABLED = -2146367452;

		private static HRESULT.DirCodes dirCodes = new HRESULT.DirCodes(1280);

		public class HResultException : SystemException
		{
			public new HRESULT HResult
			{
				get
				{
					return new HRESULT(base.HResult);
				}
				set
				{
					base.HResult = value.m_value;
				}
			}

			public HResultException(int hr)
				: base(new HRESULT(hr).ToString())
			{
				base.HResult = hr;
			}
		}

		[AttributeUsage(AttributeTargets.All)]
		private class DescriptionAttribute : Attribute
		{
			public DescriptionAttribute(string description)
			{
				this.m_description = description;
			}

			public string description
			{
				get
				{
					return this.m_description;
				}
			}

			protected string m_description;
		}

		private class DirCodes : DictionaryBase
		{
			public DirCodes()
			{
			}

			public DirCodes(int capacity)
			{
			}

			public void Add(int key, FieldInfo value)
			{
				base.Dictionary.Add(key, value);
			}

			public FieldInfo this[int key]
			{
				get
				{
					return (FieldInfo)base.Dictionary[key];
				}
				set
				{
					base.Dictionary[key] = value;
				}
			}

			public bool TryGetValue(int key, out FieldInfo value)
			{
				value = this[key];
				return value != null;
			}

			public bool Contains(int key)
			{
				return base.Dictionary.Contains(key);
			}

			public bool ContainsKey(int key)
			{
				return base.InnerHashtable.ContainsKey(key);
			}

			public void CopyTo(FieldInfo[] values, int index)
			{
				base.Dictionary.Values.CopyTo(values, index);
			}

			public ICollection Keys
			{
				get
				{
					return base.Dictionary.Keys;
				}
			}

			public ICollection Values
			{
				get
				{
					return base.Dictionary.Values;
				}
			}
		}
	}
}
