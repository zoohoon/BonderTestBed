using System;

namespace CallbackServices
{
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using ProberInterfaces;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    public class StageSupervisorCallbackService :  IFactoryModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region //..Property

        private Autofac.IContainer _Container => this.GetLoaderContainer();

        public ILoaderCommunicationManager LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();

        private InstanceContext _InstanceContext;

        public InstanceContext InstanceContext
        {
            get { return _InstanceContext; }
            set { _InstanceContext = value; }
        }

        #endregion

        #region //..Creator & Init
        public StageSupervisorCallbackService()
        {
            InstanceContext = new InstanceContext(this);
        }

        public InstanceContext GetInstanceContext()
        {
            return InstanceContext;
        }
        #endregion

        #region //..Method
        public bool IsServiceAvailable()
        {
            return true;
        }

        public void SetTitleMessage(int Cellindex, string message, string foreground = "", string background = "")
        {
            try
            {
                LoaderCommunicationManager.SetTitleMessage(Cellindex, message, foreground, background);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public void UpdateWaferAlignState(int chuckindex, AlignTypeEnum type, Element<AlignStateEnum> alignstate)
        {
            //Element<AlignStateEnum> tmp = null;

            //switch (type)
            //{
            //    case AlignTypeEnum.Wafer:
            //        tmp = this.StageSupervisor().WaferObject.AlignState;
            //        break;
            //    case AlignTypeEnum.Pin:
            //        tmp = this.StageSupervisor().ProbeCardInfo.AlignState;
            //        break;
            //    case AlignTypeEnum.Mark:
            //        tmp = this.StageSupervisor().MarkObject.AlignState;
            //        break;
            //    default:
            //        break;
            //}

            //if(tmp != null)
            //{
            //    tmp = alignstate;

            //    if (tmp.Value == AlignStateEnum.DONE)
            //    {
            //        tmp.DoneState = ProberInterfaces.State.ElementStateEnum.DONE;
            //    }
            //    else if (tmp.Value == AlignStateEnum.IDLE)
            //    {
            //        tmp.DoneState = ProberInterfaces.State.ElementStateEnum.DEFAULT;
            //    }
            //    else
            //    {
            //        LoggerManager.Error($"Unknown ERROR.");
            //    }

            //    var cell = this.LoaderCommunicationManager.Cells.Where(x => x.Index == chuckindex).FirstOrDefault();

            //    if(cell != null)
            //    {
            //        switch (type)
            //        {
            //            case AlignTypeEnum.Wafer:
            //                cell.StageInfo.WaferObject.AlignState = tmp;
            //                break;
            //            case AlignTypeEnum.Pin:
            //                cell.StageInfo.ProbeCard.AlignState = tmp;
            //                break;
            //            case AlignTypeEnum.Mark:
            //                cell.StageInfo.MarkObject.AlignState = tmp;
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        LoggerManager.Error($"[StageSupervisorCallbackService] UpdateWaferAlignState ERROR.");
            //    }
            //}
        }

        #endregion
    }
}
