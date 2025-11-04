using System;

namespace ControlModules
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Temperature.DryAir;
    using ProberInterfaces.EnvControl.Parameter;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Temperature.Temp.DryAir;
    using System.Threading;
    using System.Threading.Tasks;

    public class DryAirManager : INotifyPropertyChanged, IFactoryModule, IDryAirManager
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

        private DryAirSysParameter _DryAirSysParam;
        public DryAirSysParameter DryAirSysParam
        {
            get { return _DryAirSysParam; }
            set { _DryAirSysParam = value; }
        }

        Thread UpdateThread = null;
        private bool bIsUpdating = true;

        private IDryAirModule _DryAirModule;
        public IDryAirModule DryAirModule
        {
            get { return _DryAirModule; }
            set { _DryAirModule = value; }
        }

        #endregion

        #region .. Method
        public DryAirManager()
        {

        }
        public DryAirManager(DryAirSysParameter param)
        {
            DryAirSysParam = param;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {
                    if(DryAirSysParam.DryAirModuleMode.Value == EnumDryAirModuleMode.HUBER
                         | DryAirSysParam.DryAirModuleMode.Value == EnumDryAirModuleMode.REMOTE)
                    {
                        DryAirModule = new DryAirModule(DryAirSysParam, this);
                        DryAirModule.LoadSysParameter();
                        retVal = DryAirModule.InitModule();
                    }
                    else if(DryAirSysParam.DryAirModuleMode.Value == EnumDryAirModuleMode.EMUL)
                    {
                        DryAirModule = new EmulDryAirModule(DryAirSysParam, this);
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        // DryAirModule을 만들지 않는다.
                        retVal = EventCodeEnum.NONE;
                    }

                    if(DryAirModule != null 
                        && SystemManager.SysExcuteMode == SystemExcuteModeEnum.Prober)
                    {
                        UpdateThread = new Thread(new ThreadStart(UpdateProc));
                        bIsUpdating = true;
                        UpdateThread.Name = $"Dry Air Update thread";
                        UpdateThread.Start();
                    }

                    Initialized = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void DeInitModule()
        {
            try
            {
                DryAirModule?.DeInitModule();
                bIsUpdating = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                DryAirModule?.InitConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        //public EventCodeEnum InitServiceHost()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        ServiceHost = new DryAirServiceHost() { Manager = this };
        //        if (ServiceHost != null)
        //            ServiceHost.InitModule();
        //        retVal = EventCodeEnum.NONE;
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return retVal;
        //}

        public EnumDryAirModuleMode GetMode(int stageindex = -1)
        {
            EnumDryAirModuleMode retVal = EnumDryAirModuleMode.NONE;
            try
            {
                //retVal = _DryAirCommParam.DryAirModuleMode.Value;

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {

                    retVal = _DryAirSysParam.DryAirModuleMode.Value;
                }
                else if (SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    retVal = _DryAirSysParam.DryAirModuleMode.Value;

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void InitService()
        {
            return;
        }

        public void UpdateProc()
        {
            bool isErrorHandled = false;
            try
            {
                while (bIsUpdating)
                {
                    DryAirModule.Execute();
                    try
                    {
                        if (!bIsUpdating)
                        {
                            break;
                        }
                        DryAirModule.Execute();

                        if (isErrorHandled)
                        {
                            isErrorHandled = false;
                            LoggerManager.Debug($"[DryAirManager] Recovery thread.");
                        }
                    }
                    catch (Exception err)
                    {
                        if (isErrorHandled == false)
                        {
                            isErrorHandled = true;
                            LoggerManager.Exception(err);
                        }
                    }
                    finally
                    {
                        Thread.Sleep(3000);
                    }
                }
                UpdateThread = null;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"DryAirManager.UpdateProc(): Thread Abort.");
                LoggerManager.Exception(err);
                UpdateThread = null;
            }
        }

        /// <summary>
        /// Prober의 DryAir 를 모두 컨트롤하는 함수
        /// </summary>
        /// <param name="value"></param>   : ON/OFF
        /// <param name="stageIndex"></param>
        /// <returns></returns>
        public EventCodeEnum DryAirForProber(bool value, EnumDryAirType dryairType, int stageIndex = -1)
        {
            return DryAirModule?.DryAirForProber(value, dryairType, stageIndex) ?? EventCodeEnum.DRYAIR_SETVALUE_ERROR;
        }
        public bool GetDryAirState(EnumDryAirType dryairType, int stageIndex = -1)
        {
            return DryAirModule?.GetDryAirState(dryairType, stageIndex) ?? false;
        }
        public int GetLeakSensor(out bool value, int leakSensorIndex = 0, int stageindex = -1)
        {
            value = false;
            return DryAirModule?.GetLeakSensor(out value, leakSensorIndex) ?? -1;
        }

        public byte[] GetDryAirParam(int stageindex = -1)
        {
            return DryAirModule.GetDryAirParam(stageindex);
        }

        #endregion
    }
}
