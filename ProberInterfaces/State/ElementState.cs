using LogModule;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProberInterfaces.State
{

    public enum ElementStateEnum
    {
        UNDEFINED = 0,
        DEFAULT,
        NEEDSETUP,
        DONE,
    }

    public enum ElementSaveStateEnum
    {
        UNDEFINED = 0,
        MODIFYED,
        SAVEED
    }

    public interface IElementSetupState
    {
        ElementStateEnum GetState();
        void SetDefault();
        void SetNeedSetup();
        void SetSetuped();
    }

    public interface IElementSaveState
    {
        ElementSaveStateEnum GetState();

    }

    [Serializable()]
    public abstract class ElementSetupStateBase : IElementSetupState, INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        [XmlIgnore, JsonIgnore]
        public IElement _Element;

        public ElementSetupStateBase()
        {

        }
        public ElementSetupStateBase(IElement element)
        {
            try
            {
                _Element = element;
                //_Element.State = ElementStateEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public virtual ElementStateEnum GetState()
        {
            return ElementStateEnum.UNDEFINED;
        }
        public virtual void SetSetuped()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetDefault()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }

        public virtual void SetNeedSetup()
        {
            throw new InvalidOperationException(String.Format("State error. Curr. state = {0}", GetState()));
        }
    }

    [Serializable()]
    public class ElementDefaultState : ElementSetupStateBase, INotifyPropertyChanged
    {
        public ElementDefaultState()
        {
        }
        public ElementDefaultState(IElement element) : base(element)
        {
            _Element.DoneState = ElementStateEnum.DEFAULT;
        }
        public override ElementStateEnum GetState()
        {
            return ElementStateEnum.DEFAULT;
        }
        public override void SetSetuped()
        {
            //_Element.ChangeSetupState(new ElementSetupedState(_Element));
        }

        public override void SetDefault()
        {

        }

        public override void SetNeedSetup()
        {
            //_Element.ChangeSetupState(new ElementNeedSetupState(_Element));
        }
    }

    [Serializable()]
    public class ElementNeedSetupState : ElementSetupStateBase, INotifyPropertyChanged
    {
        public ElementNeedSetupState(IElement element) : base(element)
        {
            _Element.DoneState = ElementStateEnum.NEEDSETUP;
        }
        public override ElementStateEnum GetState()
        {
            return ElementStateEnum.NEEDSETUP;
        }
        public override void SetDefault()
        {
            //_Element.ChangeSetupState(new ElementDefaultState(_Element));
        }
        public override void SetNeedSetup()
        {

        }
    }

    [Serializable()]
    public class ElementSetupedState : ElementSetupStateBase, INotifyPropertyChanged
    {
        public ElementSetupedState(IElement element) : base(element)
        {
            _Element.DoneState = ElementStateEnum.DONE;
        }
        public override ElementStateEnum GetState()
        {
            return ElementStateEnum.DONE;
        }
        public override void SetSetuped()
        {

        }
        public override void SetDefault()
        {
            //_Element.ChangeSetupState(new ElementDefaultState(_Element));
        }
        public override void SetNeedSetup()
        {
            //_Element.ChangeSetupState(new ElementNeedSetupState(_Element));
        }
    }
}
