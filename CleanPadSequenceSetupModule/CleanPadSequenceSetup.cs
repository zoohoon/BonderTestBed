using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CleanPadSequenceSetupModule
{
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using NeedleCleanerModuleParameter;
    using NeedleCleanSequencePageView;
    using NeedleCleanViewer;
    using Newtonsoft.Json;
    using PnPControl;
    using ProbeCardObject;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign.ProbeCardData;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using RelayCommandBase;
    using SerializerUtil;
    using SubstrateObjects;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Xml.Serialization;
    using VirtualKeyboardControl;

    public class CleanPadSequenceSetup : PNPSetupBase, IHasDevParameterizable, IHasSysParameterizable, INotifyPropertyChanged, ISetup, IParamNode, ITemplateModule, IPackagable
    {
        public override Guid ScreenGUID { get; } = new Guid("52AB4222-912E-43B4-E9DC-B553F7ECCC35");
        public override bool Initialized { get; set; } = false;

        private IStateModule _CleanPadSequenceModule;
        public IStateModule CleanPadSequenceModule
        {
            get { return _CleanPadSequenceModule; }
            set
            {
                if (value != _CleanPadSequenceModule)
                {
                    _CleanPadSequenceModule = value;
                    RaisePropertyChanged();
                }
            }
        }
        private IWaferObject Wafer
        {
            get
            {
                return this.StageSupervisor().WaferObject;
            }
        }
        //public IParam DevParam { get; set; }
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
        public SubModuleStateBase SubModuleState { get; set; }

        public SubModuleMovingStateBase MovingState { get; set; }

        public CleanPadSequenceSetup()
        {
        }
        public CleanPadSequenceSetup(IStateModule Module)
        {
            _CleanPadSequenceModule = Module;
        }

        private UcNeedleCleanSequencePage _SequencePageView;
        public UcNeedleCleanSequencePage SequencePageView
        {
            get { return _SequencePageView; }
            set
            {
                if (value != _SequencePageView)
                {
                    _SequencePageView = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NeedleCleanViewModel NeedleCleanVM { get; set; }

        public new IProbeCard ProbeCard { get { return this.GetParam_ProbeCard(); } }

        #region ==> DutList
        //private ObservableCollection<IDut> _NC.TempDutList = new ObservableCollection<IDut>();
        //public ObservableCollection<IDut> NC.TempDutList
        //{
        //    get { return _NC.TempDutList; }
        //    set
        //    {
        //        if (value != _NC.TempDutList)
        //        {
        //            _NC.TempDutList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        //private ObservableCollection<IDut> _NC.TempDutList = new ObservableCollection<IDut>();
        //public ObservableCollection<IDut> NC.TempDutList
        //{
        //    get { return _NC.TempDutList; }
        //    set
        //    {
        //        if (value != _NC.TempDutList)
        //        {
        //            _NC.TempDutList = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        #endregion

        private NeedleCleanDeviceParameter _NeedleCleanDevParam;
        public NeedleCleanDeviceParameter NeedleCleanDevParam
        {
            get { return _NeedleCleanDevParam; }
            set
            {
                if (value != _NeedleCleanDevParam)
                {
                    _NeedleCleanDevParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        private NeedleCleanSystemParameter _NeedleCleanSysParam;
        public NeedleCleanSystemParameter NeedleCleanSysParam
        {
            get { return _NeedleCleanSysParam; }
            set
            {
                if (value != _NeedleCleanSysParam)
                {
                    _NeedleCleanSysParam = value;
                    RaisePropertyChanged();
                }
            }
        }

        public new NeedleCleanObject NC { get { return (NeedleCleanObject)this.StageSupervisor().NCObject; } }


        private CustomDialog _Dialog;

        public CustomDialog Dialog
        {
            get { return _Dialog; }
            set
            {
                _Dialog = value;
            }
        }


        //private int ncNum = 0;
        private double _RatioX;
        private double _RatioY;
        private float _DutWidth;
        private float _DutHeight;
        private float _WinSizeX;
        private float _WinSizeY;
        private double _MinX = 0.0d;
        private double _MinY = 0.0d;
        private double _MaxX = 0.0d;
        private double _MaxY = 0.0d;
        private double _CenterX = 0.0d;
        private double _CenterY = 0.0d;
        private double _DiffX = 0.0d;
        private double _DiffY = 0.0d;

        private long User_Dist = 1000;
        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();

        //int ncNum = 1;
        //Don`t Touch

        public EventCodeEnum DoExecute() //실제 프로세싱 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            MovingState.Moving();

            /*
                실제 프로세싱 코드 작성
             
             
             */
            MovingState.Stop();
            retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public EventCodeEnum DoClearData() //현재 Parameter Check 및 Init하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
            //  return ParamValidation();
        }
        public override EventCodeEnum ParamValidation()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //Parameter 확인한다.

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Update 상태가 있는지 없는지를 확인해준다.
                ///retVal = Extensions_IParam.ElementStateUpdateValidation(Param);

                // IParam type 의 Parameter 객체를 넘기면 객체내의 Element 타입의 파라미터들의 상태중 Apply 상태가 있는지 없는지를 확인해준다.
                ///retVal = Extensions_IParam.ElementStateApplyValidation(Param);

                //모듈의  Setup상태를 변경해준다.

                if (retVal == EventCodeEnum.NONE)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }

            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNKNOWN_EXCEPTION;
                LoggerManager.Exception(err);
            }
            return retVal;
        }



        public EventCodeEnum DoRecovery() // Recovery때 하는 코드
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            return retVal;
        }
        public EventCodeEnum DoExitRecovery()
        {
            try
            {

            }
            catch (Exception err)
            {
                throw err;
            }
            return EventCodeEnum.NONE;
        }
        #region Don`t Touch Code
        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }
        public EventCodeEnum Recovery()
        {
            return SubModuleState.Recovery();
        }
        public EventCodeEnum ExitRecovery()
        {

            return SubModuleState.ExitRecovery();
        }
        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }
        public MovingStateEnum GetMovingState()
        {
            return MovingState.GetState();
        }
        #endregion
        public override EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    //init 하는 코드 하고 나서 Idle 상태로 초기화

                    SequencePageView = new UcNeedleCleanSequencePage();
                    //NeedleCleanItem = new NeedleClean();

                    //NeedleCleanViewer = new NeedleCleanView();//==> Nick

                    CleanPadSequenceModule = this.NeedleCleaner();
                    var ncModule = (IHasDevParameterizable)CleanPadSequenceModule;
                    //NeedleCleanDevParam = (NeedleCleanDeviceParameter)ncModule.DevParam;
                    NeedleCleanDevParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;


                    if (((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).NeedleCleanPadWidth.Value == 200000 &&
                        ((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).NeedleCleanPadHeight.Value == 100000)
                    {
                        _WinSizeX = 800;
                        _WinSizeY = 400;
                    }
                    else if (((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).NeedleCleanPadWidth.Value == 160000 &&
                        ((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).NeedleCleanPadHeight.Value == 160000)
                    {
                        _WinSizeX = 640;
                        _WinSizeY = 640;
                    }
                    else
                    {
                        _WinSizeX = 800;
                        _WinSizeY = 640;
                    }

                    NC.TotalCount = 0;
                    NC.Distance = 0;
                    NC.Index = ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index;
                    NC.DistanceVisible = Visibility.Visible;

                    //Template을 아직 사용하지 않으므로 코드에서 적용.
                    SetEnableState(EnumEnableState.ENABLE);

                    NeedleCleanVM = new NeedleCleanViewModel();// ProbeCard, NeedleCleanDevParam, NC.NCSysParam, NC, (WaferObject)Wafer);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override void DeInitModule()
        {
        }

        private bool ParamChanged = false;

        public void ApplyParams(List<byte[]> datas)
        {
            try
            {
                PackagableParams = datas;

                foreach (var param in datas)
                {
                    object target;
                    SerializeManager.DeserializeFromByte(param, out target, typeof(NeedleCleanDeviceParameter));
                    if (target != null)
                    {
                        this.NeedleCleaner().NeedleCleanDeviceParameter_IParam = (NeedleCleanDeviceParameter)target;
                        //NC.TotalCount = ((NeedleCleanDeviceParameter)target).SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                        //NC.Distance = ((NeedleCleanDeviceParameter)target).SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                        break;
                    }
                }


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                var ncModule = (IHasDevParameterizable)this.NeedleCleaner();

                //NeedleCleanDevParam = ncModule.DevParam.Copy() as NeedleCleanDeviceParameter;
                //NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam.Copy() as NeedleCleanDeviceParameter;
                RetVal = this.NeedleCleaner().LoadDevParameter();
                NeedleCleanDevParam = (NeedleCleanDeviceParameter)this.NeedleCleaner().NeedleCleanDeviceParameter_IParam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //var ncModule = (IHasDevParameterizable)this.NeedleCleaner();

                retVal = this.NeedleCleaner().SaveDevParameter();

                //ncModule.DevParam = NeedleCleanDevParam.Copy() as NeedleCleanDeviceParameter;
                //this.NeedleCleaner().NeedleCleanDeviceParameter_IParam = NeedleCleanDevParam.Copy() as NeedleCleanDeviceParameter;

                //retVal = this.SaveParameter(NeedleCleanDevParam);
                //retVal = ncModule.LoadDevParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal; // EventCodeEnum.NONE;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //NeedleCleanSysParam = this.StageSupervisor().NCObject.NCSysParam_IParam.Copy() as NeedleCleanSystemParameter;

                RetVal = this.StageSupervisor().LoadNCSysObject();
                NeedleCleanSysParam = this.StageSupervisor().NCObject.NCSysParam_IParam as NeedleCleanSystemParameter;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(NC.NCSysParam);
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            //this.StageSupervisor().NCObject.NCSysParam_IParam = NeedleCleanSysParam.Copy() as NeedleCleanSystemParameter;
            //retVal = this.StageSupervisor().SaveNCSysObject();
            //retVal = this.StageSupervisor().LoadNCSysObject();

            return retVal;
        }

        private void ShowArrow(NC_CleaningDirection ArrowDir = NC_CleaningDirection.HOLD)
        {
            try
            {
                //if (ArrowDir == NC_CleaningDirection.BOTTOM)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //           new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.BOTTOM_LEFT)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.BOTTOM_RIGHT)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.LEFT)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.RIGHT)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.TOP)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.TOP_LEFT)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png", UriKind.Absolute));
                //}
                //else if (ArrowDir == NC_CleaningDirection.TOP_RIGHT)
                //{
                //    NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png", UriKind.Absolute));
                //}
                if (ArrowDir == NC_CleaningDirection.BOTTOM)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.BOTTOM_LEFT)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.BOTTOM_RIGHT)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.LEFT)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.RIGHT)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.TOP)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.TOP_LEFT)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png";
                }
                else if (ArrowDir == NC_CleaningDirection.TOP_RIGHT)
                {
                    NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png";
                }
                //ViewPort = new Rect(0, 0, 1, 1);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "ShowArrow() : Error occurred.");
            }
        }

        private void InitializePadView()
        {
            try
            {
                NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NC.TempDutList.Clear();
                        foreach (IDut displayDut in ProbeCard.ProbeCardDevObjectRef.DutList)
                        {
                            displayDut.DutSizeLeft = (displayDut.MacIndex.XIndex * _DutWidth) + (_WinSizeX / 2) - _DutWidth;
                            displayDut.DutSizeTop = ((ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY - displayDut.MacIndex.YIndex) * _DutHeight) + (_WinSizeY / 2) - _DutHeight;
                            displayDut.DutSizeHeight = _DutHeight;
                            displayDut.DutSizeWidth = _DutWidth;

                            displayDut.DutVisibility = Visibility.Hidden;
                            NC.TempDutList.Add(displayDut);
                        }

                        if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.BOTTOM)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.BOTTOM_LEFT)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.BOTTOM_RIGHT)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.LEFT)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.RIGHT)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.TOP)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.TOP_LEFT)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png";
                        }
                        else if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value == NC_CleaningDirection.TOP_RIGHT)
                        {
                            NC.ImageSource = "pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png";
                        }

                        // update display
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                        NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                        ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);
                    });
                }
                else
                {
                    NC.ImageSource = "";

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NC.TempDutList.Clear();

                        List<Element<NCCoordinate>> temppos = new List<Element<NCCoordinate>>();
                        for (int i = 0; i < NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count(); i++)
                        {
                            if (i == 0)
                            {
                                temppos.Add(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq[i]);
                            }
                            else
                            {
                                double posX;
                                double posY;
                                posX = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq[i].Value.X.Value +
                                    temppos[i - 1].Value.X.Value;

                                posY = -NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq[i].Value.Y.Value +
                                    temppos[i - 1].Value.Y.Value;

                                Element<NCCoordinate> newPos = new Element<NCCoordinate>();
                                newPos.Value = new NCCoordinate();
                                newPos.Value.X.Value = posX;
                                newPos.Value.Y.Value = posY;

                                temppos.Add(newPos);
                            }
                        }
                        foreach (Element<NCCoordinate> nPos in temppos)
                        //foreach (Element<NCCoordinate> nPos in NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq)                  
                        {
                            for (int i = 0; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count() - 1; i++)
                            {
                                Dut dut = new Dut();
                                dut.DutSizeHeight = _DutHeight;
                                dut.DutSizeWidth = _DutWidth;
                                dut.DutSizeLeft = (ProbeCard.ProbeCardDevObjectRef.DutList[i].MacIndex.XIndex * _DutWidth) + (_WinSizeX / 2) - _DutWidth + (nPos.Value.X.Value * _RatioX);
                                dut.DutSizeTop = ((ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY - ProbeCard.ProbeCardDevObjectRef.DutList[i].MacIndex.YIndex) * _DutHeight) + (_WinSizeY / 2) - _DutHeight + (nPos.Value.Y.Value * _RatioY);

                                NC.TempDutList.Add(dut);
                            }
                        }

                        CalcCenterXOffset();
                        CalcCenterYOffset();
                        for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                        {
                            NC.TempDutList[i].DutSizeLeft += _DiffX;
                            NC.TempDutList[i].DutSizeTop += _DiffY;
                        }


                        NC.DistanceVisible = Visibility.Collapsed;
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "InitializePadView() : Error occurred.");
            }
        }

        public override bool IsParameterChanged(bool issave = false)
        {
            bool retVal = false;
            try
            {
                retVal = IsParamChanged || ParamChanged;
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
                if (NC.TotalCount >= 1)
                {
                    // 필요한 파라미터가 모두 설정됨.
                    SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                }
                else
                {
                    // 필요한 파라미터가 모두 설정 안됨.
                    // setup 중 다음 단계로 넘어갈수 없다.
                    // Lot Run 시 Lot 를 동작 할 수 없다.
                    SetNodeSetupState(EnumMoudleSetupState.NOTCOMPLETED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// 현재 모듈의 PNP가 화면에 뜰때마다 호출되는 함수
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override Task<EventCodeEnum> InitViewModel()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //Pnp를 사용한다면 ListView에 띄워줄 이름을 기재해 주어야한다.
                Header = "CleanPadSequenceSetup";

                //DisplayPort (Vision 화면)와 관련된 작업. 
                //InitPnpModule() : Stage, Loader 카메라 모두 화면과 연결.
                InitPnpModuleStage_AdvenceSetting();  // :  Stage 카메라만 화면과 연결.
                //InitPnpModuleStage_AdvenceSetting();
                //Light Jog를 초기화한다. 
                //CurCam Property를 초기화 하지 않았으면 Camera Type을 같이 넘겨주어야한다.
                //CurCam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM); 이와 같은 고르고 CurCam 을 할당해주었다면 InitLightJog에 this 만 넘겨주어도 된다.
                InitLightJog(this, EnumProberCam.WAFER_HIGH_CAM);

                AdvanceSetupView = new CleanPadSequenceAdvanceSetup.View.CleanPadSequenceAdvanceSetupView();
                AdvanceSetupViewModel = new CleanPadSequenceAdvanceSetup.ViewModel.CleanPadSequenceAdvanceSetupViewModel();

                //NeedleCleanVM = new NeedleCleanViewModel(ProbeCard, NeedleCleanDevParam, NC.NCSysParam, NC, (WaferObject)Wafer);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.INITVIEWMODEL_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        public override async Task<EventCodeEnum> PageSwitched(object parameter = null)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //PackagableParams.Clear();
                //PackagableParams.Add(SerializeManager.SerializeToByte(NeedleCleanDevParam));

                this.VisionManager().SetDisplayChannelStageCameras(DisplayPort);
                retVal = await InitSetup();
                EnableUseBtn();
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PAGE_SWITCHED_EXCEPTION;
                LoggerManager.Exception(err);
                throw err;
            }
            return retVal;
        }

        /// <summary>
        /// Setup시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitSetup()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                CurCam = this.VisionManager().GetCam(EnumProberCam.PIN_LOW_CAM);
                // MainView 화면에 Camera(Vision) 화면이 나온다.

                //==> Nick
                //MainViewTarget = NeedleCleanViewer;

                // 파라미터 업데이트
                //retVal = LoadDevParameter();

                InitPnpUI();        // => Cleaning Type update

                //==> 
                MainViewTarget = SequencePageView;

                // MiniView 화면에 WaferMap 화면이 나온다.
                //MiniViewTarget = Wafer;
                // MiniView 화면에 Dut 화면이 나온다.
                //MiniViewTarget = ProbeCard;

                MiniViewTarget = null;

                MiniViewTargetVisibility = Visibility.Hidden;

                MiniViewSwapVisibility = Visibility.Hidden;
                LightJogVisibility = Visibility.Hidden;
                MotionJogVisibility = Visibility.Hidden;
                MainViewZoomVisibility = Visibility.Hidden;

                UseUserControl = UserControlFucEnum.PTRECT;

                TargetRectangleWidth = 128;
                TargetRectangleHeight = 128;


                //Dialog = new UcNeedleCleanSequence(CleanPadSequenceModule);
                //Dialog = new UcNeedleCleanSequence(NeedleCleanDevParam);

                //FiveButton.Command = new AsyncCommand(SetManualInputControl);                

                float width;
                float height;
                float padsizeratio;

                width = (float)((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).NeedleCleanPadWidth.Value;
                height = (float)((NeedleCleanSystemParameter)this.StageSupervisor().NCObject.NCSysParam_IParam).NeedleCleanPadHeight.Value;
                padsizeratio = (width / height);

                // 화면에 표시되는 영역은 가로 800, 세로 400이다. 따라서 가로 세로의 비율은 2:1며 표시하고 싶은 패드 사이즈의 비율에 따라 더 
                // 긴쪽을 기준으로 디스플레이 될 기준을 잡는다.
                if (padsizeratio > 2)
                {
                    // 가로의 길이가 세로의 길이보다 2배 이상 긴 경우. 가로를 먼저 맞춘다
                    _WinSizeX = 800;
                    _WinSizeY = 800 / padsizeratio;
                }
                else
                {
                    // 가로의 길이가 세로의 길이보다 2배 미만인 경우 혹은 같거나 세로가 더 긴 경우. 세로를 먼저 맞춘다.
                    _WinSizeY = 400;
                    _WinSizeX = 400 * padsizeratio;
                }

                _RatioX = _WinSizeX / (NC.NCSysParam.SheetDefs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].Range.Value.X.Value * 2);
                _RatioY = _WinSizeY / (NC.NCSysParam.SheetDefs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].Range.Value.Y.Value * 2);

                NC.SheetWidth = NC.NCSysParam.SheetDefs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].Range.Value.X.Value * 2 * _RatioX;
                NC.SheetHeight = NC.NCSysParam.SheetDefs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].Range.Value.Y.Value * 2 * _RatioY;

                _DutWidth = (float)(Wafer.GetPhysInfo().DieSizeX.Value * _RatioX);
                _DutHeight = (float)(Wafer.GetPhysInfo().DieSizeY.Value * _RatioY);

                InitializePadView();

                //NC.TempDutList.Clear();
                //foreach (Dut displayDut in ProbeCard.DutList)
                //{
                //    displayDut.DutSizeLeft = (displayDut.MacIndex.XIndex.Value * _DutWidth) + (_WinSizeX / 2) - _DutWidth;
                //    displayDut.DutSizeTop = (displayDut.MacIndex.YIndex.Value * _DutHeight) + (_WinSizeY / 2) - _DutHeight;
                //    displayDut.DutSizeHeight = _DutHeight;
                //    displayDut.DutSizeWidth = _DutWidth;

                //    if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                //        displayDut.DutVisibility = Visibility.Hidden;
                //    else
                //        displayDut.DutVisibility = Visibility.Visible;

                //    NC.TempDutList.Add(displayDut);
                //}

                //ViewPort = new Rect();
                //NC.ImageSource = null;
                NC.Index = ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index;

                //SetNodeSetupState(EnumMoudleSetupState.COMPLETE);
                //SetNodeSetupState(EnumMoudleSetupState.NONE);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }
        /// <summary>
        /// Recovery시에 설정할데이터 화면등을 정의.
        /// </summary>
        /// <returns></returns>
        public Task<EventCodeEnum> InitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                InitPnpUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.FromResult<EventCodeEnum>(retVal);
        }

        private EventCodeEnum InitPnpUI()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                // Button 들에서 설정할수 있는것들.
                // Caption : 버튼에 보여줄 문자
                // CaptionSize : 문자 크기
                // IconSource : 버튼에 보여줄 이미지
                // RepeatEnable : true - 버튼에 Repeat 기능 활성화 / false - 버튼에 Repeat 기능 비활성화
                // Visibility : Visibility.Visible - 버튼 보임 / Visibility.Hidden - 버튼 안보임
                // IsEnabled : true - 버튼 활성화 / false - 버튼 비활성화


                //StepLabel = "";

                //OneButton.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //    new Uri("pack://application:,,,/ImageResourcePack;component/Images/Add.png", UriKind.Absolute));
                //OneButton.Command = new RelayCommand(AddCommand);     

                //TwoButton.IconSource = new System.Windows.Media.Imaging.BitmapImage(
                //     new Uri("pack://application:,,,/ImageResourcePack;component/Images/AppDeleteIcon.png", UriKind.Absolute));
                //TwoButton.Command = new RelayCommand(ClearCommand);      

                PadJogLeftUp.Caption = null;
                PadJogLeftUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png");
                //PadJogLeftUp.Command = new RelayCommand(PadJogLeftUpCommand); 
                PadJogLeftUp.Command = new AsyncCommand(PadJogLeftUpCommand);

                PadJogRightUp.Caption = null;
                PadJogRightUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png");
                PadJogRightUp.Command = new AsyncCommand(PadJogRightUpCommand);

                PadJogLeftDown.Caption = null;
                PadJogLeftDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftDownG.png");
                PadJogLeftDown.Command = new AsyncCommand(PadJogLeftDownCommand);

                PadJogRightDown.Caption = null;
                PadJogRightDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png");
                PadJogRightDown.Command = new AsyncCommand(PadJogRightDownCommand);

                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    PadJogSelect.Caption = null;
                    PadJogSelect.Command = null;

                    OneButton.Caption = null;
                    OneButton.Command = null;
                    NC.DistanceVisible = Visibility.Visible;
                }
                else
                {
                    // cleaning Distance 받는 기능 추가
                    PadJogSelect.Caption = User_Dist.ToString();
                    PadJogSelect.Command = new RelayCommand(TextBoxClickCommand);

                    // 클리어 버튼 추가
                    OneButton.Caption = "CLEAR";
                    OneButton.Command = new RelayCommand(ClearCommand);
                    NC.DistanceVisible = Visibility.Collapsed;
                }

                PadJogLeft.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png");
                PadJogLeft.Command = new AsyncCommand(PadJogLeftCommand);

                PadJogRight.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogRightG.png");
                PadJogRight.Command = new AsyncCommand(PadJogRightCommand);

                PadJogUp.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png");
                PadJogUp.Command = new AsyncCommand(PadJogUpCommand);

                PadJogDown.SetIconSoruce("pack://application:,,,/ImageResourcePack;component/Images/JogDownG.png");
                PadJogDown.Command = new AsyncCommand(PadJogDownCommand);

                //PadJogLeft.Command = new RelayCommand(UCDisplayRectWidthMinus);
                //PadJogRight.Command = new RelayCommand(UCDisplayRectWidthPlus);
                //PadJogUp.Command = new RelayCommand(UCDisplayRectHeightPlus);
                //PadJogDown.Command = new RelayCommand(UCDisplayRectHeightMinus);

                //PadJogLeft.RepeatEnable = true;
                //PadJogRight.RepeatEnable = true;
                //PadJogUp.RepeatEnable = true;
                //PadJogDown.RepeatEnable = true;          

                //ChangeWidthValue , ChangeHeightValue 값 변경 으로 Rect 사이즈 얼마씩 조절할기 설정할수 있다.
                //Ex ) ChangeWidthValue = 8;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
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


        #region //..Command Method
        /// <summary>
        /// 매개변수 받을수 없는 동기 Command
        /// </summary>
        private void AddCommand()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Dut displayDut in ProbeCard.ProbeCardDevObjectRef.DutList)
                    {
                        displayDut.DutSizeLeft = (displayDut.MacIndex.XIndex * _DutWidth) + (_WinSizeX / 2) - _DutWidth;
                        displayDut.DutSizeTop = ((ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY - displayDut.MacIndex.YIndex) * _DutHeight) + (_WinSizeY / 2) - _DutHeight;
                        displayDut.DutSizeHeight = _DutHeight;
                        displayDut.DutSizeWidth = _DutWidth;
                        NC.TempDutList.Add(displayDut);
                    }
                });

                NC.TotalCount++;

                ParamChanged = true;
                SetStepSetupState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ClearCommand()
        {
            try
            {
                //this.ViewModelManager().Lock(this.GetHashCode(), "Lock", "Screen Locked");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    NC.TempDutList.Clear();

                    foreach (Dut displayDut in ProbeCard.ProbeCardDevObjectRef.DutList)
                    {
                        displayDut.DutSizeLeft = (displayDut.MacIndex.XIndex * _DutWidth) + (_WinSizeX / 2) - _DutWidth;
                        displayDut.DutSizeTop = ((ProbeCard.ProbeCardDevObjectRef.DutIndexSizeY - displayDut.MacIndex.YIndex) * _DutHeight) + (_WinSizeY / 2) - _DutHeight;
                        displayDut.DutSizeHeight = _DutHeight;
                        displayDut.DutSizeWidth = _DutWidth;
                        displayDut.DutVisibility = Visibility.Visible;
                        NC.TempDutList.Add(displayDut);
                    }

                    if (CalcCenterYOffset() < 0)
                    {
                        for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                        {
                            NC.TempDutList[i].DutSizeTop += _DiffY;
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                        {
                            NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                        }
                    }
                });

                NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Clear();

                //Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                //NextPos.Value = new NCCoordinate();
                //NextPos.Value.X.Value = 0;
                //NextPos.Value.Y.Value = 0;
                NCCoordinate pos = new NCCoordinate(0, 0);
                NextPos.Value = pos;

                NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                ParamChanged = true;
                SetStepSetupState();

                //this.ViewModelManager().UnLock(this.GetHashCode());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion

        #region ==> Calc Method        

        private double CalcLeftMaxValue()
        {
            try
            {
                if (NC.TempDutList.Count() > 0)
                {
                    _MaxX = NC.TempDutList[0].DutSizeLeft;
                    for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                    {
                        if (NC.TempDutList[i].DutSizeLeft > _MaxX)
                        {
                            _MaxX = NC.TempDutList[i].DutSizeLeft;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.CalcLeftMaxValue() : Error occurred.");
            }
            return _MaxX + _DutWidth;
        }

        private double CalcLeftMinValue()
        {
            try
            {
                if (NC.TempDutList.Count() > 0)
                {
                    _MinX = NC.TempDutList[0].DutSizeLeft;
                    for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                    {
                        if (NC.TempDutList[i].DutSizeLeft < _MinX)
                        {
                            _MinX = NC.TempDutList[i].DutSizeLeft;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.CalcLeftMinValue() : Error occurred.");
            }
            return _MinX;
        }

        private double CalcTopMaxValue()
        {
            try
            {
                if (NC.TempDutList.Count() > 0)
                {
                    _MaxY = NC.TempDutList[0].DutSizeTop;
                    for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                    {
                        if (NC.TempDutList[i].DutSizeTop > _MaxY)
                        {
                            _MaxY = NC.TempDutList[i].DutSizeTop;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.CalcTopMaxValue() : Error occurred.");
            }
            return _MaxY + _DutHeight;
        }

        private double CalcTopMinValue()
        {
            try
            {
                if (NC.TempDutList.Count() > 0)
                {
                    _MinY = NC.TempDutList[0].DutSizeTop;
                    for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                    {
                        if (NC.TempDutList[i].DutSizeTop < _MinY)
                        {
                            _MinY = NC.TempDutList[i].DutSizeTop;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.CalcTopMinValue() : Error occurred.");
            }
            return _MinY;
        }

        private double CalcCenterXOffset()
        {
            try
            {
                _CenterX = (CalcLeftMaxValue() - CalcLeftMinValue()) / 2 + CalcLeftMinValue();
                //_CenterX = ((CalcLeftMaxValue() - CalcLeftMinValue()) / 2) + CalcLeftMinValue();
                _DiffX = (_WinSizeX / 2) - _CenterX;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.CalcCenterXOffset() : Error occurred.");
            }
            return _DiffX;
        }

        private double CalcCenterYOffset()
        {
            try
            {
                _CenterY = (CalcTopMaxValue() - CalcTopMinValue()) / 2 + CalcTopMinValue();
                _DiffY = (_WinSizeY / 2) - _CenterY;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"{err.ToString()}.CalcCenterYOffset() : Error occurred.");
            }
            return _DiffY;
        }

        #endregion

        private void CreateDialog()
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    if (Dialog == null) Dialog = new UcNeedleCleanSequence(NeedleCleanDevParam);
            //});
            //ShowAdvanceSetupView();
        }

        #region ==>PadJogLeftCommand
        private async Task PadJogLeftCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();

                    //NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogLeftG.png", UriKind.Absolute));

                    //ViewPort = new Rect((CalcLeftMinValue() / 1000) - 1, (((CalcTopMaxValue() + CalcTopMinValue()) / 2 ) / 1000) - 2.25, 5,5);
                    //ViewPort = new Rect(0, 0,1,1);

                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.LEFT;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft - (User_Dist * _RatioX);
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop;

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeTop < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeLeft - (User_Dist * _RatioX) < 0)
                                    return;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterXOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft -= (_DiffX * _RatioX);
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft += _DiffX;
                            }
                        }

                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = User_Dist * -1;
                        NextPos.Value.Y.Value = 0;

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogLeftCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }
        }
        #endregion

        #region ==>PadJogRightCommand
        private async Task PadJogRightCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();

                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.RIGHT;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft + (User_Dist * _RatioX);
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop;
                            
                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeLeft + (User_Dist * _RatioX) + dut.DutSizeWidth - CalcLeftMinValue() > NC.SheetWidth)
                                    return;
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterXOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft += _DiffX;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft -= (_DiffX * _RatioX);
                            }
                        }

                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = User_Dist;
                        NextPos.Value.Y.Value = 0;

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogRightCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }

        }
        #endregion

        #region ==>PadJogUpCommand
        private async Task PadJogUpCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    //await this.MetroDialogManager().ShowWindow(Dialog);
                    await ShowAdvanceSetupView();

                    //NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogUpG.png", UriKind.Absolute));
                    //ViewPort = new Rect((((CalcLeftMaxValue() + CalcLeftMinValue()) / 2 ) / 1000) - 2.25, (CalcTopMaxValue() / 1000) - 1, 5, 5);
                    //ViewPort = new Rect(0, 0, 1, 1);
                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.TOP;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft;
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop - (User_Dist * _RatioY);

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeTop - (User_Dist * _RatioY) < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeLeft + dut.DutSizeWidth > NC.SheetWidth ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop - (User_Dist * _RatioY) + CalcTopMinValue() - dut.DutSizeHeight < 0)
                                    return;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterYOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop += _DiffY;
                            }
                        }

                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = 0;
                        NextPos.Value.Y.Value = User_Dist;

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogUpCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }

        }
        #endregion

        #region ==>PadJogDownCommand
        private async Task PadJogDownCommand()
        {
            try
            {

                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();

                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.BOTTOM;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft;
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop + (User_Dist * _RatioY);

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeTop + (User_Dist * _RatioY) + dut.DutSizeHeight - CalcTopMinValue() > NC.SheetHeight)
                                    return;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterYOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop += _DiffY;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                            }
                        }

                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = 0;
                        NextPos.Value.Y.Value = User_Dist * -1;

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogDownCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }

        }
        #endregion

        #region ==>PadJogLeftUpCommand
        private async Task PadJogLeftUpCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();
                    //NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogLeftUpG.png", UriKind.Absolute));
                    //ViewPort = new Rect((CalcLeftMinValue() / 1000) - 1.5, (CalcTopMaxValue() / 1000) - 1.5 , 5, 5);
                    //ViewPort = new Rect(0, 0, 1, 1);
                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.TOP_LEFT;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft - (User_Dist / 2 * Math.Sqrt(2) * _RatioX);
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop - (User_Dist / 2 * Math.Sqrt(2) * _RatioY);

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeTop < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeLeft - (User_Dist * _RatioX) < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop - (User_Dist * _RatioY) < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeLeft + dut.DutSizeWidth > NC.SheetWidth ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop - (User_Dist * _RatioY) + CalcTopMinValue() - dut.DutSizeHeight < 0)
                                    return;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterXOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft -= (_DiffX * _RatioX);
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft += _DiffX;
                            }
                        }

                        if (CalcCenterYOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop += _DiffY;
                            }
                        }

                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = User_Dist / Math.Sqrt(2) * -1;
                        NextPos.Value.Y.Value = User_Dist / Math.Sqrt(2);

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogLeftUpCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }

        }
        #endregion

        #region ==>PadJogRightUpCommand
        private async Task PadJogRightUpCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();

                    //NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogRightUpG.png", UriKind.Absolute));
                    //ViewPort = new Rect((CalcLeftMaxValue() / 1000) - 3.5, (CalcTopMaxValue() / 1000) - 1.5, 5, 5);
                    //ViewPort = new Rect(0, 0, 1, 1);
                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.TOP_RIGHT;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft + (User_Dist / 2 * Math.Sqrt(2) * _RatioX);
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop - (User_Dist / 2 * Math.Sqrt(2) * _RatioY);

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeLeft + (User_Dist * _RatioX) + dut.DutSizeWidth - CalcLeftMinValue() > NC.SheetWidth ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop - (User_Dist * _RatioY) < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeLeft + dut.DutSizeWidth > NC.SheetWidth ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop - (User_Dist * _RatioY) + CalcTopMinValue() - dut.DutSizeHeight < 0)
                                    return;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterXOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft += _DiffX;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft -= (_DiffX * _RatioX);
                            }
                        }

                        if (CalcCenterYOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop += _DiffY;
                            }
                        }

                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = User_Dist / Math.Sqrt(2);
                        NextPos.Value.Y.Value = User_Dist / Math.Sqrt(2);

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogRightUpCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }

        }
        #endregion

        #region ==>PadJogLeftDownCommand
        private async Task PadJogLeftDownCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();
                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.BOTTOM_LEFT;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft - (User_Dist / 2 * Math.Sqrt(2) * _RatioX);
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop + (User_Dist / 2 * Math.Sqrt(2) * _RatioY);

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeTop < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeLeft - (User_Dist * _RatioX) < 0 ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop + (User_Dist * _RatioY) + dut.DutSizeHeight - CalcTopMinValue() > NC.SheetHeight)
                                    return;
                            }


                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterXOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft -= (_DiffX * _RatioX);
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft += _DiffX;
                            }
                        }

                        if (CalcCenterYOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop += _DiffY;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                            }
                        }

                        // Left Down    
                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = User_Dist / Math.Sqrt(2) * -1;
                        NextPos.Value.Y.Value = User_Dist / Math.Sqrt(2) * -1;

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogLeftDownCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }
        }
        #endregion

        #region ==>PadJogRightDownCommand
        private async Task PadJogRightDownCommand()
        {
            try
            {
                if (NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value == NC_CleaningType.SINGLEDIR)
                {
                    await ShowAdvanceSetupView();

                    //NC.ImageSource = new System.Windows.Media.Imaging.BitmapImage(
                    //       new Uri("pack://application:,,,/ImageResourcePack;component/Images/JogRightDownG.png", UriKind.Absolute));
                    //ViewPort = new Rect((CalcLeftMaxValue() / 1000) - 3.5, (CalcTopMinValue() / 1000) - 3, 5, 5);
                    //ViewPort = new Rect(0, 0, 1, 1);
                    NeedleCleanDevParam = this.NeedleCleaner().NeedleCleanDeviceParameter_IParam as NeedleCleanDeviceParameter;
                    NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningCount.Value;
                    NC.Distance = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value;
                    NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value = NC_CleaningDirection.BOTTOM_RIGHT;
                    ShowArrow(NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDirection.Value);

                    ParamChanged = true;
                    SetStepSetupState();
                }
                else
                {
                    ObservableCollection<Dut> imsiDut = new ObservableCollection<Dut>();
                    if (NC.TempDutList.Count() > 0)
                    {
                        foreach (Dut item in NC.TempDutList)
                        {
                            imsiDut.Add(item);
                        }

                        for (int i = 1; i <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); i++)
                        {
                            Dut dut = new Dut();
                            dut.DutSizeHeight = NC.TempDutList[imsiDut.Count() - i].DutSizeHeight;
                            dut.DutSizeWidth = NC.TempDutList[imsiDut.Count() - i].DutSizeWidth;
                            dut.DutSizeLeft = NC.TempDutList[imsiDut.Count() - i].DutSizeLeft + (User_Dist / 2 * Math.Sqrt(2) * _RatioX);
                            dut.DutSizeTop = NC.TempDutList[imsiDut.Count() - i].DutSizeTop + (User_Dist / 2 * Math.Sqrt(2) * _RatioY);

                            for (int j = 1; j <= ProbeCard.ProbeCardDevObjectRef.DutList.Count(); j++)
                            {
                                if (NC.TempDutList[imsiDut.Count() - j].DutSizeLeft + (User_Dist * _RatioX) + dut.DutSizeWidth - CalcLeftMinValue() > NC.SheetWidth ||
                                    NC.TempDutList[imsiDut.Count() - j].DutSizeTop + (User_Dist * _RatioY) + dut.DutSizeHeight - CalcTopMinValue() > NC.SheetHeight)
                                    return;
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                NC.TempDutList.Add(dut);
                            });
                        }

                        if (CalcCenterXOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft += _DiffX;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeLeft -= (_DiffX * _RatioX);
                            }
                        }

                        if (CalcCenterYOffset() < 0)
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop += _DiffY;
                            }
                        }
                        else
                        {
                            for (int i = 0; i <= NC.TempDutList.Count() - 1; i++)
                            {
                                NC.TempDutList[i].DutSizeTop -= (_DiffY * _RatioY);
                            }
                        }

                        // Right Down
                        Element<NCCoordinate> NextPos = new Element<NCCoordinate>();
                        NextPos.Value = new NCCoordinate();
                        NextPos.Value.X.Value = User_Dist / Math.Sqrt(2);
                        NextPos.Value.Y.Value = User_Dist / Math.Sqrt(2) * -1;

                        NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Add(NextPos);
                        NC.TotalCount = NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].UserDefinedSeq.Count;

                        ParamChanged = true;
                        SetStepSetupState();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.ToString() + "PadJogRightDownCommand() : Error occurred.");
                LoggerManager.Error(err.ToString() + "Sheet Index : " + ((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index.ToString() +
                    "/Cleaning Type : " + NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningType.Value.ToString());
            }
        }
        #endregion

        #region ==>TextBoxClickCommand        
        private void TextBoxClickCommand()
        {
            try
            {
                string distance = null;

                distance = VirtualKeyboard.Show(distance, KB_TYPE.DECIMAL, 0, 100);

                if (distance != "")
                {
                    //tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                    PadJogSelect.Caption = distance;
                    User_Dist = Int32.Parse(distance);
                    //NeedleCleanDevParam.SheetDevs[((NeedleCleanObject)this.StageSupervisor().NCObject).NCSheetVMDef.Index].CleaningDistance.Value = Int32.Parse(distance);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void UpdateLabel()
        {
            throw new NotImplementedException();
        }

        public override void SetPackagableParams()
        {
            try
            {
                PackagableParams.Clear();

                PackagableParams.Add(SerializeManager.SerializeToByte(this.NeedleCleaner().NeedleCleanDeviceParameter_IParam));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        #endregion
    }
}
