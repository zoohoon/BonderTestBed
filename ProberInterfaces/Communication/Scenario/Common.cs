using LogModule;
using ProberErrorCode;
using ProberInterfaces.Communication.Tester;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.Communication.Scenario
{
    public interface ITesterScenarioManager : IFactoryModule
    {
        ITesterScenarioModule ScenarioModule { get; set; }

        EventCodeEnum InitModule(EnumTesterComType comtype);
    }

    public interface ITesterScenarioModule : INotifyPropertyChanged
    {
        ITesterCommandRecipe CommandRecipe { get; set; }
        ObservableCollection<ScenarioSet> Scenarios { get; set; }
        ScenarioSet SelectedScenario { get; set; }
        EventCodeEnum InitModule();
        EventCodeEnum MakeScenario();
        EventCodeEnum ChangeScenario(string name);

        //EventCodeEnum LoadScenario();
    }


    public interface ITesterCommandRecipe
    {
        ObservableCollection<TesterCommand> Commands { get; set; }
    }


    public class TesterCommand: INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; RaisePropertyChanged(); }
        }

        private string _Result;

        public string Result
        {
            get { return _Result; }
            set { _Result = value; RaisePropertyChanged(); }
        }

    }

    public class TesterCommandRecipe : ITesterCommandRecipe, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<TesterCommand> _Commands = new ObservableCollection<TesterCommand>();
        public ObservableCollection<TesterCommand> Commands
        {
            get { return _Commands; }
            set
            {
                if (value != _Commands)
                {
                    _Commands = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class TesterScenarioManager : ITesterScenarioManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ITesterScenarioModule ScenarioModule { get; set; }

        public EventCodeEnum InitModule(EnumTesterComType comtype)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ITesterComDriver testerComDriver = null;

                if (comtype == EnumTesterComType.GPIB)
                {
                    testerComDriver = this.GPIB().TesterComDriver;
                }
                else if (comtype == EnumTesterComType.TCPIP)
                {
                    testerComDriver = this.TCPIPModule().TesterComDriver;
                }

                if (testerComDriver != null)
                {
                    if (testerComDriver is IHasTesterScenarioModule)
                    {
                        IHasTesterScenarioModule module = testerComDriver as IHasTesterScenarioModule;

                        ScenarioModule = module.ScenarioModule;
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    public class ScenarioTree
    {
        public bool NeedChangeOrder { get; set; }

        public int ChangeOrderNo { get; set; }
        //public string RequestName { get; set; }

        public List<RequestSet> RequestSet { get; set; }
    }
    public class ScenarioSet : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<ScenarioCommand> _Commands = new ObservableCollection<ScenarioCommand>();
        public ObservableCollection<ScenarioCommand> Commands
        {
            get { return _Commands; }
            set
            {
                if (value != _Commands)
                {
                    _Commands = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Name = string.Empty;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
    public class RequestSet
    {
        public string RequestName { get; set; }

        public Func<string, ScenarioTree> getRequestNameFunc;

        public RequestSet()
        {
        }
    }
    public class ScenarioCommand
    {
        public string Name { get; set; }

        public List<RequestSet> RequestSet { get; set; }

        public ScenarioCommand(string name)
        {
            this.Name = name;

            if (this.RequestSet == null)
            {
                this.RequestSet = new List<RequestSet>();
            }
        }
    }
}
