using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LampModule
{
    using ProberInterfaces;
    using System.Collections.Concurrent;
    using System.Threading;
    using SequenceService;
    using ProberErrorCode;
    using System.ComponentModel;
    using LogModule;
    using ProberInterfaces.Lamp;
    using System.Runtime.CompilerServices;


    //using ProberInterfaces.ThreadSync;

    public class LampManager : SequenceServiceBase, ILampManager, IHasSysParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        public LampSystemParameter LampSystemParam { get; set; }
        private List<IIOBase> _IoDeviceList;

        private List<ModuleMonitorCombination> _MonitorLampComboList;
        private Dictionary<String, IStateModule> _ModuleDic;
        private LampCombination _CurrentMonitorLampCombo;
        private LampCombination _RequestLampCombo;

        public RequestCombination GetCurrentLampCombo()
        {
            RequestCombination retval = null;

            try
            {
                if(_RequestLampCombo == null)
                {
                    _RequestLampCombo = new RequestCombination();
                }

                if (_RedLampOn)
                {
                    _RequestLampCombo.RedLampStatus.Value = LampStatusEnum.On;
                }
                else
                {
                    _RequestLampCombo.RedLampStatus.Value = LampStatusEnum.Off;
                }

                if (_YellowLampOn)
                {
                    _RequestLampCombo.YellowLampStatus.Value = LampStatusEnum.On;
                }
                else
                {
                    _RequestLampCombo.YellowLampStatus.Value = LampStatusEnum.Off;
                }

                if (_BlueLampOn)
                {
                    _RequestLampCombo.BlueLampStatus.Value = LampStatusEnum.On;
                }
                else
                {
                    _RequestLampCombo.BlueLampStatus.Value = LampStatusEnum.Off;
                }
                _RequestLampCombo.BuzzerStatus.Value = LampStatusEnum.Off;

                retval = _RequestLampCombo as RequestCombination;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


            return retval;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StopBlink();

                if (Initialized)
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");
                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                    return retval;
                }

                _IoDeviceList = GetIODeviceList();
                _ModuleDic = GetModuleDic();
                _MonitorLampComboList = GetMonitorLampComboList();

                SetLampCombo(_OffLampCombo);

                retval = EventCodeEnum.NONE;
                Initialized = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        private List<IIOBase> GetIODeviceList()
        {
            List<IIOBase> ioDeviceList = new List<IIOBase>();

            foreach (IIOBase item in this.IOManager().IOServ.IOList)
            {
                if (item is IIOBase)
                {
                    ioDeviceList.Add(item);
                }
            }

            //==> Light Service에 Light Controller들을 초기화 한다.
            if (ioDeviceList.Count == 0)
            {
                LoggerManager.Error($"ioDeviceList.Count == 0 Failed");
            }

            return ioDeviceList;
        }
        private Dictionary<String, IStateModule> GetModuleDic()
        {
            Dictionary<String, IStateModule> moduleDic = new Dictionary<String, IStateModule>();

            moduleDic.Add(this.LotOPModule().GetType().Name, this.LotOPModule());
            moduleDic.Add(this.LoaderOPModule().GetType().Name, this.LoaderOPModule());

            return moduleDic;
        }
        private List<ModuleMonitorCombination> GetMonitorLampComboList()
        {
            List<ModuleMonitorCombination> monitorLampComboList = new List<ModuleMonitorCombination>();
            foreach (ModuleMonitorCombination lampCombo in LampSystemParam.ModuleLampCombination)
            {
                lampCombo.Module = GetModuleOrNull(lampCombo.ID);
                monitorLampComboList.Add(lampCombo);
            }

            monitorLampComboList.Add(new ModuleMonitorCombination(
                                ModuleStateEnum.UNDEFINED,
                                LampStatusEnum.Off,  //==> R
                                LampStatusEnum.Off, //==> Y
                                LampStatusEnum.Off, //==> B
                                LampStatusEnum.Off, //==> Buzzer
                                AlarmPriority.Emergency,
                                id: String.Empty));//==> 조건에 맞는 램프 조합이 없을 경우 Default로 램프를 끈다.

            return monitorLampComboList;
        }
        private IStateModule GetModuleOrNull(String moduleTypeName)
        {
            if (_ModuleDic.ContainsKey(moduleTypeName) == false)
            {
                return null;
            }

            return _ModuleDic[moduleTypeName];
        }

        public override ModuleStateEnum SequenceRun()
        {
            ModuleStateEnum retVal = ModuleStateEnum.RUNNING;
            try
            {
                if (IsRequestLampComboSet())
                {
                    return retVal;
                }

                ModuleMonitorCombination highPriorLampCombo = GetHighPriorLampCombo();

                if (highPriorLampCombo == _CurrentMonitorLampCombo)
                {
                    return ModuleStateEnum.DONE;
                }

                if (SystemManager.SysteMode != SystemModeEnum.Multiple)
                {
                    SetMonitorLampCombo(highPriorLampCombo);

                    if (highPriorLampCombo != null)
                    {
                        LoggerManager.Debug($"[LampManager] ID:{highPriorLampCombo.ID} INFO  R:{highPriorLampCombo.RedLampStatus.Value} Y:{highPriorLampCombo.YellowLampStatus.Value} B:{highPriorLampCombo.BlueLampStatus.Value} Buzzer : {highPriorLampCombo.BuzzerStatus.Value}");
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"Error occurred while SequenceRun ");
            }

            return retVal;
        }

        private ModuleMonitorCombination GetHighPriorLampCombo()
        {
            ModuleMonitorCombination highPriorLampCombo = _MonitorLampComboList.FirstOrDefault();

            highPriorLampCombo = null;

            foreach (ModuleMonitorCombination lampCombo in _MonitorLampComboList)
            {
                if (lampCombo.CheckModuleState() == false)
                {
                    continue;
                }

                if (highPriorLampCombo == null)
                {
                    highPriorLampCombo = lampCombo;
                }
                else
                {
                    if (lampCombo.Priority.Value > highPriorLampCombo.Priority.Value)
                    {
                        highPriorLampCombo = lampCombo;
                    }
                }

            }

            return highPriorLampCombo;
        }

        private void SetLampCombo(LampCombination combo)
        {
            try
            {
                //==> Red Lamp
                SetLampStatue(combo.RedLampStatus.Value, this.IOManager().IO.Outputs.DOREDLAMP);

                //==> Yellow Lamp
                SetLampStatue(combo.YellowLampStatus.Value, this.IOManager().IO.Outputs.DOYELLOWLAMP);

                //==> Blue Lamp
                SetLampStatue(combo.BlueLampStatus.Value, this.IOManager().IO.Outputs.DOGREENLAMP);

                //==> Buzzer
                SetBuzzerStatus(combo.BuzzerStatus.Value, this.IOManager().IO.Outputs.DOBUZZERON);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"Error occurred while SetLampCombo");
            }

        }
        private void SetLampStatue(LampStatusEnum lampStatus, IOPortDescripter<bool> lampIO)
        {
            switch (lampStatus)
            {
                case LampStatusEnum.On:
                    SetLampBlink(lampIO, false);//==> Blink Off
                    SetLamp(lampIO, true);//==> Lamp On
                    break;
                case LampStatusEnum.Off:
                    SetLampBlink(lampIO, false);//==> Blink Off
                    SetLamp(lampIO, false);//==> Lamp Off
                    break;
                case LampStatusEnum.BlinkOn:
                    SetLampBlink(lampIO, true);//==> Blink On
                    break;
            }
        }

        /// <summary>
        /// SetBuzzerStatus Wrapping
        /// </summary>
        /// <param name="lampStatusEnum"></param>
        public void WrappingSetBuzzerStatus(LampStatusEnum lampStatusEnum)
        {
            try
            {
                SetBuzzerStatus(lampStatusEnum, this.IOManager().IO.Outputs.DOBUZZERON);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private void SetBuzzerStatus(LampStatusEnum buzzerLampStatus, IOPortDescripter<bool> buzzerIO)
        {
            switch (buzzerLampStatus)
            {
                case LampStatusEnum.On:
                    SetLamp(buzzerIO, true);//==> Buzzer On
                    break;
                case LampStatusEnum.Off:
                    SetLamp(buzzerIO, false);//==> Buzzer Off
                    break;
            }
        }
        public void SetRequestLampCombo(RequestCombination requestLampCombo)
        {
            _RequestLampCombo = requestLampCombo;
            SetLampCombo(_RequestLampCombo);
        }
        public void SetMonitorLampCombo(ModuleMonitorCombination monitorLampCombo)
        {
            _CurrentMonitorLampCombo = monitorLampCombo;

            if (_CurrentMonitorLampCombo != null)
            {
                SetLampCombo(_CurrentMonitorLampCombo);
            }
            else
            {
                LoggerManager.Error($"[LampManager], SetMonitorLampCombo() : _CurrentMonitorLampCombo is null.");
            }
        }

        #region ==> Lamp On/Off/Update
        //_IoDeviceList[1].WriteBit(4, 7, value);//==>Red
        //_IoDeviceList[1].WriteBit(5, 0, value);//==>Yellow
        //_IoDeviceList[1].WriteBit(5, 1, value);//==>Blue
        //_IoDeviceList[1].WriteBit(5, 2, value);//==>Buzzer

        #region ==> OnLampType : MainTopBar 랑 Binding 되어 있음, MainWindow랑 바인딩 되어 있음.
        private EnumLampType _OnLampType;
        public EnumLampType OnLampType
        {
            get { return _OnLampType; }
            set
            {
                if (value != _OnLampType)
                {
                    _OnLampType = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _RedLampOn;
        private bool _YellowLampOn;
        private bool _BlueLampOn;
        private void SetLamp(IOPortDescripter<bool> portDesc, bool on)
        {
            try
            {
                bool lampOnFlag = portDesc.Reverse.Value ? !on : on;
                //==> Channel Descripter Index
                int channelDescIdx = portDesc.ChannelIndex.Value;

                //==> Channel Descripter Info
                int devIdx = this.IOManager().IOServ.Outputs[channelDescIdx].DevIndex;//==> Device Number
                int hwChannel = this.IOManager().IOServ.Outputs[channelDescIdx].ChannelIndex;//==> Hardware Channel
                int port = portDesc.PortIndex.Value;

                if (_IoDeviceList.Count > 0)
                {
                    if (portDesc.IOOveride.Value == EnumIOOverride.EMUL)
                    {
                        portDesc.Value = on;
                    }
                    else
                    {
                        _IoDeviceList[devIdx].WriteBit(hwChannel, port, lampOnFlag);
                    }
                }
                UpdateLampStatus(portDesc, on);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"Error Occurred while SetLamp");
            }

        }
        private void UpdateLampStatus(IOPortDescripter<bool> portDesc, bool on)
        {
            try
            {
                if (portDesc == this.IOManager().IO.Outputs.DOREDLAMP)
                {
                    _RedLampOn = on;
                }
                else if (portDesc == this.IOManager().IO.Outputs.DOYELLOWLAMP)
                {
                    _YellowLampOn = on;
                }
                else if (portDesc == this.IOManager().IO.Outputs.DOGREENLAMP)
                {
                    _BlueLampOn = on;
                }

                if (_RedLampOn)
                {
                    OnLampType = EnumLampType.Red;
                }
                else if (_YellowLampOn)
                {
                    OnLampType = EnumLampType.Yellow;
                }
                else if (_BlueLampOn)
                {
                    OnLampType = EnumLampType.Blue;
                }
                else
                {
                    OnLampType = EnumLampType.UNDEFINED;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"Error Occurred while UpdateLampStatus");
            }

        }
        #endregion

        #region ==> Lamp Blink
        private bool _BlinkRun;
        private Task _BlinkRunTask;
        //==> value(Object)는 사용하지 않아서 항상 null 값을 넣어둠, Lock 처리가 된 Hash로 사용 중
        private ConcurrentDictionary<IOPortDescripter<bool>, Object> _BlinkPortDescBag = new ConcurrentDictionary<IOPortDescripter<bool>, Object>();
        private void SetLampBlink(IOPortDescripter<bool> portDesc, bool on)
        {
            try
            {
                if (on)
                {
                    if (_BlinkPortDescBag.Keys.Contains(portDesc) == false)
                        _BlinkPortDescBag.TryAdd(portDesc, null);
                    if (_BlinkRun == false)
                        RunBlink();//==> Run Port Blink
                }
                else
                {
                    Object removePortDesc;
                    if (_BlinkPortDescBag.Keys.Contains(portDesc))
                        _BlinkPortDescBag.TryRemove(portDesc, out removePortDesc);
                    if (_BlinkPortDescBag.Count == 0)
                        StopBlink();//==> Stop Port Blink
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                LoggerManager.Debug($"Error Occurred while SetLampBlink");
            }

        }
        private void RunBlink()
        {
            if (_BlinkRun)
                return;//==> 이미 실행중이어서 더 실행 시킬 필요 없음.

            StopBlink();//==> 안전 장치

            _BlinkRun = true;

            _BlinkRunTask = Task.Run(() =>
            {
                const int blinkInterval = 500;
                bool value = false;

                do
                {
                    foreach (IOPortDescripter<bool> portDesc in _BlinkPortDescBag.Keys)
                        SetLamp(portDesc, value);
                    Thread.Sleep(blinkInterval);
                    value = !value;
                } while (_BlinkRun);
            });
        }
        private void StopBlink()
        {
            _BlinkRun = false;
            if (_BlinkRunTask != null)
                _BlinkRunTask.Wait();
        }
        #endregion

        #region ==> Request Lamp Combo
        private RequestCombination _SirenLampCombo = new RequestCombination(LampStatusEnum.On, LampStatusEnum.Off, LampStatusEnum.Off, LampStatusEnum.On);
        private RequestCombination _RedLampCombo = new RequestCombination(LampStatusEnum.On, LampStatusEnum.Off, LampStatusEnum.Off, LampStatusEnum.Off);
        private RequestCombination _WarningLampCombo = new RequestCombination(LampStatusEnum.Off, LampStatusEnum.On, LampStatusEnum.Off, LampStatusEnum.Off);
        private RequestCombination _OffLampCombo = new RequestCombination(LampStatusEnum.Off, LampStatusEnum.Off, LampStatusEnum.Off, LampStatusEnum.Off, AlarmPriority.Info);
        public void RequestSirenLamp()
        {
            SetRequestLampCombo(_SirenLampCombo);
        }
        public void RequestRedLamp()
        {
            SetRequestLampCombo(_RedLampCombo);
        }
        public void RequestWarningLamp()
        {
            SetRequestLampCombo(_WarningLampCombo);
        }
        public void ClearRequestLamp()
        {
            _RequestLampCombo = null;
        }
        private bool IsRequestLampComboSet()
        {
            return _RequestLampCombo != null;
        }
        #endregion

        #region ==> Load & Save System Parameter
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            IParam tmpParam = null;
            tmpParam = new LampSystemParameter();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";

            RetVal = this.LoadParameter(ref tmpParam, typeof(LampSystemParameter));

            if (RetVal == EventCodeEnum.NONE)
            {
                LampSystemParam = tmpParam as LampSystemParameter;
            }
            else
            {
                LoggerManager.Error(String.Format("[LampManager] Load System Param: Serialize Error"));
            }

            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            RetVal = this.SaveParameter(LampSystemParam);

            if (RetVal == EventCodeEnum.PARAM_ERROR)
            {
                LoggerManager.Error(String.Format("[LampManager] Save System Param: Serialize Error"));
                return RetVal;
            }
            return RetVal;
        }
        #endregion

        public void DeInitModule()
        {
            LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
        }
    }
}
