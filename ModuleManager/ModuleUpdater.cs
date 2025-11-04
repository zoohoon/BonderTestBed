using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
namespace ModuleManager
{
    using ProberInterfaces;
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using ProberErrorCode;
    using LogModule;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    public class ModuleUpdater : INotifyPropertyChanged, IModuleUpdater
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        MetroWindow metroWindow;
        ModuleUpdaterControl moduleUpdaterControl;

        private ObservableCollection<IModuleInformation> _ModuleInfos = new ObservableCollection<IModuleInformation>();
        public ObservableCollection<IModuleInformation> ModuleInfos
        {
            get { return _ModuleInfos; }
            set
            {
                if (value != _ModuleInfos)
                {
                    _ModuleInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _CircProgressVisibility = Visibility.Hidden;
        public Visibility CircProgressVisibility
        {
            get { return _CircProgressVisibility; }
            set
            {
                if (value != _CircProgressVisibility)
                {
                    _CircProgressVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }


        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            //LoggerManager.Debug($"ModuleUpdater has been deinitilized.");
            //LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] : ModuleUpdater has been deinitilized.");
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    metroWindow = this.MetroDialogManager().GetMetroWindow() as MetroWindow;

                    moduleUpdaterControl = new ModuleUpdaterControl();
                    moduleUpdaterControl.DataContext = this;

                    retval = AddDefaultModeuls();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"AddDefaultModeuls() Failed");
                    }

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum AddDefaultModeuls()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ModuleInfos.Clear();
                var moduleInfo = new ModuleInformation();
                moduleInfo.AssemblyInformation = new AssemblyInfo();
                moduleInfo.ModuleName = "High magnitude alignment module";
                moduleInfo.Description = "Wafer Alignment: high magnitude wafer alignment module for BUMP.";
                moduleInfo.InstalledVersion = new Version(1, 1, 5, 1);
                moduleInfo.RecentVersion = new Version(1, 1, 5, 1);
                moduleInfo.IsNeedUpdate = false;
                ModuleInfos.Add(moduleInfo);

                moduleInfo = new ModuleInformation();
                moduleInfo.AssemblyInformation = new AssemblyInfo();
                moduleInfo.ModuleName = "Low magnitude alignment module";
                moduleInfo.Description = "Wafer Alignment: low magnitude wafer alignment module for BUMP.";
                moduleInfo.InstalledVersion = new Version(1, 1, 3, 1);
                moduleInfo.RecentVersion = new Version(1, 1, 3, 1);
                moduleInfo.IsNeedUpdate = false;
                ModuleInfos.Add(moduleInfo);

                moduleInfo = new ModuleInformation();
                moduleInfo.AssemblyInformation = new AssemblyInfo();
                moduleInfo.ModuleName = "Index Alignment module";
                moduleInfo.Description = "Wafer Alignment: Index alignment module for BUMP.";
                moduleInfo.InstalledVersion = new Version(1, 1, 0, 1);
                moduleInfo.RecentVersion = new Version(1, 1, 0, 1);
                moduleInfo.IsNeedUpdate = false;
                ModuleInfos.Add(moduleInfo);

                moduleInfo = new ModuleInformation();
                moduleInfo.AssemblyInformation = new AssemblyInfo();
                moduleInfo.ModuleName = "Height measurement module[BUMP]";
                moduleInfo.Description = "Height measurement module for BUMP.";
                moduleInfo.InstalledVersion = new Version(1, 1, 2, 1);
                moduleInfo.RecentVersion = new Version(1, 1, 2, 1);
                moduleInfo.IsNeedUpdate = false;
                ModuleInfos.Add(moduleInfo);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            IParam tmpParam = null;
            tmpParam = new ModuleUpdaterParam();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

            RetVal = this.LoadParameter(ref tmpParam, typeof(ModuleUpdaterParam));

            if (RetVal == EventCodeEnum.NONE)
            {
                Param = tmpParam as ModuleUpdaterParam;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
            //string RootPath = FileManager.FileManagerParam.SystemParamRootDirectory;
            //string filePath = Param.FilePath;
            //string fileName = Param.FileName;
            //string fullPath = $"{RootPath}{filePath}\\{fileName}";

            string FullPath = this.FileManager().GetSystemParamFullPath(Param.FilePath, Param.FileName);

            try
            {
                RetVal = this.SaveParameter(Param, fixFullPath: FullPath);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"SaveParameter(): Serialize Error");
                    return RetVal;
                }
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "SaveParameter(): error occurred.");
                LoggerManager.Exception(err);
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return RetVal;
        }

        public EventCodeEnum LoadParameter()
        {
            return LoadSysParameter();
        }

        public EventCodeEnum SaveParameter()
        {
            return SaveSysParameter();
        }

        public async Task RetrieveFromSources()
        {
            try
            {
            CircProgressVisibility = Visibility.Visible;
             System.Threading.Thread.Sleep(2500);
            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //ModuleInfos.Clear();
                    //var moduleInfo = new ModuleInformation();
                    //moduleInfo.AssemblyInformation = new AssemblyInfo();
                    //moduleInfo.ModuleName = "Height measure module";
                    //moduleInfo.Description = "Wafer Alignment Height measure module for BUMP wafer.";
                    //moduleInfo.InstalledVersion = new Version(1, 0, 1, 1);
                    //moduleInfo.RecentVersion = new Version(1, 0, 2, 1);
                    //moduleInfo.IsNeedUpdate = true;
                    //ModuleInfos.Add(moduleInfo);

                    var count = ModuleInfos.Count;
                    ModuleInfos[count - 1].RecentVersion = new Version(1, 1, 3, 1);
                    if (ModuleInfos[count - 1].RecentVersion != ModuleInfos[count - 1].InstalledVersion)
                    {
                        ModuleInfos[count - 1].IsNeedUpdate = true;
                    }
                }));
            });
            CircProgressVisibility = Visibility.Hidden;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private AsyncCommand _ShowPanelCommand;
        public ICommand ShowPanelCommand
        {
            get
            {
                if (null == _ShowPanelCommand) _ShowPanelCommand
                        = new AsyncCommand(ShowPanel);
                return _ShowPanelCommand;
            }
        }
        private AsyncCommand _ClosePanelCommand;
        public ICommand ClosePanelCommand
        {
            get
            {
                if (null == _ClosePanelCommand) _ClosePanelCommand
                        = new AsyncCommand(ClosePanel);
                return _ClosePanelCommand;
            }
        }
        private AsyncCommand _UpdatePanelCommand;
        public ICommand UpdatePanelCommand
        {
            get
            {
                if (null == _UpdatePanelCommand) _UpdatePanelCommand
                        = new AsyncCommand(RetrieveFromSources);
                return _UpdatePanelCommand;
            }
        }

        private async Task ClosePanel()
        {
            await metroWindow.HideMetroDialogAsync(moduleUpdaterControl);
        }

        private ModuleUpdaterParam _Param;
        public ModuleUpdaterParam Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    RaisePropertyChanged();
                }
            }
        }

        public async Task ShowPanel()
        {
            try
            {
            await metroWindow.ShowMetroDialogAsync(moduleUpdaterControl);
            await moduleUpdaterControl.WaitUntilUnloadedAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }


        //public string FilePath { get; } = "SystemParam\\";

        //public string FileName { get; } = "ModuleUpdaterParam.xml";
    }
    [Serializable]
    public class ModuleUpdaterParam : INotifyPropertyChanged, ISystemParameterizable, IFactoryModule, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.PARAM_ERROR;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public string Genealogy { get; set; }
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

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public ModuleUpdaterParam()
        {
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {

            Updatesources.Value = new ObservableCollection<string>();

            try
            {
                Updatesources.Value.Add($"{RootPath}\\Modules");

                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, $"SetDefaultParam error occurred.");
                LoggerManager.Exception(err);

                ret = EventCodeEnum.SYSTEM_ERROR;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return ret;
        }
        public string FileName { get; } = "ModuleUpdaterParam.Json";
        public string FilePath { get; } = "";

        private Element<ObservableCollection<string>> _Updatesources = new Element<ObservableCollection<string>>();
        public Element<ObservableCollection<string>> Updatesources
        {
            get { return _Updatesources; }
            set
            {
                if (value != _Updatesources)
                {
                    _Updatesources = value;
                    NotifyPropertyChanged("Updatesources");
                }
            }
        }

        //  string ISystemParameterizable.FilePath => throw new NotImplementedException();
    }
    public class ModuleInformation : INotifyPropertyChanged, IModuleInformation
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private AssemblyInfo _AssemblyInformation = new AssemblyInfo();
        public AssemblyInfo AssemblyInformation
        {
            get { return _AssemblyInformation; }
            set
            {
                if (value != _AssemblyInformation)
                {
                    _AssemblyInformation = value;
                    NotifyPropertyChanged("AssemblyInformation");
                }
            }
        }
        private AsyncCommand _UpdateCommand;
        public ICommand UpdateCommand
        {
            get
            {
                if (null == _UpdateCommand) _UpdateCommand
                        = new AsyncCommand(Update);
                return _UpdateCommand;
            }
        }

        private Task Update()
        {
            try
            {
            return Task.Run(() =>
            {
                InstalledVersion = (Version)RecentVersion.Clone();
                IsNeedUpdate = false;
            });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private string _ModuleName = string.Empty;
        public string ModuleName
        {
            get { return _ModuleName; }
            set
            {
                if (value != _ModuleName)
                {
                    _ModuleName = value;
                    NotifyPropertyChanged("ModuleName");
                }
            }
        }

        private string _Description = string.Empty;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        private Version _InstalledVersion = new Version();
        public Version InstalledVersion
        {
            get { return _InstalledVersion; }
            set
            {
                if (value != _InstalledVersion)
                {
                    _InstalledVersion = value;
                    NotifyPropertyChanged("InstalledVersion");
                }
            }
        }
        private Version _RecentVersion = new Version();
        public Version RecentVersion
        {
            get { return _RecentVersion; }
            set
            {
                if (value != _RecentVersion)
                {
                    _RecentVersion = value;
                    NotifyPropertyChanged("RecentVersion");
                }
            }
        }
        private bool _IsNeedUpdate;
        public bool IsNeedUpdate
        {
            get { return _IsNeedUpdate; }
            set
            {
                if (value != _IsNeedUpdate)
                {
                    _IsNeedUpdate = value;
                    NotifyPropertyChanged("IsNeedUpdate");
                }
            }
        }
    }
}
