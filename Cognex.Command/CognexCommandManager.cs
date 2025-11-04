namespace Cognex.Command
{
    using Cognex.Command.CognexCommandPack;
    using Cognex.Command.CognexCommandPack.StringField;
    using CognexCommandPack.Char;
    using CognexCommandPack.Filter;
    using CognexCommandPack.General;
    using CognexCommandPack.GetInformation;
    using CognexCommandPack.Light;
    using CognexCommandPack.Region;
    using CognexCommandPack.Tune;

    public class CognexCommandManager
    {
        //==> !!! 주의 !!! 클래스 이름이랑 프로퍼티 이름을 함부로 바꾸면 안된다.
        public GetConfigLightPower GetConfigLightPower { get; set; }
        public SetConfigLightPower SetConfigLightPower { get; set; }
        public SetConfigLightMode SetConfigLightMode { get; set; }
        public SetConfigRegion SetConfigRegion { get; set; }
        public SetConfigCharSize SetConfigCharSize { get; set; }
        public GetConfigEx GetConfigEx { get; set; }
        public AcquireConfig AcquireConfig { get; set; }
        public TuneConfigEx TuneConfigEx { get; set; }
        public GetConfigTune GetConfigTune { get; set; }
        public InsertFilterOperation InsertFilterOperation { get; set; }
        public GetFilterOperationList GetFilterOperationList { get; set; }
        public SetCustomFilterName SetCustomFilterName { get; set; }
        public RemoveFilterOperationAll RemoveFilterOperationAll { get; set; }
        public SetConfigChecksum SetConfigChecksum { get; set; }
        public SetConfigRetry SetConfigRetry { get; set; }
        public SetConfigMark SetConfigMark { get; set; }
        public SetConfigOrientation SetConfigOrientation { get; set; }
        public ReadConfig ReadConfig { get; set; }
        public SetConfigFieldString SetConfigFieldString { get; set; }
        public CognexRICommand CognexRICommand { get; set; }
        public CognexCommandManager()
        {
            GetConfigLightPower = new GetConfigLightPower();
            SetConfigLightPower = new SetConfigLightPower();
            SetConfigLightMode = new SetConfigLightMode();
            SetConfigRegion = new SetConfigRegion();
            SetConfigCharSize = new SetConfigCharSize();
            GetConfigEx = new GetConfigEx();
            AcquireConfig = new AcquireConfig();
            TuneConfigEx = new TuneConfigEx();
            GetConfigTune = new GetConfigTune();
            InsertFilterOperation = new InsertFilterOperation();
            GetFilterOperationList = new GetFilterOperationList();
            SetCustomFilterName = new SetCustomFilterName();
            RemoveFilterOperationAll = new RemoveFilterOperationAll();
            SetConfigChecksum = new SetConfigChecksum();
            SetConfigRetry = new SetConfigRetry();
            SetConfigMark = new SetConfigMark();
            SetConfigOrientation = new SetConfigOrientation();
            ReadConfig = new ReadConfig();
            SetConfigFieldString = new SetConfigFieldString();
            CognexRICommand = new CognexRICommand();
        }
    }
}
