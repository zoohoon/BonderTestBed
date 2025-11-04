using System;
using BinAnalyzer;
using BinAnalyzer.Data;
using ProberErrorCode;
using ProberInterfaces;
using BinAnalyzer.Analzer;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using LogModule;
using ProberInterfaces.Enum;
using ProberInterfaces.BinData;

namespace BinAnalyzerManager
{
    public class BinAnalyzerManager : IFactoryModule, IModule, IHasDevParameterizable
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

        //private IParam _DevParam;
        //[ParamIgnore]
        //public IParam DevParam
        //{
        //    get { return _DevParam; }
        //    set
        //    {
        //        if (value != _DevParam)
        //        {
        //            _DevParam = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private BinAnalyzerBase binAnalyzer;

        public BinDevParam binDevParam { get; set; }

        public BinAnalyzerManager()
        {
            try
            {
                binAnalyzer = BinAnalyzerObjFactory.GetBinAnalyzerObj(BinType.BIN_PASSFAIL);
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
                    Initialized = true;

                    retval = EventCodeEnum.NONE;
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
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new BinDevParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(BinDevParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    binDevParam = tmpParam as BinDevParam;
                }

                //DevParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                
            }
            return retVal;
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

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(binDevParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetBinAnalyzer(BinType enumBinType)
        {
            try
            {
                HardBinData binData = null;
                PreUserBinDicCount = 0;

                if (enumBinType == BinType.BIN_U32_32BIT)
                {
                    binData = new HardBinData();
                    binData.UserBinDictionary = binDevParam.U32_32BitDictionary;
                    binData.HardBinSize = 8;
                }
                else if (enumBinType == BinType.BIN_U32_64BIT)
                {
                    binData = new HardBinData();
                    binData.UserBinDictionary = binDevParam.U32_64BitDictionary;
                    binData.HardBinSize = 16;
                }
                else if (enumBinType == BinType.BIN_USERBIN)
                {
                    binData = new HardBinData();
                    binData.UserBinDictionary = binDevParam.UserBinDictionary;
                    binData.HardBinSize = binDevParam.UserBinSize;
                }

                if (binData != null)
                {
                    PreUserBinDicCount = binData.UserBinDictionary.Count;
                }
                else
                {
                    PreUserBinDicCount = 0;
                }

                binAnalyzer = BinAnalyzerObjFactory.GetBinAnalyzerObj(enumBinType, binData);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetTestResultAnalysis(string prefixChar, string testResultCode, int siteNum, ref BinAnalysisDataArray analysisDataArray)
        {
            bool retVal = false;

            try
            {
                retVal = binAnalyzer.GetTestResultAnalysis(prefixChar, testResultCode, siteNum, ref analysisDataArray);

                if (binAnalyzer.HardBinData != null &&
                    binAnalyzer.HardBinData.UserBinDictionary != null &&
                    binAnalyzer.HardBinData.UserBinDictionary.Count != PreUserBinDicCount)
                {
                    SaveDevParameter();
                    PreUserBinDicCount = binAnalyzer.HardBinData.UserBinDictionary.Count;
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

        }

        private int PreUserBinDicCount = 0;
    }
}
