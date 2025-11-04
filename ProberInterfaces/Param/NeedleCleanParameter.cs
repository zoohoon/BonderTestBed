using LogModule;
using System;
using System.ComponentModel;

namespace ProberInterfaces.Param
{
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;

    [Serializable]
    [XmlInclude(typeof(NCPadParam))]
    public class NCPadParam:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public NCPadParam() { }
        private CatCoordinates _Offset = new CatCoordinates();
        public CatCoordinates Offset
        {
            get { return _Offset; }
            set
            {
                if (value != _Offset)
                {
                    _Offset = value;
                    NotifyPropertyChanged("Offset");
                }
            }
        }

        private CatCoordinates _Range = new CatCoordinates();
        public CatCoordinates Range
        {
            get { return _Range; }
            set
            {
                if (value != _Range)
                {
                    _Range = value;
                    NotifyPropertyChanged("Range");
                }
            }
        }
        private CatCoordinates _DiskPos = new CatCoordinates();
        public CatCoordinates DiskPos
        {
            get { return _DiskPos; }
            set
            {
                if (value != _DiskPos)
                {
                    _DiskPos = value;
                    NotifyPropertyChanged("DiskPos");
                }
            }
        }

    }
    [Serializable]
    [XmlInclude(typeof(NeedleCleanParameter))]
    public class NeedleCleanParameter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #region


        private ObservableCollection<NCPadParam> _NCPadParam;
        public ObservableCollection<NCPadParam> NCPadParam
        {
            get { return _NCPadParam; }
            set
            {
                if (value != _NCPadParam)
                {
                    _NCPadParam = value;
                    NotifyPropertyChanged("NCPadParam ");
                }
            }
        }


        private FocusParameter _NeedleFocusParam;
        public FocusParameter NeedleFocusParam
        {
            get { return _NeedleFocusParam; }
            set
            {
                if (value != _NeedleFocusParam)
                {
                    _NeedleFocusParam = value;
                    NotifyPropertyChanged("NeedleFocusParam ");
                }
            }
        }

        private CatCoordinates _NCResultFocusParam;
        public CatCoordinates NCResultFocusParam
        {
            get { return _NCResultFocusParam; }
            set
            {
                if(value != _NCResultFocusParam)
                {
                    _NCResultFocusParam = value;
                    NotifyPropertyChanged("NCResultFocusParam");
                }
            }
        }

        private ObservableCollection<NCCoordinate> _NCPadPos;
        public ObservableCollection<NCCoordinate> NCPadPos
        {
            get { return _NCPadPos; }
            set
            {
                if (value != _NCPadPos)
                {
                    _NCPadPos = value;
                    NotifyPropertyChanged("NCPadPos");
                }
            }
        }



        public NeedleCleanParameter() { }
        private void setadd()
        {
            try
            {
           

            NCPadParam ncpadparam1 = new NCPadParam();
            NCPadParam ncpadparam2 = new NCPadParam();
            NCPadParam ncpadparam3 = new NCPadParam();
            ncpadparam1.DiskPos.X.Value = 0;
            ncpadparam1.DiskPos.Y.Value = -200000;
            ncpadparam1.DiskPos.Z.Value = 8327;
            ncpadparam1.Offset.X.Value = 0;
            ncpadparam1.Offset.Y.Value = -50000;
            ncpadparam1.Range.X.Value = 80000;
            ncpadparam1.Range.Y.Value = 40000;
            NCPadParam.Add(ncpadparam1);


            ncpadparam2.DiskPos.X.Value = 0;
            ncpadparam2.DiskPos.Y.Value = -200000;
            ncpadparam2.DiskPos.Z.Value = 7943;
            ncpadparam2.Offset.X.Value = 45000;
            ncpadparam2.Offset.Y.Value = -50000;
            ncpadparam2.Range.X.Value = 40000;
            ncpadparam2.Range.Y.Value = 40000;
            NCPadParam.Add(ncpadparam2);

            ncpadparam3.DiskPos.X.Value = 0;
            ncpadparam3.DiskPos.Y.Value = -200000;
            ncpadparam3.DiskPos.Z.Value = 7943;
            ncpadparam3.Offset.X.Value = -45000;
            ncpadparam3.Offset.Y.Value = -50000;
            ncpadparam3.Range.X.Value = 40000;
            ncpadparam3.Range.Y.Value = 40000;
            NCPadParam.Add(ncpadparam3);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public void DefaultSetting()
        {
            try
            {
            
             //NeedleFocusParam = new NeedleFocusParameter();
            _NCResultFocusParam = new CatCoordinates();
            _NCPadPos = new ObservableCollection<NCCoordinate>();
            _NCPadParam = new ObservableCollection<Param.NCPadParam>();

            setadd();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        #endregion

    }
}
