using System;

namespace ProberInterfaces.DutControl
{
    using System.ComponentModel;
    public abstract class DutControlBase : INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class DutMapControl : DutControlBase
    {

        //public DutMapControl(IStageSupervisor stageSupervisor)
        //{
        //    ProbeCard = stageSupervisor.ProbeCardInfo;
        //    CoordinateManager = stageSupervisor.CoordinateManager;
        //}

        //private ICoordinateManager _CoordinateManager;
        //public ICoordinateManager CoordinateManager
        //{
        //    get { return _CoordinateManager; }
        //    set
        //    {
        //        if (value != _CoordinateManager)
        //        {
        //            _CoordinateManager = value;
        //            NotifyPropertyChanged("CoordinateManager");
        //        }
        //    }
        //}

        //private IProbeCard _ProbeCard;
        //public IProbeCard ProbeCard
        //{
        //    get { return _ProbeCard; }
        //    set
        //    {
        //        if (value != _ProbeCard)
        //        {
        //            _ProbeCard = value;
        //            NotifyPropertyChanged("ProbeCard");
        //        }
        //    }
        //}
    }
    public class DutEditControl : DutControlBase
    {
        //public DutEditControl(IStageSupervisor stageSupervisor)
        //{
        //    ProbeCard = stageSupervisor.ProbeCardInfo;
        //}

        //private IProbeCard _ProbeCard;
        //public IProbeCard ProbeCard
        //{
        //    get { return _ProbeCard; }
        //    set
        //    {
        //        if (value != _ProbeCard)
        //        {
        //            _ProbeCard = value;
        //            NotifyPropertyChanged("ProbeCard");
        //        }
        //    }
        //}
    }
    public class PTPAControl : DutControlBase
    {

        //public PTPAControl(IStageSupervisor stageSupervisor)
        //{
        //    ProbeCard = stageSupervisor.ProbeCardInfo;
        //    CoordinateManager = stageSupervisor.CoordinateManager;
        //}

        //private ICoordinateManager _CoordinateManager;
        //public ICoordinateManager CoordinateManager
        //{
        //    get { return _CoordinateManager; }
        //    set
        //    {
        //        if (value != _CoordinateManager)
        //        {
        //            _CoordinateManager = value;
        //            NotifyPropertyChanged("CoordinateManager");
        //        }
        //    }
        //}

        //private IProbeCard _ProbeCard;
        //public IProbeCard ProbeCard
        //{
        //    get { return _ProbeCard; }
        //    set
        //    {
        //        if (value != _ProbeCard)
        //        {
        //            _ProbeCard = value;
        //            NotifyPropertyChanged("ProbeCard");
        //        }
        //    }
        //}
    }
    public class PTPA2Control : DutControlBase
    {

        //public PTPA2Control(IStageSupervisor stageSupervisor)
        //{
        //    ProbeCard = stageSupervisor.ProbeCardInfo;
        //    CoordinateManager = stageSupervisor.CoordinateManager;
        //}

        //private ICoordinateManager _CoordinateManager;
        //public ICoordinateManager CoordinateManager
        //{
        //    get { return _CoordinateManager; }
        //    set
        //    {
        //        if (value != _CoordinateManager)
        //        {
        //            _CoordinateManager = value;
        //            NotifyPropertyChanged("CoordinateManager");
        //        }
        //    }
        //}

        //private IProbeCard _ProbeCard;
        //public IProbeCard ProbeCard
        //{
        //    get { return _ProbeCard; }
        //    set
        //    {
        //        if (value != _ProbeCard)
        //        {
        //            _ProbeCard = value;
        //            NotifyPropertyChanged("ProbeCard");
        //        }
        //    }
        //}
    }
    public class PTPA3Control : DutControlBase
    {

        //public PTPA3Control(IStageSupervisor stageSupervisor)
        //{
        //    ProbeCard = stageSupervisor.ProbeCardInfo;
        //    CoordinateManager = stageSupervisor.CoordinateManager;
        //}

        //private ICoordinateManager _CoordinateManager;
        //public ICoordinateManager CoordinateManager
        //{
        //    get { return _CoordinateManager; }
        //    set
        //    {
        //        if (value != _CoordinateManager)
        //        {
        //            _CoordinateManager = value;
        //            NotifyPropertyChanged("CoordinateManager");
        //        }
        //    }
        //}

        //private IProbeCard _ProbeCard;
        //public IProbeCard ProbeCard
        //{
        //    get { return _ProbeCard; }
        //    set
        //    {
        //        if (value != _ProbeCard)
        //        {
        //            _ProbeCard = value;
        //            NotifyPropertyChanged("ProbeCard");
        //        }
        //    }
        //}
    }
}
