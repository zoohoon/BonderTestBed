using LogModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ErrorMapping
{
    public class ErrorDataTable : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public ErrorDataTable()
        {
            try
            {
            _ErrorData_HOR = new ErrorDataList();
            _ErrorData_VER = new ErrorDataList();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private ErrorDataList _ErrorData_HOR;
        public ErrorDataList ErrorData_HOR
        {
            get { return _ErrorData_HOR; }
            set
            {
                if (value != _ErrorData_HOR)
                {
                    _ErrorData_HOR = value;
                    NotifyPropertyChanged("ErrorData_X");
                }
            }
        }

        private ErrorDataList _ErrorData_VER;
        public ErrorDataList ErrorData_VER
        {
            get { return _ErrorData_VER; }
            set
            {
                if (value != _ErrorData_VER)
                {
                    _ErrorData_VER = value;
                    NotifyPropertyChanged("ErrorData_Y");
                }
            }
        }


    }

    public class ErrorDataList : List<ErrorData>
    {
        public List<double> GetPosXList()
        {
            return this.OrderBy(o => o.XPos).ToList().Select(item => item.XPos).ToList();
        }
        public List<double> GetPosYList()
        {
            return this.OrderBy(o => o.YPos).ToList().Select(item => item.YPos).ToList();
        }
        public List<double> GetOffsetXList()
        {
            return this.OrderBy(o => o.XPos).ToList().Select(item => item.ErrorValue).ToList();
        }
        public List<double> GetOffsetYList()
        {
            return this.OrderBy(o => o.YPos).ToList().Select(item => item.ErrorValue).ToList();
        }
    
    }
    public class ErrorData : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public ErrorData()
        {

        }
        public ErrorData(double errValue, double xPos, double yPos, double zPos, double relPosX, double relPosY)
        {
            _ErrorValue = errValue;
            _XPos = xPos;
            _YPos = yPos;
            _ZPos = zPos;
            _RelPosX = relPosX;
            _RelPosY = relPosY;

        }
        private double _ErrorValue;
        [XmlAttribute("ErrorValue")]
        public double ErrorValue
        {
            get { return _ErrorValue; }
            set
            {
                if (value != _ErrorValue)
                {
                    _ErrorValue = value;
                    NotifyPropertyChanged("ErrorValue");
                }
            }
        }

        private double _XPos;
        [XmlAttribute("XPos")]
        public double XPos
        {
            get { return _XPos; }
            set
            {
                if (value != _XPos)
                {
                    _XPos = value;
                    NotifyPropertyChanged("XPos");
                }
            }
        }

        private double _YPos;
        [XmlAttribute("YPos")]
        public double YPos
        {
            get { return _YPos; }
            set
            {
                if (value != _YPos)
                {
                    _YPos = value;
                    NotifyPropertyChanged("YPos");
                }
            }
        }

        private double _ZPos;
        [XmlAttribute("ZPos")]
        public double ZPos
        {
            get { return _ZPos; }
            set
            {
                if (value != _ZPos)
                {
                    _ZPos = value;
                    NotifyPropertyChanged("ZPos");
                }
            }
        }

        private double _RelPosX;
        [XmlAttribute("RelPosX")]
        public double RelPosX
        {
            get { return _RelPosX; }
            set
            {
                if (value != _RelPosX)
                {
                    _RelPosX = value;
                    NotifyPropertyChanged("RelPosX");
                }
            }
        }

        private double _RelPosY;
        [XmlAttribute("RelPosY")]
        public double RelPosY
        {
            get { return _RelPosY; }
            set
            {
                if (value != _RelPosY)
                {
                    _RelPosY = value;
                    NotifyPropertyChanged("RelPosY");
                }
            }
        }

    }



}
