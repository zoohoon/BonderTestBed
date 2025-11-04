using System;
using System.Collections.Generic;

namespace LoaderServiceClientModules.PMIModule
{
    using Autofac;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using LoaderBase.Communication;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Command;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Param;
    using LogModule;
    using ProberInterfaces.Template;
    using SerializerUtil;
    using ProberInterfaces.PMI;
    using System.Windows;
    using SharpDXRender;
    using PMIModuleParameter;

    public class PMIModuleServiceClient : IPMIModule, INotifyPropertyChanged, IFactoryModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String info = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //private Autofac.IContainer _Container;
        private ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>();
            }
        }

        public bool Initialized
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    if (LoaderCommunicationManager.SelectedStage.StageInfo.IsConnected == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            set { }
        }
        public EnumCommunicationState CommunicationState
        {
            get
            {
                if (LoaderCommunicationManager.SelectedStage != null)
                {
                    return EnumCommunicationState.CONNECTED;
                }
                else
                {
                    return EnumCommunicationState.UNAVAILABLE;
                }
            }
            set { }
        }

        #region // IStateModule Implementation. Does not need on remote system.
        public ReasonOfError ReasonOfError
        {
            get
            {
                return new ReasonOfError(ModuleEnum.Temperature);
            }
            set => throw new NotImplementedException();
        }
        public CommandSlot CommandSendSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvProcSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandSlot CommandRecvDoneSlot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandTokenSet RunTokenSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandInformation CommandInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ModuleStateBase ModuleState => throw new NotImplementedException();

        public ObservableCollection<TransitionInfo> TransitionInfo => throw new NotImplementedException();

        public EnumModuleForcedState ForcedDone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanExecute(IProbeCommandToken token)
        {
            throw new NotImplementedException();
        }

        public void StateTransition(ModuleStateBase state)
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Execute()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Pause()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Resume()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum End()
        {
            throw new NotImplementedException();
        }

        public ModuleStateEnum Abort()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum ClearState()
        {
            throw new NotImplementedException();
        }

        public string GetModuleMessage()
        {
            string retval = string.Empty;

            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public bool IsBusy()
        {
            throw new NotImplementedException();
        }


        public bool IsLotReady(out string msg)
        {
            bool retval = true;
            try
            {
                msg = "";
                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }
        public EventCodeEnum ParamValidation()
        {
            throw new NotImplementedException();
        }

        public bool IsParameterChanged(bool issave = false)
        {
            throw new NotImplementedException();
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"PolishWaferModuleServiceClient() Function error: " + err.Message);
            }

            Initialized = false;
        }

        public EventCodeEnum InitModule()
        {
            Initialized = true;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum ResetPMIData()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum InitPMIResult()
        {
            throw new NotImplementedException();
        }

        public bool IsTurnOnPMIInLotRun()
        {
            throw new NotImplementedException();
        }

        public bool IsLastProbingSeqProceessd()
        {
            throw new NotImplementedException();
        }

        public bool GetPMIEnableParam()
        {
            throw new NotImplementedException();
        }

        public void ClearRenderObjects()
        {
            throw new NotImplementedException();
        }

        public RenderLayer InitPMIRenderLayer(Size size, float r, float g, float b, float a)
        {
            throw new NotImplementedException();
        }

        public Size GetLayerSize()
        {
            throw new NotImplementedException();
        }

        public void ChangeTemplateSizeCommand(JOG_DIRECTION direction)
        {
            throw new NotImplementedException();
        }

        public void ChangeTemplateOffsetCommand(SETUP_DIRECTION direction)
        {
            throw new NotImplementedException();
        }

        public void ChangeTemplateAngleCommand()
        {
            throw new NotImplementedException();
        }

        public void ChangeJudgingWindowSizeCommand(JOG_DIRECTION direction)
        {
            throw new NotImplementedException();
        }

        public void ChangeMarkSizeCommand(MARK_SIZE curMode, JOG_DIRECTION direction)
        {
            throw new NotImplementedException();
        }

        public void ChangePadPositionCommand(SELECTION_MODE mode, JOG_DIRECTION direction)
        {
            throw new NotImplementedException();
        }

        public void AddTemplateCommand(PAD_SHAPE shape, string name, PAD_COLOR color, double offset = 0.1)
        {
            throw new NotImplementedException();
        }

        public void DeleteTemplateCommand()
        {
            throw new NotImplementedException();
        }

        public void ChangeTemplateColorCommand(PAD_COLOR padColor)
        {
            throw new NotImplementedException();
        }

        public void ChangeTemplateIndexCommand(SETUP_DIRECTION direction)
        {
            throw new NotImplementedException();
        }

        public void SetSubModule(object SubModule)
        {
            throw new NotImplementedException();
        }

        public void UpdateCurrentPadIndex()
        {
            throw new NotImplementedException();
        }

        public void UpdateRenderLayer()
        {
            throw new NotImplementedException();
        }

        public Point GetRenderLayerRatio()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MovedDelegate(ImageBuffer Img)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FindPad()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoPMI()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum UpdateGroupingInformation()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum PadGroupingMethod(int curPMIPadTableNum)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MakeGroupSequence(int TableIndex)
        {
            throw new NotImplementedException();
        }

        public bool CheckPMIDieExist()
        {
            throw new NotImplementedException();
        }

        public bool CheckCurWaferPMIEnable()
        {
            throw new NotImplementedException();
        }

        public bool CheckFocusingInterval(int DutNo)
        {
            throw new NotImplementedException();
        }

        public void PadIndexMoveCommand(object direction)
        {
            throw new NotImplementedException();
        }

        public void TableIndexMoveCommand(object direction)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum DoPMIProcessing()
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> GetPnpSteps()
        {
            return null;
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IPMIModuleProxy>()?.LoadDevParameter() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager
            //    .GetPMIModuleClient()?.LoadDevParameter() ?? EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IPMIModuleProxy>()?.SaveDevParameter() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetPMIModuleClient().SaveDevParameter();
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IPMIModuleProxy>()?.InitDevParameter() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetPMIModuleClient()?.InitDevParameter() ?? EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IPMIModuleProxy>()?.LoadSysParameter() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetPMIModuleClient()?.LoadSysParameter() ?? EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoaderCommunicationManager.GetProxy<IPMIModuleProxy>()?.SaveSysParameter() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;

            //return LoaderCommunicationManager.GetPMIModuleClient()?.SaveSysParameter() ?? EventCodeEnum.UNDEFINED;
        }

        public IParam GetPMIDevIParam()
        {
            return PMIDevParam();
        }

        public PMIModuleDevParam PMIDevParam()
        {
            byte[] obj = GetPMIDevParam();
            object target = null;

            PMIModuleDevParam retval = null;

            if (obj != null)
            {
                var result = SerializeManager.DeserializeFromByte(obj, out target, typeof(PMIModuleDevParam));
                retval = target as PMIModuleDevParam;
            }

            return retval;
        }

        public byte[] GetPMIDevParam()
        {
            byte[] retval = null;

            IRemoteMediumProxy proxy = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>();

            if (proxy != null)
            {
                retval = proxy.GetPMIDevParam();
            }

            return retval;
        }

        public EventCodeEnum SetPMITrigger(PMIRemoteTriggerEnum trigger)
        {
            throw new NotImplementedException();
        }

        public void PMIInfoUpdatedToLoader()
        {
            throw new NotImplementedException();
        }

        public void AddPadTemplate(PadTemplate template)
        {
            try
            {
                LoaderCommunicationManager.GetProxy<IPMIModuleProxy>().AddPadTemplate(template);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //LoaderCommunicationManager.GetPMIModuleClient().AddPadTemplate(template);

            //IPnpManager PnpManager = this.GetLoaderContainer().Resolve<IPnpManager>();

            //(PnpManager.CurStep as IHasPMITemplateMiniViewModel)?.UpdatePMITemplateMiniViewModel();
        }

        public void ChangedPadTemplate(PadTemplate template)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum FindPad(PadTemplate padtemplate)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum SetRemoteOperation(PMIRemoteOperationEnum remotevalue)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MoveToPad(ICamera CurCam, MachineIndex Mi, int padIndex)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum MoveToMark(ICamera CurCam, MachineIndex Mi, int padIndex, int markIndex)
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum UpdateDisplayedDevices(ICamera Curcam, bool InitPads = true)
        {
            throw new NotImplementedException();
        }

        public bool IsServiceAvailable()
        {
            return true;
        }

        public IFocusing GetFocuisngModule()
        {
            throw new NotImplementedException();
        }

        public EventCodeEnum EnterMovePadPosition(ref PMIPadObject MovedPadInfo)
        {
            throw new NotImplementedException();
        }

        public List<MachineIndex> GetRemainingPMIDies()
        {
            throw new NotImplementedException();
        }

        public bool GetPMIResult()
        {
            throw new NotImplementedException();
        }

        public void InjectTemplate(ITemplateParam param)
        {
            throw new NotImplementedException();
        }

        public PMITriggerComponent GetTriggerComponent()
        {
            throw new NotImplementedException();
        }

        public IParam GetPMISysIParam()
        {
            throw new NotImplementedException();
        }
        #endregion

        public TemplateStateCollection Template { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITemplateFileParam TemplateParameter => throw new NotImplementedException();

        public ITemplateParam LoadTemplateParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISubRoutine SubRoutine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IParam PMIModuleDevParam_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IParam PMIModuleSysParam_IParam { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IPMIModuleLogger PMILogger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DoPMIData DoPMIInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
