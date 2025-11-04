using LogModule;
using System;

namespace ProberInterfaces.WaferAlign
{
    using ProberInterfaces.Align;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;
    using ProberInterfaces.Param;
    using ProberInterfaces.WaferAlignEX;
    using ProberErrorCode;
    using Newtonsoft.Json;

    [Serializable]
    public abstract class WaferAlignProcessBase : AlignProcessBase
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public override abstract EventCodeEnum UpdateResources();
        public override abstract EventCodeEnum Process();


        private WaferProcParametricTable _Param;
        public WaferProcParametricTable Params
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    NotifyPropertyChanged("Param");
                }
            }
        }

        private WaferProcResult _Result;
        [XmlIgnore, JsonIgnore]
        public WaferProcResult Result
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

        
        private ObservableCollection<WaferAlignProcessAcq> _Acquititions;
        public ObservableCollection<WaferAlignProcessAcq> Acquititions
        {
            get { return _Acquititions; }
            set
            {
                if (value != _Acquititions)
                {
                    _Acquititions = value;
                    NotifyPropertyChanged("Acquititions");
                }
            }
        }
        public WaferAlignProcessBase()
        {
            try
            {
            Params = new WaferProcParametricTable();
            Result = new WaferProcResult();
            Acquititions = new ObservableCollection<WaferAlignProcessAcq>();
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

            Params = new WaferProcParametricTable();
            Result = new WaferProcResult();
            Acquititions = new ObservableCollection<WaferAlignProcessAcq>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public abstract void InitState();

    }
}
