using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CallbackServices
{
    using Autofac;
    using LoaderBase.Communication;
    using LoaderBase.FactoryModules.ViewModelModule;
    using LoaderMapView;
    using LogModule;
    using NeedleCleanerModuleParameter;
    using ProbeCardObject;
    using ProberInterfaces;
    using ProberInterfaces.ControlClass.ViewModel;
    using ProberInterfaces.ControlClass.ViewModel.Wafer.Sequence;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Loader;
    using ProberInterfaces.Loader.RemoteDataDescription;
    using ProberInterfaces.LoaderController;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.PMI;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.SequenceRunner;
    using ProberInterfaces.State;
    using SerializerUtil;
    using SubstrateObjects;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;


    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    //[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ViewModelDataManagerCallbackService : ILoaderRemoteMediatorCallback, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Guid _LoaderDeviceChangeVMGuid { get; set; }
        private Guid _SequenceMakerVMGuid { get; set; }

        private Guid _LoaderGPCardChangeOPViewModelGUID { get; set; }

        //private Guid _LoaderMainVMGuid { get; set; }

        //private Guid _LoaderMainVMGuid { get; set; }


        #region //..Property

        private InstanceContext _InstanceContext;

        public InstanceContext InstanceContext
        {
            get { return _InstanceContext; }
            set { _InstanceContext = value; }
        }

        private Autofac.IContainer _Container => this.GetLoaderContainer();
        private IPnpManager PnpManager => _Container.Resolve<IPnpManager>();
        private ILoaderCommunicationManager _LoaderCommunicationManager => _Container.Resolve<ILoaderCommunicationManager>();
        private ILoaderViewModelManager _LoaderViewModelManager => _Container.Resolve<ILoaderViewModelManager>();

        public StageObject SelectedStageObj
        {
            get { return (StageObject)_LoaderCommunicationManager.SelectedStage; }
            set
            {
                if (value != (_LoaderCommunicationManager.SelectedStage))
                {
                    _LoaderCommunicationManager.SelectedStage = (StageObject)value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region //..Creator & Init
        public ViewModelDataManagerCallbackService()
        {
            InstanceContext = new InstanceContext(this);

            _LoaderDeviceChangeVMGuid = new Guid("2DD68236-928F-FCE8-C0F2-AFC89EBA752C");
            _SequenceMakerVMGuid = new Guid("33026329-59c0-42ca-8c48-b7fcffe4e821");
            _LoaderGPCardChangeOPViewModelGUID = new Guid("95061a72-d013-4dd8-a05d-899e77547ee9");

            // REMOVED
            //_LoaderMainVMGuid = new Guid("3491BBA7-8AF7-EE23-58DA-E028066E22A1");

            //_LoaderMainVMGuid = new Guid("6e199680-a422-4882-841d-cd4628a8c009");
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

        public async void SetWaferDevice(byte[] device)
        {
            var obj = await this.ByteArrayToObject(device);
            this.GetParam_Wafer().GetSubsInfo().DIEs = (IDeviceObject[,])obj;
        }

        public void ChangedWaferObjectDutCenter(double CenterX, double CenterY)
        {
            this.GetParam_Wafer().GetSubsInfo().DutCenX = CenterX;
            this.GetParam_Wafer().GetSubsInfo().DutCenY = CenterY;
        }

        public void ChangedProbeCardObjectDutCenter(double CenterX, double CenterY)
        {
            this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutCenX = CenterX;
            this.GetParam_ProbeCard().ProbeCardDevObjectRef.DutCenY = CenterY;
        }

        public async Task RequestGetWaferObject()
        {
            try
            {
                var stage = _LoaderCommunicationManager.GetProxy<IStageSupervisorProxy>();
                var waferObject = stage.GetWaferObject();

                waferObject.Init();
                waferObject.WaferDevObjectRef.Init();

                this.StageSupervisor().WaferObject = waferObject;

                if (this.StageSupervisor().WaferObject != null)
                {
                    this.StageSupervisor().WaferObject.AlignState = stage.GetAlignState(AlignTypeEnum.Wafer);
                }

                var substratenonserial = stage.GetSubstrateInfoNonSerialized();

                ISubstrateInfo subsinfo = waferObject.GetSubsInfo();

                subsinfo.ActualDeviceSize = substratenonserial.ActualDeviceSize;
                subsinfo.ActualDieSize = substratenonserial.ActualDieSize;
                subsinfo.WaferCenter = substratenonserial.WaferCenter;
                subsinfo.WaferObjectChangedToggle = substratenonserial.WaferObjectChangedToggle;

                subsinfo.DIEs = await stage.GetConcreteDIEs();
                subsinfo.DutCenX = substratenonserial.DutCenX;
                subsinfo.DutCenY = substratenonserial.DutCenY;

                if (this.PnPManager().SelectedPnpStep != null)
                {
                    if (this.PnPManager().SelectedPnpStep.MainViewTarget is IWaferObject)
                    {
                        this.PnPManager().SelectedPnpStep.MainViewTarget = null;
                        this.PnPManager().SelectedPnpStep.MainViewTarget = waferObject;
                    }
                    else if (this.PnPManager().SelectedPnpStep.MiniViewTarget is IWaferObject)
                    {
                        this.PnPManager().SelectedPnpStep.MiniViewTarget = null;
                        this.PnPManager().SelectedPnpStep.MiniViewTarget = waferObject;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public void UpdateDockChangeStateObject(byte[] seq)
        //{
        //    try
        //    {    
        //        ObservableCollection<ISequenceBehaviorGroupItem> deserializeObjcet =
        //            ObjectSerialize.DeSerialize(seq) as ObservableCollection<ISequenceBehaviorGroupItem>;

        //        var vm = this.ViewModelManager().GetViewModelFormGuid(_LoaderGPCardChangeOPViewModelGUID);

        //        if (vm != null)
        //        {
        //            (vm as ILoaderGPCardChangeOPViewModel).UpdateSequence_Dock_EnumState(deserializeObjcet);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        //public void UpdateUnDockChangeStateObject(byte[] seq)
        //{
        //    try
        //    {
        //        ObservableCollection<ISequenceBehaviorGroupItem> deserializeObjcet =
        //            ObjectSerialize.DeSerialize(seq) as ObservableCollection<ISequenceBehaviorGroupItem>;

        //        var vm = this.ViewModelManager().GetViewModelFormGuid(_LoaderGPCardChangeOPViewModelGUID);

        //        if (vm != null)
        //        {
        //            (vm as ILoaderGPCardChangeOPViewModel).UpdateSequence_UnDock_EnumState(deserializeObjcet);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}
        public void CallBack_Update_Error_MSG(string err_msg)
        {
            try
            {
                string error_MSG = err_msg;

                var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderGPCardChangeOPViewModelGUID);
                if (vm != null)
                {
                    (vm as ILoaderGPCardChangeOPViewModel).Update_ErrorMessage(error_MSG);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetDieType(long xindex, long yindex, DieTypeEnum dietype)
        {
            this.GetParam_Wafer().GetSubsInfo().DIEs[xindex, yindex].DieType.Value = dietype;
        }

        public void UpdateDutPadInfos(byte[] dutpadinfos)
        {
            try
            {
                var obj = this.ByteArrayToObjectSync(dutpadinfos);

                if (obj != null)
                {
                    this.GetParam_Wafer().GetSubsInfo().Pads.DutPadInfos = obj as List<DUTPadObject>;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StageDataLoaded()
        {
            LoggerManager.Debug($"StageDataLoaded(): Stage data has been updated.");
        }


        #region //..CallBack

        #region //..PNP

        public void PNPButtonsUpdated(PNPDataDescription pNPDataDescriptor)
        {
            try
            {
                if (PnpManager.SelectedPnpStep != null)
                {
                    PnpManager.SelectedPnpStep.SetPNPDataDescriptor(pNPDataDescriptor);
                    _LoaderViewModelManager.WaitUIUpdate("PNPButtons");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void JogModeUpdated(JogMode mode)
        {
            try
            {
                if (PnpManager.SelectedPnpStep != null)
                {
                    PnpManager.SelectedPnpStep.JogType = mode;
                    _LoaderViewModelManager.WaitUIUpdate("JogMode");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void WaferObjectInfoNonSerializeUpdated(WaferObjectInfoNonSerialized waferobjinfo)
        {
            try
            {
                this.StageSupervisor().WaferObject.MapViewControlMode = waferobjinfo?.MapViewRenderMode ?? MapViewMode.UNDIFIND;
                this.StageSupervisor().WaferObject.MapViewStageSyncEnable = waferobjinfo.MapViewStageSyncEnable;
                this.StageSupervisor().WaferObject.MapViewCurIndexVisiablity = waferobjinfo.MapViewCurIndexVisiablity;

                this.StageSupervisor().WaferObject.TopLeftToBottomRightLineVisible = waferobjinfo.TopLeftToBottomRightLineVisible;
                this.StageSupervisor().WaferObject.BottomLeftToTopRightLineVisible = waferobjinfo.BottomLeftToTopRightLineVisible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void WaferIndexUpdated(long xindex, long yindex)
        {
            try
            {
                //if (SelectedStageObj != null)
                //{
                //    if (SelectedStageObj.StageInfo.WaferObject != null)
                //    {
                //        SelectedStageObj.StageInfo.WaferObject.SetCurrentMIndex(xindex, yindex);
                //    }
                //}

                // TODO : Need check
                //this.StageSupervisor().WaferObject?.SetCurrentMIndex(xindex, yindex);

                if ((this.ViewModelManager() as ILoaderViewModelManager).Camera != null)
                {
                    if ((this.ViewModelManager() as ILoaderViewModelManager).Camera.IsMovingPos == false)
                    {
                        this.StageSupervisor().WaferObject?.SetCurrentMIndex(xindex, yindex);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


        public void UpdatgePMITemplateMiniViewModel(PMITemplateMiniViewModel vm)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    IHasPMITemplateMiniViewModel HasVm = (PnpManager.SeletedStep as IHasPMITemplateMiniViewModel);

                    if (HasVm != null)
                    {
                        if (HasVm.PMITemplateMiniViewModel != null)
                        {
                            HasVm.PMITemplateMiniViewModel.SelectedPadTemplateIndex = vm.SelectedPadTemplateIndex;
                            HasVm.PMITemplateMiniViewModel.PadTemplateInfoCount = vm.PadTemplateInfoCount;
                            HasVm.PMITemplateMiniViewModel.DrawingGroup = vm.DrawingGroup;

                            if (vm.SelectedPadTemplate != null)
                            {
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.Shape.Value = vm.SelectedPadTemplate.Shape.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.TemplateOffset.Value = vm.SelectedPadTemplate.TemplateOffset.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.TemplateCornerRadius.Value = vm.SelectedPadTemplate.TemplateCornerRadius.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.Area.Value = vm.SelectedPadTemplate.Area.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.Color.Value = vm.SelectedPadTemplate.Color.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.PathData.Value = vm.SelectedPadTemplate.PathData.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.ShapeName.Value = vm.SelectedPadTemplate.ShapeName.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.Angle.Value = vm.SelectedPadTemplate.Angle.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.SizeX.Value = vm.SelectedPadTemplate.SizeX.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.SizeY.Value = vm.SelectedPadTemplate.SizeY.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.JudgingWindowSizeX.Value = vm.SelectedPadTemplate.JudgingWindowSizeX.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.JudgingWindowSizeY.Value = vm.SelectedPadTemplate.JudgingWindowSizeY.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.JudgingWindowMode.Value = vm.SelectedPadTemplate.JudgingWindowMode.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.EdgeOffsetMode.Value = vm.SelectedPadTemplate.EdgeOffsetMode.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.CornerRadiusMode.Value = vm.SelectedPadTemplate.CornerRadiusMode.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.MarkWindowMinSizeX.Value = vm.SelectedPadTemplate.MarkWindowMinSizeX.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.MarkWindowMinSizeY.Value = vm.SelectedPadTemplate.MarkWindowMinSizeY.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.MarkWindowMaxSizeX.Value = vm.SelectedPadTemplate.MarkWindowMaxSizeX.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.MarkWindowMaxSizeY.Value = vm.SelectedPadTemplate.MarkWindowMaxSizeY.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.MarkWindowMinPercent.Value = vm.SelectedPadTemplate.MarkWindowMinPercent.Value;
                                HasVm.PMITemplateMiniViewModel.SelectedPadTemplate.MarkWindowMaxPercent.Value = vm.SelectedPadTemplate.MarkWindowMaxPercent.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetupStateUpdated(EnumMoudleSetupState state, bool isparent)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    PnpManager.SeletedStep.SetNodeSetupState(state, isparent);
                    _LoaderViewModelManager.WaitUIUpdate("SetupState");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetupStateHeaderUpdated(EnumMoudleSetupState state, bool isparent, string header, string recoveryHeader)
        {
            try
            {
                bool isChange = false;
                if (PnpManager.PnpSteps != null)
                {
                    foreach (var pnp in PnpManager.PnpSteps)
                    {
                        foreach (var step in pnp)
                        {
                            if (step.Header == header || step.Header == recoveryHeader)
                            {
                                step.SetNodeSetupState(state, isparent);
                                _LoaderViewModelManager.WaitUIUpdate("SetupState");
                                isChange = true;
                                break;
                            }
                        }
                    }
                }
                if (isChange == false)
                {
                    if (PnpManager.SeletedStep != null)
                    {
                        PnpManager.SeletedStep.SetNodeSetupState(state, isparent);
                        _LoaderViewModelManager.WaitUIUpdate("SetupState");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetupRecoveryStateHeaderUpdated(EnumMoudleSetupState state, bool isparent, string header, string recoveryHeader)
        {
            try
            {
                bool isChange = false;
                if (PnpManager.PnpSteps != null)
                {
                    foreach (var pnp in PnpManager.PnpSteps)
                    {
                        foreach (var step in pnp)
                        {
                            if (step.Header == header || step.Header == recoveryHeader)
                            {
                                step.SetNodeSetupRecoveryState(state, isparent);
                                _LoaderViewModelManager.WaitUIUpdate("SetupState");
                                isChange = true;
                                break;
                            }
                        }
                    }
                }
                if (isChange == false)
                {
                    if (PnpManager.SeletedStep != null)
                    {
                        PnpManager.SeletedStep.SetNodeSetupRecoveryState(state, isparent);
                        _LoaderViewModelManager.WaitUIUpdate("SetupState");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void StepLabelUpdated(string label)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).StepLabel = label;
                    _LoaderViewModelManager.WaitUIUpdate("StepLabel");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StepSecondLabelUpdated(string label)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).StepSecondLabel = label;
                    _LoaderViewModelManager.WaitUIUpdate("StepSecondLabel");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StepLabelActiveUpdated(bool active)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).StepLabelActive = active;
                    _LoaderViewModelManager.WaitUIUpdate("StepLabelActive");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StepSecondLabelActiveUpdated(bool active)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).StepSecondLabelActive = active;
                    _LoaderViewModelManager.WaitUIUpdate("StepSecondLabelActive");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void PNPMainViewImageSourceUpdated(byte[] imagesource)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {

                    //(PnpManager.SeletedStep as IPnpSetup).MainViewImageSource = biImg;

                    //BitmapImage biImg = new BitmapImage();
                    //MemoryStream ms = new MemoryStream(imagesource);

                    ////biImg.StreamSource = ms;
                    //Task task = new Task(() =>
                    //{
                    //    biImg.BeginInit();
                    //    ms.CopyTo(biImg.StreamSource);
                    //    biImg.EndInit();
                    //    biImg.Freeze();
                    //});
                    //task.Start();
                    //await task;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BitmapImage biImg = new BitmapImage();
                        using (MemoryStream ms = new MemoryStream(imagesource))
                        {
                            biImg.BeginInit();

                            biImg.StreamSource = ms;
                            biImg.CacheOption = BitmapCacheOption.OnLoad;
                            biImg.EndInit();
                            biImg.StreamSource = null;
                            biImg.Freeze();
                            (PnpManager.SeletedStep as IPnpSetup).MainViewImageSource = biImg;
                        }
                    });

                    _LoaderViewModelManager.WaitUIUpdate("PNPMainViewImageSource");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DisplayPortUserControlUpdated(UserControlFucEnum dUserControl)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).UseUserControl = dUserControl;
                    _LoaderViewModelManager.WaitUIUpdate("DisplayPortUserControl");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ImageBufferUpdated(ImageBuffer imgBuffer)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).MiniViewTarget = imgBuffer;
                    (PnpManager.SeletedStep as IPnpSetup).ImgBuffer = imgBuffer;
                    _LoaderViewModelManager.WaitUIUpdate("ImgBuffer");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void DisplayPortRetangleUpdated(double width, double height)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    (PnpManager.SeletedStep as IPnpSetup).TargetRectangleWidth = width;
                    (PnpManager.SeletedStep as IPnpSetup).TargetRectangleHeight = height;

                    //_LoaderViewModelManager.WaitUIUpdate("DisplayPortRetangle");  
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void PNPLightJogUpdated(EnumLightType lighttype, int intensity)
        {
            try
            {
                if (PnpManager.SeletedStep != null)
                {
                    if (PnpManager.PnpLightJog.CurCameraLightType == lighttype)
                    {
                        PnpManager.PnpLightJog.CurCameraLightValue = intensity;
                    }

                    _LoaderViewModelManager.WaitUIUpdate("PNPLightJog");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region //..NCObject

        public void SetNCObjectUpdated(byte[] ncobject)
        {
            try
            {
                object target = null;
                SerializeManager.DeserializeFromByte(ncobject, out target, typeof(NeedleCleanObject));
                if (target != null)
                    this.StageSupervisor().NCObject = (NeedleCleanObject)target;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void NCSheetVMDefsUpdated(byte[] ncsheetvmedfs)
        {
            try
            {
                object target = null;
                SerializeManager.DeserializeFromByte(ncsheetvmedfs, out target, typeof(NCSheetDefsInfo));
                if (target != null)
                {
                    NCSheetDefsInfo nCSheetDefs = (NCSheetDefsInfo)target;
                    ObservableCollection<NCSheetVMDefinition> updatesheet = nCSheetDefs.NCSheetVMDefs;
                    ObservableCollection<NCSheetVMDefinition> sheets = new ObservableCollection<NCSheetVMDefinition>();
                    foreach (var sheet in updatesheet)
                    {
                        sheets.Add(sheet);
                    }
                    (this.StageSupervisor().NCObject as NeedleCleanObject).NCSheetVMDefs = sheets;
                    nCSheetDefs.NCSheetVMDef.CopyTo((this.StageSupervisor().NCObject as NeedleCleanObject).NCSheetVMDef);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void NCSequencesInfoUpdated(byte[] ncsequencesinfo)
        {
            try
            {
                object target = null;
                SerializeManager.DeserializeFromByte(ncsequencesinfo, out target, typeof(NCSequencesInfo));
                if (target != null)
                {
                    NCSequencesInfo nCSequences = (NCSequencesInfo)target;
                    NeedleCleanObject ncobj = (NeedleCleanObject)this.StageSupervisor().NCObject;
                    ncobj.Index = nCSequences.Index;
                    ncobj.TotalCount = nCSequences.TotalCount;
                    ncobj.DistanceVisible = nCSequences.DistanceVisible;
                    ncobj.ImageSource = nCSequences.ImageSource;
                    ObservableCollection<IDut> duts = new ObservableCollection<IDut>();
                    foreach (var dut in nCSequences.TempDutList)
                    {
                        duts.Add(dut);
                    }
                    ncobj.TempDutList = duts;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #endregion

        #region //..Probing Sequence

        public void SequenceMakerVM_SetMXYIndex(Point mindex)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
                if (vm != null)
                {
                    (vm as ISequenceMakerVM).IsCallbackEntry = true;
                    (vm as ISequenceMakerVM).MXYIndex = mindex;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public Task SequenceMakerVM_UpdateDeviceObject(List<ExistSeqs> list)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_SequenceMakerVMGuid);
                if (vm != null)
                {
                    (vm as ISequenceMakerVM).UpdateDeviceObject(list);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }

        #endregion
        #region Device Change

        //public void UpdateShowingDevicelist(byte[] showingdeiveinfos)
        //{
        //    try
        //    {
        //        var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderDeviceChangeVMGuid);
        //        if (vm != null)
        //        {
        //            object showingdeiveinfosojb;

        //            //var result = SerializeManager.DeserializeFromByte(showingdeiveinfos, out showingdeiveinfosojb, typeof(ObservableCollection<DeviceInfo>));

        //            showingdeiveinfosojb = SerializeManager.ObjectToByte(showingdeiveinfos);

        //            if (showingdeiveinfosojb != null)
        //            {
        //                (vm as IDeviceChangeVM).ShowingDeviceInfoCollection = showingdeiveinfosojb as ObservableCollection<DeviceInfo>;
        //            }
        //            else
        //            {
        //                LoggerManager.Error($"UpdateShowingDevicelist() DeserializeFromByte error");
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //}

        public async Task UpdateShowingDevicelist(List<DeviceInfo> showingdeiveinfos)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderDeviceChangeVMGuid);

                if (vm != null)
                {
                    ObservableCollection<DeviceInfo> myCollection = new ObservableCollection<DeviceInfo>(showingdeiveinfos as List<DeviceInfo>);

                    //(vm as IDeviceChangeVM).ShowingDeviceInfoCollection = myCollection;

                    await (vm as IDeviceChangeVM).SetShowingDeviceInfoCollectio(myCollection);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }


        }

        public async Task UpdateShowingDevice(DeviceInfo showingdevice, byte[] wafer, byte[] dies, SubstrateInfoNonSerialized subNonSerialized, byte[] probecard)
        {
            try
            {
                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetType().ToString(), "Wait");

                var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderDeviceChangeVMGuid);
                if (vm != null)
                {
                    object WaferTarget = null;
                    IDeviceObject[,] DiesTarget = null;
                    object ProbeCardTarget = null;

                    var result = SerializeManager.DeserializeFromByte(wafer, out WaferTarget, typeof(WaferObject));

                    if (result == true)
                    {
                        showingdevice.DutViewControl.WaferObject = (WaferObject)WaferTarget;

                        ISubstrateInfo subsinfo = showingdevice.DutViewControl.WaferObject.GetSubsInfo();

                        var obj = await this.ByteArrayToObject(dies);
                        DiesTarget = (IDeviceObject[,])obj;
                        subsinfo.DIEs = DiesTarget;
                        subsinfo.ActualDeviceSize = subNonSerialized.ActualDeviceSize;
                        subsinfo.ActualDieSize = subNonSerialized.ActualDieSize;
                        subsinfo.WaferCenter = subNonSerialized.WaferCenter;
                        subsinfo.WaferObjectChangedToggle = subNonSerialized.WaferObjectChangedToggle;
                        showingdevice.DutViewControl.WaferObject.MapViewControlMode = MapViewMode.MapMode;
                    }
                    else
                    {
                        LoggerManager.Error("DeserializeFromByte Error. WaferObject");
                    }

                    result = SerializeManager.DeserializeFromByte(probecard, out ProbeCardTarget, typeof(ProbeCard));

                    if (result == true)
                    {
                        showingdevice.DutViewControl.ProbeCard = (ProbeCard)ProbeCardTarget;
                    }
                    else
                    {
                        LoggerManager.Error("DeserializeFromByte Error. ProbeCard");
                    }

                    this.StageSupervisor().ProbeCardInfo = showingdevice.DutViewControl.ProbeCard;
                    showingdevice.DutViewControl.StageSupervisor = this.StageSupervisor();
                    showingdevice.DutViewControl.VisionManager = this.StageSupervisor().VisionManager();
                    (vm as IDeviceChangeVM).ShowingDevice = showingdevice;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetType().ToString());
            }
        }

        #endregion

        #region //..PMI
        public void PMIInfoUpdated(byte[] pmiinfo)
        {
            try
            {
                var obj = SerializeManager.ByteToObject(pmiinfo);
                if (obj != null)
                {
                    PMIInfo pmi = (this.GetParam_Wafer() as WaferObject).WaferDevObject?.Info?.PMIInfo;
                    PMIInfo objpmi = (PMIInfo)obj;

                    if (pmi != null)
                    {
                        // TODO : Data 잘 올라오는지 확인해야 됨.
                        // PMIInfo.WaferTemplateInfo
                        // PMIInfo.NormalPMIMapTemplateInfo

                        // Selected Index
                        //pmi.SelectedWaferTemplateIndex = objpmi.SelectedWaferTemplateIndex;
                        //pmi.SelectedPadTemplateIndex = objpmi.SelectedPadTemplateIndex;
                        //pmi.SelectedNormalPMIMapTemplateIndex = objpmi.SelectedNormalPMIMapTemplateIndex;
                        //pmi.SelectedPadTableTemplateIndex = objpmi.SelectedPadTableTemplateIndex; 

                        //pmi.SetupMode = objpmi.SetupMode;

                        //if (pmi.SelectedPadTemplate != null)
                        //{
                        //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        //    {
                        //        Geometry g = Geometry.Parse(pmi.SelectedPadTemplate.PathData.Value);

                        //        if (pmi.SelectedPadTemplatePath == null)
                        //        {
                        //            pmi.SelectedPadTemplatePath = new System.Windows.Shapes.Path();
                        //        }

                        //        pmi.SelectedPadTemplatePath.Data = g;
                        //    }));
                        //}
                    }
                }
                //(this.GetParam_Wafer() as WaferObject).WaferDevObject.Info.PMIInfo = (PMIInfo)obj;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdatgeNormalPMIMapTemplateInfo(byte[] infos)
        {
            try
            {
                object target = null;

                bool vaild = SerializeManager.DeserializeFromByte(infos, out target, typeof(NormalPMIMapTemplateInfo));

                if (vaild == true && target != null)
                {
                    NormalPMIMapTemplateInfo des = target as NormalPMIMapTemplateInfo;

                    int index = des.SelectedNormalPMIMapTemplateIndex;
                    DieMapTemplate dietemplate = des.SelectedNormalPMIMapTemplate;

                    this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().SelectedNormalPMIMapTemplateIndex = index;
                    this.StageSupervisor().WaferObject.GetSubsInfo().GetPMIInfo().NormalPMIMapTemplateInfo[index] = dietemplate;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangedIsMapViewShowPMITable(bool flag)
        {
            try
            {
                this.StageSupervisor().WaferObject.IsMapViewShowPMITable = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void ChangedIsMapViewShowPMIEnable(bool flag)
        {
            try
            {
                this.StageSupervisor().WaferObject.IsMapViewShowPMIEnable = flag;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ChangedIsMapViewControlMode(MapViewMode mode)
        {
            try
            {
                this.StageSupervisor().WaferObject.MapViewControlMode = mode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public void UploadFile(byte[] bytestream, string filepath)
        {
            try
            {
                var deviceManager = _Container.Resolve<IDeviceManager>();
                string devname = SelectedStageObj.StageInfo.DeviceName;
                string devpath = deviceManager.GetLoaderDevicePath();
                var filename = filepath.Substring(filepath.IndexOf(devname));
                string filpath = String.Concat(devpath, "\\", filename);
                using (Stream stream = new MemoryStream(bytestream))
                {
                    this.DecompressFilesFromByteArray(stream, filpath);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion


        public void SetProbingDevices(int index, ObservableCollection<IDeviceObject> devs)
        {
            if (_LoaderCommunicationManager.SelectedStage != null)
            {
                if (_LoaderCommunicationManager.SelectedStage.Index == index)
                    this.ProbingModule().ProbingProcessStatus.UnderDutDevs = devs;
            }
        }

        public void ClearUnderDutDevs(int index)
        {
            try
            {
                if (_LoaderCommunicationManager.SelectedStage != null)
                {
                    if (_LoaderCommunicationManager.SelectedStage.Index == index)
                    {
                        if (this.ProbingModule().ProbingProcessStatus?.UnderDutDevs != null)
                        {
                            this.ProbingModule().ProbingProcessStatus.UnderDutDevs.Clear();
                        }
                        else
                        {
                            this.ProbingModule().ProbingProcessStatus.UnderDutDevs = new System.Collections.ObjectModel.ObservableCollection<IDeviceObject>();
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"ClearUnderDutDevs(): Stage index {index} Selected Stage index {_LoaderCommunicationManager.SelectedStage.Index}.");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private object lockobj = new object();
        public void SetContactPosInfo(Point mindex, bool iszupstate)
        {
            // CallBack으로 호출됐다는 것은, 데이터만 업데이트를 하고 싶은 것이지, Setter에서 추가 동작을 원하는 것은 아니다.
            // 따라서, 플래그를 MXYIndex Setter에서 사용할 것.
            lock (lockobj)
            {
                this.ManualContactModule().IsCallbackEntry = true;

                this.ManualContactModule().MXYIndex = mindex;
                this.ManualContactModule().IsZUpState = iszupstate;
            }
        }

        public void SetMachinePosition(MachinePosition machinepos)
        {
            this.ManualContactModule().MachinePosition = machinepos;
        }

        //public void UpdateSoakingInfo(SoakingInfo soakinfo)
        //{
        //    try
        //    {
        //        var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderStageSummaryVMGuid);
        //        if (vm != null)
        //        {
        //            if (_LoaderCommunicationManager.SelectedStageIndex == soakinfo.ChuckIndex)
        //            {
        //                //(vm as ILoaderMainVM).SoakingType = soakinfo.SoakingType;
        //                //(vm as ILoaderMainVM).SoakRemainTIme = Math.Abs(soakinfo.RemainTime).ToString();

        //                (vm as ILoaderStageSummaryViewModel).SoakingType = soakinfo.SoakingType;
        //                (vm as ILoaderStageSummaryViewModel).SoakZClearance = soakinfo.ZClearance.ToString();
        //                (vm as ILoaderStageSummaryViewModel).SoakRemainTIme = Math.Abs(soakinfo.RemainTime).ToString();
        //            }

        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public void UpdateStageMove(StageMoveInfo info)
        {
            try
            {
                LoggerManager.Debug($"[ViewModelDataManagerCallbackService]UpdateStageMove Start");
                //var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderMainVMGuid);
                var vm = this.ViewModelManager().GetViewModelFromViewGuid(this.ViewModelManager().HomeViewGuid);

                if (vm != null)
                {
                    if (_LoaderCommunicationManager.SelectedStageIndex == info.ChuckIndex)
                    {
                        (vm as ILoaderStageSummaryViewModel).StageMoveState = info.StageMove;
                        LoggerManager.Debug($" Cell#{info.ChuckIndex} - UpdateStageMove()");
                    }
                }
                LoggerManager.Debug($"[ViewModelDataManagerCallbackService]UpdateStageMove End");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public void ChangedMapDirection(MapHorDirectionEnum MapDirX, MapVertDirectionEnum MapDirY)
        {
            try
            {
                this.StageSupervisor().WaferObject.GetPhysInfo().MapDirX.Value = MapDirX;
                this.StageSupervisor().WaferObject.GetPhysInfo().MapDirY.Value = MapDirY;

                var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(this.StageSupervisor().WaferObject.GetPhysInfo().CenM.XIndex.Value, this.StageSupervisor().WaferObject.GetPhysInfo().CenM.YIndex.Value);
                this.StageSupervisor().WaferObject.GetPhysInfo().CenU.XIndex.Value = userIndex.XIndex;
                this.StageSupervisor().WaferObject.GetPhysInfo().CenU.YIndex.Value = userIndex.YIndex;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void MapScreenToLotScreen(int index)
        {
            try
            {
                if (_LoaderCommunicationManager.SelectedStage != null)
                {
                    if (_LoaderCommunicationManager.SelectedStage.Index == index)
                    {
                        this.LotOPModule().MapScreenToLotScreen();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void VisionScreenToLotScreen(int index)
        {
            try
            {
                if (_LoaderCommunicationManager.SelectedStage.Index == index)
                {
                    this.LotOPModule().VisionScreenToLotScreen();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetStageMode(int index, GPCellModeEnum cellmode)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StageJobFinished()
        {
            try
            {
                _LoaderCommunicationManager.SetStageWorkingFlag(false);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateStopOptionToStage(int index)
        {
            try
            {
                //var vm = this.ViewModelManager().GetViewModelFromGuid(_LoaderMainVMGuid);
                var vm = this.ViewModelManager().GetViewModelFromGuid(this.ViewModelManager().HomeViewGuid);

                if (vm != null)
                {
                    (vm as ILoaderStageSummaryViewModel).StopBeforeProbingCommandFunc(index);
                    (vm as ILoaderStageSummaryViewModel).StopAfterProbingCommandFunc(index);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public byte[] GetProberCardListFromCCSysparam()
        {
            byte[] retVal = null;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void UpdateDeviceChangeInfo(ChuckPlanarityDataDescription info)
        {
            try
            {
                var vm = this.ViewModelManager().GetViewModelFromGuid(new Guid("1754d2b9-4dca-41c3-b8fa-0feaf7c01f3b"));

                if (vm != null)
                {
                    (vm as IChuckPlanarityVM).UpdateDeviceChangeInfo(info);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
