using System;

namespace NeedleCleanViewer
{
    using ProberInterfaces.NeedleClean;
    using NeedleCleanerModuleParameter;
    using SubstrateObjects;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using ProberInterfaces;
    using LogModule;

    public class NeedleCleanViewModel : IFactoryModule, INotifyPropertyChanged, INeedleCleanViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public NeedleCleanRenderLayer MainRenderLayer { get; set; }
        private Size size;

        public IStageSupervisor StageSupervisor { get; set; }

        public NeedleCleanViewModel()
        {
            try
            {
                this.StageSupervisor = this.StageSupervisor();
                INeedleCleanObject NcObj = this.StageSupervisor().NCObject;
                //INeedleCleanModule NcModule = this.NeedleCleaner();
                IProbeCard probeCard = this.GetParam_ProbeCard();
                IWaferObject waferObj = this.GetParam_Wafer();

                if (((NeedleCleanSystemParameter)NcObj.NCSysParam_IParam).NeedleCleanPadWidth.Value == 200000 &&
                    ((NeedleCleanSystemParameter)NcObj.NCSysParam_IParam).NeedleCleanPadHeight.Value == 100000)
                {
                    size = new Size(680, 340);
                }
                else if (((NeedleCleanSystemParameter)NcObj.NCSysParam_IParam).NeedleCleanPadWidth.Value == 160000 &&
                    ((NeedleCleanSystemParameter)NcObj.NCSysParam_IParam).NeedleCleanPadHeight.Value == 160000)
                {
                    size = new Size(480, 480);
                }
                else
                {
                    size = new Size(680, 480);
                }

                if(this.NeedleCleaner() != null)
                {
                    MainRenderLayer = new NeedleCleanRenderLayer(size,
                                                             probeCard,
                                                             this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter,
                                                             NcObj.NCSysParam_IParam as NeedleCleanSystemParameter,
                                                             NcObj as NeedleCleanObject,
                                                             waferObj as WaferObject);

                    MainRenderLayer.Init();
                }

                //MainRenderLayer = new NeedleCleanRenderLayer(
                //    size, 
                //    probeCard,
                //    (NeedleCleanDeviceParameter)this.StageSupervisor().NeedleCleaner().NeedleCleanDeviceParameter_IParam,
                //    (NeedleCleanSystemParameter)this.StageSupervisor().NeedleCleaner().NeedleCleanSysParam_IParam,
                //    (NeedleCleanObject)this.StageSupervisor().NCObject,
                //    (WaferObject)this.StageSupervisor().WaferObject
                //    );

                
                //MainRenderLayer.ZoomSetting(5, 20, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
