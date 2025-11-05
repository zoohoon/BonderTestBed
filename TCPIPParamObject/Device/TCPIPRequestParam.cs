using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using RequestInterface;
using RequestCore.QueryPack;
using RequestCore.Query.TCPIP;
using NotifyEventModule;
using RequestCore.Query;
using Command.TCPIP;
using RequestCore.ActionPack.TCPIP;
using RequestCore.ActionPack.Internal;

namespace TCPIPParamObject
{
    [Serializable]
    public class TCPIPRequestParam : IDeviceParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[TCPIPRequestParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        [ParamIgnore]
        public string FilePath { get; } = "TCPIP";
        [ParamIgnore]
        public string FileName { get; } = "TCPIPRequestParam.json";
        public string Genealogy { get; set; } = "TCPIPRequestParam";


        private List<CommunicationRequestSet> _RequestSetList = new List<CommunicationRequestSet>();
        public List<CommunicationRequestSet> RequestSetList
        {
            get { return _RequestSetList; }
            set
            {
                if (value != _RequestSetList)
                {
                    _RequestSetList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public TCPIPRequestParam()
        {
        }

        private ResponseActionCommand GetDefaultActionCommand()
        {
            string ACK_prefix = "ACK [00] ";
            string NACK_prefix = "ACK [01] ";

            ResponseActionCommand responseActionCommand = null;

            responseActionCommand = new ResponseActionCommand();
            responseActionCommand.ACK.prefix = ACK_prefix;
            responseActionCommand.ACK.data = string.Empty;
            responseActionCommand.ACK.postfix = string.Empty;

            responseActionCommand.NACK.prefix = NACK_prefix;
            responseActionCommand.NACK.data = string.Empty;
            responseActionCommand.NACK.postfix = string.Empty;

            return responseActionCommand;
        }
        private void MakeYMTCRequestSet()
        {
            CommunicationRequestSet tmpRequestSet = null;
            //QueryMerge queryMerge = null;
            //RequestBase tmprequest = null;
            TCPIPQueryData tcpipquerydata = null;
            QueryHasAffix queryHasAffix = null;
            ResponseActionCommand responseActionCommand = null;

            try
            {
                string ACK_prefix = "ACK OK ";
                string NACK_prefix = "NACK TimingError ";

                #region Action + Special Event?

                // Zup
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "Zup";
                //tmpRequestSet.Request = new Command.TCPIP.Response_ZUP();
                //tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                //tcpipquerydata.ACK.prefix = ACK_prefix;
                //tcpipquerydata.ACK.data = "Zup";
                //tcpipquerydata.ACK.postfix = string.Empty;

                //tcpipquerydata.NACK.prefix = string.Empty;
                //tcpipquerydata.NACK.data = string.Empty;
                //tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "Zup";
                //tcpipquerydata.NACK = string.Empty;

                //tcpipquerydata.SpecialEvent = typeof(Response_ZupDone).FullName;
                //tcpipquerydata.CommandType = EnumTCPIPCommandType.ACTION;

                responseActionCommand = new ResponseActionCommand();
                responseActionCommand.ACK.prefix = ACK_prefix;
                responseActionCommand.ACK.data = string.Empty;
                responseActionCommand.ACK.postfix = string.Empty;

                responseActionCommand.NACK.prefix = NACK_prefix;
                responseActionCommand.NACK.data = string.Empty;
                responseActionCommand.NACK.postfix = string.Empty;

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.ZUp());

                RequestSetList.Add(tmpRequestSet);

                // MoveToNextIndex
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "MoveToNextIndex";
                //tmpRequestSet.Request = new Command.TCPIP.Response_MoveToNextIndex();
                //tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                //tcpipquerydata.ACK.prefix = ACK_prefix;
                //tcpipquerydata.ACK.data = "MoveToNextIndex";
                //tcpipquerydata.ACK.postfix = string.Empty;

                //tcpipquerydata.NACK.prefix = string.Empty;
                //tcpipquerydata.NACK.data = string.Empty;
                //tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "MoveToNextIndex";
                //tcpipquerydata.NACK = string.Empty;

                //tcpipquerydata.SpecialEvent = typeof(Response_MoveToNextIndexDone).FullName;

                //tcpipquerydata.CommandType = EnumTCPIPCommandType.ACTION;

                responseActionCommand = new ResponseActionCommand();
                responseActionCommand.ACK.prefix = ACK_prefix;
                responseActionCommand.ACK.data = string.Empty;
                responseActionCommand.ACK.postfix = string.Empty;

                responseActionCommand.NACK.prefix = NACK_prefix;
                responseActionCommand.NACK.data = string.Empty;
                responseActionCommand.NACK.postfix = string.Empty;

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.MoveToNextPosition());

                RequestSetList.Add(tmpRequestSet);

                // MoveToNextIndex
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "UnloadWafer";
                //tmpRequestSet.Request = new Command.TCPIP.Response_UnloadWafer();
                //tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                //tcpipquerydata.ACK.prefix = ACK_prefix;
                //tcpipquerydata.ACK.data = "UnloadWafer";
                //tcpipquerydata.ACK.postfix = string.Empty;

                //tcpipquerydata.NACK.prefix = NACK_prefix;
                //tcpipquerydata.NACK.data = "UnloadWafer";
                //tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "UnloadWafer";
                //tcpipquerydata.NACK = "UnloadWafer";

                //tcpipquerydata.SpecialEvent = typeof(Response_UnloadWaferDone).FullName;

                //tcpipquerydata.CommandType = EnumTCPIPCommandType.ACTION;

                responseActionCommand = new ResponseActionCommand();
                responseActionCommand.ACK.prefix = ACK_prefix;
                responseActionCommand.ACK.data = string.Empty;
                responseActionCommand.ACK.postfix = string.Empty;

                responseActionCommand.NACK.prefix = NACK_prefix;
                responseActionCommand.NACK.data = string.Empty;
                responseActionCommand.NACK.postfix = string.Empty;

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.UnloadWafer());

                RequestSetList.Add(tmpRequestSet);

                // StopAndAlarm
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "StopAndAlarm";
                //tmpRequestSet.Request = new Command.TCPIP.Response_StopAndAlarm();
                //tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                //tcpipquerydata.ACK.prefix = ACK_prefix;
                //tcpipquerydata.ACK.data = "StopAndAlarm";
                //tcpipquerydata.ACK.postfix = string.Empty;

                //tcpipquerydata.NACK.prefix = string.Empty;
                //tcpipquerydata.NACK.data = string.Empty;
                //tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.SpecialEvent = typeof(Response_StopAndAlarmDone).FullName;

                //tcpipquerydata.CommandType = EnumTCPIPCommandType.ACTION;

                responseActionCommand = new ResponseActionCommand();
                responseActionCommand.ACK.prefix = ACK_prefix;
                responseActionCommand.ACK.data = string.Empty;
                responseActionCommand.ACK.postfix = string.Empty;

                responseActionCommand.NACK.prefix = NACK_prefix;
                responseActionCommand.NACK.data = string.Empty;
                responseActionCommand.NACK.postfix = string.Empty;

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.LotPause());

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region Query

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentChuckTemp?";

                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCurTemp();

                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "CurrentChuckTemp?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "CurrentChuckTemp?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "CurrentChuckTemp?";
                //tcpipquerydata.NACK = "CurrentChuckTemp?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " TEMP=";
                queryHasAffix.Data = new CurTemp();
                queryHasAffix.DataFormat = "{0:f1}";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProberID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetProberID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "ProberID?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = string.Empty;
                tcpipquerydata.NACK.data = string.Empty;
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "ProberID?";
                //tcpipquerydata.NACK = string.empty;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " ID=";
                queryHasAffix.Data = new ProberID();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "LotName?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetLotName();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "LotName?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = string.Empty;
                tcpipquerydata.NACK.data = string.Empty;
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "LotName?";
                //tcpipquerydata.NACK = "";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " NAME=";
                queryHasAffix.Data = new LotNmae();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "KindName?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetKindNmae();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "KindName?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = string.Empty;
                tcpipquerydata.NACK.data = string.Empty;
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "KindName?";
                //tcpipquerydata.NACK = "";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " NAME=";
                queryHasAffix.Data = new DeviceName();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "WaferSize?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetWaferSize();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "WaferSize?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = string.Empty;
                tcpipquerydata.NACK.data = string.Empty;
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "WaferSize?";
                //tcpipquerydata.NACK = "";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " Unit=inch SIZE=";
                //queryHasAffix.Data = new WaferSizeInch();

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsWafer6Inch(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "6" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsWafer8Inch(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "8" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsWafer12Inch(),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "12" },
                            Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                        }
                    }
                };

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OrientationFlatAngle?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOrientationFlatAngle();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "OrientationFlatAngle?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = string.Empty;
                tcpipquerydata.NACK.data = string.Empty;
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "OrientationFlatAngle?";
                //tcpipquerydata.NACK = "";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " ANGLE=";
                queryHasAffix.Data = new FlatOrientation();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CardID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCardID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "CardID?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = string.Empty;
                tcpipquerydata.NACK.data = string.Empty;
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "CardID?";
                //tcpipquerydata.NACK = "";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " ID=";
                queryHasAffix.Data = new CardID();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "WaferID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetWaferID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "WaferID?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "WaferID?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "WaferID?";
                //tcpipquerydata.NACK = "WaferID?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " ID=";
                queryHasAffix.Data = new WaferID();
                queryHasAffix.InvaildData = "none";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CarrierStatus?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCarrierStatus();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "CarrierStatus?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "CarrierStatus?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "CarrierStatus?";
                //tcpipquerydata.NACK = "CarrierStatus?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " STATUS=";
                queryHasAffix.Data = new CarrierStatus();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentSlotNumber?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCurrentSlotNumber();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "CurrentSlotNumber?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "CurrentSlotNumber?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "CurrentSlotNumber?";
                //tcpipquerydata.NACK = "CurrentSlotNumber?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " No=";
                queryHasAffix.Data = new CurrentSlotNumber();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "MoveResult?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetMoveResult();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "MoveResult?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "MoveResult?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "MoveResult?";
                //tcpipquerydata.NACK = "MoveResult?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " EndInf=";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsZUP(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "OK" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsRemainProbingSequecne(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "OK" },
                        Negative = new RequestCore.QueryPack.FixData() { ResultData = "WaferEnd" }
                    }
                };

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OverDrive?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOverDrive();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "OverDrive?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "OverDrive?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "OverDrive?";
                //tcpipquerydata.NACK = "OverDrive?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " OD=";
                queryHasAffix.Data = new OverDrive();
                queryHasAffix.DataFormat = "{0:f1}";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentX?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetXCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "CurrentX?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "CurrentX?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "CurrentX?";
                //tcpipquerydata.NACK = "CurrentX?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " X=";
                queryHasAffix.Data = new CurrentXCoordinate();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentY?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetYCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.ACK.prefix = ACK_prefix;
                tcpipquerydata.ACK.data = "CurrentY?";
                tcpipquerydata.ACK.postfix = string.Empty;

                tcpipquerydata.NACK.prefix = NACK_prefix;
                tcpipquerydata.NACK.data = "CurrentY?";
                tcpipquerydata.NACK.postfix = string.Empty;

                //tcpipquerydata.ACK = "CurrentY?";
                //tcpipquerydata.NACK = "CurrentY?";

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = " Y=";
                queryHasAffix.Data = new CurrentYCoordinate();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MakeCATANIARequestSet()
        {
            CommunicationRequestSet tmpRequestSet = null;
            //QueryMerge queryMerge = null;
            //RequestBase tmprequest = null;
            TCPIPQueryData tcpipquerydata = null;
            QueryHasAffix queryHasAffix = null;

            try
            {
                #region Action

                #region Zup

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "Zup";
                tmpRequestSet.Request = new QueryMerge();
                var responseActionCommand = GetDefaultActionCommand();
                responseActionCommand.Validation = new RequestCore.Controller.Branch()
                {
                    Condition = new IsProbingModuleSuspendedState(),
                    Positive = new RequestCore.ActionPack.Internal.ZUp(),
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsProbingModuleRunningState(),
                        Positive = new RequestCore.ActionPack.Internal.ZUp(),
                        Negative = new IsProbingModuleRunningState()
                    }
                };

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region Zdown

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "Zdown";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.ZDown());

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region MoveToNextIndex

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "MoveToNextIndex";
                tmpRequestSet.Request = new QueryMerge();

                responseActionCommand = GetDefaultActionCommand();
                responseActionCommand.Validation = new RequestCore.Controller.Branch()
                {
                    Condition = new IsProbingModuleSuspendedState(),
                    Positive = new RequestCore.ActionPack.Internal.MoveToNextPosition(),
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsProbingModuleRunningState(),
                        Positive = new RequestCore.ActionPack.Internal.MoveToNextPosition(),
                        Negative = new IsProbingModuleRunningState()
                    }
                };

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region UnloadWafer

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "UnloadWafer";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.UnloadWafer());

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region StopAndAlarm

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "StopAndAlarm";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.Internal.LotPause());

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region BN : BIN CODE

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "BN";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.TCPIP.GetCalcualtePassFailYield());

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region DW

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "DW";
                tmpRequestSet.Request = new QueryMerge();

                responseActionCommand = GetDefaultActionCommand();
                responseActionCommand.Validation = new DRDWValueValidation();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.TCPIP.SetDataUsingDWID());

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region LPV

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "LPV";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());

                // REAL
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.TCPIP.SetLotProcessingVerify());
                //(tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_LotProcessingVerifyDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region ProbeCardLoadStart
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProbeCardLoadStart";
                tmpRequestSet.Request = new QueryMerge();

                responseActionCommand = GetDefaultActionCommand();
                responseActionCommand.Validation = new RequestCore.Controller.Branch()
                {
                    Condition = new IsCardChangeGoingOn(),
                    Negative = new IsCardChangeGoingOn(), 
                    Positive = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsLotIdleState(),
                        Positive = new RequestCore.ActionPack.TCPIP.SetProbeCardinfoVerify(),
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotPausedState(),
                            Positive = new RequestCore.ActionPack.TCPIP.SetProbeCardinfoVerify(),
                            Negative = new IsLotPausedState()
                        } 
                    }
                };

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);

                RequestSetList.Add(tmpRequestSet);
                #endregion

                #region ERROREND

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "TestAbort";
                tmpRequestSet.Request = new QueryMerge();

                responseActionCommand = GetDefaultActionCommand();
                responseActionCommand.Validation = new RequestCore.Controller.Branch()
                {
                    Condition = new IsLotRunningState(),
                    Positive = new RequestCore.ActionPack.Internal.TestAbort(),
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsLotIdleState(),
                        Positive = new HandleFailedRequest() { Result = EventCodeEnum.ERROR_END },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotPausedState(),
                            Positive = new HandleFailedRequest() { Result = EventCodeEnum.ERROR_END },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new IsLotErrorState(),
                                Positive = new HandleFailedRequest() { Result = EventCodeEnum.ERROR_END },
                                Negative = new HandleFailedRequest() { Result = EventCodeEnum.ERROR_END },
                            }
                        }
                    }
                };

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);

                RequestSetList.Add(tmpRequestSet);

                #endregion
                #endregion

                #region Query

                #region CurrentChuckTemp?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentChuckTemp?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCurTemp();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new CurTemp();
                queryHasAffix.DataFormat = "{0:f1}";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region ProberID?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProberID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetProberID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new ProberID();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);
                #endregion

                #region ProbeCardID?
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProbeCardID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetProbeCardID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new ProbeCardID();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);
                #endregion

                #region LotName?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "LotName?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetLotName();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new LotNmae();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region RecipeName?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "RecipeName?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetKindNmae();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new DeviceName();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion


                #region WaferSize?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "WaferSize?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetWaferSize();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsWafer6Inch(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "6" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsWafer8Inch(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "8" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsWafer12Inch(),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "12" },
                            Negative = new RequestCore.QueryPack.FixData() { ResultData = "Undefined" }
                        }
                    }
                };

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region OrientationFlatAngle?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OrientationFlatAngle?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOrientationFlatAngle();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new FlatOrientation();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region WaferID?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "WaferID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetWaferID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new WaferID();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CarrierStatus?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CarrierStatus?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCarrierStatus();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new CarrierStatus();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion


                #region CurrentSlotNumber?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentSlotNumber?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCurrentSlotNumber();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new CurrentSlotNumber();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region OverDrive?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OverDrive?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOverDrive();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new OverDrive();
                queryHasAffix.DataFormat = "{0:f0}";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CurrentX?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentX?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetXCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new CurrentXCoordinate();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CurrentY?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentY?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetYCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new CurrentYCoordinate();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region ProberStatus?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProberStatus?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetProberStatus();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsLotIdleState(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "I" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsLotRunningState(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "R" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotPausedState(),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "P" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new IsLotErrorState(),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "E" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "" },
                            }
                        }
                    }
                };

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region OnWafer?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OnWafer?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOnWaferInfo();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new OnWaferInformation();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region DR

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "DR";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetDRInfo();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new DRValue();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region DMI?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "DMI?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetDutMapInformation();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                queryHasAffix.Data = new DutMapInformation();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MakeCATANIADummyRequestSet()
        {
            CommunicationRequestSet tmpRequestSet = null;
            //QueryMerge queryMerge = null;
            //RequestBase tmprequest = null;
            TCPIPQueryData tcpipquerydata = null;
            QueryHasAffix queryHasAffix = null;

            try
            {
                //string ACK_prefix = "ACK [00] ";
                //string NACK_prefix = "ACK [01] ";

                #region Action

                #region Zup

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "Zup";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_ZupDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region MoveToNextIndex

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "MoveToNextIndex";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_MoveToNextIndexDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region UnloadWafer

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "UnloadWafer";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_UnloadWaferDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region StopAndAlarm

                // StopAndAlarm
                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "StopAndAlarm";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_StopAndAlarmDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region BN : BIN CODE

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "BN";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_BINCodeDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region DW

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "DW";
                tmpRequestSet.Request = new QueryMerge();

                var responseActionCommand = GetDefaultActionCommand();
                responseActionCommand.Validation = new DRDWValueValidation();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(responseActionCommand);

                // REAL
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.TCPIP.SetDataUsingDWID());
                //(tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_DWDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region LPV

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "LPV";
                tmpRequestSet.Request = new QueryMerge();

                (tmpRequestSet.Request as QueryMerge).Querys.Add(GetDefaultActionCommand());

                // REAL
                (tmpRequestSet.Request as QueryMerge).Querys.Add(new RequestCore.ActionPack.TCPIP.SetLotProcessingVerify());
                //(tmpRequestSet.Request as QueryMerge).Querys.Add(new EventRaising(typeof(Response_LotProcessingVerifyDone).FullName));

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #endregion

                #region Query

                #region CurrentChuckTemp?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentChuckTemp?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCurTemp();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new CurTemp();
                //queryHasAffix.Data = new TCPIPDummyQueryData();
                queryHasAffix.DataFormat = "{0:f1}";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region ProberID?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProberID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetProberID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new ProberID();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region LotName?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "LotName?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetLotName();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new LotNmae();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region RecipeName?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "RecipeName?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetKindNmae();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new DeviceName();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region WaferSize?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "WaferSize?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetWaferSize();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region OrientationFlatAngle?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OrientationFlatAngle?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOrientationFlatAngle();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new FlatOrientation();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region WaferID?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "WaferID?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetWaferID();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                queryHasAffix.Data = new WaferID();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CarrierStatus?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CarrierStatus?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCarrierStatus();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CurrentSlotNumber?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentSlotNumber?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetCurrentSlotNumber();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new CurrentSlotNumber(); 
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region OverDrive?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OverDrive?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOverDrive();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new OverDrive();
                //queryHasAffix.Data = new TCPIPDummyQueryData();
                queryHasAffix.DataFormat = "{0:f0}";

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CurrentX?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentX?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetXCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new CurrentXCoordinate();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region CurrentY?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "CurrentY?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetYCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new CurrentYCoordinate();
                //queryHasAffix.Data = new TCPIPDummyQueryData();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region ProberStatus?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "ProberStatus?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetYCoordinate();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";
                queryHasAffix.Data = new RequestCore.Controller.Branch()
                {
                    Condition = new IsLotIdleState(),
                    Positive = new RequestCore.QueryPack.FixData() { ResultData = "I" },
                    Negative = new RequestCore.Controller.Branch()
                    {
                        Condition = new IsLotRunningState(),
                        Positive = new RequestCore.QueryPack.FixData() { ResultData = "R" },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotPausedState(),
                            Positive = new RequestCore.QueryPack.FixData() { ResultData = "P" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new IsLotErrorState(),
                                Positive = new RequestCore.QueryPack.FixData() { ResultData = "E" },
                                Negative = new RequestCore.QueryPack.FixData() { ResultData = "" },
                            }
                        }
                    }
                };

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region OnWafer?  

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "OnWafer?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetOnWaferInfo();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new OnWaferInformation();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region DR

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "DR";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetDRInfo();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new DRValue();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #region DMI?

                tmpRequestSet = new CommunicationRequestSet();
                tmpRequestSet.Name = "DMI?";
                tmpRequestSet.Request = new RequestCore.Query.TCPIP.GetDutMapInformation();
                tcpipquerydata = tmpRequestSet.Request as TCPIPQueryData;

                tcpipquerydata.CommandType = EnumTCPIPCommandType.QUERY;

                queryHasAffix = new QueryHasAffix();
                queryHasAffix.prefix = "=";

                // REAL
                queryHasAffix.Data = new DutMapInformation();

                tcpipquerydata.ACKExtensionQueries.Querys.Add(queryHasAffix);

                RequestSetList.Add(tmpRequestSet);

                #endregion

                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                MakeCATANIARequestSet();
                //MakeCATANIADummyRequestSet();

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SetDefaultParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
