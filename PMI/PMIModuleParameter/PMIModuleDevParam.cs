using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogModule;
using ProberInterfaces.PMI;
using Newtonsoft.Json;
using System.Xml.Serialization;
using ProberInterfaces.Param;
using Focusing;
using ProberInterfaces.Enum;

namespace PMIModuleParameter
{
    [Serializable]
    public class PMIFocusingDLLInfo : IPMIFocusingDLLInfo, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ModuleDllInfo _NormalFocusingInfo;
        public ModuleDllInfo NormalFocusingInfo
        {
            get { return _NormalFocusingInfo; }
            set
            {
                if (value != _NormalFocusingInfo)
                {
                    _NormalFocusingInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ModuleDllInfo _BumpFocusingInfo;
        public ModuleDllInfo BumpFocusingInfo
        {
            get { return _BumpFocusingInfo; }
            set
            {
                if (value != _BumpFocusingInfo)
                {
                    _BumpFocusingInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
    [Serializable]
    public class PMIFTP : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<string> _FTPHostAddress = new Element<string>();
        public Element<string> FTPHostAddress
        {
            get { return _FTPHostAddress; }
            set
            {
                if (value != _FTPHostAddress)
                {
                    _FTPHostAddress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _FTPUserID = new Element<string>();
        public Element<string> FTPUserID
        {
            get { return _FTPUserID; }
            set
            {
                if (value != _FTPUserID)
                {
                    _FTPUserID = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _FTPPassword = new Element<string>();
        public Element<string> FTPPassword
        {
            get { return _FTPPassword; }
            set
            {
                if (value != _FTPPassword)
                {
                    _FTPPassword = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _LogPath = new Element<string>();
        public Element<string> LogPath
        {
            get { return _LogPath; }
            set
            {
                if (value != _LogPath)
                {
                    _LogPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _FailImagePath = new Element<string>();
        public Element<string> FailImagePath
        {
            get { return _FailImagePath; }
            set
            {
                if (value != _FailImagePath)
                {
                    _FailImagePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _PassImagePath = new Element<string>();
        public Element<string> PassImagePath
        {
            get { return _PassImagePath; }
            set
            {
                if (value != _PassImagePath)
                {
                    _PassImagePath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _NetworkDriveDirectory = new Element<string>();
        public Element<string> NetworkDriveDirectory
        {
            get { return _NetworkDriveDirectory; }
            set
            {
                if (value != _NetworkDriveDirectory)
                {
                    _NetworkDriveDirectory = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PMIFTP()
        {

        }
    }

    [Serializable]
    public class PMIFailCode : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<bool> _FailCodeEnableBondEdge = new Element<bool>();
        public Element<bool> FailCodeEnableBondEdge
        {
            get { return _FailCodeEnableBondEdge; }
            set
            {
                if (value != _FailCodeEnableBondEdge)
                {
                    _FailCodeEnableBondEdge = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableNoProbeMark = new Element<bool>();
        public Element<bool> FailCodeEnableNoProbeMark
        {
            get { return _FailCodeEnableNoProbeMark; }
            set
            {
                if (value != _FailCodeEnableNoProbeMark)
                {
                    _FailCodeEnableNoProbeMark = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableSmallMarkSize = new Element<bool>();
        public Element<bool> FailCodeEnableSmallMarkSize
        {
            get { return _FailCodeEnableSmallMarkSize; }
            set
            {
                if (value != _FailCodeEnableSmallMarkSize)
                {
                    _FailCodeEnableSmallMarkSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableBigMarkSize = new Element<bool>();
        public Element<bool> FailCodeEnableBigMarkSize
        {
            get { return _FailCodeEnableBigMarkSize; }
            set
            {
                if (value != _FailCodeEnableBigMarkSize)
                {
                    _FailCodeEnableBigMarkSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableSmallMarkArea = new Element<bool>();
        public Element<bool> FailCodeEnableSmallMarkArea
        {
            get { return _FailCodeEnableSmallMarkArea; }
            set
            {
                if (value != _FailCodeEnableSmallMarkArea)
                {
                    _FailCodeEnableSmallMarkArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableBigMarkArea = new Element<bool>();
        public Element<bool> FailCodeEnableBigMarkArea
        {
            get { return _FailCodeEnableBigMarkArea; }
            set
            {
                if (value != _FailCodeEnableBigMarkArea)
                {
                    _FailCodeEnableBigMarkArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableMarkCntOver = new Element<bool>();
        public Element<bool> FailCodeEnableMarkCntOver
        {
            get { return _FailCodeEnableMarkCntOver; }
            set
            {
                if (value != _FailCodeEnableMarkCntOver)
                {
                    _FailCodeEnableMarkCntOver = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailCodeEnableBallBumpDent = new Element<bool>();
        public Element<bool> FailCodeEnableBallBumpDent
        {
            get { return _FailCodeEnableBallBumpDent; }
            set
            {
                if (value != _FailCodeEnableBallBumpDent)
                {
                    _FailCodeEnableBallBumpDent = value;
                    RaisePropertyChanged();
                }
            }
        }


        public PMIFailCode()
        {

        }
    }

    [Serializable]
    public class PMILog : IPMILog, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<LOGGING_MODE> _LoggingMode = new Element<LOGGING_MODE>();
        public Element<LOGGING_MODE> LoggingMode
        {
            get { return _LoggingMode; }
            set
            {
                if (value != _LoggingMode)
                {
                    _LoggingMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<bool> _LogSaveHDDEnable = new Element<bool>();
        public Element<bool> LogSaveHDDEnable
        {
            get { return _LogSaveHDDEnable; }
            set
            {
                if (value != _LogSaveHDDEnable)
                {
                    _LogSaveHDDEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _LogSaveFTPEnable = new Element<bool>();
        public Element<bool> LogSaveFTPEnable
        {
            get { return _LogSaveFTPEnable; }
            set
            {
                if (value != _LogSaveFTPEnable)
                {
                    _LogSaveFTPEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _LogSaveNETEnable = new Element<bool>();
        public Element<bool> LogSaveNETEnable
        {
            get { return _LogSaveNETEnable; }
            set
            {
                if (value != _LogSaveNETEnable)
                {
                    _LogSaveNETEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailImageSaveHDDEnable = new Element<bool>();
        public Element<bool> FailImageSaveHDDEnable
        {
            get { return _FailImageSaveHDDEnable; }
            set
            {
                if (value != _FailImageSaveHDDEnable)
                {
                    _FailImageSaveHDDEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailImageSaveFTPEnable = new Element<bool>();
        public Element<bool> FailImageSaveFTPEnable
        {
            get { return _FailImageSaveFTPEnable; }
            set
            {
                if (value != _FailImageSaveFTPEnable)
                {
                    _FailImageSaveFTPEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FailImageSaveNETEnable = new Element<bool>();
        public Element<bool> FailImageSaveNETEnable
        {
            get { return _FailImageSaveNETEnable; }
            set
            {
                if (value != _FailImageSaveNETEnable)
                {
                    _FailImageSaveNETEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PassImageSaveHDDEnable = new Element<bool>();
        public Element<bool> PassImageSaveHDDEnable
        {
            get { return _PassImageSaveHDDEnable; }
            set
            {
                if (value != _PassImageSaveHDDEnable)
                {
                    _PassImageSaveHDDEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PassImageSaveFTPEnable = new Element<bool>();
        public Element<bool> PassImageSaveFTPEnable
        {
            get { return _PassImageSaveFTPEnable; }
            set
            {
                if (value != _PassImageSaveFTPEnable)
                {
                    _PassImageSaveFTPEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _PassImageSaveNETEnable = new Element<bool>();
        public Element<bool> PassImageSaveNETEnable
        {
            get { return _PassImageSaveNETEnable; }
            set
            {
                if (value != _PassImageSaveNETEnable)
                {
                    _PassImageSaveNETEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _OriginalImageSaveHDDEnable = new Element<bool>();
        public Element<bool> OriginalImageSaveHDDEnable
        {
            get { return _OriginalImageSaveHDDEnable; }
            set
            {
                if (value != _OriginalImageSaveHDDEnable)
                {
                    _OriginalImageSaveHDDEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PMILog()
        {

        }
    }

    [Serializable]
    public class PMIModuleDevParam : IPMIModuleDevParam, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PMIModuleDevParam()
        {

        }
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath { get; } = "PMIModule";
        public string FileName { get; } = "PMIModuleDevParameter.json";
        private string _Genealogy;
        public string Genealogy
        {
            get { return _Genealogy; }
            set { _Genealogy = value; }
        }
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

        public List<object> Nodes { get; set; }

        private FocusParameter _NormalFocusParam;
        public FocusParameter NormalFocusParam
        {
            get { return _NormalFocusParam; }
            set
            {
                if (value != _NormalFocusParam)
                {
                    _NormalFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FocusParameter _BumpFocusParam;
        public FocusParameter BumpFocusParam
        {
            get { return _BumpFocusParam; }
            set
            {
                if (value != _BumpFocusParam)
                {
                    _BumpFocusParam = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<double> _FocusingROIExpansionRatio = new Element<double>() { Value = 1.2 };
        /// <summary>
        /// Ratio = Pad Focusing ROI / Pad Size 
        /// Operating Condition: Focusing ROI Size = Grab Size
        /// TODO: 추후 FocusingROI 를 설정할 수 있는 화면이 생기면 의도적으로 ROI를 수정했다고 판단하고 Ratio 를 0으로 만들어야함. 
        /// </summary>
        public Element<double> FocusingROIExpansionRatio
        {
            get { return _FocusingROIExpansionRatio; }
            set
            {
                if (value != _FocusingROIExpansionRatio)
                {
                    _FocusingROIExpansionRatio = value;
                    RaisePropertyChanged();
                }
            }
        }

      
        #region //..Parameters
        //private Element<OP_MODE> _OperationMode = new Element<OP_MODE>();
        //public Element<OP_MODE> OperationMode
        //{
        //    get { return _OperationMode; }
        //    set
        //    {
        //        if (value != _OperationMode)
        //        {
        //            _OperationMode = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<int> _WaferInterval = new Element<int>();
        //public Element<int> WaferInterval
        //{
        //    get { return _WaferInterval; }
        //    set
        //    {
        //        if (value != _WaferInterval)
        //        {
        //            _WaferInterval = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private Element<int> _ContactCountInterval = new Element<int>();
        //public Element<int> ContactCountInterval
        //{
        //    get { return _ContactCountInterval; }
        //    set
        //    {
        //        if (value != _ContactCountInterval)
        //        {
        //            _ContactCountInterval = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<OP_MODE> _NormalPMI = new Element<OP_MODE>();
        public Element<OP_MODE> NormalPMI
        {
            get { return _NormalPMI; }
            set
            {
                if (value != _NormalPMI)
                {
                    _NormalPMI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<OP_MODE_ON_RETEST> _OperationModeOnRetest = new Element<OP_MODE_ON_RETEST>();
        public Element<OP_MODE_ON_RETEST> OperationModeOnRetest
        {
            get { return _OperationModeOnRetest; }
            set
            {
                if (value != _OperationModeOnRetest)
                {
                    _OperationModeOnRetest = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PMI_PAUSE_METHOD> _PauseMethod = new Element<PMI_PAUSE_METHOD>();
        public Element<PMI_PAUSE_METHOD> PauseMethod
        {
            get { return _PauseMethod; }
            set
            {
                if (value != _PauseMethod)
                {
                    _PauseMethod = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private Element<bool> _PauseImmediately = new Element<bool>();
        //public Element<bool> PauseImmediately
        //{
        //    get { return _PauseImmediately; }
        //    set
        //    {
        //        if (value != _PauseImmediately)
        //        {
        //            _PauseImmediately = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private Element<double> _PausePadCntPerWafer = new Element<double>();
        public Element<double> PausePadCntPerWafer
        {
            get { return _PausePadCntPerWafer; }
            set
            {
                if (value != _PausePadCntPerWafer)
                {
                    _PausePadCntPerWafer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PausePercentPerDie = new Element<double>();
        public Element<double> PausePercentPerDie
        {
            get { return _PausePercentPerDie; }
            set
            {
                if (value != _PausePercentPerDie)
                {
                    _PausePercentPerDie = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _PauseContinuousFailDie = new Element<int>();
        public Element<int> PauseContinuousFailDie
        {
            get { return _PauseContinuousFailDie; }
            set
            {
                if (value != _PauseContinuousFailDie)
                {
                    _PauseContinuousFailDie = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _SkipPMIDiesAfterPause = new Element<bool>();
        public Element<bool> SkipPMIDiesAfterPause
        {
            get { return _SkipPMIDiesAfterPause; }
            set
            {
                if (value != _SkipPMIDiesAfterPause)
                {
                    _SkipPMIDiesAfterPause = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<MARK_COMPARE_MODE> _MarkCompareMode = new Element<MARK_COMPARE_MODE>();
        public Element<MARK_COMPARE_MODE> MarkCompareMode
        {
            get { return _MarkCompareMode; }
            set
            {
                if (value != _MarkCompareMode)
                {
                    _MarkCompareMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<MARK_COMPARE_UNIT> _MarkCompareUnit = new Element<MARK_COMPARE_UNIT>();
        public Element<MARK_COMPARE_UNIT> MarkCompareUnit
        {
            get { return _MarkCompareUnit; }
            set
            {
                if (value != _MarkCompareUnit)
                {
                    _MarkCompareUnit = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<MARK_AREA_CALCULATE_MODE> _MarkAreaCalculateMode = new Element<MARK_AREA_CALCULATE_MODE>(0);
        /// <summary>
        /// 블롭 면적 계산 방식 
        /// </summary>
        public Element<MARK_AREA_CALCULATE_MODE> MarkAreaCalculateMode
        {
            get { return _MarkAreaCalculateMode; }
            set
            {
                if (value != _MarkAreaCalculateMode)
                {
                    _MarkAreaCalculateMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _MaximumMarkCnt = new Element<int>();
        public Element<int> MaximumMarkCnt
        {
            get { return _MaximumMarkCnt; }
            set
            {
                if (value != _MaximumMarkCnt)
                {
                    _MaximumMarkCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadEdgeBlockingValue = new Element<double>();
        public Element<double> PadEdgeBlockingValue
        {
            get { return _PadEdgeBlockingValue; }
            set
            {
                if (value != _PadEdgeBlockingValue)
                {
                    _PadEdgeBlockingValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _MaximumBlobMarkCnt = new Element<int>();
        public Element<int> MaximumBlobMarkCnt
        {
            get { return _MaximumBlobMarkCnt; }
            set
            {
                if (value != _MaximumBlobMarkCnt)
                {
                    _MaximumBlobMarkCnt = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<GROUPING_METHOD> _PadGroupingMethod = new Element<GROUPING_METHOD>();
        public Element<GROUPING_METHOD> PadGroupingMethod
        {
            get { return _PadGroupingMethod; }
            set
            {
                if (value != _PadGroupingMethod)
                {
                    _PadGroupingMethod = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadGroupingMargin = new Element<double>();
        public Element<double> PadGroupingMargin
        {
            get { return _PadGroupingMargin; }
            set
            {
                if (value != _PadGroupingMargin)
                {
                    _PadGroupingMargin = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadFindMarginX = new Element<double>();
        public Element<double> PadFindMarginX
        {
            get { return _PadFindMarginX; }
            set
            {
                if (value != _PadFindMarginX)
                {
                    _PadFindMarginX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _PadFindMarginY = new Element<double>();
        public Element<double> PadFindMarginY
        {
            get { return _PadFindMarginY; }
            set
            {
                if (value != _PadFindMarginY)
                {
                    _PadFindMarginY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _BallBumpDentTolerance = new Element<double>();
        public Element<double> BallBumpDentTolerance
        {
            get { return _BallBumpDentTolerance; }
            set
            {
                if (value != _BallBumpDentTolerance)
                {
                    _BallBumpDentTolerance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MinMarkNoiseSizeX = new Element<double>();
        public Element<double> MinMarkNoiseSizeX
        {
            get { return _MinMarkNoiseSizeX; }
            set
            {
                if (value != _MinMarkNoiseSizeX)
                {
                    _MinMarkNoiseSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MinMarkNoiseSizeY = new Element<double>();
        public Element<double> MinMarkNoiseSizeY
        {
            get { return _MinMarkNoiseSizeY; }
            set
            {
                if (value != _MinMarkNoiseSizeY)
                {
                    _MinMarkNoiseSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MinMarkNoiseArea = new Element<double>();
        public Element<double> MinMarkNoiseArea
        {
            get { return _MinMarkNoiseArea; }
            set
            {
                if (value != _MinMarkNoiseArea)
                {
                    _MinMarkNoiseArea = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// %
        /// </summary>
        private Element<double> _MarkOverlapPercentOfToleranceX = new Element<double>();
        public Element<double> MarkOverlapPercentOfToleranceX
        {
            get { return _MarkOverlapPercentOfToleranceX; }
            set
            {
                if (value != _MarkOverlapPercentOfToleranceX)
                {
                    _MarkOverlapPercentOfToleranceX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkOverlapPercentOfToleranceY = new Element<double>();
        public Element<double> MarkOverlapPercentOfToleranceY
        {
            get { return _MarkOverlapPercentOfToleranceY; }
            set
            {
                if (value != _MarkOverlapPercentOfToleranceY)
                {
                    _MarkOverlapPercentOfToleranceY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _FocusingEnable = new Element<bool>();
        public Element<bool> FocusingEnable
        {
            get { return _FocusingEnable; }
            set
            {
                if (value != _FocusingEnable)
                {
                    _FocusingEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _FocusingDutInterval = new Element<int>();
        public Element<int> FocusingDutInterval
        {
            get { return _FocusingDutInterval; }
            set
            {
                if (value != _FocusingDutInterval)
                {
                    _FocusingDutInterval = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 그룹마다 포커싱을 수행할지 결정하는 파라미터
        /// 기존의 FocusingEnable 파라미터는 Dut Interval에 따라 수행되는 파라미터로써
        /// 같은 다이에서 포커싱을 한번 진행하고 해당 Z값이 사용되지만, 이 파라미터는 한 다이 내에서도 그룹이 여러개일 때, 각 그룹마다 포커싱을 진행한다.
        /// 한 다이내에서도 높이 차이가 날 때, 대응하기 위한 파라미터로 생각하면 됨.
        /// </summary>
        private Element<bool> _FocusingEachGroupEnable = new Element<bool>();
        public Element<bool> FocusingEachGroupEnable
        {
            get { return _FocusingEachGroupEnable; }
            set
            {
                if (value != _FocusingEachGroupEnable)
                {
                    _FocusingEachGroupEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _AutoLightEnable = new Element<bool>();
        public Element<bool> AutoLightEnable
        {
            get { return _AutoLightEnable; }
            set
            {
                if (value != _AutoLightEnable)
                {
                    _AutoLightEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ushort> _LightValue = new Element<ushort>();
        public Element<ushort> LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ushort> _ObliqLightValue = new Element<ushort>();
        public Element<ushort> ObliqLightValue
        {
            get { return _ObliqLightValue; }
            set
            {
                if (value != _ObliqLightValue)
                {
                    _ObliqLightValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ushort> _AutoLightStart = new Element<ushort>();
        public Element<ushort> AutoLightStart
        {
            get { return _AutoLightStart; }
            set
            {
                if (value != _AutoLightStart)
                {
                    _AutoLightStart = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ushort> _AutoLightEnd = new Element<ushort>();
        public Element<ushort> AutoLightEnd
        {
            get { return _AutoLightEnd; }
            set
            {
                if (value != _AutoLightEnd)
                {
                    _AutoLightEnd = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ushort> _AutoLightInterval = new Element<ushort>();
        public Element<ushort> AutoLightInterval
        {
            get { return _AutoLightInterval; }
            set
            {
                if (value != _AutoLightInterval)
                {
                    _AutoLightInterval = value;
                    RaisePropertyChanged();
                }
            }
        }



        private Element<bool> _DisplayPadDuringPMI = new Element<bool>();
        public Element<bool> DisplayPadDuringPMI
        {
            get { return _DisplayPadDuringPMI; }
            set
            {
                if (value != _DisplayPadDuringPMI)
                {
                    _DisplayPadDuringPMI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _DisplayMarkDuringPMI = new Element<bool>();
        public Element<bool> DisplayMarkDuringPMI
        {
            get { return _DisplayMarkDuringPMI; }
            set
            {
                if (value != _DisplayMarkDuringPMI)
                {
                    _DisplayMarkDuringPMI = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _SkipProcessing = new Element<bool>();
        public Element<bool> SkipProcessing
        {
            get { return _SkipProcessing; }
            set
            {
                if (value != _SkipProcessing)
                {
                    _SkipProcessing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _SearchPadEnable = new Element<bool>();
        public Element<bool> SearchPadEnable
        {
            get { return _SearchPadEnable; }
            set
            {
                if (value != _SearchPadEnable)
                {
                    _SearchPadEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkAlignTriggerToleranceUsingPadOffsetX = new Element<double>();
        public Element<double> MarkAlignTriggerToleranceUsingPadOffsetX
        {
            get { return _MarkAlignTriggerToleranceUsingPadOffsetX; }
            set
            {
                if (value != _MarkAlignTriggerToleranceUsingPadOffsetX)
                {
                    _MarkAlignTriggerToleranceUsingPadOffsetX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _MarkAlignTriggerToleranceUsingPadOffsetY = new Element<double>();
        public Element<double> MarkAlignTriggerToleranceUsingPadOffsetY
        {
            get { return _MarkAlignTriggerToleranceUsingPadOffsetY; }
            set
            {
                if (value != _MarkAlignTriggerToleranceUsingPadOffsetY)
                {
                    _MarkAlignTriggerToleranceUsingPadOffsetY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumProberCam> _ProcessingCamType = new Element<EnumProberCam>();
        public Element<EnumProberCam> ProcessingCamType
        {
            get { return _ProcessingCamType; }
            set
            {
                if (value != _ProcessingCamType)
                {
                    _ProcessingCamType = value;
                    RaisePropertyChanged();
                }
            }
        }


        private PMIFTP _FTPInfo;
        public PMIFTP FTPInfo
        {
            get { return _FTPInfo; }
            set
            {
                if (value != _FTPInfo)
                {
                    _FTPInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMIFailCode _FailCodeInfo;
        public PMIFailCode FailCodeInfo
        {
            get { return _FailCodeInfo; }
            set
            {
                if (value != _FailCodeInfo)
                {
                    _FailCodeInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMITriggerComponent _TriggerComponent;
        public PMITriggerComponent TriggerComponent
        {
            get { return _TriggerComponent; }
            set
            {
                if (value != _TriggerComponent)
                {
                    _TriggerComponent = value;
                    RaisePropertyChanged();
                }
            }
        }


        private IPMILog _LogInfo;
        public IPMILog LogInfo
        {
            get { return _LogInfo; }
            set
            {
                if (value != _LogInfo)
                {
                    _LogInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IPMIFocusingDLLInfo _FocusingDllInfo;
        public IPMIFocusingDLLInfo FocusingDllInfo
        {
            get { return _FocusingDllInfo; }
            set
            {
                if (value != _FocusingDllInfo)
                {
                    _FocusingDllInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        //[Obsolete("This variable is deprecated. Please use 'PadMarkDifferentiation' instead.")]
        private Element<bool> _PatternInPadMaskingFilterEnable = new Element<bool>();
        /// <summary>
        /// [Obsolete("This variable is deprecated. Please use 'PadMarkDifferentiation' instead.")]
        /// PMI 검사시 이미지 전처리 여부
        /// </summary>
        public Element<bool> PatternInPadMaskingFilterEnable
        {
            get { return _PatternInPadMaskingFilterEnable; }
            set
            {
                if (value != _PatternInPadMaskingFilterEnable)
                {
                    _PatternInPadMaskingFilterEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 사용자 선택 옵션 바인딩 용
        /// </summary>
        [JsonIgnore]
        public System.Collections.ObjectModel.ObservableCollection<PAD_MARK_DIFFERENTIATION_MODE> PadMarkDifferentiationModes { get; set; }
            = new System.Collections.ObjectModel.ObservableCollection<PAD_MARK_DIFFERENTIATION_MODE>()
            { 
                PAD_MARK_DIFFERENTIATION_MODE.SimpleThreshold,
                PAD_MARK_DIFFERENTIATION_MODE.PadNoiseAwareThreshold,
                PAD_MARK_DIFFERENTIATION_MODE.ReferenceImage
            };

        private Element<PAD_MARK_DIFFERENTIATION_MODE> _PadMarkDifferentiation = new Element<PAD_MARK_DIFFERENTIATION_MODE>() { Value = PAD_MARK_DIFFERENTIATION_MODE.None };
        /// <summary>
        /// PMI 검사시 패드 이미지 전처리 옵션 선택        
        /// </summary>
        public Element<PAD_MARK_DIFFERENTIATION_MODE> PadMarkDifferentiation
        {
            get => _PadMarkDifferentiation;
            set
            {
                if(value!= _PadMarkDifferentiation)
                {
                    _PadMarkDifferentiation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _DelayInFirstGroup = new Element<int>();
        public Element<int> DelayInFirstGroup
        {
            get { return _DelayInFirstGroup; }
            set
            {
                if (value != _DelayInFirstGroup)
                {
                    _DelayInFirstGroup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _OrgImgCropEnable = new Element<bool>() { Value = true };
        public Element<bool> OrgImgCropEnable
        {
            get { return _OrgImgCropEnable; }
            set
            {
                if (value != _OrgImgCropEnable)
                {
                    _OrgImgCropEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<PMIImageCropMode> _OrgImgCropMode = new Element<PMIImageCropMode>();
        public Element<PMIImageCropMode> OrgImgCropMode
        {
            get { return _OrgImgCropMode; }
            set
            {
                if (value != _OrgImgCropMode)
                {
                    _OrgImgCropMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<double> _OrgImgCropSize = new Element<double>() { Value = 480 };
        public Element<double> OrgImgCropSize
        {
            get { return _OrgImgCropSize; }
            set
            {
                if (value != _OrgImgCropSize)
                {
                    _OrgImgCropSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _DelayAfterMoveToPad = new Element<int>();
        public Element<int> DelayAfterMoveToPad
        {
            get { return _DelayAfterMoveToPad; }
            set
            {
                if (value != _DelayAfterMoveToPad)
                {
                    _DelayAfterMoveToPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    if (NormalFocusParam == null)
                    {
                        NormalFocusParam = new NormalFocusParameter();
                    }

                    retVal = this.FocusManager().ValidationFocusParam(NormalFocusParam);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, NormalFocusParam);
                    }

                    if (BumpFocusParam == null)
                    {
                        BumpFocusParam = new NormalFocusParameter();
                    }

                    retVal = this.FocusManager().ValidationFocusParam(BumpFocusParam);

                    if (retVal != EventCodeEnum.NONE)
                    {
                        this.FocusManager().MakeDefalutFocusParam(EnumProberCam.WAFER_HIGH_CAM, EnumAxisConstants.Z, BumpFocusParam);
                    }

                    if (FTPInfo == null)
                    {
                        FTPInfo = new PMIFTP();
                    }

                    if (FailCodeInfo == null)
                    {
                        FailCodeInfo = new PMIFailCode();
                    }

                    if (LogInfo == null)
                    {
                        LogInfo = new PMILog();
                    }

                    if (TriggerComponent == null)
                    {
                        TriggerComponent = new PMITriggerComponent();
                    }

                    if (PadFindMarginX != null && PadFindMarginX.Value == 0)
                    {
                        PadFindMarginX.Value = 25;
                    }

                    if (PadFindMarginY != null && PadFindMarginY.Value == 0)
                    {
                        PadFindMarginY.Value = 25;
                    }

                    if (MarkAlignTriggerToleranceUsingPadOffsetX != null && MarkAlignTriggerToleranceUsingPadOffsetX.Value == 0)
                    {
                        MarkAlignTriggerToleranceUsingPadOffsetX.Value = 10;
                    }

                    if (MarkAlignTriggerToleranceUsingPadOffsetY != null && MarkAlignTriggerToleranceUsingPadOffsetY.Value == 0)
                    {
                        MarkAlignTriggerToleranceUsingPadOffsetY.Value = 10;
                    }

                    if (MarkOverlapPercentOfToleranceX != null && MarkOverlapPercentOfToleranceX.Value == 0)
                    {
                        MarkOverlapPercentOfToleranceX.Value = 80;
                    }

                    if (MarkOverlapPercentOfToleranceY != null && MarkOverlapPercentOfToleranceY.Value == 0)
                    {
                        MarkOverlapPercentOfToleranceY.Value = 80;
                    }

                    if (PauseMethod != null && PauseMethod.Value == PMI_PAUSE_METHOD.UNDEFINED)
                    {
                        PauseMethod.Value = PMI_PAUSE_METHOD.NOTUSE;
                    }

                    //PMI 이미지 전처리 옵션 마이그레이션
                    if (this.PadMarkDifferentiation.Value == PAD_MARK_DIFFERENTIATION_MODE.None)
                    {   
                        IPMIInfo PMIInfo = this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo();
                        
                        //기본값
                        this.PadMarkDifferentiation.Value = PAD_MARK_DIFFERENTIATION_MODE.SimpleThreshold;

                        //기존 옵션중 이미지 필터 사용시
                        if (this.PatternInPadMaskingFilterEnable.Value)
                        {
                            this.PadMarkDifferentiation.Value = PAD_MARK_DIFFERENTIATION_MODE.PadNoiseAwareThreshold;
                        }

                        // 레퍼런스 이미지 있는 경우
                        for (int i = 0; i < PMIInfo.PadTemplateInfo.Count; ++i)
                        {
                            string refernceFilePath = PMIModuleKeyWordInfo.PadTemplateReferencePath(this.FileManager(), i);
                            if (System.IO.File.Exists(refernceFilePath))
                            {
                                this.PadMarkDifferentiation.Value = PAD_MARK_DIFFERENTIATION_MODE.ReferenceImage;
                                break;
                            }
                        }
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }

                retVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                #region //..Set Default Values
                _NormalPMI.Value = OP_MODE.Disable;
                _OperationModeOnRetest.Value = OP_MODE_ON_RETEST.BeforeRetestOnly;
                _PauseMethod.Value = PMI_PAUSE_METHOD.NOTUSE;
                //_PauseImmediately.Value = false;
                _PausePadCntPerWafer.Value = 0;
                _PausePercentPerDie.Value = 0;
                _PauseContinuousFailDie.Value = 0;
                _MarkCompareMode.Value = MARK_COMPARE_MODE.Area;
                _MarkCompareUnit.Value = MARK_COMPARE_UNIT.Individual;
                _MarkAreaCalculateMode.Value = MARK_AREA_CALCULATE_MODE.Square;
                _MaximumMarkCnt.Value = 5;
                _PadEdgeBlockingValue.Value = 0;
                _MaximumBlobMarkCnt.Value = 10;
                //_PadGroupingMethod.Value            = GROUPING_METHOD.Multi;
                _PadGroupingMargin.Value = 10;
                _BallBumpDentTolerance.Value = 5;
                _MinMarkNoiseSizeX.Value = 2;
                _MinMarkNoiseSizeY.Value = 2;
                _MinMarkNoiseArea.Value = 4;
                _FocusingEnable.Value = false;
                _FocusingDutInterval.Value = 0;
                _AutoLightEnable.Value = false;
                _LightValue.Value = 100;
                _ObliqLightValue.Value = 100;
                _AutoLightStart.Value = 0;
                _AutoLightEnd.Value = 255;
                _AutoLightInterval.Value = 5;
                _DisplayPadDuringPMI.Value = false;
                _DisplayMarkDuringPMI.Value = false;

                _SearchPadEnable.Value = false;

                _ProcessingCamType.Value = EnumProberCam.WAFER_HIGH_CAM;

                if (LogInfo == null)
                {
                    LogInfo = new PMILog();
                }

                LogInfo.LoggingMode.Value = LOGGING_MODE.All;
                LogInfo.LogSaveHDDEnable.Value = false;
                LogInfo.LogSaveFTPEnable.Value = false;
                LogInfo.LogSaveNETEnable.Value = false;
                LogInfo.FailImageSaveHDDEnable.Value = false;
                LogInfo.FailImageSaveFTPEnable.Value = false;
                LogInfo.FailImageSaveNETEnable.Value = false;
                LogInfo.PassImageSaveHDDEnable.Value = false;
                LogInfo.PassImageSaveFTPEnable.Value = false;
                LogInfo.PassImageSaveNETEnable.Value = false;

                LogInfo.OriginalImageSaveHDDEnable.Value = false;

                if (FailCodeInfo == null)
                {
                    FailCodeInfo = new PMIFailCode();
                }

                FailCodeInfo.FailCodeEnableBondEdge.Value = false;
                FailCodeInfo.FailCodeEnableNoProbeMark.Value = false;
                FailCodeInfo.FailCodeEnableSmallMarkSize.Value = false;
                FailCodeInfo.FailCodeEnableBigMarkSize.Value = false;
                FailCodeInfo.FailCodeEnableSmallMarkArea.Value = false;
                FailCodeInfo.FailCodeEnableBigMarkArea.Value = false;
                FailCodeInfo.FailCodeEnableMarkCntOver.Value = false;
                FailCodeInfo.FailCodeEnableBallBumpDent.Value = false;

                if (FTPInfo == null)
                {
                    FTPInfo = new PMIFTP();
                }

                FTPInfo.FTPHostAddress.Value = @"\\192.168.1.5\\";
                FTPInfo.FTPUserID.Value = @"Admin";
                FTPInfo.FTPPassword.Value = @"12345";
                FTPInfo.LogPath.Value = @"\\PMI_Log\\";
                FTPInfo.FailImagePath.Value = @"\\Fail_Image\\";
                FTPInfo.PassImagePath.Value = @"\\Pass_Image\\";
                FTPInfo.NetworkDriveDirectory.Value = @"\\PMI_Log\\";

                OrgImgCropEnable.Value = true;
                OrgImgCropMode.Value = PMIImageCropMode.AUTO;
                OrgImgCropSize.Value = 480.0;

                #endregion

                #region //..Temporary Element Settings

                _PausePadCntPerWafer.LowerLimit = 0;
                _PausePadCntPerWafer.UpperLimit = 30000;
                _PausePercentPerDie.LowerLimit = 0;
                _PausePercentPerDie.UpperLimit = 99;
                _PauseContinuousFailDie.LowerLimit = 0;
                _PauseContinuousFailDie.UpperLimit = 9999;
                _MaximumMarkCnt.LowerLimit = 1;
                _MaximumMarkCnt.UpperLimit = 99;
                _PadEdgeBlockingValue.LowerLimit = 1;
                _PadEdgeBlockingValue.UpperLimit = 10;
                _MaximumBlobMarkCnt.LowerLimit = 1;
                _MaximumBlobMarkCnt.UpperLimit = 10;
                //_PadGroupingMethod.LowerLimit = GROUPING_METHOD.Multi;
                //_PadGroupingMethod.UpperLimit = GROUPING_METHOD.Single;
                _PadGroupingMargin.LowerLimit = 10;
                _PadGroupingMargin.UpperLimit = 100;
                _BallBumpDentTolerance.LowerLimit = 1;
                _BallBumpDentTolerance.UpperLimit = 100;
                _MinMarkNoiseSizeX.LowerLimit = 0;
                _MinMarkNoiseSizeX.UpperLimit = 15;
                _MinMarkNoiseSizeY.LowerLimit = 0;
                _MinMarkNoiseSizeY.UpperLimit = 15;
                _MinMarkNoiseArea.LowerLimit = 0;
                _MinMarkNoiseArea.UpperLimit = 15;
                _FocusingDutInterval.LowerLimit = 0;
                _FocusingDutInterval.UpperLimit = 4;            // Dut Site Num
                _LightValue.LowerLimit = 0;
                _LightValue.UpperLimit = 255;
                _ObliqLightValue.LowerLimit = 0;
                _ObliqLightValue.UpperLimit = 255;
                _AutoLightStart.LowerLimit = 0;
                _AutoLightStart.UpperLimit = 255;
                _AutoLightEnd.LowerLimit = 0;
                _AutoLightEnd.UpperLimit = 255;
                _AutoLightInterval.LowerLimit = 1;
                _AutoLightInterval.UpperLimit = 50;

                FTPInfo.FTPHostAddress.LowerLimit = 0;
                FTPInfo.FTPHostAddress.UpperLimit = 100;
                FTPInfo.FTPUserID.LowerLimit = 0;
                FTPInfo.FTPUserID.UpperLimit = 100;
                FTPInfo.FTPPassword.LowerLimit = 0;
                FTPInfo.FTPPassword.UpperLimit = 100;
                FTPInfo.LogPath.LowerLimit = 0;
                FTPInfo.LogPath.UpperLimit = 100;
                FTPInfo.FailImagePath.LowerLimit = 0;
                FTPInfo.FailImagePath.UpperLimit = 100;
                FTPInfo.PassImagePath.LowerLimit = 0;
                FTPInfo.PassImagePath.UpperLimit = 100;
                FTPInfo.NetworkDriveDirectory.LowerLimit = 0;
                FTPInfo.NetworkDriveDirectory.UpperLimit = 100;

                if (FocusingDllInfo == null)
                {
                    FocusingDllInfo = new PMIFocusingDLLInfo();
                }

                FocusingDllInfo.NormalFocusingInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();
                FocusingDllInfo.BumpFocusingInfo = FocusingDLLInfo.GetNomalFocusingDllInfo();

                if (NormalFocusParam == null)
                {
                    NormalFocusParam = new NormalFocusParameter();
                }

                if (BumpFocusParam == null)
                {
                    BumpFocusParam = new NormalFocusParameter();
                }


                #region Trigger

                if (TriggerComponent == null)
                {
                    TriggerComponent = new PMITriggerComponent();
                }

                TriggerComponent.EveryWaferInterval.Value = 0;
                TriggerComponent.TotalNumberOfWafersToPerform.Value = 0;
                TriggerComponent.TouchdownCountInterval.Value = 0;

                #endregion

                //FocusingDllInfo.NormalFocusingInfo = new ModuleDllInfo("Focusing", "Focusing.PMI", "PMINormalFocusing");
                //FocusingDllInfo.BumpFocusingInfo = new ModuleDllInfo("Focusing", "Focusing.PMI", "PMINormalFocusing");

                #endregion

                //..Normal End
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retVal = EventCodeEnum.PARAM_ERROR;
            }

            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = SetDefaultParam();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


}
