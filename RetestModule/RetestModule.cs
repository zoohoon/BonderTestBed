using System;
using ProberInterfaces;
using ProberErrorCode;
using LogModule;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using RetestObject;
using ProberInterfaces.Retest;
using SerializerUtil;
using System.ServiceModel;

namespace RetestModule
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RetestModule : IRetestModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IParam _RetestModuleDevParam_IParam;
        public IParam RetestModuleDevParam_IParam
        {
            get { return _RetestModuleDevParam_IParam; }
            set
            {
                if (value != _RetestModuleDevParam_IParam)
                {
                    _RetestModuleDevParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IParam GetRetestIParam()
        {
            return this.RetestModuleDevParam_IParam;
        }

        public byte[] GetRetestParam()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(RetestModuleDevParam_IParam, typeof(RetestDeviceParam));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public void SetRetestIParam(byte[] param)
        {
            try
            {
                object target = null;

                var result = SerializeManager.DeserializeFromByte(param, out target, typeof(RetestDeviceParam));

                if (target != null)
                {
                    this.RetestModuleDevParam_IParam = target as RetestDeviceParam;
                    this.SaveDevParameter();
                }
                else
                {
                    LoggerManager.Error($"SetRetestIParam function is faild.");
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
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Initialized = true;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadDevParameter() // Parameter Type만 변경
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new RetestDeviceParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retval = this.LoadParameter(ref tmpParam, typeof(RetestDeviceParam));

                if (retval == EventCodeEnum.NONE)
                {
                    RetestModuleDevParam_IParam = tmpParam;
                }

                ForcedLotModeEnum forcedLotModeEnum = (RetestModuleDevParam_IParam as RetestDeviceParam).ForcedLotMode.Value;

                if(forcedLotModeEnum == ForcedLotModeEnum.ForcedCP1)
                {
                    this.StageSupervisor().ChangeLotMode(LotModeEnum.CP1);
                }
                else if (forcedLotModeEnum == ForcedLotModeEnum.ForcedMPP)
                {
                    this.StageSupervisor().ChangeLotMode(LotModeEnum.MPP);
                }
                else
                {
                    // Nothing : UNDEFINED, CONTINUEPROBING
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = this.SaveParameter(RetestModuleDevParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
    }
}
