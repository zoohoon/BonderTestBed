using System;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;
using WaferDisappearControl;

namespace LoaderCore
{
    [Serializable]
    internal class CardARMModule : AttachedModuleBase, ICardARMModule
    {
        private IOPortDescripter<bool> DOAIRON;
        private IOPortDescripter<bool> DOAIROFF;
        private IOPortDescripter<bool> DICARD_ON_MODULE;
        private IOPortDescripter<bool> DIAIR_ON_MODULE;
        private bool _WriteVacuum = false;
        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.CARDARM;

        public CardARMDefinition Definition { get; set; }

        public CardARMDevice Device { get; set; }

        public CardHolder Holder { get; set; }

        public bool Enable { get; set; }
        public EventCodeEnum SetDefinition(CardARMDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                DOAIRON = Loader.IOManager.GetIOPortDescripter(Definition.DOAIRON.Value);
              

                if (Definition.DICARDEXIST.Value != null)
                {
                    DICARD_ON_MODULE = Loader.IOManager.GetIOPortDescripter(Definition.DICARDEXIST.Value);
                   // DICARD_ON_MODULE.PropertyChanged += DICARD_ON_MODULE_PropertyChanged;
                }
                if (Definition.DIWAFERONMODULE.Value != null)
                {
                    DIAIR_ON_MODULE = Loader.IOManager.GetIOPortDescripter(Definition.DIWAFERONMODULE.Value);
                   // DIAIR_ON_MODULE.PropertyChanged += DIAIR_ON_MODULE_PropertyChanged;
                }

                if (Definition.DOAIROFF.Value != null)
                {
                    _WriteVacuum = true;
                    DOAIROFF = Loader.IOManager.GetIOPortDescripter(Definition.DOAIROFF.Value);
                }

                Holder = new CardHolder();
                Holder.SetOwner(this);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void DIAIR_ON_MODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (DIAIR_ON_MODULE.Value == false)
                {
                }
                else if (sender == DIAIR_ON_MODULE && e.PropertyName == "Value")
                {
                    RecoveryCardStatus();
                }
                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void DICARD_ON_MODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (DICARD_ON_MODULE.Value == false)
                {
                }
                else if (sender == DICARD_ON_MODULE && e.PropertyName == "Value")
                {
                    RecoveryCardStatus();
                }
                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum SetDevice(CardARMDevice device)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Device = device;

                if (string.IsNullOrEmpty(device.Label.Value) == false)
                    this.ID = ModuleID.Create(ID.ModuleType, ID.Index, device.Label.Value);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    RecoveryCardStatus();

                    Initialized = false;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override void DeInitModule()
        {

        }

        public EventCodeEnum WriteVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (_WriteVacuum == true)
                {
                    retval = Loader.IOManager.WriteIO(DOAIRON, value);
                    if (DOAIROFF != null)
                    {
                        bool reverse = !value;
                        retval = Loader.IOManager.WriteIO(DOAIROFF, reverse);
                        if (value == false)
                        {
                            System.Threading.Thread.Sleep(1000);
                            retval = Loader.IOManager.WriteIO(DOAIROFF, value);
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

        public EventCodeEnum MonitorForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.MonitorForIO(DIAIR_ON_MODULE, value, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum MonitorForCARDExist(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.MonitorForIO(DICARD_ON_MODULE, value, Definition.IOCheckMaintainTime.Value, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum WaitForVacuum(bool value)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = Loader.IOManager.WaitForIO(DIAIR_ON_MODULE, value, Definition.IOWaitTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum RecoveryCardStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            if (DICARD_ON_MODULE == null)
            {
                try
                {
                    bool isAcess = true;

                    if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                    {
                        if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDTRAY||
                            Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDBUFFER ||
                            Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_STAGE ||
                            Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDBUFFER_TO_CARDARM ||
                             Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDTRAY_TO_CARDARM ||
                              Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.STAGE_TO_CARDARM )
                        {
                                isAcess = false;
                        }
                    }
                    if (isAcess)
                    {
                        retVal = WriteVacuum(true);

                        if (Holder.Status == EnumSubsStatus.UNDEFINED)
                        {
                            //Check no wafer on module. 
                            retVal = MonitorForVacuum(false);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                //obj : undefined, io : no exist
                                Holder.SetUnload();
                                retVal = WriteVacuum(false);
                                retVal = WaitForVacuum(false);
                            }
                            else
                            {
                                //Check wafer on module. 
                                retVal = MonitorForVacuum(true);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    //obj : undefined, io : exist
                                    Holder.SetAllocate();
                                }
                                else
                                {
                                    //obj : undefined, io : unknown
                                    Holder.SetUnknown();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                        {
                            //Check no wafer on module.
                            retVal = MonitorForVacuum(false);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                //obj : not exist, io : not exist
                                //No changed.
                                retVal = WriteVacuum(false);
                                retVal = WaitForVacuum(false);
                            }
                            else
                            {

                                //obj : exist, io : unknown
                                retVal = MonitorForVacuum(true);
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    Holder.SetUnknown();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                                else
                                {
                                    bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.NOT_EXIST.ToString(), EnumSubsStatus.EXIST.ToString());
                                    if (isChecked)
                                    {
                                        Holder.SetAllocate();
                                        retVal = WriteVacuum(true);
                                        retVal = WaitForVacuum(true);
                                    }
                                    else
                                    {
                                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                        Holder.SetUnknown();
                                    }

                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.EXIST)
                        {
                            //Check wafer on module.
                            retVal = MonitorForVacuum(true);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                //obj : exist, io : exist
                                //No changed.
                            }
                            else
                            {
                                //obj : exist, io : unknown
                                retVal = MonitorForVacuum(false);
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    Holder.SetUnknown();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                                else
                                {
                                    bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.EXIST.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                                    if (isChecked)
                                    {
                                        Holder.SetUnload();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                    else
                                    {
                                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                        Holder.SetUnknown();
                                    }
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                        {
                            //Check no wafer on module. 
                            //** Unknwon상태에서는 사용자가 직접 제거해야 한다.

                            retVal = MonitorForVacuum(false);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.UNKNOWN.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                                //obj : unknown, io : no exist
                                if (isChecked)
                                {
                                    Holder.SetUnload();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                                else
                                {
                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                }
                            }
                            else
                            {

                                bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.UNKNOWN.ToString(), EnumSubsStatus.EXIST.ToString());
                                if (isChecked)
                                {
                                    retVal = MonitorForVacuum(false);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetUnload();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                    else
                                    {
                                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    }
                                }
                                else
                                {

                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    //obj : unknown, io : unknown
                                    //No changed.
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                            }

                        }
                        else
                        {
                            retVal = WriteVacuum(false);
                            retVal = WaitForVacuum(false);

                            throw new NotImplementedException("InitWaferStatus()");
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
            else
            {
                try
                {
                    bool isAcess = true;

                    if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                    {
                        if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDTRAY ||
                            Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDBUFFER ||
                            Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_STAGE ||
                            Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDBUFFER_TO_CARDARM ||
                             Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDTRAY_TO_CARDARM ||
                              Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.STAGE_TO_CARDARM)
                        {
                            isAcess = false;
                        }
                    }
                    if (isAcess)
                    {
                        retVal = WriteVacuum(true);

                        if (Holder.Status == EnumSubsStatus.UNDEFINED)
                        {
                            //Check no wafer on module. 
                            retVal = MonitorForVacuum(false);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = MonitorForCARDExist(true);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetAllocateCarrier();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                                else
                                {
                                    Holder.SetUnload();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                                    //obj : undefined, io : no exist
                            }
                            else
                            {
                                //Check wafer on module. 
                                retVal = MonitorForVacuum(true);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    //obj : undefined, io : exist
                                    Holder.SetAllocate();
                                }
                                else
                                {
                                    retVal = MonitorForCARDExist(true);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocateCarrier();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                    else
                                    {
                                        Holder.SetUnload();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                        {
                            //Check no wafer on module.
                            retVal = MonitorForVacuum(false);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                retVal = MonitorForCARDExist(true);
                                if (retVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetAllocateCarrier();
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                                else
                                {
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                            }
                            else
                            {

                                //obj : exist, io : unknown
                                retVal = MonitorForVacuum(true);
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    retVal = MonitorForCARDExist(true);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocateCarrier();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                    else
                                    {
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                }
                                else
                                {
                                    bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.NOT_EXIST.ToString(), EnumSubsStatus.EXIST.ToString());
                                    if (isChecked)
                                    {
                                        Holder.SetAllocate();
                                        retVal = WriteVacuum(true);
                                        retVal = WaitForVacuum(true);
                                    }
                                    else
                                    {
                                        retVal = MonitorForCARDExist(true);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            Holder.SetAllocateCarrier();
                                            retVal = WriteVacuum(false);
                                            retVal = WaitForVacuum(false);
                                        }
                                        else
                                        {
                                            retVal = WriteVacuum(false);
                                            retVal = WaitForVacuum(false);
                                        }
                                    }

                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.EXIST || Holder.Status == EnumSubsStatus.CARRIER)
                        {
                            //Check wafer on module.
                            retVal = MonitorForVacuum(true);
                            if (retVal == EventCodeEnum.NONE)
                            {
                                //obj : exist, io : exist
                                //No changed.
                            }
                            else
                            {
                                //obj : exist, io : unknown
                                retVal = MonitorForVacuum(false);
                                if (retVal != EventCodeEnum.NONE)
                                {
                                    retVal = MonitorForCARDExist(true);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocateCarrier();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                    else
                                    {
                                        Holder.SetUnload();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                }
                                else
                                {
                                    bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.EXIST.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                                    if (isChecked)
                                    {
                                        retVal = MonitorForCARDExist(true);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            Holder.SetAllocateCarrier();
                                            retVal = WriteVacuum(false);
                                            retVal = WaitForVacuum(false);
                                        }
                                        else
                                        {
                                            Holder.SetUnload();
                                            retVal = WriteVacuum(false);
                                            retVal = WaitForVacuum(false);
                                        }
                                    }
                                    else
                                    {
                                        retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                        Holder.SetUnknown();
                                    }
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                        {
                            //Check no wafer on module. 
                            //** Unknwon상태에서는 사용자가 직접 제거해야 한다.

                            retVal = MonitorForVacuum(false);

                            if (retVal == EventCodeEnum.NONE)
                            {
                                bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.UNKNOWN.ToString(), EnumSubsStatus.NOT_EXIST.ToString());
                                //obj : unknown, io : no exist
                                if (isChecked)
                                {
                                    retVal = MonitorForCARDExist(true);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocateCarrier();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                    else
                                    {
                                        Holder.SetUnload();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                }
                                else
                                {
                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                }
                            }
                            else
                            {

                                bool isChecked = WaferDisappearVM.Show(Loader.StageContainer, ModuleType.ToString(), EnumSubsStatus.UNKNOWN.ToString(), EnumSubsStatus.EXIST.ToString());
                                if (isChecked)
                                {
                                    retVal = MonitorForVacuum(false);
                                    if (retVal == EventCodeEnum.NONE)
                                    {
                                        retVal = MonitorForCARDExist(true);
                                        if (retVal == EventCodeEnum.NONE)
                                        {
                                            Holder.SetAllocateCarrier();
                                            retVal = WriteVacuum(false);
                                            retVal = WaitForVacuum(false);
                                        }
                                        else
                                        {
                                            Holder.SetUnload();
                                            retVal = WriteVacuum(false);
                                            retVal = WaitForVacuum(false);
                                        }
                                    }
                                    else
                                    {
                                        Holder.SetAllocate();
                                        retVal = WriteVacuum(false);
                                        retVal = WaitForVacuum(false);
                                    }
                                }
                                else
                                {

                                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                                    //obj : unknown, io : unknown
                                    //No changed.
                                    retVal = WriteVacuum(false);
                                    retVal = WaitForVacuum(false);
                                }
                            }

                        }
                        else
                        {
                            retVal = WriteVacuum(false);
                            retVal = WaitForVacuum(false);

                            throw new NotImplementedException("InitWaferStatus()");
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    retVal = EventCodeEnum.LOADER_STATE_INVALID;
                }
            }
               
            return retVal;
        }
        public EventCodeEnum AllocateCarrier()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Holder.SetAllocateCarrier();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                retVal = EventCodeEnum.LOADER_STATE_INVALID;
            }
            return retVal;
        }
        public void ValidateCardStatus()
        {
            try
            {
                bool isExistObj = Holder.Status == EnumSubsStatus.EXIST;

                //=> get iostate
                EventCodeEnum ioRetVal;
                ioRetVal = WriteVacuum(true);
                ioRetVal = MonitorForVacuum(isExistObj);

                if (isExistObj)
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        //obj : exist, io : exist
                        //No changed.
                    }
                    else
                    {
                        WriteVacuum(false);
                        WaitForVacuum(false);

                        //obj : exist, io : not exist
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        WriteVacuum(false);
                        WaitForVacuum(false);

                        //obj : not exist, io : not exist
                        //No changed.
                    }
                    else
                    {
                        //obj : not exist, io : exist
                        //status error - unknown detected.
                        Holder.SetUnknown();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
