using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PinGroupSettingModule
{
    using DutViewer.ViewModel;
    using LogModule;
    using MetroDialogInterfaces;
    using Newtonsoft.Json;
    using PnPControl;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.AlignEX;
    using ProberInterfaces.Enum;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using RelayCommandBase;
    using System.Xml.Serialization;

    public class PinGroupSettingModule : PNPSetupBase, ISetup, ITemplateModule, IHasDevParameterizable
    {
        public override bool Initialized { get; set; } = false;

        public override Guid ScreenGUID { get; } = new Guid("6B6A825B-A349-464F-37A5-B89B33660F23");

        private List<IGroupData> _PinGroupListBackup = new List<IGroupData>();
        public List<IGroupData> PinGroupListBackup
        {
            get { return _PinGroupListBackup; }
            set { _PinGroupListBackup = value; }
        }
      
        public new string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public new Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public new List<object> Nodes { get; set; }
        public PinGroupDutViewerViewModel DutViewer { get; set; }
        public new UserControlFucEnum UseUserControl { get; set; }
        public AlginParamBase Param { get; set; }
        public SubModuleMovingStateBase MovingState { get; set; }

        public new EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //_CoordinateManager = container.Resolve<ICoordinateManager>();
                    //_PinAligner = container.Resolve<IPinAligner>();

                    retval = InitBackupData();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"InitBackupData() Failed");
                    }

                    CurrMaskingLevel = this.ProberStation().MaskingLevel;

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitModule() : Error occured.");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DoExecute()
        {
            return EventCodeEnum.NONE;
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum deserialRes = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return deserialRes;
        }
        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }
        private EventCodeEnum InitBackupData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {                
                if (PinGroupListBackup != null)
                {
                    PinGroupListBackup.Clear();                   
                }
                else
                {
                    PinGroupListBackup = new List<IGroupData>();
                }

                foreach (GroupData group in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList)
                {
                    PinGroupListBackup.Add(new GroupData(group));                   
                }

                if (this.StageSupervisor().ProbeCardInfo.DisplayPinList == null)
                    this.StageSupervisor().ProbeCardInfo.DisplayPinList = new List<IPinData>();
                else
                {
                    this.StageSupervisor().ProbeCardInfo.DisplayPinList.Clear();
                }

                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                //foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.TestDutList)
                {
                    foreach(PinData pin in dut.PinList)
                    {
                        //pin.DutInfo = new Dut(dut);
                        pin.DutNumber.Value = dut.DutNumber;
                        pin.DutMacIndex.Value = dut.MacIndex;
                        pin.DutLLconerPos.Value = dut.RefCorner;
                        this.StageSupervisor().ProbeCardInfo.DisplayPinList.Add(new PinData(pin));
                    }
                    
                }

                if(this.StageSupervisor().ProbeCardInfo.CandidateDutList == null)
                {
                    this.StageSupervisor().ProbeCardInfo.CandidateDutList = new ObservableCollection<IDut>();
                }
                else if (this.StageSupervisor().ProbeCardInfo.CandidateDutList.Count > 0)
                {
                    this.StageSupervisor().ProbeCardInfo.CandidateDutList.Clear();                    
                }

                if (this.StageSupervisor().ProbeCardInfo.CandidateDutList.Count == 0 || this.StageSupervisor().ProbeCardInfo.CandidateDutList == null)
                {
                    this.StageSupervisor().ProbeCardInfo.CandidateDutList = new ObservableCollection<IDut>();
                    foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        this.StageSupervisor().ProbeCardInfo.CandidateDutList.Add(new Dut(dut));
                    }
                }

                for (int i = 0; i < this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count; i++)
                {
                    //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DisplayDutArr[i] = new Dut(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.CandidateDutList[i]);
                }

                //BackUpDutList = new ObservableCollection<Dut>(this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "SaveProbeCardData() : Error occured.");
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                Header = "Pin Group Setting";
                retVal = InitPnpModuleStage();
                retVal = InitLightJog(this);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = await InitSetup();
                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                //LoggerManager.Debug(err);
                throw err;
            }
            return retVal;
        }

        public EventCodeEnum InitSetupProcType()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitSetupProcType() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.StageSupervisor().ProbeCardInfo.GetPinDataFromPads();

                retVal = InitPNPSetupUI();

                this.StageSupervisor().ProbeCardInfo.LineOffsetX = 0;
                this.StageSupervisor().ProbeCardInfo.LineOffsetY = 0;      

                DutViewer = new PinGroupDutViewerViewModel(new Size(892, 911));

                MainViewTarget = DutViewer;

                MiniViewTarget = null;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitSetup() : Error occured.");
                LoggerManager.Exception(err);
            }

            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPNPSetupUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Buttons = new ObservableCollection<PNPCommandButtonDescriptor>();


                ProcessingType = EnumSetupProgressState.IDLE;


                //PadJogLeftUp.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //        new Uri("pack://application:,,,/ImageResourcePack;component/Images/Left.png");
                //PadJogRightUp.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //        new Uri("pack://application:,,,/ImageResourcePack;component/Images/Right.png");

                PadJogLeftUp.Caption = "↙";
                PadJogRightUp.Caption = "↘";
                PadJogLeftDown.Caption = "Set";
                PadJogRightDown.Caption = "Save";

                PadJogLeftDown.IsEnabled = true;
                PadJogRightDown.IsEnabled = true;
                PadJogLeftUp.IsEnabled = true;
                PadJogRightUp.IsEnabled = true;

                PadJogLeftUp.Command = new RelayCommand(RotateLeft);
                PadJogRightUp.Command = new RelayCommand(RotateRight);
                PadJogLeftDown.Command = new AsyncCommand(Set, false);
                PadJogRightDown.Command = new AsyncCommand(Save, false);

                PadJogUp.Caption = "";
                //PadJogUp.Command = new RelayCommand(null);

                PadJogDown.Caption = "";
                //PadJogDown.Command = new RelayCommand(null);

                PadJogLeft.Caption = "+";
                PadJogLeft.Command = new RelayCommand(ZoomIn);

                PadJogRight.Caption = "-";
                PadJogRight.Command = new RelayCommand(ZoomOut);

                PadJogSelect.Caption = "";

                PadJogUp.IsEnabled = false;
                PadJogDown.IsEnabled = false;
                PadJogLeft.IsEnabled = true;
                PadJogRight.IsEnabled = true;
                PadJogSelect.IsEnabled = false;

                MainViewZoomVisibility = Visibility.Hidden;
                MiniViewZoomVisibility = Visibility.Hidden;

                //OneButton.Caption = "Reset";
                //OneButton.MaskingLevel = 3;
                //OneButton.Command = Button1Command;

                //FourButton.Caption = "OK";
                //FourButton.MaskingLevel = 3;
                //FourButton.Command = Button4Command;

                OneButton.Visibility = System.Windows.Visibility.Collapsed;
                TwoButton.Visibility = System.Windows.Visibility.Collapsed;
                ThreeButton.Visibility = System.Windows.Visibility.Collapsed;
                FourButton.Visibility = System.Windows.Visibility.Collapsed;
                FiveButton.Visibility = System.Windows.Visibility.Collapsed;

                UseUserControl = UserControlFucEnum.DEFAULT;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "InitPNPSetupUI() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private void RotateLeft()
        {
            try
            {
            DutViewer.LeftRotate();
            this.StageSupervisor().ProbeCardInfo.LineOffsetX -= 5;
            if(this.StageSupervisor().ProbeCardInfo.LineOffsetX  < 0)
            {
                this.StageSupervisor().ProbeCardInfo.LineOffsetX = 355;
            }
            
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private void RotateRight()
        {
            try
            {
            DutViewer.RightRotate();
            this.StageSupervisor().ProbeCardInfo.LineOffsetX += 5;
            if (this.StageSupervisor().ProbeCardInfo.LineOffsetX > 360)
            {
                this.StageSupervisor().ProbeCardInfo.LineOffsetX = 5;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        private async Task Set()
        {
            try
            {
                //EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog("Pin group setting", "Do you want to set pin groups?", "Ok", "Cancel", EnumMessageStyle.AffirmativeAndNegative);
                EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog("Pin group setting", "Do you want to set pin groups?", EnumMessageStyle.AffirmativeAndNegative);

                if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    PinGroupListBackup = new List<IGroupData>();

                    foreach (GroupData group in this.StageSupervisor().ProbeCardInfo.CandidatePinGroupList)
                    {
                        PinGroupListBackup.Add(new GroupData(group));
                    }
                }
            }
            catch(Exception err)
            {
                //LoggerManager.Error($err + "Set() : Error occured.");
                LoggerManager.Exception(err);
            }
        }
        private async Task AutoGroup()
        {
            try
            {
                double PinCenterX = 0;
                double PinCenterY = 0;
                double SumX = 0;
                double SumY = 0;
                int PinCnt = 0;
                
                EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog("Pin group setting", "Do you want to set up pin groups automatically?", EnumMessageStyle.AffirmativeAndNegative);
                if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        foreach (PinData pin in dut.PinList)
                        {
                            SumX += pin.AbsPos.GetX();
                            SumY += pin.AbsPos.GetY();
                            PinCnt++;
                        }
                    }
                    PinCenterX = SumX / PinCnt;
                    PinCenterY = SumY / PinCnt;

                    PinGroupListBackup = new List<IGroupData>();

                    PinGroupListBackup.Add(new GroupData());
                    PinGroupListBackup.Add(new GroupData());
                    PinGroupListBackup.Add(new GroupData());
                    PinGroupListBackup.Add(new GroupData());


                    foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                    {
                        foreach (PinData pin in dut.PinList)
                        {
                            if (pin.AbsPos.GetX() <= PinCenterX && pin.AbsPos.GetY() >= PinCenterY)
                            {
                                if (PinGroupListBackup[0].PinNumList == null)
                                    PinGroupListBackup[0].PinNumList = new List<int>();
                                PinGroupListBackup[0].PinNumList.Add(pin.PinNum.Value);
                            }
                            else if (pin.AbsPos.GetX() > PinCenterX && pin.AbsPos.GetY() > PinCenterY)
                            {
                                if (PinGroupListBackup[1].PinNumList == null)
                                    PinGroupListBackup[1].PinNumList = new List<int>();
                                PinGroupListBackup[1].PinNumList.Add(pin.PinNum.Value);
                            }
                            else if (pin.AbsPos.GetX() > PinCenterX && pin.AbsPos.GetY() < PinCenterY)
                            {
                                if (PinGroupListBackup[2].PinNumList == null)
                                    PinGroupListBackup[2].PinNumList = new List<int>();
                                PinGroupListBackup[2].PinNumList.Add(pin.PinNum.Value);
                            }
                            else if (pin.AbsPos.GetX() < PinCenterX && pin.AbsPos.GetY() < PinCenterY)
                            {
                                if (PinGroupListBackup[3].PinNumList == null)
                                    PinGroupListBackup[3].PinNumList = new List<int>();
                                PinGroupListBackup[3].PinNumList.Add(pin.PinNum.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "AddPin() : Error occured.");
                LoggerManager.Exception(err);
            }
        }
        private async new Task Save()
        {
            try
            {
                EnumMessageDialogResult answer = await this.MetroDialogManager().ShowMessageDialog("Pin group setting", "Do you want to save pin sroup data?", EnumMessageStyle.AffirmativeAndNegative);

                if (answer == EnumMessageDialogResult.AFFIRMATIVE)
                {
                    this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList = new List<IGroupData>();
                    //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.TestPinGroupList = new List<GroupData>();

                    foreach (GroupData group in PinGroupListBackup)
                    {
                        this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList.Add(new GroupData(group));
                        //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.TestPinGroupList.Add(new GroupData(group));
                    }

                    //this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList = new ObservableCollection<Dut>(BackUpDutList);
                    this.StageSupervisor().SaveProberCard();
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "Save() : Error occured.");
                LoggerManager.Exception(err);

            }
        }

        void ZoomIn()
        {
            DutViewer.ZoomIn();
        }
        void ZoomOut()
        {
            DutViewer.ZoomOut();
        }

        #region pnp button 1
        private AsyncCommand _Button1Command;
        public ICommand Button1Command
        {
            get
            {
                if (null == _Button1Command) _Button1Command = new AsyncCommand(
                    Button1
                    //, EvaluationPrivilege.Evaluate(
                    //        CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                    //         new Action(() => { ShowMessages("UIModeChange"); })
                             );
                return _Button1Command;
            }
        }
        private Task Button1()
        {
            return Task.CompletedTask;
        }
        #endregion

        #region pnp button 2
        private RelayCommand _Button2Command;
        public ICommand Button2Command
        {
            get
            {
                if (null == _Button2Command) _Button2Command = new RelayCommand(
                    Button2//, EvaluationPrivilege.Evaluate(
                           // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                           // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button2Command;
            }
        }

        private void Button2()
        {

        }
        #endregion

        #region pnp button 3
        private RelayCommand _Button3Command;
        public ICommand Button3Command
        {
            get
            {
                if (null == _Button3Command) _Button3Command = new RelayCommand(
                    Button3//, EvaluationPrivilege.Evaluate(
                           // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                           // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button3Command;
            }
        }

        private void Button3()
        {

        }
        #endregion

        #region pnp button 4
        private RelayCommand _Button4Command;
        public ICommand Button4Command
        {
            get
            {
                if (null == _Button4Command) _Button4Command = new RelayCommand(
                    Button4//, EvaluationPrivilege.Evaluate(
                           // CurrMaskingLevel, Properties.Resources.UIModeChangePriviliage),
                           // new Action(() => { ShowMessages("UIModeChange"); })
                            );
                return _Button4Command;
            }
        }

        private void Button4()
        {

        }
        #endregion             


        private SubModuleStateBase _AlignModuleState;
        public SubModuleStateBase SubModuleState
        {
            get { return _AlignModuleState; }
            set
            {
                if (value != _AlignModuleState)
                {
                    _AlignModuleState = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum LoadDevParameter()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum Modify()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum SaveDevParameter()
        {
            return EventCodeEnum.UNDEFINED;
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }


        public EventCodeEnum ClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                int pinNum = 0;
                int DutPinPairNum = 0;
                foreach (Dut dut in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList)
                {
                    pinNum += dut.PinList.Count;
                }

                foreach (GroupData Group in this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.PinGroupList)
                {
                    DutPinPairNum += Group.PinNumList.Count;
                }

                if (pinNum == DutPinPairNum)
                {

              //      retVal = SubModuleState.SetIdleState();
                    //this.AlignModuleState = new PinGroupSettingIdleState(this);                    
                }
                else
                {
              //      retVal = SubModuleState.SetErrorState();
                    //this.AlignModuleState = new PinGroupSettingNoDataState(this);
                    retVal = EventCodeEnum.NODATA;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR;
                //this._ModuleState = new PinGroupSettingInvaliedDataState(this);
                //     retVal = SubModuleState.SetErrorState();
                //LoggerManager.Error($err + "PreRun() : Error occured.");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void DeInitModule()
        {
            
        }

        public override EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override void SetStepSetupState(string header = null)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }

     
        public EventCodeEnum Recovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public override Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }
        //public override async Task<EventCodeEnum> Cleanup(object parameter = null)
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        retVal = ParamValidation();

        //        if (retVal != EventCodeEnum.NONE)
        //        {
        //            retVal = await base.Cleanup(null);
        //        }
        //        else
        //        {

        //            SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
        //            //this.PinAligner().StopDrawPinOverlay(CurCam);
        //            retVal = await base.Cleanup(parameter);

        //        }

        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Debug(err);
        //        throw;
        //    }
        //    return retVal;
        //}
    }

}
