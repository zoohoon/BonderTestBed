using ProberErrorCode;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces.PreAligner
{
    public interface IPAManager : IFactoryModule, IModule, IHasSysParameterizable
    {
        List<IPreAligner> PAModules { get; }
    }
    public interface IPreAligner : IFactoryModule, IModule
    {
        EnumPACONNECTIONState ConnectionState { get; }
        int ErrorCode { get; }
        bool IsAttatched { get; set; }
        void Dispose();
        EventCodeEnum IsSubstrateExist(out bool isExist);
        EventCodeEnum SetParam(IParam param);
        EventCodeEnum HoldSubstrate();
        EventCodeEnum ReleaseSubstrate();
        EventCodeEnum MoveTo(double posx, double posy, double angle);
        EventCodeEnum MoveTo(double angle);
        EventCodeEnum DoPreAlign(double angle,bool isBusy=false);
        Task<EventCodeEnum> DoPreAlignAsync(double angle);
        int WaitForPA(int timeOut);
        EventCodeEnum ModuleReset();
        EventCodeEnum ModuleCurPos();
        EventCodeEnum ModuleInit();
        Task<EventCodeEnum> ModuleInitAsync();
        EventCodeEnum UpdateState();        
        EventCodeEnum Rotate(double angle);
        IPAState State { get; }
        EnumPAStatus PAStatus { get; }
        EventCodeEnum SetDeviceSize(SubstrateSizeEnum size, WaferNotchTypeEnum notch);
        EventCodeEnum SetPAStatus(EnumPAStatus enumPAStatus);
    }
    public enum EnumPAStatus { Unknown, Idle, Error}

    public enum EnumPACONNECTIONState
    {
        UNDEFINED = 0,
        DISCONNECTED = 1,
        CONNECTED,
        BUSY,
        ERROR,
    }
    public enum EnumRPAVPACommands
    {
        READ_VER,
        ORG,
        RST,
        GETPOS,
        VACON,
        VACOFF,
        WAF_CHECK_VAC,
        WAF_CHECK_CCD,
        ALIGN,
        ALIGNANGLE,
        TURN,
        DATAWRITE,
        DATAREAD,
        READ_ERR,
        READ_STATUS,
        READ_STAT,
        DEV_RESET,
        SET_WAF_TYPE,
        SET_WAF_SIZE,
        ALIGN_POS,
        MOVEREL,
    }

    public interface IPAState
    {
        bool Busy { get; set; }
        bool OriginDone { get; set; }
        bool OverRun { get; set; }
        bool Error { get; set; }
        bool VacStatus { get; set; }
        bool AlignDone { get; set; }
        bool PAAlignAbort { get; set; }
        void SetState(int state);
    }
}
