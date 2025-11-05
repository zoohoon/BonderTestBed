using Autofac;
using E84;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LoaderBase;
using LogModule;
using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.Communication.E84;
using ProberInterfaces.E84;
using ProberInterfaces.E84.ProberInterfaces;
using ProberInterfaces.Foup;
using ProberViewModel.View.E84.Setting;
using ProberViewModel.ViewModel.E84;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using WinAPIWrapper;

namespace E84SimulatorDialog
{
    public class E84Execute
    {
        public bool Flag { get; set; }
        public object Type { get; set; }
    }

    public class E84SimualtorViewObj
    {
        public Window window { get; set; }
        public GridSplitter grs { get; set; }
        public ColumnDefinition GC1 { get; set; }
        public ColumnDefinition GC3 { get; set; }
        public ListView Graphlv { get; set; }
    }

    public class E84SimulatorDialogViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private const string ScriptRootPath = @"C:\ProberSystem\Simulator\E84\";

        private const string ScriptPath = @"C:\ProberSystem\Simulator\E84\Scripts.json";

        private ObservableCollection<E84SimulControllerViewModel> _E84SimulControllerViewModels = new ObservableCollection<E84SimulControllerViewModel>();
        public ObservableCollection<E84SimulControllerViewModel> E84SimulControllerViewModels
        {
            get { return _E84SimulControllerViewModels; }
            set
            {
                if (_E84SimulControllerViewModels != value)
                {
                    _E84SimulControllerViewModels = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84SimulControllerViewModel _SelectedE84SimulControllerViewModel;
        public E84SimulControllerViewModel SelectedE84SimulControllerViewModel
        {
            get { return _SelectedE84SimulControllerViewModel; }
            set
            {
                if (_SelectedE84SimulControllerViewModel != value)
                {
                    _SelectedE84SimulControllerViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _E84InfosCount;
        public int E84InfosCount
        {
            get { return _E84InfosCount; }
            set
            {
                if (value != _E84InfosCount)
                {
                    _E84InfosCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84Script> _Scripts;
        public ObservableCollection<E84Script> Scripts
        {
            get { return _Scripts; }
            set
            {
                if (value != _Scripts)
                {
                    _Scripts = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _nextActionDelay;
        public int nextActionDelay
        {
            get { return _nextActionDelay; }
            set
            {
                _nextActionDelay = value;
                RaisePropertyChanged();
            }
        }

        private int _StartDelay;
        public int StartDelay
        {
            get { return _StartDelay; }
            set
            {
                _StartDelay = value;
                RaisePropertyChanged();
            }
        }

        private int _nextBehaviorDelay;
        public int nextBehaviorDelay
        {
            get { return _nextBehaviorDelay; }
            set
            {
                _nextBehaviorDelay = value;
                RaisePropertyChanged();
            }
        }

        private E84Simulator _Simulator;
        public E84Simulator Simulator
        {
            get { return _Simulator; }
            set
            {
                _Simulator = value;
                RaisePropertyChanged();
            }
        }

        #region Commands
        public ICommand SelectedModuleChangedCommand { get; private set; }
        public ICommand ScriptAddCommand { get; private set; }
        public ICommand ScriptDelCommand { get; private set; }
        public ICommand ScriptLoadCommand { get; private set; }
        public ICommand ScriptSaveCommand { get; private set; }
        public ICommand AddInputOnCommand { get; private set; }
        public ICommand AddInputOffCommand { get; private set; }

        public ICommand AddOutputOnCommand { get; private set; }
        public ICommand AddOutputOffCommand { get; private set; }

        public ICommand LightCurtainOnOffCommand { get; private set; }

        public ICommand ActionsDelCommand { get; private set; }

        public ICommand BehaviorAddCommand { get; private set; }

        public ICommand BehaviorDelCommand { get; private set; }

        public ICommand ResetCommand { get; private set; }

        public ICommand SettingClickCommand { get; private set; }

        public ICommand CARD_ATTACH_Command { get; private set; }
        public ICommand CARD_DETACH_Command { get; private set; }

        public ICommand CST_ATTACH_Command { get; private set; }
        public ICommand CST_DETACH_Command { get; private set; }

        public ICommand CLAMP_LOCK_Command { get; private set; }
        public ICommand CLAMP_UNLOCK_Command { get; private set; }

        public ICommand DI_CST12_PRESsONCommand { get; private set; }
        public ICommand DI_CST12_PRESsOFFCommand { get; private set; }

        public ICommand DI_CST12_PRES2sONCommand { get; private set; }
        public ICommand DI_CST12_PRES2sOFFCommand { get; private set; }

        public ICommand DI_CST_ExistsONCommand { get; private set; }
        public ICommand DI_CST_ExistsOFFCommand { get; private set; }


        #endregion

        public void Init()
        {
            try
            {
                ScriptLoadCommandFunc();

                var e84Module = this.E84Module();

                if (e84Module != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        E84SimulControllerViewModels.Clear();

                        foreach (var foupinfo in e84Module.E84SysParam.E84Moduls)
                        {
                            var e84SimulControllerViewModel = new E84SimulControllerViewModel();

                            var controller = e84Module.GetE84Controller(foupinfo.FoupIndex, foupinfo.E84OPModuleType);

                            e84SimulControllerViewModel.E84Info = new E84Info(foupinfo.FoupIndex, controller, foupinfo.E84OPModuleType);
                            //e84SimulControllerViewModel.ViewObj = obj;
                            e84SimulControllerViewModel.SelectedScript = Scripts.FirstOrDefault();
                            e84SimulControllerViewModel.Simulator = GetSimulator(controller);
                            
                            e84SimulControllerViewModel.Init();

                            E84SimulControllerViewModels.Add(e84SimulControllerViewModel);
                        }

                        E84InfosCount = e84Module.E84SysParam.E84Moduls.Count;

                        if (E84InfosCount > 0)
                        {
                            SelectedE84SimulControllerViewModel = E84SimulControllerViewModels.FirstOrDefault();
                        }
                    });
                }

                InitializeCommands();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Deinit()
        {
            try
            {
                foreach (var item in E84SimulControllerViewModels)
                {
                    item.Deinit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void InitializeCommands()
        {
            try
            {
                SelectedModuleChangedCommand = new RelayCommand(SelectedModuleChangedCommandFunc);

                ScriptAddCommand = new RelayCommand(ScriptAddCommandFunc);
                ScriptDelCommand = new RelayCommand(ScriptDelCommandFunc);
                ScriptLoadCommand = new RelayCommand(ScriptLoadCommandFunc);
                ScriptSaveCommand = new RelayCommand(ScriptSaveCommandFunc);


                ActionsDelCommand = new RelayCommand(ActionsDelCommandFunc);

                BehaviorAddCommand = new RelayCommand(BehaviorAddCommandFunc);
                BehaviorDelCommand = new RelayCommand(BehaviorDelCommandFunc);

                AddInputOnCommand = new RelayCommand<object>(AddActionCommandFunc);
                AddInputOffCommand = new RelayCommand<object>(AddActionCommandFunc);
                AddOutputOnCommand = new RelayCommand<object>(AddActionCommandFunc);
                AddOutputOffCommand = new RelayCommand<object>(AddActionCommandFunc);

                LightCurtainOnOffCommand = new RelayCommand<object>(LightCurtainOnOffCommandFunc);

                ResetCommand = new RelayCommand(ResetCommandFunc);

                SettingClickCommand = new RelayCommand(SettingClickCommandFunc);

                CARD_ATTACH_Command = new RelayCommand(CARD_ATTACH_CommandFunc);
                CARD_DETACH_Command = new RelayCommand(CARD_DETACH_CommandFunc);

                CST_ATTACH_Command = new RelayCommand(CST_ATTACH_CommandFunc);
                CST_DETACH_Command = new RelayCommand(CST_DETACH_CommandFunc);

                CLAMP_LOCK_Command = new RelayCommand(CLAMP_LOCK_CommandFunc);
                CLAMP_UNLOCK_Command = new RelayCommand(CLAMP_UNLOCK_CommandFunc);

                DI_CST12_PRESsONCommand = new RelayCommand(DI_CST12_PRESsONCommandFunc);
                DI_CST12_PRESsOFFCommand = new RelayCommand(DI_CST12_PRESsOFFCommandFunc);
                DI_CST12_PRES2sONCommand = new RelayCommand(DI_CST12_PRES2sONCommandFunc);
                DI_CST12_PRES2sOFFCommand = new RelayCommand(DI_CST12_PRES2sOFFCommandFunc);
                DI_CST_ExistsONCommand = new RelayCommand(DI_CST_ExistsONCommandFunc);
                DI_CST_ExistsOFFCommand = new RelayCommand(DI_CST_ExistsOFFCommandFunc);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CARD_ATTACH_CommandFunc()
        {
            try
            {
                ILoaderModule Loader = this.GetLoaderContainer().Resolve<ILoaderModule>();

                string a = "DICardBuffs.0";
                string b = "DICardOnCarrierVacs.0";
                string c = "DICardExistSensorInBuffer";

                var DICARDONMODULE = Loader.IOManager.GetIOPortDescripter(a);
                
                if (DICARDONMODULE != null)
                {
                    DICARDONMODULE.ForcedIO.IsForced = true;
                    DICARDONMODULE.ForcedIO.ForecedValue = true;
                }

                var DICARRIERVAC = Loader.IOManager.GetIOPortDescripter(b);

                if (DICARRIERVAC != null)
                {
                    DICARRIERVAC.ForcedIO.IsForced = true;
                    DICARRIERVAC.ForcedIO.ForecedValue = true;
                }

                var DICARDATTACHMODULE = Loader.IOManager.GetIOPortDescripter(c);
                
                if (DICARDATTACHMODULE != null)
                {
                    DICARDATTACHMODULE.ForcedIO.IsForced = true;
                    DICARDATTACHMODULE.ForcedIO.ForecedValue = true;
                }

                Thread.Sleep(100);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CARD_DETACH_CommandFunc()
        {
            try
            {
                ILoaderModule Loader = this.GetLoaderContainer().Resolve<ILoaderModule>();

                string a = "DICardBuffs.0";
                string b = "DICardOnCarrierVacs.0";
                string c = "DICardExistSensorInBuffer";

                var DICARDONMODULE = Loader.IOManager.GetIOPortDescripter(a);

                if (DICARDONMODULE != null)
                {
                    DICARDONMODULE.ForcedIO.IsForced = true;
                    DICARDONMODULE.ForcedIO.ForecedValue = true;
                }

                var DICARRIERVAC = Loader.IOManager.GetIOPortDescripter(b);

                if (DICARRIERVAC != null)
                {
                    DICARRIERVAC.ForcedIO.IsForced = true;
                    DICARRIERVAC.ForcedIO.ForecedValue = true;
                }

                var DICARDATTACHMODULE = Loader.IOManager.GetIOPortDescripter(c);

                if (DICARDATTACHMODULE != null)
                {
                    DICARDATTACHMODULE.ForcedIO.IsForced = true;
                    DICARDATTACHMODULE.ForcedIO.ForecedValue = false;
                }

                Thread.Sleep(100);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CST_DETACH_CommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                // LOCK이 되어 있으면, Unlock이 먼저 이루어져야 함.

                FoupModuleInfo foupModuleInfo = this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.GetFoupModuleInfo();

                if (foupModuleInfo.DockingPlateState == DockingPlateStateEnum.LOCK)
                {
                    this.FoupOpModule().FoupControllers[foupindex - 1].Execute(new FoupDockingPlateUnlockCommand());
                }

                var presence1 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1];
                var presence2 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1];
                var cstExist = this.GetFoupIO().IOMap.Inputs.DI_CST_Exists[foupindex - 1];

                presence1.ForcedIO.IsForced = true;
                presence1.ForcedIO.ForecedValue = false;

                presence2.ForcedIO.IsForced = true;
                presence2.ForcedIO.ForecedValue = false;

                cstExist.ForcedIO.IsForced = true;
                cstExist.ForcedIO.ForecedValue = false;

                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CST_ATTACH_CommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var presence1 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1];
                var presence2 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1];
                var cstExist = this.GetFoupIO().IOMap.Inputs.DI_CST_Exists[foupindex - 1];

                presence1.ForcedIO.IsForced = true;
                presence1.ForcedIO.ForecedValue = true;

                presence2.ForcedIO.IsForced = true;
                presence2.ForcedIO.ForecedValue = true;

                cstExist.ForcedIO.IsForced = true;
                cstExist.ForcedIO.ForecedValue = true;

                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CLAMP_LOCK_CommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;
                IFoupController foupController = this.FoupOpModule().FoupControllers[foupindex - 1];
                FoupModuleInfo foupModuleInfo = foupController.Service.FoupModule.GetFoupModuleInfo();

                if (foupModuleInfo.DockingPlateState == DockingPlateStateEnum.UNLOCK)
                {
                    foupController.Execute(new FoupDockingPlateLockCommand());
                    foupController.Service.FoupModule.BroadcastFoupStateAsync();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CLAMP_UNLOCK_CommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;
                IFoupController foupController = this.FoupOpModule().FoupControllers[foupindex - 1];
                FoupModuleInfo foupModuleInfo = foupController.Service.FoupModule.GetFoupModuleInfo();

                if (foupModuleInfo.DockingPlateState == DockingPlateStateEnum.LOCK)
                {
                    foupController.Execute(new FoupDockingPlateUnlockCommand());
                    foupController.Service.FoupModule.BroadcastFoupStateAsync();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DI_CST12_PRESsONCommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var presence1 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1];

                presence1.ForcedIO.IsForced = true;
                presence1.ForcedIO.ForecedValue = true;
                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_CST12_PRESsOFFCommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var presence1 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRESs[foupindex - 1];

                presence1.ForcedIO.IsForced = true;
                presence1.ForcedIO.ForecedValue = false;
                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_CST12_PRES2sONCommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var presence2 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1];

                presence2.ForcedIO.IsForced = true;
                presence2.ForcedIO.ForecedValue = true;
                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DI_CST12_PRES2sOFFCommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var presence2 = this.GetFoupIO().IOMap.Inputs.DI_CST12_PRES2s[foupindex - 1];

                presence2.ForcedIO.IsForced = true;
                presence2.ForcedIO.ForecedValue = true;
                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DI_CST_ExistsONCommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var cstExist = this.GetFoupIO().IOMap.Inputs.DI_CST_Exists[foupindex - 1];

                cstExist.ForcedIO.IsForced = true;
                cstExist.ForcedIO.ForecedValue = true;
                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DI_CST_ExistsOFFCommandFunc()
        {
            try
            {
                var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;

                var cstExist = this.GetFoupIO().IOMap.Inputs.DI_CST_Exists[foupindex - 1];

                cstExist.ForcedIO.IsForced = true;
                cstExist.ForcedIO.ForecedValue = false;
                Thread.Sleep(100);

                this.FoupOpModule().FoupControllers[foupindex - 1].Service.FoupModule.BroadcastFoupStateAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SettingClickCommandFunc()
        {
            try
            {
                var e84Module = this.E84Module();

                if (e84Module != null)
                {
                    E84ControlSettingView settingView = new E84ControlSettingView();

                    var foupindex = SelectedE84SimulControllerViewModel.E84Info.FoupIndex;
                    var moduleytpe = SelectedE84SimulControllerViewModel.E84Info.E84OPModuleType;

                    settingView.DataContext = new E84ControlSettingViewModel(e84Module.GetE84Controller(foupindex, moduleytpe));

                    string windowName = $"[LOAD PORT #{foupindex}] E84 Setting";
                    bool bFindWindow = Win32APIWrapper.CheckWindowExists(windowName);

                    if (!bFindWindow)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Window container = new Window();
                            container.Content = settingView;
                            container.Width = 460;
                            container.Height = 800;
                            container.WindowStyle = WindowStyle.ToolWindow;
                            container.Title = windowName;
                            container.Topmost = true;
                            container.VerticalAlignment = VerticalAlignment.Center;
                            container.HorizontalAlignment = HorizontalAlignment.Center;
                            container.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            container.Show();
                        });
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private E84Simulator GetSimulator(IE84Controller controller)
        {
            E84Simulator retval = null;

            try
            {
                if (controller.CommModule != null &&
                    controller.E84ModuleParaemter.E84ConnType == E84ConnTypeEnum.SIMUL)
                {
                    if (controller.CommModule is SimulE84CommModule CommModule)
                    {
                        if (CommModule.Simulator != null)
                        {
                            retval = CommModule.Simulator;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private void ResetCommandFunc()
        {
            try
            {
                // CHART 데이터 초기화

                if (SelectedE84SimulControllerViewModel != null)
                {
                    SelectedE84SimulControllerViewModel.ResetCharts();

                    if (SelectedE84SimulControllerViewModel.Simulator != null)
                    {
                        // E84Input과 E84Output 데이터 초기화
                        SelectedE84SimulControllerViewModel.Simulator.SignalReset();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void ScriptAddCommandFunc()
        {
            try
            {
                // Generate the new script's name based on the current count of scripts
                int scriptCount = Scripts.Count + 1;
                string newScriptName = "Script_" + scriptCount;

                E84Script newScript = new E84Script(newScriptName);
                Scripts.Add(newScript);

                SelectedE84SimulControllerViewModel.SelectedScript = newScript;

                // Additional operations or logic can be performed here after adding the new script.
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ScriptDelCommandFunc()
        {
            try
            {
                if (SelectedE84SimulControllerViewModel.SelectedScript != null && Scripts.Contains(SelectedE84SimulControllerViewModel.SelectedScript))
                {
                    int currentIndex = Scripts.IndexOf(SelectedE84SimulControllerViewModel.SelectedScript);
                    Scripts.Remove(SelectedE84SimulControllerViewModel.SelectedScript);

                    // Set the SelectedScript to the previous item if any items are remaining
                    if (currentIndex > 0)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript = Scripts[currentIndex - 1];
                    }
                    // Set the SelectedScript to the next item if the previous item was the first item
                    else if (currentIndex == 0 && Scripts.Count > 0)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript = Scripts[currentIndex];
                    }
                    // No items remaining, so set SelectedScript to null
                    else
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript = null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private void ScriptSaveCommandFunc()
        {
            try
            {
                // Convert the Scripts to JSON
                var json = JsonConvert.SerializeObject(Scripts);

                // Save the JSON to the file
                File.WriteAllText(ScriptPath, json);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ScriptLoadCommandFunc()
        {
            try
            {
                // Check Root Path 
                if (Directory.Exists(ScriptRootPath) == false)
                {
                    Directory.CreateDirectory(ScriptRootPath);
                }

                if (File.Exists(ScriptPath) == true)
                {
                    // Read the file and deserialize JSON to a type
                    var json = File.ReadAllText(ScriptPath);
                    Scripts = JsonConvert.DeserializeObject<ObservableCollection<E84Script>>(json);

                    if (Scripts != null && Scripts.Count > 0 && SelectedE84SimulControllerViewModel != null)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript = Scripts.FirstOrDefault();
                    }
                }
                else
                {
                    // File 없을 때
                    Scripts = new ObservableCollection<E84Script>();

                    var s_l = new E84Script("Script_1");
                    s_l.MakeLoadSequence();

                    Scripts.Add(s_l);

                    var s_ul = new E84Script("Script_2");
                    s_ul.MakeUnLoadSequence();

                    Scripts.Add(s_ul);

                    ScriptSaveCommandFunc();

                    if (Scripts != null && Scripts.Count > 0 && SelectedE84SimulControllerViewModel != null)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript = Scripts.FirstOrDefault();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SelectedModuleChangedCommandFunc()
        {
            try
            {
                // TODO
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ActionsDelCommandFunc()
        {
            try
            {

                if (SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction != null &&
                    SelectedE84SimulControllerViewModel.SelectedScript.Actions.Contains(SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction))
                {
                    int currentIndex = SelectedE84SimulControllerViewModel.SelectedScript.Actions.IndexOf(SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction);
                    SelectedE84SimulControllerViewModel.SelectedScript.Actions.Remove(SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction);

                    // Set the SelectedScript.SelectedAction to the previous item if any items are remaining
                    if (currentIndex > 0)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction = SelectedE84SimulControllerViewModel.SelectedScript.Actions[currentIndex - 1];
                    }
                    // Set the SelectedScript.SelectedAction to the next item if the previous item was the first item
                    else if (currentIndex == 0 && SelectedE84SimulControllerViewModel.SelectedScript.Actions.Count > 0)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction = SelectedE84SimulControllerViewModel.SelectedScript.Actions[currentIndex];
                    }
                    // No items remaining, so set SelectedScript.SelectedAction to null
                    else
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction = null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void BehaviorAddCommandFunc()
        {
            try
            {
                if (SelectedE84SimulControllerViewModel.SelectedScript != null &&
               SelectedE84SimulControllerViewModel.SelectedScript.Actions.Count > 0)
                {
                    ObservableCollection<SignalState> actions = new ObservableCollection<SignalState>(SelectedE84SimulControllerViewModel.SelectedScript.Actions);
                    E84Behavior newBehavior = SelectedE84SimulControllerViewModel.SelectedScript.CreateBehavior(actions, StartDelay, nextBehaviorDelay);
                    SelectedE84SimulControllerViewModel.SelectedScript.Behaviors.Add(newBehavior);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void BehaviorDelCommandFunc()
        {
            try
            {
                if (SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior != null &&
               SelectedE84SimulControllerViewModel.SelectedScript.Behaviors.Contains(SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior))
                {
                    int currentIndex = SelectedE84SimulControllerViewModel.SelectedScript.Behaviors.IndexOf(SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior);
                    SelectedE84SimulControllerViewModel.SelectedScript.Behaviors.Remove(SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior);

                    // Set the SelectedScript.SelectedBehavior to the previous item if any items are remaining
                    if (currentIndex > 0)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior = SelectedE84SimulControllerViewModel.SelectedScript.Behaviors[currentIndex - 1];
                    }
                    // Set the SelectedScript.SelectedBehavior to the next item if the previous item was the first item
                    else if (currentIndex == 0 && SelectedE84SimulControllerViewModel.SelectedScript.Behaviors.Count > 0)
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior = SelectedE84SimulControllerViewModel.SelectedScript.Behaviors[currentIndex];
                    }
                    // No items remaining, so set SelectedScript.SelectedBehavior to null
                    else
                    {
                        SelectedE84SimulControllerViewModel.SelectedScript.SelectedBehavior = null;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AddAction(string name, bool flag)
        {
            try
            {
                if (SelectedE84SimulControllerViewModel.SelectedScript != null)
                {
                    SignalState signalState = new SignalState(name, flag, this.nextActionDelay);

                    SelectedE84SimulControllerViewModel.SelectedScript.Actions.Add(signalState);

                    SelectedE84SimulControllerViewModel.SelectedScript.SelectedAction = signalState;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void LightCurtainOnOffCommandFunc(object parameter)
        {
            try
            {
                if (parameter is bool flag)
                {
                    if(SelectedE84SimulControllerViewModel != null && SelectedE84SimulControllerViewModel.Simulator  != null)
                    {
                        SelectedE84SimulControllerViewModel.Simulator.LightCurtainOnOff(flag);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AddActionCommandFunc(object parameter)
        {
            try
            {
                if (parameter is E84Execute param)
                {
                    bool flag = param.Flag;
                    object Type = param.Type;

                    AddAction(Type.ToString(), flag);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}


