using System;

namespace Shared.Interops
{
	[Flags]
	public enum SPFeature : uint
	{
		Feature_None = 0U,
		Feature_NoiseClean = 1U,
		Feature_Dereverb = 2U,
		Feature_OpenOffice = 4U,
		Feature_Echo = 8U
	}
}
