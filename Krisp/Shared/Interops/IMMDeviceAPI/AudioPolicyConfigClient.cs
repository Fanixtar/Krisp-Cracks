using System;

namespace Shared.Interops.IMMDeviceAPI
{
	public class AudioPolicyConfigClient
	{
		public HRESULT SetEndpointVisibility(string deviceId, bool isVisible)
		{
			HRESULT hresult;
			try
			{
				hresult = ((IPolicyConfig)new PolicyConfigClient()).SetEndpointVisibility(deviceId, isVisible ? 1 : 0);
			}
			catch (Exception ex)
			{
				hresult = ex.HResult;
			}
			return hresult;
		}

		public HRESULT SetDefaultEndpoint(string deviceId, ERole role = ERole.eMultimedia)
		{
			HRESULT hresult;
			try
			{
				hresult = ((IPolicyConfig)new PolicyConfigClient()).SetDefaultEndpoint(deviceId, role);
			}
			catch (Exception ex)
			{
				hresult = ex.HResult;
			}
			return hresult;
		}
	}
}
