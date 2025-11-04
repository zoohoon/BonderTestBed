using ProberInterfaces.Enum;
using System;
using System.Collections.Generic;
using ProberErrorCode;
using ProberInterfaces.Command;
using RequestInterface;
using ProberInterfaces.Communication.Tester;
using ProberInterfaces.BinData;

namespace ProberInterfaces
{
    public enum ENUMSTB
    {
        UNKNOWN = 0,
        STARTDIE,
        ZUP,
        N_REMAINSEQ,
        Y_REMAINSEQ,
        //REQ_ZUP = 'Z',
        //ONWAFERINFO = 'O',
        //X_YCOORDINATE = 'Q',
        //MOVE_AND_BIN = 'M',
        //UNLOADWAFER = 'L'
    }

    public enum EnumGpibEnable
    {
        DISABLE = 0,
        ENABLE
    }
    public enum EnumGpibCommType
    {
        UNKNOWN = 0x0000,
        TEL = 0x0001,
        TSK = 0x0002,
        EG = 0x0003,
        TSK_SPECIAL = 0x0004,
        OPUS = 0x0005,

        TEL_EMUL = 0x1001,
        TSK_EMUL = 0x1002,
        TSK_SPECIAL_EMUL = 0x1003,
    }

    public enum EnumGpibProbingMode
    {
        INTERNAL = 0x0000,
        EXTERNAL = 0x2000
    }

    public enum GPIBStateEnum
    {
        IDLE = 0,
        RUNNING,
        PAUSED,
        ERROR,
        ABORTED,
        DONE,
        NOTCONNECTED,
        //STARTDIE,
        //Z_UpedState,
        //OnWaferInfoState,
        //X_Y_Coord,
        //NO_RemainSeq,
    }

    public enum GpibStatusFlags : int
    {
        DCAS = 1 << 0,
        DTAS = 1 << 1,
        LACS = 1 << 2,
        TACS = 1 << 3,
        ATN = 1 << 4,
        CIC = 1 << 5,
        REM = 1 << 6,
        LOK = 1 << 7,
        CMPL = 1 << 8,
        RQS = 1 << 11,
        SRQI = 1 << 12,
        END = 1 << 13,
        TIMO = 1 << 14,
        ERR = 1 << 15,
        NONE = 0
    }

    public enum GpibCommunicationActionType
    {
        CONN,   //Connect
        READ,   //Read
        WRT,    //Write
        STB,    //STB
    }

    public interface IGPIBSysParam
    {
        Element<EnumGpibCommType> EnumGpibComType { get; set; }
        Element<bool> ExistPrefixInRetVal { get; set; }
        Element<bool> UseAutoLocationNum { get; set; }
        Element<bool> SpecialGCMDMultiDutMode { get; set; }
        Element<bool> TelGCMDForcedOver8MultiDutMode { get; set; }
        Element<bool> TelGCMDMultiTestYesNoFM { get; set; }
        Element<int> EdgeCorrection { get; set; }
        Element<int> ConsecutiveFailMode { get; set; }
        Element<int> ConsecutiveFailSkipLine { get; set; }
        Element<int> MultiTestLocFor2Chan { get; set; }
        Element<string> TelMultiTestYesNo { get; set; }
        Element<int> TelSendSlotNumberForSmallNCommand { get; set; }
        Element<int> AddSignForLargeA { get; set; }
        Element<bool> IsTestCompleteIncluded { get; set; }// DevParam에서 변경됨. 
    }

  
    public interface IGPIB : IStateModule, ICommunicationable, IUseTesterDriver,
                             IHasSysParameterizable,
                             IHasDevParameterizable
    {
        EnumGpibEnable GetGPIBEnable();
        void SetGPIBEnable(EnumGpibEnable param);

        IGPIBSysParam GPIBSysParam_IParam { get; set; }
        //IParam GPIBTesterCommDevParam_IParam { get; set; }
        CommandRecipe CommandRecipeRef { get; }
        EventCodeEnum Connect();
        EventCodeEnum DisConnect();
        EventCodeEnum ReInitializeAndConnect();
        EventCodeEnum WriteString(string query_command);
        EventCodeEnum WriteSTB(int? command);
        BinAnalysisDataArray AnalyzeBin(string binCode);
        List<CommunicationRequestSet> GpibRequestSetList { get; }
        URUWConnectorBase GetURUWConnector(int id);
        BinType GetBinCalulateType();
        string GetFullSite(long mX, long mY);

        EventCodeEnum ProcessReadObject(string source, out string commandName, out string argu, out CommunicationRequestSet findreqset);

        //long EnumToLong(GpibStatusFlags flags );
        //int EnumToInt(GpibCommunicationActionType commtype);
    }

    [Serializable]
    public class URUWConnectorBase
    {
        public int ID { get; set; }

        public RequestBase WriteValidationRule { get; set; }
        public RequestBase WriteValueConverter { get; set; }
        public RequestBase ReadValueConverter { get; set; }

        public bool IsReadOnly { get; set; }
        public URUWConnectorBase()
        {
            ID = 0;
            IsReadOnly = false;
    }
    }

    public class UWDataBase
    {
        public bool IsValid { get; set; }
        public int ID { get; set; }
        public string value { get; set; }

        public UWDataBase()
        {
            IsValid = false;
            ID = 0;
            value = string.Empty;
        }
    }

}
