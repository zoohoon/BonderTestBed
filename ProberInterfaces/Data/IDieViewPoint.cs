using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.Param;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProberInterfaces
{
    public  interface IDieViewPoint
    {
        DeviceParams DeviceParam { get; set; }
        CatCoordinates CalcViewPoint(CatCoordinates relPos, IndexCoord curIndex);
    }

    public class DeviceParams : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private IndexInfo _IndexInfo;
        public IndexInfo IndexInfo
        {
            get { return _IndexInfo; }
            set
            {
                if (value != _IndexInfo)
                {
                    _IndexInfo = value;
                    NotifyPropertyChanged("IndexInfo");
                }
            }
        }

        
        //public WaferAlignInfo WaferAlignInfo;

        public void SetDefaultParams()
        {
            try
            {
            _IndexInfo = new IndexInfo();

            _IndexInfo.DirX = 1;
            _IndexInfo.DirY = 1;
            _IndexInfo.RefUX = 0;
            _IndexInfo.RefUY = 0;

            //WaferAlignInfo = new WaferAlignInfo();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public DeviceParams()
        {

        }

    }

    public class IndexInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private double _SizeX;
        public double SizeX
        {
            get { return _SizeX; }
            set
            {
                if (value != _SizeX)
                {
                    _SizeX = value;
                    NotifyPropertyChanged("SizeX");
                }
            }
        }

        private double _SizeY;
        public double SizeY
        {
            get { return _SizeY; }
            set
            {
                if (value != _SizeY)
                {
                    _SizeY = value;
                    NotifyPropertyChanged("SizeY");
                }
            }
        }

        private int _OrgMX;
        public int OrgMX
        {
            get { return _OrgMX; }
            set
            {
                if (value != _OrgMX)
                {
                    _OrgMX = value;
                    NotifyPropertyChanged("OrgMX");
                }
            }
        }

        private int _OrgMY;
        public int OrgMY
        {
            get { return _OrgMY; }
            set
            {
                if (value != _OrgMY)
                {
                    _OrgMY = value;
                    NotifyPropertyChanged("OrgMY");
                }
            }
        }

        private int _OrgUX;
        public int OrgUX
        {
            get { return _OrgUX; }
            set
            {
                if (value != _OrgUX)
                {
                    _OrgUX = value;
                    NotifyPropertyChanged("OrgUX");
                }
            }
        }

        private int _OrgUY;
        public int OrgUY
        {
            get { return _OrgUY; }
            set
            {
                if (value != _OrgUY)
                {
                    _OrgUY = value;
                    NotifyPropertyChanged("OrgUY");
                }
            }
        }

        private int _DirX;
        public int DirX
        {
            get { return _DirX; }
            set
            {
                if (value != _DirX)
                {
                    _DirX = value;
                    NotifyPropertyChanged("DirX");
                }
            }
        }

        private int _DirY;
        public int DirY
        {
            get { return _DirY; }
            set
            {
                if (value != _DirY)
                {
                    _DirY = value;
                    NotifyPropertyChanged("DirY");
                }
            }
        }

        private int _NumX;
        public int NumX
        {
            get { return _NumX; }
            set
            {
                if (value != _NumX)
                {
                    _NumX = value;
                    NotifyPropertyChanged("NumX");
                }
            }
        }

        private int _NumY;
        public int NumY
        {
            get { return _NumY; }
            set
            {
                if (value != _NumY)
                {
                    _NumY = value;
                    NotifyPropertyChanged("NumY");
                }
            }
        }

        private int[] _RefM = new int[2];
        public int[] RefM
        {
            get { return _RefM; }
            set
            {
                if (value != _RefM)
                {
                    _RefM = value;
                    NotifyPropertyChanged("RefM");
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        private int _RefUX;
        public int RefUX
        {
            get { return _RefUX; }
            set
            {
                if (value != _RefUX)
                {
                    _RefUX = value;
                    NotifyPropertyChanged("RefUX");
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        private int _RefUY;
        public int RefUY
        {
            get { return _RefUY; }
            set
            {
                if (value != _RefUY)
                {
                    _RefUY = value;
                    NotifyPropertyChanged("RefUY");
                }
            }
        }

        //private int[] _RefU = new int[2];
        //public int[] RefU
        //{
        //    get { return _RefU; }
        //    set
        //    {
        //        if (value != _RefU)
        //        {
        //            _RefU = value;
        //            NotifyPropertyChanged("RefU");
        //        }
        //    }
        //}

        private int[] _TchM = new int[2];
        public int[] TchM
        {
            get { return _TchM; }
            set
            {
                if (value != _TchM)
                {
                    _TchM = value;
                    NotifyPropertyChanged("TchM");
                }
            }
        }

        private int[] _TchU = new int[2];
        public int[] TchU
        {
            get { return _TchU; }
            set
            {
                if (value != _TchU)
                {
                    _TchU = value;
                    NotifyPropertyChanged("TchU");
                }
            }
        }
    }

    public class WaferAlignInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private double[] _PadOrg = new double[2];
        public double[] PadOrg
        {
            get { return _PadOrg; }
            set
            {
                if (value != _PadOrg)
                {
                    _PadOrg = value;
                    NotifyPropertyChanged("PadOrg");
                }
            }
        }

        private double[] _PadOrgRelPosFromLL = new double[2];
        public double[] PadOrgRelPosFromLL
        {
            get { return _PadOrgRelPosFromLL; }
            set
            {
                if (value != _PadOrgRelPosFromLL)
                {
                    _PadOrgRelPosFromLL = value;
                    NotifyPropertyChanged("PadOrgRelPosFromLL");
                }
            }
        }

    }
}
