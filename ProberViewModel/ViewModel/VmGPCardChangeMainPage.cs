using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPCardChangeMainPageViewModel
{
    using CylType;
    using LoaderController.GPController;
    using LogModule;
    using MetroDialogInterfaces;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.CardChange;
    using ProberInterfaces.Param;
    using ProberInterfaces.ViewModel;
    using RelayCommandBase;
    using SequenceRunner;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Windows.Input;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class VmGPCardChangeMainPage : ICCObservationVM, IMainScreenViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ETC...
        public Guid ScreenGUID { get { return _ViewModelGUID; } }
        readonly Guid _ViewModelGUID = new Guid("8c3a01c7-adbd-406f-b9b7-f7f3d5f3068b");
        public bool Initialized { get; set; } = false;
        //public bool CardRegMode { get; set; }
        public EnumCCAlignModule AlignMode { get; set; }

        public IStageSupervisor StageSupervisor => this.StageSupervisor();
        private ICardChangeSysParam CardChangeSysParam => this.CardChangeModule()?.CcSysParams_IParam as ICardChangeSysParam;
        private ICardChangeDevParam CardChangeDevParam => this.CardChangeModule()?.CcDevParams_IParam as ICardChangeDevParam;
        #endregion

        //==> Binding Property

        #region ==> AssignedCamera
        private ICamera _AssignedCamera;
        public ICamera AssignedCamera
        {
            get { return _AssignedCamera; }
            set
            {
                if (value != _AssignedCamera)
                {
                    _AssignedCamera = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _PodSetupBtnEnable;
        public bool PodSetupBtnEnable
        {
            get { return _PodSetupBtnEnable; }
            set
            {
                if (value != _PodSetupBtnEnable)
                {
                    _PodSetupBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region ==> CARDSetupBtnEnable
        private bool _CARDSetupBtnEnable;
        public bool CARDSetupBtnEnable
        {
            get { return _CARDSetupBtnEnable; }
            set
            {
                if (value != _CARDSetupBtnEnable)
                {
                    _CARDSetupBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> POGOSetupBtnEnable
        private bool _POGOSetupBtnEnable;
        public bool POGOSetupBtnEnable
        {
            get { return _POGOSetupBtnEnable; }
            set
            {
                if (value != _POGOSetupBtnEnable)
                {
                    _POGOSetupBtnEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedLightType
        private EnumLightType _SelectedLightType;
        public EnumLightType SelectedLightType
        {
            get { return _SelectedLightType; }
            set
            {
                if (value != _SelectedLightType)
                {
                    _SelectedLightType = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LightValue
        private ushort _LightValue;
        public ushort LightValue
        {
            get { return _LightValue; }
            set
            {
                if (value != _LightValue)
                {
                    _LightValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MarkPositionList
        private ObservableCollection<MarkPosition> _MarkPositionList;
        public ObservableCollection<MarkPosition> MarkPositionList
        {
            get { return _MarkPositionList; }
            set
            {
                if (value != _MarkPositionList)
                {
                    _MarkPositionList = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> SelectedMarkPosition
        private IGPCCObservationMarkPosition _SelectedMarkPosition;
        public IGPCCObservationMarkPosition SelectedMarkPosition
        {
            get { return _SelectedMarkPosition; }
            set
            {
                if (value != _SelectedMarkPosition)
                {
                    _SelectedMarkPosition = value;
                    RaisePropertyChanged();

                    if (_SelectedMarkPosition == null)
                    {
                        return;
                    }

                    foreach (MarkPosition item in MarkPositionList)
                    {
                        if (item == _SelectedMarkPosition)
                        {
                            continue;
                        }
                        item.ButtonEnable = false;
                    }
                    _SelectedMarkPosition.ButtonEnable = true;
                    _SelectedMarkPosition.MoveToMark();
                }
            }
        }
        #endregion

        #region ==> PatternWidth
        private double _PatternWidth;
        public double PatternWidth
        {
            get { return _PatternWidth; }
            set
            {
                if (value != _PatternWidth)
                {
                    _PatternWidth = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> PatternHeight
        private double _PatternHeight;
        public double PatternHeight
        {
            get { return _PatternHeight; }
            set
            {
                if (value != _PatternHeight)
                {
                    _PatternHeight = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZTickValue
        private int _ZTickValue;
        public int ZTickValue
        {
            get { return _ZTickValue; }
            set
            {
                if (value != _ZTickValue)
                {
                    _ZTickValue = value;

                    ZDistanceValue = Math.Pow(10, _ZTickValue);
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ZDistanceValue
        private double _ZDistanceValue;
        public double ZDistanceValue
        {
            get { return _ZDistanceValue; }
            set
            {
                if (value != _ZDistanceValue)
                {
                    _ZDistanceValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> LightTickValue
        private int _LightTickValue;
        public int LightTickValue
        {
            get { return _LightTickValue; }
            set
            {
                if (value != _LightTickValue)
                {
                    _LightTickValue = value;

                    if (_LightTickValue == 0)
                    {
                        SelectedLightType = EnumLightType.COAXIAL;
                    }
                    else
                    {
                        SelectedLightType = EnumLightType.OBLIQUE;
                    }

                    if (this.VisionManager() != null)
                    {
                        ICamera cam = this.VisionManager().GetCam(SelectedProberCam);
                        //  LightValue = (ushort)cam.GetLight(SelectedLightType);
                    }


                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        //==> Command

        #region ==> CardSettingCommand
        private AsyncCommand _CardSettingCommand;
        public IAsyncCommand CardSettingCommand
        {
            get
            {
                if (null == _CardSettingCommand) _CardSettingCommand = new AsyncCommand(CardSettingCommandFunc);
                return _CardSettingCommand;
            }
        }
        public async Task CardSettingCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    AlignMode = EnumCCAlignModule.CARD;         // 포지션 업데이트 전에 설정 필.
                    if (CardChangeSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                    {
                        string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                        ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                        if (String.IsNullOrEmpty(probeCardID))
                        {
                            probeCardID = "DefaultCard";
                        }

                        ProberCardListParameter proberCard = cardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);

                        StageCylinderType.MoveWaferCam.Retract();
                        List<CatCoordinates> markPosList = new List<CatCoordinates>();

                        markPosList.Clear();

                        foreach (var fid in proberCard.FiducialMarInfos)
                        {
                            markPosList.Add(fid.FiducialMarkPos);
                        }

                        SetAlignSettingMode(
                            EnumProberCam.PIN_LOW_CAM,
                            markPosList,
                            CardChangeDevParam.GP_CardPatternWidth,
                            CardChangeDevParam.GP_CardPatternHeight,
                            CardChangeSysParam.GP_PL_COAXIAL,
                            CardChangeSysParam.GP_PL_OBLIQUE);
                        this.StageSupervisor().StageModuleState.CCZCLEARED();
                    }
                    else
                    {
                        SetAlignSettingMode(
                            EnumProberCam.WAFER_LOW_CAM,
                            new List<CatCoordinates>(CardChangeSysParam.GP_SearchedCardMarkPosList),
                            CardChangeDevParam.GP_CardPatternWidth,
                            CardChangeDevParam.GP_CardPatternHeight,
                            CardChangeSysParam.GP_WL_COAXIAL,
                            CardChangeSysParam.GP_WL_OBLIQUE);
                        this.StageSupervisor().StageModuleState.CCZCLEARED();
                        StageCylinderType.MoveWaferCam.Extend();
                    }
                   
                    CARDSetupBtnEnable = false;
                    POGOSetupBtnEnable = true;
                    PodSetupBtnEnable = true;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PogoSettingCommand
        private AsyncCommand _PogoSettingCommand;
        public IAsyncCommand PogoSettingCommand
        {
            get
            {
                if (null == _PogoSettingCommand) _PogoSettingCommand = new AsyncCommand(PogoSettingCommandFunc);
                return _PogoSettingCommand;
            }
        }
        public async Task PogoSettingCommandFunc()
        {
            try
            {
                if (this.CardChangeModule().GetCardDockingStatus() == EnumCardDockingStatus.DOCKED)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Pogo Setting", "Card and pogo docked. can not set to pogo position", EnumMessageStyle.Affirmative);
                }
                else
                {
                    Task task = new Task(() =>
                    {
                        AlignMode = EnumCCAlignModule.POGO;     // 포지션 업데이트 전에 설정 필.

                        if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                        {
                            SetAlignSettingMode(
                               EnumProberCam.PIN_LOW_CAM,
                               new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList),
                               CardChangeDevParam.GP_PogoPatternWidth,
                               CardChangeDevParam.GP_PogoPatternHeight,
                               CardChangeSysParam.GP_PL_COAXIAL,
                               CardChangeSysParam.GP_PL_OBLIQUE);
                        }
                        else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                        {
                            SetAlignSettingMode(
                               EnumProberCam.PIN_LOW_CAM,
                               new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList3P),
                               CardChangeDevParam.GP_PogoPatternWidth,
                               CardChangeDevParam.GP_PogoPatternHeight,
                               CardChangeSysParam.GP_PL_COAXIAL,
                               CardChangeSysParam.GP_PL_OBLIQUE);
                        }
                        else
                        {
                            LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                        }

                        this.StageSupervisor().StageModuleState.CCZCLEARED();

                        StageCylinderType.MoveWaferCam.Retract();
                        CARDSetupBtnEnable = true;
                        POGOSetupBtnEnable = false;
                        PodSetupBtnEnable = true;
                    });
                    task.Start();
                    await task;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PodSettingCommand
        private AsyncCommand _PodSettingCommand;
        public IAsyncCommand PodSettingCommand
        {
            get
            {
                if (null == _PodSettingCommand) _PodSettingCommand = new AsyncCommand(PodSettingCommandFunc);
                return _PodSettingCommand;
            }
        }
        public async Task PodSettingCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    AlignMode = EnumCCAlignModule.POD;     // 포지션 업데이트 전에 설정 필.
                    if(CardChangeSysParam.CardPodMarkPosList.Count != 3)
                    {

                        CardChangeSysParam.CardPodMarkPosList.Add(new WaferCoordinate(-165000, 0, 7000));
                        CardChangeSysParam.CardPodMarkPosList.Add(new WaferCoordinate(
                            Math.Cos(60 / Math.PI * 180) * 16500, Math.Sin(60 / Math.PI * 180) * 16500, 7000));
                        CardChangeSysParam.CardPodMarkPosList.Add(new WaferCoordinate(
                            Math.Cos(60 / Math.PI * 180) * 16500, Math.Sin(60 / Math.PI * 180) * -16500, 7000));
                    }
                    SetAlignSettingMode(
                               EnumProberCam.WAFER_LOW_CAM,
                               new List<CatCoordinates>(CardChangeSysParam.CardPodMarkPosList),
                            CardChangeDevParam.CardPodPatternWidth,
                            CardChangeDevParam.CardPodPatternHeight,
                            CardChangeSysParam.GP_WL_COAXIAL,
                            CardChangeSysParam.GP_WL_OBLIQUE);

                    this.StageSupervisor().StageModuleState.CCZCLEARED();
                    
                    StageCylinderType.MoveWaferCam.Retract();
                    // Down pod
                    var beh = new GP_DropPCardPod();
                    var ret = this.CardChangeModule().BehaviorRun(beh);



                    ////StageCylinderType.MoveWaferCam.Extend();
                    //var moveResult = this.StageSupervisor().StageModuleState.CardViewMove(0, 0);
                    //if (moveResult != EventCodeEnum.NONE)
                    //{
                    //    LoggerManager.Debug($"GPCardAlign CamAbsMove func. CardViewMove(X,Y) fail return code:{moveResult}");
                    //}
                    //else
                    //{
                    //    // Raise pod
                    //    var raiseBeh = new GP_RaisePCardPod();
                    //    ret = this.CardChangeModule().BehaviorRun(raiseBeh);

                    //}

                    CARDSetupBtnEnable = true;
                    POGOSetupBtnEnable = true;
                    PodSetupBtnEnable = false;
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        #region ==> FocusingCommand
        private AsyncCommand _FocusingCommand;
        public IAsyncCommand FocusingCommand
        {
            get
            {
                if (null == _FocusingCommand) _FocusingCommand = new AsyncCommand(FocusingCommandFunc, new Func<bool>(() => !FocusingCommandCanExecute));
                return _FocusingCommand;
            }
        }

        public async Task<EventCodeEnum> FocusingCommandFunc()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            //  this.MarkAligner().MarkFocusModel.ShowFocusGraph();
            try
            {

                if (!CARDSetupBtnEnable)
                {
                    ret = await Task.Run(() => CardFocusingFunc());
                    if (ret == EventCodeEnum.NONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Card Focusing", "Card focusing is done successfully.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Card Focusing", "Card focusing failed.", EnumMessageStyle.Affirmative);
                    }
                }
                else if (!POGOSetupBtnEnable)
                {
                    ret = await Task.Run(() => PogoFocusingFunc());
                    if (ret == EventCodeEnum.NONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("POGO Focusing", "POGO focusing is done successfully.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("POGO Focusing", "POGO focusing failed.", EnumMessageStyle.Affirmative);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        private bool _FocusingCommandCanExecute;
        public bool FocusingCommandCanExecute
        {
            get { return _FocusingCommandCanExecute; }
            set
            {
                if (value != _FocusingCommandCanExecute)
                {
                    _FocusingCommandCanExecute = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventCodeEnum CardFocusingFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                FocusingCommandCanExecute = false;

                RetVal = this.MarkAligner().MarkFocusModel.Focusing_Retry(CardChangeDevParam.CardFocusParam, false, false, false, this);

                if (RetVal != EventCodeEnum.NONE)
                {
                    RetVal = EventCodeEnum.FOCUS_FAILED;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.FOCUS_FAILED;
            }
            finally
            {
                FocusingCommandCanExecute = true;

            }
            return RetVal;
        }


        public EventCodeEnum PogoFocusingFunc()
        {
            EventCodeEnum RetVal = EventCodeEnum.NONE;

            try
            {
                FocusingCommandCanExecute = false;
                //RetVal = this.MarkAligner().MarkFocusModel.Focusing_Retry((this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).PogoFocusParam, false, false, false);
                RetVal = this.MarkAligner().MarkFocusModel.Focusing_Retry(CardChangeDevParam.PogoFocusParam, false, false, false, this);

                if (RetVal != EventCodeEnum.NONE)
                {
                    RetVal = EventCodeEnum.FOCUS_FAILED;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.FOCUS_FAILED;
            }
            finally
            {
                FocusingCommandCanExecute = true;

            }
            return RetVal;
        }

        #endregion


        #region ==> AlignCommand
        private AsyncCommand _AlignCommand;
        public IAsyncCommand AlignCommand
        {
            get
            {
                if (null == _AlignCommand) _AlignCommand = new AsyncCommand(AlignCommandFunc);
                return _AlignCommand;
            }
        }
        public async Task AlignCommandFunc()
        {
            try
            {

                Task task = new Task(() =>
                {
                    if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM)
                    {
                        double cardCenterDiffX;
                        double cardCenterDiffY;
                        double cardDegreeDiff;
                        double cardAvgZ;
                        if (AlignMode == EnumCCAlignModule.CARD)
                        {
                            if (this.GPCardAligner().AlignCard(out cardCenterDiffX, out cardCenterDiffY, out cardDegreeDiff, out cardAvgZ) == false)
                            {
                                LoggerManager.Error($"CardAligner Fail : Card Algin");
                                this.MetroDialogManager().ShowMessageDialog("CardAligner", $"Fail card align", EnumMessageStyle.Affirmative);
                                return;
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Done", $"Succeed", EnumMessageStyle.Affirmative);
                            }
                        }
                        else if (AlignMode == EnumCCAlignModule.POD)
                        {
                            if (this.GPCardAligner().AlignPogo(out cardCenterDiffX, out cardCenterDiffY, out cardDegreeDiff, out cardAvgZ) == false)
                            {
                                LoggerManager.Error($"CardAligner Fail : Card Algin");
                                this.MetroDialogManager().ShowMessageDialog("CardAligner", $"Fail card align", EnumMessageStyle.Affirmative);
                                return;
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Done", $"Succeed", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                    else if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM)
                    {
                        if(AlignMode == EnumCCAlignModule.CARD)
                        {
                            string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                            ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                            if (String.IsNullOrEmpty(probeCardID))
                            {
                                probeCardID = "DefaultCard";
                            }

                            ProberCardListParameter probeCard = cardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);
                            if(probeCard == null)
                            {
                                probeCard = (this.LoaderController() as GP_LoaderController).DownloadProbeCardInfo(probeCardID);
                                if(probeCard == null)
                                {
                                    LoggerManager.Debug($"AlignCommandFunc() Probe card info({probeCardID}) is not exist in cell and loader");
                                    this.MetroDialogManager().ShowMessageDialog("CardAligner", $"Probe card info({probeCardID}) is not exist in cell and loader", EnumMessageStyle.Affirmative);
                                    return;
                                }
                            }

                            double cardCenterDiffX;
                            double cardCenterDiffY;
                            double cardDegreeDiff;
                            double cardAvgZ;
                            if (this.GPCardAligner().AlignCard(out cardCenterDiffX, out cardCenterDiffY, out cardDegreeDiff, out cardAvgZ, probeCard) == false)
                            {
                                LoggerManager.Error($"CardAligner Fail : Card Algin");
                                this.MetroDialogManager().ShowMessageDialog("CardAligner", $"Fail card align", EnumMessageStyle.Affirmative);
                                return;
                            }
                            else
                            {
                                var beh = new GP_MoveChuckToLoader();
                                var ret = this.CardChangeModule().BehaviorRun(beh).Result;

                                if(ret == EventCodeEnum.NONE)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Done", $"Succeed", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    CardChangeSysParam.CardAlignState = AlignStateEnum.FAIL;
                                    LoggerManager.Debug("Align Position Move Fail");
                                }
                            }
                        }
                        else
                        {
                            double pogoCenterDiffX = 0;
                            double pogoCenterDiffY = 0;
                            double pogoDegreeDiff = 0;
                            double pogoAvgZ = 0;
                            bool pogoAlignResult = false;
                            if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                            {
                                pogoAlignResult = this.GPCardAligner().AlignPogo(out pogoCenterDiffX, out pogoCenterDiffY, out pogoDegreeDiff, out pogoAvgZ);
                            }
                            else if (CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                            {
                                pogoAlignResult = this.GPCardAligner().AlignPogo3P(out pogoCenterDiffX, out pogoCenterDiffY, out pogoDegreeDiff, out pogoAvgZ);
                            }
                            else
                            {
                                LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                                pogoAlignResult = false;
                            }

                            if (pogoAlignResult == false)
                            {
                                LoggerManager.Error($"CardAligner Fail : Pogo Algin");
                                this.MetroDialogManager().ShowMessageDialog("CardAligner", $"Fail pogo align", EnumMessageStyle.Affirmative);

                                return;
                            }
                            else
                            {
                                this.MetroDialogManager().ShowMessageDialog("Done", $"Succeed", EnumMessageStyle.Affirmative);
                            }
                        }

                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion


        #region ==> PatternWidthPlusCommand
        private RelayCommand _PatternWidthPlusCommand;
        public ICommand PatternWidthPlusCommand
        {
            get
            {
                if (null == _PatternWidthPlusCommand) _PatternWidthPlusCommand = new RelayCommand(PatternWidthPlusCommandFunc);
                return _PatternWidthPlusCommand;
            }
        }
        public void PatternWidthPlusCommandFunc()
        {
            try
            {
                if (PatternWidth + 10 > 890)
                {
                    return;
                }

                PatternWidth += 10;

                SavePatternWidthHeightParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PatternWidthMinusCommand
        private RelayCommand _PatternWidthMinusCommand;
        public ICommand PatternWidthMinusCommand
        {
            get
            {
                if (null == _PatternWidthMinusCommand) _PatternWidthMinusCommand = new RelayCommand(PatternWidthMinusCommandFunc);
                return _PatternWidthMinusCommand;
            }
        }
        public void PatternWidthMinusCommandFunc()
        {
            try
            {
                if (PatternWidth - 10 < 10)
                {
                    return;
                }

                PatternWidth -= 10;

                SavePatternWidthHeightParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PatternHeightPlusCommand
        private RelayCommand _PatternHeightPlusCommand;
        public ICommand PatternHeightPlusCommand
        {
            get
            {
                if (null == _PatternHeightPlusCommand) _PatternHeightPlusCommand = new RelayCommand(PatternHeightPlusCommandFunc);
                return _PatternHeightPlusCommand;
            }
        }
        public void PatternHeightPlusCommandFunc()
        {
            try
            {
                if (PatternHeight + 10 > 890)
                {
                    return;
                }

                PatternHeight += 10;
                SavePatternWidthHeightParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> PatternHeightMinusCommand
        private RelayCommand _PatternHeightMinusCommand;
        public ICommand PatternHeightMinusCommand
        {
            get
            {
                if (null == _PatternHeightMinusCommand) _PatternHeightMinusCommand = new RelayCommand(PatternHeightMinusCommandFunc);
                return _PatternHeightMinusCommand;
            }
        }
        public void PatternHeightMinusCommandFunc()
        {
            try
            {
                if (PatternHeight - 10 < 10)
                {
                    return;
                }

                PatternHeight -= 10;

                SavePatternWidthHeightParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> WaferCamExtendCommand
        private AsyncCommand _WaferCamExtendCommand;
        public IAsyncCommand WaferCamExtendCommand
        {
            get
            {
                if (null == _WaferCamExtendCommand) _WaferCamExtendCommand = new AsyncCommand(WaferCamExtendCommandFunc);
                return _WaferCamExtendCommand;
            }
        }
        public async Task WaferCamExtendCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    double axisZsafeOffset = 15000; //마크를 봤을때 핀하이, 웨이퍼하이의 거리는 35.5mm이다 마크 보는 포지션에서 척은 pz보다 20mm높다. 
                    double axisPZsafeOffset = 35000;
                    var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                    var markRefposZ = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;
                    if (axisz.Status.RawPosition.Actual > markRefposZ + axisZsafeOffset)
                    {
                        this.StageSupervisor().StageModuleState.CCZCLEARED();
                    }
                    if (axispz.Status.RawPosition.Actual > markRefposZ + axisPZsafeOffset)
                    {
                        this.StageSupervisor().StageModuleState.CCZCLEARED();
                    }
                    StageCylinderType.MoveWaferCam.Extend();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> WaferCamFoldCommand
        private AsyncCommand _WaferCamFoldCommand;
        public IAsyncCommand WaferCamFoldCommand
        {
            get
            {
                if (null == _WaferCamFoldCommand) _WaferCamFoldCommand = new AsyncCommand(WaferCamFoldCommandFunc);
                return _WaferCamFoldCommand;
            }
        }
        public async Task WaferCamFoldCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    StageCylinderType.MoveWaferCam.Retract();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> MoveToCenterCommand
        private AsyncCommand _MoveToCenterCommand;
        public IAsyncCommand MoveToCenterCommand
        {
            get
            {
                if (null == _MoveToCenterCommand) _MoveToCenterCommand = new AsyncCommand(MoveToCenterCommandFunc);
                return _MoveToCenterCommand;
            }
        }
        public async Task MoveToCenterCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    EventCodeEnum result = EventCodeEnum.NONE;

                    result = this.StageSupervisor().StageModuleState.ZCLEARED();
                    if (result != EventCodeEnum.NONE)
                    {
                        return;
                    }

                    ICamera cam = this.VisionManager().GetCam(SelectedProberCam);

                    if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM)
                    {
                        cam.SetLight(EnumLightType.COAXIAL, CardChangeSysParam.GP_WL_COAXIAL);
                        cam.SetLight(EnumLightType.OBLIQUE, CardChangeSysParam.GP_WL_OBLIQUE);

                        result = this.StageSupervisor().StageModuleState.CardViewMove(0, 0);
                        if (result != EventCodeEnum.NONE)
                        {
                            return;
                        }

                        double axisZsafeOffset = 15000; //마크를 봤을때 핀하이, 웨이퍼하이의 거리는 35.5mm이다 마크 보는 포지션에서 척은 pz보다 20mm높다. 
                        double axisPZsafeOffset = 35000;
                        var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                        var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                        var markRefposZ = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;
                        if (axisz.Status.RawPosition.Actual > markRefposZ + axisZsafeOffset)
                        {
                            this.StageSupervisor().StageModuleState.CCZCLEARED();
                        }
                        if (axispz.Status.RawPosition.Actual > markRefposZ + axisPZsafeOffset)
                        {
                            this.StageSupervisor().StageModuleState.CCZCLEARED();
                        }

                        StageCylinderType.MoveWaferCam.Extend();
                    }
                    else if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM)
                    {
                        StageCylinderType.MoveWaferCam.Retract();

                        cam.SetLight(EnumLightType.COAXIAL, CardChangeSysParam.GP_PL_COAXIAL);
                        cam.SetLight(EnumLightType.OBLIQUE, CardChangeSysParam.GP_PL_OBLIQUE);

                        PinCoordinate pinPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();

                        result = this.StageSupervisor().StageModuleState.PogoViewMove(0, 0, pinPos.Z.Value);
                        if (result != EventCodeEnum.NONE)
                        {
                            return;
                        }
                    }
                    else
                    {
                        ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                        ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                        this.MotionManager().AbsMove(xAxis, 0, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                        this.MotionManager().AbsMove(yAxis, 0, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> ReadyToGetCardCommand
        private AsyncCommand _ReadyToGetCardCommand;
        public IAsyncCommand ReadyToGetCardCommand
        {
            get
            {
                if (null == _ReadyToGetCardCommand) _ReadyToGetCardCommand = new AsyncCommand(ReadyToGetCardCommandFunc);
                return _ReadyToGetCardCommand;
            }
        }
        public async Task ReadyToGetCardCommandFunc()
        {
            try
            {
                var beh = new GP_ReadyToGetCard();

                var ret = await this.CardChangeModule().BehaviorRun(beh);
                if (ret != EventCodeEnum.NONE)
                {
                    await this.MetroDialogManager().ShowMessageDialog($"Fail {beh}", $"Error code:{ret}", EnumMessageStyle.Affirmative);
                }
                else
                {
                    await this.MetroDialogManager().ShowMessageDialog("Success", "Done", EnumMessageStyle.Affirmative);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> RaiseZCommand
        private AsyncCommand _RaiseZCommand;
        public IAsyncCommand RaiseZCommand
        {
            get
            {
                if (null == _RaiseZCommand) _RaiseZCommand = new AsyncCommand(RaiseZCommandFunc);
                return _RaiseZCommand;
            }
        }
        public async Task RaiseZCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    this.GPCardAligner().RelMoveZ(SelectedProberCam, ZDistanceValue);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"RaiseZCommandFunc(): Dist. = {ZDistanceValue}, Curr. cam = {SelectedProberCam}");
            }
        }
        #endregion

        #region ==> DropZCommand
        private AsyncCommand _DropZCommand;
        public IAsyncCommand DropZCommand
        {
            get
            {
                if (null == _DropZCommand) _DropZCommand = new AsyncCommand(DropZCommandFunc);
                return _DropZCommand;
            }
        }
        public async Task DropZCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    this.GPCardAligner().RelMoveZ(SelectedProberCam, ZDistanceValue * -1);
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {

                LoggerManager.Debug($"DropZCommandFunc(): Dist. = {ZDistanceValue}, Curr. cam = {SelectedProberCam}");
            }
        }
        #endregion

        #region ==> IncreaseLightIntensityCommand
        private RelayCommand _IncreaseLightIntensityCommand;
        public ICommand IncreaseLightIntensityCommand
        {
            get
            {
                if (null == _IncreaseLightIntensityCommand) _IncreaseLightIntensityCommand = new RelayCommand(IncreaseLightIntensityCommandFunc);
                return _IncreaseLightIntensityCommand;
            }
        }
        public void IncreaseLightIntensityCommandFunc()
        {
            try
            {
                ICamera cam = this.VisionManager().GetCam(SelectedProberCam);

                int lightIntensity = cam.GetLight(SelectedLightType);
                if (lightIntensity < 0)
                {
                    return;
                }

                lightIntensity += 1;
                if (lightIntensity > 255)
                {
                    return;
                }

                cam.SetLight(SelectedLightType, (ushort)lightIntensity);
                LightValue = (ushort)cam.GetLight(SelectedLightType);

                SaveLightParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> _SetMFModelLightsCommand
        private AsyncCommand _SetMFModelLightsCommand;
        public IAsyncCommand SetMFModelLightsCommand
        {
            get
            {
                if (null == _SetMFModelLightsCommand) _SetMFModelLightsCommand = new AsyncCommand(SetMFModelLightsCommandFunc);
                return _SetMFModelLightsCommand;
            }
        }
        public async Task SetMFModelLightsCommandFunc()
        {
            Task task = new Task(() =>
            {
                try
                {
                    var modelinfos = CardChangeDevParam.ModelInfos;
                    ICamera cam = this.VisionManager().GetCam(SelectedProberCam);

                    for (int i = 0; i < modelinfos.Count; i++)
                    {
                        var lights = modelinfos[i].Lights;
                        for (int j = 0; j < lights.Count; j++)
                        {
                            int intensity = cam.GetLight(lights[j].Type.Value);
                            CardChangeDevParam.ModelInfos[i].Lights[j].ChannelMapIdx.Value = intensity;
                            //set obj from cur light
                        }

                    }

                    this.CardChangeModule().SaveDevParameter();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            });
            task.Start();
            await task;
        }
        #endregion


        #region ==> _SetMFChildLightsCommand
        private AsyncCommand _SetMFChildLightsCommand;
        public IAsyncCommand SetMFChildLightsCommand
        {
            get
            {
                if (null == _SetMFChildLightsCommand) _SetMFChildLightsCommand = new AsyncCommand(SetMFChildLightsCommandFunc);
                return _SetMFChildLightsCommand;
            }
        }

        public async Task SetMFChildLightsCommandFunc()
        {

            Task task = new Task(() =>
            {
                try
                {
                    var modelinfos = CardChangeDevParam.ModelInfos;
                    ICamera cam = this.VisionManager().GetCam(SelectedProberCam);

                    for (int i = 0; i < modelinfos.Count; i++)
                    {
                        var lights = modelinfos[i].Lights;
                        for (int j = 0; j < lights.Count; j++)
                        {
                            int intensity = cam.GetLight(lights[j].Type.Value);
                            CardChangeDevParam.ModelInfos[i].Child.Lights[j].ChannelMapIdx.Value = intensity;
                            //set obj from cur light
                        }

                    }

                    this.CardChangeModule().SaveDevParameter();
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            });
            task.Start();
            await task;


        }
        #endregion

        #region ==> DecreaseLightIntensityCommand
        private RelayCommand _DecreaseLightIntensityCommand;
        public ICommand DecreaseLightIntensityCommand
        {
            get
            {
                if (null == _DecreaseLightIntensityCommand) _DecreaseLightIntensityCommand = new RelayCommand(DecreaseLightIntensityCommandFunc);
                return _DecreaseLightIntensityCommand;
            }
        }
        public void DecreaseLightIntensityCommandFunc()
        {
            try
            {
                ICamera cam = this.VisionManager().GetCam(SelectedProberCam);

                int lightIntensity = cam.GetLight(SelectedLightType);
                if (lightIntensity < 0)
                {
                    return;
                }

                lightIntensity -= 1;
                if (lightIntensity < 0)
                {
                    return;
                }

                cam.SetLight(SelectedLightType, (ushort)lightIntensity);
                LightValue = (ushort)cam.GetLight(SelectedLightType);

                SaveLightParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> RegisterPatternCommand
        private AsyncCommand _RegisterPatternCommand;
        public IAsyncCommand RegisterPatternCommand
        {
            get
            {
                if (null == _RegisterPatternCommand) _RegisterPatternCommand = new AsyncCommand(RegisterPatternCommandFunc);
                return _RegisterPatternCommand;
            }
        }
        public async Task RegisterPatternCommandFunc()
        {
            try
            {
                await _SelectedMarkPosition.RegisterPatternCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> RegisterPosCommand
        private AsyncCommand _RegisterPosCommand;
        public IAsyncCommand RegisterPosCommand
        {
            get
            {
                if (null == _RegisterPosCommand) _RegisterPosCommand = new AsyncCommand(RegisterPosCommandFunc);
                return _RegisterPosCommand;
            }
        }
        public async Task RegisterPosCommandFunc()
        {
            try
            {
                
                await _SelectedMarkPosition.RegisterPosCommandFunc();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> MoveToMarkCommand

        private AsyncCommand _MoveToMarkCommand;
        public IAsyncCommand MoveToMarkCommand
        {
            get
            {
                if (null == _MoveToMarkCommand) _MoveToMarkCommand = new AsyncCommand(MoveToMarkCommandFunc);
                return _MoveToMarkCommand;
            }
        }
        public async Task MoveToMarkCommandFunc()
        {
            try
            {
                Task task = new Task(() =>
                {
                    _SelectedMarkPosition.MoveToMark();
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        #region ==> Set SelectedmarkPositionCommand
        private AsyncCommand<int> _SetSelectedMarkPosCommand;
        public IAsyncCommand SetSelectedMarkPosCommand
        {
            get
            {
                if (null == _SetSelectedMarkPosCommand) _SetSelectedMarkPosCommand = new AsyncCommand<int>(SetSelectedMarkPosCommandFunc);
                return _SetSelectedMarkPosCommand;
            }
        }
        public async Task SetSelectedMarkPosCommandFunc(int selectedmarkposIdx)
        {
            try
            {
                SelectedMarkPosition = MarkPositionList[selectedmarkposIdx];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> Set SelectLightTypeCommand
        private AsyncCommand<EnumLightType> _SetSelectLightTypeCommand;
        public IAsyncCommand SetSelectLightTypeCommand
        {
            get
            {
                if (null == _SetSelectLightTypeCommand) _SetSelectLightTypeCommand = new AsyncCommand<EnumLightType>(SetSelectLightTypeCommandFunc);
                return _SetSelectLightTypeCommand;
            }
        }
        public async Task SetSelectLightTypeCommandFunc(EnumLightType lighttype)
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (lighttype != null)
                    {
                        SelectedLightType = lighttype;

                        if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM && SelectedLightType == EnumLightType.COAXIAL)
                        {
                            LightValue = CardChangeSysParam.GP_PL_COAXIAL;
                        }
                        else if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM && SelectedLightType == EnumLightType.OBLIQUE)
                        {
                            LightValue = CardChangeSysParam.GP_PL_OBLIQUE;
                        }
                        else if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM && SelectedLightType == EnumLightType.COAXIAL)
                        {
                            LightValue = CardChangeSysParam.GP_WL_COAXIAL;
                        }
                        else if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM && SelectedLightType == EnumLightType.OBLIQUE)
                        {
                            LightValue = CardChangeSysParam.GP_WL_OBLIQUE;
                        }
                        ICamera cam = this.VisionManager().GetCam(SelectedProberCam);
                        cam.SetLight(SelectedLightType, (ushort)LightValue);
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> Set LightValueCommand
        private AsyncCommand<ushort> _SetLightValueCommand;
        public IAsyncCommand SetLightValueCommand
        {
            get
            {
                if (null == _SetLightValueCommand) _SetLightValueCommand = new AsyncCommand<ushort>(SetLightValueCommandFunc);
                return _SetLightValueCommand;
            }
        }
        public async Task SetLightValueCommandFunc(ushort lightvalue)
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (lightvalue != null)
                    {
                        LightValue = lightvalue;
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> SetZTickValueCommand
        private AsyncCommand<int> _SetZTickValueCommand;
        public IAsyncCommand SetZTickValueCommand
        {
            get
            {
                if (null == _SetZTickValueCommand) _SetZTickValueCommand = new AsyncCommand<int>(SetZTickValueCommandFunc);
                return _SetZTickValueCommand;
            }
        }
        public async Task SetZTickValueCommandFunc(int ztickvalue)
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (ztickvalue != null)
                    {
                        ZTickValue = ztickvalue;
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> SetZDistanceValueCommand
        private AsyncCommand<double> _SetZDistanceValueCommand;
        public IAsyncCommand SetZDistanceValueCommand
        {
            get
            {
                if (null == _SetZDistanceValueCommand) _SetZDistanceValueCommand = new AsyncCommand<double>(SetZDistanceValueCommandFunc);
                return _SetZDistanceValueCommand;
            }
        }
        public async Task SetZDistanceValueCommandFunc(double zdistance)
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (zdistance != null)
                    {
                        ZDistanceValue = zdistance;
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> SetLightTickValueCommand
        private AsyncCommand<int> _SetLightTickValueCommand;
        public IAsyncCommand SetLightTickValueCommand
        {
            get
            {
                if (null == _SetLightTickValueCommand) _SetLightTickValueCommand = new AsyncCommand<int>(SetLightTickValueCommandFunc);
                return _SetLightTickValueCommand;
            }
        }
        public async Task SetLightTickValueCommandFunc(int lighttick)
        {
            try
            {
                Task task = new Task(() =>
                {
                    if (lighttick != null)
                    {
                        LightTickValue = lighttick;
                    }
                });
                task.Start();
                await task;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region ==> PageSwitchCommand
        private AsyncCommand<bool> _PageSwitchCommand;
        public IAsyncCommand PageSwitchCommand
        {
            get
            {
                if (null == _PageSwitchCommand) _PageSwitchCommand = new AsyncCommand<bool>(PageSwitchCommandFunc);
                return _PageSwitchCommand;
            }
        }
        public async Task PageSwitchCommandFunc(bool observation)
        {
            try
            {
                await PageSwitched(observation);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
        #region ==> CleanUpCommand
        private AsyncCommand _CleanUpCommand;
        public IAsyncCommand CleanUpCommand
        {
            get
            {
                if (null == _CleanUpCommand) _CleanUpCommand = new AsyncCommand(CleanUpCommandFunc);
                return _CleanUpCommand;
            }
        }
        public async Task CleanUpCommandFunc()
        {
            try
            {
                await Cleanup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion

        //const int SAVE_DELAY = 3;
        //int patterSizeSaveWait = 0;
        private void SavePatternWidthHeightParam()
        {
            try
            {
                if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    CardChangeDevParam.GP_CardPatternWidth = PatternWidth;
                    CardChangeDevParam.GP_CardPatternHeight = PatternHeight;
                }
                else if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    if(AlignMode == EnumCCAlignModule.CARD)
                    {
                        CardChangeDevParam.GP_CardPatternWidth = PatternWidth;
                        CardChangeDevParam.GP_CardPatternHeight = PatternHeight;
                    }
                    else
                    {
                        CardChangeDevParam.GP_PogoPatternWidth = PatternWidth;
                        CardChangeDevParam.GP_PogoPatternHeight = PatternHeight;
                    }
                }

                LoggerManager.Debug("Save Pattern Size");

                EventCodeEnum saveResult = this.CardChangeModule().SaveDevParameter();

                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pattern Parameter Fail");
                }

                //if (patterSizeSaveWait > 0)
                //{
                //    patterSizeSaveWait = SAVE_DELAY;
                //    return;
                //}

                //Task.Run(() =>
                //{
                //    patterSizeSaveWait = SAVE_DELAY;
                //    while (true)
                //    {
                //        System.Threading.Thread.Sleep(1000);

                //        patterSizeSaveWait--;

                //        if (patterSizeSaveWait < 1)
                //        {
                //            break;
                //        }
                //    }

                //    if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM)
                //    {
                //        CardChangeDevParam.GP_CardPatternWidth = PatternWidth;
                //        CardChangeDevParam.GP_CardPatternHeight = PatternHeight;
                //    }
                //    else if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM)
                //    {
                //        CardChangeDevParam.GP_PogoPatternWidth = PatternWidth;
                //        CardChangeDevParam.GP_PogoPatternHeight = PatternHeight;
                //    }

                //    LoggerManager.Debug("Save Pattern Size");

                //    EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();

                //    if (saveResult != EventCodeEnum.NONE)
                //    {
                //        LoggerManager.Error("Save Pattern Parameter Fail");
                //    }
                //});
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //int lightSaveWait = 0;
        private void SaveLightParam()
        {
            try
            {
                if (AlignMode == EnumCCAlignModule.CARD && CardChangeSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    // Fiducial Mark Align 셋팅 중에서는 아래의 파라미터의 값을 사용하지 않기 때문에 값을 변경시키고 저장할 필요가 없습니다.
                    return;
                }

                if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM && SelectedLightType == EnumLightType.COAXIAL)
                {
                    CardChangeSysParam.GP_PL_COAXIAL = LightValue;
                }
                else if (SelectedProberCam == EnumProberCam.PIN_LOW_CAM && SelectedLightType == EnumLightType.OBLIQUE)
                {
                    CardChangeSysParam.GP_PL_OBLIQUE = LightValue;
                }
                else if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM && SelectedLightType == EnumLightType.COAXIAL)
                {
                    CardChangeSysParam.GP_WL_COAXIAL = LightValue;
                }
                else if (SelectedProberCam == EnumProberCam.WAFER_LOW_CAM && SelectedLightType == EnumLightType.OBLIQUE)
                {
                    CardChangeSysParam.GP_WL_OBLIQUE = LightValue;
                }

                LoggerManager.Debug("Save Light");
                EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();
                if (saveResult != EventCodeEnum.NONE)
                {
                    LoggerManager.Error("Save Pattern Parameter Fail");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void PositionSetting(EnumProberCam camType, List<CatCoordinates> markCoordinates)
        {
            try
            {
                MarkPositionList = new ObservableCollection<MarkPosition>();
                for (int i = 0; i < markCoordinates.Count; ++i)
                {
                    CatCoordinates markPosition = markCoordinates[i];
                    MarkPositionList.Add(new MarkPosition(camType, markPosition, i, AlignMode));
                }
                SelectedProberCam = camType;
                AssignedCamera = this.VisionManager().GetCam(SelectedProberCam);
                
                this.VisionManager().StartGrab(SelectedProberCam, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void SetAlignSettingMode(EnumProberCam camType, List<CatCoordinates> markCoordinates, double patternWidth, double patternHeight, ushort coaxialLightValue, ushort obliqueLightValue)
        {
            try
            {
                EventCodeEnum result = EventCodeEnum.UNDEFINED;

                if (this.VisionManager().StopGrab(SelectedProberCam) != EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"{camType} : {SelectedProberCam}, Start grab Error");
                }
                //if(MarkPositionList == null)
                //{
                //    MarkPositionList = new ObservableCollection<MarkPosition>();
                //}
                //MarkPositionList.Clear();
                MarkPositionList = new ObservableCollection<MarkPosition>();
                for (int i = 0; i < markCoordinates.Count; ++i)
                {
                    CatCoordinates markPosition = markCoordinates[i];
                    MarkPositionList.Add(new MarkPosition(camType, markPosition, i, AlignMode));
                }

                PatternWidth = patternWidth;
                PatternHeight = patternHeight;

                if (SelectedLightType == EnumLightType.COAXIAL)
                {
                    LightValue = coaxialLightValue;
                }
                else if (SelectedLightType == EnumLightType.OBLIQUE)
                {
                    LightValue = obliqueLightValue;
                }

                SelectedProberCam = camType;

                //==> Light Value Update
                ICamera cam = this.VisionManager().GetCam(SelectedProberCam);
                cam.SetLight(SelectedLightType, LightValue);
                LightValue = (ushort)cam.GetLight(SelectedLightType);

                //==> Change camera state
                AssignedCamera = this.VisionManager().GetCam(SelectedProberCam);

                this.VisionManager().StartGrab(SelectedProberCam, this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task PogoAlignPointCommandFunc(EnumPogoAlignPoint point)
        {
            try
            {
                LoggerManager.Debug($"PogoAlignPointCommandFunc() point={point}");
                CardChangeSysParam.PogoAlignPoint.Value = point;
                await PogoSettingCommandFunc();
                this.CardChangeModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EnumProberCam SelectedProberCam { get; set; }
        public VmGPCardChangeMainPage()
        {
            try
            {
                //==> View Model
                SelectedProberCam = EnumProberCam.PIN_LOW_CAM;

                ZTickValue = 1;//==> 10^1, ZValue
                LightTickValue = 1;//==> OBLIQUE, Select Light Type, Light Value
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public async Task<EventCodeEnum> Cleanup(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //==> Z 는 안전하게 내리고
                retVal = this.StageSupervisor().StageModuleState.ZCLEARED();
                //==> Wafer Cam은 접는다.
                WaferCamFoldCommandFunc();

                var cam = this.VisionManager().GetCam(SelectedProberCam);
                cam.SetLight(EnumLightType.COAXIAL, 0);
                cam.SetLight(EnumLightType.OBLIQUE, 0);

                retVal = this.StageSupervisor().StageModuleState.UnLockCCState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
        public void DeInitModule()
        {
        }
        public async Task<EventCodeEnum> DeInitViewModel(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum InitModule()
        {
            return EventCodeEnum.NONE;
        }
        public async Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            try
            {
                Task task = new Task(() =>
                {
                    SetAlignSettingMode(
                                  EnumProberCam.PIN_LOW_CAM,
                                  new List<CatCoordinates>(CardChangeSysParam.GP_SearchedPogoMarkPosList),
                                  CardChangeDevParam.GP_PogoPatternWidth,
                                  CardChangeDevParam.GP_PogoPatternHeight,
                                  CardChangeSysParam.GP_PL_COAXIAL,
                                  CardChangeSysParam.GP_PL_OBLIQUE);
                    this.StageSupervisor().StageModuleState.LockCCState();
                });
                task.Start();
                await task;

                await PogoSettingCommandFunc();

                Task task2 = new Task(() =>
                {
                    if(parameter != null && parameter is bool)
                    {
                        bool observation = Convert.ToBoolean(parameter);
                        if (observation)
                        {
                            string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                            ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                            cardChangeSysParam.CardAlignState = AlignStateEnum.IDLE;

                            if (cardChangeSysParam.ProberCardList == null)
                            {
                                cardChangeSysParam.ProberCardList = new List<ProberCardListParameter>();
                            }

                            ProberCardListParameter proberCard = cardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);
                            if (proberCard == null)
                            {
                                proberCard = (this.LoaderController() as GP_LoaderController).DownloadProbeCardInfo(probeCardID);
                                if (proberCard == null)
                                {
                                    ProberCardListParameter newProberCard = new ProberCardListParameter();
                                    newProberCard.FiducialMarInfos = new List<PinBaseFiducialMarkParameter>();

                                    PinBaseFiducialMarkParameter fidInfo = new PinBaseFiducialMarkParameter();
                                    newProberCard.FiducialMarInfos.Clear();
                                    var mpccZ = this.CoordinateManager().StageCoord.MarkPosInChuckCoord.Z.Value;
                                    var zNegLim = this.MotionManager().GetAxis(EnumAxisConstants.PZ).Param.NegSWLimit.Value;
                                    fidInfo.FiducialMarkPos = new PinCoordinate(-42000, -335000, zNegLim + mpccZ);
                                    fidInfo.CardCenterOffset = new PinCoordinate(50000, -151000, 0);
                                    newProberCard.FiducialMarInfos.Add(fidInfo);

                                    fidInfo = new PinBaseFiducialMarkParameter();
                                    fidInfo.FiducialMarkPos = new PinCoordinate(58000, -335000, zNegLim + mpccZ);
                                    fidInfo.CardCenterOffset = new PinCoordinate(-50000, -151000, 0);
                                    newProberCard.FiducialMarInfos.Add(fidInfo);

                                    if (String.IsNullOrEmpty(probeCardID))
                                    {
                                        newProberCard.CardID = "DefaultCard";
                                    }
                                    else
                                    {
                                        newProberCard.CardID = probeCardID;
                                    }

                                    int alreadyExist = cardChangeSysParam.ProberCardList.Count(x => x.CardID == "DefaultCard");
                                    if (alreadyExist == 0 || newProberCard.CardID != "DefaultCard")
                                    {
                                        cardChangeSysParam.ProberCardList.Add(newProberCard);
                                    }
                                    this.CardChangeModule().SaveSysParameter();
                                }
                                else
                                {
                                    cardChangeSysParam.ProberCardList.Add(proberCard);
                                    this.CardChangeModule().SaveSysParameter();
                                }
                            }
                        }
                        LoggerManager.Debug($"VmGPCardChangeMainPage.PageSwithced() - observation({observation})");
                    }
                });
                task2.Start();
                await task2;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public string GetPostion()
        {
            string retVal = null;
            try
            {
                retVal = SelectedMarkPosition.Description;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public GPCardChangeVMData GetGPCCData()
        {
            GPCardChangeVMData info = new GPCardChangeVMData();
            info.MarkPositions = new ObservableCollection<MarkPosCoord>();

            try
            {
                info.PatternHeight = this.PatternHeight;
                info.PatternWidth = this.PatternWidth;
                info.SelectCam = this.SelectedProberCam;
                info.LightValue = this.LightValue;
                info.ZTickValue = this.ZTickValue;
                info.ZDistanceValue = this.ZDistanceValue;
                info.LightTickValue = this.LightTickValue;
                info.SelectedLightType = this.SelectedLightType;
                double Zpos = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref Zpos);
                double PZpos = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref PZpos);
                info.ZActualPos = Zpos;
                info.PZActualPos = PZpos;
                for (int i = 0; i < MarkPositionList.Count; i++)
                {
                    MarkPosCoord mkc = new MarkPosCoord();
                    mkc.XPos = MarkPositionList[i]._Position.X.Value;
                    mkc.YPos = MarkPositionList[i]._Position.Y.Value;
                    mkc.ZPos = MarkPositionList[i]._Position.Z.Value;
                    mkc.TPos = MarkPositionList[i]._Position.T.Value;
                    mkc.Index = MarkPositionList[i].Index;

                    info.MarkPositions.Add(mkc);
                }
                //info.MarkPositions = MarkPositionList[0]._Position;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return info;
        }
    }

    [Serializable]
    public class MarkPosition : INotifyPropertyChanged, IFactoryModule, IGPCCObservationMarkPosition
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> Description
        private String _Description;
        public String Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> ButtonEnable
        private bool _ButtonEnable;
        public bool ButtonEnable
        {
            get { return _ButtonEnable; }
            set
            {
                if (value != _ButtonEnable)
                {
                    _ButtonEnable = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RegisterPatternCommand
        private AsyncCommand _RegisterPatternCommand;
        public ICommand RegisterPatternCommand
        {
            get
            {
                if (null == _RegisterPatternCommand) _RegisterPatternCommand = new AsyncCommand(RegisterPatternCommandFunc);
                return _RegisterPatternCommand;
            }
        }
        public async Task RegisterPatternCommandFunc()
        {
            string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
            ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

            if (String.IsNullOrEmpty(probeCardID))
            {
                probeCardID = "DefaultCard";
            }

            ProberCardListParameter probeCard = cardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);
            
            var retVal = this.GPCardAligner().RegisterPattern(_ProberCam, Index, AlignModule, probeCard);
            if (retVal != EventCodeEnum.NONE)
            {

                await this.MetroDialogManager().ShowMessageDialog("Regist Pattern Fail", "Save Z Position Fail", EnumMessageStyle.Affirmative);
            }
            else
            {
                await this.MetroDialogManager().ShowMessageDialog("Regist Pattern Done", $"Succeed", EnumMessageStyle.Affirmative);
            }
        }
        #endregion

        #region ==> RegisterPosCommand
        private AsyncCommand _RegisterPosCommand;
        public ICommand RegisterPosCommand
        {
            get
            {
                if (null == _RegisterPosCommand) _RegisterPosCommand = new AsyncCommand(RegisterPosCommandFunc);
                return _RegisterPosCommand;
            }
        }

        public async Task RegisterPosCommandFunc()
        {
            await SaveCurrentPos();
        }
        #endregion

        private EnumCCAlignModule _AlignModule;
        public EnumCCAlignModule AlignModule
        {
            get { return _AlignModule; }
            set
            {
                if (value != _AlignModule)
                {
                    _AlignModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void MoveToMark()
        {
            EventCodeEnum result = EventCodeEnum.NONE;

            try
            {
                double posX = _Position.X.Value;
                double posY = _Position.Y.Value;
                double posZ = _Position.Z.Value;

                ICamera cam = this.VisionManager().GetCam(_ProberCam);

                if (_ProberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    //==> Wafer Cam과 Card의 상판은 너무 가깝다, 그래서 안전하게 움직이기 위해 다음처럼 Z를 안전하게 내린다.
                    //result = this.StageSupervisor().StageModuleState.ZCLEARED();
                    if (result != EventCodeEnum.NONE)
                    {
                        return;
                    }

                    cam.SetLight(EnumLightType.COAXIAL, CardChangeSysParam.GP_WL_COAXIAL);
                    cam.SetLight(EnumLightType.OBLIQUE, CardChangeSysParam.GP_WL_OBLIQUE);

                    result = this.StageSupervisor().StageModuleState.CardViewMove(posX, posY);
                    if (result != EventCodeEnum.NONE)
                    {
                        return;
                    }

                    double axisZsafeOffset = 15000; //마크를 봤을때 핀하이, 웨이퍼하이의 거리는 35.5mm이다 마크 보는 포지션에서 척은 pz보다 20mm높다. 
                    double axisPZsafeOffset = 35000;
                    var axisz = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    var axispz = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                    var markRefposZ = this.CoordinateManager().StageCoord.RefMarkPos.Z.Value;
                    if (axisz.Status.RawPosition.Actual > markRefposZ + axisZsafeOffset)
                    {
                        this.StageSupervisor().StageModuleState.CCZCLEARED();
                    }
                    if (axispz.Status.RawPosition.Actual > markRefposZ + axisPZsafeOffset)
                    {
                        this.StageSupervisor().StageModuleState.CCZCLEARED();
                    }
                    //==> 움직인 후에 Wafer Camera를 움직이는 게 안전할듯.
                    StageCylinderType.MoveWaferCam.Extend();
                }
                else if (_ProberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    //==> 안전하게 먼저 Wafer Camera 접는다.
                    StageCylinderType.MoveWaferCam.Retract();

                    cam.SetLight(EnumLightType.COAXIAL, CardChangeSysParam.GP_PL_COAXIAL);
                    cam.SetLight(EnumLightType.OBLIQUE, CardChangeSysParam.GP_PL_OBLIQUE);

                    ////==> TODO : 좌표계 변환으로 인한 조치
                    //{
                    //    result = this.StageSupervisor().StageModuleState.ZCLEARED_SWNegLimit();
                    //    if (result != EventCodeEnum.NONE)l
                    //    {
                    //        return;
                    //    }
                    //    var currentPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                    //    posZ = currentPos.Z.Value;
                    //}
                    //posZ = 4000; //Safety Position TODO Remove!!! Lloyd 190819

                    bool canMove = false;
                    bool isPodUp = false;
                    EventCodeEnum isShutterCloseRetVal = EventCodeEnum.UNDEFINED;

                    isShutterCloseRetVal = this.LoaderController().IsShutterClose();
                    isPodUp = this.StageSupervisor().StageModuleState.IsCardExist();

                    if (AlignModule == EnumCCAlignModule.CARD)
                    {
                        if (isPodUp)
                        {
                            // Card Pod 이 올라와 있는 상태에서는 Card Fiducial Mark Pos 이동 불가.
                            canMove = false;
                        }
                        else
                        {
                            canMove = true;
                        }
                    }
                    else if (AlignModule == EnumCCAlignModule.POGO)
                    {
                        if (isShutterCloseRetVal != EventCodeEnum.NONE)
                        {
                            // LCC Arm이 Stage안쪽으로 들어와 있는 상태. Pogo Pattern Pos 이동 불가.
                            canMove = false;
                        }
                        else
                        {
                            canMove = true;
                        }
                    }

                    if (canMove)
                    {
                        result = this.StageSupervisor().StageModuleState.PogoViewMove(posX, posY, posZ);
                        if (result != EventCodeEnum.NONE)
                        {
                            return;
                        }
                    }
                    else
                    {
                        this.MetroDialogManager().ShowMessageDialog($"Move Error", $"It cannot be moved because the card pod is up or the card arm is inside the stage. \nAlign Mode : {AlignModule}, Pod Up : {isPodUp}, LCC State : {isShutterCloseRetVal}", EnumMessageStyle.Affirmative);
                    }
                }
                else
                {
                    ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    this.MotionManager().AbsMove(xAxis, 0, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                    this.MotionManager().AbsMove(yAxis, 0, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private async Task SaveCurrentPos()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CatCoordinates currentPos = null;
                CatCoordinates selectedMarkPos = null;
                bool isSave = true;
                double dist = 0;
                string msg = null;

                string probeCardID = (this.LoaderController() as GP_LoaderController).GPLoaderService.CTR_GetProbeCardIDLastTwoWord();
                if (String.IsNullOrEmpty(probeCardID))
                {
                    probeCardID = "DefaultCard";
                }
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ProberCardListParameter proberCard = cardChangeSysParam.ProberCardList.First(x => x.CardID == probeCardID);

                if (_ProberCam == EnumProberCam.WAFER_LOW_CAM)
                {
                    currentPos = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                    if (AlignModule == EnumCCAlignModule.CARD)
                    {
                        selectedMarkPos = CardChangeSysParam.GP_SearchedCardMarkPosList[Index];
                        for (int i = 0; i < CardChangeSysParam.GP_SearchedCardMarkPosList.Count; i++)
                        {
                            if (i == Index)
                            {
                                continue;
                            }
                            else
                            {
                                var pos = CardChangeSysParam.GP_SearchedCardMarkPosList[i];
                                dist = Math.Sqrt(Math.Pow(currentPos.X.Value - pos.X.Value, 2) + Math.Pow(currentPos.Y.Value - pos.Y.Value, 2));
                                if (dist <= CardChangeSysParam.DistanceOffset)
                                {
                                    isSave = false;
                                    msg = $"Current Index:{Index + 1}, DistanceOffset:{CardChangeSysParam.DistanceOffset}, Distance Between {i + 1} and {Index + 1} : {dist}\n Would you like to proceed?";
                                    break;
                                }
                            }

                        }
                    }
                    else if (AlignModule == EnumCCAlignModule.POD)
                    {
                        selectedMarkPos = CardChangeSysParam.CardPodMarkPosList[Index];
                        for (int i = 0; i < CardChangeSysParam.CardPodMarkPosList.Count; i++)
                        {
                            if (i == Index)
                            {
                                continue;
                            }
                            else
                            {
                                var pos = CardChangeSysParam.CardPodMarkPosList[i];
                                dist = Math.Sqrt(Math.Pow(currentPos.X.Value - pos.X.Value, 2) + Math.Pow(currentPos.Y.Value - pos.Y.Value, 2));
                                if (dist <= CardChangeSysParam.DistanceOffset)
                                {
                                    isSave = false;
                                    msg = $"Current Index:{Index + 1}, DistanceOffset:{CardChangeSysParam.DistanceOffset}, Distance Between {i + 1} and {Index + 1} : {dist}\n Would you like to proceed?";
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (_ProberCam == EnumProberCam.PIN_LOW_CAM)
                {
                    if(AlignModule == EnumCCAlignModule.CARD)
                    {
                        if(proberCard != null)
                        {
                            currentPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                            selectedMarkPos = proberCard.FiducialMarInfos[Index].FiducialMarkPos;
                            for (int i = 0; i < proberCard.FiducialMarInfos.Count; i++)
                            {
                                if (i == Index)
                                {
                                    continue;
                                }
                                else
                                {
                                    var pos = proberCard.FiducialMarInfos[i].FiducialMarkPos;
                                    dist = Math.Sqrt(Math.Pow(currentPos.X.Value - pos.X.Value, 2) + Math.Pow(currentPos.Y.Value - pos.Y.Value, 2));
                                    if (dist <= CardChangeSysParam.DistanceOffset)
                                    {
                                        isSave = false;
                                        msg = $"Current Index:{Index + 1}, DistanceOffset:{CardChangeSysParam.DistanceOffset}, Distance Between {i + 1} and {Index + 1} : {dist}\n Would you like to proceed?";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if(CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_4)
                        {
                            currentPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                            selectedMarkPos = CardChangeSysParam.GP_SearchedPogoMarkPosList[Index];
                            for (int i = 0; i < CardChangeSysParam.GP_SearchedPogoMarkPosList.Count; i++)
                            {
                                if (i == Index)
                                {
                                    continue;
                                }
                                else
                                {
                                    var pos = CardChangeSysParam.GP_SearchedPogoMarkPosList[i];
                                    dist = Math.Sqrt(Math.Pow(currentPos.X.Value - pos.X.Value, 2) + Math.Pow(currentPos.Y.Value - pos.Y.Value, 2));
                                    if (dist <= CardChangeSysParam.DistanceOffset)
                                    {
                                        isSave = false;
                                        msg = $"Current Index:{Index + 1}, DistanceOffset:{CardChangeSysParam.DistanceOffset}, Distance Between {i + 1} and {Index + 1} : {dist}\n Would you like to proceed?";
                                        break;
                                    }
                                }
                            }
                        }
                        else if(CardChangeSysParam.PogoAlignPoint.Value == EnumPogoAlignPoint.POINT_3)
                        {
                            currentPos = this.CoordinateManager().PinLowPinConvert.CurrentPosConvert();
                            selectedMarkPos = CardChangeSysParam.GP_SearchedPogoMarkPosList3P[Index];
                            for (int i = 0; i < CardChangeSysParam.GP_SearchedPogoMarkPosList3P.Count; i++)
                            {
                                if (i == Index)
                                {
                                    continue;
                                }
                                else
                                {
                                    var pos = CardChangeSysParam.GP_SearchedPogoMarkPosList3P[i];
                                    dist = Math.Sqrt(Math.Pow(currentPos.X.Value - pos.X.Value, 2) + Math.Pow(currentPos.Y.Value - pos.Y.Value, 2));
                                    if (dist <= CardChangeSysParam.DistanceOffset)
                                    {
                                        isSave = false;
                                        msg = $"Current Index:{Index + 1}, DistanceOffset:{CardChangeSysParam.DistanceOffset}, Distance Between {i + 1} and {Index + 1} : {dist}\n Would you like to proceed?";
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggerManager.Debug($"CardChangeSysParam.PogoAlignPoint.Value is INVALID");
                        }
                    }
                }

                if (currentPos == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Position Update Error", "Error", EnumMessageStyle.Affirmative);
                }

                if (selectedMarkPos == null)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Position Update Error", "Error", EnumMessageStyle.Affirmative);
                }
                if (!isSave)
                {
                    var retmsg = await this.MetroDialogManager().ShowMessageDialog(
                            "DistanceOffset Warning",
                            msg,
                            EnumMessageStyle.AffirmativeAndNegative);
                    if (retmsg == EnumMessageDialogResult.AFFIRMATIVE)
                    {
                        isSave = true;
                    }
                }
                if (isSave)
                {
                    selectedMarkPos.X.Value = currentPos.X.Value;
                    selectedMarkPos.Y.Value = currentPos.Y.Value;
                    selectedMarkPos.Z.Value = currentPos.Z.Value;
                    _Position.X.Value = currentPos.X.Value;
                    _Position.Y.Value = currentPos.Y.Value;
                    Description = $"X : {string.Format("{0:0.00}", (_Position.X.Value))}  Y :  {string.Format("{0:0.00}", (_Position.Y.Value))}";
                    EventCodeEnum saveResult = this.CardChangeModule().SaveSysParameter();

                    ProberCardListParameter probeCard = cardChangeSysParam.ProberCardList.FirstOrDefault(x => x.CardID == probeCardID);
                    if(probeCard != null)
                    {
                        this.LoaderController().UploadProbeCardInfo(probeCard);
                    }

                    if (saveResult != EventCodeEnum.NONE)
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Position Update Error", "Error", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        await this.MetroDialogManager().ShowMessageDialog("Position Update Done", $"Succeed", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }

        private ICardChangeSysParam CardChangeSysParam => this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
        private ICardChangeDevParam CardChangeDevParam => this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

        public EnumProberCam _ProberCam;
        public CatCoordinates _Position;

        public int Index { get; set; }
        public MarkPosition(EnumProberCam proberCam, CatCoordinates position, int index, EnumCCAlignModule module)
        {
            _ProberCam = proberCam;
            _Position = position;
            Index = index;

            Description = $"X : {string.Format("{0:0.00}", (_Position.X.Value))}  Y :  {string.Format("{0:0.00}", (_Position.Y.Value))}";
            ButtonEnable = false;

            AlignModule = module;

        }
    }


}
