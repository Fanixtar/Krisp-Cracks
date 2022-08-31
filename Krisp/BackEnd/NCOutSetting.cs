using System;

namespace Krisp.BackEnd
{
	public class NCOutSetting : BaseProfileSetting
	{
		public StateOnlySetting krisp_mic_as_default { get; set; }

		public StateOnlySetting krisp_speaker_as_default { get; set; }

		public StateOnlyBaseSetting open_office_mode { get; set; }

		public BaseProfileSetting room_echo { get; set; }

		public NCBalance minutes_settings { get; set; }
	}
}
