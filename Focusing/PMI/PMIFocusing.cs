using System;

namespace Focusing.PMI
{
    using FocusGraphControl;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using System.ComponentModel;
    using System.Threading;

    [Serializable]
    public class PMINormalFocusing : FocusingBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        ManualResetEvent mreUpdateEvent = new ManualResetEvent(false);
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private object lockObject = new object();
        public override Type ParamType { get; set; } = typeof(PMIFocusParameter);


        public PMINormalFocusing()
        {

        }

        FocusGraph focusGraph = null;

        public override void ShowFocusGraph()
        {
            try
            {
                if (focusGraph != null)
                {
                    focusGraph.Activate();
                    return;
                }

                focusGraph = new FocusGraph();
                // graphX.Owner = Model.ProberMain;
                focusGraph.Closed += (o, args) => focusGraph = null;
                focusGraph.Show();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override EventCodeEnum Focusing(IFocusParameter focusparam,
                           object callerAssembly = null,
                           bool isOutRangeFind = false,
                           string SavePassPath = "",
                           string SaveFailPath = "",
                           PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum Focusing_Retry(IFocusParameter focusparam,
                                         bool lightChange_retry,
                                         bool bruteForce_retry,
                                         bool outRangeFind_retry,
                                         object callerassembly = null,
                                         int TargetGrayLevel = 0,
                                         bool ForcedApplyAutolight = false,
                                         string SavePassPath = "",
                                         string SaveFailPath = "",
                                         PeakSelectionStrategy peakSelectionStrategy = PeakSelectionStrategy.NONE)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                throw new Exception();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
