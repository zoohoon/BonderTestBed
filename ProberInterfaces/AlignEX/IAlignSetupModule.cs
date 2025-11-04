using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.AlignEX
{
    using ProberInterfaces.Align;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Error;
    using Autofac;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using ProberErrorCode;

    public interface IAlignSetupModule : IFactoryModule
    {
        AlignModuleSetupStateBase SetupState { get; set; }
        bool UseRectProp { get; set; }
        double TargetRectangleLeft { get; set; }
        double TargetRectangleTop { get; set; }
        double TargetRectangleWidth { get; set; }
        double TargetRectangleHeight { get; set; }
        ErrorCodeEnum LoadingModule();
        ErrorCodeEnum Run();
        ErrorCodeEnum ReSet();
    }

    //public abstract class AlignSetupModuleBase : IAlignSetupModule
    //{
    //    public abstract int InitPriority { get; set; }

    //    public abstract AlignModuleSetupStateBase SetupState { get; set; }

    //    public abstract ErrorCodeEnum InitModule(Autofac.IContainer container);

    //    public abstract ErrorCodeEnum ReSet();

    //    public abstract ErrorCodeEnum Run();
    //}

    public abstract class URectAlignSetupModuleBase : IAlignSetupModule, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        private bool _UseRectProp;

        public bool UseRectProp
        {
            get { return _UseRectProp; }
            set { _UseRectProp = value; }
        }


        #region ==> UcDispaly Port Target Rectangle
        private double _TargetRectangleLeft;
        public double TargetRectangleLeft
        {
            get { return _TargetRectangleLeft; }
            set
            {
                if (value != _TargetRectangleLeft)
                {
                    _TargetRectangleLeft = value;
                    NotifyPropertyChanged("TargetRectangleLeft");
                }
            }
        }

        private double _TargetRectangleTop;
        public double TargetRectangleTop
        {
            get { return _TargetRectangleTop; }
            set
            {
                if (value != _TargetRectangleTop)
                {
                    _TargetRectangleTop = value;
                    NotifyPropertyChanged("TargetRectangleTop");
                }
            }
        }

        private double _TargetRectangleWidth;
        public double TargetRectangleWidth
        {
            get { return _TargetRectangleWidth; }
            set
            {
                if (value != _TargetRectangleWidth)
                {
                    _TargetRectangleWidth = value;
                    NotifyPropertyChanged("TargetRectangleWidth");
                }
            }
        }

        private double _TargetRectangleHeight;
        public double TargetRectangleHeight
        {
            get { return _TargetRectangleHeight; }
            set
            {
                if (value != _TargetRectangleHeight)
                {
                    _TargetRectangleHeight = value;
                    NotifyPropertyChanged("TargetRectangleHeight");
                }
            }
        }
        #endregion


        public abstract int InitPriority { get; set; }

        public abstract AlignModuleSetupStateBase SetupState { get; set; }
        public void DeInitModule()
        {

        }

        public URectAlignSetupModuleBase()
        {
            UseRectProp = true;
        }

        public abstract ErrorCodeEnum InitModule(Autofac.IContainer container);

        public abstract ErrorCodeEnum LoadingModule();

        public abstract ErrorCodeEnum ReSet();

        public abstract ErrorCodeEnum Run();

        public ErrorCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            throw new NotImplementedException();
        }

        public ErrorCodeEnum SetContainer(Autofac.IContainer container)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class IRectAlignSetupModuleBase : IAlignSetupModule
    {

        private bool _UseRectProp;

        public bool UseRectProp
        {
            get { return _UseRectProp; }
            set { _UseRectProp = value; }
        }
        public double TargetRectangleLeft { get; set; }
        public double TargetRectangleTop { get; set; }
        public double TargetRectangleWidth { get; set; }
        public double TargetRectangleHeight { get; set; }

        public abstract int InitPriority { get; set; }

        public abstract AlignModuleSetupStateBase SetupState { get; set; }

        public IRectAlignSetupModuleBase()
        {
            UseRectProp = false;
        }

        public abstract ErrorCodeEnum InitModule(Autofac.IContainer container);

        public abstract ErrorCodeEnum LoadingModule();

        public abstract ErrorCodeEnum ReSet();

        public abstract ErrorCodeEnum Run();

        public ErrorCodeEnum InitModule(Autofac.IContainer container, object param)
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
}
