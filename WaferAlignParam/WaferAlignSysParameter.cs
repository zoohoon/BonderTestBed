namespace WaferAlignParam
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public class WaferAlignSysParameter : ISystemParameterizable, IParamNode, INotifyPropertyChanged
    {
        #region <!-- PropertyChanged -->
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region <!-- IParamNode Property -->
        [ParamIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [JsonIgnore, ParamIgnore]
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
        public List<object> Nodes { get; set; }
        #endregion

        #region <!-- IParam Property -->
        [JsonIgnore, ParamIgnore]
        public string FilePath { get; } = "WaferAlign";
        [JsonIgnore, ParamIgnore]
        public string FileName { get; } = "WaferAlignSysParameter.Json";
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        #endregion



        private Element<double> _VerifyCenterLimitX
            = new Element<double>() { Value = 5, LowerLimit = 0, UpperLimit = 10 };
        /// <summary>
        /// 등록된 패턴 위치와 얼라인 이후 등록된 패턴 위치로 갔을 때 얼마나 차이나는 지를 검사하기 위한 값 
        /// 왜냐면 등록위치가 웨이퍼 센터로부터 상대거리로 저장되고 얼라인 이후 웨이퍼 센터가 변경되기 때문에 등록된 위치로 다시 갔을 때 그 자리에 있기를 기대함. 
        /// 프로세싱오차나 이동 정밀도를 확인하기 위함.
        /// Min: 0, Max: 10 = 카메라 Ratio, 비전얼라인을 하므로 비전에서 발생하는 1픽셀 당 오차 +=5.4로 하면 10
        /// Value가 0이면 Verify 안한다.
        /// </summary>
        public Element<double> VerifyCenterLimitX
        {
            get { return _VerifyCenterLimitX; }
            set
            {
                if (value != _VerifyCenterLimitX)
                {
                    _VerifyCenterLimitX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _VerifyCenterLimitY
            = new Element<double>() { Value = 5, LowerLimit = 0, UpperLimit = 10 };
        /// <summary>
        /// 등록된 패턴 위치와 얼라인 이후 등록된 패턴 위치로 갔을 때 얼마나 차이나는 지를 검사하기 위한 값 
        /// 왜냐면 등록위치가 웨이퍼 센터로부터 상대거리로 저장되고 얼라인 이후 웨이퍼 센터가 변경되기 때문에 등록된 위치로 다시 갔을 때 그 자리에 있기를 기대함. 
        /// 프로세싱오차나 이동 정밀도를 확인하기 위함.
        /// Min: 0, Max: 10 = 카메라 Ratio, 비전얼라인을 하므로 비전에서 발생하는 1픽셀 당 오차 +=5.4로 하면 10
        /// Value가 0이면 Verify 안한다.
        /// </summary>
        public Element<double> VerifyCenterLimitY
        {
            get { return _VerifyCenterLimitY; }
            set
            {
                if (value != _VerifyCenterLimitY)
                {
                    _VerifyCenterLimitY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _EnableSkipWafer_WhenWaferAlignFailed
            = new Element<bool>() { Value = false };
        /// <summary>
        /// Wafer Align Fail 발생 시 웨이퍼 자동 Skip 하도록 처리할 지 결정 하는 값
        /// </summary>
        public Element<bool> EnableSkipWafer_WhenWaferAlignFailed 
        {
            get { return _EnableSkipWafer_WhenWaferAlignFailed; }
            set
            {
                if (value != _EnableSkipWafer_WhenWaferAlignFailed)
                {
                    _EnableSkipWafer_WhenWaferAlignFailed = value;
                    RaisePropertyChanged();
                }
            }
        }
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

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void SetElementMetaData()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
