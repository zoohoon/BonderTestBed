using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace E84
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using Microsoft.VisualBasic;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.E84;
    using ProberInterfaces.E84.ProberInterfaces;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class E84Module : IE84Module, INotifyPropertyChanged, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> Property </remarks>

        public bool Initialized { get; set; } = false;

        private E84Parameters _E84ParameterObject;
        public E84Parameters E84ParameterObject
        {
            get { return _E84ParameterObject; }
            set
            {
                if (value != _E84ParameterObject)
                {
                    _E84ParameterObject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84Parameters _E84SysParam;
        public IE84Parameters E84SysParam
        {
            get { return _E84SysParam; }
            set
            {
                if (value != _E84SysParam)
                {
                    _E84SysParam = (E84Parameters)value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<IE84Controller> _E84Controllers = new List<IE84Controller>();

        public List<IE84Controller> E84Controllers
        {
            get { return _E84Controllers; }
            set { _E84Controllers = value; }
        }
        private bool IsCassetteAutoLock { get; set; }
        private bool IsCassetteAutoLockLeftOHT { get; set; }

        #endregion

        #region <remarks> Init & DeInit </remarks>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = InitModules();
                    //SetFoupCassetteLockOption();
                    //thread = new Thread(new ThreadStart(ExcuteRun));
                    //thread.Start();
                    //TODO조건이 들어가야한다. 

                    Initialized = true;

                    //retval = EventCodeEnum.NONE;
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
        public EventCodeEnum InitModules()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (E84SysParam.E84Moduls.Count > 0)
                {
                    foreach (var moduleparam in E84SysParam.E84Moduls)
                    {
                        if (moduleparam.E84ModuleType == E84CommModuleTypeEnum.EMOTIONTEK)
                        {
                            var e84module = new E84Controller();

                            retVal = e84module.InitModule(moduleparam, E84SysParam.E84PinSignal);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                E84Controllers.Add(e84module);
                            }
                            else
                            {
                                E84Controllers.Add(e84module);//Commodule이 생성 & 연결되지 않아도 Controller는 추가.
                                LoggerManager.Debug($" [{this.GetType().Name}] : InitModules(),  ComModule init fail. Return Value = {retVal}, E84Controller.Index = {moduleparam.FoupIndex}");
                            }
                        }
                        else if (moduleparam.E84ModuleType == E84CommModuleTypeEnum.EMUL)
                        {
                            var e84module = new E84EmulController();

                            retVal = e84module.InitModule(moduleparam);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                E84Controllers.Add(e84module);
                            }
                        }
                    }
                }
                else
                {
                    retVal = EventCodeEnum.NONE;
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
                foreach (var controller in E84Controllers)
                {
                    controller.DisConnectCommodule();
                }

                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> Parameter Methods </remarks>
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new E84Parameters();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(E84Parameters));

                if (RetVal == EventCodeEnum.NONE)
                {
                    E84SysParam = tmpParam as E84Parameters;
                    //SaveSysParameter();
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

                RetVal = SaveE84SysParam();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum LoadE84SysParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    IParam tmpParam = null;
                    RetVal = this.LoadParameter(ref tmpParam, typeof(E84Parameters));
                    if (RetVal == EventCodeEnum.NONE)
                    {
                        E84SysParam = tmpParam as E84Parameters;
                    }
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("[GPIB] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum SaveE84SysParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    RetVal = this.SaveParameter(_E84SysParam);
                    if (RetVal == EventCodeEnum.PARAM_ERROR)
                    {
                        LoggerManager.Error($"[E84] SaveSysParam(): Serialize Error");
                        return RetVal;
                    }
                    else
                    {
                        RetVal = SetFoupCassetteLockOption();
                    }
                    RetVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    RetVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("[E84] SaveSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public E84CassetteLockParam GetE84CassetteLockParam()
        {
            E84CassetteLockParam param = null;
            try
            {
                if (_E84SysParam != null)
                {
                    param = _E84SysParam.CassetteLockParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }
        public void SetE84CassetteLockParam(E84CassetteLockParam param)
        {
            try
            {
                if (_E84SysParam != null)
                {
                    _E84SysParam.CassetteLockParam.CopyFrom(param);
                    SetFoupCassetteLockOption();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region <remarks> Methods </remarks>
        public IE84Controller GetE84Controller(int foupindex, E84OPModuleTypeEnum oPModuleTypeEnum)
        {
            IE84Controller controller = null;
            try
            {
                controller = E84Controllers?.Find(ctoller => ctoller.E84ModuleParaemter.FoupIndex == foupindex && ctoller.E84ModuleParaemter.E84OPModuleType == oPModuleTypeEnum);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
            return controller;
        }
        public E84SignalTypeEnum PinConvertSignalType(E84SignalActiveEnum activeenum, int pin)
        {
            E84SignalTypeEnum type = E84SignalTypeEnum.INVALID;
            try
            {
                type = _E84SysParam.E84PinSignal.Find(param => param.Pin == pin && param.ActiveType == activeenum)?.Signal ?? E84SignalTypeEnum.INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return type;
        }
        public int SignalTypeConvertPin(E84SignalTypeEnum signal)
        {
            int pin = -1;
            try
            {
                pin = _E84SysParam.E84PinSignal.Find(param => param.Signal == signal)?.Pin ?? -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pin;
        }
        public EventCodeEnum SetSignal(int foupindex, E84OPModuleTypeEnum oPModuleTypeEnum, E84SignalTypeEnum signal, bool flag)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var Controller = E84Controllers?.Find(controller => controller.E84ModuleParaemter.FoupIndex == foupindex && controller.E84ModuleParaemter.E84OPModuleType == oPModuleTypeEnum);
                if (Controller != null)
                {
                    retVal = Controller.SetSignal(signal, flag);
                    if (retVal == EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"[E84] - SetSignal() success. foupIndex : {foupindex}, signal : {signal}, flag : {flag}");
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84] - SetSignal() failed. foupIndex : {foupindex}, signal : {signal}, flag : {flag}");
                    }
                }
                else
                {
                    if (_E84SysParam != null)
                    {
                        if (_E84SysParam.E84Moduls != null)
                        {
                            var param = _E84SysParam.E84Moduls.SingleOrDefault(mparam => mparam.FoupIndex == foupindex && mparam.E84OPModuleType == oPModuleTypeEnum);
                            if (param != null)
                            {
                                if (param.E84_Attatched == true || param.E84ModuleType == E84CommModuleTypeEnum.EMOTIONTEK)
                                {
                                    LoggerManager.Debug($"[E84] - SetSignal() failed. not exist controlller. foupIndex : {foupindex}, signal : {signal}, flag : {flag}");
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

        
        public bool GetSignal(int foupindex, E84SignalTypeEnum signal)
        {
            bool retVal = false;
            try
            {
                var Controller = E84Controllers?.Find(controller => controller.E84ModuleParaemter.FoupIndex == foupindex);
                if (Controller != null)
                {
                    retVal = Controller.GetSignal(signal);
                }
            }
            catch (Exception err)
            {
                retVal = false;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetClampSignal(int foupindex, bool onflag)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var Controller = E84Controllers?.Find(controller => controller.E84ModuleParaemter.FoupIndex == foupindex && controller.E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP);
                if (Controller != null)
                {
                    retVal = Controller.SetClampSignal(onflag);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool GetIsCDBypass()
        {
            return _E84SysParam?.IsCDBypass ?? false;
        }
        public double GetCDBypassDelayTimeInSec()
        {
            return _E84SysParam?.CDBypassDelayTimeInSec ?? 0.00;
        }
        public void SetIsCassetteAutoLock(bool flag)
        {
            IsCassetteAutoLock = flag;
        }
        public void SetIsCassetteAutoLockLeftOHT(bool flag)
        {
            IsCassetteAutoLockLeftOHT = flag;
        }
        public E84PresenceTypeEnum GetE84PreseceType()
        {
            return _E84SysParam?.E84PreseceType.Value ?? E84PresenceTypeEnum.UNDEFINE;
        }
        public void SetE84PreseceType(E84PresenceTypeEnum e84PresenceType)
        {
            try
            {
                if (_E84SysParam != null)
                {
                    _E84SysParam.E84PreseceType.Value = e84PresenceType;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public long GetTimeoutOnPresenceAfterOnExistSensor()
        {
            return _E84SysParam?.TimeoutOnPresenceAfterOnExistSensorMs.Value ?? 3000;
        }
        public void SetTimeoutOnPresenceAfterOnExistSensor(long timeout)
        {
            try
            {
                if (_E84SysParam != null)
                {
                    _E84SysParam.TimeoutOnPresenceAfterOnExistSensorMs.Value = timeout;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum SetFoupCassetteLockOption()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var FoupE84Controllers = E84Controllers.Where(item => item.E84ModuleParaemter.E84OPModuleType == E84OPModuleTypeEnum.FOUP);
                if (_E84SysParam.CassetteLockParam != null)
                {
                    if (_E84SysParam.CassetteLockParam.AutoSetCassetteLockEnable)
                    {
                        LoggerManager.Debug($"[E84 AutoSetCassetteLockEnable is enable");
                        foreach (var e84Controller in FoupE84Controllers)
                        {
                            var moduleParam = _E84SysParam.E84Moduls.FirstOrDefault(moduleinfo => moduleinfo.FoupIndex
                            == e84Controller.E84ModuleParaemter.FoupIndex);
                            if (moduleParam != null)
                            {
                                if (moduleParam.E84_Attatched)
                                {
                                    //if (moduleParam.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                                    //{
                                    var foupControlller = this.FoupOpModule().GetFoupController(e84Controller.E84ModuleParaemter.FoupIndex);

                                    CassetteLockModeEnum lockmode = CassetteLockModeEnum.UNDEFINED;
                                    if (e84Controller.CommModule.RunMode == E84Mode.AUTO)
                                    {
                                        LoggerManager.Debug($"[E84 #{e84Controller.E84ModuleParaemter.FoupIndex} RunMode] is {e84Controller.CommModule.RunMode}");
                                        if (_E84SysParam.CassetteLockParam.PortFullLockMode)
                                        {
                                            lockmode = _E84SysParam.CassetteLockParam.CassetteLockE84AutoMode;
                                            LoggerManager.Debug($"[E84 #{e84Controller.E84ModuleParaemter.FoupIndex} Auto LockOption] : {lockmode}");
                                        }
                                        else
                                        {
                                            E84PortLockOptionParam param = _E84SysParam.CassetteLockParam.E84PortEachLockOptions.
                                            ToList<E84PortLockOptionParam>().Find(options => options.FoupNumber == foupControlller.FoupModuleInfo.FoupNumber);


                                            if (param != null)
                                            {
                                                lockmode = param.CassetteLockE84AutoMode;
                                                LoggerManager.Debug($"[E84 #{e84Controller.E84ModuleParaemter.FoupIndex} Auto LockOption] : {lockmode}");
                                            }
                                        }

                                        if (lockmode == CassetteLockModeEnum.ATTACH)
                                        {
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLock = true;
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = false;
                                        }
                                        else if (lockmode == CassetteLockModeEnum.LEFTOHT)
                                        {
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLock = false;
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = true;
                                        }
                                        else if (lockmode == CassetteLockModeEnum.NORMAL)
                                        {
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLock = false;
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = false;
                                        }
                                    }
                                    else if (e84Controller.CommModule.RunMode == E84Mode.MANUAL || e84Controller.CommModule.RunMode == E84Mode.UNDEFIND)
                                    {
                                        LoggerManager.Debug($"[E84 #{e84Controller.E84ModuleParaemter.FoupIndex} Manual RunMode] is Manual");
                                        if (_E84SysParam.CassetteLockParam.PortFullLockMode)
                                        {
                                            lockmode = _E84SysParam.CassetteLockParam.CassetteLockE84ManualMode;
                                            LoggerManager.Debug($"[E84 #{e84Controller.E84ModuleParaemter.FoupIndex} Manual LockOption] : {lockmode}");
                                        }
                                        else
                                        {
                                            E84PortLockOptionParam param = _E84SysParam.CassetteLockParam.E84PortEachLockOptions.
                                            ToList<E84PortLockOptionParam>().Find(options => options.FoupNumber == foupControlller.FoupModuleInfo.FoupNumber);

                                            if (param != null)
                                            {
                                                lockmode = param.CassetteLockE84ManualMode;
                                                LoggerManager.Debug($"[E84 #{e84Controller.E84ModuleParaemter.FoupIndex} Manual LockOption] : {lockmode}");
                                            }
                                        }
                                        if (lockmode == CassetteLockModeEnum.ATTACH)
                                        {
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLock = true;
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = false;
                                        }
                                        else if (lockmode == CassetteLockModeEnum.LEFTOHT)
                                        {
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLock = false;
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = true;
                                        }
                                        else if (lockmode == CassetteLockModeEnum.NORMAL)
                                        {
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLock = false;
                                            foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = false;
                                        }
                                    }
                                    //}

                                }
                            }
                        }

                        if (this.GetLoaderContainer() != null)
                        {
                            // LOT 옵션과 중복 설정 되지 않도록 E84 Option 이 Enable 되면 LOT Option 은 Disalbe 하기.
                            ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                            if (loaderMaster != null)
                            {
                                if (loaderMaster.GetIsCassetteAutoLock())
                                {
                                    loaderMaster.SetIsCassetteAutoLock(false);
                                }
                                if (loaderMaster.GetIsCassetteAutoLockLeftOHT())
                                {
                                    loaderMaster.SetIsCassetteAutoLockLeftOHT(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84 AutoSetCassetteLockEnable is disable");
                        foreach (var e84Controller in FoupE84Controllers)
                        {
                            var moduleParam = _E84SysParam.E84Moduls.FirstOrDefault(moduleinfo => moduleinfo.FoupIndex
                            == e84Controller.E84ModuleParaemter.FoupIndex);
                            if (moduleParam != null)

                            {
                                //if (moduleParam.E84OPModuleType == E84OPModuleTypeEnum.FOUP)
                                //{
                                var foupControlller = this.FoupOpModule().GetFoupController(e84Controller.E84ModuleParaemter.FoupIndex);
                                foupControlller.FoupModuleInfo.IsCassetteAutoLock = IsCassetteAutoLock;
                                foupControlller.FoupModuleInfo.IsCassetteAutoLockLeftOHT = IsCassetteAutoLockLeftOHT;
                                //}

                            }
                        }
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
        public bool CheckE84Attatched(int index)
        {
            bool retVal = false;
            try
            {
                if (_E84SysParam != null)
                {
                    if (_E84SysParam.E84Moduls != null)
                    {
                        foreach (var moduleparam in E84SysParam.E84Moduls)
                        {
                            if (moduleparam.FoupIndex == index)
                            {
                                if (moduleparam.E84ModuleType == E84CommModuleTypeEnum.EMOTIONTEK
                                    || moduleparam.E84ModuleType == E84CommModuleTypeEnum.EMUL)
                                {
                                    retVal = moduleparam.E84_Attatched;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84] CheckE84Attatched() e84system.e84modules param is null");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[E84] CheckE84Attatched() e84system param is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        /// <summary>
        /// ComModule을 생성하고 Event 등록, 통신 Connect, Controller Tread Start, Commodule Tread Start
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EventCodeEnum SetAttatched(int index, E84OPModuleTypeEnum optype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (E84SysParam != null)
                {
                    if (E84SysParam.E84Moduls != null)
                    {
                        foreach (var moduleparam in E84SysParam.E84Moduls)
                        {
                            if (moduleparam.FoupIndex == index && moduleparam.E84OPModuleType == optype)
                            {
                                moduleparam.E84_Attatched = true;
                                var Controller = E84Controllers?.Find(controller => controller.E84ModuleParaemter.FoupIndex == index);
                                retVal = Controller.ConnectCommodule(moduleparam, E84SysParam.E84PinSignal);
                                SaveSysParameter();
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84] CheckE84Attatched() e84system.e84modules param is null");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[E84] CheckE84Attatched() e84system param is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        /// <summary>
        ///  Event 해제, 통신 DisConnect, Controller Tread Stop, Commodule Tread stop
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EventCodeEnum SetDetached(int index, E84OPModuleTypeEnum optype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (E84SysParam != null)
                {
                    if (E84SysParam.E84Moduls != null)
                    {
                        foreach (var moduleparam in E84SysParam.E84Moduls)
                        {
                            if (moduleparam.FoupIndex == index && moduleparam.E84OPModuleType == optype)
                            {
                                moduleparam.E84_Attatched = false;
                                var Controller = E84Controllers?.Find(controller => controller.E84ModuleParaemter.FoupIndex == index);
                                retVal = Controller.DisConnectCommodule();
                                SaveSysParameter();
                            }
                        }
                    }
                    else
                    {
                        LoggerManager.Debug($"[E84] CheckE84Attatched() e84system.e84modules param is null");
                    }
                }
                else
                {
                    LoggerManager.Debug($"[E84] CheckE84Attatched() e84system param is null");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ValidationFoupLoadedState(int foupNumber, ref string foupstateStr)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    if (this.GetLoaderContainer() != null)
                    {
                        ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        if (loaderMaster != null)
                        {
                            retVal = loaderMaster.ValidationFoupLoadedState(foupNumber, ref foupstateStr);
                        }
                    }
                }
                else
                {
                    // Single 장비 인 경우 확인 로직 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ValidationFoupUnloadedState(int foupNumber, ref string foupstateStr)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    if (this.GetLoaderContainer() != null)
                    {
                        ILoaderSupervisor loaderMaster = this.GetLoaderContainer().Resolve<ILoaderSupervisor>();
                        if (loaderMaster != null)
                        {
                            retVal = loaderMaster.ValidationFoupUnloadedState(foupNumber, ref foupstateStr);
                        }
                    }
                }
                else
                {
                    // Single 장비 인 경우 확인 로직 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        
        #endregion

        #region <remarks> Thread </remarks>
        public void ExcuteRun()
        {
            try
            {
                while (true)
                {
                    foreach (var controller in E84Controllers)
                    {
                        controller.Execute();
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
