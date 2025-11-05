namespace ProberInterfaces.E84
{
    using System;
    using System.Collections.Generic;
    using ProberErrorCode;
    using LogModule;
    using Newtonsoft.Json;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Collections.ObjectModel;
    public interface IE84Parameters
    {
        ObservableCollection<E84ModuleParameter> E84Moduls { get; set; }
        ObservableCollection<E84ErrorParameter> E84Errors { get; set; }
        List<E84PinSignalParameter> E84PinSignal { get; set; }
        E84OPTypeEnum E84OPType { get; set; }
    }
    [Serializable]
    public class E84Parameters : IE84Parameters, ISystemParameterizable, IParamNode, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <remarks> ISystem & IParam Property </remarks>
        [JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }
        [JsonIgnore, ParamIgnore]
        public object Owner { get; set; }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
        [JsonIgnore, ParamIgnore]
        public string FilePath { get; } = "E84Module";
        [JsonIgnore, ParamIgnore]
        public string FileName { get; } = "E84SysParam.json";
        #endregion

        #region <catefory> Property </remarks>
        private ObservableCollection<E84ModuleParameter> _E84Moduls = new ObservableCollection<E84ModuleParameter>();
        public ObservableCollection<E84ModuleParameter> E84Moduls
        {
            get { return _E84Moduls; }
            set
            {
                if (value != _E84Moduls)
                {
                    _E84Moduls = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<E84ErrorParameter> _E84Errors = new ObservableCollection<E84ErrorParameter>();
        public ObservableCollection<E84ErrorParameter> E84Errors
        {
            get { return _E84Errors; }
            set
            {
                if (value != _E84Errors)
                {
                    _E84Errors = value;
                    RaisePropertyChanged();
                }
            }
        }

        private List<E84PinSignalParameter> _E84PinSignal = new List<E84PinSignalParameter>();
        public List<E84PinSignalParameter> E84PinSignal
        {
            get { return _E84PinSignal; }
            set
            {
                if (value != _E84PinSignal)
                {
                    _E84PinSignal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84OPTypeEnum _E84OPType;
        public E84OPTypeEnum E84OPType
        {
            get { return _E84OPType; }
            set
            {
                if (value != _E84OPType)
                {
                    _E84OPType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsCDBypass;
        public bool IsCDBypass
        {
            get { return _IsCDBypass; }
            set
            {
                if (value != _IsCDBypass)
                {
                    _IsCDBypass = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _CDBypassDelayTimeInSec;
        public double CDBypassDelayTimeInSec
        {
            get { return _CDBypassDelayTimeInSec; }
            set
            {
                if (value != _CDBypassDelayTimeInSec)
                {
                    _CDBypassDelayTimeInSec = value;
                    RaisePropertyChanged();
                }
            }
        }

        private E84CassetteLockParam _CassetteLockParam = new E84CassetteLockParam();
        public E84CassetteLockParam CassetteLockParam
        {
            get { return _CassetteLockParam; }
            set
            {
                if (value != _CassetteLockParam)
                {
                    _CassetteLockParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<E84PresenceTypeEnum> _E84PreseceType = new Element<E84PresenceTypeEnum>() { Value = E84PresenceTypeEnum.EXIST };
        public Element<E84PresenceTypeEnum> E84PreseceType
        {
            get { return _E84PreseceType; }
            set
            {
                if (value != _E84PreseceType)
                {
                    _E84PreseceType = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Exist Sensor 가 ON 된 뒤, Presence 가 눌릴때까지의 허용하는 TimeOut.
        /// </summary>
        private Element<long> _TimeoutOnPresenceAfterOnExistSensorMs = new Element<long>() { Value = 3000 };
        public Element<long> TimeoutOnPresenceAfterOnExistSensorMs
        {
            get { return _TimeoutOnPresenceAfterOnExistSensorMs; }
            set
            {
                if (value != _TimeoutOnPresenceAfterOnExistSensorMs)
                {
                    _TimeoutOnPresenceAfterOnExistSensorMs = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CassetteLockParam.E84PortEachLockOptions != null)
                {
                    if (CassetteLockParam.E84PortEachLockOptions.Count == 0)

                    {
                        if (SystemModuleCount.ModuleCnt != null)
                        {
                            for (int index = 1; index <= SystemModuleCount.ModuleCnt.FoupCount; index++)
                            {
                                CassetteLockParam.E84PortEachLockOptions.Add(new E84PortLockOptionParam(index));
                            }
                        }
                    }
                    else
                    {
                        //현재 개수에 추가
                        for (int index = 1 + CassetteLockParam.E84PortEachLockOptions.Count; index <= SystemModuleCount.ModuleCnt.FoupCount; index++)
                        {
                            CassetteLockParam.E84PortEachLockOptions.Add(new E84PortLockOptionParam(index));
                            LoggerManager.Debug($"CassetteLockParam.E84PortEachLockOptions[{index}] Added.");
                        }
                    }
                }
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        public void DefaultSetting()
        {
            try
            {
                MPTParam();
                DefaultErrorParam();
                DefaultPinSignalParam();

                E84OPType = E84OPTypeEnum.SINGLE;
                E84PreseceType.Value = E84PresenceTypeEnum.PRESENCE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MPTParam()
        {
            try
            {
                #region <remarks> MPT </remarks>
                //E84Moduls.Add(new E84ModuleParameter());
                //E84Moduls[0].FoupIndex = 1;
                //E84Moduls[0].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                //E84Moduls[0].E84ConnType = E84ConnTypeEnum.ETHERNET;
                //E84Moduls[0].E84OPModuleType = E84OPModuleTypeEnum.FOUP;
                //E84Moduls[0].E84_Attatched = true;
                //E84Moduls[0].NetID = 2;
                //E84Moduls[0].TP1.Value = 60;
                //E84Moduls[0].TP2.Value = 3;
                //E84Moduls[0].TP3.Value = 60;
                //E84Moduls[0].TP4.Value = 60;
                //E84Moduls[0].TP5.Value = 3;
                //E84Moduls[0].TP6.Value = 10;
                //E84Moduls[0].TD0.Value = 1;
                //E84Moduls[0].TD1.Value = 10;
                //E84Moduls[0].RecoveryTimeout.Value = 150;
                //E84Moduls[0].RetryCount.Value = 3;

                //E84Moduls.Add(new E84ModuleParameter());
                //E84Moduls[1].FoupIndex = 2;
                //E84Moduls[1].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                //E84Moduls[1].E84ConnType = E84ConnTypeEnum.ETHERNET;
                //E84Moduls[1].E84OPModuleType = E84OPModuleTypeEnum.FOUP;
                //E84Moduls[1].E84_Attatched = true;
                //E84Moduls[1].NetID = 3;
                //E84Moduls[1].TP1.Value = 60;
                //E84Moduls[1].TP2.Value = 3;
                //E84Moduls[1].TP3.Value = 60;
                //E84Moduls[1].TP4.Value = 60;
                //E84Moduls[1].TP5.Value = 3;
                //E84Moduls[1].TP6.Value = 10;
                //E84Moduls[1].TD0.Value = 0.1;
                //E84Moduls[1].TD1.Value = 10;
                //E84Moduls[1].RecoveryTimeout.Value = 150;
                //E84Moduls[1].RetryCount.Value = 3;

                //E84Moduls.Add(new E84ModuleParameter());
                //E84Moduls[2].FoupIndex = 1;
                //E84Moduls[2].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                //E84Moduls[2].E84ConnType = E84ConnTypeEnum.ETHERNET;
                //E84Moduls[2].E84OPModuleType = E84OPModuleTypeEnum.CARD;
                //E84Moduls[2].E84_Attatched = true;
                //E84Moduls[2].NetID = 4;
                //E84Moduls[2].TP1.Value = 60;
                //E84Moduls[2].TP2.Value = 3;
                //E84Moduls[2].TP3.Value = 60;
                //E84Moduls[2].TP4.Value = 60;
                //E84Moduls[2].TP5.Value = 3;
                //E84Moduls[2].TP6.Value = 10;
                //E84Moduls[2].TD0.Value = 0.1;
                //E84Moduls[2].TD1.Value = 10;
                //E84Moduls[2].RecoveryTimeout.Value = 150;
                //E84Moduls[2].RetryCount.Value = 3;
                #endregion

                //int netIdInitValue = 10;  // NetID: YMTC는 10부터 쓰고 있음.
                int netIdInitValue = 2;     // NetID: HYNIX는 2부터 쓰고 있음.
                int count = 0;
                if (SystemManager.SysteMode == SystemModeEnum.Multiple && System.AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                {
                    //E84 on Foup
                    for (count = 0; count < SystemModuleCount.ModuleCnt.FoupCount; count++)
                    {
                        E84Moduls.Add(new E84ModuleParameter());
                        E84Moduls[count].FoupIndex = (count + 1);
                        E84Moduls[count].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                        E84Moduls[count].E84ConnType = E84ConnTypeEnum.ETHERNET;
                        E84Moduls[count].E84OPModuleType = E84OPModuleTypeEnum.FOUP;
                        E84Moduls[count].E84_Attatched = true;
                        E84Moduls[count].NetID = (netIdInitValue + count);
                        E84Moduls[count].TP1.Value = 60;
                        E84Moduls[count].TP2.Value = 3;
                        E84Moduls[count].TP3.Value = 60;
                        E84Moduls[count].TP4.Value = 60;
                        E84Moduls[count].TP5.Value = 3;
                        E84Moduls[count].TP6.Value = 10;
                        E84Moduls[count].TD0.Value = 1;
                        E84Moduls[count].TD1.Value = 10;
                        E84Moduls[count].RecoveryTimeout.Value = 150;
                        E84Moduls[count].RetryCount.Value = 3;
                        E84Moduls[count].HeartBeat.Value = 0;
                        E84Moduls[count].InputFilter.Value = 0;
                        E84Moduls[count].AUXIN0.Value = 0;
                        E84Moduls[count].AUXIN1.Value = 0;
                        E84Moduls[count].AUXIN2.Value = 0;
                        E84Moduls[count].AUXIN3.Value = 1;      //LightCurtain 사용.
                        E84Moduls[count].AUXIN4.Value = 0;
                        E84Moduls[count].AUXIN5.Value = 0;
                        E84Moduls[count].AUXOUT0.Value = 0;
                        E84Moduls[count].UseLP1.Value = 0;
                        E84Moduls[count].UseES.Value = 1;       //LightCurtain 사용.
                        E84Moduls[count].UseHOAVBL.Value = 1;   //LightCurtain 사용.
                        E84Moduls[count].ReadyOff.Value = 1;    //**Foup-E84와 Card-E84 둘다 Read_Off, Valid_Off, Valid_On 세개의 옵션을 사용하고 있다. 이 옵션을 사용하지 않으면 간헐적으로 에러가 난다. 
                        E84Moduls[count].UseCS1.Value = 0;
                        E84Moduls[count].ValidOff.Value = 1;    //**Foup-E84와 Card-E84 둘다 Read_Off, Valid_Off, Valid_On 세개의 옵션을 사용하고 있다. 이 옵션을 사용하지 않으면 간헐적으로 에러가 난다. 
                        E84Moduls[count].ValidOn.Value = 1;     //**Foup-E84와 Card-E84 둘다 Read_Off, Valid_Off, Valid_On 세개의 옵션을 사용하고 있다. 이 옵션을 사용하지 않으면 간헐적으로 에러가 난다. 
                        E84Moduls[count].RVAUXIN0.Value = 0;
                        E84Moduls[count].RVAUXIN1.Value = 0;
                        E84Moduls[count].RVAUXIN2.Value = 0;
                        E84Moduls[count].RVAUXIN3.Value = 1;    //LightCurtain 사용.
                        E84Moduls[count].RVAUXIN4.Value = 0;
                        E84Moduls[count].RVAUXIN5.Value = 0;
                        E84Moduls[count].RVAUXOUT0.Value = 0;
                    }
                    int buffercount = 0;
                    //E84 on CardBuffer
                    for (buffercount = 0; buffercount < SystemModuleCount.ModuleCnt.CardBufferCount; buffercount++)
                    {
                        E84Moduls.Add(new E84ModuleParameter());
                        E84Moduls[count].FoupIndex = (buffercount + 1);     //CardBufferModule을 찾을때 FoupIndex를 사용하여 찾음...
                        E84Moduls[count].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                        E84Moduls[count].E84ConnType = E84ConnTypeEnum.ETHERNET;
                        E84Moduls[count].E84OPModuleType = E84OPModuleTypeEnum.CARD;
                        E84Moduls[count].E84_Attatched = true;
                        E84Moduls[count].NetID = netIdInitValue + count;
                        E84Moduls[count].TP1.Value = 60;
                        E84Moduls[count].TP2.Value = 3;
                        E84Moduls[count].TP3.Value = 60;
                        E84Moduls[count].TP4.Value = 60;
                        E84Moduls[count].TP5.Value = 3;
                        E84Moduls[count].TP6.Value = 10;
                        E84Moduls[count].TD0.Value = 1;
                        E84Moduls[count].TD1.Value = 10;
                        E84Moduls[count].RecoveryTimeout.Value = 150;
                        E84Moduls[count].RetryCount.Value = 3;
                        E84Moduls[count].HeartBeat.Value = 0;
                        E84Moduls[count].InputFilter.Value = 0;
                        E84Moduls[count].AUXIN0.Value = 0;
                        E84Moduls[count].AUXIN1.Value = 0;
                        E84Moduls[count].AUXIN2.Value = 0;
                        E84Moduls[count].AUXIN3.Value = 0;      //LightCurtain 미사용.
                        E84Moduls[count].AUXIN4.Value = 0;
                        E84Moduls[count].AUXIN5.Value = 0;
                        E84Moduls[count].AUXOUT0.Value = 0;
                        E84Moduls[count].UseLP1.Value = 0;
                        E84Moduls[count].UseES.Value = 0;       //LightCurtain 미사용.
                        E84Moduls[count].UseHOAVBL.Value = 0;   //LightCurtain 미사용.
                        E84Moduls[count].ReadyOff.Value = 1;    //**Foup-E84와 Card-E84 둘다 Read_Off, Valid_Off, Valid_On 세개의 옵션을 사용하고 있다. 이 옵션을 사용하지 않으면 간헐적으로 에러가 난다.  
                        E84Moduls[count].UseCS1.Value = 0;
                        E84Moduls[count].ValidOff.Value = 1;    //**Foup-E84와 Card-E84 둘다 Read_Off, Valid_Off, Valid_On 세개의 옵션을 사용하고 있다. 이 옵션을 사용하지 않으면 간헐적으로 에러가 난다. 
                        E84Moduls[count].ValidOn.Value = 1;     //**Foup-E84와 Card-E84 둘다 Read_Off, Valid_Off, Valid_On 세개의 옵션을 사용하고 있다. 이 옵션을 사용하지 않으면 간헐적으로 에러가 난다.  
                        E84Moduls[count].RVAUXIN0.Value = 0;
                        E84Moduls[count].RVAUXIN1.Value = 0;
                        E84Moduls[count].RVAUXIN2.Value = 0;
                        E84Moduls[count].RVAUXIN3.Value = 0;    //LightCurtain 미사용.
                        E84Moduls[count].RVAUXIN4.Value = 0;
                        E84Moduls[count].RVAUXIN5.Value = 0;
                        E84Moduls[count].RVAUXOUT0.Value = 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OperaParam()
        {
            try
            {
                #region <remarks> MPT </remarks>
                E84Moduls.Add(new E84ModuleParameter());
                E84Moduls[0].FoupIndex = 1;
                E84Moduls[0].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                E84Moduls[0].E84ConnType = E84ConnTypeEnum.ETHERNET;
                E84Moduls[0].E84OPModuleType = E84OPModuleTypeEnum.FOUP;
                E84Moduls[0].E84_Attatched = true;
                E84Moduls[0].NetID = 10;
                E84Moduls[0].TP1.Value = 60;
                E84Moduls[0].TP2.Value = 3;
                E84Moduls[0].TP3.Value = 60;
                E84Moduls[0].TP4.Value = 60;
                E84Moduls[0].TP5.Value = 3;
                E84Moduls[0].TP6.Value = 10;
                E84Moduls[0].TD0.Value = 1;
                E84Moduls[0].TD1.Value = 10;
                E84Moduls[0].RecoveryTimeout.Value = 150;
                E84Moduls[0].RetryCount.Value = 3;

                E84Moduls.Add(new E84ModuleParameter());
                E84Moduls[1].FoupIndex = 2;
                E84Moduls[1].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                E84Moduls[1].E84ConnType = E84ConnTypeEnum.ETHERNET;
                E84Moduls[1].E84OPModuleType = E84OPModuleTypeEnum.FOUP;
                E84Moduls[1].E84_Attatched = true;
                E84Moduls[1].NetID = 11;
                E84Moduls[1].TP1.Value = 60;
                E84Moduls[1].TP2.Value = 3;
                E84Moduls[1].TP3.Value = 60;
                E84Moduls[1].TP4.Value = 60;
                E84Moduls[1].TP5.Value = 3;
                E84Moduls[1].TP6.Value = 10;
                E84Moduls[1].TD0.Value = 0.1;
                E84Moduls[1].TD1.Value = 10;
                E84Moduls[1].RecoveryTimeout.Value = 150;
                E84Moduls[1].RetryCount.Value = 3;

                E84Moduls.Add(new E84ModuleParameter());
                E84Moduls[2].FoupIndex = 3;
                E84Moduls[2].E84ModuleType = E84CommModuleTypeEnum.EMOTIONTEK;
                E84Moduls[2].E84ConnType = E84ConnTypeEnum.ETHERNET;
                E84Moduls[2].E84OPModuleType = E84OPModuleTypeEnum.FOUP;
                E84Moduls[2].E84_Attatched = true;
                E84Moduls[2].NetID = 12;
                E84Moduls[2].TP1.Value = 60;
                E84Moduls[2].TP2.Value = 3;
                E84Moduls[2].TP3.Value = 60;
                E84Moduls[2].TP4.Value = 60;
                E84Moduls[2].TP5.Value = 3;
                E84Moduls[2].TP6.Value = 10;
                E84Moduls[2].TD0.Value = 0.1;
                E84Moduls[2].TD1.Value = 10;
                E84Moduls[2].RecoveryTimeout.Value = 150;
                E84Moduls[2].RetryCount.Value = 3;
                #endregion
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DefaultErrorParam()
        {
            try
            {
                E84Errors.Add(new E84ErrorParameter(21, "TP1 Timeout Error", "TP1 Timeout Error", E84EventCode.TP1_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(22, "TP2 Timeout Error", "TP2 Timeout Error", E84EventCode.TP2_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(23, "TP3 Timeout Error", "TP3 Timeout Error", E84EventCode.TP3_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(24, "TP4 Timeout Error", "TP4 Timeout Error", E84EventCode.TP4_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(25, "TP5 Timeout Error", "TP5 Timeout Error", E84EventCode.TP5_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(26, "TP6 Timeout Error", "TP6 Timeout Error", E84EventCode.TP6_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));

                E84Errors.Add(new E84ErrorParameter(31, "TD0 Delay Timer Warning", "TD0 Delay Timer Warning", E84EventCode.TD0_DELAY, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(32, "TD1 Delay Timer Warning", "TD1 Delay Timer Warning", E84EventCode.TD1_DELAY, E84ErrorActEnum.ERROR_Ho_Off));

                E84Errors.Add(new E84ErrorParameter(34, "Heartbeat Timeout Error", "Heartbeat Timeout Error", E84EventCode.HEARTBEAT_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(41, "Light Curtain Blocked Error", "Light Curtain Error", E84EventCode.LIGHT_CURTAIN_ERROR, E84ErrorActEnum.ERROR_Emergency));
                E84Errors.Add(new E84ErrorParameter(51, "Cannot determine Transfer Type Error", "In time of ‘L_REQ’ On or ‘UL_REQ’ On , E84 board can not decision in some case, which is Loading Sequence or Unloading Sequence", E84EventCode.DETERMINE_TRANSFET_TYPE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(52, "Unclamp Timeout Error", "", E84EventCode.UNCLAMP_TIMEOUT, E84ErrorActEnum.ERROR_Ho_Off));

                E84Errors.Add(new E84ErrorParameter(70, "“HO_AVBL” On Sequence Error", "When “HO_AVBL” is On,In case of above one signal is On status between “CS_0 / CS_1”“VALID”,”TR_REQ”,”BUSY”,”COMPT”", E84EventCode.HO_AVBL_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(71, "“CS_0/CS_1” On Sequence Error", "When “CS_0/CS_1” is On, In case of above one signal is on status between “VALID”, ”TR_REQ”, ”BUSY”, ”COMPT”", E84EventCode.CS_ON_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(72, "“VALID” OnSequence Error", "When “VALID” is On,In case of “CS_0 / CS_1” is Off, or,In case of above one signal is On status between“VALID”, “TR_REQ”, “BUSY”, “COMPT” ", E84EventCode.VALED_ON_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(73, "“TR_REQ” On Sequence Error", "When “TR_REQ” is On, In case of above one signal is Off status between “CS_0/CS_1” or “VALID”, or, In case of above one signal is On status between “TR_REQ”, “BUSY”, “COMPT”", E84EventCode.TR_REQ_ON_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(74, "Clamp Off Sequence Error", "In case of Clamp On status before “READY” On status", E84EventCode.CLAMP_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(75, "“BUSY” On Sequence Error", "When “BUSY” is On, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, “TR_REQ”, or, In case of “COMPT” On status, or, In case of Clamp On status before “BUSY” On", E84EventCode.BUSY_ON_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(76, "Carrier Status Change Step Sequence Error", "In case of Clamp On status before Carrier status changed", E84EventCode.CARRIER_SUATUS_CHANGE_STEP_SEQUENC_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(77, "“BUSY” Off Sequence Error", "When “BUSY” is Off, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, “TR_REQ”, or, In case of “COMPT” On", E84EventCode.BUSY_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(78, "“TR_REQ” Off Sequence Error", "When “TR_REQ” is Off, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, or, In case of above one signal is On status between “BUSY”, “COMPT” ", E84EventCode.TR_REQ_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(79, "“COMPT” On Sequence Error", "When “COMPT” is On, In case of above one signal is Off status between “CS_0 / CS_1”, “VALID”, or, In case of above one signal is On status between “TR_REQ”, “BUSY” ", E84EventCode.COMPT_ON_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(80, "“VALID” Off Sequence Error", "When “VALID” is Off, In case of above one signal is Off status between “CS_0 / CS1”, “COMPT”, or, In case of above one signal is On status between “TR_REQ”, “BUSY” ", E84EventCode.VALID_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(81, "“COMPT” Off Sequence Error ", "When “COMPT” is Off, In case of “CS_0 / CS_1” is Off status, or, In case of above one signal is On status between “VALID”, “TR_REQ”, “BUSY” ", E84EventCode.COMPT_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(82, "“CS_0/CS_1” Off Sequence Error", "When “CS_0/CS_1” is Off, In case of above one signal is On status between “VALID”, “TR_REQ”, “BUSY”, “COMPT”", E84EventCode.CS_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(83, "“HO_AVBL” Off Sequence Error", "When “CS_0 / CS_1” is On, In case of “HO_AVBL” Off Status", E84EventCode.HO_AVAL_OFF_SEQUENCE_ERROR, E84ErrorActEnum.ERROR_Ho_Off));

                // 아래의 에러코드는 semics 정의 
                E84Errors.Add(new E84ErrorParameter(84, "Sensor Error (Load Sequence)", "Only on PRESENCS sensor during Load sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_LOAD_ONLY_PRESENCS, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(85, "Sensor Error (Load Sequence)", "Only on PLANCEMENT sensor during Load sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_LOAD_ONLY_PLANCEMENT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(86, "Sensor Error (Unload Sequence)", "Still on PRESENCS sensor during Unload sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PRESENCS, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(87, "Sensor Error (Unload Sequence)", "Still on PLANCEMENT sensor during Unload sequence. Recover LP by User from E84 page after OHT left", E84EventCode.SENSOR_ERROR_UNLOAD_STILL_PLANCEMENT, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(88, "Operation Error(Load Sequence)", "Carrier Presenced Alreay", E84EventCode.HAND_SHAKE_ERROR_LOAD_PRESENCE, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(89, "Sensor Error (Unload Sequence)", "Cannot Change Mode. CS, VALID is On", E84EventCode.HAND_SHAKE_ERROR_LOAD_MODE, E84ErrorActEnum.ERROR_Ho_Off));
                E84Errors.Add(new E84ErrorParameter(90, "Sequence Error", "Clamp On Before Busy Off", E84EventCode.CLAMP_ON_BEFORE_OFF_BUSY, E84ErrorActEnum.ERROR_Ho_Off));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DefaultPinSignalParam()
        {
            try
            {
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.L_REQ, 0, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.U_REQ, 1, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.VA, 2, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.READY, 3, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.VS_0, 4, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.VS_1, 5, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.HO_AVBL, 6, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.ES, 7, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.SELECT, 8, E84SignalActiveEnum.OUTPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.MODE, 9, E84SignalActiveEnum.OUTPUT));

                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.VALID, 0, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.CS_0, 1, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.CS_1, 2, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.AM_AVBL, 3, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.TR_REQ, 4, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.BUSY, 5, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.COMPT, 6, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.CONT, 7, E84SignalActiveEnum.INPUT));
                E84PinSignal.Add(new E84PinSignalParameter(E84SignalTypeEnum.GO, 8, E84SignalActiveEnum.INPUT));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                //TODO ReturnValue 다시 할 수 있게 
                DefaultSetting();
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ret;
        }

    }
}
