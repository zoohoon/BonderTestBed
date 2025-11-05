using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using System.ServiceModel;
using System.Reflection;
using System.Timers;

namespace IOMappingsObject
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [Serializable]
    public class IOMappings : INotifyPropertyChanged, IIOMappingsParameter, ISystemParameterizable, IParamNode
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
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

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public string FilePath { get; } = "IO";

        public string FileName { get; } = "IOMapping.Json";

        public IOMappings()
        {

        }

        #region // Device indices
        //private List<IODevAddress> _InputDevAddresses = new List<IODevAddress>();

        // public List<IODevAddress> InputDevAddresses
        //{
        //    get { return _InputDevAddresses; }
        //    set { _InputDevAddresses = value; }
        //}
        //private List<IODevAddress> _OutputDevAddresses = new List<IODevAddress>();

        //public List<IODevAddress> OutputDevAddresses
        //{
        //    get { return _OutputDevAddresses; }
        //    set { _OutputDevAddresses = value; }
        //}
        #endregion

        private InputPortDefinitions _Inputs = new InputPortDefinitions();

        public InputPortDefinitions Inputs
        {
            get { return _Inputs; }
            set
            {
                if (value != this._Inputs)
                {
                    _Inputs = value;
                    NotifyPropertyChanged("Inputs");
                }
            }
        }
        private OutputPortDefinitions _Outputs = new OutputPortDefinitions();

        public OutputPortDefinitions Outputs
        {
            get { return _Outputs; }
            set
            {
                if (value != this._Outputs)
                {
                    _Outputs = value;
                    NotifyPropertyChanged("Outputs");
                }
            }
        }
        private RemoteInputPortDefinitions _RemoteInputs = new RemoteInputPortDefinitions();

        public RemoteInputPortDefinitions RemoteInputs
        {
            get { return _RemoteInputs; }
            set
            {
                if (value != this._RemoteInputs)
                {
                    _RemoteInputs = value;
                    NotifyPropertyChanged("RemoteInputs");
                }
            }
        }
        private RemoteOutputPortDefinitions _RemoteOutputs = new RemoteOutputPortDefinitions();

        public RemoteOutputPortDefinitions RemoteOutputs
        {
            get { return _RemoteOutputs; }
            set
            {
                if (value != this._RemoteOutputs)
                {
                    _RemoteOutputs = value;
                    NotifyPropertyChanged("RemoteOutputs");
                }
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                //SetDefaultParam_GP_OPRT();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From IOMappings. {err.Message}");
            }
            return RetVal;
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                //SetDefaultParam_OPUSVLoader();
                //SetDefaultParam_BSCI1();
                SetDefaultParam_OPUSV_Test();
                //SetDefaultParam_GP();
                //SetDefaultParam_GP_OPRT();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        private void SetDefaultParam_GP()
        {
            Inputs.DIWAFERCAMMIDDLE.ChannelIndex.Value = 1;
            Inputs.DIWAFERCAMMIDDLE.PortIndex.Value = 8;
            Inputs.DIWAFERCAMMIDDLE.Reverse.Value = false;
            Inputs.DIWAFERCAMMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIWAFERCAMMIDDLE.MaintainTime.Value = 50;
            Inputs.DIWAFERCAMMIDDLE.TimeOut.Value = 10000;

            Inputs.DIWAFERCAMREAR.ChannelIndex.Value = 1;
            Inputs.DIWAFERCAMREAR.PortIndex.Value = 9;
            Inputs.DIWAFERCAMREAR.Reverse.Value = false;
            Inputs.DIWAFERCAMREAR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIWAFERCAMREAR.MaintainTime.Value = 50;
            Inputs.DIWAFERCAMREAR.TimeOut.Value = 10000;

            Inputs.DICARDCHANGE_IN.ChannelIndex.Value = 2;
            Inputs.DICARDCHANGE_IN.PortIndex.Value = 1;
            Inputs.DICARDCHANGE_IN.Reverse.Value = false;
            Inputs.DICARDCHANGE_IN.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DICARDCHANGE_IN.MaintainTime.Value = 50;
            Inputs.DICARDCHANGE_IN.TimeOut.Value = 10000;

            Inputs.DICARDCHANGE_OUT.ChannelIndex.Value = 2;
            Inputs.DICARDCHANGE_OUT.PortIndex.Value = 2;
            Inputs.DICARDCHANGE_OUT.Reverse.Value = false;
            Inputs.DICARDCHANGE_OUT.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DICARDCHANGE_OUT.MaintainTime.Value = 50;
            Inputs.DICARDCHANGE_OUT.TimeOut.Value = 10000;

            Inputs.DICARDCHANGE_IDLE.ChannelIndex.Value = 2;
            Inputs.DICARDCHANGE_IDLE.PortIndex.Value = 0;
            Inputs.DICARDCHANGE_IDLE.Reverse.Value = false;
            Inputs.DICARDCHANGE_IDLE.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DICARDCHANGE_IDLE.MaintainTime.Value = 50;
            Inputs.DICARDCHANGE_IDLE.TimeOut.Value = 10000;

            int ffuCount = 2;
            Inputs.DI_FFU_ONLINES = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < ffuCount; i++)
            {
                Inputs.DI_FFU_ONLINES.Add(new IOPortDescripter<bool>(nameof(Inputs.DI_FFU_ONLINES), EnumIOType.INPUT, EnumIOOverride.NONE));
            }
            Inputs.DI_FFU_ONLINES[0].ChannelIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[0].PortIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[0].Reverse.Value = false;
            Inputs.DI_FFU_ONLINES[0].IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DI_FFU_ONLINES[0].MaintainTime.Value = 500;
            Inputs.DI_FFU_ONLINES[0].TimeOut.Value = 10000;
            Inputs.DI_FFU_ONLINES[0].Description.Value = "FFU_FRONT";

            Inputs.DI_FFU_ONLINES[1].ChannelIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[1].PortIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[1].Reverse.Value = false;
            Inputs.DI_FFU_ONLINES[1].IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DI_FFU_ONLINES[1].MaintainTime.Value = 500;
            Inputs.DI_FFU_ONLINES[1].TimeOut.Value = 10000;
            Inputs.DI_FFU_ONLINES[1].Description.Value = "FFU_BACK";


            Outputs.DOWAFERMIDDLE.ChannelIndex.Value = 1;
            Outputs.DOWAFERMIDDLE.PortIndex.Value = 10;
            Outputs.DOWAFERMIDDLE.Reverse.Value = false;
            Outputs.DOWAFERMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOWAFERMIDDLE.MaintainTime.Value = 50;
            Outputs.DOWAFERMIDDLE.TimeOut.Value = 10000;


            Outputs.DOWAFERREAR.ChannelIndex.Value = 1;
            Outputs.DOWAFERREAR.PortIndex.Value = 11;
            Outputs.DOWAFERREAR.Reverse.Value = false;
            Outputs.DOWAFERREAR.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOWAFERREAR.MaintainTime.Value = 50;
            Outputs.DOWAFERREAR.TimeOut.Value = 10000;

            #region // Remote loader
            int foupCount = SystemModuleCount.ModuleCnt.FoupCount;
            int cardTrayCnt = 1;
            int fixTrayCount = 9;
            int btCount = 5;
            int cardBuff = 22;
            int pressureSensorCount = 4;

            RemoteInputs.DIARM1VAC = new IOPortDescripter<bool>(0, 0, "DIARM1VAC", EnumIOType.INPUT);
            RemoteInputs.DIARM2VAC = new IOPortDescripter<bool>(0, 1, "DIARM2VAC", EnumIOType.INPUT);
            RemoteInputs.DICCARMVAC = new IOPortDescripter<bool>(0, 2, "DICCARMVAC", EnumIOType.INPUT);
            RemoteInputs.DIEMGState = new IOPortDescripter<bool>(4, 0, "DIEMGState", EnumIOType.INPUT);
            RemoteInputs.DILeftDoorOpen = new IOPortDescripter<bool>(4, 3, "DILeftDoorOpen", EnumIOType.INPUT);
            RemoteInputs.DILeftDoorClose = new IOPortDescripter<bool>(4, 4, "DILeftDoorClose", EnumIOType.INPUT);
            RemoteInputs.DIRightDoorClose = new IOPortDescripter<bool>(4, 5, "DIRightDoorClose", EnumIOType.INPUT);
            RemoteInputs.DIRightDoorOpen = new IOPortDescripter<bool>(4, 6, "DIRightDoorOpen", EnumIOType.INPUT);
            RemoteInputs.DILD_PCW_LEAK_STATE = new IOPortDescripter<bool>(4, 0, "DILD_PCW_LEAK_STATE", EnumIOType.INPUT);

            RemoteInputs.DIBUFVACS = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < btCount; i++)
            {
                RemoteInputs.DIBUFVACS.Add(new IOPortDescripter<bool>(0, 5 + i, $"DIBUFVACS", EnumIOType.INPUT));
            }

            RemoteInputs.DIFixTrays = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI6inchWaferOnFixTs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI8inchWaferOnFixTs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < fixTrayCount; i++)
            {
                RemoteInputs.DIFixTrays.Add(new IOPortDescripter<bool>(1, 0 + i, $"DIFixTray{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI6inchWaferOnFixTs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI6inchWaferOnFixT{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI8inchWaferOnFixTs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI8inchWaferOnFixT{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICardBuffs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < cardBuff; i++)
            {
                RemoteInputs.DICardBuffs.Add(new IOPortDescripter<bool>(2, 0 + i, $"DICardBuff{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DIMainAirs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < pressureSensorCount; i++)
            {
                RemoteInputs.DIMainAirs.Add(new IOPortDescripter<bool>(4, 7 + i, $"DIMainAir{i + 1}", EnumIOType.INPUT));
            }



            RemoteInputs.DIMovedInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIWaferOnInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI6inchWaferOnInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI8inchWaferOnInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIOpenInSPs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DIMovedInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DIMovedInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DIWaferOnInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DIWaferOnInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI6inchWaferOnInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI6inchWaferOnInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI8inchWaferOnInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI8inchWaferOnInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DIOpenInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DIOpenInSP{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICardOnCarrierVacs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < cardTrayCnt; i++)
            {
                RemoteInputs.DICardOnCarrierVacs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICardOnCarrierVacs{i + 1}", EnumIOType.INPUT));
            }
            #region // Foup
            RemoteInputs.DICSTLocks = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTLocks.Add(new IOPortDescripter<bool>(6, 7, "DICSTLock1", EnumIOType.INPUT));
            RemoteInputs.DICSTLocks.Add(new IOPortDescripter<bool>(8, 7, "DICSTLock1", EnumIOType.INPUT));
            RemoteInputs.DICSTLocks.Add(new IOPortDescripter<bool>(10, 7, "DICSTLock1", EnumIOType.INPUT));
            RemoteInputs.DICSTUnlocks = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTUnlocks.Add(new IOPortDescripter<bool>(6, 8, "DICSTUnlock1", EnumIOType.INPUT));
            RemoteInputs.DICSTUnlocks.Add(new IOPortDescripter<bool>(8, 8, "DICSTUnlock2", EnumIOType.INPUT));
            RemoteInputs.DICSTUnlocks.Add(new IOPortDescripter<bool>(10, 8, "DICSTUnlock3", EnumIOType.INPUT));

            RemoteInputs.DICSTLoads = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTLoads.Add(new IOPortDescripter<bool>(6, 12, "DICSTLoad1", EnumIOType.INPUT));
            RemoteInputs.DICSTLoads.Add(new IOPortDescripter<bool>(8, 12, "DICSTLoad2", EnumIOType.INPUT));
            RemoteInputs.DICSTLoads.Add(new IOPortDescripter<bool>(10, 12, "DICSTLoad3", EnumIOType.INPUT));
            RemoteInputs.DICSTUnLoads = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTUnLoads.Add(new IOPortDescripter<bool>(6, 13, "DICSTUnLoad1", EnumIOType.INPUT));
            RemoteInputs.DICSTUnLoads.Add(new IOPortDescripter<bool>(8, 13, "DICSTUnLoad2", EnumIOType.INPUT));
            RemoteInputs.DICSTUnLoads.Add(new IOPortDescripter<bool>(10, 13, "DICSTUnLoad3", EnumIOType.INPUT));

            RemoteInputs.DICSTCoverCloses = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTCoverCloses.Add(new IOPortDescripter<bool>(5, 1, "DICSTCoverClose1", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverCloses.Add(new IOPortDescripter<bool>(7, 1, "DICSTCoverClose2", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverCloses.Add(new IOPortDescripter<bool>(9, 1, "DICSTCoverClose3", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverOpens = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTCoverOpens.Add(new IOPortDescripter<bool>(5, 0, "DICSTCoverOpen1", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverOpens.Add(new IOPortDescripter<bool>(7, 0, "DICSTCoverOpen2", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverOpens.Add(new IOPortDescripter<bool>(9, 0, "DICSTCoverOpen3", EnumIOType.INPUT));

            RemoteInputs.DIMappings = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIMappings.Add(new IOPortDescripter<bool>(2, 5, "DIMapping1", EnumIOType.INPUT));
            RemoteInputs.DIMappings.Add(new IOPortDescripter<bool>(3, 5, "DIMapping2", EnumIOType.INPUT));
            RemoteInputs.DIMappings.Add(new IOPortDescripter<bool>(4, 5, "DIMapping3", EnumIOType.INPUT));

            RemoteInputs.DIWaferOut = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIWaferOut.Add(new IOPortDescripter<bool>(5, 5, "DIWaferOut1", EnumIOType.INPUT));
            RemoteInputs.DIWaferOut.Add(new IOPortDescripter<bool>(7, 5, "DIWaferOut2", EnumIOType.INPUT));
            RemoteInputs.DIWaferOut.Add(new IOPortDescripter<bool>(9, 5, "DIWaferOut3", EnumIOType.INPUT));

            RemoteInputs.DICoverVac = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICoverVac.Add(new IOPortDescripter<bool>(5, 10, "DICoverVac1", EnumIOType.INPUT));
            RemoteInputs.DICoverVac.Add(new IOPortDescripter<bool>(7, 10, "DICoverVac2", EnumIOType.INPUT));
            RemoteInputs.DICoverVac.Add(new IOPortDescripter<bool>(9, 10, "DICoverVac3", EnumIOType.INPUT));

            RemoteOutputs.DOCSTCoverOpens = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverOpens.Add(new IOPortDescripter<bool>(2, 0, "DOCSTCoverOpen1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverOpens.Add(new IOPortDescripter<bool>(3, 0, "DOCSTCoverOpen2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverOpens.Add(new IOPortDescripter<bool>(4, 0, "DOCSTCoverOpen3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTCoverCloses = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverCloses.Add(new IOPortDescripter<bool>(2, 1, "DOCSTCoverClose1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverCloses.Add(new IOPortDescripter<bool>(3, 1, "DOCSTCoverClose2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverCloses.Add(new IOPortDescripter<bool>(4, 1, "DOCSTCoverClose3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTCoverLocks = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverLocks.Add(new IOPortDescripter<bool>(2, 2, "DOCSTCoverLock1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverLocks.Add(new IOPortDescripter<bool>(3, 2, "DOCSTCoverLock2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverLocks.Add(new IOPortDescripter<bool>(4, 2, "DOCSTCoverLock3", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverUnlock = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverUnlock.Add(new IOPortDescripter<bool>(2, 3, "DOCSTCoverUnloc1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverUnlock.Add(new IOPortDescripter<bool>(3, 3, "DOCSTCoverUnloc2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverUnlock.Add(new IOPortDescripter<bool>(4, 3, "DOCSTCoverUnloc3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTLoadLamps = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTLoadLamps.Add(new IOPortDescripter<bool>(2, 12, "DOCSTLoadLamp1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTLoadLamps.Add(new IOPortDescripter<bool>(3, 12, "DOCSTLoadLamp2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTLoadLamps.Add(new IOPortDescripter<bool>(4, 12, "DOCSTLoadLamp3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTUnloadLamps = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTUnloadLamps.Add(new IOPortDescripter<bool>(2, 13, "DOCSTUnloadLamp1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTUnloadLamps.Add(new IOPortDescripter<bool>(3, 13, "DOCSTUnloadLamp2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTUnloadLamps.Add(new IOPortDescripter<bool>(4, 13, "DOCSTUnloadLamp3", EnumIOType.OUTPUT));


            RemoteInputs.DICST12Locks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST12Locks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST12Lock{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICST12UnLocks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST12UnLocks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST12UnLock{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICST8Locks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST8Locks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST8Lock{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICST8Unlocks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST8Unlocks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST8Unlock{i + 1}", EnumIOType.INPUT));
            }

            #endregion

            RemoteOutputs.DOARM1Vac = new IOPortDescripter<bool>(0, 0, "DOARM1Vac", EnumIOType.OUTPUT);
            RemoteOutputs.DOARMVac2 = new IOPortDescripter<bool>(0, 0, "DOARMVac2", EnumIOType.OUTPUT);
            RemoteOutputs.DOCCArmVac = new IOPortDescripter<bool>(0, 0, "DOCCArmVac", EnumIOType.OUTPUT);
            RemoteOutputs.DOCCArmVac_Break = new IOPortDescripter<bool>(0, 0, "DOCCArmVac_Break", EnumIOType.OUTPUT);
            RemoteOutputs.DOBuffVacs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < btCount; i++)
            {
                RemoteOutputs.DOBuffVacs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DOBuffVacs.{i + 1}", EnumIOType.OUTPUT));
            }

            RemoteOutputs.DOFrontSigRed = new IOPortDescripter<bool>(0, 0, "DOFrontSigRed", EnumIOType.OUTPUT);
            RemoteOutputs.DOFrontSigYl = new IOPortDescripter<bool>(0, 0, "DOFrontSigYl", EnumIOType.OUTPUT);
            RemoteOutputs.DOFrontSigGrn = new IOPortDescripter<bool>(0, 0, "DOFrontSigGrn", EnumIOType.OUTPUT);
            RemoteOutputs.DOFrontSigBuz = new IOPortDescripter<bool>(0, 0, "DOFrontSigBuz", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigRed = new IOPortDescripter<bool>(0, 0, "DORearSigRed", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigYl = new IOPortDescripter<bool>(0, 0, "DORearSigYl", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigGrn = new IOPortDescripter<bool>(0, 0, "DORearSigGrn", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigBuz = new IOPortDescripter<bool>(0, 0, "DORearSigBuz", EnumIOType.OUTPUT);

            RemoteInputs.DICardExistSensorInBuffer = new IOPortDescripter<bool>(4, 16, "DICardExistSensorInBuffer", EnumIOType.INPUT); 
  
            #endregion

            #region ==> GROUP Prober CC

            //==> INPUT
            Inputs.DIUPMODULE_LEFT_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_LEFT_SENSOR.PortIndex.Value = 4;
            Inputs.DIUPMODULE_LEFT_SENSOR.Reverse.Value = false;
            Inputs.DIUPMODULE_LEFT_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_LEFT_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_LEFT_SENSOR.TimeOut.Value = 0;

            Inputs.DIUPMODULE_RIGHT_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_RIGHT_SENSOR.PortIndex.Value = 6;
            Inputs.DIUPMODULE_RIGHT_SENSOR.Reverse.Value = false;
            Inputs.DIUPMODULE_RIGHT_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_RIGHT_SENSOR.TimeOut.Value = 0;

            Inputs.DIUPMODULE_CARDEXIST_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.PortIndex.Value = 5;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.Reverse.Value = false;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value = 0;

            Inputs.DIUPMODULE_VACU_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_VACU_SENSOR.PortIndex.Value = 2;
            Inputs.DIUPMODULE_VACU_SENSOR.Reverse.Value = true;
            Inputs.DIUPMODULE_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_VACU_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_VACU_SENSOR.TimeOut.Value = 0;

            Inputs.DITESTER_DOCKING_SENSOR.ChannelIndex.Value = 1;
            Inputs.DITESTER_DOCKING_SENSOR.PortIndex.Value = 0;
            Inputs.DITESTER_DOCKING_SENSOR.Reverse.Value = true;
            Inputs.DITESTER_DOCKING_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITESTER_DOCKING_SENSOR.MaintainTime.Value = 0;
            Inputs.DITESTER_DOCKING_SENSOR.TimeOut.Value = 0;

            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.ChannelIndex.Value = 1;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.PortIndex.Value = 1;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.Reverse.Value = false;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.MaintainTime.Value = 0;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.TimeOut.Value = 0;

            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.ChannelIndex.Value = 1;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.PortIndex.Value = 2;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.Reverse.Value = false;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.MaintainTime.Value = 0;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.TimeOut.Value = 0;

            Inputs.DIPOGOCARD_VACU_SENSOR.ChannelIndex.Value = 1;
            Inputs.DIPOGOCARD_VACU_SENSOR.PortIndex.Value = 3;
            Inputs.DIPOGOCARD_VACU_SENSOR.Reverse.Value = false;
            Inputs.DIPOGOCARD_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIPOGOCARD_VACU_SENSOR.MaintainTime.Value = 0;
            Inputs.DIPOGOCARD_VACU_SENSOR.TimeOut.Value = 0;

            Inputs.DIPOGOTESTER_VACU_SENSOR.ChannelIndex.Value = 1;
            Inputs.DIPOGOTESTER_VACU_SENSOR.PortIndex.Value = 4;
            Inputs.DIPOGOTESTER_VACU_SENSOR.Reverse.Value = false;
            Inputs.DIPOGOTESTER_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIPOGOTESTER_VACU_SENSOR.MaintainTime.Value = 0;
            Inputs.DIPOGOTESTER_VACU_SENSOR.TimeOut.Value = 0;

            Inputs.DITPLATEIN_SENSOR.ChannelIndex.Value = 1;
            Inputs.DITPLATEIN_SENSOR.PortIndex.Value = 5;
            Inputs.DITPLATEIN_SENSOR.Reverse.Value = true;
            Inputs.DITPLATEIN_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITPLATEIN_SENSOR.MaintainTime.Value = 0;
            Inputs.DITPLATEIN_SENSOR.TimeOut.Value = 0;

            //==> OUTPUT
            Outputs.DOUPMODULE_DOWN.ChannelIndex.Value = 1;
            Outputs.DOUPMODULE_DOWN.PortIndex.Value = 11;
            Outputs.DOUPMODULE_DOWN.Reverse.Value = false;
            Outputs.DOUPMODULE_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOUPMODULE_DOWN.MaintainTime.Value = 0;
            Outputs.DOUPMODULE_DOWN.TimeOut.Value = 0;

            Outputs.DOUPMODULE_UP.ChannelIndex.Value = 1;
            Outputs.DOUPMODULE_UP.PortIndex.Value = 10;
            Outputs.DOUPMODULE_UP.Reverse.Value = false;
            Outputs.DOUPMODULE_UP.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOUPMODULE_UP.MaintainTime.Value = 0;
            Outputs.DOUPMODULE_UP.TimeOut.Value = 0;

            Outputs.DOUPMODULE_VACU.ChannelIndex.Value = 1;
            Outputs.DOUPMODULE_VACU.PortIndex.Value = 5;
            Outputs.DOUPMODULE_VACU.Reverse.Value = false;
            Outputs.DOUPMODULE_VACU.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOUPMODULE_VACU.MaintainTime.Value = 0;
            Outputs.DOUPMODULE_VACU.TimeOut.Value = 0;

            Outputs.DOTPLATE_PCLATCH_LOCK.ChannelIndex.Value = 2;
            Outputs.DOTPLATE_PCLATCH_LOCK.PortIndex.Value = 0;
            Outputs.DOTPLATE_PCLATCH_LOCK.Reverse.Value = false;
            Outputs.DOTPLATE_PCLATCH_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOTPLATE_PCLATCH_LOCK.MaintainTime.Value = 0;
            Outputs.DOTPLATE_PCLATCH_LOCK.TimeOut.Value = 0;

            Outputs.DOPOGOCARD_VACU_RELEASE.ChannelIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU_RELEASE.PortIndex.Value = 1;
            Outputs.DOPOGOCARD_VACU_RELEASE.Reverse.Value = false;
            Outputs.DOPOGOCARD_VACU_RELEASE.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOCARD_VACU_RELEASE.MaintainTime.Value = 0;
            Outputs.DOPOGOCARD_VACU_RELEASE.TimeOut.Value = 0;

            Outputs.DOPOGOCARD_VACU.ChannelIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU.PortIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU.Reverse.Value = false;
            Outputs.DOPOGOCARD_VACU.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOCARD_VACU.MaintainTime.Value = 0;
            Outputs.DOPOGOCARD_VACU.TimeOut.Value = 0;


            Outputs.DOPOGOCARD_VACU_RELEASE_SUB.ChannelIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU_RELEASE_SUB.PortIndex.Value = 3;
            Outputs.DOPOGOCARD_VACU_RELEASE_SUB.Reverse.Value = false;
            Outputs.DOPOGOCARD_VACU_RELEASE_SUB.IOOveride.Value = EnumIOOverride.EMUL;
            Outputs.DOPOGOCARD_VACU_RELEASE_SUB.MaintainTime.Value = 0;
            Outputs.DOPOGOCARD_VACU_RELEASE_SUB.TimeOut.Value = 0;

            Outputs.DOPOGOCARD_VACU_SUB.ChannelIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU_SUB.PortIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU_SUB.Reverse.Value = false;
            Outputs.DOPOGOCARD_VACU_SUB.IOOveride.Value = EnumIOOverride.EMUL;
            Outputs.DOPOGOCARD_VACU_SUB.MaintainTime.Value = 0;
            Outputs.DOPOGOCARD_VACU_SUB.TimeOut.Value = 0;

            Outputs.DOPOGOTESTER_VACU_RELEASE.ChannelIndex.Value = 2;
            Outputs.DOPOGOTESTER_VACU_RELEASE.PortIndex.Value = 3;
            Outputs.DOPOGOTESTER_VACU_RELEASE.Reverse.Value = false;
            Outputs.DOPOGOTESTER_VACU_RELEASE.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOTESTER_VACU_RELEASE.MaintainTime.Value = 0;
            Outputs.DOPOGOTESTER_VACU_RELEASE.TimeOut.Value = 0;

            Outputs.DOPOGOTESTER_VACU.ChannelIndex.Value = 2;
            Outputs.DOPOGOTESTER_VACU.PortIndex.Value = 4;
            Outputs.DOPOGOTESTER_VACU.Reverse.Value = false;
            Outputs.DOPOGOTESTER_VACU.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOTESTER_VACU.MaintainTime.Value = 0;
            Outputs.DOPOGOTESTER_VACU.TimeOut.Value = 0;
            #endregion

            #region ==> Bernoulli
            Inputs.DIBERNOULLI_HANDLER_UP.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_HANDLER_UP.PortIndex.Value = 0;
            Inputs.DIBERNOULLI_HANDLER_UP.Reverse.Value = true;
            Inputs.DIBERNOULLI_HANDLER_UP.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_HANDLER_UP.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_HANDLER_UP.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_HANDLER_DOWN.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_HANDLER_DOWN.PortIndex.Value = 1;
            Inputs.DIBERNOULLI_HANDLER_DOWN.Reverse.Value = true;
            Inputs.DIBERNOULLI_HANDLER_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_HANDLER_DOWN.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_HANDLER_DOWN.TimeOut.Value = 10000;

            Inputs.DIBERNOULLIWAFER_EXIST.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLIWAFER_EXIST.PortIndex.Value = 2;
            Inputs.DIBERNOULLIWAFER_EXIST.Reverse.Value = true;
            Inputs.DIBERNOULLIWAFER_EXIST.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLIWAFER_EXIST.MaintainTime.Value = 0;
            Inputs.DIBERNOULLIWAFER_EXIST.TimeOut.Value = 1000;

            Inputs.DIBERNOULLI_ALIGN1_ON.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN1_ON.PortIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN1_ON.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN1_ON.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN1_ON.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN1_ON.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN1_OFF.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN1_OFF.PortIndex.Value = 4;
            Inputs.DIBERNOULLI_ALIGN1_OFF.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN1_OFF.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN1_OFF.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN1_OFF.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN2_ON.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN2_ON.PortIndex.Value = 5;
            Inputs.DIBERNOULLI_ALIGN2_ON.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN2_ON.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN2_ON.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN2_ON.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN2_OFF.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN2_OFF.PortIndex.Value = 6;
            Inputs.DIBERNOULLI_ALIGN2_OFF.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN2_OFF.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN2_OFF.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN2_OFF.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN3_ON.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN3_ON.PortIndex.Value = 7;
            Inputs.DIBERNOULLI_ALIGN3_ON.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN3_ON.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN3_ON.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN3_ON.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN3_OFF.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN3_OFF.PortIndex.Value = 8;
            Inputs.DIBERNOULLI_ALIGN3_OFF.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN3_OFF.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN3_OFF.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN3_OFF.TimeOut.Value = 10000;


            Outputs.DOBERNOULLI_HANDLER_UP.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_HANDLER_UP.PortIndex.Value = 0;
            Outputs.DOBERNOULLI_HANDLER_UP.Reverse.Value = false;
            Outputs.DOBERNOULLI_HANDLER_UP.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_HANDLER_UP.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_HANDLER_UP.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_HANDLER_DOWN.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_HANDLER_DOWN.PortIndex.Value = 1;
            Outputs.DOBERNOULLI_HANDLER_DOWN.Reverse.Value = false;
            Outputs.DOBERNOULLI_HANDLER_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_HANDLER_DOWN.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_HANDLER_DOWN.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_6INCH.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_6INCH.PortIndex.Value = 2;
            Outputs.DOBERNOULLI_6INCH.Reverse.Value = false;
            Outputs.DOBERNOULLI_6INCH.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_6INCH.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_6INCH.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_8INCH.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_8INCH.PortIndex.Value = 3;
            Outputs.DOBERNOULLI_8INCH.Reverse.Value = false;
            Outputs.DOBERNOULLI_8INCH.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_8INCH.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_8INCH.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_12INCH.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_12INCH.PortIndex.Value = 6;
            Outputs.DOBERNOULLI_12INCH.Reverse.Value = false;
            Outputs.DOBERNOULLI_12INCH.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_12INCH.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_12INCH.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ALIGN_EXTEND.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.PortIndex.Value = 4;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.Reverse.Value = false;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.TimeOut.Value = 0;

            Outputs.DOCHUCK_BLOW.ChannelIndex.Value = 3;
            Outputs.DOCHUCK_BLOW.PortIndex.Value = 5;
            Outputs.DOCHUCK_BLOW.Reverse.Value = false;
            Outputs.DOCHUCK_BLOW.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOCHUCK_BLOW.MaintainTime.Value = 150;
            Outputs.DOCHUCK_BLOW.TimeOut.Value = 0;

            Outputs.DOCHUCK_BLOW_12.ChannelIndex.Value = 0;
            Outputs.DOCHUCK_BLOW_12.PortIndex.Value = 13;
            Outputs.DOCHUCK_BLOW_12.Reverse.Value = false;
            Outputs.DOCHUCK_BLOW_12.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOCHUCK_BLOW_12.MaintainTime.Value = 150;
            Outputs.DOCHUCK_BLOW_12.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ALIGN_RETRACT.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.PortIndex.Value = 4;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.Reverse.Value = false;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ANTIPAD.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ANTIPAD.PortIndex.Value = 5;
            Outputs.DOBERNOULLI_ANTIPAD.Reverse.Value = false;
            Outputs.DOBERNOULLI_ANTIPAD.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ANTIPAD.MaintainTime.Value = 1500;
            Outputs.DOBERNOULLI_ANTIPAD.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ANTIPAD2.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ANTIPAD2.PortIndex.Value = 6;
            Outputs.DOBERNOULLI_ANTIPAD2.Reverse.Value = true;
            Outputs.DOBERNOULLI_ANTIPAD2.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ANTIPAD2.MaintainTime.Value = 1500;
            Outputs.DOBERNOULLI_ANTIPAD2.TimeOut.Value = 0;

            #endregion

            Outputs.DOTESTER_CLAMPED.ChannelIndex.Value = 2;
            Outputs.DOTESTER_CLAMPED.PortIndex.Value = 8;
            Outputs.DOTESTER_CLAMPED.Reverse.Value = false;
            Outputs.DOTESTER_CLAMPED.IOOveride.Value = EnumIOOverride.EMUL;
            Outputs.DOTESTER_CLAMPED.MaintainTime.Value = 50;
            Outputs.DOTESTER_CLAMPED.TimeOut.Value = 10000;

            Outputs.DOTESTER_UNCLAMPED.ChannelIndex.Value = 2;
            Outputs.DOTESTER_UNCLAMPED.PortIndex.Value = 0;
            Outputs.DOTESTER_UNCLAMPED.Reverse.Value = false;
            Outputs.DOTESTER_UNCLAMPED.IOOveride.Value = EnumIOOverride.EMUL;
            Outputs.DOTESTER_UNCLAMPED.MaintainTime.Value = 50;
            Outputs.DOTESTER_UNCLAMPED.TimeOut.Value = 10000;

            Inputs.DI_BACKSIDE_DOOR_OPEN.ChannelIndex.Value = 0;
            Inputs.DI_BACKSIDE_DOOR_OPEN.PortIndex.Value = 0;
            Inputs.DI_BACKSIDE_DOOR_OPEN.Reverse.Value = false;
            Inputs.DI_BACKSIDE_DOOR_OPEN.IOOveride.Value = EnumIOOverride.EMUL;
            Inputs.DI_BACKSIDE_DOOR_OPEN.MaintainTime.Value = 50;
            Inputs.DI_BACKSIDE_DOOR_OPEN.TimeOut.Value = 0;

            Inputs.DITESTERHEAD_PURGE.ChannelIndex.Value = 0;
            Inputs.DITESTERHEAD_PURGE.PortIndex.Value = 0;
            Inputs.DITESTERHEAD_PURGE.Reverse.Value = false;
            Inputs.DITESTERHEAD_PURGE.IOOveride.Value = EnumIOOverride.EMUL;
            Inputs.DITESTERHEAD_PURGE.MaintainTime.Value = 50;
            Inputs.DITESTERHEAD_PURGE.TimeOut.Value = 0;
        }

        private void SetDefaultParam_GP_OPRT()
        {
            Inputs.DIWAFERCAMMIDDLE.ChannelIndex.Value = 1;
            Inputs.DIWAFERCAMMIDDLE.PortIndex.Value = 8;
            Inputs.DIWAFERCAMMIDDLE.Reverse.Value = false;
            Inputs.DIWAFERCAMMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIWAFERCAMMIDDLE.MaintainTime.Value = 50;
            Inputs.DIWAFERCAMMIDDLE.TimeOut.Value = 10000;

            Inputs.DIWAFERCAMREAR.ChannelIndex.Value = 1;
            Inputs.DIWAFERCAMREAR.PortIndex.Value = 9;
            Inputs.DIWAFERCAMREAR.Reverse.Value = false;
            Inputs.DIWAFERCAMREAR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIWAFERCAMREAR.MaintainTime.Value = 50;
            Inputs.DIWAFERCAMREAR.TimeOut.Value = 10000;

            Inputs.DICARDCHANGE_IN.ChannelIndex.Value = 2;
            Inputs.DICARDCHANGE_IN.PortIndex.Value = 1;
            Inputs.DICARDCHANGE_IN.Reverse.Value = false;
            Inputs.DICARDCHANGE_IN.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DICARDCHANGE_IN.MaintainTime.Value = 50;
            Inputs.DICARDCHANGE_IN.TimeOut.Value = 10000;

            Inputs.DICARDCHANGE_OUT.ChannelIndex.Value = 2;
            Inputs.DICARDCHANGE_OUT.PortIndex.Value = 2;
            Inputs.DICARDCHANGE_OUT.Reverse.Value = false;
            Inputs.DICARDCHANGE_OUT.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DICARDCHANGE_OUT.MaintainTime.Value = 50;
            Inputs.DICARDCHANGE_OUT.TimeOut.Value = 10000;

            Inputs.DICARDCHANGE_IDLE.ChannelIndex.Value = 2;
            Inputs.DICARDCHANGE_IDLE.PortIndex.Value = 0;
            Inputs.DICARDCHANGE_IDLE.Reverse.Value = false;
            Inputs.DICARDCHANGE_IDLE.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DICARDCHANGE_IDLE.MaintainTime.Value = 50;
            Inputs.DICARDCHANGE_IDLE.TimeOut.Value = 10000;

            int ffuCount = 2;
            Inputs.DI_FFU_ONLINES = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < ffuCount; i++)
            {
                Inputs.DI_FFU_ONLINES.Add(new IOPortDescripter<bool>(nameof(Inputs.DI_FFU_ONLINES), EnumIOType.INPUT, EnumIOOverride.NONE));
            }
            Inputs.DI_FFU_ONLINES[0].ChannelIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[0].PortIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[0].Reverse.Value = false;
            Inputs.DI_FFU_ONLINES[0].IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DI_FFU_ONLINES[0].MaintainTime.Value = 500;
            Inputs.DI_FFU_ONLINES[0].TimeOut.Value = 10000;
            Inputs.DI_FFU_ONLINES[0].Description.Value = "FFU_FRONT";

            Inputs.DI_FFU_ONLINES[1].ChannelIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[1].PortIndex.Value = 0;
            Inputs.DI_FFU_ONLINES[1].Reverse.Value = false;
            Inputs.DI_FFU_ONLINES[1].IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DI_FFU_ONLINES[1].MaintainTime.Value = 500;
            Inputs.DI_FFU_ONLINES[1].TimeOut.Value = 10000;
            Inputs.DI_FFU_ONLINES[1].Description.Value = "FFU_BACK";


            Outputs.DOWAFERMIDDLE.ChannelIndex.Value = 1;
            Outputs.DOWAFERMIDDLE.PortIndex.Value = 10;
            Outputs.DOWAFERMIDDLE.Reverse.Value = false;
            Outputs.DOWAFERMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOWAFERMIDDLE.MaintainTime.Value = 50;
            Outputs.DOWAFERMIDDLE.TimeOut.Value = 10000;


            Outputs.DOWAFERREAR.ChannelIndex.Value = 1;
            Outputs.DOWAFERREAR.PortIndex.Value = 11;
            Outputs.DOWAFERREAR.Reverse.Value = false;
            Outputs.DOWAFERREAR.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOWAFERREAR.MaintainTime.Value = 50;
            Outputs.DOWAFERREAR.TimeOut.Value = 10000;

            #region // Remote loader
            int foupCount = 3;
            int cardTrayCnt = 1;
            int fixTrayCount = 9;
            int btCount = 5;
            int cardBuff = 15;
            int pressureSensorCount = 4;

            RemoteInputs.DIARM1VAC = new IOPortDescripter<bool>(0, 0, "DIARM1VAC", EnumIOType.INPUT);
            RemoteInputs.DIARM2VAC = new IOPortDescripter<bool>(0, 1, "DIARM2VAC", EnumIOType.INPUT);
            RemoteInputs.DICCARMVAC = new IOPortDescripter<bool>(0, 2, "DICCARMVAC", EnumIOType.INPUT);
            RemoteInputs.DIEMGState = new IOPortDescripter<bool>(4, 0, "DIEMGState", EnumIOType.INPUT);
            RemoteInputs.DILeftDoorOpen = new IOPortDescripter<bool>(4, 3, "DILeftDoorOpen", EnumIOType.INPUT);
            RemoteInputs.DILeftDoorClose = new IOPortDescripter<bool>(4, 4, "DILeftDoorClose", EnumIOType.INPUT);
            RemoteInputs.DIRightDoorClose = new IOPortDescripter<bool>(4, 5, "DIRightDoorClose", EnumIOType.INPUT);
            RemoteInputs.DIRightDoorOpen = new IOPortDescripter<bool>(4, 6, "DIRightDoorOpen", EnumIOType.INPUT);

            RemoteInputs.DIBUFVACS = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < btCount; i++)
            {
                RemoteInputs.DIBUFVACS.Add(new IOPortDescripter<bool>(0, 5 + i, $"DIBUFVACS", EnumIOType.INPUT));
            }

            RemoteInputs.DIFixTrays = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI6inchWaferOnFixTs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI8inchWaferOnFixTs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < fixTrayCount; i++)
            {
                RemoteInputs.DIFixTrays.Add(new IOPortDescripter<bool>(1, 0 + i, $"DIFixTray{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI6inchWaferOnFixTs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI6inchWaferOnFixT{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI8inchWaferOnFixTs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI8inchWaferOnFixT{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICardBuffs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < cardBuff; i++)
            {
                RemoteInputs.DICardBuffs.Add(new IOPortDescripter<bool>(4, 0 + i, $"DICardBuff{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DIMainAirs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < pressureSensorCount; i++)
            {
                RemoteInputs.DIMainAirs.Add(new IOPortDescripter<bool>(4, 7 + i, $"DIMainAir{i + 1}", EnumIOType.INPUT));
            }



            RemoteInputs.DIMovedInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIWaferOnInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI6inchWaferOnInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DI8inchWaferOnInSPs = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIOpenInSPs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DIMovedInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DIMovedInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DIWaferOnInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DIWaferOnInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI6inchWaferOnInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI6inchWaferOnInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DI8inchWaferOnInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DI8inchWaferOnInSP{i + 1}", EnumIOType.INPUT));
                RemoteInputs.DIOpenInSPs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DIOpenInSP{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICardOnCarrierVacs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < cardTrayCnt; i++)
            {
                RemoteInputs.DICardOnCarrierVacs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICardOnCarrierVacs{i + 1}", EnumIOType.INPUT));
            }
            #region // Foup
            RemoteInputs.DICSTLocks = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTLocks.Add(new IOPortDescripter<bool>(6, 7, "DICSTLock1", EnumIOType.INPUT));
            RemoteInputs.DICSTLocks.Add(new IOPortDescripter<bool>(8, 7, "DICSTLock1", EnumIOType.INPUT));
            RemoteInputs.DICSTLocks.Add(new IOPortDescripter<bool>(10, 7, "DICSTLock1", EnumIOType.INPUT));
            RemoteInputs.DICSTUnlocks = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTUnlocks.Add(new IOPortDescripter<bool>(6, 8, "DICSTUnlock1", EnumIOType.INPUT));
            RemoteInputs.DICSTUnlocks.Add(new IOPortDescripter<bool>(8, 8, "DICSTUnlock2", EnumIOType.INPUT));
            RemoteInputs.DICSTUnlocks.Add(new IOPortDescripter<bool>(10, 8, "DICSTUnlock3", EnumIOType.INPUT));

            RemoteInputs.DICSTLoads = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTLoads.Add(new IOPortDescripter<bool>(6, 12, "DICSTLoad1", EnumIOType.INPUT));
            RemoteInputs.DICSTLoads.Add(new IOPortDescripter<bool>(8, 12, "DICSTLoad2", EnumIOType.INPUT));
            RemoteInputs.DICSTLoads.Add(new IOPortDescripter<bool>(10, 12, "DICSTLoad3", EnumIOType.INPUT));
            RemoteInputs.DICSTUnLoads = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTUnLoads.Add(new IOPortDescripter<bool>(6, 13, "DICSTUnLoad1", EnumIOType.INPUT));
            RemoteInputs.DICSTUnLoads.Add(new IOPortDescripter<bool>(8, 13, "DICSTUnLoad2", EnumIOType.INPUT));
            RemoteInputs.DICSTUnLoads.Add(new IOPortDescripter<bool>(10, 13, "DICSTUnLoad3", EnumIOType.INPUT));

            RemoteInputs.DICSTCoverCloses = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTCoverCloses.Add(new IOPortDescripter<bool>(5, 1, "DICSTCoverClose1", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverCloses.Add(new IOPortDescripter<bool>(7, 1, "DICSTCoverClose2", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverCloses.Add(new IOPortDescripter<bool>(9, 1, "DICSTCoverClose3", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverOpens = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICSTCoverOpens.Add(new IOPortDescripter<bool>(5, 0, "DICSTCoverOpen1", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverOpens.Add(new IOPortDescripter<bool>(7, 0, "DICSTCoverOpen2", EnumIOType.INPUT));
            RemoteInputs.DICSTCoverOpens.Add(new IOPortDescripter<bool>(9, 0, "DICSTCoverOpen3", EnumIOType.INPUT));

            RemoteInputs.DIMappings = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIMappings.Add(new IOPortDescripter<bool>(2, 5, "DIMapping1", EnumIOType.INPUT));
            RemoteInputs.DIMappings.Add(new IOPortDescripter<bool>(3, 5, "DIMapping2", EnumIOType.INPUT));
            RemoteInputs.DIMappings.Add(new IOPortDescripter<bool>(4, 5, "DIMapping3", EnumIOType.INPUT));

            RemoteInputs.DIWaferOut = new List<IOPortDescripter<bool>>();
            RemoteInputs.DIWaferOut.Add(new IOPortDescripter<bool>(5, 5, "DIWaferOut1", EnumIOType.INPUT));
            RemoteInputs.DIWaferOut.Add(new IOPortDescripter<bool>(7, 5, "DIWaferOut2", EnumIOType.INPUT));
            RemoteInputs.DIWaferOut.Add(new IOPortDescripter<bool>(9, 5, "DIWaferOut3", EnumIOType.INPUT));

            RemoteInputs.DICoverVac = new List<IOPortDescripter<bool>>();
            RemoteInputs.DICoverVac.Add(new IOPortDescripter<bool>(5, 10, "DICoverVac1", EnumIOType.INPUT));
            RemoteInputs.DICoverVac.Add(new IOPortDescripter<bool>(7, 10, "DICoverVac2", EnumIOType.INPUT));
            RemoteInputs.DICoverVac.Add(new IOPortDescripter<bool>(9, 10, "DICoverVac3", EnumIOType.INPUT));

            RemoteOutputs.DOCSTCoverOpens = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverOpens.Add(new IOPortDescripter<bool>(2, 0, "DOCSTCoverOpen1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverOpens.Add(new IOPortDescripter<bool>(3, 0, "DOCSTCoverOpen2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverOpens.Add(new IOPortDescripter<bool>(4, 0, "DOCSTCoverOpen3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTCoverCloses = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverCloses.Add(new IOPortDescripter<bool>(2, 1, "DOCSTCoverClose1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverCloses.Add(new IOPortDescripter<bool>(3, 1, "DOCSTCoverClose2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverCloses.Add(new IOPortDescripter<bool>(4, 1, "DOCSTCoverClose3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTCoverLocks = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverLocks.Add(new IOPortDescripter<bool>(2, 2, "DOCSTCoverLock1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverLocks.Add(new IOPortDescripter<bool>(3, 2, "DOCSTCoverLock2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverLocks.Add(new IOPortDescripter<bool>(4, 2, "DOCSTCoverLock3", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverUnlock = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTCoverUnlock.Add(new IOPortDescripter<bool>(2, 3, "DOCSTCoverUnloc1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverUnlock.Add(new IOPortDescripter<bool>(3, 3, "DOCSTCoverUnloc2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTCoverUnlock.Add(new IOPortDescripter<bool>(4, 3, "DOCSTCoverUnloc3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTLoadLamps = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTLoadLamps.Add(new IOPortDescripter<bool>(2, 12, "DOCSTLoadLamp1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTLoadLamps.Add(new IOPortDescripter<bool>(3, 12, "DOCSTLoadLamp2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTLoadLamps.Add(new IOPortDescripter<bool>(4, 12, "DOCSTLoadLamp3", EnumIOType.OUTPUT));

            RemoteOutputs.DOCSTUnloadLamps = new List<IOPortDescripter<bool>>();
            RemoteOutputs.DOCSTUnloadLamps.Add(new IOPortDescripter<bool>(2, 13, "DOCSTUnloadLamp1", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTUnloadLamps.Add(new IOPortDescripter<bool>(3, 13, "DOCSTUnloadLamp2", EnumIOType.OUTPUT));
            RemoteOutputs.DOCSTUnloadLamps.Add(new IOPortDescripter<bool>(4, 13, "DOCSTUnloadLamp3", EnumIOType.OUTPUT));


            RemoteInputs.DICST12Locks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST12Locks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST12Lock{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICST12UnLocks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST12UnLocks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST12UnLock{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICST8Locks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST8Locks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST8Lock{i + 1}", EnumIOType.INPUT));
            }
            RemoteInputs.DICST8Unlocks = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < foupCount; i++)
            {
                RemoteInputs.DICST8Unlocks.Add(new IOPortDescripter<bool>(0, 0 + i, $"DICST8Unlock{i + 1}", EnumIOType.INPUT));
            }
            #endregion

            RemoteOutputs.DOARM1Vac = new IOPortDescripter<bool>(0, 0, "DOARM1Vac", EnumIOType.OUTPUT);
            RemoteOutputs.DOARMVac2 = new IOPortDescripter<bool>(0, 0, "DOARMVac2", EnumIOType.OUTPUT);
            RemoteOutputs.DOCCArmVac = new IOPortDescripter<bool>(0, 0, "DOCCArmVac", EnumIOType.OUTPUT);
            RemoteOutputs.DOBuffVacs = new List<IOPortDescripter<bool>>();
            for (int i = 0; i < btCount; i++)
            {
                RemoteOutputs.DOBuffVacs.Add(new IOPortDescripter<bool>(0, 0 + i, $"DOBuffVacs.{i + 1}", EnumIOType.OUTPUT));
            }

            RemoteOutputs.DOFrontSigRed = new IOPortDescripter<bool>(0, 0, "DOFrontSigRed", EnumIOType.OUTPUT);
            RemoteOutputs.DOFrontSigYl = new IOPortDescripter<bool>(0, 0, "DOFrontSigYl", EnumIOType.OUTPUT);
            RemoteOutputs.DOFrontSigGrn = new IOPortDescripter<bool>(0, 0, "DOFrontSigGrn", EnumIOType.OUTPUT);
            RemoteOutputs.DOFrontSigBuz = new IOPortDescripter<bool>(0, 0, "DOFrontSigBuz", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigRed = new IOPortDescripter<bool>(0, 0, "DORearSigRed", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigYl = new IOPortDescripter<bool>(0, 0, "DORearSigYl", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigGrn = new IOPortDescripter<bool>(0, 0, "DORearSigGrn", EnumIOType.OUTPUT);
            RemoteOutputs.DORearSigBuz = new IOPortDescripter<bool>(0, 0, "DORearSigBuz", EnumIOType.OUTPUT);
            #endregion

            #region ==> GROUP Prober CC

            //==> INPUT
            Inputs.DIUPMODULE_LEFT_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_LEFT_SENSOR.PortIndex.Value = 4;
            Inputs.DIUPMODULE_LEFT_SENSOR.Reverse.Value = false;
            Inputs.DIUPMODULE_LEFT_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_LEFT_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_LEFT_SENSOR.TimeOut.Value = 0;

            Inputs.DIUPMODULE_RIGHT_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_RIGHT_SENSOR.PortIndex.Value = 6;
            Inputs.DIUPMODULE_RIGHT_SENSOR.Reverse.Value = false;
            Inputs.DIUPMODULE_RIGHT_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_RIGHT_SENSOR.TimeOut.Value = 0;

            Inputs.DIUPMODULE_CARDEXIST_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.PortIndex.Value = 5;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.Reverse.Value = false;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value = 0;

            Inputs.DIUPMODULE_VACU_SENSOR.ChannelIndex.Value = 0;
            Inputs.DIUPMODULE_VACU_SENSOR.PortIndex.Value = 2;
            Inputs.DIUPMODULE_VACU_SENSOR.Reverse.Value = true;
            Inputs.DIUPMODULE_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIUPMODULE_VACU_SENSOR.MaintainTime.Value = 0;
            Inputs.DIUPMODULE_VACU_SENSOR.TimeOut.Value = 0;

            Inputs.DITESTER_DOCKING_SENSOR.ChannelIndex.Value = 1;
            Inputs.DITESTER_DOCKING_SENSOR.PortIndex.Value = 0;
            Inputs.DITESTER_DOCKING_SENSOR.Reverse.Value = true;
            Inputs.DITESTER_DOCKING_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITESTER_DOCKING_SENSOR.MaintainTime.Value = 0;
            Inputs.DITESTER_DOCKING_SENSOR.TimeOut.Value = 0;

            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.ChannelIndex.Value = 1;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.PortIndex.Value = 1;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.Reverse.Value = false;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.MaintainTime.Value = 0;
            Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.TimeOut.Value = 0;

            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.ChannelIndex.Value = 1;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.PortIndex.Value = 2;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.Reverse.Value = false;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.MaintainTime.Value = 0;
            Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.TimeOut.Value = 0;

            Inputs.DIPOGOCARD_VACU_SENSOR.ChannelIndex.Value = 1;
            Inputs.DIPOGOCARD_VACU_SENSOR.PortIndex.Value = 3;
            Inputs.DIPOGOCARD_VACU_SENSOR.Reverse.Value = false;
            Inputs.DIPOGOCARD_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIPOGOCARD_VACU_SENSOR.MaintainTime.Value = 0;
            Inputs.DIPOGOCARD_VACU_SENSOR.TimeOut.Value = 0;

            Inputs.DIPOGOTESTER_VACU_SENSOR.ChannelIndex.Value = 1;
            Inputs.DIPOGOTESTER_VACU_SENSOR.PortIndex.Value = 4;
            Inputs.DIPOGOTESTER_VACU_SENSOR.Reverse.Value = false;
            Inputs.DIPOGOTESTER_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIPOGOTESTER_VACU_SENSOR.MaintainTime.Value = 0;
            Inputs.DIPOGOTESTER_VACU_SENSOR.TimeOut.Value = 0;

            Inputs.DITPLATEIN_SENSOR.ChannelIndex.Value = 1;
            Inputs.DITPLATEIN_SENSOR.PortIndex.Value = 5;
            Inputs.DITPLATEIN_SENSOR.Reverse.Value = true;
            Inputs.DITPLATEIN_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DITPLATEIN_SENSOR.MaintainTime.Value = 0;
            Inputs.DITPLATEIN_SENSOR.TimeOut.Value = 0;

            //==> OUTPUT
            Outputs.DOUPMODULE_DOWN.ChannelIndex.Value = 1;
            Outputs.DOUPMODULE_DOWN.PortIndex.Value = 11;
            Outputs.DOUPMODULE_DOWN.Reverse.Value = false;
            Outputs.DOUPMODULE_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOUPMODULE_DOWN.MaintainTime.Value = 0;
            Outputs.DOUPMODULE_DOWN.TimeOut.Value = 0;

            Outputs.DOUPMODULE_UP.ChannelIndex.Value = 1;
            Outputs.DOUPMODULE_UP.PortIndex.Value = 10;
            Outputs.DOUPMODULE_UP.Reverse.Value = false;
            Outputs.DOUPMODULE_UP.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOUPMODULE_UP.MaintainTime.Value = 0;
            Outputs.DOUPMODULE_UP.TimeOut.Value = 0;

            Outputs.DOUPMODULE_VACU.ChannelIndex.Value = 1;
            Outputs.DOUPMODULE_VACU.PortIndex.Value = 5;
            Outputs.DOUPMODULE_VACU.Reverse.Value = false;
            Outputs.DOUPMODULE_VACU.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOUPMODULE_VACU.MaintainTime.Value = 0;
            Outputs.DOUPMODULE_VACU.TimeOut.Value = 0;

            Outputs.DOTPLATE_PCLATCH_LOCK.ChannelIndex.Value = 2;
            Outputs.DOTPLATE_PCLATCH_LOCK.PortIndex.Value = 0;
            Outputs.DOTPLATE_PCLATCH_LOCK.Reverse.Value = false;
            Outputs.DOTPLATE_PCLATCH_LOCK.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOTPLATE_PCLATCH_LOCK.MaintainTime.Value = 0;
            Outputs.DOTPLATE_PCLATCH_LOCK.TimeOut.Value = 0;

            Outputs.DOPOGOCARD_VACU_RELEASE.ChannelIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU_RELEASE.PortIndex.Value = 1;
            Outputs.DOPOGOCARD_VACU_RELEASE.Reverse.Value = false;
            Outputs.DOPOGOCARD_VACU_RELEASE.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOCARD_VACU_RELEASE.MaintainTime.Value = 0;
            Outputs.DOPOGOCARD_VACU_RELEASE.TimeOut.Value = 0;

            Outputs.DOPOGOCARD_VACU.ChannelIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU.PortIndex.Value = 2;
            Outputs.DOPOGOCARD_VACU.Reverse.Value = false;
            Outputs.DOPOGOCARD_VACU.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOCARD_VACU.MaintainTime.Value = 0;
            Outputs.DOPOGOCARD_VACU.TimeOut.Value = 0;

            Outputs.DOPOGOTESTER_VACU_RELEASE.ChannelIndex.Value = 2;
            Outputs.DOPOGOTESTER_VACU_RELEASE.PortIndex.Value = 3;
            Outputs.DOPOGOTESTER_VACU_RELEASE.Reverse.Value = false;
            Outputs.DOPOGOTESTER_VACU_RELEASE.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOTESTER_VACU_RELEASE.MaintainTime.Value = 0;
            Outputs.DOPOGOTESTER_VACU_RELEASE.TimeOut.Value = 0;

            Outputs.DOPOGOTESTER_VACU.ChannelIndex.Value = 2;
            Outputs.DOPOGOTESTER_VACU.PortIndex.Value = 4;
            Outputs.DOPOGOTESTER_VACU.Reverse.Value = false;
            Outputs.DOPOGOTESTER_VACU.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOPOGOTESTER_VACU.MaintainTime.Value = 0;
            Outputs.DOPOGOTESTER_VACU.TimeOut.Value = 0;
            #endregion

            #region ==> Bernoulli
            Inputs.DIBERNOULLI_HANDLER_UP.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_HANDLER_UP.PortIndex.Value = 0;
            Inputs.DIBERNOULLI_HANDLER_UP.Reverse.Value = true;
            Inputs.DIBERNOULLI_HANDLER_UP.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_HANDLER_UP.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_HANDLER_UP.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_HANDLER_DOWN.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_HANDLER_DOWN.PortIndex.Value = 1;
            Inputs.DIBERNOULLI_HANDLER_DOWN.Reverse.Value = true;
            Inputs.DIBERNOULLI_HANDLER_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_HANDLER_DOWN.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_HANDLER_DOWN.TimeOut.Value = 10000;

            Inputs.DIBERNOULLIWAFER_EXIST.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLIWAFER_EXIST.PortIndex.Value = 2;
            Inputs.DIBERNOULLIWAFER_EXIST.Reverse.Value = true;
            Inputs.DIBERNOULLIWAFER_EXIST.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLIWAFER_EXIST.MaintainTime.Value = 0;
            Inputs.DIBERNOULLIWAFER_EXIST.TimeOut.Value = 1000;

            Inputs.DIBERNOULLI_ALIGN1_ON.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN1_ON.PortIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN1_ON.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN1_ON.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN1_ON.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN1_ON.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN1_OFF.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN1_OFF.PortIndex.Value = 4;
            Inputs.DIBERNOULLI_ALIGN1_OFF.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN1_OFF.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN1_OFF.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN1_OFF.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN2_ON.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN2_ON.PortIndex.Value = 5;
            Inputs.DIBERNOULLI_ALIGN2_ON.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN2_ON.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN2_ON.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN2_ON.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN2_OFF.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN2_OFF.PortIndex.Value = 6;
            Inputs.DIBERNOULLI_ALIGN2_OFF.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN2_OFF.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN2_OFF.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN2_OFF.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN3_ON.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN3_ON.PortIndex.Value = 7;
            Inputs.DIBERNOULLI_ALIGN3_ON.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN3_ON.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN3_ON.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN3_ON.TimeOut.Value = 10000;

            Inputs.DIBERNOULLI_ALIGN3_OFF.ChannelIndex.Value = 3;
            Inputs.DIBERNOULLI_ALIGN3_OFF.PortIndex.Value = 8;
            Inputs.DIBERNOULLI_ALIGN3_OFF.Reverse.Value = true;
            Inputs.DIBERNOULLI_ALIGN3_OFF.IOOveride.Value = EnumIOOverride.NONE;
            Inputs.DIBERNOULLI_ALIGN3_OFF.MaintainTime.Value = 0;
            Inputs.DIBERNOULLI_ALIGN3_OFF.TimeOut.Value = 10000;


            Outputs.DOBERNOULLI_HANDLER_UP.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_HANDLER_UP.PortIndex.Value = 0;
            Outputs.DOBERNOULLI_HANDLER_UP.Reverse.Value = false;
            Outputs.DOBERNOULLI_HANDLER_UP.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_HANDLER_UP.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_HANDLER_UP.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_HANDLER_DOWN.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_HANDLER_DOWN.PortIndex.Value = 1;
            Outputs.DOBERNOULLI_HANDLER_DOWN.Reverse.Value = false;
            Outputs.DOBERNOULLI_HANDLER_DOWN.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_HANDLER_DOWN.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_HANDLER_DOWN.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_6INCH.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_6INCH.PortIndex.Value = 2;
            Outputs.DOBERNOULLI_6INCH.Reverse.Value = false;
            Outputs.DOBERNOULLI_6INCH.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_6INCH.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_6INCH.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_8INCH.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_8INCH.PortIndex.Value = 3;
            Outputs.DOBERNOULLI_8INCH.Reverse.Value = false;
            Outputs.DOBERNOULLI_8INCH.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_8INCH.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_8INCH.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_12INCH.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_12INCH.PortIndex.Value = 6;
            Outputs.DOBERNOULLI_12INCH.Reverse.Value = false;
            Outputs.DOBERNOULLI_12INCH.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_12INCH.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_12INCH.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ALIGN_EXTEND.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.PortIndex.Value = 4;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.Reverse.Value = false;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_ALIGN_EXTEND.TimeOut.Value = 0;

            Outputs.DOCHUCK_BLOW.ChannelIndex.Value = 3;
            Outputs.DOCHUCK_BLOW.PortIndex.Value = 5;
            Outputs.DOCHUCK_BLOW.Reverse.Value = false;
            Outputs.DOCHUCK_BLOW.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOCHUCK_BLOW.MaintainTime.Value = 150;
            Outputs.DOCHUCK_BLOW.TimeOut.Value = 0;

            Outputs.DOCHUCK_BLOW_12.ChannelIndex.Value = 0;
            Outputs.DOCHUCK_BLOW_12.PortIndex.Value = 13;
            Outputs.DOCHUCK_BLOW_12.Reverse.Value = false;
            Outputs.DOCHUCK_BLOW_12.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOCHUCK_BLOW_12.MaintainTime.Value = 150;
            Outputs.DOCHUCK_BLOW_12.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ALIGN_RETRACT.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.PortIndex.Value = 4;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.Reverse.Value = false;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.MaintainTime.Value = 0;
            Outputs.DOBERNOULLI_ALIGN_RETRACT.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ANTIPAD.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ANTIPAD.PortIndex.Value = 5;
            Outputs.DOBERNOULLI_ANTIPAD.Reverse.Value = false;
            Outputs.DOBERNOULLI_ANTIPAD.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ANTIPAD.MaintainTime.Value = 1500;
            Outputs.DOBERNOULLI_ANTIPAD.TimeOut.Value = 0;

            Outputs.DOBERNOULLI_ANTIPAD2.ChannelIndex.Value = 3;
            Outputs.DOBERNOULLI_ANTIPAD2.PortIndex.Value = 6;
            Outputs.DOBERNOULLI_ANTIPAD2.Reverse.Value = true;
            Outputs.DOBERNOULLI_ANTIPAD2.IOOveride.Value = EnumIOOverride.NONE;
            Outputs.DOBERNOULLI_ANTIPAD2.MaintainTime.Value = 1500;
            Outputs.DOBERNOULLI_ANTIPAD2.TimeOut.Value = 0;


            #endregion

        }
        private void SetDefaultParam_OPUSV()
        {
            try
            {


                Inputs.DIWAFERCAMMIDDLE.ChannelIndex.Value = 1;
                Inputs.DIWAFERCAMMIDDLE.PortIndex.Value = 8;
                Inputs.DIWAFERCAMMIDDLE.Reverse.Value = false;
                Inputs.DIWAFERCAMMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERCAMMIDDLE.MaintainTime.Value = 50;
                Inputs.DIWAFERCAMMIDDLE.TimeOut.Value = 10000;

                Inputs.DIWAFERCAMREAR.ChannelIndex.Value = 1;
                Inputs.DIWAFERCAMREAR.PortIndex.Value = 9;
                Inputs.DIWAFERCAMREAR.Reverse.Value = false;
                Inputs.DIWAFERCAMREAR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERCAMREAR.MaintainTime.Value = 50;
                Inputs.DIWAFERCAMREAR.TimeOut.Value = 10000;

                Inputs.DICARDCHANGE_IN.ChannelIndex.Value = 2;
                Inputs.DICARDCHANGE_IN.PortIndex.Value = 1;
                Inputs.DICARDCHANGE_IN.Reverse.Value = false;
                Inputs.DICARDCHANGE_IN.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DICARDCHANGE_IN.MaintainTime.Value = 50;
                Inputs.DICARDCHANGE_IN.TimeOut.Value = 10000;

                Inputs.DICARDCHANGE_OUT.ChannelIndex.Value = 2;
                Inputs.DICARDCHANGE_OUT.PortIndex.Value = 2;
                Inputs.DICARDCHANGE_OUT.Reverse.Value = false;
                Inputs.DICARDCHANGE_OUT.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DICARDCHANGE_OUT.MaintainTime.Value = 50;
                Inputs.DICARDCHANGE_OUT.TimeOut.Value = 10000;

                Inputs.DICARDCHANGE_IDLE.ChannelIndex.Value = 2;
                Inputs.DICARDCHANGE_IDLE.PortIndex.Value = 0;
                Inputs.DICARDCHANGE_IDLE.Reverse.Value = false;
                Inputs.DICARDCHANGE_IDLE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DICARDCHANGE_IDLE.MaintainTime.Value = 50;
                Inputs.DICARDCHANGE_IDLE.TimeOut.Value = 10000;

                Outputs.DOWAFERMIDDLE.ChannelIndex.Value = 1;
                Outputs.DOWAFERMIDDLE.PortIndex.Value = 10;
                Outputs.DOWAFERMIDDLE.Reverse.Value = false;
                Outputs.DOWAFERMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOWAFERMIDDLE.MaintainTime.Value = 50;
                Outputs.DOWAFERMIDDLE.TimeOut.Value = 10000;


                Outputs.DOWAFERREAR.ChannelIndex.Value = 1;
                Outputs.DOWAFERREAR.PortIndex.Value = 11;
                Outputs.DOWAFERREAR.Reverse.Value = false;
                Outputs.DOWAFERREAR.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOWAFERREAR.MaintainTime.Value = 50;
                Outputs.DOWAFERREAR.TimeOut.Value = 10000;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetDefaultParam_OPUSVLoader()
        {
            try
            {

                Inputs.DIWAFERONARM.ChannelIndex.Value = 0;
                Inputs.DIWAFERONARM.PortIndex.Value = 1;
                Inputs.DIWAFERONARM.Reverse.Value = false;
                Inputs.DIWAFERONARM.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONARM.MaintainTime.Value = 50;
                Inputs.DIWAFERONARM.TimeOut.Value = 60000;

                Inputs.DIWAFERONARM2.ChannelIndex.Value = 0;
                Inputs.DIWAFERONARM2.PortIndex.Value = 2;
                Inputs.DIWAFERONARM2.Reverse.Value = false;
                Inputs.DIWAFERONARM2.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONARM2.MaintainTime.Value = 50;
                Inputs.DIWAFERONARM2.TimeOut.Value = 60000;

                Inputs.DIMAINAIR.ChannelIndex.Value = 0;
                Inputs.DIMAINAIR.PortIndex.Value = 4;
                Inputs.DIMAINAIR.Reverse.Value = false;
                Inputs.DIMAINAIR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIMAINAIR.MaintainTime.Value = 50;
                Inputs.DIMAINAIR.TimeOut.Value = 60000;

                Inputs.DIMAINVAC.ChannelIndex.Value = 0;
                Inputs.DIMAINVAC.PortIndex.Value = 5;
                Inputs.DIMAINVAC.Reverse.Value = false;
                Inputs.DIMAINVAC.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIMAINVAC.MaintainTime.Value = 50;
                Inputs.DIMAINVAC.TimeOut.Value = 60000;

                Inputs.DIDOORCLOSE.ChannelIndex.Value = 1;
                Inputs.DIDOORCLOSE.PortIndex.Value = 0;
                Inputs.DIDOORCLOSE.Reverse.Value = false;
                Inputs.DIDOORCLOSE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIDOORCLOSE.MaintainTime.Value = 50;
                Inputs.DIDOORCLOSE.TimeOut.Value = 60000;

                Inputs.DIWAFERSENSOR.ChannelIndex.Value = 1;
                Inputs.DIWAFERSENSOR.PortIndex.Value = 1;
                Inputs.DIWAFERSENSOR.Reverse.Value = false;
                Inputs.DIWAFERSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERSENSOR.MaintainTime.Value = 50;
                Inputs.DIWAFERSENSOR.TimeOut.Value = 60000;

                Inputs.DI8PRS1.ChannelIndex.Value = 1;
                Inputs.DI8PRS1.PortIndex.Value = 2;
                Inputs.DI8PRS1.Reverse.Value = false;
                Inputs.DI8PRS1.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS1.MaintainTime.Value = 50;
                Inputs.DI8PRS1.TimeOut.Value = 60000;

                Inputs.DI8PRS2.ChannelIndex.Value = 1;
                Inputs.DI8PRS2.PortIndex.Value = 3;
                Inputs.DI8PRS2.Reverse.Value = false;
                Inputs.DI8PRS2.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS2.MaintainTime.Value = 50;
                Inputs.DI8PRS2.TimeOut.Value = 60000;

                Inputs.DI8PRS3.ChannelIndex.Value = 1;
                Inputs.DI8PRS3.PortIndex.Value = 4;
                Inputs.DI8PRS3.Reverse.Value = false;
                Inputs.DI8PRS3.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS3.MaintainTime.Value = 50;
                Inputs.DI8PRS3.TimeOut.Value = 60000;

                Inputs.DI8PLA.ChannelIndex.Value = 1;
                Inputs.DI8PLA.PortIndex.Value = 5;
                Inputs.DI8PLA.Reverse.Value = false;
                Inputs.DI8PLA.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PLA.MaintainTime.Value = 50;
                Inputs.DI8PLA.TimeOut.Value = 60000;

                Inputs.DI6PLA.ChannelIndex.Value = 1;
                Inputs.DI6PLA.PortIndex.Value = 6;
                Inputs.DI6PLA.Reverse.Value = false;
                Inputs.DI6PLA.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI6PLA.MaintainTime.Value = 50;
                Inputs.DI6PLA.TimeOut.Value = 60000;

                Inputs.DIFOUPSWINGSENSOR.ChannelIndex.Value = 1;
                Inputs.DIFOUPSWINGSENSOR.PortIndex.Value = 7;
                Inputs.DIFOUPSWINGSENSOR.Reverse.Value = false;
                Inputs.DIFOUPSWINGSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFOUPSWINGSENSOR.MaintainTime.Value = 50;
                Inputs.DIFOUPSWINGSENSOR.TimeOut.Value = 60000;

                Inputs.DIWAFERDETECTSENSOR.ChannelIndex.Value = 1;
                Inputs.DIWAFERDETECTSENSOR.PortIndex.Value = 8;
                Inputs.DIWAFERDETECTSENSOR.Reverse.Value = false;
                Inputs.DIWAFERDETECTSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERDETECTSENSOR.MaintainTime.Value = 50;
                Inputs.DIWAFERDETECTSENSOR.TimeOut.Value = 60000;

                Inputs.DISCANMAPPINGSENSOR.ChannelIndex.Value = 1;
                Inputs.DISCANMAPPINGSENSOR.PortIndex.Value = 10;
                Inputs.DISCANMAPPINGSENSOR.Reverse.Value = false;
                Inputs.DISCANMAPPINGSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DISCANMAPPINGSENSOR.MaintainTime.Value = 50;
                Inputs.DISCANMAPPINGSENSOR.TimeOut.Value = 60000;

                Inputs.DITRAYDETECTSENSOR.ChannelIndex.Value = 1;
                Inputs.DITRAYDETECTSENSOR.PortIndex.Value = 11;
                Inputs.DITRAYDETECTSENSOR.Reverse.Value = false;
                Inputs.DITRAYDETECTSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITRAYDETECTSENSOR.MaintainTime.Value = 50;
                Inputs.DITRAYDETECTSENSOR.TimeOut.Value = 60000;

                Inputs.DIWAFERONSUBCHUCK.ChannelIndex.Value = 0;
                Inputs.DIWAFERONSUBCHUCK.PortIndex.Value = 12;
                Inputs.DIWAFERONSUBCHUCK.Reverse.Value = false;
                Inputs.DIWAFERONSUBCHUCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONSUBCHUCK.MaintainTime.Value = 50;
                Inputs.DIWAFERONSUBCHUCK.TimeOut.Value = 60000;


                Outputs.DOOCR1LIGHT.ChannelIndex.Value = 0;
                Outputs.DOOCR1LIGHT.PortIndex.Value = 0;
                Outputs.DOOCR1LIGHT.Reverse.Value = false;
                Outputs.DOOCR1LIGHT.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOOCR1LIGHT.MaintainTime.Value = 50;
                Outputs.DOOCR1LIGHT.TimeOut.Value = 60000;

                Outputs.DOOCR2LIGHT.ChannelIndex.Value = 0;
                Outputs.DOOCR2LIGHT.PortIndex.Value = 1;
                Outputs.DOOCR2LIGHT.Reverse.Value = false;
                Outputs.DOOCR2LIGHT.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOOCR2LIGHT.MaintainTime.Value = 50;
                Outputs.DOOCR2LIGHT.TimeOut.Value = 60000;

                Outputs.DOOCR3LIGHT.ChannelIndex.Value = 0;
                Outputs.DOOCR3LIGHT.PortIndex.Value = 2;
                Outputs.DOOCR3LIGHT.Reverse.Value = false;
                Outputs.DOOCR3LIGHT.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOOCR3LIGHT.MaintainTime.Value = 50;
                Outputs.DOOCR3LIGHT.TimeOut.Value = 60000;

                Outputs.DOPACL.ChannelIndex.Value = 0;
                Outputs.DOPACL.PortIndex.Value = 3;
                Outputs.DOPACL.Reverse.Value = false;
                Outputs.DOPACL.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPACL.MaintainTime.Value = 50;
                Outputs.DOPACL.TimeOut.Value = 60000;

                Outputs.DOREDLAMP.ChannelIndex.Value = 1;
                Outputs.DOREDLAMP.PortIndex.Value = 0;
                Outputs.DOREDLAMP.Reverse.Value = false;
                Outputs.DOREDLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOREDLAMP.MaintainTime.Value = 50;
                Outputs.DOREDLAMP.TimeOut.Value = 60000;

                Outputs.DOGREENLAMP.ChannelIndex.Value = 1;
                Outputs.DOGREENLAMP.PortIndex.Value = 1;
                Outputs.DOGREENLAMP.Reverse.Value = false;
                Outputs.DOGREENLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOGREENLAMP.MaintainTime.Value = 50;
                Outputs.DOGREENLAMP.TimeOut.Value = 60000;

                Outputs.DOYELLOWLAMP.ChannelIndex.Value = 1;
                Outputs.DOYELLOWLAMP.PortIndex.Value = 2;
                Outputs.DOYELLOWLAMP.Reverse.Value = false;
                Outputs.DOYELLOWLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOYELLOWLAMP.MaintainTime.Value = 50;
                Outputs.DOYELLOWLAMP.TimeOut.Value = 60000;

                Outputs.DOCASSETTELOCK.ChannelIndex.Value = 2;
                Outputs.DOCASSETTELOCK.PortIndex.Value = 0;
                Outputs.DOCASSETTELOCK.Reverse.Value = false;
                Outputs.DOCASSETTELOCK.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCASSETTELOCK.MaintainTime.Value = 50;
                Outputs.DOCASSETTELOCK.TimeOut.Value = 60000;

                Outputs.DOFOUPSWING.ChannelIndex.Value = 2;
                Outputs.DOFOUPSWING.PortIndex.Value = 1;
                Outputs.DOFOUPSWING.Reverse.Value = false;
                Outputs.DOFOUPSWING.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOFOUPSWING.MaintainTime.Value = 50;
                Outputs.DOFOUPSWING.TimeOut.Value = 60000;


                Outputs.DOSUBCHUCKAIRON.ChannelIndex.Value = 2;
                Outputs.DOSUBCHUCKAIRON.PortIndex.Value = 2;
                Outputs.DOSUBCHUCKAIRON.Reverse.Value = false;
                Outputs.DOSUBCHUCKAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOSUBCHUCKAIRON.MaintainTime.Value = 50;
                Outputs.DOSUBCHUCKAIRON.TimeOut.Value = 60000;

                Outputs.DOARMAIRON.ChannelIndex.Value = 2;
                Outputs.DOARMAIRON.PortIndex.Value = 5;
                Outputs.DOARMAIRON.Reverse.Value = false;
                Outputs.DOARMAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOARMAIRON.MaintainTime.Value = 50;
                Outputs.DOARMAIRON.TimeOut.Value = 60000;

                Outputs.DOARM2AIRON.ChannelIndex.Value = 2;
                Outputs.DOARM2AIRON.PortIndex.Value = 6;
                Outputs.DOARM2AIRON.Reverse.Value = false;
                Outputs.DOARM2AIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOARM2AIRON.MaintainTime.Value = 50;
                Outputs.DOARM2AIRON.TimeOut.Value = 60000;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetDefaultParam_OPUSV_Test()
        {
            try
            {
                Inputs.DIWAFERCAMMIDDLE.ChannelIndex.Value = 1;
                Inputs.DIWAFERCAMMIDDLE.PortIndex.Value = 8;
                Inputs.DIWAFERCAMMIDDLE.Reverse.Value = false;
                Inputs.DIWAFERCAMMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERCAMMIDDLE.MaintainTime.Value = 50;
                Inputs.DIWAFERCAMMIDDLE.TimeOut.Value = 10000;

                Inputs.DIWAFERCAMREAR.ChannelIndex.Value = 1;
                Inputs.DIWAFERCAMREAR.PortIndex.Value = 9;
                Inputs.DIWAFERCAMREAR.Reverse.Value = false;
                Inputs.DIWAFERCAMREAR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERCAMREAR.MaintainTime.Value = 50;
                Inputs.DIWAFERCAMREAR.TimeOut.Value = 10000;


                Inputs.DIWAFERONCHUCK.ChannelIndex.Value = 5;
                Inputs.DIWAFERONCHUCK.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK_6.ChannelIndex.Value = 5;
                Inputs.DIWAFERONCHUCK_6.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK_6.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK_6.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK_6.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK_6.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK_8.ChannelIndex.Value = 5;
                Inputs.DIWAFERONCHUCK_8.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK_8.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK_8.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK_8.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK_8.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK_12.ChannelIndex.Value = 5;
                Inputs.DIWAFERONCHUCK_12.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK_12.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK_12.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK_12.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK_12.TimeOut.Value = 10000;

                Inputs.DINC_SENSOR.ChannelIndex.Value = 1;
                Inputs.DINC_SENSOR.PortIndex.Value = 8;
                Inputs.DINC_SENSOR.Reverse.Value = false;
                Inputs.DINC_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DINC_SENSOR.MaintainTime.Value = 50;
                Inputs.DINC_SENSOR.TimeOut.Value = 10000;

                Inputs.DIEMGSTOPSW.ChannelIndex.Value = 1;
                Inputs.DIEMGSTOPSW.PortIndex.Value = 2;
                Inputs.DIEMGSTOPSW.Reverse.Value = false;
                Inputs.DIEMGSTOPSW.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIEMGSTOPSW.MaintainTime.Value = 50;
                Inputs.DIEMGSTOPSW.TimeOut.Value = 10000;

                //==> Group Prober Card Change
                Inputs.DIUPMODULE_LEFT_SENSOR.ChannelIndex.Value = 0;
                Inputs.DIUPMODULE_LEFT_SENSOR.PortIndex.Value = 4;
                Inputs.DIUPMODULE_LEFT_SENSOR.Reverse.Value = false;
                Inputs.DIUPMODULE_LEFT_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIUPMODULE_LEFT_SENSOR.MaintainTime.Value = 0;
                Inputs.DIUPMODULE_LEFT_SENSOR.TimeOut.Value = 0;

                Inputs.DIUPMODULE_RIGHT_SENSOR.ChannelIndex.Value = 0;
                Inputs.DIUPMODULE_RIGHT_SENSOR.PortIndex.Value = 6;
                Inputs.DIUPMODULE_RIGHT_SENSOR.Reverse.Value = false;
                Inputs.DIUPMODULE_RIGHT_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value = 0;
                Inputs.DIUPMODULE_RIGHT_SENSOR.TimeOut.Value = 0;

                Inputs.DIUPMODULE_CARDEXIST_SENSOR.ChannelIndex.Value = 0;
                Inputs.DIUPMODULE_CARDEXIST_SENSOR.PortIndex.Value = 5;
                Inputs.DIUPMODULE_CARDEXIST_SENSOR.Reverse.Value = false;
                Inputs.DIUPMODULE_CARDEXIST_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value = 0;
                Inputs.DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value = 0;

                Inputs.DIUPMODULE_VACU_SENSOR.ChannelIndex.Value = 0;
                Inputs.DIUPMODULE_VACU_SENSOR.PortIndex.Value = 2;
                Inputs.DIUPMODULE_VACU_SENSOR.Reverse.Value = true;
                Inputs.DIUPMODULE_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIUPMODULE_VACU_SENSOR.MaintainTime.Value = 0;
                Inputs.DIUPMODULE_VACU_SENSOR.TimeOut.Value = 0;

                Inputs.DITESTER_DOCKING_SENSOR.ChannelIndex.Value = 1;
                Inputs.DITESTER_DOCKING_SENSOR.PortIndex.Value = 0;
                Inputs.DITESTER_DOCKING_SENSOR.Reverse.Value = true;
                Inputs.DITESTER_DOCKING_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITESTER_DOCKING_SENSOR.MaintainTime.Value = 0;
                Inputs.DITESTER_DOCKING_SENSOR.TimeOut.Value = 0;

                Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.ChannelIndex.Value = 1;
                Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.PortIndex.Value = 1;
                Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.Reverse.Value = false;
                Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.MaintainTime.Value = 0;
                Inputs.DITPLATE_PCLATCH_SENSOR_LOCK.TimeOut.Value = 0;

                Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.ChannelIndex.Value = 1;
                Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.PortIndex.Value = 2;
                Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.Reverse.Value = false;
                Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.MaintainTime.Value = 0;
                Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK.TimeOut.Value = 0;

                Inputs.DIPOGOCARD_VACU_SENSOR.ChannelIndex.Value = 1;
                Inputs.DIPOGOCARD_VACU_SENSOR.PortIndex.Value = 3;
                Inputs.DIPOGOCARD_VACU_SENSOR.Reverse.Value = false;
                Inputs.DIPOGOCARD_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIPOGOCARD_VACU_SENSOR.MaintainTime.Value = 0;
                Inputs.DIPOGOCARD_VACU_SENSOR.TimeOut.Value = 0;

                Inputs.DIPOGOTESTER_VACU_SENSOR.ChannelIndex.Value = 1;
                Inputs.DIPOGOTESTER_VACU_SENSOR.PortIndex.Value = 4;
                Inputs.DIPOGOTESTER_VACU_SENSOR.Reverse.Value = false;
                Inputs.DIPOGOTESTER_VACU_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIPOGOTESTER_VACU_SENSOR.MaintainTime.Value = 0;
                Inputs.DIPOGOTESTER_VACU_SENSOR.TimeOut.Value = 0;

                Inputs.DITPLATEIN_SENSOR.ChannelIndex.Value = 1;
                Inputs.DITPLATEIN_SENSOR.PortIndex.Value = 5;
                Inputs.DITPLATEIN_SENSOR.Reverse.Value = true;
                Inputs.DITPLATEIN_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITPLATEIN_SENSOR.MaintainTime.Value = 0;
                Inputs.DITPLATEIN_SENSOR.TimeOut.Value = 0;



                Outputs.DOWAFERMIDDLE.ChannelIndex.Value = 1;
                Outputs.DOWAFERMIDDLE.PortIndex.Value = 10;
                Outputs.DOWAFERMIDDLE.Reverse.Value = false;
                Outputs.DOWAFERMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOWAFERMIDDLE.MaintainTime.Value = 50;
                Outputs.DOWAFERMIDDLE.TimeOut.Value = 10000;


                Outputs.DOWAFERREAR.ChannelIndex.Value = 1;
                Outputs.DOWAFERREAR.PortIndex.Value = 11;
                Outputs.DOWAFERREAR.Reverse.Value = false;
                Outputs.DOWAFERREAR.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOWAFERREAR.MaintainTime.Value = 50;
                Outputs.DOWAFERREAR.TimeOut.Value = 10000;


                Outputs.DOCHUCKAIRON_0.ChannelIndex.Value = 1;
                Outputs.DOCHUCKAIRON_0.PortIndex.Value = 0;
                Outputs.DOCHUCKAIRON_0.Reverse.Value = false;
                Outputs.DOCHUCKAIRON_0.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCHUCKAIRON_0.MaintainTime.Value = 50;
                Outputs.DOCHUCKAIRON_0.TimeOut.Value = 10000;

                Outputs.DOCHUCKAIRON_1.ChannelIndex.Value = 1;
                Outputs.DOCHUCKAIRON_1.PortIndex.Value = 1;
                Outputs.DOCHUCKAIRON_1.Reverse.Value = false;
                Outputs.DOCHUCKAIRON_1.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCHUCKAIRON_1.MaintainTime.Value = 50;
                Outputs.DOCHUCKAIRON_1.TimeOut.Value = 10000;

                Outputs.DOCHUCKAIRON_2.ChannelIndex.Value = 1;
                Outputs.DOCHUCKAIRON_2.PortIndex.Value = 2;
                Outputs.DOCHUCKAIRON_2.Reverse.Value = false;
                Outputs.DOCHUCKAIRON_2.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCHUCKAIRON_2.MaintainTime.Value = 50;
                Outputs.DOCHUCKAIRON_2.TimeOut.Value = 10000;

                //==> Group Prober Card Change
                Outputs.DOUPMODULE_DOWN.ChannelIndex.Value = 1;
                Outputs.DOUPMODULE_DOWN.PortIndex.Value = 11;
                Outputs.DOUPMODULE_DOWN.Reverse.Value = false;
                Outputs.DOUPMODULE_DOWN.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOUPMODULE_DOWN.MaintainTime.Value = 0;
                Outputs.DOUPMODULE_DOWN.TimeOut.Value = 0;

                Outputs.DOUPMODULE_UP.ChannelIndex.Value = 1;
                Outputs.DOUPMODULE_UP.PortIndex.Value = 10;
                Outputs.DOUPMODULE_UP.Reverse.Value = false;
                Outputs.DOUPMODULE_UP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOUPMODULE_UP.MaintainTime.Value = 0;
                Outputs.DOUPMODULE_UP.TimeOut.Value = 0;

                Outputs.DOUPMODULE_VACU.ChannelIndex.Value = 1;
                Outputs.DOUPMODULE_VACU.PortIndex.Value = 5;
                Outputs.DOUPMODULE_VACU.Reverse.Value = false;
                Outputs.DOUPMODULE_VACU.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOUPMODULE_VACU.MaintainTime.Value = 0;
                Outputs.DOUPMODULE_VACU.TimeOut.Value = 0;

                Outputs.DOTPLATE_PCLATCH_LOCK.ChannelIndex.Value = 2;
                Outputs.DOTPLATE_PCLATCH_LOCK.PortIndex.Value = 0;
                Outputs.DOTPLATE_PCLATCH_LOCK.Reverse.Value = false;
                Outputs.DOTPLATE_PCLATCH_LOCK.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOTPLATE_PCLATCH_LOCK.MaintainTime.Value = 0;
                Outputs.DOTPLATE_PCLATCH_LOCK.TimeOut.Value = 0;

                Outputs.DOPOGOCARD_VACU_RELEASE.ChannelIndex.Value = 2;
                Outputs.DOPOGOCARD_VACU_RELEASE.PortIndex.Value = 1;
                Outputs.DOPOGOCARD_VACU_RELEASE.Reverse.Value = false;
                Outputs.DOPOGOCARD_VACU_RELEASE.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPOGOCARD_VACU_RELEASE.MaintainTime.Value = 0;
                Outputs.DOPOGOCARD_VACU_RELEASE.TimeOut.Value = 0;

                Outputs.DOPOGOCARD_VACU.ChannelIndex.Value = 2;
                Outputs.DOPOGOCARD_VACU.PortIndex.Value = 2;
                Outputs.DOPOGOCARD_VACU.Reverse.Value = false;
                Outputs.DOPOGOCARD_VACU.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPOGOCARD_VACU.MaintainTime.Value = 0;
                Outputs.DOPOGOCARD_VACU.TimeOut.Value = 0;

                Outputs.DOPOGOTESTER_VACU_RELEASE.ChannelIndex.Value = 2;
                Outputs.DOPOGOTESTER_VACU_RELEASE.PortIndex.Value = 3;
                Outputs.DOPOGOTESTER_VACU_RELEASE.Reverse.Value = false;
                Outputs.DOPOGOTESTER_VACU_RELEASE.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPOGOTESTER_VACU_RELEASE.MaintainTime.Value = 0;
                Outputs.DOPOGOTESTER_VACU_RELEASE.TimeOut.Value = 0;

                Outputs.DOPOGOTESTER_VACU.ChannelIndex.Value = 2;
                Outputs.DOPOGOTESTER_VACU.PortIndex.Value = 4;
                Outputs.DOPOGOTESTER_VACU.Reverse.Value = false;
                Outputs.DOPOGOTESTER_VACU.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPOGOTESTER_VACU.MaintainTime.Value = 0;
                Outputs.DOPOGOTESTER_VACU.TimeOut.Value = 0;

                //loader
                Inputs.DIWAFERONARM.ChannelIndex.Value = 3;
                Inputs.DIWAFERONARM.PortIndex.Value = 1;
                Inputs.DIWAFERONARM.Reverse.Value = false;
                Inputs.DIWAFERONARM.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONARM.MaintainTime.Value = 50;
                Inputs.DIWAFERONARM.TimeOut.Value = 60000;

                Inputs.DIWAFERONARM2.ChannelIndex.Value = 3;
                Inputs.DIWAFERONARM2.PortIndex.Value = 2;
                Inputs.DIWAFERONARM2.Reverse.Value = false;
                Inputs.DIWAFERONARM2.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONARM2.MaintainTime.Value = 50;
                Inputs.DIWAFERONARM2.TimeOut.Value = 60000;

                Inputs.DIMAINAIR.ChannelIndex.Value = 3;
                Inputs.DIMAINAIR.PortIndex.Value = 4;
                Inputs.DIMAINAIR.Reverse.Value = false;
                Inputs.DIMAINAIR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIMAINAIR.MaintainTime.Value = 50;
                Inputs.DIMAINAIR.TimeOut.Value = 60000;

                Inputs.DIMAINVAC.ChannelIndex.Value = 3;
                Inputs.DIMAINVAC.PortIndex.Value = 5;
                Inputs.DIMAINVAC.Reverse.Value = false;
                Inputs.DIMAINVAC.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIMAINVAC.MaintainTime.Value = 50;
                Inputs.DIMAINVAC.TimeOut.Value = 60000;

                Inputs.DIDOORCLOSE.ChannelIndex.Value = 4;
                Inputs.DIDOORCLOSE.PortIndex.Value = 0;
                Inputs.DIDOORCLOSE.Reverse.Value = false;
                Inputs.DIDOORCLOSE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIDOORCLOSE.MaintainTime.Value = 50;
                Inputs.DIDOORCLOSE.TimeOut.Value = 60000;

                Inputs.DIWAFERSENSOR.ChannelIndex.Value = 4;
                Inputs.DIWAFERSENSOR.PortIndex.Value = 1;
                Inputs.DIWAFERSENSOR.Reverse.Value = false;
                Inputs.DIWAFERSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERSENSOR.MaintainTime.Value = 50;
                Inputs.DIWAFERSENSOR.TimeOut.Value = 60000;

                Inputs.DI8PRS1.ChannelIndex.Value = 4;
                Inputs.DI8PRS1.PortIndex.Value = 2;
                Inputs.DI8PRS1.Reverse.Value = false;
                Inputs.DI8PRS1.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS1.MaintainTime.Value = 50;
                Inputs.DI8PRS1.TimeOut.Value = 60000;

                Inputs.DI8PRS2.ChannelIndex.Value = 4;
                Inputs.DI8PRS2.PortIndex.Value = 3;
                Inputs.DI8PRS2.Reverse.Value = false;
                Inputs.DI8PRS2.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS2.MaintainTime.Value = 50;
                Inputs.DI8PRS2.TimeOut.Value = 60000;

                Inputs.DI8PRS3.ChannelIndex.Value = 4;
                Inputs.DI8PRS3.PortIndex.Value = 4;
                Inputs.DI8PRS3.Reverse.Value = false;
                Inputs.DI8PRS3.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS3.MaintainTime.Value = 50;
                Inputs.DI8PRS3.TimeOut.Value = 60000;

                Inputs.DI8PLA.ChannelIndex.Value = 4;
                Inputs.DI8PLA.PortIndex.Value = 5;
                Inputs.DI8PLA.Reverse.Value = false;
                Inputs.DI8PLA.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PLA.MaintainTime.Value = 50;
                Inputs.DI8PLA.TimeOut.Value = 60000;

                Inputs.DI6PLA.ChannelIndex.Value = 4;
                Inputs.DI6PLA.PortIndex.Value = 6;
                Inputs.DI6PLA.Reverse.Value = false;
                Inputs.DI6PLA.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI6PLA.MaintainTime.Value = 50;
                Inputs.DI6PLA.TimeOut.Value = 60000;

                Inputs.DIFOUPSWINGSENSOR.ChannelIndex.Value = 4;
                Inputs.DIFOUPSWINGSENSOR.PortIndex.Value = 7;
                Inputs.DIFOUPSWINGSENSOR.Reverse.Value = false;
                Inputs.DIFOUPSWINGSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFOUPSWINGSENSOR.MaintainTime.Value = 50;
                Inputs.DIFOUPSWINGSENSOR.TimeOut.Value = 60000;

                Inputs.DIWAFERDETECTSENSOR.ChannelIndex.Value = 4;
                Inputs.DIWAFERDETECTSENSOR.PortIndex.Value = 8;
                Inputs.DIWAFERDETECTSENSOR.Reverse.Value = false;
                Inputs.DIWAFERDETECTSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERDETECTSENSOR.MaintainTime.Value = 50;
                Inputs.DIWAFERDETECTSENSOR.TimeOut.Value = 60000;

                Inputs.DISCANMAPPINGSENSOR.ChannelIndex.Value = 4;
                Inputs.DISCANMAPPINGSENSOR.PortIndex.Value = 10;
                Inputs.DISCANMAPPINGSENSOR.Reverse.Value = false;
                Inputs.DISCANMAPPINGSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DISCANMAPPINGSENSOR.MaintainTime.Value = 50;
                Inputs.DISCANMAPPINGSENSOR.TimeOut.Value = 60000;

                Inputs.DITRAYDETECTSENSOR.ChannelIndex.Value = 4;
                Inputs.DITRAYDETECTSENSOR.PortIndex.Value = 11;
                Inputs.DITRAYDETECTSENSOR.Reverse.Value = false;
                Inputs.DITRAYDETECTSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITRAYDETECTSENSOR.MaintainTime.Value = 50;
                Inputs.DITRAYDETECTSENSOR.TimeOut.Value = 60000;

                Inputs.DIWAFERONSUBCHUCK.ChannelIndex.Value = 3;
                Inputs.DIWAFERONSUBCHUCK.PortIndex.Value = 12;
                Inputs.DIWAFERONSUBCHUCK.Reverse.Value = false;
                Inputs.DIWAFERONSUBCHUCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONSUBCHUCK.MaintainTime.Value = 50;
                Inputs.DIWAFERONSUBCHUCK.TimeOut.Value = 60000;


                Outputs.DOOCR1LIGHT.ChannelIndex.Value = 4;
                Outputs.DOOCR1LIGHT.PortIndex.Value = 0;
                Outputs.DOOCR1LIGHT.Reverse.Value = false;
                Outputs.DOOCR1LIGHT.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOOCR1LIGHT.MaintainTime.Value = 50;
                Outputs.DOOCR1LIGHT.TimeOut.Value = 60000;

                Outputs.DOOCR2LIGHT.ChannelIndex.Value = 4;
                Outputs.DOOCR2LIGHT.PortIndex.Value = 1;
                Outputs.DOOCR2LIGHT.Reverse.Value = false;
                Outputs.DOOCR2LIGHT.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOOCR2LIGHT.MaintainTime.Value = 50;
                Outputs.DOOCR2LIGHT.TimeOut.Value = 60000;

                Outputs.DOOCR3LIGHT.ChannelIndex.Value = 4;
                Outputs.DOOCR3LIGHT.PortIndex.Value = 2;
                Outputs.DOOCR3LIGHT.Reverse.Value = false;
                Outputs.DOOCR3LIGHT.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOOCR3LIGHT.MaintainTime.Value = 50;
                Outputs.DOOCR3LIGHT.TimeOut.Value = 60000;

                Outputs.DOPACL.ChannelIndex.Value = 4;
                Outputs.DOPACL.PortIndex.Value = 3;
                Outputs.DOPACL.Reverse.Value = false;
                Outputs.DOPACL.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPACL.MaintainTime.Value = 50;
                Outputs.DOPACL.TimeOut.Value = 60000;

                Outputs.DOREDLAMP.ChannelIndex.Value = 5;
                Outputs.DOREDLAMP.PortIndex.Value = 0;
                Outputs.DOREDLAMP.Reverse.Value = false;
                Outputs.DOREDLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOREDLAMP.MaintainTime.Value = 50;
                Outputs.DOREDLAMP.TimeOut.Value = 60000;

                Outputs.DOGREENLAMP.ChannelIndex.Value = 5;
                Outputs.DOGREENLAMP.PortIndex.Value = 1;
                Outputs.DOGREENLAMP.Reverse.Value = false;
                Outputs.DOGREENLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOGREENLAMP.MaintainTime.Value = 50;
                Outputs.DOGREENLAMP.TimeOut.Value = 60000;

                Outputs.DOYELLOWLAMP.ChannelIndex.Value = 5;
                Outputs.DOYELLOWLAMP.PortIndex.Value = 2;
                Outputs.DOYELLOWLAMP.Reverse.Value = false;
                Outputs.DOYELLOWLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOYELLOWLAMP.MaintainTime.Value = 50;
                Outputs.DOYELLOWLAMP.TimeOut.Value = 60000;


                Outputs.DOREDLAMPON.ChannelIndex.Value = 5;
                Outputs.DOREDLAMPON.PortIndex.Value = 0;
                Outputs.DOREDLAMPON.Reverse.Value = false;
                Outputs.DOREDLAMPON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOREDLAMPON.MaintainTime.Value = 50;
                Outputs.DOREDLAMPON.TimeOut.Value = 60000;

                Outputs.DOBLUELAMPON.ChannelIndex.Value = 5;
                Outputs.DOBLUELAMPON.PortIndex.Value = 1;
                Outputs.DOBLUELAMPON.Reverse.Value = false;
                Outputs.DOBLUELAMPON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOBLUELAMPON.MaintainTime.Value = 50;
                Outputs.DOBLUELAMPON.TimeOut.Value = 60000;

                Outputs.DOYELLOWLAMPON.ChannelIndex.Value = 5;
                Outputs.DOYELLOWLAMPON.PortIndex.Value = 2;
                Outputs.DOYELLOWLAMPON.Reverse.Value = false;
                Outputs.DOYELLOWLAMPON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOYELLOWLAMPON.MaintainTime.Value = 50;
                Outputs.DOYELLOWLAMPON.TimeOut.Value = 60000;


                Outputs.DOCASSETTELOCK.ChannelIndex.Value = 6;
                Outputs.DOCASSETTELOCK.PortIndex.Value = 0;
                Outputs.DOCASSETTELOCK.Reverse.Value = false;
                Outputs.DOCASSETTELOCK.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCASSETTELOCK.MaintainTime.Value = 50;
                Outputs.DOCASSETTELOCK.TimeOut.Value = 60000;

                Outputs.DOFOUPSWING.ChannelIndex.Value = 6;
                Outputs.DOFOUPSWING.PortIndex.Value = 1;
                Outputs.DOFOUPSWING.Reverse.Value = false;
                Outputs.DOFOUPSWING.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOFOUPSWING.MaintainTime.Value = 50;
                Outputs.DOFOUPSWING.TimeOut.Value = 60000;


                Outputs.DOSUBCHUCKAIRON.ChannelIndex.Value = 6;
                Outputs.DOSUBCHUCKAIRON.PortIndex.Value = 2;
                Outputs.DOSUBCHUCKAIRON.Reverse.Value = false;
                Outputs.DOSUBCHUCKAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOSUBCHUCKAIRON.MaintainTime.Value = 50;
                Outputs.DOSUBCHUCKAIRON.TimeOut.Value = 60000;

                Outputs.DOARMAIRON.ChannelIndex.Value = 6;
                Outputs.DOARMAIRON.PortIndex.Value = 5;
                Outputs.DOARMAIRON.Reverse.Value = false;
                Outputs.DOARMAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOARMAIRON.MaintainTime.Value = 50;
                Outputs.DOARMAIRON.TimeOut.Value = 60000;

                Outputs.DOARM2AIRON.ChannelIndex.Value = 6;
                Outputs.DOARM2AIRON.PortIndex.Value = 6;
                Outputs.DOARM2AIRON.Reverse.Value = false;
                Outputs.DOARM2AIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOARM2AIRON.MaintainTime.Value = 50;
                Outputs.DOARM2AIRON.TimeOut.Value = 60000;

                // 251017 sebas : 실제 장비 연결하면 위 내용도 바꺼야할듯
                Inputs.DI_ARM_BLOW1.ChannelIndex.Value = 4;
                Inputs.DI_ARM_BLOW1.PortIndex.Value = 10;
                Inputs.DI_ARM_BLOW1.Reverse.Value = false;
                Inputs.DI_ARM_BLOW1.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI_ARM_BLOW1.MaintainTime.Value = 50;
                Inputs.DI_ARM_BLOW1.TimeOut.Value = 60000;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultParam_BSCI1()
        {
            try
            {
                //stage
                Inputs.DIWAFERCAMMIDDLE.ChannelIndex.Value = 2;
                Inputs.DIWAFERCAMMIDDLE.PortIndex.Value = 12;
                Inputs.DIWAFERCAMMIDDLE.Reverse.Value = false;
                Inputs.DIWAFERCAMMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERCAMMIDDLE.MaintainTime.Value = 50;
                Inputs.DIWAFERCAMMIDDLE.TimeOut.Value = 10000;

                Inputs.DIWAFERCAMREAR.ChannelIndex.Value = 2;
                Inputs.DIWAFERCAMREAR.PortIndex.Value = 11;
                Inputs.DIWAFERCAMREAR.Reverse.Value = false;
                Inputs.DIWAFERCAMREAR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERCAMREAR.MaintainTime.Value = 50;
                Inputs.DIWAFERCAMREAR.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK.ChannelIndex.Value = 6;
                Inputs.DIWAFERONCHUCK.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK_6.ChannelIndex.Value = 6;
                Inputs.DIWAFERONCHUCK_6.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK_6.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK_6.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK_6.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK_6.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK_8.ChannelIndex.Value = 6;
                Inputs.DIWAFERONCHUCK_8.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK_8.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK_8.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK_8.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK_8.TimeOut.Value = 10000;

                Inputs.DIWAFERONCHUCK_12.ChannelIndex.Value = 6;
                Inputs.DIWAFERONCHUCK_12.PortIndex.Value = 1;
                Inputs.DIWAFERONCHUCK_12.Reverse.Value = false;
                Inputs.DIWAFERONCHUCK_12.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONCHUCK_12.MaintainTime.Value = 50;
                Inputs.DIWAFERONCHUCK_12.TimeOut.Value = 10000;

                Inputs.DINC_SENSOR.ChannelIndex.Value = 2;
                Inputs.DINC_SENSOR.PortIndex.Value = 10;
                Inputs.DINC_SENSOR.Reverse.Value = false;
                Inputs.DINC_SENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DINC_SENSOR.MaintainTime.Value = 50;
                Inputs.DINC_SENSOR.TimeOut.Value = 10000;

                Inputs.DINCPAD_VAC.ChannelIndex.Value = 6;
                Inputs.DINCPAD_VAC.PortIndex.Value = 2;
                Inputs.DINCPAD_VAC.Reverse.Value = false;
                Inputs.DINCPAD_VAC.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DINCPAD_VAC.MaintainTime.Value = 50;
                Inputs.DINCPAD_VAC.TimeOut.Value = 10000;

                Inputs.DIFRONTDOOROPEN.ChannelIndex.Value = 1;
                Inputs.DIFRONTDOOROPEN.PortIndex.Value = 10;
                Inputs.DIFRONTDOOROPEN.Reverse.Value = false;
                Inputs.DIFRONTDOOROPEN.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFRONTDOOROPEN.MaintainTime.Value = 50;
                Inputs.DIFRONTDOOROPEN.TimeOut.Value = 10000;

                Inputs.DIFRONTDOORCLOSE.ChannelIndex.Value = 1;
                Inputs.DIFRONTDOORCLOSE.PortIndex.Value = 11;
                Inputs.DIFRONTDOORCLOSE.Reverse.Value = false;
                Inputs.DIFRONTDOORCLOSE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFRONTDOORCLOSE.MaintainTime.Value = 50;
                Inputs.DIFRONTDOORCLOSE.TimeOut.Value = 10000;

                Inputs.DIFRONTDOOR_LOCK_R.ChannelIndex.Value = 1;
                Inputs.DIFRONTDOOR_LOCK_R.PortIndex.Value = 0;
                Inputs.DIFRONTDOOR_LOCK_R.Reverse.Value = false;
                Inputs.DIFRONTDOOR_LOCK_R.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFRONTDOOR_LOCK_R.MaintainTime.Value = 50;
                Inputs.DIFRONTDOOR_LOCK_R.TimeOut.Value = 10000;

                Inputs.DIFRONTDOOR_UNLOCK_R.ChannelIndex.Value = 1;
                Inputs.DIFRONTDOOR_UNLOCK_R.PortIndex.Value = 1;
                Inputs.DIFRONTDOOR_UNLOCK_R.Reverse.Value = false;
                Inputs.DIFRONTDOOR_UNLOCK_R.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFRONTDOOR_UNLOCK_R.MaintainTime.Value = 50;
                Inputs.DIFRONTDOOR_UNLOCK_R.TimeOut.Value = 10000;

                Inputs.DIFRONTDOOR_LOCK_L.ChannelIndex.Value = 1;
                Inputs.DIFRONTDOOR_LOCK_L.PortIndex.Value = 8;
                Inputs.DIFRONTDOOR_LOCK_L.Reverse.Value = false;
                Inputs.DIFRONTDOOR_LOCK_L.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFRONTDOOR_LOCK_L.MaintainTime.Value = 50;
                Inputs.DIFRONTDOOR_LOCK_L.TimeOut.Value = 10000;

                Inputs.DIFRONTDOOR_UNLOCK_L.ChannelIndex.Value = 1;
                Inputs.DIFRONTDOOR_UNLOCK_L.PortIndex.Value = 9;
                Inputs.DIFRONTDOOR_UNLOCK_L.Reverse.Value = false;
                Inputs.DIFRONTDOOR_UNLOCK_L.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFRONTDOOR_UNLOCK_L.MaintainTime.Value = 50;
                Inputs.DIFRONTDOOR_UNLOCK_L.TimeOut.Value = 10000;

                Inputs.DITOPPLATE_OPEN.ChannelIndex.Value = 1;
                Inputs.DITOPPLATE_OPEN.PortIndex.Value = 12;
                Inputs.DITOPPLATE_OPEN.Reverse.Value = false;
                Inputs.DITOPPLATE_OPEN.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITOPPLATE_OPEN.MaintainTime.Value = 50;
                Inputs.DITOPPLATE_OPEN.TimeOut.Value = 10000;

                Inputs.DITOPPLATE_CLOSE.ChannelIndex.Value = 1;
                Inputs.DITOPPLATE_CLOSE.PortIndex.Value = 13;
                Inputs.DITOPPLATE_CLOSE.Reverse.Value = false;
                Inputs.DITOPPLATE_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITOPPLATE_CLOSE.MaintainTime.Value = 50;
                Inputs.DITOPPLATE_CLOSE.TimeOut.Value = 10000;

                Inputs.DILOADERDOOR_OPEN.ChannelIndex.Value = 2;
                Inputs.DILOADERDOOR_OPEN.PortIndex.Value = 13;
                Inputs.DILOADERDOOR_OPEN.Reverse.Value = false;
                Inputs.DILOADERDOOR_OPEN.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DILOADERDOOR_OPEN.MaintainTime.Value = 50;
                Inputs.DILOADERDOOR_OPEN.TimeOut.Value = 10000;

                Inputs.DILOADERDOOR_CLOSE.ChannelIndex.Value = 2;
                Inputs.DILOADERDOOR_CLOSE.PortIndex.Value = 14;
                Inputs.DILOADERDOOR_CLOSE.Reverse.Value = false;
                Inputs.DILOADERDOOR_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DILOADERDOOR_CLOSE.MaintainTime.Value = 50;
                Inputs.DILOADERDOOR_CLOSE.TimeOut.Value = 10000;

                Outputs.DOWAFERMIDDLE.ChannelIndex.Value = 1;
                Outputs.DOWAFERMIDDLE.PortIndex.Value = 8;
                Outputs.DOWAFERMIDDLE.Reverse.Value = false;
                Outputs.DOWAFERMIDDLE.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOWAFERMIDDLE.MaintainTime.Value = 50;
                Outputs.DOWAFERMIDDLE.TimeOut.Value = 10000;


                Outputs.DOWAFERREAR.ChannelIndex.Value = 1;
                Outputs.DOWAFERREAR.PortIndex.Value = 9;
                Outputs.DOWAFERREAR.Reverse.Value = false;
                Outputs.DOWAFERREAR.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOWAFERREAR.MaintainTime.Value = 50;
                Outputs.DOWAFERREAR.TimeOut.Value = 10000;


                Outputs.DOCHUCKAIRON_0.ChannelIndex.Value = 2;
                Outputs.DOCHUCKAIRON_0.PortIndex.Value = 8;
                Outputs.DOCHUCKAIRON_0.Reverse.Value = false;
                Outputs.DOCHUCKAIRON_0.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCHUCKAIRON_0.MaintainTime.Value = 50;
                Outputs.DOCHUCKAIRON_0.TimeOut.Value = 10000;

                Outputs.DOCHUCKAIRON_1.ChannelIndex.Value = 2;
                Outputs.DOCHUCKAIRON_1.PortIndex.Value = 9;
                Outputs.DOCHUCKAIRON_1.Reverse.Value = false;
                Outputs.DOCHUCKAIRON_1.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCHUCKAIRON_1.MaintainTime.Value = 50;
                Outputs.DOCHUCKAIRON_1.TimeOut.Value = 10000;

                Outputs.DOCHUCKAIRON_2.ChannelIndex.Value = 2;
                Outputs.DOCHUCKAIRON_2.PortIndex.Value = 10;
                Outputs.DOCHUCKAIRON_2.Reverse.Value = false;
                Outputs.DOCHUCKAIRON_2.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCHUCKAIRON_2.MaintainTime.Value = 50;
                Outputs.DOCHUCKAIRON_2.TimeOut.Value = 10000;

                Outputs.DONEEDLECLEANAIRON.ChannelIndex.Value = 1;
                Outputs.DONEEDLECLEANAIRON.PortIndex.Value = 14;
                Outputs.DONEEDLECLEANAIRON.Reverse.Value = false;
                Outputs.DONEEDLECLEANAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DONEEDLECLEANAIRON.MaintainTime.Value = 50;
                Outputs.DONEEDLECLEANAIRON.TimeOut.Value = 10000;

                Outputs.DOTHREEPOD_COOLING_ON.ChannelIndex.Value = 2;
                Outputs.DOTHREEPOD_COOLING_ON.PortIndex.Value = 12;
                Outputs.DOTHREEPOD_COOLING_ON.Reverse.Value = false;
                Outputs.DOTHREEPOD_COOLING_ON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOTHREEPOD_COOLING_ON.MaintainTime.Value = 50;
                Outputs.DOTHREEPOD_COOLING_ON.TimeOut.Value = 10000;

                Outputs.DOFDOOR_LOCK.ChannelIndex.Value = 1;
                Outputs.DOFDOOR_LOCK.PortIndex.Value = 11;
                Outputs.DOFDOOR_LOCK.Reverse.Value = false;
                Outputs.DOFDOOR_LOCK.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOFDOOR_LOCK.MaintainTime.Value = 50;
                Outputs.DOFDOOR_LOCK.TimeOut.Value = 10000;

                Outputs.DOFDOOR_UNLOCK.ChannelIndex.Value = 1;
                Outputs.DOFDOOR_UNLOCK.PortIndex.Value = 10;
                Outputs.DOFDOOR_UNLOCK.Reverse.Value = false;
                Outputs.DOFDOOR_UNLOCK.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOFDOOR_UNLOCK.MaintainTime.Value = 50;
                Outputs.DOFDOOR_UNLOCK.TimeOut.Value = 10000;

                Outputs.DOLOADERDOOR_CLOSE.ChannelIndex.Value = 1;
                Outputs.DOLOADERDOOR_CLOSE.PortIndex.Value = 12;
                Outputs.DOLOADERDOOR_CLOSE.Reverse.Value = false;
                Outputs.DOLOADERDOOR_CLOSE.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOLOADERDOOR_CLOSE.MaintainTime.Value = 50;
                Outputs.DOLOADERDOOR_CLOSE.TimeOut.Value = 10000;

                Outputs.DOLOADERDOOR_OPEN.ChannelIndex.Value = 1;
                Outputs.DOLOADERDOOR_OPEN.PortIndex.Value = 13;
                Outputs.DOLOADERDOOR_OPEN.Reverse.Value = false;
                Outputs.DOLOADERDOOR_OPEN.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOLOADERDOOR_OPEN.MaintainTime.Value = 50;
                Outputs.DOLOADERDOOR_OPEN.TimeOut.Value = 10000;


                // loader

                Inputs.DIWAFERONARM.ChannelIndex.Value = 3;
                Inputs.DIWAFERONARM.PortIndex.Value = 1;
                Inputs.DIWAFERONARM.Reverse.Value = false;
                Inputs.DIWAFERONARM.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONARM.MaintainTime.Value = 50;
                Inputs.DIWAFERONARM.TimeOut.Value = 10000;

                Inputs.DIWAFERONARM2.ChannelIndex.Value = 3;
                Inputs.DIWAFERONARM2.PortIndex.Value = 2;
                Inputs.DIWAFERONARM2.Reverse.Value = false;
                Inputs.DIWAFERONARM2.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONARM2.MaintainTime.Value = 50;
                Inputs.DIWAFERONARM2.TimeOut.Value = 60000;


                Inputs.DIMAINAIR.ChannelIndex.Value = 3;
                Inputs.DIMAINAIR.PortIndex.Value = 5;
                Inputs.DIMAINAIR.Reverse.Value = false;
                Inputs.DIMAINAIR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIMAINAIR.MaintainTime.Value = 50;
                Inputs.DIMAINAIR.TimeOut.Value = 60000;

                Inputs.DIMAINVAC.ChannelIndex.Value = 3;
                Inputs.DIMAINVAC.PortIndex.Value = 4;
                Inputs.DIMAINVAC.Reverse.Value = false;
                Inputs.DIMAINVAC.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIMAINVAC.MaintainTime.Value = 50;
                Inputs.DIMAINVAC.TimeOut.Value = 60000;

                Inputs.DIDOORCLOSE.ChannelIndex.Value = -1;
                Inputs.DIDOORCLOSE.PortIndex.Value = -1;
                Inputs.DIDOORCLOSE.Reverse.Value = false;
                Inputs.DIDOORCLOSE.IOOveride.Value = EnumIOOverride.NLO;
                Inputs.DIDOORCLOSE.MaintainTime.Value = 50;
                Inputs.DIDOORCLOSE.TimeOut.Value = 60000;

                Inputs.DIWAFERSENSOR.ChannelIndex.Value = 4;
                Inputs.DIWAFERSENSOR.PortIndex.Value = 1;
                Inputs.DIWAFERSENSOR.Reverse.Value = false;
                Inputs.DIWAFERSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERSENSOR.MaintainTime.Value = 50;
                Inputs.DIWAFERSENSOR.TimeOut.Value = 60000;

                Inputs.DI8PRS1.ChannelIndex.Value = 4;
                Inputs.DI8PRS1.PortIndex.Value = 2;
                Inputs.DI8PRS1.Reverse.Value = false;
                Inputs.DI8PRS1.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS1.MaintainTime.Value = 50;
                Inputs.DI8PRS1.TimeOut.Value = 60000;

                Inputs.DI8PRS2.ChannelIndex.Value = 4;
                Inputs.DI8PRS2.PortIndex.Value = 3;
                Inputs.DI8PRS2.Reverse.Value = false;
                Inputs.DI8PRS2.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS2.MaintainTime.Value = 50;
                Inputs.DI8PRS2.TimeOut.Value = 60000;

                Inputs.DI8PRS3.ChannelIndex.Value = 4;
                Inputs.DI8PRS3.PortIndex.Value = 4;
                Inputs.DI8PRS3.Reverse.Value = false;
                Inputs.DI8PRS3.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PRS3.MaintainTime.Value = 50;
                Inputs.DI8PRS3.TimeOut.Value = 60000;

                Inputs.DI8PLA.ChannelIndex.Value = 4;
                Inputs.DI8PLA.PortIndex.Value = 5;
                Inputs.DI8PLA.Reverse.Value = false;
                Inputs.DI8PLA.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI8PLA.MaintainTime.Value = 50;
                Inputs.DI8PLA.TimeOut.Value = 60000;

                Inputs.DI6PLA.ChannelIndex.Value = 4;
                Inputs.DI6PLA.PortIndex.Value = 6;
                Inputs.DI6PLA.Reverse.Value = false;
                Inputs.DI6PLA.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DI6PLA.MaintainTime.Value = 50;
                Inputs.DI6PLA.TimeOut.Value = 60000;

                Inputs.DIFOUPSWINGSENSOR.ChannelIndex.Value = 4;
                Inputs.DIFOUPSWINGSENSOR.PortIndex.Value = 7;
                Inputs.DIFOUPSWINGSENSOR.Reverse.Value = false;
                Inputs.DIFOUPSWINGSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIFOUPSWINGSENSOR.MaintainTime.Value = 50;
                Inputs.DIFOUPSWINGSENSOR.TimeOut.Value = 60000;

                Inputs.DIWAFERDETECTSENSOR.ChannelIndex.Value = 4;
                Inputs.DIWAFERDETECTSENSOR.PortIndex.Value = 8;
                Inputs.DIWAFERDETECTSENSOR.Reverse.Value = false;
                Inputs.DIWAFERDETECTSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERDETECTSENSOR.MaintainTime.Value = 50;
                Inputs.DIWAFERDETECTSENSOR.TimeOut.Value = 60000;

                Inputs.DISCANMAPPINGSENSOR.ChannelIndex.Value = 4;
                Inputs.DISCANMAPPINGSENSOR.PortIndex.Value = 10;
                Inputs.DISCANMAPPINGSENSOR.Reverse.Value = false;
                Inputs.DISCANMAPPINGSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DISCANMAPPINGSENSOR.MaintainTime.Value = 50;
                Inputs.DISCANMAPPINGSENSOR.TimeOut.Value = 60000;

                Inputs.DITRAYDETECTSENSOR.ChannelIndex.Value = 4;
                Inputs.DITRAYDETECTSENSOR.PortIndex.Value = 11;
                Inputs.DITRAYDETECTSENSOR.Reverse.Value = false;
                Inputs.DITRAYDETECTSENSOR.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DITRAYDETECTSENSOR.MaintainTime.Value = 50;
                Inputs.DITRAYDETECTSENSOR.TimeOut.Value = 60000;

                Inputs.DIWAFERONSUBCHUCK.ChannelIndex.Value = 3;
                Inputs.DIWAFERONSUBCHUCK.PortIndex.Value = 12;
                Inputs.DIWAFERONSUBCHUCK.Reverse.Value = false;
                Inputs.DIWAFERONSUBCHUCK.IOOveride.Value = EnumIOOverride.NONE;
                Inputs.DIWAFERONSUBCHUCK.MaintainTime.Value = 50;
                Inputs.DIWAFERONSUBCHUCK.TimeOut.Value = 60000;


                Outputs.DOPACL.ChannelIndex.Value = 3;
                Outputs.DOPACL.PortIndex.Value = 0;
                Outputs.DOPACL.Reverse.Value = false;
                Outputs.DOPACL.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOPACL.MaintainTime.Value = 50;
                Outputs.DOPACL.TimeOut.Value = 60000;

                Outputs.DOREDLAMP.ChannelIndex.Value = 4;
                Outputs.DOREDLAMP.PortIndex.Value = 0;
                Outputs.DOREDLAMP.Reverse.Value = false;
                Outputs.DOREDLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOREDLAMP.MaintainTime.Value = 50;
                Outputs.DOREDLAMP.TimeOut.Value = 60000;

                Outputs.DOGREENLAMP.ChannelIndex.Value = 4;
                Outputs.DOGREENLAMP.PortIndex.Value = 1;
                Outputs.DOGREENLAMP.Reverse.Value = false;
                Outputs.DOGREENLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOGREENLAMP.MaintainTime.Value = 50;
                Outputs.DOGREENLAMP.TimeOut.Value = 60000;

                Outputs.DOYELLOWLAMP.ChannelIndex.Value = 4;
                Outputs.DOYELLOWLAMP.PortIndex.Value = 2;
                Outputs.DOYELLOWLAMP.Reverse.Value = false;
                Outputs.DOYELLOWLAMP.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOYELLOWLAMP.MaintainTime.Value = 50;
                Outputs.DOYELLOWLAMP.TimeOut.Value = 60000;

                Outputs.DOBUZZERON.ChannelIndex.Value = 4;
                Outputs.DOBUZZERON.PortIndex.Value = 3;
                Outputs.DOBUZZERON.Reverse.Value = false;
                Outputs.DOBUZZERON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOBUZZERON.MaintainTime.Value = 50;
                Outputs.DOBUZZERON.TimeOut.Value = 60000;


                Outputs.DOCASSETTELOCK.ChannelIndex.Value = 5;
                Outputs.DOCASSETTELOCK.PortIndex.Value = 0;
                Outputs.DOCASSETTELOCK.Reverse.Value = true;
                Outputs.DOCASSETTELOCK.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOCASSETTELOCK.MaintainTime.Value = 50;
                Outputs.DOCASSETTELOCK.TimeOut.Value = 60000;

                Outputs.DOFOUPSWING.ChannelIndex.Value = 5;
                Outputs.DOFOUPSWING.PortIndex.Value = 1;
                Outputs.DOFOUPSWING.Reverse.Value = false;
                Outputs.DOFOUPSWING.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOFOUPSWING.MaintainTime.Value = 50;
                Outputs.DOFOUPSWING.TimeOut.Value = 60000;

                Outputs.DOSUBCHUCKAIRON.ChannelIndex.Value = 5;
                Outputs.DOSUBCHUCKAIRON.PortIndex.Value = 2;
                Outputs.DOSUBCHUCKAIRON.Reverse.Value = false;
                Outputs.DOSUBCHUCKAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOSUBCHUCKAIRON.MaintainTime.Value = 50;
                Outputs.DOSUBCHUCKAIRON.TimeOut.Value = 60000;

                Outputs.DOARMAIRON.ChannelIndex.Value = 5;
                Outputs.DOARMAIRON.PortIndex.Value = 5;
                Outputs.DOARMAIRON.Reverse.Value = false;
                Outputs.DOARMAIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOARMAIRON.MaintainTime.Value = 50;
                Outputs.DOARMAIRON.TimeOut.Value = 60000;

                Outputs.DOARM2AIRON.ChannelIndex.Value = 5;
                Outputs.DOARM2AIRON.PortIndex.Value = 6;
                Outputs.DOARM2AIRON.Reverse.Value = false;
                Outputs.DOARM2AIRON.IOOveride.Value = EnumIOOverride.NONE;
                Outputs.DOARM2AIRON.MaintainTime.Value = 50;
                Outputs.DOARM2AIRON.TimeOut.Value = 60000;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public List<IOPortDescripter<bool>> GetInputPorts()
        {
            List<IOPortDescripter<bool>> ports = new List<IOPortDescripter<bool>>();
            try
            {
                IOPortDescripter<bool> port;
                PropertyInfo[] propertyInfos;
                object propObject;

                propertyInfos = Inputs.GetType().GetProperties();
                foreach (var item in propertyInfos)
                {
                    if (item.PropertyType == typeof(IOPortDescripter<bool>))
                    {
                        port = new IOPortDescripter<bool>();
                        propObject = item.GetValue(Inputs);
                        port = (IOPortDescripter<bool>)propObject;
                        //port.PropertyChanged += OnPortStateUpdated;
                        if (ioDict.ContainsKey(port.Key.Value) == false) ioDict.Add(port.Key.Value, port);
                        ports.Add(port);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ports;
        }
        public bool IsServiceAvailable()
        {
            bool retVal = false;

            if (ServiceCallBack != null)
            {
                retVal = true;
            }
            return retVal;
        }

        [JsonIgnore, XmlIgnore]
        private IIOMappingsCallback ServiceCallBack { get; set; }
        [JsonIgnore, XmlIgnore]
        private Dictionary<string, IOPortDescripter<bool>> ioDict = new Dictionary<string, IOPortDescripter<bool>>();

        private void Channel_Faulted(object sender, EventArgs e)
        {
            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
            if (ServiceCallBack != null)
            {
                if ((ServiceCallBack as ICommunicationObject).State == CommunicationState.Faulted || (ServiceCallBack as ICommunicationObject).State == CommunicationState.Closed)
                {
                    DeInitService();
                    LoggerManager.Debug($"Callback Channel faulted. Sender = {sender}");
                }
                else
                {
                    LoggerManager.Debug($"Ignore Callback Channel faulted. Sender = {sender}, Already Reconnected");
                }
            }
        }

        public void InitService()
        {
            try
            {
                ServiceCallBack = OperationContext.Current.GetCallbackChannel<IIOMappingsCallback>();
                OperationContext.Current.Channel.Faulted += Channel_Faulted;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[{this.GetType().Name}] InitService() - Failed IIOMappings Callback Channel Init (Exception : {err}");
            }
        }

        public void DeInitService()
        {
            try
            {
                ServiceCallBack = null;
                LoggerManager.Debug($"DeInit IIOMappingsCallback Channel.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public IOPortDescripter<bool> GetPortStatus(string key)
        {
            IOPortDescripter<bool> port;
            if (ioDict.TryGetValue(key, out port) == false)
            {
                LoggerManager.Debug($"IO Port does not exist in port dictionary. Key = {key}");
            }
            return port;
        }

        public List<IOPortDescripter<bool>> GetOutputPorts()
        {
            List<IOPortDescripter<bool>> ports = new List<IOPortDescripter<bool>>();
            IOPortDescripter<bool> port;
            PropertyInfo[] propertyInfos;
            object propObject;

            propertyInfos = Outputs.GetType().GetProperties();
            foreach (var item in propertyInfos)
            {
                if (item.PropertyType == typeof(IOPortDescripter<bool>))
                {
                    port = new IOPortDescripter<bool>();
                    propObject = item.GetValue(Outputs);
                    port = (IOPortDescripter<bool>)propObject;
                    //port.PropertyChanged += OnPortStateUpdated;
                    if (ioDict.ContainsKey(port.Key.Value) == false) ioDict.Add(port.Key.Value, port);
                    ports.Add(port);
                }
            }
            return ports;
        }

        public IOPortDescripter<bool> GetIOPort(IOPortDescripter<bool> ioport)
        {
            IOPortDescripter<bool> retval = null;

            try
            {
                object target = null;

                if (ioport.IOType.Value == EnumIOType.INPUT)
                {
                    target = Inputs;
                }
                else if (ioport.IOType.Value == EnumIOType.OUTPUT)
                {
                    target = Outputs;
                }

                if(target != null)
                {
                    var propertyInfos = target.GetType().GetProperties();

                    foreach (var item in propertyInfos)
                    {
                        if (item.PropertyType == typeof(IOPortDescripter<bool>))
                        {
                            var propObject = item.GetValue(Inputs) as IOPortDescripter<bool>;

                            if (propObject.ChannelIndex.Value == ioport.ChannelIndex.Value &&
                                propObject.PortIndex.Value == ioport.PortIndex.Value &&
                                propObject.Key.Value == ioport.Key.Value &&
                                propObject.Description.Value == ioport.Description.Value)
                            {
                                retval = propObject;
                            }
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

        public void SetForcedIO(IOPortDescripter<bool> ioport, bool IsForced, bool ForecedValue)
        {
            try
            {
                var port = GetIOPort(ioport);

                if(port != null)
                {
                    port.ForcedIO.ForecedValue = ForecedValue;
                    port.ForcedIO.IsForced = IsForced;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void OnPortStateUpdated(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is IOPortDescripter<bool>)
                {
                    IOPortDescripter<bool> iOPort = (IOPortDescripter<bool>)sender;

                    ServiceCallBack.OnPortStateUpdated(iOPort);
                }

            }
            catch (Exception err)
            {

                LoggerManager.Error($"OnPortStateUpdated(): Error occurred. Err = {err.Message}");

            }
        }
        public void SetPortStateAs(IOPortDescripter<bool> port, bool value)
        {
            IOPortDescripter<bool> iOPort;
            if (ioDict.TryGetValue(port.Key.Value, out iOPort) == true)
            {
                if (value == true) iOPort.SetValue();
                else iOPort.ResetValue();
            }
        }

    }
}
