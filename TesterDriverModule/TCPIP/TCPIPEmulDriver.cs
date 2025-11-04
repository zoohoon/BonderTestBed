using System;

namespace TesterDriverModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Communication.Scenario;
    using RequestCore.Query.TCPIP;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Sockets;

    public class TCPIPEmulDriver : ITesterComDriver, IFactoryModule
    {
        // The port number for the remote device.  
        private const int port = 5500;
        private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        public static Socket client;
        public Socket ClientSock;

        private static String response = String.Empty;
        private static String lastSendMsg = string.Empty;
        
        private String lastReceiveQueryMsg = string.Empty;

        public static EnumTesterDriverState State = EnumTesterDriverState.UNDEFINED;

        //private int CurrentOrderCount = 0;

        public List<string> TestString = new List<string>();

        public List<string> ReplaceDataForActionResponse = new List<string>(
            new string[] { "ACK", "NACK", "OK", "TimingError" });

        public TCPIPEmulDriver()
        {
            MakeTesterResponseRecipe();
        }

        public EventCodeEnum Connect(object connectparam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                SetState(EnumTesterDriverState.CONNECTED);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void Reset()
        {

        }

        public string Read()
        {
            string tmp = null;
            try
            {
                if(TestString.Count > 0)
                {
                    tmp = TestString[0];
                    TestString.RemoveAt(0);
                }

                // 호스트에서 모든 커맨드를 가져감.
                if (TestString.Count == 0)
                {
                    SetState(EnumTesterDriverState.SENDED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return tmp;
        }

        public void WriteSTB(object command)
        {
            // command => string type

            string STB = null;

            try
            {
                LoggerManager.Debug($"[TCPIPEmulDriver] Received Command : {command}");

                STB = (string)command;
                lastSendMsg = STB;

                ReceiveCallback();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private List<int> _MultiOrder = new List<int>();
        public List<int> MultiOrder
        {
            get { return _MultiOrder; }
            set
            {
                if (value != _MultiOrder)
                {
                    _MultiOrder = value;
                }
            }
        }

        public ObservableCollection<CollectionComponent> CommandCollection { get; set; }

        /// <summary>
        /// 시나리오에 정의 된 순서와 동일하게 커맨드를 주고 받는지 확인
        /// </summary>
        /// <param name="InputCommand"></param>
        private void VerifyCommandOrder(string InputCommand, EnumTCPIPCommandType type)
        {
            //bool IsValid = false;

            //try
            //{
            //    // 예외
            //    // 1. WaferStart#
            //    // 2. ChipStart#
            //    // 3. LotEnd#

            //    string ContainText = InputCommand;

            //    if (InputCommand.Contains("WaferStart") == true)
            //    {
            //        ContainText = "WaferStart";
            //    }
            //    else if (InputCommand.Contains("ChipStart") == true)
            //    {
            //        ContainText = "ChipStart";
            //    }
            //    else if (InputCommand.Contains("LotEnd") == true)
            //    {
            //        ContainText = "LotEnd";
            //    }

            //    if (InputCommand != null && InputCommand != string.Empty)
            //    {
            //        if (MultiOrder.Count > 0)
            //        {
            //            if (ContainText == "ChipStart")
            //            {
            //                CurrentOrderCount = MultiOrder[0];
            //                MultiOrder.Clear();
            //            }
            //            else if(ContainText == "LotEnd")
            //            {
            //                CurrentOrderCount = MultiOrder[1];
            //                MultiOrder.Clear();
            //            }
            //        }
                    
            //        if (Scenario.CommandsOrder[CurrentOrderCount].multiOrderList != null)
            //        {
            //            foreach (var count in Scenario.CommandsOrder[CurrentOrderCount].multiOrderList)
            //            {
            //                MultiOrder.Add(count);
            //            }
            //        }

            //        TesterScenarioCommand scenariocommand = Scenario.CommandsOrder.ToList().Find(x => x.order == CurrentOrderCount);

            //        string CorrectCommandName = string.Empty;

            //        // TODO : 쓰려면 로직 추가 필요. 단순 주석 처리 진행되어 있음.

            //        //if (scenariocommand.ScenarioCommands.Any(x => x.CommandName.Contains(ContainText)) == true)
            //        //{
            //        //    ScenarioCommand scenarioCommand = scenariocommand.ScenarioCommands.FirstOrDefault(x => x.CommandName.Contains(ContainText));

            //        //    CorrectCommandName = scenarioCommand.CommandName;

            //        //    IsValid = true;

            //        //    CurrentOrderCount++;

            //        //    //if (scenarioCommand.NeedChangeOrder == false)
            //        //    //{
            //        //    //    CurrentOrderCount++;
            //        //    //}
            //        //    //else
            //        //    //{
            //        //    //    CurrentOrderCount = scenarioCommand.ChangeOrderNo;
            //        //    //}

            //        //    // TODO : 구조 변경으로 손봐야 함.
            //        //    if(type != EnumTCPIPCommandType.QUERY)
            //        //    {
            //        //        TestString.Add("ACK " + InputCommand);
            //        //    }
            //        //    ////

            //        //    // 시나리오에 정의되어 있는 데이터를 이용하여 커맨드 데이터 세트 구성
            //        //    if (scenariocommand.RequestSet != null && scenariocommand.RequestSet.Count > 0)
            //        //    {
            //        //        GetReqeustcommandSet(scenariocommand.RequestSet);
            //        //    }
            //        //}
            //        //else
            //        //{
            //        //    IsValid = false;
            //        //    LoggerManager.Error($"[TCPIPEmulDriver], WriteString() : Correct command name = {InputCommand}, Order No.{CurrentOrderCount}");
            //        //}
            //    }
            //    else
            //    {
            //        LoggerManager.Error($"[TCPIPEmulDriver], WriteString() : Correct command name = {InputCommand}, Order No.{CurrentOrderCount}");
            //    }
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}
        }

        public void WriteString(string query_command)
        {
            try
            {
                SetState(EnumTesterDriverState.BEGINSEND);

                LoggerManager.Debug($"[TCPIPEmulDriver] Received Command : {query_command}");

                //bool IsValid = false;

                string[] querysplit = query_command.Split(' ');
                string QueryCommandName = string.Empty;

                bool IsQuery = false;
                //bool IsActionResponse = false;

                if (query_command.Contains("?"))
                {
                    foreach (var splitdata in querysplit)
                    {
                        IsQuery = splitdata.Contains("?");

                        if (IsQuery == true)
                        {
                            QueryCommandName = splitdata;
                            break;
                        }
                    }
                }
                else
                {
                    QueryCommandName = query_command;

                    foreach (var replace in ReplaceDataForActionResponse)
                    {
                        QueryCommandName = QueryCommandName.Replace(replace, "");
                    }

                    QueryCommandName = QueryCommandName.Trim();
                }

                lastReceiveQueryMsg = query_command;

                VerifyCommandOrder(QueryCommandName, EnumTCPIPCommandType.QUERY);

                // 서버에서 바로 가져갔다고 가정.
                SetState(EnumTesterDriverState.SENDED);

                // 다음 커맨드가 남아 있는 경우, 실제로는 서버로부터 커맨드를 받은 후와 같은 상태.
                if (TestString.Count > 0)
                {
                    SetState(EnumTesterDriverState.RECEIVED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ReceiveCallback()
        {
            try
            {
                if (lastSendMsg != null && lastSendMsg != string.Empty)
                {
                    MakeResponseDatasUsingRecipe(lastSendMsg);

                    SetState(EnumTesterDriverState.RECEIVED);

                    //LoggerManager.Debug($"TCPIP Receive message = {response}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public object GetState()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return State;
        }

        public void SetState(EnumTesterDriverState state)
        {
            try
            {
                State = state;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                SetState(EnumTesterDriverState.BEGINRECEIVE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void GetReqeustcommandSet(List<RequestSet> requestset)
        {
            //try
            //{
            //    TesterCommand tmp = null;

            //    ScenarioTree tree = null;

            //    string requestname = string.Empty;

            //    foreach (var request in requestset)
            //    {
            //        // 분기에 따른 Request를 얻어와야하는 경우 처리
            //        if(request.getRequestNameFunc != null)
            //        {
            //            tree = request.getRequestNameFunc(this.lastReceiveQueryMsg);

            //            if(tree.NeedChangeOrder == true)
            //            {
            //                CurrentOrderCount = tree.ChangeOrderNo;
            //            }

            //            if(tree.RequestSet != null)
            //            {
            //                GetReqeustcommandSet(tree.RequestSet);
            //            }

            //            //requestname = tree.RequestName;
            //        }
            //        else
            //        {
            //            requestname = request.RequestName;
            //        }

            //        if(requestname != null && requestname != string.Empty)
            //        {
            //            tmp = CommandRecipe.Commands.Find(x => x.CommandName.Contains(requestname));

            //            if(tmp != null)
            //            {
            //                TestString.Add(tmp.CommandName);
            //            }
            //        }
            //    }
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}
        }

        private void MakeResponseDatasUsingRecipe(string readstring)
        {
            string retval = string.Empty;

            try
            {
                VerifyCommandOrder(readstring, EnumTCPIPCommandType.INTERRUPT);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        //public EventCodeEnum StartReceive()
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        Receive(client);

        //        retval = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        private void MakeTesterResponseRecipe()
        {
            //try
            //{
            //    TCPIPTesterInterruptCommand InterruptCommand = null;
            //    TCPIPTesterActionCommand ActionCommand = null;
            //    TCPIPTesterQueryCommand QueryCommand = null;

            //    // ProberError
            //    InterruptCommand = new TCPIPTesterInterruptCommand();
            //    InterruptCommand.CommandName = "ProberError";
            //    InterruptCommand.ACK = "ProberError";
            //    InterruptCommand.NACK = "ProberError";

            //    CommandRecipe.Commands.Add(InterruptCommand);

            //    // MoveEnd
            //    InterruptCommand = new TCPIPTesterInterruptCommand();
            //    InterruptCommand.CommandName = "MoveEnd";
            //    InterruptCommand.ACK = "MoveEnd";
            //    InterruptCommand.NACK = "MoveEnd";

            //    CommandRecipe.Commands.Add(InterruptCommand);

            //    // ProberStart
            //    InterruptCommand = new TCPIPTesterInterruptCommand();
            //    InterruptCommand.CommandName = "ProberStart";
            //    InterruptCommand.ACK = "ProberStart";
            //    InterruptCommand.NACK = "ProberStart";

            //    CommandRecipe.Commands.Add(InterruptCommand);

            //    // LotEnd#
            //    InterruptCommand = new TCPIPTesterInterruptCommand();
            //    InterruptCommand.CommandName = "LotEnd";
            //    InterruptCommand.ACK = "LotEnd";
            //    InterruptCommand.NACK = string.Empty;

            //    CommandRecipe.Commands.Add(InterruptCommand);

            //    // WaferStart#
            //    InterruptCommand = new TCPIPTesterInterruptCommand();
            //    InterruptCommand.CommandName = "WaferStart";
            //    InterruptCommand.ACK = "WaferStart";
            //    InterruptCommand.NACK = string.Empty;

            //    CommandRecipe.Commands.Add(InterruptCommand);

            //    // ChipStart#
            //    InterruptCommand = new TCPIPTesterInterruptCommand();
            //    InterruptCommand.CommandName = "ChipStart";
            //    InterruptCommand.ACK = "ChipStart";
            //    InterruptCommand.NACK = string.Empty;

            //    CommandRecipe.Commands.Add(InterruptCommand);

            //    // CurrentX?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "CurrentX?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // CurrentY?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "CurrentY?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // Zup
            //    ActionCommand = new TCPIPTesterActionCommand();
            //    ActionCommand.CommandName = "Zup";
            //    ActionCommand.ACK.Add("Zup");
            //    ActionCommand.ACK.Add("MoveEnd");

            //    CommandRecipe.Commands.Add(ActionCommand);

            //    // WaferID?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "WaferID?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // UnloadWafer
            //    ActionCommand = new TCPIPTesterActionCommand();
            //    ActionCommand.CommandName = "UnloadWafer";
            //    ActionCommand.ACK.Add("UnloadWafer");
            //    ActionCommand.ACK.Add("MoveEnd");
            //    ActionCommand.NACK.Add("UnloadWafer");

            //    CommandRecipe.Commands.Add(ActionCommand);

            //    // CurrentChuckTemp?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "CurrentChuckTemp?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // OverDrive?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "OverDrive?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // MoveToNextIndex
            //    ActionCommand = new TCPIPTesterActionCommand();
            //    ActionCommand.CommandName = "MoveToNextIndex";

            //    CommandRecipe.Commands.Add(ActionCommand);

            //    // CarrierStatus?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "CarrierStatus?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // CurrentSlotNumber?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "CurrentSlotNumber?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // MoveResult?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "MoveResult?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // ProberID?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "ProberID?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // LotName?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "LotName?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // KindName?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "KindName?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // WaferSize?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "WaferSize?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // OrientationFlatAngle?
            //    QueryCommand = new TCPIPTesterQueryCommand();
            //    QueryCommand.CommandName = "OrientationFlatAngle?";

            //    CommandRecipe.Commands.Add(QueryCommand);

            //    // StopAndAlarm
            //    ActionCommand = new TCPIPTesterActionCommand();
            //    ActionCommand.CommandName = "StopAndAlarm";
            //    ActionCommand.ACK.Add("StopAndAlarm");
            //    CommandRecipe.Commands.Add(ActionCommand);
            //}
            //catch (Exception err)
            //{
            //    LoggerManager.Exception(err);
            //}
        }
    }
}
