using System;
using System.ComponentModel;

namespace ProberInterfaces.State
{
    public enum MovingStateEnum
    {
        STOP,
        MOVING
    }

    [Serializable]
    public abstract class SubModuleMovingStateBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public SubModuleMovingStateBase()
        {

        }
     
     
        public SubModuleMovingStateBase(IProcessingModule module)
        {
            Module = module;
        }
        private IProcessingModule _Module;

        public IProcessingModule Module
        {
            get { return _Module; }
            set { _Module = value; }
        }


        public abstract MovingStateEnum GetState();
        public abstract void Moving();

        public abstract void Stop();


    }

    [Serializable]
    public class SubModuleStopState : SubModuleMovingStateBase
    {
        public SubModuleStopState()
        {

        }
        public SubModuleStopState(IProcessingModule module) : base(module)
        {
        }
        public override MovingStateEnum GetState()
        {
            return MovingStateEnum.STOP;
        }

        public override void Moving()
        {
            Module.MovingState = new SubModuleMovingState(Module);
        }

        public override void Stop()
        {
           
        }
    }
    [Serializable]
    public class SubModuleMovingState : SubModuleMovingStateBase
    {
        public SubModuleMovingState()
        {

        }
        public SubModuleMovingState(IProcessingModule module) : base(module)
        {
        }
        public override MovingStateEnum GetState()
        {
            return MovingStateEnum.MOVING;
        }

        public override void Moving()
        {
            throw new Exception("Already MovingState");
        }

        public override void Stop()
        {
            Module.MovingState = new SubModuleStopState(Module);
        }
    }

}
