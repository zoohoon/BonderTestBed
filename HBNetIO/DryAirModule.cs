using System;
using System.Collections.Generic;

namespace Temperature.Temp.DryAir
{
    using LogModule;
    using HBDryAir.Processor;
    using ProberInterfaces.EnvControl.Parameter;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    using SerializerUtil;

    public class DryAirModule : IDryAirModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region .. Property
        public bool Initialized { get; set; } = false;

        private IDryAirManager _Manager;
        public IDryAirManager Manager
        {
            get { return _Manager; }
            set { _Manager = value; }
        }

        private IDryAirController _Processor;
        public IDryAirController Processor
        {
            get { return _Processor; }
            set { _Processor = value; }
        }

        private DryAirSysParameter _DryAirSysParam;
        public DryAirSysParameter DryAirSysParam
        {
            get { return _DryAirSysParam; }
            set { _DryAirSysParam = value; }
        }

        private IIOService _IOServ;
        public IIOService IOServ
        {
            get { return this.IOManager().IOServ; }
            set { _IOServ = value; }
        }

        private DryAirNetIOMappings _HBNetIOMap;
        public DryAirNetIOMappings HBNetIOMap
        {
            get { return _HBNetIOMap; }
        }

        private HashSet<IOPortDescripter<bool>> OutPorts = new HashSet<IOPortDescripter<bool>>();
        private HashSet<IOPortDescripter<bool>> InPorts = new HashSet<IOPortDescripter<bool>>();

        //private double _DryAirActivableHighTemp;

        public double DryAirActivableHighTemp
        {
            get { return DryAirSysParam?.ActivatableHighTemp?.Value ?? 0 ; }
            set { DryAirSysParam.ActivatableHighTemp.Value = value; }
        }

        #endregion


        public DryAirModule()
        {

        }
        public DryAirModule(DryAirSysParameter param, IDryAirManager manager)
        {
            DryAirSysParam = param;
            Manager = manager;
        }

        bool isDisposed = false;
        public void Dispose()
        {
            try
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {

                    if (DryAirSysParam.DryAirModuleMode.Value == EnumDryAirModuleMode.HUBER)
                    {
                        if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Remote)
                            Processor = new DryAirLoaderCommander();
                        else if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                            Processor = new DryAirCommander();
                    }
                    else if (DryAirSysParam.DryAirModuleMode.Value == EnumDryAirModuleMode.REMOTE)
                    {
                        Processor = new DryAirRemote();
                        //InitRemoteParam(Processor.GetDryAirParam());
                    }

                    if (Processor != null)
                    {
                        Processor.InitModule();

                        if (SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                        {
                            //DryAirForProber(false, EnumDryAirType.STG);
                            //DryAirForProber(false, EnumDryAirType.STGBOTTOM);
                            //DryAirForProber(false, EnumDryAirType.LOADER);

                            //DryAirForProber(true, EnumDryAirType.STG);
                            //DryAirForProber(true, EnumDryAirType.STGBOTTOM);
                            //DryAirForProber(true, EnumDryAirType.LOADER);
                        }
                    }


                    isDisposed = false;
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    //if(Processor != null)
                    //{
                    //    Processor.InitModule();
                    //}
                    //LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private void InitRemoteParam(byte[] param)
        {
            try
            {
                if(param != null)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(DryAirSysParameter));
                    if(target != null)
                    {
                        DryAirSysParameter sysparam = (DryAirSysParameter)target;
                        DryAirSysParam.ActivatableHighTemp = sysparam.ActivatableHighTemp;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
                this.Dispose();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = LoadHBIOMapDefinitions();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadHBIOMapDefinitions()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(DryAirNetIOMappings));
                if (RetVal == EventCodeEnum.NONE)
                {
                    _HBNetIOMap = tmpParam as DryAirNetIOMappings;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                LoggerManager.Exception(err);

                throw;
            }

            return RetVal;
        }
        public List<IOPortDescripter<bool>> GetInputPorts()
        {
            List<IOPortDescripter<bool>> retVal = new List<IOPortDescripter<bool>>();

            Type type = _HBNetIOMap.Inputs.GetType();
            PropertyInfo[] propertyinfos = type.GetProperties();

            foreach (PropertyInfo property in propertyinfos)
            {
                IOPortDescripter<bool> port = property.GetValue(_HBNetIOMap.Inputs) as IOPortDescripter<bool>;

                if (port != null)
                {
                    retVal.Add(port);
                }
            }

            return retVal;
        }

        public List<IOPortDescripter<bool>> GetOutputPorts()
        {
            List<IOPortDescripter<bool>> retVal = new List<IOPortDescripter<bool>>();

            Type type = _HBNetIOMap.Outputs.GetType();
            PropertyInfo[] propertyinfos = type.GetProperties();

            foreach (PropertyInfo property in propertyinfos)
            {
                IOPortDescripter<bool> port = property.GetValue(_HBNetIOMap.Outputs) as IOPortDescripter<bool>;

                if (port != null)
                {
                    retVal.Add(port);
                }
            }

            return retVal;
        }

        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            return Processor.DryAirForProber(value, dryairType, stageIndex);
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageIndex = -1)
        {
            return Processor.GetLeakSensor(out value, leakSensorIndex, stageIndex);
        }
        public bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1)
        {
            return Processor.GetDryAirState(dryairType, stageIndex);
        }

