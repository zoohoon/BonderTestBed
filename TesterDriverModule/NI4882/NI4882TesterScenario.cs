using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication.Scenario;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TesterDriverModule.NI4882
{

    public class NI4882TesterScenario : ITesterScenarioModule
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

        private ITesterCommandRecipe _CommandRecipe;
        public ITesterCommandRecipe CommandRecipe
        {
            get { return _CommandRecipe; }
            set
            {
                if (value != _CommandRecipe)
                {
                    _CommandRecipe = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MakeCommands();
                MakeScenario();

                // TODO : 
                SelectedScenario = Scenarios.FirstOrDefault(x => x.Name == "Hynix-MPT");

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum MakeScenario()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Scenarios == null)
                {
                    Scenarios = new ObservableCollection<ScenarioSet>();
                }

                Scenarios.Clear();

                Scenarios.Add(MakeDefaultScenario());
                Scenarios.Add(MakeHynixScenario());
                Scenarios.Add(MakeHynix2Scenario());

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private ScenarioSet MakeDefaultScenario()
        {
            ScenarioSet retval = new ScenarioSet();

            try
            {
                retval.Name = "Default";

                ScenarioCommand command = null;

                command = new ScenarioCommand("70");

                command.RequestSet.Add(new RequestSet { RequestName = "Z" });

                retval.Commands.Add(command);

                command = new ScenarioCommand("67");

                command.RequestSet.Add(new RequestSet { RequestName = "O" });
                command.RequestSet.Add(new RequestSet { RequestName = "Q" });
                command.RequestSet.Add(new RequestSet { RequestName = "M" });

                retval.Commands.Add(command);

                command = new ScenarioCommand("81");

                command.RequestSet.Add(new RequestSet { RequestName = "L" });

                retval.Commands.Add(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private ScenarioSet MakeHynixScenario()
        {
            ScenarioSet retval = new ScenarioSet();

            try
            {
                retval.Name = "Hynix-MPT";

                ScenarioCommand command = null;

                // LotStartEvent
                command = new ScenarioCommand("121");

                command.RequestSet.Add(new RequestSet { RequestName = "G" });
                command.RequestSet.Add(new RequestSet { RequestName = "ur1227" });
                command.RequestSet.Add(new RequestSet { RequestName = "ur0137" });
                command.RequestSet.Add(new RequestSet { RequestName = "ur0281" });

                retval.Commands.Add(command);

                // GoToStartDieEvent
                command = new ScenarioCommand("70");

                command.RequestSet.Add(new RequestSet { RequestName = "ur0204" });
                command.RequestSet.Add(new RequestSet { RequestName = "ur0033" });
                command.RequestSet.Add(new RequestSet { RequestName = "f1" });
                command.RequestSet.Add(new RequestSet { RequestName = "n" });
                command.RequestSet.Add(new RequestSet { RequestName = "d" });
                command.RequestSet.Add(new RequestSet { RequestName = "Z" });

                retval.Commands.Add(command);

                // ProbingZUpProcessEvent
                command = new ScenarioCommand("67");

                command.RequestSet.Add(new RequestSet { RequestName = "D" });

                retval.Commands.Add(command);

                // ProbingZDownProcessEvent
                command = new ScenarioCommand("68");

                command.RequestSet.Add(new RequestSet { RequestName = "Q" });

                retval.Commands.Add(command);

                // WaferEndEvent
                command = new ScenarioCommand("75");

                command.RequestSet.Add(new RequestSet { RequestName = "U" });

                retval.Commands.Add(command);

                // LotEndEvent
                command = new ScenarioCommand("72");

                retval.Commands.Add(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private ScenarioSet MakeHynix2Scenario()
        {
            ScenarioSet retval = new ScenarioSet();

            try
            {
                retval.Name = "Hynix-MPT2";

                ScenarioCommand command = null;

                command = new ScenarioCommand("70");

                command.RequestSet.Add(new RequestSet { RequestName = "V" });
                command.RequestSet.Add(new RequestSet { RequestName = "ku" });
                command.RequestSet.Add(new RequestSet { RequestName = "B" });
                command.RequestSet.Add(new RequestSet { RequestName = "b" });
                command.RequestSet.Add(new RequestSet { RequestName = "x" });
                command.RequestSet.Add(new RequestSet { RequestName = "Z" });

                retval.Commands.Add(command);

                // ProbingZUpProcessEvent
                command = new ScenarioCommand("67");

                command.RequestSet.Add(new RequestSet { RequestName = "O" });                
                command.RequestSet.Add(new RequestSet { RequestName = "Q" });
                command.RequestSet.Add(new RequestSet { RequestName = "f" });
                command.RequestSet.Add(new RequestSet { RequestName = "M" });

                retval.Commands.Add(command);

                // WaferEndEvent
                command = new ScenarioCommand("69");

                command.RequestSet.Add(new RequestSet { RequestName = "J" });

                retval.Commands.Add(command);

                // LotEndEvent
                command = new ScenarioCommand("81");

                command.RequestSet.Add(new RequestSet { RequestName = "L" });

                retval.Commands.Add(command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum ChangeScenario(string name)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(!string.IsNullOrEmpty(name))
                {
                    SelectedScenario = Scenarios.FirstOrDefault(x => x.Name == name);
                }
                else
                {
                    LoggerManager.Debug($"[{this.GetType().Name}], ChangeScenario() : name is Unknown.");
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private NI4882TesterActionCommand MakeActionCommand(string name)
        {
            NI4882TesterActionCommand retval = new NI4882TesterActionCommand();

            try
            {
                retval.Name = name;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private NI4882TesterQueryCommand MakeQueryCommand(string name)
        {
            NI4882TesterQueryCommand retval = new NI4882TesterQueryCommand();

            try
            {
                retval.Name = name;
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

                if(CommandRecipe == null)
                {
                    CommandRecipe = new TesterCommandRecipe();
                }

                CommandRecipe.Commands.Clear();

           
                // Original

                // Z
                CommandRecipe.Commands.Add(MakeActionCommand("Z"));

                // Q
                CommandRecipe.Commands.Add(MakeActionCommand("Q"));

             
                // L
                CommandRecipe.Commands.Add(MakeActionCommand("L"));

                // J
                CommandRecipe.Commands.Add(MakeActionCommand("J"));


                // Hynix

                // M
                CommandRecipe.Commands.Add(MakeActionCommand("M"));


                // G
                CommandRecipe.Commands.Add(MakeQueryCommand("G"));

                // f
                CommandRecipe.Commands.Add(MakeQueryCommand("f"));

                // f1
                CommandRecipe.Commands.Add(MakeQueryCommand("f1"));

                // n
                CommandRecipe.Commands.Add(MakeQueryCommand("n"));

                // d
                CommandRecipe.Commands.Add(MakeQueryCommand("d"));

                // D
                CommandRecipe.Commands.Add(MakeActionCommand("D"));

                // L
                CommandRecipe.Commands.Add(MakeActionCommand("L"));

                // V
                CommandRecipe.Commands.Add(MakeActionCommand("V"));

                // ku
                CommandRecipe.Commands.Add(MakeActionCommand("ku"));

                // B
                CommandRecipe.Commands.Add(MakeActionCommand("B"));

                // b
                CommandRecipe.Commands.Add(MakeActionCommand("b"));

                // x
                CommandRecipe.Commands.Add(MakeActionCommand("x"));
                
                // O
                CommandRecipe.Commands.Add(MakeActionCommand("O"));


                // C : 사용 안하는 것 같은데?
                //Commands.Commands.Add(MakeActionCommand("C"));

                // U
                CommandRecipe.Commands.Add(MakeActionCommand("U"));

                // ur1227
                CommandRecipe.Commands.Add(MakeQueryCommand("ur1227"));

                // ur0137
                CommandRecipe.Commands.Add(MakeQueryCommand("ur0137"));

                // ur0281
                CommandRecipe.Commands.Add(MakeQueryCommand("ur0281"));

                // ur0204
                CommandRecipe.Commands.Add(MakeQueryCommand("ur0204"));

                // ur0033
                CommandRecipe.Commands.Add(MakeQueryCommand("ur0033"));
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
    }
}
