using System;

namespace Krisp.Core.Internals
{
	internal class PeakMeterData
	{
		internal void resetValues()
		{
			this._absPeak = 0f;
			this._currPeak = 0f;
			this._cpeakCount = 0;
		}

		internal float _absPeak;

		internal float _currPeak;

		internal int _cpeakCount;
	}
}
