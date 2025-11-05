using LogModule;
using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TesterDriverModule.TCPIP
{

    public class ScenarioCommand
    {
        public string CommandName { get; set; }

        public ScenarioCommand()
        {

        }
        public ScenarioCommand(string commandname)
        {
            this.CommandName = commandname;
        }
    }

    public class ScenarioTree
    {
        public bool NeedChangeOrder { get; set; }

        public int ChangeOrderNo { get; set; }
        //public string RequestName { get; set; }

        public List<TestRequestSet> RequestSet { get; set; }
    }
    public class TestRequestSet
    {
        //public string LastMessage { get;set; }
        // 분기 설정을 위함.
        public string RequestName { get; set; }

        public Func<string, ScenarioTree> getRequestNameFunc;

        public TestRequestSet(Func<string, ScenarioTree> func)
        {
            getRequestNameFunc = func;
        }

        public TestRequestSet()
        {

        }
    }

    public class TCPIPTesterScenarioCommand
    {
        // TODO : 커맨드 분기 지원
        //public List<ScenarioCommand> ScenarioCommands { get; set; }

        public ScenarioCommand ScenarioCommand { get; set; }

        public int order { get; set; }
        public List<int> multiOrderList { get; set; }
        //public List<string> RequestSet { get; set; }

        public List<TestRequestSet> RequestSet { get; set; }

        public TCPIPTesterScenarioCommand()
        {
            //if (this.ScenarioCommands == null)
            //{
            //    this.ScenarioCommands = new List<ScenarioCommand>();
            //}

            if (this.RequestSet == null)
            {
                this.RequestSet = new List<TestRequestSet>();
            }
        }

    }

    public enum ScenarioType
    {
        YMTC,
        CATANIA
    }

    public abstract class TestScenario
    {
        public abstract EventCodeEnum MakeScenario();

        public TestScenario()
        {
            this.MakeScenario();
        }
    }

    /// <summary>
    /// 정해진 플로우 제작 및 플로우 흐름 확인 용도
    /// </summary>
    public class TCPIPDefaultTestScenario : TestScenario, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<TCPIPTesterScenarioCommand> _CommandsOrder = new ObservableCollection<TCPIPTesterScenarioCommand>();
        public ObservableCollection<TCPIPTesterScenarioCommand> CommandsOrder
        {
            get { return _CommandsOrder; }
            set
            {
                if (value != _CommandsOrder)
                {
                    _CommandsOrder = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScenarioType type = ScenarioType.CATANIA;

        public override EventCodeEnum MakeScenario()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (type == ScenarioType.YMTC)
                {
                    //MakeYMTCScenario();
                }
                else if (type == ScenarioType.CATANIA)
                {
                    MakeCATANIAScenario();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public void MakeYMTCScenario()
        //{
        //    try
        //    {
        //        TCPIPTesterScenarioCommand command = null;
        //        ScenarioCommand scenarioCommand = null;
        //        TestRequestSet requestSet = null;

        //        int StartOrder = 0;

        //        // ProberStart
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("ProberStart"));
        //        command.order = StartOrder;

        //        CommandsOrder.Add(command);

        //        // WaferStart#
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("WaferStart"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // ChipStart#
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("ChipStart"));
        //        command.order = ++StartOrder;

        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CurrentChuckTemp?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "ProberID?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "LotName?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "KindName?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "WaferSize?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "OrientationFlatAngle?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CardID?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "WaferID?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CarrierStatus?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CurrentSlotNumber?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "Zup" });

        //        CommandsOrder.Add(command);

        //        // CurrentChuckTemp?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CurrentChuckTemp?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // ProberID?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("ProberID?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // LotName?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("LotName?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // KindName?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("KindName?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // WaferSize?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("WaferSize?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // OrientationFlatAngle?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("OrientationFlatAngle?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // CardID?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CardID?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // WaferID?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("WaferID?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // CarrierStatus?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CarrierStatus?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // CurrentSlotNumber?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CurrentSlotNumber?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // Zup
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("Zup"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveEnd"));
        //        command.order = ++StartOrder;

        //        command.RequestSet.Add(new TestRequestSet { RequestName = "MoveResult?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "OverDrive?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CurrentX?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CurrentY?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "MoveToNextIndex" });

        //        CommandsOrder.Add(command);

        //        // MoveResult?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveResult?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // OverDrive?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("OverDrive?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // CurrentX?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CurrentX?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // CurrentY?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CurrentY?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveToNextIndex
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveToNextIndex"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveEnd"));
        //        command.order = ++StartOrder;


        //        command.RequestSet.Add(new TestRequestSet { RequestName = "MoveResult?" });

        //        CommandsOrder.Add(command);

        //        // MoveResult?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveResult?"));
        //        command.order = ++StartOrder;

        //        requestSet = new TestRequestSet();

        //        // Case1) 시퀀스가 더 이상 존재하지 않는 경우 => Return ACK OK MoveResult? EndInf=WaferEnd => UnloadWafer 커맨드 수행
        //        // Case2) 시퀀스가 남아 있는 경우 => Return ACK OK MoveResult? EndInf=OK => Zup

        //        requestSet.getRequestNameFunc = GetNextCommandForMoveResult;
        //        command.RequestSet.Add(requestSet);

        //        ScenarioTree GetNextCommandForMoveResult(string lastmsg)
        //        {
        //            ScenarioTree tree = new ScenarioTree();

        //            if (lastmsg.Contains("WaferEnd"))
        //            {
        //                tree.RequestSet = new List<TestRequestSet>();
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "UnloadWafer" });

        //                return tree;
        //            }
        //            else
        //            {
        //                tree.RequestSet = new List<TestRequestSet>();
        //                tree.NeedChangeOrder = true;
        //                tree.ChangeOrderNo = 15;

        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "OverDrive?" });
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "CurrentX?" });
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "CurrentY?" });
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "MoveToNextIndex" });

        //                return tree;
        //            }
        //        }

        //        CommandsOrder.Add(command);

        //        // UnloadWafer
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("UnloadWafer"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveEnd"));
        //        command.order = ++StartOrder;
        //        command.multiOrderList = new List<int>();
        //        command.multiOrderList.Add(2);
        //        command.multiOrderList.Add(23);

        //        CommandsOrder.Add(command);

        //        // LotEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("LotEnd"));
        //        command.order = ++StartOrder;

        //        requestSet = new TestRequestSet();

        //        requestSet.getRequestNameFunc = SenarioInitialize;
        //        command.RequestSet.Add(requestSet);

        //        ScenarioTree SenarioInitialize(string lastmsg)
        //        {
        //            ScenarioTree tree = new ScenarioTree();

        //            tree.RequestSet = null;
        //            tree.NeedChangeOrder = true;

        //            // Next LOT
        //            tree.ChangeOrderNo = 1;

        //            return tree;
        //        }

        //        CommandsOrder.Add(command);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void MakeCATANIAScenario()
        {
            try
            {
                TCPIPTesterScenarioCommand command = null;
                ScenarioCommand scenarioCommand = null;
                TestRequestSet requestSet = null;


                // ProberStart
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("ProberStart");

                CommandsOrder.Add(command);

                // Allocated
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("Allocated");

                CommandsOrder.Add(command);

                // LotStart#
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("LotStart");

                CommandsOrder.Add(command);

                // WaferStart#
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("WaferStart");

                command.RequestSet.Add(new TestRequestSet { RequestName = "CurrentChuckTemp?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "ProberID?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "LotName?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "RecipeName?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "WaferSize?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "OrientationFlatAngle?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "WaferID?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "CarrierStatus?" });
                command.RequestSet.Add(new TestRequestSet { RequestName = "Zup" });

                CommandsOrder.Add(command);

                // CurrentChuckTemp?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("CurrentChuckTemp?");

                CommandsOrder.Add(command);

                // ProberID?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("ProberID?");

                CommandsOrder.Add(command);

                // LotName?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("LotName?");

                CommandsOrder.Add(command);

                // RecipeName?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("RecipeName?");

                CommandsOrder.Add(command);

                // WaferSize?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("WaferSize?");

                CommandsOrder.Add(command);

                // OrientationFlatAngle?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("OrientationFlatAngle?");

                CommandsOrder.Add(command);

                // WaferID?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("WaferID?");

                CommandsOrder.Add(command);

                // CarrierStatus?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("CarrierStatus?");

                CommandsOrder.Add(command);

                // Zup
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("Zup");

                CommandsOrder.Add(command);

                //// MoveEnd
                //command = new TCPIPTesterScenarioCommand();
                //command.ScenarioCommand = new ScenarioCommand("MoveEnd");

                //command.RequestSet.Add(new TestRequestSet { RequestName = "OnWafer?" });
                //command.RequestSet.Add(new TestRequestSet { RequestName = "BN" });
                //command.RequestSet.Add(new TestRequestSet { RequestName = "OverDrive?" });
                //command.RequestSet.Add(new TestRequestSet { RequestName = "MoveToNextIndex" });

                //CommandsOrder.Add(command);

                // OnWafer?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("OnWafer?");

                CommandsOrder.Add(command);

                // BN
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("BN");

                CommandsOrder.Add(command);

                // OverDrive?
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("OverDrive?");

                CommandsOrder.Add(command);

                // MoveToNextIndex
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("MoveToNextIndex");

                CommandsOrder.Add(command);

                // MoveEnd
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("MoveEnd");

                requestSet = new TestRequestSet();

                // Case1) 시퀀스가 더 이상 존재하지 않는 경우 => Return ACK OK MoveResult? EndInf=WaferEnd => UnloadWafer 커맨드 수행
                // Case2) 시퀀스가 남아 있는 경우 => Return ACK OK MoveResult? EndInf=OK => Zup

                requestSet.getRequestNameFunc = GetNextCommandForMoveResult;
                command.RequestSet.Add(requestSet);

                ScenarioTree GetNextCommandForMoveResult(string lastmsg)
                {
                    ScenarioTree tree = new ScenarioTree();

                    if (lastmsg.Contains("WaferEnd"))
                    {
                        tree.RequestSet = new List<TestRequestSet>();
                        tree.RequestSet.Add(new TestRequestSet { RequestName = "UnloadWafer" });

                        return tree;
                    }
                    else
                    {
                        tree.RequestSet = new List<TestRequestSet>();
                        //tree.NeedChangeOrder = true;
                        //tree.ChangeOrderNo = 13;

                        tree.RequestSet.Add(new TestRequestSet { RequestName = "OnWafer?" });
                        tree.RequestSet.Add(new TestRequestSet { RequestName = "BN" });
                        tree.RequestSet.Add(new TestRequestSet { RequestName = "OverDrive?" });
                        tree.RequestSet.Add(new TestRequestSet { RequestName = "MoveToNextIndex" });

                        return tree;
                    }
                }

                CommandsOrder.Add(command);

                // UnloadWafer
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("UnloadWafer");
                //command.multiOrderList = new List<int>();
                //command.multiOrderList.Add(2);
                //command.multiOrderList.Add(23);

                CommandsOrder.Add(command);

                // LotEnd
                command = new TCPIPTesterScenarioCommand();
                command.ScenarioCommand = new ScenarioCommand("LotEnd");

                requestSet = new TestRequestSet();

                requestSet.getRequestNameFunc = SenarioInitialize;
                command.RequestSet.Add(requestSet);

                ScenarioTree SenarioInitialize(string lastmsg)
                {
                    ScenarioTree tree = new ScenarioTree();

                    tree.RequestSet = null;
                    //tree.NeedChangeOrder = true;

                    // Next LOT
                    //tree.ChangeOrderNo = 1;

                    return tree;
                }

                CommandsOrder.Add(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region 2021-02-24
        //public void MakeCATANIAScenario()
        //{
        //    try
        //    {
        //        TCPIPTesterScenarioCommand command = null;
        //        ScenarioCommand scenarioCommand = null;
        //        TestRequestSet requestSet = null;

        //        int StartOrder = 0;

        //        // ProberStart
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("ProberStart"));
        //        command.order = StartOrder;

        //        CommandsOrder.Add(command);

        //        // LotStart#
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("LotStart"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // WaferStart#
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("WaferStart"));
        //        command.order = ++StartOrder;

        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CurrentChuckTemp?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "ProberID?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "LotName?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "RecipeName?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "WaferSize?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "OrientationFlatAngle?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "WaferID?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "CarrierStatus?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "Zup" });

        //        CommandsOrder.Add(command);

        //        // CurrentChuckTemp?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CurrentChuckTemp?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // ProberID?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("ProberID?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // LotName?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("LotName?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // RecipeName?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("RecipeName?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // WaferSize?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("WaferSize?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // OrientationFlatAngle?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("OrientationFlatAngle?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // WaferID?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("WaferID?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // CarrierStatus?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("CarrierStatus?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // Zup
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("Zup"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveEnd"));
        //        command.order = ++StartOrder;

        //        command.RequestSet.Add(new TestRequestSet { RequestName = "OnWafer?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "BN" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "OverDrive?" });
        //        command.RequestSet.Add(new TestRequestSet { RequestName = "MoveToNextIndex" });

        //        CommandsOrder.Add(command);

        //        // OnWafer?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("OnWafer?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // BN
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("BN"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // OverDrive?
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("OverDrive?"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveToNextIndex
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveToNextIndex"));
        //        command.order = ++StartOrder;

        //        CommandsOrder.Add(command);

        //        // MoveEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("MoveEnd"));
        //        command.order = ++StartOrder;

        //        requestSet = new TestRequestSet();

        //        // Case1) 시퀀스가 더 이상 존재하지 않는 경우 => Return ACK OK MoveResult? EndInf=WaferEnd => UnloadWafer 커맨드 수행
        //        // Case2) 시퀀스가 남아 있는 경우 => Return ACK OK MoveResult? EndInf=OK => Zup

        //        requestSet.getRequestNameFunc = GetNextCommandForMoveResult;
        //        command.RequestSet.Add(requestSet);

        //        ScenarioTree GetNextCommandForMoveResult(string lastmsg)
        //        {
        //            ScenarioTree tree = new ScenarioTree();

        //            if (lastmsg.Contains("WaferEnd"))
        //            {
        //                tree.RequestSet = new List<TestRequestSet>();
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "UnloadWafer" });

        //                return tree;
        //            }
        //            else
        //            {
        //                tree.RequestSet = new List<TestRequestSet>();
        //                tree.NeedChangeOrder = true;
        //                tree.ChangeOrderNo = 13;

        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "OnWafer?" });
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "BN" });
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "OverDrive?" });
        //                tree.RequestSet.Add(new TestRequestSet { RequestName = "MoveToNextIndex" });

        //                return tree;
        //            }
        //        }

        //        CommandsOrder.Add(command);

        //        // UnloadWafer
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("UnloadWafer"));
        //        command.order = ++StartOrder;
        //        command.multiOrderList = new List<int>();
        //        command.multiOrderList.Add(2);
        //        command.multiOrderList.Add(23);

        //        CommandsOrder.Add(command);

        //        // LotEnd
        //        command = new TCPIPTesterScenarioCommand();
        //        command.ScenarioCommands.Add(new ScenarioCommand("LotEnd"));
        //        command.order = ++StartOrder;

        //        requestSet = new TestRequestSet();

        //        requestSet.getRequestNameFunc = SenarioInitialize;
        //        command.RequestSet.Add(requestSet);

        //        ScenarioTree SenarioInitialize(string lastmsg)
        //        {
        //            ScenarioTree tree = new ScenarioTree();

        //            tree.RequestSet = null;
        //            tree.NeedChangeOrder = true;

        //            // Next LOT
        //            tree.ChangeOrderNo = 1;

        //            return tree;
        //        }

        //        CommandsOrder.Add(command);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        #endregion

        public TCPIPDefaultTestScenario()
        {
        }
    }
}