        public byte[] GetDryAirParam(int stageIndex = -1)
        {
            byte[] param = null;
            try
            {
                param = SerializeManager.SerializeToByte(DryAirSysParam, typeof(DryAirSysParameter));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }

        public EventCodeEnum Execute()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    retVal = ExecuteGroupProber();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        private EventCodeEnum ExecuteGroupProber()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (Processor != null)
                {
                    if (Manager.GetMode() == EnumDryAirModuleMode.REMOTE)
                    {
                        if (this.EnvControlManager().GetEnvcontrolMode() == EnumEnvControlModuleMode.REMOTE)
                        {
                            if (this.LoaderController().GetconnectFlag() == false)
                            {
                                return retVal = EventCodeEnum.ENVCONTROL_NOT_CONNECTED;
                            }
                        }
                        else
                        {
                            return retVal = EventCodeEnum.ENVCONTROL_NOT_CONNECTED;
                        }
                    }
                    double curTemp = this.TempController().TempInfo.CurTemp.Value;
                    double targetTemp = this.TempController().TempInfo.TargetTemp.Value;
                    double dryAirActivableHighTemp = DryAirActivableHighTemp;

                    //현재온도가  DryAirActivableHighTemp 보다 낮으면 ON
                    if (curTemp <= dryAirActivableHighTemp)
                    {
                        if (!GetDryAirState(EnumDryAirType.STG))
                        {
                            LoggerManager.Debug($"[DryAirModule] Dry Air turn on. Cut Temp : {curTemp}, DryAirActivableHighTemp : {dryAirActivableHighTemp}");
                            retVal = DryAirForProber(true, EnumDryAirType.STG);
                            if (retVal != EventCodeEnum.NONE)
                            {
                                LoggerManager.Debug($"Valve state invalid. Ret = {retVal}");
                            }
                        }
                    }
                    else //Chiller 사용시 현재온도가  DryAirActivableHighTemp 보다 높으면
                    {
                        if (this.EnvControlManager().GetChillerModule() != null && this.EnvControlManager().GetChillerModule().GetCommState() == ProberInterfaces.Enum.EnumCommunicationState.CONNECTED)
                        {
                            // 칠러 사용 시
                            // 칠러의 Ambient 온도보다 Target 온도가 낮으면 On
                            // Dew Point 가 Chiller 의 SV 보다 높으면 ON
                            double chillerAmbientTemp = this.EnvControlManager().GetChillerModule().ChillerParam?.AmbientTemp?.Value ?? 0;
                            double chillerSV = this.EnvControlManager().GetChillerModule().ChillerInfo?.SetTemp ?? 0;
                            double dewPoint = this.EnvControlManager().GetDewPointModule().CurDewPoint;
                            if ((targetTemp <= chillerAmbientTemp) || dewPoint >= chillerSV)
                            {
                                if (!GetDryAirState(EnumDryAirType.STG))
                                {
                                    LoggerManager.Debug($"[DryAirModule] Dry Air turn on. Target Temp : {targetTemp}, Chiller AmbientTemp : {chillerAmbientTemp}");
                                    LoggerManager.Debug($"[DryAirModule] Dry Air turn on. Cut Temp : {curTemp}, DryAirActivableHighTemp : {dryAirActivableHighTemp}");
                                    retVal = DryAirForProber(true, EnumDryAirType.STG);
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"Valve state invalid. Ret = {retVal}");
                                    }
                                }
                            }
                            else
                            {
                                if (GetDryAirState(EnumDryAirType.STG))
                                {
                                    LoggerManager.Debug($"[DryAirModule] Dry Air turn off. Target Temp : {targetTemp}, Chiller AmbientTemp : {chillerAmbientTemp}");
                                    LoggerManager.Debug($"[DryAirModule] Dry Air turn off. Cut Temp : {curTemp}, DryAirActivableHighTemp : {dryAirActivableHighTemp}");
                                    retVal = DryAirForProber(false, EnumDryAirType.STG);
                                    if (retVal != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"Valve state invalid. Ret = {retVal}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

    }
}
