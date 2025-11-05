using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.BinData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace BinParamObject
{


    [Serializable]
    public class BINInfo : IBINInfo, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        /// <summary>
        /// BIN 번호
        /// </summary>
        private Element<int> _binCode = new Element<int>();
        public Element<int> BinCode
        {
            get { return _binCode; }
            set
            {
                if (value != _binCode)
                {
                    _binCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Pass로 간주할 것인지, Fail로 간주할 것인지..
        /// '-- 0: Fail, 1: Pass
        /// </summary>
        private Element<int> _passFail = new Element<int>();
        public Element<int> PassFail
        {
            get { return _passFail; }
            set
            {
                if (value != _passFail)
                {
                    _passFail = value;
                    RaisePropertyChanged();
                }
            }
        }

        // TODO : 맵에 표시되는 색상 데이터 필요

        /// <summary>
        /// 결과맵에 표시되는 설명(?)
        /// </summary>
        private Element<string> _description = new Element<string>();
        public Element<string> Description
        {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 첫 번째, Retest 할것인지 말것인지
        /// 프로빙 시퀀스 다 끝내고 조건에 의해서 동작할 때....(Online Retest)
        /// </summary>
        private Element<bool> _retestForCP1Enable = new Element<bool>();
        public Element<bool> RetestForCP1Enable
        {
            get { return _retestForCP1Enable; }
            set
            {
                if (value != _retestForCP1Enable)
                {
                    _retestForCP1Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// CP2를 할것인지 말것인지
        /// </summary>
        private Element<bool> _retestForCP2Enable = new Element<bool>();
        public Element<bool> RetestForCP2Enable
        {
            get { return _retestForCP2Enable; }
            set
            {
                if (value != _retestForCP2Enable)
                {
                    _retestForCP2Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// N번째 Retest를 할것인지 말것인지
        /// N번째 Retest의 경우, BIN 개수를 이용하여 트리거가 된다.
        /// </summary>
        private Element<bool> _retestForNthEnable = new Element<bool>();
        public Element<bool> RretestForNthEnable
        {
            get { return _retestForNthEnable; }
            set
            {
                if (value != _retestForNthEnable)
                {
                    _retestForNthEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// CP1에서 즉시, 한번 더 할 것인지 말 것인지
        /// </summary>
        private Element<bool> _instantRetestForCP1Enable = new Element<bool>();
        public Element<bool> InstantRetestForCP1Enable
        {
            get { return _instantRetestForCP1Enable; }
            set
            {
                if (value != _instantRetestForCP1Enable)
                {
                    _instantRetestForCP1Enable = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Retest에서 즉시, 한번 더 할 것인지 말 것인지
        /// </summary>
        private Element<bool> _instantRetestForRetestEnable = new Element<bool>();
        public Element<bool> InstantRetestForRetestEnable
        {
            get { return _instantRetestForRetestEnable; }
            set
            {
                if (value != _instantRetestForRetestEnable)
                {
                    _instantRetestForRetestEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// nthRetest 트리거를 위해 사용되는 BIN 개수
        /// </summary>
        private Element<int> _nthRetestBINCnt = new Element<int>();
        public Element<int> NthRetestBINCnt
        {
            get { return _nthRetestBINCnt; }
            set
            {
                if (value != _nthRetestBINCnt)
                {
                    _nthRetestBINCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// BIN이 연속 몇개 나왔을 때, 에러를 발생시킬 것인지
        /// </summary>
        private Element<int> _continuousFailCnt = new Element<int>();
        public Element<int> ContinuousFailCnt
        {
            get { return _continuousFailCnt; }
            set
            {
                if (value != _continuousFailCnt)
                {
                    _continuousFailCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// BIN이 누적하여 몇개 나왔을 때, 에러를 발생시킬 것인지
        /// </summary>
        private Element<int> _accumulateFailCnt = new Element<int>();
        public Element<int> AccumulateFailCnt
        {
            get { return _accumulateFailCnt; }
            set
            {
                if (value != _accumulateFailCnt)
                {
                    _accumulateFailCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

       

        private Element<int> _bINGroupNo = new Element<int>();
        public Element<int> BINGroupNo
        {
            get { return _bINGroupNo; }
            set
            {
                if (value != _bINGroupNo)
                {
                    _bINGroupNo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _Unusable = new Element<bool>();
        public Element<bool> Unusable
        {
            get { return _Unusable; }
            set
            {
                if (value != _Unusable)
                {
                    _Unusable = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable]
    public class BinDeviceParam : IBinDeviceParam, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "Bin";

        public string FileName { get; } = "Bin.json";

        [XmlIgnore, JsonIgnore]
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

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        private Element<List<BINInfo>> _BinInfos = new Element<List<BINInfo>>();
        public Element<List<BINInfo>> BinInfos
        {
            get { return _BinInfos; }
            set
            {
                if (value != _BinInfos)
                {
                    _BinInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if(BinInfos.Value == null)
                {
                    BinInfos.Value = new List<BINInfo>();
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SetDefaultParam();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
