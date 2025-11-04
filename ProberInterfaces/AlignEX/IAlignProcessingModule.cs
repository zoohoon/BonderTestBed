using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProberInterfaces.AlignEX
{
    using Autofac;
    using ProberErrorCode;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Error;
    using ProberInterfaces.WaferAlignEX;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public interface IAlignProcessingModule : IFactoryModule
    {
        AlignModuleProcessStateBase ProcState { get; set; }
        ErrorCodeEnum Run(AlginParamBase param);
    }



    [Serializable]
    public abstract class AlignProcessingModuleBase : IAlignProcessingModule
    {
        [XmlIgnore]
        public int InitPriority { get; set; }
        public AlignModuleProcessStateBase ProcState { get; set; }

        public abstract ErrorCodeEnum InitModule(Autofac.IContainer container);

        public ErrorCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }

        public ErrorCodeEnum Run()
        {
            throw new NotImplementedException();
        }

        public ErrorCodeEnum Run(AlginParamBase param)
        {
            throw new NotImplementedException();
        }
        public void DeInitModule()
        {

        }
        public ErrorCodeEnum SetContainer(Autofac.IContainer container)
        {
            throw new NotImplementedException();
        }
    }


    //public abstract class EdgeProcessingModuleBase : IAlignProcessingModule, INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void NotifyPropertyChanged(String info)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(info));
    //        }
    //    }
    //    private AlignModuleProcessStateBase _ProcState;
    //    public AlignModuleProcessStateBase ProcState
    //    {
    //        get { return _ProcState; }
    //        set
    //        {
    //            if (value != _ProcState)
    //            {
    //                _ProcState = value;
    //                NotifyPropertyChanged("ProcState");
    //            }
    //        }
    //    }

    //    public int InitPriority { get; }

    //    public ErrorCodeEnum InitModule(Autofac.IContainer container)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public abstract ErrorCodeEnum Run(AlginParamBase param);
    //}


}
