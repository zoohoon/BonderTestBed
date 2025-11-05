using System;
using System.Collections.Generic;
using ProberErrorCode;
using RequestInterface;
using ProberInterfaces.Communication.Tester;
using ProberInterfaces.Network;
using ProberInterfaces.BinData;
using LogModule;

namespace ProberInterfaces
{
    public enum TCPIPStateEnum
    {
        IDLE = 0,
        CONNECTED,
        PAUSED,
        ERROR,
        ABORTED,
        DONE,
        NOTCONNECTED,
    }

    public enum EnumTesterDriverState
    {
        UNDEFINED = 0,
        DISCONNECT = 1,
        CONNECTED = 2,
        BEGINRECEIVE = 3,
        BEGINSEND = 4,
        RECEIVED = 5,
        SENDED = 6,
        ERROR = 7
    }

    public enum EnumTCPIPEnable
    {
        DISABLE = 0,
        ENABLE
    }

    public enum EnumTCPIPCommType
    {
        UNDEFINED = 0,
        EMUL,
        REAL
    }

    public interface ITCPIPSysParam
    {
        Element<EnumTCPIPEnable> EnumTCPIPOnOff { get; set; }
        Element<EnumTCPIPCommType> EnumTCPIPComType { get; set; }
        Element<int> SendPort { get; set; }
        Element<int> ReceivePort { get; set; }
        Element<IPAddressVer4> IP { get; set; }

        //Element<string> Terminator { get; set; }

        Element<int> SendDelayTime { get; set; }
    }

    public interface ITCPIP : IStateModule, ICommunicationable, IUseTesterDriver,
                             IHasSysParameterizable,
                             IHasDevParameterizable
    {
        IParam TCPIPSysParam_IParam { get; }
        EnumTCPIPEnable GetTCPIPOnOff();

        EventCodeEnum WriteString(string command);

        List<CommunicationRequestSet> RequestSetList { get; }

        CardchangeTempInfo CardchangeTempInfo { get; set; }
        EventCodeEnum WriteSTB(string command);

        EventCodeEnum ReInitializeAndConnect();

        EventCodeEnum CheckAndConnect();
        EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo);
        BinAnalysisDataArray AnalyzeBin(string binCode);

        DRDWConnectorBase GetDRDWConnector(int id);

        DWDataBase GetDWDataBase(string argument);
    }

    [Serializable]
    public class FoupAllocatedInfo
    {
        public int FoupNumber { get; set; }
        public string DeviceName { get; set; }
        public string LotName { get; set; }

        public FoupAllocatedInfo(int foupnumber, string devicename, string lotname)
        {
            try
            {
                this.FoupNumber = foupnumber;
                this.DeviceName = devicename;
                this.LotName = lotname;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class CardchangeTempInfo
    {
        public bool Result { get; set; }
        public string Cardid { get; set; }

        public CardchangeTempInfo(bool result, string cardid)
        {
            try
            {
                this.Result = result;
                this.Cardid = cardid;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    [Serializable]
    public class DRDWConnectorBase
    {
        public int ID { get; set; }

        public RequestBase WriteValidationRule { get; set; }
        public RequestBase WriteValueConverter { get; set; }
        public RequestBase ReadValueConverter { get; set; }

        public bool IsReadOnly { get; set; }
        public DRDWConnectorBase()
        {
            ID = 0;
            IsReadOnly = false;
        }
    }

    public class DWDataBase
    {
        public bool IsValid { get; set; }
        public int ID { get; set; }
        public string value { get; set; }

        public DWDataBase()
        {
            IsValid = false;
            ID = 0;
            value = string.Empty;
        }
    }


}
