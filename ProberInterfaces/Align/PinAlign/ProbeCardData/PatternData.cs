using LogModule;
using ProberInterfaces.Param;
using System;
using System.ComponentModel;

namespace ProberInterfaces.PinAlign.ProbeCardData
{
    public interface IPatternData
    {
        string FileName { get; set; }
        int PatternSerialNumber { get; set; }
        double Score { get; set; }        
        PinCoordinate PatternPos { get; set; }   
        PinCoordinate OffsetToRefPin { get; set; }
        PINALIGNRESULT Result { get; set; }
    }
    [Serializable]
    public class PatternData : IPatternData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private string _FileName;
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        private int _PatternSerialNumber;
        public int PatternSerialNumber
        {
            get { return _PatternSerialNumber; }
            set
            {
                if (value != _PatternSerialNumber)
                {
                    _PatternSerialNumber = value;
                    NotifyPropertyChanged("PatternSerialNumber");
                }
            }
        }
        private double _Score;
        public double Score
        {
            get { return _Score; }
            set
            {
                if (value != _Score)
                {
                    _Score = value;
                    NotifyPropertyChanged("Score");
                }
            }
        }

        private PinCoordinate _PatternPos;
        public PinCoordinate PatternPos
        {
            get { return _PatternPos; }
            set
            {
                if (value != _PatternPos)
                {
                    _PatternPos = value;
                    NotifyPropertyChanged("PatternPos");
                }
            }
        }

        private PinCoordinate _OffsetToRefPin;
        public PinCoordinate OffsetToRefPin
        {
            get { return _OffsetToRefPin; }
            set
            {
                if (value != _OffsetToRefPin)
                {
                    _OffsetToRefPin = value;
                    NotifyPropertyChanged("OffsetToRefPin");
                }
            }
        }


        private PINALIGNRESULT _Result;
        public PINALIGNRESULT Result
        {
            get { return _Result; }
            set
            {
                if (value != _Result)
                {
                    _Result = value;
                    NotifyPropertyChanged("Result");
                }
            }
        }
        public PatternData(PatternData param)
        {
            try
            {
            _PatternSerialNumber = param.PatternSerialNumber;
            _Score = param.Score;
            _PatternPos = new PinCoordinate(param.PatternPos);
            _Result = PINALIGNRESULT.PIN_NOT_PERFORMED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public PatternData()
        {
           
        }
    }
}
