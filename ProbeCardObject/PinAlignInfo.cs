using System;
using System.Collections.Generic;

namespace ProbeCardObject
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using ProberInterfaces;
    using ProberInterfaces.PinAlign;

    [Serializable]
    public class PinAlignInfo : INotifyPropertyChanged, IPinAlignInfo
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

        //[JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        //[ParamIgnore]
        //[XmlIgnore, JsonIgnore]
        public PinAlignResultes AlignResult { get; set; } = new PinAlignResultes();

        //[ParamIgnore]
        //[XmlIgnore, JsonIgnore]
        public PinAlignProcVariables AlignProcInfo { get; set; } = new PinAlignProcVariables();

        //[ParamIgnore]
        //[XmlIgnore, JsonIgnore]
        //public ObservableCollection<PinAlignDisplayData> DisplayData { get; set; } = new ObservableCollection<PinAlignDisplayData>();

        //[ParamIgnore]
        //[XmlIgnore, JsonIgnore]
        //public PinAlignLastAlignResult AlignLastResult { get; set; } = new PinAlignLastAlignResult();

        public PinAlignInfo()
        {
        }
    }


    public class PinAlignProcVariables
    {
        public PinAlignProcVariables()
        {
            _ProcSampleAlign = SAMPLEPINALGINRESULT.CONTINUE;
        }

        public PinAlignProcVariables(PinAlignProcVariables variable)
        {
            _ProcSampleAlign = variable.ProcSampleAlign;

            foreach (int item in variable.RequiredAlignList)
            {
                _RequiredAlignList.Add(item);
            }
        }

        //[XmlIgnore, JsonIgnore]
        private SAMPLEPINALGINRESULT _ProcSampleAlign;
        public SAMPLEPINALGINRESULT ProcSampleAlign
        {
            get { return _ProcSampleAlign; }
            set { _ProcSampleAlign = value; }
        }

        //[XmlIgnore, JsonIgnore]
        private List<int> _RequiredAlignList = new List<int>();
        public List<int> RequiredAlignList
        {
            get { return _RequiredAlignList; }
            set { _RequiredAlignList = value; }
        }
    }

    //[Serializable]
    //public class PinAlignDisplayData : INotifyPropertyChanged
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion

    //    public PinAlignDisplayData()
    //    {

    //    }
    //    public PinAlignDisplayData(int PinNumber, int DifferenceX, int DifferenceY, int DifferenceZ, PINALIGNRESULT Result)
    //    {
    //        try
    //        {
    //            _PinNumber = PinNumber;
    //            _DifferenceX = DifferenceX;
    //            _DifferenceY = DifferenceY;
    //            _DifferenceZ = DifferenceZ;                
    //            _Result = Result;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //            throw;
    //        }
    //    }

    //    #region ==> PinNumber
    //    //[XmlIgnore, JsonIgnore]
    //    private int _PinNumber;
    //    public int PinNumber
    //    {
    //        get { return _PinNumber; }
    //        set
    //        {
    //            if (value != _PinNumber)
    //            {
    //                _PinNumber = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    #endregion

    //    #region ==> Result
    //    //[XmlIgnore, JsonIgnore]
    //    private PINALIGNRESULT _Result;
    //    public PINALIGNRESULT Result
    //    {
    //        get { return _Result; }
    //        set
    //        {
    //            if (value != _Result)
    //            {
    //                _Result = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    #endregion

    //    #region ==> DifferenceX
    //    //[XmlIgnore, JsonIgnore]
    //    private int _DifferenceX;
    //    public int DifferenceX
    //    {
    //        get { return _DifferenceX; }
    //        set
    //        {
    //            if (value != _DifferenceX)
    //            {
    //                _DifferenceX = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    #endregion

    //    #region ==> DifferenceY
    //    //[XmlIgnore, JsonIgnore]
    //    private int _DifferenceY;
    //    public int DifferenceY
    //    {
    //        get { return _DifferenceY; }
    //        set
    //        {
    //            if (value != _DifferenceY)
    //            {
    //                _DifferenceY = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    #endregion

    //    #region ==> DifferenceZ
    //    //[XmlIgnore, JsonIgnore]
    //    private int _DifferenceZ;
    //    public int DifferenceZ
    //    {
    //        get { return _DifferenceZ; }
    //        set
    //        {
    //            if (value != _DifferenceZ)
    //            {
    //                _DifferenceZ = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //    #endregion
                
    //}

    //[Serializable]
    //public class PinAlignLastAlignResult : INotifyPropertyChanged
    //{
    //    #region ==> PropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion

    //    public PinAlignLastAlignResult()
    //    {

    //    }
    //    public PinAlignLastAlignResult(long totalProc, long totalFail, long totalPass, int passPercentage)
    //    {
    //        try
    //        {
    //            _TotalAlignPinCount = totalProc;
    //            _TotalFailPinCount = totalFail;
    //            _TotalPassPinCount = totalPass;
    //            _PassPercentage = passPercentage;
    //        }
    //        catch (Exception err)
    //        {
    //            LoggerManager.Exception(err);
    //            throw;
    //        }
    //    }

    //    //[XmlIgnore, JsonIgnore]
    //    private long _TotalAlignPinCount;
    //    public long TotalAlignPinCount
    //    {
    //        get { return _TotalAlignPinCount; }
    //        set
    //        {
    //            if (value != _TotalAlignPinCount)
    //            {
    //                _TotalAlignPinCount = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    //[XmlIgnore, JsonIgnore]
    //    private long _TotalFailPinCount;
    //    public long TotalFailPinCount
    //    {
    //        get { return _TotalFailPinCount; }
    //        set
    //        {
    //            if (value != _TotalFailPinCount)
    //            {
    //                _TotalFailPinCount = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    //[XmlIgnore, JsonIgnore]
    //    private long _TotalPassPinCount;
    //    public long TotalPassPinCount
    //    {
    //        get { return _TotalPassPinCount; }
    //        set
    //        {
    //            if (value != _TotalPassPinCount)
    //            {
    //                _TotalPassPinCount = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }

    //    //[XmlIgnore, JsonIgnore]
    //    private int _PassPercentage;
    //    public int PassPercentage
    //    {
    //        get { return _PassPercentage; }
    //        set
    //        {
    //            if (value != _PassPercentage)
    //            {
    //                _PassPercentage = value;
    //                RaisePropertyChanged();
    //            }
    //        }
    //    }
    //}


}
