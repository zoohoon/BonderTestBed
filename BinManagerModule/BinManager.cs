using BinParamObject;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BinManagerModule
{
    public class BinManager : IHasDevParameterizable, IModule, IFactoryModule, INotifyPropertyChanged
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


        private IParam _BinDevParam_IParam;
        public IParam BinDevParam_IParam
        {
            get { return _BinDevParam_IParam; }
            set
            {
                if (value != _BinDevParam_IParam)
                {
                    _BinDevParam_IParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public BinDeviceParam BindevParam
        {
            get { return BinDevParam_IParam as BinDeviceParam; }
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    retval = LoadDevParameter();

                    Initialized = true;
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

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                retval = this.LoadParameter(ref tmpParam, typeof(BinDeviceParam));

                if (retval == EventCodeEnum.NONE)
                {
                    BinDevParam_IParam = tmpParam;
                }

                //if(BindevParam.BinInfos.Value != null)
                //{
                //    if(BindevParam.BinInfos.Value.Count == 0)
                //    {
                //        BINInfo tmpbINInfo = new BINInfo();

                //        tmpbINInfo.BinCode.Value = 1;
                //        tmpbINInfo.RetestForCP2Enable.Value = true;
                //        BindevParam.BinInfos.Value.Add(tmpbINInfo);
                //    }
                //}

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetBinInfos(List<IBINInfo> binInfos)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(BindevParam.BinInfos != null && binInfos != null)
                {
                    BindevParam.BinInfos.Value = binInfos.ConvertAll(o => (BINInfo)o);

                    retval = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<IBINInfo> GetBinInfos()
        {
            List<IBINInfo> retval = null;

            try
            {
                if (BindevParam.BinInfos.Value != null)
                {
                    retval = BindevParam.BinInfos.Value.ToList<IBINInfo>();
                }
                else
                {
                    LoggerManager.Error($"[BinManager], GetBinInfos() : BindevParam.BinInfos.Value is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        //public EventCodeEnum UpdateBinDevParam()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

        //    try
        //    {
        //        //retVal = BindevParam.ConvertParam();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retVal;
        //}

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.SaveParameter(BinDevParam_IParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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
    }
}
