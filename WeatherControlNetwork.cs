using ProtoBuf;

namespace WeatherController
{
    [ProtoContract]
    public class WeatherOptionsRequest
    {
        [ProtoMember(1)]
        public bool ForceReload { get; set; }
    }

    [ProtoContract]
    public class WeatherOptionEntry
    {
        [ProtoMember(1)]
        public string Code { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
    }

    [ProtoContract]
    public class WeatherOptionsPacket
    {
        [ProtoMember(1)]
        public WeatherOptionEntry[] WeatherPatterns { get; set; }

        [ProtoMember(2)]
        public WeatherOptionEntry[] WeatherEvents { get; set; }

        [ProtoMember(3)]
        public WeatherOptionEntry[] WindPatterns { get; set; }

        [ProtoMember(4)]
        public string CurrentPatternCode { get; set; }

        [ProtoMember(5)]
        public string CurrentEventCode { get; set; }

        [ProtoMember(6)]
        public string CurrentWindCode { get; set; }

        [ProtoMember(7)]
        public bool? CurrentEventAllowStop { get; set; }

        [ProtoMember(8)]
        public bool AutoChangeEnabled { get; set; }

        [ProtoMember(9)]
        public float? OverridePrecipitation { get; set; }

        [ProtoMember(10)]
        public double RainCloudDaysOffset { get; set; }

        [ProtoMember(11)]
        public bool HasControlPrivilege { get; set; }

        [ProtoMember(12)]
        public bool RegionAvailable { get; set; }

        [ProtoMember(13)]
        public string Message { get; set; }

        [ProtoMember(14)]
        public WeatherOptionEntry[] TemporalStormModes { get; set; }

        [ProtoMember(15)]
        public string CurrentTemporalStormMode { get; set; }
    }

    public enum WeatherControlAction
    {
        RequestOptions,
        SetRegionPattern,
        SetGlobalPattern,
        SetRegionEvent,
        SetGlobalEvent,
        SetGlobalWind,
        SetAutoChange,
        SetPrecipitationOverride,
        ClearPrecipitationOverride,
        RefreshOptions,
        SetTemporalStormMode
    }

    [ProtoContract]
    public class WeatherControlCommand
    {
        [ProtoMember(1)]
        public WeatherControlAction Action { get; set; }

        [ProtoMember(2)]
        public string Code { get; set; }

        [ProtoMember(3)]
        public bool AllowStop { get; set; }

        [ProtoMember(4)]
        public bool UseAllowStop { get; set; }

        [ProtoMember(5)]
        public bool AutoChangeEnabled { get; set; }

        [ProtoMember(6)]
        public bool UseAutoChange { get; set; }

        [ProtoMember(7)]
        public float PrecipitationOverride { get; set; }

        [ProtoMember(8)]
        public bool UsePrecipitationOverride { get; set; }

        [ProtoMember(9)]
        public bool UpdateInstantly { get; set; }
    }
}
