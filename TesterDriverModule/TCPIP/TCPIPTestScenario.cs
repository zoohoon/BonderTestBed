using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication.Scenario;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TesterDriverModule.TCPIP
{
    public class TCPIPTesterScenario : ITesterScenarioModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<ScenarioSet> _Scenarios = new ObservableCollection<ScenarioSet>();
        public ObservableCollection<ScenarioSet> Scenarios
        {
            get { return _Scenarios; }
            set
            {
                if (value != _Scenarios)
                {
                    _Scenarios = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScenarioSet _SelectedScenario;
        public ScenarioSet SelectedScenario
        {
            get { return _SelectedScenario; }
            set
            {
                if (value != _SelectedScenario)
                {
                    _SelectedScenario = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum MakeScenario()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(Scenarios == null)
                {
                    Scenarios = new ObservableCollection<ScenarioSet>();
                }

                Scenarios.Clear();

                Scenarios.Add(MakeCATANIAScenario());

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ScenarioSet MakeCATANIAScenario()
        {
            ScenarioSet retval = new ScenarioSet();

            try
            {
                retval.Name = "CATANIA";

                ScenarioCommand command = null;
                RequestSet requestSet = null;


                // ProberStart
                command = new ScenarioCommand("ProberStart");

                retval.Commands.Add(command);

                // LotStart#
                command = new ScenarioCommand("LotStart");

                retval.Commands.Add(command);

                // WaferStart#
                command = new ScenarioCommand("WaferStart");

                command.RequestSet.Add(new RequestSet { RequestName = "CurrentChuckTemp?" });
                command.RequestSet.Add(new RequestSet { RequestName = "ProberID?" });
                command.RequestSet.Add(new RequestSet { RequestName = "LotName?" });
                command.RequestSet.Add(new RequestSet { RequestName = "RecipeName?" });
                command.RequestSet.Add(new RequestSet { RequestName = "WaferSize?" });
                command.RequestSet.Add(new RequestSet { RequestName = "OrientationFlatAngle?" });
                command.RequestSet.Add(new RequestSet { RequestName = "WaferID?" });
                command.RequestSet.Add(new RequestSet { RequestName = "CarrierStatus?" });
                command.RequestSet.Add(new RequestSet { RequestName = "Zup" });

                retval.Commands.Add(command);

                // CurrentChuckTemp?
                command = new ScenarioCommand("CurrentChuckTemp?");

                retval.Commands.Add(command);

                // ProberID?
                command = new ScenarioCommand("ProberID?");

                retval.Commands.Add(command);

                // LotName?
                command = new ScenarioCommand("LotName?");

                retval.Commands.Add(command);

                // RecipeName?
                command = new ScenarioCommand("RecipeName?");

                retval.Commands.Add(command);

                // WaferSize?
                command = new ScenarioCommand("WaferSize?");

                retval.Commands.Add(command);

                // OrientationFlatAngle?
                command = new ScenarioCommand("OrientationFlatAngle?");

                retval.Commands.Add(command);

                // WaferID?
                command = new ScenarioCommand("WaferID?");

                retval.Commands.Add(command);

                // CarrierStatus?
                command = new ScenarioCommand("CarrierStatus?");

                retval.Commands.Add(command);

                // Zup
                command = new ScenarioCommand("Zup");

                retval.Commands.Add(command);

                //// MoveEnd
                //command = new TCPIPScenarioCommand();
                //command.Name = new ScenarioCommand("MoveEnd");

                //command.RequestSet.Add(new RequestSet { RequestName = "OnWafer?" });
                //command.RequestSet.Add(new RequestSet { RequestName = "BN" });
                //command.RequestSet.Add(new RequestSet { RequestName = "OverDrive?" });
                //command.RequestSet.Add(new RequestSet { RequestName = "MoveToNextIndex" });

                //retval.Commands.Add(command);

                // OnWafer?
                command = new ScenarioCommand("OnWafer?");

                retval.Commands.Add(command);

                // BN
                command = new ScenarioCommand("BN");

                retval.Commands.Add(command);

                // OverDrive?
                command = new ScenarioCommand("OverDrive?");

                retval.Commands.Add(command);

                // MoveToNextIndex
                command = new ScenarioCommand("MoveToNextIndex");

                retval.Commands.Add(command);

                // MoveEnd
                command = new ScenarioCommand("MoveEnd");

                requestSet = new RequestSet();

                // Case1) 시퀀스가 더 이상 존재하지 않는 경우 => Return ACK OK MoveResult? EndInf=WaferEnd => UnloadWafer 커맨드 수행
                // Case2) 시퀀스가 남아 있는 경우 => Return ACK OK MoveResult? EndInf=OK => Zup

                requestSet.getRequestNameFunc = GetNextCommandForMoveResult;
                command.RequestSet.Add(requestSet);

                ScenarioTree GetNextCommandForMoveResult(string lastmsg)
                {
                    ScenarioTree tree = new ScenarioTree();

                    if (lastmsg.Contains("WaferEnd"))
                    {
                        tree.RequestSet = new List<RequestSet>();
                        tree.RequestSet.Add(new RequestSet { RequestName = "UnloadWafer" });

                        return tree;
                    }
                    else
                    {
                        tree.RequestSet = new List<RequestSet>();
                        //tree.NeedChangeOrder = true;
                        //tree.ChangeOrderNo = 13;

                        tree.RequestSet.Add(new RequestSet { RequestName = "OnWafer?" });
                        tree.RequestSet.Add(new RequestSet { RequestName = "BN" });
                        tree.RequestSet.Add(new RequestSet { RequestName = "OverDrive?" });
                        tree.RequestSet.Add(new RequestSet { RequestName = "MoveToNextIndex" });

                        return tree;
                    }
                }

                retval.Commands.Add(command);

                // UnloadWafer
                command = new ScenarioCommand("UnloadWafer");
                //command.multiOrderList = new List<int>();
                //command.multiOrderList.Add(2);
                //command.multiOrderList.Add(23);

                retval.Commands.Add(command);

                // LotEnd
                command = new ScenarioCommand("LotEnd");

                requestSet = new RequestSet();

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

                retval.Commands.Add(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ITesterCommandRecipe CommandRecipe { get; set; }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MakeCommands();
                MakeScenario();

                SelectedScenario = Scenarios.FirstOrDefault(x => x.Name == "CATANIA");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void MakeCommands()
        {
            try
            {
                if (CommandRecipe == null)
                {
                    CommandRecipe = new TesterCommandRecipe();
                }

                CommandRecipe.Commands.Clear();

                TCPIPTesterInterruptCommand InterruptCommand = null;
                TCPIPTesterActionCommand ActionCommand = null;
                TCPIPTesterQueryCommand QueryCommand = null;

                // ProberError
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "ProberError";
                InterruptCommand.ACK = "ProberError";
                InterruptCommand.NACK = "ProberError";

                CommandRecipe.Commands.Add(InterruptCommand);

                // MoveEnd
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "MoveEnd";
                InterruptCommand.ACK = "MoveEnd";
                InterruptCommand.NACK = "MoveEnd";

                CommandRecipe.Commands.Add(InterruptCommand);

                // ProberStart
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "ProberStart";
                InterruptCommand.ACK = "ProberStart";
                InterruptCommand.NACK = "ProberStart";

                CommandRecipe.Commands.Add(InterruptCommand);

                // ProberStatus
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "ProberStatus";
                InterruptCommand.ACK = "ProberStatus";
                InterruptCommand.NACK = "ProberStatus";

                CommandRecipe.Commands.Add(InterruptCommand);

                // LotEnd#
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "LotEnd";
                InterruptCommand.ACK = "LotEnd";
                InterruptCommand.NACK = string.Empty;

                CommandRecipe.Commands.Add(InterruptCommand);

                // LotStart#
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "LotStart";
                InterruptCommand.ACK = "LotStart";
                InterruptCommand.NACK = string.Empty;

                CommandRecipe.Commands.Add(InterruptCommand);

                // WaferStart#
                InterruptCommand = new TCPIPTesterInterruptCommand();
                InterruptCommand.Name = "WaferStart";
                InterruptCommand.ACK = "WaferStart";
                InterruptCommand.NACK = string.Empty;

                CommandRecipe.Commands.Add(InterruptCommand);

                // CurrentX?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "CurrentX?";

                CommandRecipe.Commands.Add(QueryCommand);

                // CurrentY?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "CurrentY?";

                CommandRecipe.Commands.Add(QueryCommand);

                // Zup
                ActionCommand = new TCPIPTesterActionCommand();
                ActionCommand.Name = "Zup";
                ActionCommand.ACK.Add("Zup");
                ActionCommand.ACK.Add("MoveEnd");

                CommandRecipe.Commands.Add(ActionCommand);

                // WaferID?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "WaferID?";

                CommandRecipe.Commands.Add(QueryCommand);

                // UnloadWafer
                ActionCommand = new TCPIPTesterActionCommand();
                ActionCommand.Name = "UnloadWafer";
                ActionCommand.ACK.Add("UnloadWafer");
                ActionCommand.ACK.Add("MoveEnd");
                ActionCommand.NACK.Add("UnloadWafer");

                CommandRecipe.Commands.Add(ActionCommand);

                // CurrentChuckTemp?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "CurrentChuckTemp?";

                CommandRecipe.Commands.Add(QueryCommand);

                // BIN INFO
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "BN";

                CommandRecipe.Commands.Add(QueryCommand);

                // On Wafer INFO
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "OnWafer?";

                CommandRecipe.Commands.Add(QueryCommand);

                // OverDrive?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "OverDrive?";

                CommandRecipe.Commands.Add(QueryCommand);

                // MoveToNextIndex
                ActionCommand = new TCPIPTesterActionCommand();
                ActionCommand.Name = "MoveToNextIndex";

                CommandRecipe.Commands.Add(ActionCommand);

                // CarrierStatus?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "CarrierStatus?";

                CommandRecipe.Commands.Add(QueryCommand);

                // CurrentSlotNumber?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "CurrentSlotNumber?";

                CommandRecipe.Commands.Add(QueryCommand);

                // ProberID?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "ProberID?";

                CommandRecipe.Commands.Add(QueryCommand);

                // LotName?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "LotName?";

                CommandRecipe.Commands.Add(QueryCommand);

                // KindName?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "RecipeName?";

                CommandRecipe.Commands.Add(QueryCommand);

                // WaferSize?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "WaferSize?";

                CommandRecipe.Commands.Add(QueryCommand);

                // OrientationFlatAngle?
                QueryCommand = new TCPIPTesterQueryCommand();
                QueryCommand.Name = "OrientationFlatAngle?";

                CommandRecipe.Commands.Add(QueryCommand);

                // StopAndAlarm
                ActionCommand = new TCPIPTesterActionCommand();
                ActionCommand.Name = "StopAndAlarm";
                ActionCommand.ACK.Add("StopAndAlarm");
                CommandRecipe.Commands.Add(ActionCommand);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Reset()
        {
            // Nothing
        }

        public EventCodeEnum ChangeScenario(string name)
        {
            return EventCodeEnum.NONE;
        }
    }
}
