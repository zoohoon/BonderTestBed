using System;

namespace ProberInterfaces.Align
{
    using System.ComponentModel;
    using System.Xml.Serialization;
    using ProberErrorCode;
    using Newtonsoft.Json;

    public interface IAlignProcess : INotifyPropertyChanged
    {
        //ProcParametricTable Param { get; }
        //ProcResult Result { get; }
        AlignProcessStateBase State { get; }
        void StateTransition(AlignProcessStateBase state);
        //ObservableCollection<IProcessAcq> Acquititions { get; }
        //ObservableCollection<AlignProcResource> AlignResources { get; }
        EventCodeEnum Process();
    }


    [Serializable]
    public abstract class AlignProcessBase : IAlignProcess
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private Autofac.IContainer _Container;
        [XmlIgnore, JsonIgnore]
        public Autofac.IContainer Container
        {
            get { return _Container; }
            protected set { _Container = value; }
        }


        public void StateTransition(AlignProcessStateBase state)
        {
            State = state;
        }

        public abstract EventCodeEnum Process();


        public abstract EventCodeEnum UpdateResources();

        public abstract EventCodeEnum InitModule(Autofac.IContainer container);
        protected void SetState(AlignProcessStateBase state)
        {
            this.State = state;
        }

        public EventCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }

        private AlignProcessStateBase _State;
        [XmlIgnore, JsonIgnore]
        public AlignProcessStateBase State
        {
            get { return _State; }
            private set
            {
                if (value != _State)
                {
                    _State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }
    }


}
