using System;
using System.Linq;

using Autofac;
using LoaderBase;
using ProberInterfaces;
using LoaderParameters;
using ProberErrorCode;
using LogModule;

namespace LoaderCore
{
    internal class CardBufferTrayModule : AttachedModuleBase, ICardBufferTrayModule
    {
        private IOPortDescripter<bool> DICARDONMODULE;

        private IOPortDescripter<bool> DIDRAWERSENSOR;

        private IOPortDescripter<bool> DICARDONMODULE_DOWN;

        private IOPortDescripter<bool> DICARDATTACHHOLDER; // Holder 위 Card 가 있는지 확인

        public override bool Initialized { get; set; } = false;
        public override ModuleTypeEnum ModuleType => ModuleTypeEnum.CARDTRAY;

        public CardBufferTrayDefinition Definition { get; set; }

        public CardBufferTrayDevice Device { get; set; }

        public CardHolder Holder { get; set; }
        public bool Enable { get; set; }

        public TransferObject GetSourceDeviceInfo()
        {
            TransferObject retval = null;

            try
            {
                retval = Device?.AllocateDeviceInfo ?? null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDefinition(CardBufferTrayDefinition definition, int index)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                Enable = true;
                Definition = definition;
                ID = ModuleID.Create(ModuleType, index, "");

                DICARDONMODULE = Loader.IOManager.GetIOPortDescripter(Definition.DICARDONMODULE.Value);
                DIDRAWERSENSOR = Loader.IOManager.GetIOPortDescripter(Definition.DIDRAWERSENSOR.Value);

                if (Definition.DICARDONMODULE_DOWN.Value != null)
                {
                    DICARDONMODULE_DOWN = Loader.IOManager.GetIOPortDescripter(Definition.DICARDONMODULE_DOWN.Value);
                    DICARDONMODULE_DOWN.PropertyChanged += DICARDONMODULE_DOWN_PropertyChanged;
                }

                if (Definition.DICARDATTACHHOLDER.Value != null)
                {
                    DICARDATTACHHOLDER = Loader.IOManager.GetIOPortDescripter(Definition.DICARDATTACHHOLDER.Value);
                    DICARDATTACHHOLDER.PropertyChanged += DICARDATTACHHOLDER_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[CardBufferTrayModule], SetDefinition(), DICARDATTACHHOLDER is null.");
                }

                if(DICARDONMODULE != null)
                {
                    DICARDONMODULE.PropertyChanged += DICARDONMODULE_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[CardBufferTrayModule], SetDefinition(), DICARDONMODULE is null.");
                }

                if (DIDRAWERSENSOR != null)
                {
                    DIDRAWERSENSOR.PropertyChanged += DIDRAWERSENSOR_PropertyChanged;
                }
                else
                {
                    LoggerManager.Error($"[CardBufferTrayModule], SetDefinition(), DIDRAWERSENSOR is null.");
                }

                Holder = new CardHolder();
                Holder.SetOwner(this);

                RecoveryCardStatus();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        private void DICARDONMODULE_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {

                RecoveryCardStatus();

                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DICARDONMODULE_DOWN_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryCardStatus();

                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DICARDATTACHHOLDER_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryCardStatus();

                this.Loader.BroadcastLoaderInfo();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DIDRAWERSENSOR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                RecoveryCardStatus();

                this.Loader.BroadcastLoaderInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool IsDrawerSensorOn()
        {
            bool retVal = false;
            try
            {
                if (DIDRAWERSENSOR.IOOveride.Value == EnumIOOverride.NONE)
                {
                    if (DIDRAWERSENSOR.Value == false)
                    {
                        retVal = false;
                    }
                    else
                    {
                        retVal = true;
                    }
                } else
                {
                    retVal = true;
                }
            }
            catch (Exception err)
            {
                retVal = false;
                LoggerManager.Exception(err);
            }
            return retVal;
        }


  
        public EventCodeEnum SetDevice(CardBufferTrayDevice device)
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

        public EventCodeEnum MonitorForSubstrate(bool onTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(DICARDONMODULE, onTray, 100, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public EventCodeEnum MonitorForCardAttachHolder(bool onHoler)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (DICARDATTACHHOLDER == null)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else 
                {
                    retVal = Loader.IOManager.MonitorForIO(DICARDATTACHHOLDER, onHoler, 100, Definition.IOCheckDelayTimeout.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum MonitorForSubstrate_Down(bool onTray)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = Loader.IOManager.MonitorForIO(DICARDONMODULE_DOWN, onTray, 100, Definition.IOCheckDelayTimeout.Value);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public EventCodeEnum RecoveryCardStatus()
        {
            EventCodeEnum subOnTrayRetVal = EventCodeEnum.UNDEFINED;
            if (DICARDONMODULE_DOWN==null)
            {
                try
                {
                    bool isAcess = true;

                    if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                    {
                        if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDTRAY)
                        {
                            if (Loader.ProcModuleInfo.Destnation.Index == ID.Index)
                            {
                                isAcess = false;
                            }
                        }
                        else if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDTRAY_TO_CARDARM)
                        {
                            if (Loader.ProcModuleInfo.Source.Index == ID.Index)
                            {
                                isAcess = false;
                            }
                        }

                        subOnTrayRetVal = EventCodeEnum.NONE;
                    }

                    if (isAcess)
                    {
                        if (Holder.Status == EnumSubsStatus.UNDEFINED)
                        {
                            subOnTrayRetVal = MonitorForSubstrate(false);

                            //Check no wafer on module.
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {

                                //Check wafer on module.
                                subOnTrayRetVal = MonitorForSubstrate(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    //Holder.SetAllocateCarrier();
                                    Holder.SetAllocate();

                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                        {
                            subOnTrayRetVal = MonitorForSubstrate(false);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {
                                subOnTrayRetVal = MonitorForSubstrate(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetAllocate();
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.EXIST)
                        {
                            subOnTrayRetVal = MonitorForSubstrate(true);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                //Holder.SetAllocate();
                            }
                            else
                            {
                                subOnTrayRetVal = MonitorForSubstrate(false);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetUnload();
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                        {
                            subOnTrayRetVal = MonitorForSubstrate(false);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {
                                subOnTrayRetVal = MonitorForSubstrate(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetAllocate();
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else
                        {
                            throw new NotImplementedException("InitWaferStatus()");
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            else
            {
                try
                {
                    bool isAcess = true;

                    if (!IsDrawerSensorOn())
                    {
                        isAcess = false;
                        Holder.SetUnload();
                    }
                    else if (Loader.ModuleState == ModuleStateEnum.RUNNING)
                    {
                        if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDARM_TO_CARDTRAY)
                        {
                            if (Loader.ProcModuleInfo.Destnation.Index == ID.Index)
                            {
                                isAcess = false;
                            }
                        }
                        else if (Loader.ProcModuleInfo.ProcModule == LoaderProcModuleEnum.CARDTRAY_TO_CARDARM)
                        {
                            if (Loader.ProcModuleInfo.Source.Index == ID.Index)
                            {
                                isAcess = false;
                            }
                        }

                        subOnTrayRetVal = EventCodeEnum.NONE;
                    }

                    if (isAcess)
                    {
                        if (Holder.Status == EnumSubsStatus.UNDEFINED)
                        {
                            subOnTrayRetVal = MonitorForSubstrate_Down(false);

                            //Check no wafer on module.
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {

                                //Check wafer on module.
                                subOnTrayRetVal = MonitorForSubstrate_Down(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    subOnTrayRetVal = MonitorForSubstrate(true);
                                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocate();
                                        Holder.isCardAttachHolder = MonitorForCardAttachHolder(true) == EventCodeEnum.NONE ? true : false;
                                    }
                                    else
                                    {
                                        Holder.SetAllocateCarrier();
                                    }

                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.NOT_EXIST)
                        {
                            subOnTrayRetVal = MonitorForSubstrate_Down(false);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {
                                subOnTrayRetVal = MonitorForSubstrate_Down(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    subOnTrayRetVal = MonitorForSubstrate(true);
                                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocate();
                                        Holder.isCardAttachHolder = MonitorForCardAttachHolder(true) == EventCodeEnum.NONE ? true : false;
                                    }
                                    else
                                    {
                                        Holder.SetAllocateCarrier();
                                    }

                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.EXIST)
                        {
                            subOnTrayRetVal = MonitorForSubstrate_Down(true);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                subOnTrayRetVal = MonitorForSubstrate(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.isCardAttachHolder = MonitorForCardAttachHolder(true) == EventCodeEnum.NONE ? true : false;
                                }
                                else
                                {
                                    Holder.SetAllocateCarrier();
                                }
                            }
                            else
                            {
                                subOnTrayRetVal = MonitorForSubstrate_Down(false);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    Holder.SetUnload();
                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.UNKNOWN)
                        {
                            subOnTrayRetVal = MonitorForSubstrate_Down(false);
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {
                                subOnTrayRetVal = MonitorForSubstrate_Down(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    subOnTrayRetVal = MonitorForSubstrate(true);
                                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocate();
                                        Holder.isCardAttachHolder = MonitorForCardAttachHolder(true) == EventCodeEnum.NONE ? true : false;
                                    }
                                    else
                                    {
                                        Holder.SetAllocateCarrier();
                                    }

                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else if (Holder.Status == EnumSubsStatus.CARRIER)
                        {
                            subOnTrayRetVal = MonitorForSubstrate_Down(false);

                            //Check no wafer on module.
                            if (subOnTrayRetVal == EventCodeEnum.NONE)
                            {
                                Holder.SetUnload();
                            }
                            else
                            {
                                //Check wafer on module.
                                subOnTrayRetVal = MonitorForSubstrate_Down(true);
                                if (subOnTrayRetVal == EventCodeEnum.NONE)
                                {
                                    subOnTrayRetVal = MonitorForSubstrate(true);
                                    if (subOnTrayRetVal == EventCodeEnum.NONE)
                                    {
                                        Holder.SetAllocate();
                                        Holder.isCardAttachHolder = MonitorForCardAttachHolder(true) == EventCodeEnum.NONE ? true : false;
                                    }
                                    else
                                    {
                                        Holder.SetAllocateCarrier();
                                    }

                                }
                                else
                                {
                                    Holder.SetUnknown();
                                }
                            }
                        }
                        else
                        {
                            throw new NotImplementedException("InitWaferStatus()");
                        }
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            return subOnTrayRetVal;
        }

        public void ValidateCardStatus()
        {
            try
            {
                bool isExistObj = Holder.Status == EnumSubsStatus.EXIST;

                //=> get iostate
                EventCodeEnum ioRetVal;
                ioRetVal = MonitorForSubstrate(isExistObj);

                if (isExistObj)
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
                        //obj : exist, io : exist
                        //No changed.
                    }
                    else
                    {
                        //obj : exist, io : not exist
                        Holder.SetUnknown();
                    }
                }
                else
                {
                    if (ioRetVal == EventCodeEnum.NONE)
                    {
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

        public CardBufferTrayAccessParam GetAccessParam(SubstrateTypeEnum type, SubstrateSizeEnum size)
        {
            CardBufferTrayAccessParam system = null;

            try
            {
                system = Definition.AccessParams
               .Where(
               item =>
               item.SubstrateType.Value == type &&
               item.SubstrateSize.Value == size
               ).FirstOrDefault();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return system;
        }

    }

}
