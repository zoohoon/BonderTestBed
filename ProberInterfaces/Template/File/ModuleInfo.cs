using LogModule;
using System;

namespace ProberInterfaces
{
    using ProberInterfaces.State;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberInterfaces.PnpSetup.StepInfo;
    using Newtonsoft.Json;

    [Serializable]

    public class ModuleInfo : CategoryModuleBase, IModuleInfo, INotifyPropertyChanged
    {

        #region ..Property

        private ObservableCollection<IProcessingModule> _AlignModule
            = new ObservableCollection<IProcessingModule>();
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<IProcessingModule> AlignModule
        {
            get { return _AlignModule; }
            set
            {
                if (value != _AlignModule)
                {
                    _AlignModule = value;
                    NotifyPropertyChanged("AlignModule");
                }
            }
        }


        private ModuleDllInfo _DllInfo;
        public ModuleDllInfo DllInfo
        {
            get { return _DllInfo; }
            set
            {
                if (value != _DllInfo)
                {
                    _DllInfo = value;
                    NotifyPropertyChanged("DllInfo");
                }
            }
        }

        private EnumEnableState _SetupEnableState
             = EnumEnableState.ENABLE;
        public EnumEnableState SetupEnableState
        {
            get { return _SetupEnableState; }
            set
            {
                if (value != _SetupEnableState)
                {
                    _SetupEnableState = value;
                    NotifyPropertyChanged("SetupEnableState");
                }
            }
        }

        #endregion

        public ModuleInfo()
        {

        }
        public ModuleInfo(ModuleDllInfo module)
        {
            DllInfo = module;
        }
        public ModuleInfo(ModuleDllInfo module, EnumEnableState enablestate)
        {
            try
            {
            DllInfo = module;
            SetupEnableState = enablestate;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public ModuleInfo(string dllpath, string assemblyname, string Objectname, int version, bool enablebackwardcompatibility)
        {
            try
            {
            DllInfo.DLLPath = dllpath;
            DllInfo.AssemblyName = assemblyname;
            DllInfo.Version = version;
            DllInfo.EnableBackwardCompatibility = enablebackwardcompatibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public ModuleInfo(string dllpath, string assemblyname, int version, bool enablebackwardcompatibility)
        {
            try
            {
            DllInfo.DLLPath = dllpath;
            DllInfo.AssemblyName = assemblyname;
            DllInfo.Version = version;
            DllInfo.EnableBackwardCompatibility = enablebackwardcompatibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public ModuleInfo(string dllpath, string assemblyname, string classname, int version, string parampath, string paramname, bool enablebackwardcompatibility)
        {
            try
            {
            DllInfo.DLLPath = dllpath;
            DllInfo.AssemblyName = assemblyname;
            DllInfo.ClassName.Add(classname);
            DllInfo.Version = version;
            DllInfo.ParamPath = parampath;
            DllInfo.ParamName = paramname;
            DllInfo.EnableBackwardCompatibility = enablebackwardcompatibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public ModuleInfo(string dllpath, string assemblyname, ObservableCollection<string> classname, int version, string parampath, string paramname, bool enablebackwardcompatibility)
        {
            try
            {
            DllInfo.DLLPath = dllpath;
            DllInfo.AssemblyName = assemblyname;
            DllInfo.ClassName = classname;
            DllInfo.Version = version;
            DllInfo.ParamPath = parampath;
            DllInfo.ParamName = paramname;
            DllInfo.EnableBackwardCompatibility = enablebackwardcompatibility;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public ModuleInfo(string dllpath, string assemblyname, int version)
        {
            try
            {
            DllInfo.DLLPath = dllpath;
            DllInfo.AssemblyName = assemblyname;
            DllInfo.Version = version;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
