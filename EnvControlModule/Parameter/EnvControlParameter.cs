

namespace EnvControlModule.Parameter
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.EnvControl.Parameter;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class EnvControlParameter : IParam, ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        #region .. ISystemParameterizable Property
        public string FilePath { get; } = "EnvControl";
        public string FileName { get; } = "EnvControlParam.json";
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }
        #endregion

        #region .. Property

        private EnumEnvControlModuleMode _EnvControlMode;
        public EnumEnvControlModuleMode EnvControlMode
        {
            get { return _EnvControlMode; }
            set
            {
                if (value != _EnvControlMode)
                {
                    _EnvControlMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FFUSysParameter _FFUSysParam
            = new FFUSysParameter();
        public FFUSysParameter FFUSysParam
        {
            get { return _FFUSysParam; }
            set
            {
                if (value != _FFUSysParam)
                {
                    _FFUSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private ChillerSysParameter _ChillerSysParam
             = new ChillerSysParameter();
        public ChillerSysParameter ChillerSysParam
        {
            get { return _ChillerSysParam; }
            set
            {
                if (value != _ChillerSysParam)
                {
                    _ChillerSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DryAirSysParameter _DryAirSysParam
             = new DryAirSysParameter();
        public DryAirSysParameter DryAirSysParam
        {
            get { return _DryAirSysParam; }
            set
            {
                if (value != _DryAirSysParam)
                {
                    _DryAirSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ValveSysParameter _ValveSysParam
             = new ValveSysParameter();
        public ValveSysParameter ValveSysParam
        {
            get { return _ValveSysParam; }
            set
            {
                if (value != _ValveSysParam)
                {
                    _ValveSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        #endregion

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
                {
                    //if(EnvControlMode != EnumEnvControlModuleMode.REMOTE)
                    //{
                    //    EnvControlMode = EnumEnvControlModuleMode.REMOTE;
                    //}
                    //if(ValveSysParam.ValveModuleType.Value != EnumValveModuleType.REMOTE)
                    //{
                    //    ValveSysParam.ValveModuleType.Value = EnumValveModuleType.REMOTE;
                    //}
                }

                else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    if (ValveSysParam != null && ValveSysParam.DryAirMappingParam != null && ValveSysParam.DryAirMappingParam.Count == 0)
                    {
                        ValveSysParam.SetGPDryAirMapping();
                    }
                }

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitParam();

                FFUSysParam.SetDefaultParam();
                ChillerSysParam.SetDefaultParam();
                DryAirSysParam.SetDefaultParam();
                ValveSysParam.SetDefaultParam();
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void InitParam()
        {
            if (System.AppDomain.CurrentDomain.FriendlyName == "ProberSystem.exe")
            {
                EnvControlMode = EnumEnvControlModuleMode.REMOTE;
            }

            else if (System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
            {
                EnvControlMode = EnumEnvControlModuleMode.LOCAL;
            }

        }

        public void SetElementMetaData()
        {
            try
            {
                ChillerSysParam.SetElementMetaData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
