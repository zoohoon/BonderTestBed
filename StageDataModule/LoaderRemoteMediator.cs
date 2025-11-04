using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoaderRemoteMediatorModule
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.Enum;
    using RelayCommandBase;
    using System.IO;
    using System.ServiceModel;
    using System.Collections.ObjectModel;
    using ProberInterfaces.ViewModel;
    using PnPControl;
    using ProberInterfaces.State;
    using System.Windows.Media.Imaging;
    using SharpDXRender.RenderObjectPack;
    using System.Timers;
    using System.Windows;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.ControlClass.ViewModel;
    using ProberInterfaces.ControlClass.ViewModel.DutEditor;
    using SerializerUtil;
    using SubstrateObjects;
    using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
    using ProberInterfaces.ControlClass.ViewModel.Polish_Wafer;
    using ProberInterfaces.Loader.RemoteDataDescription;
    using ProberInterfaces.PMI;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.ControlClass.ViewModel.PMI;
    using ProberInterfaces.Retest;
    using ProberInterfaces.BinData;
    using ProberInterfaces.Param;
    using ProberInterfaces.Utility;
    using LogModule.LoggerParam;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.WaferAlignEX;
    using System.Windows.Input;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class LoaderRemoteMediator : ILoaderRemoteMediator
    {
        #region //..Property

        #region //..ViewGUID
        private Guid _InspectionVMGuid { get; set; }
        private Guid _WaferMapMakerVMGuid { get; set; }
        private Guid _DutEditorVMGuid { get; set; }
        private Guid _ManualContactVMGuid { get; set; }

        private Guid _SoakingRecipeVMGuid { get; set; }
        private Guid _DeviceChangeVMGuid { get; set; }
        private Guid _GPCC_ObservationVMGuid { get; set; }
        private Guid _GPCC_OPVMGuid { get; set; }

        private Guid _SequenceMakerVMGuid { get; set; }

        private Guid _RetestViewModelGuid { get; set; }
        private Guid _PolishWaferMakeSourceVMGuid { get; set; }

        private Guid _PolishWaferRecipeSettingVMGuid { get; set; }

        private Guid _ChuckPlanarityVMGuid { get; set; }

        private Guid _PMIViewerVMGuid { get; set; }

        #endregion


        #endregion

        #region //..Init & DeInit


        public ILoaderRemoteMediatorCallback ServiceCallBack { get; set; }

        public ILoaderRemoteMediatorCallback GetServiceCallBack()
        {
            ILoaderRemoteMediatorCallback callback = null;
            if (ServiceCallBack != null)
            {
                ICommunicationObject obj = (ICommunicationObject)ServiceCallBack;

                if (this.LoaderController().GetconnectFlag())
                {
                    var s = obj.State;

                    if (obj.State != CommunicationState.Faulted && obj.State != CommunicationState.Closed)
                    {
                        callback = ServiceCallBack;
                    }
                }
            }

            return callback;
        }
        public bool Initialized { get; set; }

        public void DeInitModule()
        {
            return;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (!Initialized)
                {
                    _InspectionVMGuid = new Guid("652c9ce4-f811-47ab-88f2-da972eeb66d9");
                    _WaferMapMakerVMGuid = new Guid("156C4231-360D-138C-ED86-6E586C45F359");
                    _DutEditorVMGuid = new Guid("5899DCFE-3032-5360-03D7-1F356B7A0800");
                    _ManualContactVMGuid = new Guid("48468e20-b3dc-4e45-b075-690056f566bf");
                    _SoakingRecipeVMGuid = new Guid("48221362-A0EA-4BD1-81D5-E77895E939BF");
                    _DeviceChangeVMGuid = new Guid("39d5c48c-0bd7-4b6a-ab2d-113df85dce0e");
                    _GPCC_ObservationVMGuid = new Guid("8c3a01c7-adbd-406f-b9b7-f7f3d5f3068b");
                    _GPCC_OPVMGuid = new Guid("fe7caf28-719b-43bd-8926-f97a6f1faed0");
                    _SequenceMakerVMGuid = new Guid("C3BC83A1-C6CA-4BB4-0DE2-34DF6EA06DF2");

                    _RetestViewModelGuid = new Guid("753bbe30-60ae-4731-989e-a8afe5e12561");

                    _PolishWaferMakeSourceVMGuid = new Guid("fbc1cb9c-9aaa-404b-b578-bccbdbe67c47");
                    _PolishWaferRecipeSettingVMGuid = new Guid("666B9F0E-B3E1-8D45-89F0-84A496E4A1FF");

                    _ChuckPlanarityVMGuid = new Guid("060f91cf-30ff-4d81-9025-d434822d384a");

                    _PMIViewerVMGuid = new Guid("60165151-0af4-48fc-9c1f-70497576f4af");

                    Initialized = true;
                }
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public void InitService()
        {
            try
            {
                ServiceCallBack = OperationContext.Current.GetCallbackChannel<ILoaderRemoteMediatorCallback>();
                OperationContext.Current.Channel.Faulted += Channel_Faulted;

                LoggerManager.Debug($"LoaderRemoteMediator Callback Channel initialized.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitService()
        {
            try
            {
                ServiceCallBack = null;
                LoggerManager.Debug($"DeInit LoaderRemoteMediator Channel.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            //minskim// faulted event 발생전에 이미 재연결된 경우가 있을 수 있다.
            if (ServiceCallBack != null)
            {
                if ((ServiceCallBack as ICommunicationObject).State == CommunicationState.Faulted || (ServiceCallBack as ICommunicationObject).State == CommunicationState.Closed)
                {
                    DeInitService();
                    LoggerManager.Debug($"Callback Channel faulted. Sender = {sender}");
                }
                else
                {
                    LoggerManager.Debug($"Ignore Callback Channel faulted. Sender = {sender}, Already Reconnected");
                }
            }
        }
        #endregion

        #region //.. Method

        #region //..Common(WaferObject)
        //public void SetWaferPhysicalInfo(IPhysicalInfo physinfo)
        //{
        //    this.GetParam_Wafer().SetPhysInfo(physinfo);
        //}
        public void SetWaferPhysicalInfo(byte[] physinfo)
        {
            this.GetParam_Wafer().SetPhysInfo((IPhysicalInfo)this.ByteArrayToObjectSync(physinfo));
        }

        public byte[] GetWaferDevObjectbyFileToStream()
        {
            byte[] ret = null;
            try
            {
                ret = this.CompressFileToStream(this.GetFullPath(this.GetParam_Wafer().WaferDevObjectRef));
                // return this.ObjectToByteArray(this.GetParam_Wafer());
                //return this.ToByteArray(this.GetParam_Wafer());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        public byte[] GetWaferDevObject()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(this.GetParam_Wafer().WaferDevObjectRef, typeof(WaferDevObject));
                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetPolishWaferParam(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        #endregion

        #region //..Common(ViewModelManagerModule)

        public Guid GetViewGuidFromViewModelGuid(Guid guid)
        {
            return this.ViewModelManager().GetViewGuidFromViewModelGuid(guid);
        }
        public async Task<EventCodeEnum> PageSwitched(Guid viewGuid, object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var viewmodel = this.ViewModelManager().GetViewModelFromViewGuid(viewGuid);

                if (viewmodel != null)
                {
                    this.VisionManager().AllStageCameraStopGrab();
                    //retVal = viewmodel.PageSwitched(parameter).GetAwaiter().GetResult();

                    retVal = await viewmodel.PageSwitched(parameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public async Task<EventCodeEnum> CleanUp(Guid viewGuid, object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                var viewmodel = this.ViewModelManager().GetViewModelFromViewGuid(viewGuid);

                if (viewmodel != null)
                {
                    this.VisionManager().AllStageCameraStopGrab();
                    //retVal = viewmodel.Cleanup(parameter).GetAwaiter().GetResult();

                    retVal = await viewmodel.Cleanup(parameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        #endregion

        #region //..PNP

        public EventCodeEnum GetCuiBtnParam(object module, Guid cuiguid, out Guid viewguid, out List<Guid> stepguids, bool extrastep = false)
        {
            return this.PnPManager().GetCuiBtnParam(module, cuiguid, out viewguid, out stepguids, extrastep);
        }

        public ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false)
        {
            try
            {
                //this.LoaderRemoteMediator()?.GetServiceCallBack()?.RequestGetWaferObject();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return this.PnPManager().GetCategoryNameList(modulename, interfacename, cuiguid, extrastep);
        }
        public async Task<bool> CheckWaferAlignPossibleSetup()
        {
            return await this.WaferAligner().CheckPossibleSetup();
        }

        public byte[] GetPMITemplateMiniViewModel()
        {
            PMITemplateMiniViewModel retval = null;
            byte[] bytes = null;

            try
            {
                PNPSetupBase pnp = (PNPSetupBase)this.PnPManager().CurStep;

                IPMITemplateMiniViewModel vm = (pnp as IPMITemplateMiniViewModel);

                vm.UpdatePMITemplateMiniViewModel();

                if (vm != null)
                {
                    retval = new PMITemplateMiniViewModel();

                    retval.SelectedPadTemplateIndex = vm.SelectedPadTemplateIndex;
                    retval.PadTemplateInfoCount = vm.PadTemplateInfoCount;
                    retval.SelectedPadTemplate = vm.SelectedPadTemplate;
                    retval.DrawingGroup = vm.DrawingGroup;

                    bytes = SerializeManager.SerializeToByte(retval, typeof(PMITemplateMiniViewModel));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return bytes;
        }

        public ICommand GetPNPCommand(PNPCommandButtonType type)
        {
            ICommand retval = null;

            try
            {
                var descriptor = GetPNPCommandButtonDescriptorCurStep(type);

                if (descriptor != null)
                {
                    retval = descriptor.Command;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public PNPCommandButtonDescriptor GetPNPCommandButtonDescriptorCurStep(PNPCommandButtonType type)
        {
            PNPCommandButtonDescriptor retval = null;

            try
            {
                var curstep = (IPnpSetup)this.PnPManager().CurStep;

                if (curstep != null)
                {
                    switch (type)
                    {
                        case PNPCommandButtonType.PADJOGLEFT:
                            retval = curstep.PadJogLeft;
                            break;
                        case PNPCommandButtonType.PADJOGRIGHT:
                            retval = curstep.PadJogRight;
                            break;
                        case PNPCommandButtonType.PADJOGUP:
                            retval = curstep.PadJogUp;
                            break;
                        case PNPCommandButtonType.PADJOGDOWN:
                            retval = curstep.PadJogDown;
                            break;
                        case PNPCommandButtonType.PADJOGSELECT:
                            retval = curstep.PadJogSelect;
                            break;
                        case PNPCommandButtonType.PADJOGLEFTUP:
                            retval = curstep.PadJogLeftUp;
                            break;
                        case PNPCommandButtonType.PADJOGRIGHTUP:
                            retval = curstep.PadJogRightUp;
                            break;
                        case PNPCommandButtonType.PADJOGLEFTDOWN:
                            retval = curstep.PadJogLeftDown;
                            break;
                        case PNPCommandButtonType.PADJOGRIGHTDOWN:
                            retval = curstep.PadJogRightDown;
                            break;
                        case PNPCommandButtonType.ONEBUTTON:
                            retval = curstep.OneButton;
                            break;
                        case PNPCommandButtonType.TWOBUTTON:
                            retval = curstep.TwoButton;
                            break;
                        case PNPCommandButtonType.THREEBUTTON:
                            retval = curstep.ThreeButton;
                            break;
                        case PNPCommandButtonType.FOURBUTTON:
                            retval = curstep.FourButton;
                            break;
                        case PNPCommandButtonType.FIVEBUTTON:
                            retval = curstep.FiveButton;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public PNPDataDescription GetPNPDataDescriptor()
        {
            PNPDataDescription pNPData = null;

            try
            {
                if (this.PnPManager().CurStep != null)
                {
                    PNPSetupBase pnp = (PNPSetupBase)this.PnPManager().CurStep;
                    pNPData = new PNPDataDescription();
                    pNPData.OneButton = pnp.OneButton;
                    pNPData.TwoButton = pnp.TwoButton;
                    pNPData.ThreeButton = pnp.ThreeButton;
                    pNPData.FourButton = pnp.FourButton;
                    pNPData.FiveButton = pnp.FiveButton;
                    if (pnp.FiveButton.IsEnabled & pnp.FiveButton.Command != null)
                    {
                        pNPData.FiveButton.IsEnabled = true;
                    }
                    else
                        pNPData.FiveButton.IsEnabled = false;
                    pNPData.PadJogUp = pnp.PadJogUp;
                    pNPData.PadJogDown = pnp.PadJogDown;
                    pNPData.PadJogLeft = pnp.PadJogLeft;
                    pNPData.PadJogLeftDown = pnp.PadJogLeftDown;
                    pNPData.PadJogLeftUp = pnp.PadJogLeftUp;
                    pNPData.PadJogRight = pnp.PadJogRight;
                    pNPData.PadJogRightDown = pnp.PadJogRightDown;
                    pNPData.PadJogRightUp = pnp.PadJogRightUp;
                    pNPData.PadJogSelect = pnp.PadJogSelect;

                    pNPData.StepLabel = pnp.StepLabel;
                    pNPData.StepSecondLabel = pnp.StepSecondLabel;
                    pNPData.TargetRectangleHeight = pnp.TargetRectangleHeight;
                    pNPData.TargetRectangleWidth = pnp.TargetRectangleWidth;
                    pNPData.UseUserControl = pnp.UseUserControl;
                    if (pnp.SharpDXLayer != null)
                    {
                        pNPData.UseRender = true;
                        pNPData.RenderHeight = pnp.SharpDXLayer.LayerSize.Height;
                        pNPData.RenderWidth = pnp.SharpDXLayer.LayerSize.Width;
                        pNPData.RenderContainers = pnp.SharpDXLayer.GetRenderContainers();
                    }
                    if (pnp.MainViewTarget != null)
                        pNPData.MainViewTarget = pnp.MainViewTarget.GetType().ToString();
                    if (pnp.MiniViewTarget != null)
                        pNPData.MiniViewTarget = pnp.MiniViewTarget.ToString();
                    if (pnp.AdvanceSetupView != null)
                        pNPData.IsExistAdvenceSetting = true;
                    pNPData.MiniViewHorizontalAlignment = pnp.MiniViewHorizontalAlignment;
                    pNPData.MiniViewVerticalAlignment = pnp.MiniViewVerticalAlignment;
                    pNPData.DisplayIsHitTestVisible = pnp.DisplayIsHitTestVisible;
                    pNPData.MiniViewTargetVisibility = pnp.MiniViewTargetVisibility;
                    pNPData.MiniViewSwapVisibility = pnp.MiniViewSwapVisibility;
                    pNPData.LightJogVisibility = pnp.LightJogVisibility;
                    pNPData.MotionJogVisibility = pnp.MotionJogVisibility;
                    pNPData.JogType = pnp.JogType;
                    pNPData.DisplayClickToMoveEnalbe = pnp.DisplayClickToMoveEnalbe;
                    pNPData.MotionJogEnabled = pnp.MotionJogEnabled;

                    pNPData.SideViewTextBlock = pnp.SideViewTextBlock;
                    //pNPData.ExpanderItem_01 = pnp.ExpanderItem_01;
                    //pNPData.ExpanderItem_02 = pnp.ExpanderItem_02;
                    //pNPData.ExpanderItem_03 = pnp.ExpanderItem_03;
                    //pNPData.ExpanderItem_04 = pnp.ExpanderItem_04;
                    //pNPData.ExpanderItem_05 = pnp.ExpanderItem_05;
                    //pNPData.ExpanderItem_06 = pnp.ExpanderItem_06;
                    //pNPData.ExpanderItem_07 = pnp.ExpanderItem_07;
                    //pNPData.ExpanderItem_08 = pnp.ExpanderItem_08;
                    //pNPData.ExpanderItem_09 = pnp.ExpanderItem_09;
                    //pNPData.ExpanderItem_10 = pnp.ExpanderItem_10;

                    pNPData.SideViewDisplayMode = pnp.SideViewDisplayMode;
                    pNPData.SideViewTargetVisibility = pnp.SideViewTargetVisibility;
                    pNPData.SideViewSwitchVisibility = pnp.SideViewSwitchVisibility;
                    pNPData.SideViewExpanderVisibility = pnp.SideViewExpanderVisibility;
                    pNPData.SideViewTextVisibility = pnp.SideViewTextVisibility;
                    pNPData.SideViewVerticalAlignment = pnp.SideViewVerticalAlignment;
                    pNPData.SideViewHorizontalAlignment = pnp.SideViewHorizontalAlignment;
                    pNPData.SideViewWidth = pnp.SideViewWidth;
                    pNPData.SideViewHeight = pnp.SideViewHeight;
                    pNPData.SideViewMargin = pnp.SideViewMargin;
                    pNPData.SideViewTitle = pnp.SideViewTitle;
                    pNPData.SideViewTitleFontSize = pnp.SideViewTitleFontSize;

                    pNPData.SideViewTitleFontColorString = pnp.SideViewTitleFontColor.ToString();
                    pNPData.SideViewTitleBackgroundString = pnp.SideViewTitleBackground.ToString();

                    pNPData.SideViewTextBlocks = pnp.SideViewTextBlocks;

                    pNPData.SetupState = pnp.SetupState.GetState();
                    var mindex = this.GetParam_Wafer().GetCurrentMIndex();
                    pNPData.MapXIndex = mindex.XIndex;
                    pNPData.MapYIndex = mindex.YIndex;

                    if (pnp.MainViewImageSource != null)
                    {
                        byte[] buffer;
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(pnp.MainViewImageSource));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            buffer = ms.ToArray();
                        }
                        pNPData.MainViewImageSource = buffer;
                    }


                    if (pnp.AdvanceSetupView != null)
                    {
                        var classname = pnp.AdvanceSetupView.ToString().Split('.');
                        pNPData.AdvanceSetupViewModuleInfo = new ModuleDllInfo(
                                 System.Reflection.Assembly.GetAssembly(pnp.AdvanceSetupView.GetType()).GetName().Name + ".dll",
                                 classname[classname.Length - 1]);
                    }

                    if (pnp.AdvanceSetupViewModel != null)
                    {
                        var classname = pnp.AdvanceSetupViewModel.ToString().Split('.');
                        pNPData.AdvanceSetupViewModelModuleInfo = new ModuleDllInfo(
                            System.Reflection.Assembly.GetAssembly(pnp.AdvanceSetupViewModel.GetType()).GetName().Name + ".dll",
                            classname[classname.Length - 1]);
                    }

                    if (pnp.PackagableParams.Count != 0)
                    {
                        pNPData.Params = pnp.PackagableParams;
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pNPData;
        }

        public PnpUIData GetRemoteData()
        {
            PnpUIData retVal = null;
            try
            {
                if (this.PnPManager().CurStep != null)
                {
                    retVal = new PnpUIData();
                    PNPSetupBase pnp = (PNPSetupBase)this.PnPManager().CurStep;
                    retVal.StepLabel = pnp.StepLabel;
                    retVal.StepSecondLabel = pnp.StepSecondLabel;
                    retVal.TargetRectangleHeight = pnp.TargetRectangleHeight;
                    retVal.TargetRectangleWidth = pnp.TargetRectangleWidth;
                    retVal.UseUserControl = pnp.UseUserControl;
                    retVal.SideViewTargetVisibility = pnp.SideViewTargetVisibility;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void PNPSetPackagableParams()
        {
            try
            {
                if (this.PnPManager().CurStep != null)
                {
                    (this.PnPManager().CurStep as IPnpSetup).SetPackagableParams();

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> PNPGetPackagableParams()
        {
            List<byte[]> param = null;
            try
            {
                PNPSetupBase pnp = this.PnPManager().CurStep as PNPSetupBase;
                if (pnp != null)
                {
                    if (pnp.PackagableParams.Count != 0)
                    {
                        param = pnp.PackagableParams;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return param;
        }

        /// <summary>
        /// GetPNPDataDescriptor 보다 간단한 정보.
        /// </summary>
        /// <returns></returns>
        public PNPDataDescription GetPNPDataDescriptor_Basic()
        {
            PNPDataDescription pNPData = null;
            try
            {
                if (this.PnPManager().CurStep != null)
                {
                    PNPSetupBase pnp = (PNPSetupBase)this.PnPManager().CurStep;
                    pNPData = new PNPDataDescription();
                    pNPData.OneButton = pnp.OneButton;
                    pNPData.TwoButton = pnp.TwoButton;
                    pNPData.ThreeButton = pnp.ThreeButton;
                    pNPData.FourButton = pnp.FourButton;
                    pNPData.FiveButton = pnp.FiveButton;

                    pNPData.PadJogUp = pnp.PadJogUp;
                    pNPData.PadJogDown = pnp.PadJogDown;
                    pNPData.PadJogLeft = pnp.PadJogLeft;
                    pNPData.PadJogLeftDown = pnp.PadJogLeftDown;
                    pNPData.PadJogLeftUp = pnp.PadJogLeftUp;
                    pNPData.PadJogRight = pnp.PadJogRight;
                    pNPData.PadJogRightDown = pnp.PadJogRightDown;
                    pNPData.PadJogRightUp = pnp.PadJogRightUp;
                    pNPData.PadJogSelect = pnp.PadJogSelect;

                    pNPData.StepLabel = pnp.StepLabel;
                    pNPData.StepSecondLabel = pnp.StepSecondLabel;
                    pNPData.UseUserControl = pnp.UseUserControl;
                    if (pnp.SharpDXLayer != null)
                    {
                        pNPData.UseRender = true;
                        pNPData.RenderHeight = pnp.SharpDXLayer.LayerSize.Height;
                        pNPData.RenderWidth = pnp.SharpDXLayer.LayerSize.Width;
                        pNPData.RenderContainers = pnp.SharpDXLayer.GetRenderContainers();
                    }
                    if (pnp.MainViewTarget != null)
                        pNPData.MainViewTarget = pnp.MainViewTarget.GetType().ToString();
                    if (pnp.MiniViewTarget != null)
                        pNPData.MiniViewTarget = pnp.MiniViewTarget.ToString();
                    if (pnp.AdvanceSetupView != null)
                        pNPData.IsExistAdvenceSetting = true;
                    pNPData.MiniViewHorizontalAlignment = pnp.MiniViewHorizontalAlignment;
                    pNPData.MiniViewVerticalAlignment = pnp.MiniViewVerticalAlignment;
                    pNPData.DisplayIsHitTestVisible = pnp.DisplayIsHitTestVisible;
                    pNPData.MiniViewTargetVisibility = pnp.MiniViewTargetVisibility;
                    pNPData.MiniViewSwapVisibility = pnp.MiniViewSwapVisibility;
                    pNPData.LightJogVisibility = pnp.LightJogVisibility;
                    pNPData.MotionJogVisibility = pnp.MotionJogVisibility;
                    pNPData.JogType = pnp.JogType;
                    pNPData.DisplayClickToMoveEnalbe = pnp.DisplayClickToMoveEnalbe;
                    pNPData.MotionJogEnabled = pnp.MotionJogEnabled;

                    pNPData.SideViewTextBlock = pnp.SideViewTextBlock;
                    //pNPData.ExpanderItem_01 = pnp.ExpanderItem_01;
                    //pNPData.ExpanderItem_02 = pnp.ExpanderItem_02;
                    //pNPData.ExpanderItem_03 = pnp.ExpanderItem_03;
                    //pNPData.ExpanderItem_04 = pnp.ExpanderItem_04;
                    //pNPData.ExpanderItem_05 = pnp.ExpanderItem_05;
                    //pNPData.ExpanderItem_06 = pnp.ExpanderItem_06;
                    //pNPData.ExpanderItem_07 = pnp.ExpanderItem_07;
                    //pNPData.ExpanderItem_08 = pnp.ExpanderItem_08;
                    //pNPData.ExpanderItem_09 = pnp.ExpanderItem_09;
                    //pNPData.ExpanderItem_10 = pnp.ExpanderItem_10;

                    pNPData.SideViewDisplayMode = pnp.SideViewDisplayMode;
                    pNPData.SideViewTargetVisibility = pnp.SideViewTargetVisibility;
                    pNPData.SideViewSwitchVisibility = pnp.SideViewSwitchVisibility;
                    pNPData.SideViewExpanderVisibility = pnp.SideViewExpanderVisibility;
                    pNPData.SideViewTextVisibility = pnp.SideViewTextVisibility;
                    pNPData.SideViewVerticalAlignment = pnp.SideViewVerticalAlignment;
                    pNPData.SideViewHorizontalAlignment = pnp.SideViewHorizontalAlignment;
                    pNPData.SideViewWidth = pnp.SideViewWidth;
                    pNPData.SideViewHeight = pnp.SideViewHeight;
                    pNPData.SideViewMargin = pnp.SideViewMargin;
                    pNPData.SideViewTitle = pnp.SideViewTitle;
                    pNPData.SideViewTitleFontSize = pnp.SideViewTitleFontSize;

                    pNPData.SideViewTitleFontColorString = pnp.SideViewTitleFontColor.ToString();
                    pNPData.SideViewTitleBackgroundString = pnp.SideViewTitleBackground.ToString();

                    pNPData.SideViewTextBlocks = pnp.SideViewTextBlocks;

                    if (pnp.MainViewImageSource != null)
                    {
                        byte[] buffer;
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(pnp.MainViewImageSource));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            buffer = ms.ToArray();
                        }
                        pNPData.MainViewImageSource = buffer;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pNPData;
        }

        public IPnpSetup GetPnpSetup()
        {
            IPnpSetup pnp = null;
            try
            {
                if (this.PnPManager().CurStep != null)
                    pnp = (IPnpSetup)this.PnPManager().CurStep;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return pnp;
        }

        public void ApplyParams(List<byte[]> parameters)
        {
            this.PnPManager().ApplyParams(parameters);
        }

        public void CloseAdvanceSetupView()
        {
            this.PnPManager().CloseAdvanceSetupView();
        }

        public void SetDislayPortTargetRectInfo(double left, double top)
        {
            this.PnPManager().SetDislayPortTargetRectInfo(left, top);
        }

        public void SetPackagableParams()
        {
            if (this.PnPManager().CurStep != null)
            {
                ((IPnpSetup)this.PnPManager().CurStep as IPnpSetup).SetPackagableParams();
            }
        }

        #region //..Button

        public void PNPButtonExecuteSync(object param, PNPCommandButtonType type)
        {
            try
            {
                GetPNPCommand(type).Execute(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task PNPButtonExecuteAsync(object param, PNPCommandButtonType type)
        {
            try
            {
                var command = GetPNPCommand(type);

                if (command != null)
                {
                    if (command is AsyncCommand)
                    {
                        AsyncCommand asyncCommand = command as AsyncCommand;

                        // Temporarily set _showWaitDialog to false
                        bool originalShowWaitDialog = asyncCommand.ShowWaitDialog;
                        asyncCommand.ShowWaitDialog = false;

                        try
                        {
                            await asyncCommand.ExecuteAsync(param);
                        }
                        finally
                        {
                            // Restore the original value of _showWaitDialog
                            asyncCommand.ShowWaitDialog = originalShowWaitDialog;
                        }
                    }
                    else
                    {
                        command.Execute(param);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public EnumProberCam GetCamType()
        {
            try
            {
                //if (this.PnPManager().SelectedPnpStep != null)
                //{
                //    if (this.PnPManager().SelectedPnpStep.CurCam != null)
                //        return this.PnPManager().SelectedPnpStep.CurCam.GetChannelType();
                //}

                var digitizers = this.VisionManager().DigitizerService;
                if (digitizers.ToList<IDigitizer>().Find
                    (digi => digi.GrabberService.bContinousGrab == true) != null)
                {
                    return digitizers.ToList<IDigitizer>().Find
                    (digi => digi.GrabberService.bContinousGrab == true).CurCamera.GetChannelType();
                }
                //if (this.PnPManager().SelectedPnpStep != null)
                //{
                //    if (this.PnPManager().SelectedPnpStep.CurCam != null)
                //        return this.PnPManager().SelectedPnpStep.CurCam.GetChannelType();
                //}


                //return ((IPnpSetup)this.WaferAligner().Template.GetProcessingModule()[0]).CurCam.GetChannelType();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EnumProberCam.UNDEFINED;
        }

        public void SetSetupState(string moduleheader)
        {
            try
            {
                this.PnPManager().SetSetupState(moduleheader);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetMiniViewTarget(object miniView)
        {
            try
            {
                this.PnPManager().SetMiniViewTarget(miniView);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumMoudleSetupState GetSetupState(string header = null)
        {
            try
            {
                return this.PnPManager().GetSetupState(header);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EnumMoudleSetupState.UNDEFINED;
        }

        public List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = null;
            try
            {
                containers = this.PnPManager().GetRenderContainers();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return containers;
        }

        public async Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter)
        {
            return await this.PnPManager().StepPageSwitching(moduleheader, parameter);
        }

        public async Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter)
        {
            return await this.PnPManager().StepCleanup(moduleheader, parameter);
        }

        public bool StepIsParameterChanged(string moduleheader, bool issave)
        {
            return this.PnPManager().StepIsParameterChanged(moduleheader, issave);
        }

        public EventCodeEnum StepParamValidation(string moduleheader)
        {
            return this.PnPManager().StepParamValidation(moduleheader);
        }
        public async Task SetCurrentStep(string moduleheader)
        {
            await this.PnPManager().SetCurrentStep(moduleheader);
        }

        #endregion

        #region //..Jog

        public void ChangeCamPosition(EnumProberCam cam)
        {
            this.PnPManager().PnpLightJog.ChangeCamPosition(cam);
        }

        public void UpdateCamera(EnumProberCam cam, string interfaceType)
        {
            this.PnPManager().PnpLightJog.UpdateCamera(cam);
            if (this.CoordinateManager().OverlayUpdateDelegate != null)
                this.CoordinateManager().OverlayUpdateDelegate();

        }
        //public bool GetReverseManualMoveX()
        //{
        //    bool ret = false;
        //    try
        //    {
        //        ret = this.CoordinateManager().GetReverseManualMoveX();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return ret;
        //}

        //public bool GetReverseManualMoveY()
        //{
        //    bool ret = false;
        //    try
        //    {
        //        ret = this.CoordinateManager().GetReverseManualMoveY();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return ret;
        //}

        public void SetLightValue(int intensity)
        {
            ((LightJogViewModel)this.PnPManager().PnpLightJog).SetLightValue(intensity);
        }
        public void SetLightChannel(EnumLightType lightchnnel)
        {
            ((LightJogViewModel)this.PnPManager().PnpLightJog).SetLightType(lightchnnel);
        }
        public List<EnumLightType> GetLightTypes()
        {
            var lightJog = this.PnPManager().PnpLightJog;
            if (((LightJogViewModel)lightJog).AssignedCamera != null)
            {
                return ((LightJogViewModel)this.PnPManager().PnpLightJog).AssignedCamera.LightsChannels.Select(light => light.Type.Value).ToList();
            }
            else
            {
                return null;
            }
        }
        public int GetLightValue(EnumLightType lightchannel)
        {
            if (((LightJogViewModel)this.PnPManager().PnpLightJog).AssignedCamera != null)
            {
                return ((LightJogViewModel)this.PnPManager().PnpLightJog).AssignedCamera.GetLight(lightchannel);
            }
            else
                return -1;

        }

        public void StickIndexMove(JogParam parameter, bool setzoffsetenable)
        {
            this.PnPManager().PnpMotionJog.StickIndexMove(parameter, setzoffsetenable);
        }

        public void StickStepMove(JogParam parameter)
        {
            this.PnPManager().PnpMotionJog.StickStepMove(parameter);
        }

        #endregion

        #region //..DisplayPort
        public void SetStageClickMoveTarget(double xpos, double ypos)
        {

        }
        public void StageClickMove(bool enableClickToMove)
        {
            this.StageSupervisor().ClickToMoveLButtonDown(enableClickToMove);
        }

        //public DispFlipEnum GetDispHorFlip()
        //{
        //    DispFlipEnum ret = DispFlipEnum.NONE;
        //    try
        //    {
        //        ret = this.VisionManager().GetDispHorFlip();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return ret;
        //}

        //public DispFlipEnum GetDispVerFlip()
        //{
        //    DispFlipEnum ret = DispFlipEnum.NONE;
        //    try
        //    {
        //        ret = this.VisionManager().GetDispVerFlip();
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return ret;
        //}
        #endregion

        #region //..Polish Wafer

        #endregion

        #region //..Soaking

        public void SetSoakingParam(byte[] param)
        {
            this.SoakingModule().SetDevParam(param);
        }

        public bool GetManaulContactMovingStage()
        {
            bool retval = true;

            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                retval = (vm as IManualContactControlVM).GetManaulContactMovingStage();
            }

            return retval;
        }

        public ManaulContactDataDescription GetManualContactInfo()
        {
            ManaulContactDataDescription des = null;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);
            if (vm != null)
            {
                des = (vm as IManualContactControlVM).GetManaulContactSetupDataDescription();
            }
            return des;
        }

        #endregion

        #region //..RetestViewModel

        public async Task RetestViewModel_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_RetestViewModelGuid);
            if (vm != null)
            {
                await (vm as IRetestSettingViewModel).PageSwitched();
            }
        }

        public async Task RetestViewModel_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_RetestViewModelGuid);
            if (vm != null)
            {
                await (vm as IRetestSettingViewModel).Cleanup();
            }
        }

        public void RetestViewModel_SetRetestIParam(byte[] param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_RetestViewModelGuid);
            if (vm != null)
            {
                (vm as IRetestSettingViewModel).SetRetestIParam(param);
            }
        }

        #endregion

        #region //..PolishWaferMakeSourceVM
        public async Task PolishWaferMakeSourceVM_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);
            if (vm != null)
            {
                await (vm as IPolishWaferMakeSourceVM).PageSwitched();
            }
        }

        public async Task PolishWaferMakeSourceVM_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);
            if (vm != null)
            {
                await (vm as IPolishWaferMakeSourceVM).Cleanup();
            }
        }

        public async Task PolishWaferMakeSourceVM_AddSourceCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

            if (vm != null)
            {
                await (vm as IPolishWaferMakeSourceVM).AddSourceRemoteCommand();
            }
        }

        public async Task PolishWaferMakeSourceVM_RemoveSourceCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

            if (vm != null)
            {
                await (vm as IPolishWaferMakeSourceVM).RemoveSourceRemoteCommand();
            }
        }

        public async Task PolishWaferMakeSourceVM_AssignCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

            if (vm != null)
            {
                //(vm as IPolishWaferMakeSourceVM).AssignCommand.Execute(null);

                await (vm as IPolishWaferMakeSourceVM).AssignRemoteCommand();
            }
        }

        public async Task PolishWaferMakeSourceVM_RemoveCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

            if (vm != null)
            {
                //(vm as IPolishWaferMakeSourceVM).RemoveCommand.Execute(null);

                await (vm as IPolishWaferMakeSourceVM).RemoveRemoteCommand();
            }
        }

        public void PolishWaferMakeSourceVM_UpdateCleaningParameters(string sourcename)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

            if (vm != null)
            {
                (vm as IPolishWaferMakeSourceVM).UpdateCleaningParameters(sourcename);
            }
        }

        public async Task PolishWaferMakeSourceVM_SelectedObjectCommandExcute(object param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

            if (vm != null)
            {
                //(vm as IPolishWaferMakeSourceVM).SelectedObjectCommand.Execute(param);

                await (vm as IPolishWaferMakeSourceVM).SelectedObjectRemoteCommand(param);

            }
        }

        //public async Task PolishWaferMakeSourceVM_SetPolishWaferIParam(object param)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);

        //    if (vm != null)
        //    {
        //        //(vm as IPolishWaferMakeSourceVM).SelectedObjectCommand.Execute(param);

        //        await (vm as IPolishWaferMakeSourceVM).SelectedObjectRemoteCommand(param);
        //    }
        //}

        //public void PolishWaferMakeSourceVM_SetPolishWaferIParam(byte[] param)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);
        //    if (vm != null)
        //    {
        //        //(vm as IPolishWaferRecipeSettingVM).CleaningDeleteCommand.Execute(param);
        //        (vm as IPolishWaferMakeSourceVM).SetPolishWaferIParam(param);
        //    }
        //}

        //public void PolishWaferMakeSourceVM_SetSelectedObjectCommand(byte[] info)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);
        //    if (vm != null)
        //    {
        //        (vm as IPolishWaferMakeSourceVM).SetSelectedObjectCommand(info);
        //    }
        //}

        //// This asynchronously implemented operation is never called because 
        //// there is a synchronous version of the same method.
        //public IAsyncResult BeginSampleMethod(string msg, AsyncCallback callback, object asyncState)
        //{
        //    Console.WriteLine("BeginSampleMethod called with: " + msg);

        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferMakeSourceVMGuid);
        //    if (vm != null)
        //    {
        //        (vm as IPolishWaferMakeSourceVM).SetSelectedObjectCommand(null);
        //    }

        //    return new CompletedAsyncResult<string>(msg);
        //}

        //public string EndSampleMethod(IAsyncResult r)
        //{
        //    CompletedAsyncResult<string> result = r as CompletedAsyncResult<string>;
        //    Console.WriteLine("EndSampleMethod called with: " + result.Data);
        //    return result.Data;
        //}


        #endregion

        #region //..PolishWaferRecipeSettingVM

        public async Task PolishWaferRecipeSettingVM_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);
            if (vm != null)
            {
                await (vm as IPolishWaferRecipeSettingVM).PageSwitched();
            }
        }

        public async Task PolishWaferRecipeSettingVM_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);
            if (vm != null)
            {
                await (vm as IPolishWaferRecipeSettingVM).Cleanup();
            }
        }

        public async Task PolishWaferRecipeSettingVM_IntervalAddCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

            if (vm != null)
            {
                //(vm as IPolishWaferRecipeSettingVM).IntervalAddCommand.Execute(null);

                await (vm as IPolishWaferRecipeSettingVM).IntervalAddRemoteCommand();
            }
        }

        public void PolishWaferRecipeSettingVM_IntervalDelete(int param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

            if (vm != null)
            {
                (vm as IPolishWaferRecipeSettingVM).IntervalDelete(param);
            }
        }

        //public void PolishWaferRecipeSettingVM_IntervalDelete(object param)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

        //    if (vm != null)
        //    {
        //        (vm as IPolishWaferRecipeSettingVM).IntervalDeleteRemoteCommand(param);
        //    }
        //}

        //public async Task PolishWaferRecipeSettingVM_CleaningDeleteCommandExcute(byte[] param)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

        //    if (vm != null)
        //    {
        //        //(vm as IPolishWaferRecipeSettingVM).CleaningDeleteCommand.Execute(param);
        //        await (vm as IPolishWaferRecipeSettingVM).CleaningDeleteCommandWrapper(param);
        //    }
        //}
        public void PolishWaferRecipeSettingVM_CleaningDelete(PolishWaferIndexModel param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);
            if (vm != null)
            {
                (vm as IPolishWaferRecipeSettingVM).CleaningDelete(param);
            }
        }
        public void PolishWaferRecipeSettingVM_SetPolishWaferIParam(byte[] param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);
            if (vm != null)
            {
                //(vm as IPolishWaferRecipeSettingVM).CleaningDeleteCommand.Execute(param);
                (vm as IPolishWaferRecipeSettingVM).SetPolishWaferIParam(param);
            }
        }

        public async Task PolishWaferRecipeSettingVM_SetSelectedInfos(SelectionUIType selectiontype, byte[] cleaningparam, byte[] pwinfo, byte[] intervalparam, int intervalindex, int cleaningindex)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

            if (vm != null)
            {
                await (vm as IPolishWaferRecipeSettingVM).SetSelectedInfos(selectiontype, cleaningparam, pwinfo, intervalparam, intervalindex, cleaningindex);
            }
        }

        //public async Task PolishWaferRecipeSettingVM_CleaningAddCommandExcute(object param)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

        //    if (vm != null)
        //    {
        //        //(vm as IPolishWaferRecipeSettingVM).CleaningAddCommand.Execute(param);
        //        await (vm as IPolishWaferRecipeSettingVM).CleaningAddRemoteCommand(param);
        //    }
        //}
        public void PolishWaferRecipeSettingVM_CleaningAdd(int param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PolishWaferRecipeSettingVMGuid);

            if (vm != null)
            {
                //(vm as IPolishWaferRecipeSettingVM).CleaningAddCommand.Execute(param);
                (vm as IPolishWaferRecipeSettingVM).CleaningAdd(param);
            }
        }
        #endregion

        #region //..PolishWafer

        public void SetPolishWaferParam(byte[] param)
        {
            this.PolishWaferModule().SetDevParam(param);
        }

        public async Task<EventCodeEnum> DoManualPolishWaferCleaningCommand(byte[] param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = await this.PolishWaferModule().DoManualPolishWaferCleaning(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public async Task ManualPolishWaferFocusingCommand(byte[] param)
        {
            try
            {
                await this.PolishWaferModule().ManualPolishWaferFocusing(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region //..Sequence Maker

        public async Task SequenceMakerVM_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).Cleanup();
            }
        }

        public async Task SequenceMakerVM_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).PageSwitched();
            }
        }

        public async Task SequenceMakerVM_SetMXYIndex(Point mxyindex)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                //(vm as ISequenceMakerVM).MXYIndex = mxyindex;
                await (vm as ISequenceMakerVM).SetMXYIndex(mxyindex);
            }
        }

        public async Task<EventCodeEnum> SequenceMakerVM_GetUnderDutDices(MachineIndex mxy)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                retval = await (vm as ISequenceMakerVM).GetUnderDutDices(mxy);
            }

            return retval;
        }

        public async Task SequenceMakerVM_MoveToPrevSeqCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).MoveToPrevSeq();
            }
        }

        public async Task SequenceMakerVM_MoveToNextSeqCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).MoveToNextSeq();
            }
        }

        public async Task SequenceMakerVM_InsertSeqCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).InsertSeq();
            }
        }

        public void SequenceMakerVM_ChangeAutoAddSeqEnable(bool flag)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                (vm as ISequenceMakerVM).AutoAddSeqEnable = flag;
            }
        }
        public async Task SequenceMakerVM_DeleteSeqCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).DeleteSeq();
            }
        }
        public async Task SequenceMakerVM_MapMoveCommandExcute(object param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).MapMove(param);
            }
        }

        public async Task SequenceMakerVM_SeqNumberSeletedCommandExcute(object param)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
                if (vm != null)
                {
                    await (vm as ISequenceMakerVM).SeqNumberSeletedRemote(param);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task SequenceMakerVM_AutoMakeSeqCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).AutoMakeSeq();
            }
        }

        public async Task SequenceMakerVM_DeleteAllSeqCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
            if (vm != null)
            {
                await (vm as ISequenceMakerVM).DeleteAllSeq();
            }
        }

        public SequenceMakerDataDescription GetSequenceMakerInfo()
        {
            SequenceMakerDataDescription info = null;

            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);

            if (vm != null)
            {
                info = (vm as ISequenceMakerVM).GetSequenceMakerInfo();
            }

            return info;
        }

        public List<DeviceObject> GetUnderDutDevices()
        {
            List<DeviceObject> retval = null;

            var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);

            if (vm != null)
            {
                retval = (vm as ISequenceMakerVM).GetUnderDutDevices();
            }

            return retval;
        }

        #endregion

        #region //..InspectionVM

        public async Task<InspcetionDataDescription> GetInspectionInfo()
        {
            InspcetionDataDescription info = null;

            try
            {
                Task task = new Task(() =>
                {
                    var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);

                    if (vm != null)
                    {
                        info = (vm as IInspectionControlVM).GetInscpetionInfo();
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return info;

        }

        public async Task<MarkShiftValues> GetUserSystemMarkShiftValue()
        {
            MarkShiftValues retval = null;
            try
            {

                Task task = new Task(() =>
                {
                   
                     retval = this.ProbingModule().GetUserSystemMarkShiftValue();
                    
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public async Task InspectionVM_SetFromCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).SetFromRemoteCommand();
            }
        }
        public async Task<EventCodeEnum> InspectionVM_CheckPMShiftLimit(double checkvalue)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
                if (vm != null)
                {
                    ret = await (vm as IInspectionControlVM).CheckPMShiftLimit(checkvalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public async Task InspectionVM_SaveCommandExcute(InspcetionDataDescription info)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).SaveRemoteCommand(info);
            }
        }
        public async Task InspectionVM_ApplyCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).ApplyRemoteCommand();
            }
        }
        public async Task InspectionVM_SystemApplyCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).SystemApplyRemoteCommand();
            }
        }

        public async Task InspectionVM_ClearCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).ClearRemoteCommand();
            }
        }

        public async Task InspectionVM_SystemClearCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).SystemClearRemoteCommand();
            }
        }

        public async Task InspectionVM_PrevDutCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).PrevDutRemoteCommand();
            }
        }

        public async Task InspectionVM_NextDutCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).NextDutRemoteCommand();
            }
        }

        public async Task InspectionVM_PadPrevCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).PadPrevRemoteCommand();
            }
        }

        public async Task InspectionVM_PadNextCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).PadNextRemoteCommand();
            }
        }

        //public async Task InspectionVM_ManualSetIndexCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
        //    if (vm != null)
        //    {
        //        await (vm as IInspectionControlVM).ManualSetIndexRemoteCommand();
        //    }
        //}

        public async Task InspectionVM_PinAlignCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).PinAlignRemoteCommand();

            }
        }
        public async Task InspectionVM_WaferAlignCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).WaferAlignRemoteCommand();
            }
        }
        public void InspectionVM_ChangeXManualCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                (vm as IInspectionControlVM).ChangeXManualCommand.Execute(null);
            }
        }
        public void InspectionVM_ChangeYManualCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                (vm as IInspectionControlVM).ChangeYManualCommand.Execute(null);
            }
        }

        public void InspectionVM_ChangeXManualIndex(long index)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                (vm as IInspectionControlVM).Inspection_ChangeXManualIndex(index);
            }
        }

        public void InspectionVM_ChangeYManualIndex(long index)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                (vm as IInspectionControlVM).Inspection_ChangeYManualIndex(index);
            }
        }

        public async Task InspectionVM_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).PageSwitched();
            }
        }

        public async Task InspectionVM_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).Cleanup();
            }
        }

        public async Task InspectionVM_SavePads()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
            if (vm != null)
            {
                await (vm as IInspectionControlVM).SavePadsRemoteCommand();
            }
        }

        public async Task InspectionVM_SaveTempOffset(ObservableDictionary<double, CatCoordinates> table)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_InspectionVMGuid);
                if (vm != null)
                {
                    await (vm as IInspectionControlVM).SaveTempOffsetRemoteCommand(table);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region PMI Viewer

        public async Task PMIViewer_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);
            if (vm != null)
            {
                await (vm as IPMIViewerVM).PageSwitched();
            }

        }

        public int PMIViewer_GetTotalImageCount()
        {
            int retval = 0;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    retval = (vm as IPMIViewerVM).GetTotalImageCount();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void PMIViewer_UpdateFilterDatas(DateTime Startdate, DateTime Enddate, PadStatusResultEnum Status)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    (vm as IPMIViewerVM).UpdateFilterDatas(Startdate, Enddate, Status);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void PMIViewer_LoadImage()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    (vm as IPMIViewerVM).LoadImage();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public PMIImageInformationPack PMIViewer_GetImageFileData(int index)
        {
            PMIImageInformationPack retval = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    retval = (vm as IPMIViewerVM).GetImageFileData(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public ObservableCollection<PMIWaferInfo> PMIViewer_GetWaferlist()
        {
            ObservableCollection<PMIWaferInfo> retval = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    retval = (vm as IPMIViewerVM).GetWaferlist();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void PMIViewer_ChangedWaferListItem(PMIWaferInfo pmiwaferinfo)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    (vm as IPMIViewerVM).ChangedWaferListItem(pmiwaferinfo);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void PMIViewer_WaferListClear()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_PMIViewerVMGuid);

                if (vm != null)
                {
                    (vm as IPMIViewerVM).WaferListClear();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region PMI

        public byte[] GetPMIDevParam()
        {
            byte[] retval = null;

            try
            {
                retval = this.PMIModule().GetPMIDevParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

        #region ResultMap

        public byte[] GetResultMapConvParam()
        {
            byte[] retval = null;

            try
            {
                retval = this.ResultMapManager().GetResultMapConvParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveResultMapConvParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ResultMapManager().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public void SetResultMapConvIParam(byte[] param)
        {
            try
            {
                this.ResultMapManager().SetResultMapConvIParam(param);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool SetResultMapByFileName(byte[] device, string resultmapname)
        {
            bool retval = false;
            try
            {
                retval = this.ResultMapManager().SetResultMapByFileName(device, resultmapname);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public string[] GetNamerAliaslist()
        {
            string[] retval = null;

            try
            {
                retval = this.ResultMapManager().GetNamerAliaslist();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        #region Probing

        public void SetProbingDevParam(byte[] param)
        {
            object obj = this.ByteArrayToObjectSync(param);

            IParam iParam = obj as IParam;

            this.ProbingModule().SetProbingDevParam(iParam);

            //this.ProbingModule().SetProbingDevParam(param);
        }

        public EventCodeEnum ProbingModuleSaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ProbingModule().SaveDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum UpdateSysparam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ProbingModule().LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public CatCoordinates GetSetTemperaturePMShifhtValue()
        {
            CatCoordinates retval = new CatCoordinates();

            try
            {
                retval = this.ProbingModule().GetSetTemperaturePMShifhtValue();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public Dictionary<double, CatCoordinates> GetTemperaturePMShifhtTable()
        {
            Dictionary<double, CatCoordinates> retval = new Dictionary<double, CatCoordinates>();

            try
            {
                retval = this.ProbingModule().GetTemperaturePMShifhtTable();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public byte[] GetProbingDevParam()
        {
            byte[] retval = null;

            try
            {
                retval = this.ProbingModule().GetProbingDevParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public byte[] GetBinDevParam()
        {
            byte[] retval = null;

            try
            {
                retval = this.ProbingModule().GetBinDevParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetBinInfos(byte[] binInfos)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                object obj = this.ByteArrayToObjectSync(binInfos);

                List<IBINInfo> bINInfos = obj as List<IBINInfo>;

                retval = this.ProbingModule().SetBinInfos(bINInfos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SaveBinDevParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ProbingModule().SaveBinDevParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        public async Task SetMCM_XYInex(Point index)
        {
            try
            {
                Task task = new Task(() =>
                {
                    this.ManualContactModule().MXYIndex = index;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MCMIncreaseX()
        {
            try
            {
                this.ManualContactModule().IncreaseX();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MCMIncreaseY()
        {
            try
            {
                this.ManualContactModule().IncreaseY();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MCMDecreaseX()
        {
            try
            {
                this.ManualContactModule().DecreaseX();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MCMDecreaseY()
        {
            try
            {
                this.ManualContactModule().DecreaseY();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void MCMSetIndex(EnumMovingDirection dirx, EnumMovingDirection diry)
        {
            try
            {
                this.ManualContactModule().SetIndex(dirx, diry);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ManualContactVM_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                await (vm as IManualContactControlVM).PageSwitched();
            }
        }

        public async Task ManualContactVM_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                await (vm as IManualContactControlVM).Cleanup();
            }
        }

        public async Task ManualContactVM_FirstContactSetCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).FirstContactSetCommand.Execute(null);
                await (vm as IManualContactControlVM).FirstContactSetRemoteCommand();
            }
        }

        public async Task ManualContactVM_AllContactSetCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).AllContactSetCommand.Execute(null);
                await (vm as IManualContactControlVM).AllContactSetRemoteCommand();
            }
        }
        public async Task ManualContactVM_ResetContactStartPositionCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).ResetContactStartPositionCommand.Execute(null);

                await (vm as IManualContactControlVM).ResetContactStartPositionRemoteCommand();
            }
        }
        public async Task ManualContactVM_OverDriveTBClickCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).OverDriveTBClickCommand.Execute(null);

                await (vm as IManualContactControlVM).OverDriveTBClickRemoteCommand();
            }
        }
        public async Task ManualContactVM_CPC_Z1_ClickCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).OverDriveTBClickCommand.Execute(null);

                await (vm as IManualContactControlVM).CPC_Z1_ClickRemoteCommand();
            }
        }

        public async Task ManualContactVM_CPC_Z2_ClickCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).OverDriveTBClickCommand.Execute(null);

                await (vm as IManualContactControlVM).CPC_Z2_ClickRemoteCommand();
            }
        }

        public async Task ManualContactVM_ChangeZUpStateCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).ChangeZUpStateCommand.Execute(null);

                await (vm as IManualContactControlVM).ChangeZUpStateRemoteCommand();
            }
        }

        public async Task ManualContactVM_MoveToWannaZIntervalPlusCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).MoveToWannaZIntervalPlusCommand.Execute(null);

                await (vm as IManualContactControlVM).MoveToWannaZIntervalPlusRemoteCommand();

            }
        }
        public async Task ManualContactVM_WantToMoveZIntervalTBClickCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).WantToMoveZIntervalTBClickCommand.Execute(null);

                await (vm as IManualContactControlVM).WantToMoveZIntervalTBClickRemoteCommand();
            }
        }

        public async Task ManualContactVM_SetOverDriveCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).SetOverDriveCommand.Execute(null);
                await (vm as IManualContactControlVM).SetOverDriveRemoteCommand();
            }
        }

        public async Task ManualContactVM_MoveToWannaZIntervalMinusCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);

            if (vm != null)
            {
                //(vm as IManualContactControlVM).MoveToWannaZIntervalMinusCommand.Execute(null);

                await (vm as IManualContactControlVM).MoveToWannaZIntervalMinusRemoteCommand();
            }
        }

        public void MCMChangeOverDrive(string overdrive)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);
            if (vm != null)
            {
                (vm as IManualContactControlVM).MCMChangeOverDrive(overdrive);
            }
        }

        public void MCMChangeCPC_Z1(string z1)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);
            if (vm != null)
            {
                (vm as IManualContactControlVM).MCMChangeCPC_Z1(z1);
            }
        }

        public void MCMChangeCPC_Z2(string z2)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);
            if (vm != null)
            {
                (vm as IManualContactControlVM).MCMChangeCPC_Z2(z2);
            }
        }
        public void ManualContactVM_GoToInspectionViewCommandExcute()
        {

        }

        public void GetOverDriveFromProbingModule()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ManualContactVMGuid);
            if (vm != null)
            {
                (vm as IManualContactControlVM).GetOverDriveFromProbingModule();
            }
        }
        //public void MCMSetAlawaysMoveToFirstDut(bool flag)
        //{
        //    this.ManualContactModule().AlawaysMoveToTeachDie = false;
        //}
        #endregion
        public bool IsAvailable()
        {
            return true;
        }
        #region //..Soaking Recipe VM

        public SoakingRecipeDataDescription GetSaokingRecipeInfo()
        {
            SoakingRecipeDataDescription des = null;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SoakingRecipeVMGuid);

            if (vm != null)
            {
                des = (vm as ISoakingRecipeSettingVM).GetSoakingRecipeDataDescription();
            }

            return des;
        }

        public void SoakingRecipeVM_DropDownClosedCommandExecute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_SoakingRecipeVMGuid);
            if (vm != null)
            {
                (vm as ISoakingRecipeSettingVM).DropDownClosedCommand.Execute(null);
            }
        }
        #endregion


        #region //..Wafer Map Maker VM
        public void WaferMapMaker_UpdateWaferSize(EnumWaferSize waferSize)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).WaferSize = waferSize;
            }
        }

        public void WaferMapMaker_UpdateWaferSizeOffset(double WaferSize_Offset_um)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                this.GetParam_Wafer().GetPhysInfo().WaferSize_Offset_um.Value = WaferSize_Offset_um;
                (vm as IWaferMapMakerVM).WaferSize_Offset_um = WaferSize_Offset_um;
            }
        }
        public void WaferMapMakerVM_WaferSubstrateType(WaferSubstrateTypeEnum wafersubstratetype)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).WaferSubstrateType = wafersubstratetype;
            }
        }

        public void WaferMapMakerVM_UpdateDieSizeX(double diesizex)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).DieSizeX = diesizex;
            }
        }
        public void WaferMapMakerVM_UpdateDieSizeY(double diesizey)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).DieSizeY = diesizey;
            }
        }
        public void WaferMapMakerVM_UpdateThickness(double thickness)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                this.GetParam_Wafer().GetPhysInfo().Thickness.Value = thickness;
                this.GetParam_Wafer().GetSubsInfo().ActualThickness = thickness;
            }
        }
        public void WaferMapMakeVM_UpdateEdgeMargin(double margin)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).EdgeMargin = margin;
            }
        }
        public void WaferMapMakerVM_NotchAngle(double notchangle)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).NotchAngle = notchangle;
            }
        }
        public void WaferMapMakerVM_NotchType(WaferNotchTypeEnum notchtype)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).NotchType = notchtype;
            }
        }

        public void WaferMapMakerVM_NotchAngleOffset(double notchangleoffset)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).NotchAngleOffset = notchangleoffset;
            }
        }

        public async Task WaferMapMaker_Cleanup()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
                if (vm != null)
                {
                    await (vm as IWaferMapMakerVM).Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task WaferMapMakerVM_ApplyCreateWaferMapCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                await ((vm as IWaferMapMakerVM).ApplyCreateWaferMap());
            }
        }
        public async Task WaferMapMakerVM_MoveToWaferThicknessCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                await ((vm as IWaferMapMakerVM).MoveToWaferThicknessCommandFunc());
            }
        }

        public async Task WaferMapMakerVM_AdjustWaferHeightCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                await ((vm as IWaferMapMakerVM).AdjustWaferHeightCommandFunc());
            }
        }
        public async Task<EventCodeEnum> WaferMapMakerVM_CmdImportWaferDataCommandExcute(Stream fileStream)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).CSVFileStream = fileStream;
                retVal = await (vm as IWaferMapMakerVM).ImportWaferData();
            }
            return retVal;
        }
        public void WaferMapMakerVM_ImportFilePath(string filePath)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).CSVFilePath = filePath;
            }
        }

        public void WaferMapMakerVM_SetHighStandardParam(HeightPointEnum heightpoint)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                (vm as IWaferMapMakerVM).SelectionChangedFunc(heightpoint);
            }
        }

        public HeightPointEnum WaferMapMakerVM_GetHeightProfiling()
        {
            HeightPointEnum retVal = HeightPointEnum.UNDEFINED;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_WaferMapMakerVMGuid);
            if (vm != null)
            {
                retVal = (vm as IWaferMapMakerVM).HeightPoint;
            }
            return retVal;
        }
        #endregion

        #region //..Probing
        public void SetProbingDevices(ObservableCollection<IDeviceObject> devs)
        {
            this.ProbingModule().SetProbingUnderdut(devs);
        }
        #endregion

        #region //..Dut Editor VM

        public byte[] DutEditor_GetDutlist()
        {
            byte[] dultist = null;

            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                dultist = (vm as IDutEditorVM).GetDutlist();
            }

            return dultist;
        }

        public DutEditorDataDescription GetDutEditorInfo()
        {
            DutEditorDataDescription des = null;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);

            if (vm != null)
            {
                des = (vm as IDutEditorVM).GetDutEditorInfo();
            }

            return des;
        }

        public void DutEditor_ChangedSelectedCoordM(MachineIndex param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).ChangedSelectedCoordM(param);
            }
        }

        public void DutEditor_ChangedChangedFirstDutM(MachineIndex param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).ChangedChangedFirstDutM(param);
            }
        }

        public void DutEditor_ChangedAddCheckBoxIsChecked(bool? param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).ChangedAddCheckBoxIsChecked(param);
            }
        }

        public void DutEditor_ImportFilePath(string filePath)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).CSVFilePath = filePath;
            }
        }

        public async Task<EventCodeEnum> DutEditor_CmdImportCardDataCommandExcute(Stream fileStream)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).CSVFileStream = fileStream;
                retVal = await (vm as IDutEditorVM).ImportCardData();
            }
            return retVal;
        }

        public async Task DutEditor_InitializePalletCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                await (vm as IDutEditorVM).FuncInitializePalletCommand();
            }
        }

        public async Task DutEditor_DutAddCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                await (vm as IDutEditorVM).DutAdd();
            }
        }

        public async Task DutEditor_DutDeleteCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                await (vm as IDutEditorVM).DutDelete();
            }
        }

        public void DutEditor_ZoomInCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).ZoomInCommand.Execute(null);
            }
        }

        public void DutEditor_ZoomOutCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                (vm as IDutEditorVM).ZoomOutCommand.Execute(null);
            }
        }

        public async Task DutEditor_DutEditerMoveCommandExcute(EnumArrowDirection param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                await (vm as IDutEditorVM).DutEditerMoveCommandFunc(param);
            }
        }

        public async Task DutEditor_DutAddMouseDownCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                await (vm as IDutViewControlVM).DutAddbyMouseDown();
            }
        }

        public async Task DutEditor_Cleanup()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
                if (vm != null)
                {
                    await (vm as IDutEditorVM).Cleanup();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task DutEditor_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DutEditorVMGuid);
            if (vm != null)
            {
                await (vm as IDutEditorVM).PageSwitched();
            }
        }

        #endregion

        #region //.. Device Change (FileManager)

        //public async Task GetParamFromDevice(DeviceInfo device)
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);

        //    if (vm != null)
        //    {
        //        await (vm as IDeviceChangeVM).GetParamFromDevice(device);
        //    }
        //}

        public async Task GetParamFromDevice(DeviceInfo device)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);

            if (vm != null)
            {
                await (vm as IDeviceChangeVM).GetParamFromDevice(device);
            }
        }


        public DeviceChangeDataDescription GetDeviceChangeInfo()
        {
            DeviceChangeDataDescription info = null;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                info = (vm as IDeviceChangeVM).GetDeviceChangeInfo();
            }
            return info;
        }

        public async Task DeviceChange_GetDevList(bool isPageSwiching = false)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);

            if (vm != null)
            {
                await (vm as IDeviceChangeVM).GetDevList(isPageSwiching);
            }
        }

        public async Task DeviceChange_GetParamFromDeviceCommandExcute(DeviceInfo device)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                await (vm as IDeviceChangeVM).GetParamFromDevice(device);

            }
        }
        public void DeviceChange_RefreshDevListCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                (vm as IDeviceChangeVM).RefreshDevListCommand.Execute(null);
            }
        }

        public void DeviceChange_ClearSearchDataCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                (vm as IDeviceChangeVM).ClearSearchDataCommand.Execute(null);
            }
        }

        public void DeviceChange_SearchTBClickCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                (vm as IDeviceChangeVM).SearchTBClickCommand.Execute(null);
            }
        }

        public void DeviceChange_PageSwitchingCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                (vm as IDeviceChangeVM).PageSwitchingCommand.Execute(null);
            }
        }

        public async Task DeviceChange_ChangeDeviceCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                await ((vm as IDeviceChangeVM).ChangeDeviceFunc());
            }
        }

        public async Task DeviceChangeWithName_ChangeDeviceCommandExcute(string deviceName)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                await ((vm as IDeviceChangeVM).ChangeDeviceFunc(deviceName));
            }
        }

        public async Task DeviceChange_CreateNewDeviceCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                await ((vm as IDeviceChangeVM).CreateNewDeviceFunc());
            }
        }

        public async Task DeviceChange_SaveAsDeviceCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                await ((vm as IDeviceChangeVM).SaveAsDeviceFunc());
            }
        }

        public async Task DeviceChange_DeleteDeviceCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
            if (vm != null)
            {
                await ((vm as IDeviceChangeVM).DeleteDeviceFunc());
            }
        }

        //public async Task DeviceChange_ChangeDeviceCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_DeviceChangeVMGuid);
        //    if (vm != null)
        //    {
        //        await (vm as IInspectionControlVM).PrevDutCommand.ExecuteAsync(null);
        //    }
        //}

        //public byte[] DeviceChange_CreateDeviceFolder(string devpath)
        //{
        //    return this.FileManager().GetNewDeviceFolderUsingName(devpath);
        //}

        //public byte[] DeviceChange_SaveAsDeviceFolder(string devpath)
        //{
        //    return this.FileManager().GetSaveAsDeviceUsingName(devpath);
        //}

        #endregion

        #region ChuckPlanrity

        public async Task ChuckPlanarity_ChuckMoveCommandExcute(EnumChuckPosition param)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
                if (vm != null)
                {
                    await ((vm as IChuckPlanarityVM).ChuckMoveCommandFunc(param));
                    //await ((vm as IChuckPlanarityVM).ChuckMoveCommand as IAsyncCommand<EnumChuckPosition>).ExecuteAsync(param);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task ChuckPlanarity_MeasureOnePositionCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                var ret = await ((vm as IChuckPlanarityVM).MeasureOnePositionCommandFunc());
            }
        }

        public async Task ChuckPlanarity_MeasureAllPositionCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                var ret = await ((vm as IChuckPlanarityVM).MeasureAllPositionCommandFunc());
            }
        }

        public async Task ChuckPlanarity_SetAdjustPlanartyFuncExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                var ret = await ((vm as IChuckPlanarityVM).SetAdjustPlanartyFunc());
            }
        }

        public void ChuckPlanarity_ChangeMarginValue(double value)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                (vm as IChuckPlanarityVM).ChangeMarginValue(value);
            }
        }

        public void ChuckPlanarity_FocusingRangeValue(double value)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                (vm as IChuckPlanarityVM).ChangeFocusingRange(value);
            }
        }

        public ChuckPlanarityDataDescription ChuckPlanarity_GetChuckPlanarityInfo()
        {
            ChuckPlanarityDataDescription info = null;

            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                info = (vm as IChuckPlanarityVM).GetChuckPlanarityInfo();
            }

            return info;
        }

        public async Task ChuckPlanarity_PageSwitched()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                await (vm as IChuckPlanarityVM).PageSwitched();
            }
        }

        public async Task ChuckPlanarity_Cleanup()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                await (vm as IChuckPlanarityVM).Cleanup();
            }
        }

        public async Task ChuckMoveCommand(ChuckPos param)
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_ChuckPlanarityVMGuid);
            if (vm != null)
            {
                await (vm as IChuckPlanarityVM).ChuckMoveCommandFunc(param);
            }
        }

        #endregion
        #region //.. GPCC_Observation View Model


        public async Task GPCC_Observation_CardSettingCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).CardSettingCommand.Execute(null);
                    await (vm as ICCObservationVM).CardSettingCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_PogoSettingCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).PogoSettingCommand.Execute(null);
                    await (vm as ICCObservationVM).PogoSettingCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_PodSettingCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).PogoSettingCommand.Execute(null);
                    await (vm as ICCObservationVM).PodSettingCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GPCC_Observation_PatternWidthPlusCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).PatternWidthPlusCommand.Execute(null);
                    (vm as ICCObservationVM).PatternWidthPlusCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GPCC_Observation_PatternWidthMinusCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    (vm as ICCObservationVM).PatternWidthMinusCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GPCC_Observation_PatternHeightPlusCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).PatternHeightPlusCommand.Execute(null);
                    (vm as ICCObservationVM).PatternHeightPlusCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Observation_PatternHeightMinusCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).PatternHeightMinusCommand.Execute(null);
                    (vm as ICCObservationVM).PatternHeightMinusCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public String GPCC_GetPosition()
        {
            string retVal = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    retVal = (vm as ICCObservationVM).GetPostion();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsCheckCardPodState()
        {
            bool retVal = false;
            try
            {
                retVal = this.CardChangeModule().IsCheckCardPodState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task GPCC_Observation_WaferCamExtendCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).WaferCamExtendCommand.Execute(null);
                    await (vm as ICCObservationVM).WaferCamExtendCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_WaferCamFoldCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).WaferCamFoldCommand.Execute(null);
                    await (vm as ICCObservationVM).WaferCamFoldCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_MoveToCenterCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).MoveToCenterCommand.Execute(null);
                    await (vm as ICCObservationVM).MoveToCenterCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_ReadyToGetCardCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).ReadyToGetCardCommand.Execute(null);
                    await (vm as ICCObservationVM).ReadyToGetCardCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_RaiseZCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).RaiseZCommand.Execute(null);
                    await (vm as ICCObservationVM).RaiseZCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public Task GPCC_Observation_ManualZMoveCommand(EnumProberCam camType, double Value)
        {
            try
            {
                this.GPCardAligner().RelMoveZ(camType, Value);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
            return Task.CompletedTask;
        }

        public async Task GPCC_Observation_SetMFModelLightsCommandExcute()
        {

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetMFModelLightsCommand.Execute(null);
                    await (vm as ICCObservationVM).SetMFModelLightsCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_SetMFChildLightsCommandExcute()
        {

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetMFChildLightsCommand.Execute(null);
                    await (vm as ICCObservationVM).SetMFChildLightsCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_DropZCommandExcute()
        {

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).DropZCommand.Execute(null);
                    await (vm as ICCObservationVM).DropZCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GPCC_Observation_IncreaseLightIntensityCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).IncreaseLightIntensityCommand.Execute(null);
                    (vm as ICCObservationVM).IncreaseLightIntensityCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void GPCC_Observation_DecreaseLightIntensityCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).DecreaseLightIntensityCommand.Execute(null);
                    (vm as ICCObservationVM).DecreaseLightIntensityCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_RegisterPatternCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).RegisterPatternCommand.Execute(null);
                    await (vm as ICCObservationVM).RegisterPatternCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_PogoAlignPointCommandExcute(EnumPogoAlignPoint point)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).RegisterPatternCommand.Execute(null);
                    await (vm as ICCObservationVM).PogoAlignPointCommandFunc(point);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_RegisterPosCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    await (vm as ICCObservationVM).RegisterPosCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public GPCardChangeVMData GetGPCCData()
        {
            GPCardChangeVMData info = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    info = (vm as ICCObservationVM).GetGPCCData();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return info;
        }
        public async Task GPCC_Observation_MoveToMarkCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).MoveToMarkCommand.Execute(null);
                    await (vm as ICCObservationVM).MoveToMarkCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_SetSelectedMarkPosCommandExcute(int selectedmarkposIdx)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetSelectedMarkPosCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(selectedmarkpos));
                    await (vm as ICCObservationVM).SetSelectedMarkPosCommandFunc(selectedmarkposIdx);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public byte[] GPCC_OP_GetDockSequence()
        {
            byte[] retval = null;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                //(vm as ICCObservationVM).SetSelectedMarkPosCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(selectedmarkpos));
                retval = (vm as ICardChangeOPVM).GetDockSequences();
            }
            return retval;
        }
        public byte[] GPCC_OP_GetUnDockSequence()
        {
            byte[] retval = null;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                //(vm as ICCObservationVM).SetSelectedMarkPosCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(selectedmarkpos));
                retval = (vm as ICardChangeOPVM).GetUnDockSequences();
            }
            return retval;
        }

        public int GPCC_OP_GetCurBehaviorIdx()
        {
            int retval = 0;
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                retval = (vm as ICardChangeOPVM).GetCurBehaviorIdx();
            }
            return retval;
        }
        public void GPCC_Docking_PauseState()
        {
            try
            {
                this.CardChangeModule().Dock_PauseCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Docking_StepUpState()
        {
            try
            {
                this.CardChangeModule().Dock_StepUpCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Docking_ContinueState()
        {
            try
            {
                this.CardChangeModule().Dock_ContinueCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_Docking_AbortState()
        {
            try
            {
                this.CardChangeModule().Dock_AbortCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_PauseState()
        {
            try
            {
                this.CardChangeModule().UnDock_PauseCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_StepUpState()
        {
            try
            {
                this.CardChangeModule().UnDock_StepUpCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_ContinueState()
        {
            try
            {
                this.CardChangeModule().UnDock_ContinueCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_UnDocking_AbortState()
        {
            try
            {
                this.CardChangeModule().UnDock_AbortCommand();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_SetSelectLightTypeCommandExcute(EnumLightType selectlight)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetSelectLightTypeCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(selectlight));
                    await (vm as ICCObservationVM).SetSelectLightTypeCommandFunc(selectlight);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_SetLightValueCommandExcute(ushort lightvalue)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetLightValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(lightvalue));
                    await (vm as ICCObservationVM).SetLightValueCommandFunc(lightvalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_SetZTickValueCommandExcute(int ztickvalue)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetZTickValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(ztickvalue));
                    await (vm as ICCObservationVM).SetZTickValueCommandFunc(ztickvalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_SetZDistanceValueCommandExcute(double zdistancevalue)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetZDistanceValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(zdistancevalue));
                    await (vm as ICCObservationVM).SetZDistanceValueCommandFunc(zdistancevalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_SetLightTickValueCommandExcute(int lighttickvalue)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    //(vm as ICCObservationVM).SetLightTickValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(lighttickvalue));
                    await (vm as ICCObservationVM).SetLightTickValueCommandFunc(lighttickvalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_PageSwitchCommandExcute(bool observation)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    await ((vm as ICCObservationVM).PageSwitchCommandFunc(observation));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_Observation_CleanUpCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    await ((vm as ICCObservationVM).CleanUpCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_FocusingCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    await ((vm as ICCObservationVM).FocusingCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_Observation_AlignCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_ObservationVMGuid);
                if (vm != null)
                {
                    await ((vm as ICCObservationVM).AlignCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region GPCC_OP
        public async Task GPCC_OP_SetCardFuncsRangeCommandExcute(double offsetz)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetFocusRangeValueCommandFunc(offsetz);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetUndockContactOffsetzCommandExcute(double offsetz)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetUndockContactOffsetZValueCommandFunc(offsetz);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetContactOffsetzCommandExcute(double offsetz)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    //(vm as ICardChangeOPVM).SetContactOffsetZValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(offsetz));
                    await (vm as ICardChangeOPVM).SetContactOffsetZValueFunc(offsetz);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_SetContactOffsetxCommandExcute(double offsetx)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    //(vm as ICardChangeOPVM).SetContactOffsetZValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(offsetz));
                    await (vm as ICardChangeOPVM).SetContactOffsetXValueFunc(offsetx);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_SetContactOffsetyCommandExcute(double offsety)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    //(vm as ICardChangeOPVM).SetContactOffsetZValueCommand.Execute(SerializerUtil.ObjectSerialize.DeSerialize(offsetz));
                    await (vm as ICardChangeOPVM).SetContactOffsetYValueFunc(offsety);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SmallRaiseChuckCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SmallRaiseChuckCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void GPCC_OP_CardContactSettingZCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    (vm as ICardChangeOPVM).CardContactSettingZCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void GPCC_OP_CardUndockContactSettingZCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        (vm as ICardChangeOPVM).CardUndockContactSettingZCommand.Execute(null);
        //    }
        //}
        public void GPCC_OP_CardUndockContactSettingZCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    (vm as ICardChangeOPVM).CardUndockContactSettingZCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_CardFocusRangeSettingZCommandExcute(double rangevalue)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).CardFocusRangeSettingCommandFunc(rangevalue);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetAlignRetryCountCommandExcute(int retrycount)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetAlignRetryCountCommandFunc(retrycount);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetDistanceOffsetCommandExcute(double distanceoffset)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetDistanceOffsetCommandFunc(distanceoffset);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardTopFromChuckPlaneSettingCommandExcute(double distance)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardTopFromChuckPlaneSettingCommandFunc(distance);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardCenterOffsetX1CommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardCenterOffsetX1CommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public async Task GPCC_OP_SetCardCenterOffsetX2CommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardCenterOffsetX2CommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardCenterOffsetY1CommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardCenterOffsetY1CommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardCenterOffsetY2CommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardCenterOffsetY2CommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_SetCardPodCenterXCommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardPodCenterXCommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardPodCenterYCommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardPodCenterYCommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardLoadZLimitCommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardLoadZLimitCommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_SetCardLoadZIntervalCommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardLoadZIntervalCommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_SetCardUnloadZOffsetCommandExcute(double value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetCardUnloadZOffsetCommandFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_ZifToggleCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                await (vm as ICardChangeOPVM).ZifToggleCommandFunc();
            }
        }
        public async Task GPCC_OP_SmallDropChuckCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SmallDropChuckCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void GPCC_OP_MoveToZClearedCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        (vm as ICardChangeOPVM).MoveToZClearedCommand.Execute(null);
        //    }
        //}

        public void GPCC_OP_MoveToZClearedCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    (vm as ICardChangeOPVM).MoveToZClearedCommandFunc();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_MoveToLoaderCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).MoveToLoaderCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_MoveToLoaderCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                await ((vm as ICardChangeOPVM).MoveToLoaderCommandFunc());
            }
        }
        public async Task GPCC_OP_MoveToCenterCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                await ((vm as ICardChangeOPVM).MoveToCenterCommand as IAsyncCommand).ExecuteAsync(null);
            }
        }
        public async Task GPCC_OP_MoveToBackCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                await ((vm as ICardChangeOPVM).MoveToBackCommand as IAsyncCommand).ExecuteAsync(null);
            }
        }
        public async Task GPCC_OP_MoveToFrontCommandExcute()
        {
            var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
            if (vm != null)
            {
                await ((vm as ICardChangeOPVM).MoveToFrontCommand as IAsyncCommand).ExecuteAsync(null);
            }
        }
        public async Task GPCC_OP_RaisePodAfterMoveCardAlignPosCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).RaisePodAfterMoveCardAlignPosCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_RaisePodCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).RaisePodCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_DropPodCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).DropPodCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_DropPodCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).DropPodCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task<EventCodeEnum> GPCC_OP_ForcedDropPodCommandExcute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    retVal = await ((vm as ICardChangeOPVM).ForcedDropPodCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //public async Task GPCC_OP_TopPlateSolLockCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).TopPlateSolLockCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_TopPlateSolLockCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).TopPlateSolLockCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_TopPlateSolUnLockCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).TopPlateSolUnLockCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_TopPlateSolUnLockCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).TopPlateSolUnLockCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_PCardPodVacuumOffCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).PCardPodVacuumOffCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_PCardPodVacuumOffCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).PCardPodVacuumOffCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_PCardPodVacuumOnCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).PCardPodVacuumOnCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_PCardPodVacuumOnCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).PCardPodVacuumOnCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_UpPlateTesterCOfftactVacuumOffCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).UpPlateTesterCOfftactVacuumOffCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_UpPlateTesterCOfftactVacuumOffCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).UpPlateTesterCOfftactVacuumOffCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_UpPlateTesterContactVacuumOnCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).UpPlateTesterContactVacuumOnCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_UpPlateTesterContactVacuumOnCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).UpPlateTesterContactVacuumOnCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_UpPlateTesterPurgeAirOffCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).UpPlateTesterPurgeAirOffCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public async Task GPCC_OP_UpPlateTesterPurgeAirOnCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).UpPlateTesterPurgeAirOnCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task GPCC_OP_PogoVacuumOnCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).PogoVacuumOnCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task GPCC_OP_PogoVacuumOffCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).PogoVacuumOffCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        //public async Task GPCC_OP_DockCardCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).DockCardCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_DockCardCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).DockCardCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_UnDockCardCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).UnDockCardCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_UnDockCardCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).UnDockCardCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_CardAlignCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).CardAlignCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_CardAlignCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).CardAlignCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_PageSwitchCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).PageSwitchCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_PageSwitchCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).PageSwitchCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public async Task GPCC_OP_CleanUpCommandExcute()
        //{
        //    var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
        //    if (vm != null)
        //    {
        //        await ((vm as ICardChangeOPVM).CleanUpCommand as IAsyncCommand).ExecuteAsync(null);
        //    }
        //}

        public async Task GPCC_OP_CleanUpCommandExcute()
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await ((vm as ICardChangeOPVM).CleanUpCommandFunc());
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public byte[] GetGPCardChangeSysParamData()
        {
            CardChangeSysparamData info = null;
            byte[] bytes = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    info = (vm as ICardChangeOPVM).GetCardChangeSysParam();
                    bytes = SerializeManager.SerializeToByte(info, typeof(CardChangeSysparamData));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return bytes;
        }

        public async Task<CardChangeDevparamData> GetGPCardChangeDevParamData()
        {
            CardChangeDevparamData info = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    info = await (vm as ICardChangeOPVM).GetCardChangeDevParam();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return info;
        }



        public CardChangeVacuumAndIOStatus GetCCVacuumStatus()
        {
            CardChangeVacuumAndIOStatus info = null;

            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    info = (vm as ICardChangeOPVM).GetCardChangeVacAndIOStatus();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return info;
        }

        #endregion


        #region // Get temp. module
        public ITempController GetTempModule()
        {
            ITempController tempController = this.TempController();
            return tempController;
        }






        #endregion

        #region TemplateManager

        public EventCodeEnum CheckTemplateUsedType(string moduletype, bool applyload = true, int index = -1)
        {
            ITemplate module = null;

            if (moduletype.ToString() == "PinAligner")
            {
                module = this.PinAligner();
            }

            return this.TemplateManager().CheckTemplate(module, applyload, index);
        }

        public void GPCC_OP_LoaderDoorOpenCommandExcute()
        {
            this.StageSupervisor().StageModuleState.LoaderDoorOpen();
        }

        public void GPCC_OP_LoaderDoorCloseCommandExcute()
        {
            this.StageSupervisor().StageModuleState.LoaderDoorClose();
        }
        public bool GPCC_OP_IsLoaderDoorCloseCommandExcute(bool writelog = true)
        {
            bool isClose = false;
            this.StageSupervisor().StageModuleState.IsLoaderDoorClose(ref isClose, writelog);
            return isClose;
        }

        public bool GPCC_OP_IsLoaderDoorOpenCommandExcute(bool writelog = true)
        {
            bool isOpen = false;
            this.StageSupervisor().StageModuleState.IsLoaderDoorOpen(ref isOpen, writelog);
            return isOpen;
        }
        public void GPCC_OP_CardDoorOpenCommandExcute()
        {
            this.StageSupervisor().StageModuleState.CardDoorOpen();
        }
        public void GPCC_OP_CardDoorCloseCommandExcute()
        {
            this.StageSupervisor().StageModuleState.CardDoorClose();
        }

        public bool GPCC_OP_IsCardExistExcute()
        {
            bool isExist = false;
            isExist = this.StageSupervisor().StageModuleState.IsCardExist();
            return isExist;
        }

        public async Task GPCC_SetWaitForCardPermitEnable(bool value)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_GPCC_OPVMGuid);
                if (vm != null)
                {
                    await (vm as ICardChangeOPVM).SetWaitForCardPermitEnableFunc(value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region Move

        #endregion
        #region LogTransfer
        public List<List<string>> LogTransfer_UpdateLogFile()
        {
            List<List<string>> LogFileList = null;

            try
            {
                LogFileList = this.StageSupervisor().UpdateLogFile();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return LogFileList;
        }
        public byte[] LogTransfer_OpenLogFile(string selectedFilePath)
        {
            byte[] compressedFile = null;

            try
            {
                compressedFile = this.StageSupervisor().OpenLogFile(selectedFilePath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return compressedFile;
        }

        public ObservableDictionary<string, string> GetLogPathInfos()
        {
            ObservableDictionary<string, string> logPathInfos = new ObservableDictionary<string, string>();
            try
            {
                logPathInfos.Add("FilePath", LoggerManager.LoggerManagerParam.FilePath);
                logPathInfos.Add("DEBUG", LoggerManager.LoggerManagerParam.DebugLoggerParam.LogDirPath);
                logPathInfos.Add("PROLOG", LoggerManager.LoggerManagerParam.ProLoggerParam.LogDirPath);
                logPathInfos.Add("EVENT", LoggerManager.LoggerManagerParam.EventLoggerParam.LogDirPath);
                logPathInfos.Add("GPIB", LoggerManager.LoggerManagerParam.GpibLoggerParam.LogDirPath);
                logPathInfos.Add("PIN", LoggerManager.LoggerManagerParam.PinLoggerParam.LogDirPath);
                logPathInfos.Add("PMI", LoggerManager.LoggerManagerParam.PMILoggerParam.LogDirPath);
                logPathInfos.Add("TEMP", LoggerManager.LoggerManagerParam.TempLoggerParam.LogDirPath);
                logPathInfos.Add("LOT", LoggerManager.LoggerManagerParam.LOTLoggerParam.LogDirPath);
                logPathInfos.Add("PARAM", LoggerManager.LoggerManagerParam.ParamLoggerParam.LogDirPath);
                logPathInfos.Add("TCPIP", LoggerManager.LoggerManagerParam.TCPIPLoggerParam.LogDirPath);
                logPathInfos.Add("SOAKING", LoggerManager.LoggerManagerParam.SoakingLoggerParam.LogDirPath);
                logPathInfos.Add("MONITORING", LoggerManager.LoggerManagerParam.MonitoringLoggerParam.LogDirPath);
                logPathInfos.Add("COGNEX", LoggerManager.LoggerManagerParam.CognexLoggerParam.LogDirPath);
                logPathInfos.Add("EXCEPTION", LoggerManager.LoggerManagerParam.ExceptionLoggerParam.LogDirPath);
                logPathInfos.Add("IMAGE", LoggerManager.LoggerManagerParam.ImageLoggerParam.LogDirPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return logPathInfos;
        }

        public EventCodeEnum SetLogPathInfos(ObservableDictionary<string, string> infos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (infos != null)
                {
                    var infoDic = infos.ToDictionary(i => i.Key, i => i.Value);
                    infoDic.TryGetValue("FilePath", out string path);
                    
                    if (infoDic.All(x => x.Value.Contains(path.Substring(0, 1))))
                    {
                        bool isNeedFileManagerInit = false;
                        LoggerManager.SetParameter(infos.ToDictionary(info => info.Key, info => info.Value), out isNeedFileManagerInit);
                        if (isNeedFileManagerInit)
                        {
                            this.FileManager().InitData();
                        }
                        retVal = EventCodeEnum.NONE;
                        
                        LoggerManager.Debug($"SetLogPathInfos() Success.");
                    }
                    else
                    {
                        LoggerManager.Debug($"SetLogPathInfos() Fail, The Root Directory of the Path for each log type must match 'FilePath'. FilePath : {LoggerManager.LoggerManagerParam.FilePath}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        //public ObservableDictionary<string, string> GetImageLogPathInfos()
        //{
        //    ObservableDictionary<string, string> logPathInfos = new ObservableDictionary<string, string>();
        //    try
        //    {
        //        logPathInfos.Add("[IMAGE_PMI]", this.FileManager().GetFileSavePath(EnumProberModule.PMI));
        //        logPathInfos.Add("[IMAGE_PIN]", this.FileManager().GetFileSavePath(EnumProberModule.PINALIGNER));
        //        logPathInfos.Add("[IMAGE_CC]", this.FileManager().GetFileSavePath(EnumProberModule.CARDCHANGE));
        //        logPathInfos.Add("[IMAGE_SOAKING]", this.FileManager().GetFileSavePath(EnumProberModule.SOAKING));
        //        logPathInfos.Add("[IMAGE_WAFER]", this.FileManager().GetFileSavePath(EnumProberModule.WAFERALIGNER));
        //        logPathInfos.Add("[IMAGE_PROCESSING]", this.FileManager().GetFileSavePath(EnumProberModule.VISIONPROCESSING));
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //    return logPathInfos;
        //}


        public ImageLoggerParam GetImageLoggerParam()
        {
            ImageLoggerParam imageLoggerParam = null;
            try
            {
                imageLoggerParam = LoggerManager.LoggerManagerParam.ImageLoggerParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return imageLoggerParam;
        }

        public void SetmageLoggerParam(ImageLoggerParam imageLoggerParam)
        {
            try
            {
                LoggerManager.LoggerManagerParam.ImageLoggerParam = imageLoggerParam;
                LoggerManager.SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public ImageDataSet GetImageDataSet(EnumProberModule moduletype, string moduleStartTime, string hashCode)
        {
            ImageDataSet retval = null;

            try
            {
                retval = this.VisionManager().GetImageDataSet(moduletype, moduleStartTime, hashCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        #endregion

        #region LightAdmin
        public void LightAdmin_LoadLUT()
        {
            try
            {
                this.StageSupervisor().LoadLUT();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region TCPIP
        public EventCodeEnum ReInitializeAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.TCPIPModule().ReInitializeAndConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }


        public EventCodeEnum CheckAndConnect()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.TCPIPModule().CheckAndConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum FoupAllocated(FoupAllocatedInfo allocatedInfo)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.TCPIPModule().FoupAllocated(allocatedInfo);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }


        public void SetOperatorName(string name)
        {
            try
            {
                if (name == null)
                {
                    this.LotOPModule().LotInfo.OperatorID.Value = string.Empty;
                }
                else
                {
                    this.LotOPModule().LotInfo.OperatorID.Value = name;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Update_Error_MSG(string err_msg)
        {
            try
            {
                if (GetServiceCallBack() != null)
                {
                    GetServiceCallBack().CallBack_Update_Error_MSG(err_msg);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

    }

}
