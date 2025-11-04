using Autofac;
using CylinderManagerModule;
using CylType;
using HexagonJogControl;
using LogModule;
using MarkObjects;
using MetroDialogInterfaces;
using NotifyEventModule;
using ProbeCardObject;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Data;
using ProberInterfaces.Device;
using ProberInterfaces.Event;
using ProberInterfaces.LightJog;
using ProberInterfaces.NeedleClean;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign;
using ProberInterfaces.State;
using RelayCommandBase;
using SerializerUtil;
using StageMoveParameter;
using SubstrateObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SystemExceptions;
using SystemExceptions.InOutException;
using SystemExceptions.MotionException;
using SystemExceptions.VisionException;
using TouchSensorSystemParameter;

namespace StageModule
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class StageSupervisor : IStageSupervisor, INotifyPropertyChanged, IModule
    {
        public int AbsoluteIndex { get; internal set; }

        public bool Initialized { get; set; } = false;


        double IStageSupervisor.WaferINCH6Size => 0;

        double IStageSupervisor.WaferINCH8Size => 200000;

        double IStageSupervisor.WaferINCH12Size => 300000;


        private StageStateEnum _StageMoveState;

        public StageStateEnum StageMoveState
        {
            get { return _StageMoveState; }
            set { _StageMoveState = value; }
        }

        private StageStateEnum _StageMovePrevState;

        public StageStateEnum StageMovePrevState
        {
            get { return _StageMovePrevState; }
            set { _StageMovePrevState = value; }
        }

        private IStageMove _StageModuleState;

        public IStageMove StageModuleState
        {
            get { return _StageModuleState; }
            set { _StageModuleState = value; }
        }
        private StageMoveParam _MoveParam;

        public StageMoveParam MoveParam
        {
            get { return _MoveParam; }
            set { _MoveParam = value; }
        }

        //private int _SlotIndex;

        //public int SlotIndex
        //{
        //    get { return _SlotIndex; }
        //    set { _SlotIndex = value; }
        //}

        private GPCellModeEnum _StageMode;

        public GPCellModeEnum StageMode
        {
            get { return _StageMode; }
            set
            {
                if (value != _StageMode)
                {
                    _StageMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsModeChanging = false;
        public bool IsModeChanging
        {
            get { return _IsModeChanging; }
            set
            {
                if (value != _IsModeChanging)
                {
                    _IsModeChanging = value;
                    RaisePropertyChanged();
                }
            }
        }


        private StageLockMode _LockMode;

        public StageLockMode LockMode
        {
            get { return _LockMode; }
            set
            {
                _LockMode = value;
                RaisePropertyChanged();
            }
        }
        private TCW_Mode _TCWMode = TCW_Mode.OFF;

        private bool _IsRecoveryMode;
        //리커버리가 필요한 모드일때는 Loader쪽에서 Cell을 함부로 제어해선 안된다.
        public bool IsRecoveryMode
        {
            get { return _IsRecoveryMode; }
            set { _IsRecoveryMode = value; }
        }

        private StreamingModeEnum _StreamingMode = StreamingModeEnum.STREAMING_OFF;

        public StreamingModeEnum StreamingMode
        {
            get { return _StreamingMode; }
            set { _StreamingMode = value; }
        }

        public IVisionManager VisionManager { get; set; }
        public bool StageMoveFlag_Display { get; set; } = false;

        private event EventHandler _ChangedWaferObjectEvent;
        public event EventHandler ChangedWaferObjectEvent
        {
            add
            {
                if (_ChangedWaferObjectEvent == null || !_ChangedWaferObjectEvent.GetInvocationList().Contains(value))
                {
                    _ChangedWaferObjectEvent += value;
                }
            }
            remove
            {
                _ChangedWaferObjectEvent -= value;
            }
        }

        private event EventHandler _ChangedProbeCardObjectEvent;
        public event EventHandler ChangedProbeCardObjectEvent
        {
            add
            {
                if (_ChangedProbeCardObjectEvent == null || !_ChangedProbeCardObjectEvent.GetInvocationList().Contains(value))
                {
                    _ChangedProbeCardObjectEvent += value;
                }
            }
            remove
            {
                _ChangedProbeCardObjectEvent -= value;
            }
        }


        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        [OnDeserialized]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static object StageInitlock = new object();
        private CellInitModeEnum StageInitState { get; set; } = CellInitModeEnum.BeforeInit;
        public bool PatternRectViewChecked
        { get; set; }

        private string probeCardId { get; set; }

        #region ==> PnP UI에서 사용 할 Light Jog
        //==> Light Jog
        public LightJogViewModel PnpLightJog { get; set; }
        //==> Motion Jog
        public IHexagonJogViewModel PnpMotionJog { get; set; }
        #endregion

        private TouchSensorSysParameter _TouchSensorParam;
        public TouchSensorSysParameter TouchSensorParam
        {
            get { return _TouchSensorParam; }
            set
            {
                if (value != _TouchSensorParam)
                {
                    _TouchSensorParam = value;
                    RaisePropertyChanged();
                }
            }
        }
        private SlotInfo _SlotInformation;

        public SlotInfo SlotInformation
        {
            get { return _SlotInformation; }
            set { _SlotInformation = value; }
        }

        public IStageMoveLockStatus IStageMoveLockStatus { get; set; }

        private StageMoveLockStatus _StageMoveLockStatus;

        public StageMoveLockStatus StageMoveLockStatus
        {
            get { return _StageMoveLockStatus; }
            set { _StageMoveLockStatus = value; }
        }

        public IStageMoveLockParameter IStageMoveLockParam { get; set; }

        private StageMoveLockParameter _StageMoveLockParam;

        public StageMoveLockParameter StageMoveLockParam
        {
            get { return _StageMoveLockParam; }
            set { _StageMoveLockParam = value; }
        }

        public IStageSlotInformation GetSlotInfo()
        {
            return SlotInformation;
        }


        public ICylinderManager IStageCylinderManager { get; set; }

        private CylinderManager StageCylinderManager;
        private CylinderMappingParameter MakeCylinderParameter(string cylindername,
                                                        string ex_out_key,
                                                        string re_out_key,
                                                        List<string> ex_in_key_list,
                                                        List<string> re_in_key_list)
        {
            CylinderMappingParameter ret = new CylinderMappingParameter();

            try
            {
                ret.CylinderName = cylindername;
                ret.Extend_Output_Key = ex_out_key;
                ret.Retract_OutPut_Key = re_out_key;

                if (ret.Extend_Input_key_list == null)
                {
                    ret.Extend_Input_key_list = new List<string>();
                }

                ret.Extend_Input_key_list = ex_in_key_list;

                if (ret.Retract_Input_key_list == null)
                {
                    ret.Retract_Input_key_list = new List<string>();
                }

                ret.Retract_Input_key_list = re_in_key_list;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return ret;
        }
        private CylinderParams SetDefaultStageCylinderParam()
        {
            CylinderParams Params = new CylinderParams();

            try
            {
                List<string> Extand_In = new List<string>();
                List<string> Retract_In = new List<string>();


                Extand_In.Add(this.IOManager().IO.Inputs.DIWAFERCAMMIDDLE.Key.Value);
                Retract_In.Add(this.IOManager().IO.Inputs.DIWAFERCAMREAR.Key.Value);

                Params.CylinderMappingParameterList.Add
                    (
                        MakeCylinderParameter
                            (
                                "MoveWaferCam",
                                this.IOManager().IO.Outputs.DOWAFERMIDDLE.Key.Value,
                                this.IOManager().IO.Outputs.DOWAFERREAR.Key.Value,
                                Extand_In.ToList(),
                                Retract_In.ToList()
                            )
                    );
                Extand_In.Clear();
                Retract_In.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return Params;
        }

        public EventCodeEnum LoadCylinderManagerParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                List<IOPortDescripter<bool>> ioportdescriptors = new List<IOPortDescripter<bool>>();
                BindingFlags _BindFlags = BindingFlags.Public | BindingFlags.Instance;
                string FullPath = string.Empty;
                CylinderParams Params = new CylinderParams();

                StageCylinderManager = new CylinderManager();
                StageCylinderManager.Cylinders = new StageCylinderType();
                FullPath = this.FileManager().GetSystemParamFullPath(Params.FilePath, "StageCylinderIOParameter.json");
                PropertyInfo curPropInfo = null;

                try
                {
                    // Inputs
                    foreach (PropertyInfo propInfo in this.IOManager().IO.Inputs.GetType().GetProperties(_BindFlags))
                    {
                        curPropInfo = propInfo;
                        Object paramValue = propInfo.GetValue(this.IOManager().IO.Inputs);
                        if (paramValue == null)
                            continue;
                        if (ReflectionUtil.IsObjectAssignable(paramValue.GetType(), typeof(IOPortDescripter<bool>)))
                        {
                            ioportdescriptors.Add((IOPortDescripter<bool>)paramValue);
                        }
                    }
                }
                catch (Exception err)
                {
                    if (curPropInfo != null)
                    {
                        LoggerManager.Debug($"{curPropInfo}: Error occurred. Err = {err.Message}");
                    }
                    else
                    {
                        LoggerManager.Debug($"Error occurred. Err = {err.Message}");
                    }
                }

                // Outputs
                foreach (PropertyInfo propInfo in this.IOManager().IO.Outputs.GetType().GetProperties(_BindFlags))
                {
                    Object paramValue = propInfo.GetValue(this.IOManager().IO.Outputs);
                    if (paramValue == null)
                        continue;

                    if (ReflectionUtil.IsObjectAssignable(paramValue.GetType(), typeof(IOPortDescripter<bool>)))
                    {
                        ioportdescriptors.Add((IOPortDescripter<bool>)paramValue);
                    }
                }


                if (Directory.Exists(Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
                }

                if (File.Exists(FullPath) == false)
                {
                    Params = SetDefaultStageCylinderParam();
                    StageCylinderManager.LoadParameter(FullPath, ioportdescriptors, Params);
                }
                else
                {
                    StageCylinderManager.LoadParameter(FullPath, ioportdescriptors);
                }

                IStageCylinderManager = StageCylinderManager;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            RetVal = EventCodeEnum.NONE;
            return RetVal;
        }
        public EventCodeEnum SaveCylinderManagerParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                CylinderParams Params = new CylinderParams();
                string FullPath = this.FileManager().GetSystemParamFullPath(Params.FilePath, "StageCylinderIOParameter.json");
                StageCylinderManager.SaveParameter(FullPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveSlotInfo()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                string fullPath = SlotInformation.FilePath + SlotInformation.FileName;
                RetVal = Extensions_IParam.SaveParameter(null, SlotInformation, null, fullPath);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[SlotInformation] SaveSysParam(): Serialize Error");
                    return RetVal;
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }



        public EventCodeEnum LoadSlotInfo()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (SlotInformation == null)
                {
                    SlotInformation = new SlotInfo(this.LoaderController().GetChuckIndex());
                }

                IParam tmpParam = null;
                tmpParam = new SlotInfo();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(SlotInfo), null, SlotInformation.FilePath + SlotInformation.FileName);

                if (RetVal == EventCodeEnum.NONE)
                {
                    string filePath = SlotInformation.FilePath;
                    SlotInformation = tmpParam as SlotInfo;
                    SlotInformation.FilePath = filePath;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

        public EventCodeEnum SaveStageMoveLockStatusParam()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                string fullPath = StageMoveLockStatus.FilePath + StageMoveLockStatus.FileName;
                RetVal = Extensions_IParam.SaveParameter(null, StageMoveLockStatus, null, fullPath);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[SaveStageMoveLockStateParam] SaveSysParam(): Serialize Error");
                    return RetVal;
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveStageMoveLockParam()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                string fullPath = StageMoveLockParam.FilePath + StageMoveLockParam.FileName;
                RetVal = this.SaveParameter(StageMoveLockParam);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[SaveStageMoveLockParam] SaveSysParam(): Serialize Error");
                    return RetVal;
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }


        public EventCodeEnum LoadStageMoveLockParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam tmpParam = null;
                tmpParam = new StageMoveLockParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(StageMoveLockParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    StageMoveLockParam = tmpParam as StageMoveLockParameter;
                    IStageMoveLockParam = StageMoveLockParam;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public EventCodeEnum LoadStageMoveLockStatusParam()
        {

            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMoveLockStatus == null)
                {
                    StageMoveLockStatus = new StageMoveLockStatus(this.LoaderController().GetChuckIndex());
                }
                IParam tmpParam = null;
                tmpParam = new StageMoveLockStatus();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(StageMoveLockStatus), null, StageMoveLockStatus.FilePath + StageMoveLockStatus.FileName);

                if (RetVal == EventCodeEnum.NONE)
                {
                    string filePath = StageMoveLockStatus.FilePath;
                    StageMoveLockStatus = tmpParam as StageMoveLockStatus;
                    StageMoveLockStatus.FilePath = filePath;
                }
                IStageMoveLockStatus = StageMoveLockStatus;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public IWaferObject WaferObject
        {
            get
            {
                return Wafer as IWaferObject;
            }
            set
            {
                Wafer = value as WaferObject;
                RaisePropertyChanged();
            }
        }
        public WaferObject Wafer { get; set; } = new WaferObject();

        public OCRDevParameter OCRDevParam { get; set; } = new OCRDevParameter();

        private INeedleCleanObject _NCObject = new NeedleCleanObject();
        public INeedleCleanObject NCObject
        {
            get { return _NCObject; }
            set
            {
                _NCObject = value;
                RaisePropertyChanged();
            }
        }


        private ITouchSensorObject _TouchSensorObject
             = new TouchSensorObject.TouchSensorObject();

        public ITouchSensorObject TouchSensorObject
        {
            get { return _TouchSensorObject; }
            set { _TouchSensorObject = value; }
        }


        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = LoadOCRDevParam();
                RetVal = LoadWaferObject();
                RetVal = LoadProberCard();

                if (OCRDevParam.OCRAngle != Wafer.GetPhysInfo().OCRAngle.Value)
                {
                    Wafer.GetPhysInfo().OCRAngle.Value = OCRDevParam.OCRAngle;
                    //OCR앵글이 웨이퍼 맵에도 있고,ocr dev에도 있음으로 ocr dev를 따라 가야한다.
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum LoadProberCard()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = ProbeCardInfo.LoadDevParameter();
                if (RetVal == EventCodeEnum.NONE)
                {
                    ProbeCardConcreteParam = ProbeCardInfo as ProbeCard;

                    ProbeCardInfo = ProbeCardConcreteParam;
                    Extensions_IParam.CollectCommonElement(ProbeCardInfo, this.GetType().Name);
                }
                // GetStagePIV().ProbeCardID.Value = ProbeCardInfo.GetProbeCardID();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        public EventCodeEnum LoadWaferObject()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                var wafertype = this.WaferObject.GetWaferType();
                RetVal = Wafer.LoadDevParameter();
                Wafer.GetSubsInfo().WaferType = wafertype;

                if (RetVal == EventCodeEnum.NONE)
                {
                    Wafer.DispHorFlip = this.VisionManager().GetDispHorFlip();
                    Wafer.DispVerFlip = this.VisionManager().GetDispVerFlip();

                    CallWaferobjectChangedEvent();
                    Extensions_IParam.CollectCommonElement(Wafer, this.GetType().Name);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }
        public int GetWaferObjHashCode()
        {
            int HashCode = -1;
            try
            {
                HashCode = Wafer.WaferDevObject.GetHashCode();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return HashCode;
        }
        public EventCodeEnum LoadOCRDevParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new OCRDevParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(OCRDevParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    OCRDevParam = tmpParam as OCRDevParameter;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public void CallWaferobjectChangedEvent()
        {
            this._ChangedWaferObjectEvent?.Invoke(this, new WaferObjectEventArgs(WaferObject));
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = SaveWaferObject();
                RetVal = SaveProberCard();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SaveProberCard()
        {
            try
            {
                EventCodeEnum retVal = ProbeCardInfo.SaveDevParameter();
                retVal = ProbeCardInfo.SaveSysParameter();
                _ChangedProbeCardObjectEvent?.Invoke(this, new ProbeCardObjectEventArgs(ProbeCardInfo));
                this.StageSupervisor().ProbeCardInfo.ProbeCardChangedToggle.Value = false;
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum SaveWaferObject()
        {
            try
            {
                EventCodeEnum retVal = this.Wafer.SaveDevParameter();
                _ChangedWaferObjectEvent?.Invoke(this, new WaferObjectEventArgs(Wafer));
                Wafer.GetSubsInfo().WaferObjectChangedToggle.Value = false;
                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                ProbeCardInfo.ProbeCardDevObjectRef.TouchdownCount.Value = 0;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam tmpParam = null;
                tmpParam = new StageMoveParam();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                retVal = this.LoadParameter(ref tmpParam, typeof(StageMoveParam));

                if (retVal == EventCodeEnum.NONE)
                {
                    MoveParam = tmpParam as StageMoveParam;
                    LoadMoveDll();
                }

                retVal = NCObject.LoadSysParameter();

                if (MoveParam.UseOPUSVMove.Value == true)
                {
                    if (this.GetContainer().IsRegistered<IIOManager>() == true)
                    {
                        retVal = LoadCylinderManagerParam();

                        if (retVal != EventCodeEnum.NONE)
                        {
                            retVal = EventCodeEnum.IO_PARAM_ERROR;
                            return retVal;
                        }
                    }
                    else
                    {

                    }
                }

                retVal = ProbeCardInfo.LoadSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            RetVal = this.SaveParameter(MoveParam);

            RetVal = SaveCylinderManagerParam();

            SaveStageMoveLockParam();

            return RetVal;
        }

        private ObservableCollection<String> _setups;
        public ObservableCollection<String> setups
        {
            get { return _setups; }
            set
            {
                if (value != _setups)
                {
                    _setups = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMarkObject _MarkObject;

        public IMarkObject MarkObject
        {
            get { return _MarkObject; }
            set { _MarkObject = value; }
        }
        public MarkObject Mark { get; set; }
        public ProbeCard ProbeCardConcreteParam { get; set; }
        public IProbeCard ProbeCardInfo { get; set; } = new ProbeCard();
        public NeedleCleanObject NeedleCleanObj { get; set; }

        //ProberInterfaces.StageState IStageSupervisor.StageModuleState => throw new NotImplementedException();

        private ProbingInfo _ProbingProcessStatus;

        public ProbingInfo ProbingProcessStatus
        {
            get { return _ProbingProcessStatus; }
            set { _ProbingProcessStatus = value; }
        }

        private double _MoveTargetPosX;
        public double MoveTargetPosX
        {
            get { return _MoveTargetPosX; }
            set
            {
                if (value != _MoveTargetPosX)
                {
                    _MoveTargetPosX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MoveTargetPosY;
        public double MoveTargetPosY
        {
            get { return _MoveTargetPosY; }
            set
            {
                if (value != _MoveTargetPosY)
                {
                    _MoveTargetPosY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserCoordXPos;
        public double UserCoordXPos
        {
            get { return _UserCoordXPos; }
            set
            {
                if (value != _UserCoordXPos)
                {
                    _UserCoordXPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserCoordYPos;
        public double UserCoordYPos
        {
            get { return _UserCoordYPos; }
            set
            {
                if (value != _UserCoordYPos)
                {
                    _UserCoordYPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserCoordZPos;
        public double UserCoordZPos
        {
            get { return _UserCoordZPos; }
            set
            {
                if (value != _UserCoordZPos)
                {
                    _UserCoordZPos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserWaferIndexX;
        public double UserWaferIndexX
        {
            get { return _UserWaferIndexX; }
            set
            {
                if (value != _UserWaferIndexX)
                {
                    _UserWaferIndexX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _UserWaferIndexY;
        public double UserWaferIndexY
        {
            get { return _UserWaferIndexY; }
            set
            {
                if (value != _UserWaferIndexY)
                {
                    _UserWaferIndexY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinZClearance;
        public double PinZClearance
        {
            get { return _PinZClearance; }
            set
            {
                if (value != _PinZClearance)
                {
                    _PinZClearance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinMinRegRange;
        public double PinMinRegRange
        {
            get { return _PinMinRegRange; }
            set
            {
                if (value != _PinMinRegRange)
                {
                    _PinMinRegRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _WaferMaxThickness;
        public double WaferMaxThickness
        {
            get { return _WaferMaxThickness; }
            set
            {
                if (value != _WaferMaxThickness)
                {
                    _WaferMaxThickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _PinMaxRegRange;
        public double PinMaxRegRange
        {
            get { return _PinMaxRegRange; }
            set
            {
                if (value != _PinMaxRegRange)
                {
                    _PinMaxRegRange = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double _WaferRegRange;
        public double WaferRegRange
        {
            get { return _WaferRegRange; }
            set
            {
                if (value != _WaferRegRange)
                {
                    _WaferRegRange = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MoveToXvalue;
        public double MoveToXvalue
        {
            get { return _MoveToXvalue; }
            set
            {
                if (value != _MoveToXvalue)
                {
                    _MoveToXvalue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MoveToYvalue;
        public double MoveToYvalue
        {
            get { return _MoveToYvalue; }
            set
            {
                if (value != _MoveToYvalue)
                {
                    _MoveToYvalue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsRecipeDownloadEnable = true;

        public bool IsRecipeDownloadEnable
        {
            get { return _IsRecipeDownloadEnable; }
            set { _IsRecipeDownloadEnable = value; }
        }

        private string _WaitCancelDialogHashCode;

        public string WaitCancelDialogHashCode
        {
            get { return _WaitCancelDialogHashCode; }
            set { _WaitCancelDialogHashCode = value; }
        }


        private EventCodeEnum CheckEmergencyButton()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.IOManager().IO.Inputs.DIEMGSTOPSW.Value == true)
                {
                    ret = EventCodeEnum.MONITORING_EMERGENCY_BUTTON_ON;
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }

                LoggerManager.Debug($"Checked emergency{ret}");
            }
            catch (Exception err)
            {
                var errordata = ConvertToExceptionErrorCode(err);

                LoggerManager.Exception(err, $"Message:{errordata.Message}  ErrorCode:{errordata.ErrorCode}  ReturnValue:{errordata.ReturnValue}");
            }
            return ret;
        }

        private EventCodeEnum CheckMainAir()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            // <-- 251031 sebas Init 수정으로 인한 추가
            ret = EventCodeEnum.NONE;
            return ret;
            // -->

            try
            {
                if (this.IOManager().IO.Inputs.DIMAINAIR.Value == false)
                {
                    if (this.IOManager().IO.Inputs.DIMAINAIR.IOOveride.Value == EnumIOOverride.EMUL)
                    {
                        LoggerManager.Debug($"StageSupervisor, CheckMainAir() Affected by the Emul type of DIMAINAIR(IO).");

                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        ret = EventCodeEnum.MONITORING_MAIN_AIR_ERROR;
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }

                LoggerManager.Debug($"Checked main air {ret}");
            }
            catch (Exception err)
            {
                var errordata = ConvertToExceptionErrorCode(err);
                LoggerManager.Exception(err, $"Message:{errordata.Message}  ErrorCode:{errordata.ErrorCode}  ReturnValue:{errordata.ReturnValue}");
            }
            return ret;
        }

        private EventCodeEnum CheckMainVacuum()
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            // <-- 251031 sebas Init 수정으로 인한 추가
            ret = EventCodeEnum.NONE;
            return ret;
            // -->

            try
            {
                if (this.IOManager().IO.Inputs.DIMAINVAC.Value == false)
                {
                    if (this.IOManager().IO.Inputs.DIMAINVAC.IOOveride.Value == EnumIOOverride.EMUL)
                    {
                        LoggerManager.Debug($"StageSupervisor, CheckMainVacuum() Affected by the Emul type of DIMAINVAC(IO).");

                        ret = EventCodeEnum.NONE;
                    }
                    else
                    {
                        ret = EventCodeEnum.MONITORING_MAIN_VACUUM_ERROR;
                    }
                }
                else
                {
                    ret = EventCodeEnum.NONE;
                }

                LoggerManager.Debug($"Checked main vacuum {ret}");
            }
            catch (Exception err)
            {
                var errordata = ConvertToExceptionErrorCode(err);
                LoggerManager.Exception(err, $"Message:{errordata.Message}  ErrorCode:{errordata.ErrorCode}  ReturnValue:{errordata.ReturnValue}");
            }
            return ret;
        }

        protected EventCodeEnum ResultValidate(object funcname, EventCodeEnum retcode)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            ret = retcode;
            if (retcode != EventCodeEnum.NONE)
            {
                LoggerManager.Debug($"ResultValidate Fail :  Error code = {retcode.ToString()}, fucntion name = {funcname.ToString()}");
                throw new Exception($"FunctionName: {funcname.ToString()} Returncode: {retcode.ToString()} Error occurred");
            }

            return ret;
        }

        public EventHandler MachineInitEvent { get; set; }
        public EventHandler MachineInitEndEvent { get; set; }

        private EventCodeEnum CheckLastLockStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                TempList.Clear();
                if (StageMoveLockStatus.LastStageMoveLockState == StageLockMode.LOCK)
                {
                    if (StageMoveLockStatus.LastStageMoveLockReasonList.Count != 0)
                    {
                        for (int i = 0; i < StageMoveLockStatus.LastStageMoveLockReasonList.Count; i++)
                        {
                            if (StageMoveLockStatus.LastStageMoveLockReasonList[i] == ReasonOfStageMoveLock.STAGE_BACKSIDEDOOR_OPEN)
                            {
                                if (this.StageSupervisor().IStageMoveLockParam.DoorInterLockEnable.Value == true)
                                {
                                    var backsideIO = this.IOManager().IO.Inputs.DI_BACKSIDE_DOOR_OPEN;
                                    if (backsideIO.IOOveride.Value == EnumIOOverride.NONE)
                                    {
                                        bool value = false;
                                        var ioRetVal = this.IOManager().IOServ.ReadBit(backsideIO, out value);
                                        if (ioRetVal == IORet.NO_ERR && value == true)  //전장도어 열림
                                        {
                                            SetStageLock(StageMoveLockStatus.LastStageMoveLockReasonList[i]);
                                            LoggerManager.Debug($"LastStageMoveLockState: {StageMoveLockStatus.LastStageMoveLockState}, LastStageMoveLockReason:{StageMoveLockStatus.LastStageMoveLockReasonList[i]}");
                                        }
                                        else if (ioRetVal == IORet.NO_ERR && value == false) //전장도어 닫힘
                                        {
                                            TempList.Add(StageMoveLockStatus.LastStageMoveLockReasonList[i]);
                                            SetStageUnlock(StageMoveLockStatus.LastStageMoveLockReasonList[i]);
                                            StageMoveLockStatus.LastStageMoveLockReasonList.Insert(0, TempList[0]);
                                            LoggerManager.Debug($"LastStageMoveLockState: {StageMoveLockStatus.LastStageMoveLockState}, LastStageMoveLockReason:{StageMoveLockStatus.LastStageMoveLockReasonList[i]}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                SetStageLock(StageMoveLockStatus.LastStageMoveLockReasonList[i]);
                                LoggerManager.Debug($"LastStageMoveLockState: {StageMoveLockStatus.LastStageMoveLockState}, LastStageMoveLockReason:{StageMoveLockStatus.LastStageMoveLockReasonList[i]}");
                            }
                        }
                        StageMoveLockStatus.LastStageMoveLockReasonList.RemoveAll(TempList.Contains);
                        if (StageMoveLockStatus.LastStageMoveLockReasonList.Count == 0)
                        {
                            retVal = EventCodeEnum.NONE;    //이닛 가능
                        }
                        else
                        {
                            retVal = EventCodeEnum.STAGEMOVE_LOCK;  //남은 LockReason이 있어 이닛 불가능
                        }
                    }
                }
                else if (StageMoveLockStatus.LastStageMoveLockState == StageLockMode.UNLOCK) //마지막상태 UnLock, 남은 Reason없음
                {
                    LoggerManager.Debug($"LastStageMoveLockState: {StageMoveLockStatus.LastStageMoveLockState}");
                    retVal = EventCodeEnum.NONE;    //이닛 가능
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private async Task CheckTouchSensorSetup()
        {
            try
            {
                await Task.Run(() =>
                {

                    TouchSensorParam = TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;
                    TouchSensorParam.FlagTouchSensorBaseConfirmed = false;

                    if (TouchSensorParam != null)
                    {
                        if (TouchSensorParam.TouchSensorAttached.Value == true)
                        {
                            double diffX = 0.0;
                            double diffY = 0.0;
                            double diffZ = 0.0;

                            if (TouchSensorParam.TouchSensorBaseRegistered.Value == true)
                            {
                                //BasePos Check
                                EventCodeEnum base_result = this.TouchSensorBaseSetupModule().TouchSensorBaseSetupSystemInit(out diffX, out diffY, out diffZ); //Base Pos 보상
                                if (base_result == EventCodeEnum.NONE)
                                {
                                    if (TouchSensorParam.TouchSensorRegistered.Value == true)
                                    {
                                        EventCodeEnum tip_result = this.TouchSensorTipSetupModule().TouchSensorTipSetupSystemInit(diffX, diffY, diffZ); //Base Pos 보상된 만큼 
                                        if (tip_result == EventCodeEnum.NONE)
                                        {
                                        }
                                        else
                                        {
                                            TouchSensorParam.TouchSensorRegistered.Value = false;
                                            TouchSensorParam.TouchSensorOffsetRegistered.Value = false;
                                        }
                                    }
                                }
                                else
                                {
                                    TouchSensorParam.TouchSensorBaseRegistered.Value = false;
                                    TouchSensorParam.TouchSensorRegistered.Value = false;
                                    TouchSensorParam.TouchSensorOffsetRegistered.Value = false;
                                }

                            }

                            if (TouchSensorParam.TouchSensorPadBaseRegistered.Value == true)
                            {
                                //Trial Pos Check
                                EventCodeEnum trialpos_result = this.TouchSensorPadRefSetupModule().TouchSensorPadRefSetupSystemInit();
                                if (trialpos_result == EventCodeEnum.NONE)
                                {

                                }
                                else
                                {
                                    TouchSensorParam.TouchSensorPadBaseRegistered.Value = false;
                                    TouchSensorParam.TouchSensorOffsetRegistered.Value = false;
                                }
                            }

                            if (TouchSensorParam.TouchSensorOffsetRegistered.Value == true)
                            {
                                //Cal Offset + Time Save
                                EventCodeEnum offset_result = this.TouchSensorCalcOffsetModule().TouchSensorCalcOffsetSystemInit();
                                if (offset_result == EventCodeEnum.NONE)
                                {
                                    TouchSensorParam.FlagTouchSensorBaseConfirmed = true;
                                }
                                else
                                {
                                    TouchSensorParam.TouchSensorOffsetRegistered.Value = false;
                                }

                            }

                            //Extensions_IParam.SaveParameter(null, TouchSensorParam);

                            if (TouchSensorParam.FlagTouchSensorBaseConfirmed == false)
                            {
                                if (StageModuleState.LoaderController() != null)
                                {
                                    this.MetroDialogManager().ShowMessageDialog("Check Touch Sensor Setup", $"CELL{StageModuleState.LoaderController().GetChuckIndex()}, Please re-setting the touch sensor information", EnumMessageStyle.Affirmative);
                                }
                                LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Register_Touch_Sensor_Setup_UNDEFINED, description: $"CELL{StageModuleState.LoaderController().GetChuckIndex()} Touch Sensor Setup Info Error ");
                            }
                        }
                    }
                });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                TouchSensorObject.SaveSysParameter();
                StageModuleState.ZCLEARED();//-->IDLE
                StageCylinderType.MoveWaferCam.Retract(); //-->Wafer 브릿지 접기.
            }
        }
        public async Task<ErrorCodeResult> SystemInit()
        {
            ErrorCodeResult retVal = new ErrorCodeResult();
            ErrorCodeResult ret = new ErrorCodeResult();

            IEventManager EventManager = this.EventManager();

            try
            {
                SetStageInitState(CellInitModeEnum.BeforeInit);

                // 머신 이닛 후, 마크 얼라인과 보상이 진행될 수 있도록 마크 얼라인을 깨놓도록 하자.
                this.MarkObject.SetAlignState(AlignStateEnum.IDLE);
                this.ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                this.WaferObject.SetAlignState(AlignStateEnum.IDLE);
                if (Extensions_IParam.ProberRunMode != RunMode.EMUL && SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    retVal.ErrorCode = CheckEmergencyButton();
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal.ErrorCode);

                    retVal.ErrorCode = CheckMainAir();
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal.ErrorCode);

                    retVal.ErrorCode = CheckMainVacuum();
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal.ErrorCode);
                }
                else if (Extensions_IParam.ProberRunMode != RunMode.EMUL && SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    retVal.ErrorCode = this.LoaderController().GetLoaderEmergency();
                    ResultValidate(MethodBase.GetCurrentMethod(), retVal.ErrorCode);
                }

                //Purge Air false, Backup Data도 false
                //this.TempController().IsPurgeAirBackUpValue = false;
                //var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTERHEAD_PURGE, this.TempController().IsPurgeAirBackUpValue);

                //if (this.CardChangeModule().ModuleState.GetState() != ModuleStateEnum.ERROR
                //  && (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE ||
                //  this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED ||
                //  this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.SUSPENDED))

                // TODO : Check Concept
                // Wafer Camera가 펴져 있고, 카드가 위험한 위치에 존재하는 경우, Initialize를 진행하지 않고 에러를 띄우자.
                //bool CanInit = true;

                //bool WaferCamExtracted = false;

                //bool iowafercammiddle = false;
                //bool iowafercamrear = false;

                //bool IsCardInStage = false;
                //IORet ioret = IORet.ERROR;

                //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERCAMMIDDLE, out iowafercammiddle);

                //if (ioret != IORet.NO_ERR)
                //{
                //    CanInit = false;
                //}

                //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIWAFERCAMREAR, out iowafercamrear);

                //if (ioret != IORet.NO_ERR)
                //{
                //    CanInit = false;
                //}

                //if ((iowafercammiddle == false) && (iowafercamrear == true))
                //{
                //    // 접혀 있음
                //    WaferCamExtracted = false;
                //}
                //else if ((iowafercammiddle == true) && (iowafercamrear == false))
                //{
                //    // 펴져 있음
                //    WaferCamExtracted = true;
                //}
                //else
                //{
                //    // 무빙 중... 잘 모르겠으니 일단 안전을 위해 펴져 있는 것으로 간주한다.
                //    WaferCamExtracted = true;
                //}

                ////==> Check Card in stage. 현재는 Card Carrier가 올라와 있는지만 확인해보자.

                ////ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                ////if (ioret != IORet.NO_ERR)
                ////{
                ////    CanInit = false;
                ////}

                //bool diupmodule_left_sensor;
                //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    CanInit = false;
                //}

                //bool diupmodule_right_sensor;
                //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    CanInit = false;
                //}

                //if ((diupmodule_left_sensor == true) && (diupmodule_right_sensor == true))
                //{
                //    IsCardInStage = true;
                //}

                //// IO Check가 정상적으로 이루어지지 않았거나, (웨이퍼 카메라가 Middle에 존재하며 => 해당 조건 일단 생략), 카드가 Card carrier위에 존재할 때, 머신 이닛을 하면 안된다.
                ////if ( (CanInit != true) || ( (WaferCamExtracted == true) && (IsCardInStage == true) ) )
                //if ((CanInit != true) || (IsCardInStage == true))
                //{
                //    retVal = EventCodeEnum.SYSTEM_ERROR;

                //    return retVal;
                //}
                if (this.CardChangeModule().ModuleState.GetState() != ModuleStateEnum.ERROR)
                {
                    //EventManager.RaisingEvent(typeof(MachineInitOnEvent).FullName);
                    MachineInitEvent?.Invoke(this, new EventArgs());

                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "System Initializing");

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        retVal.ErrorCode = this.FoupOpModule().FoupInitState();
                        ResultValidate(MethodBase.GetCurrentMethod(), retVal.ErrorCode);
                        this.FoupOpModule().InitProcedures();
                        ResultValidate(MethodBase.GetCurrentMethod(), retVal.ErrorCode);

                        //LoggerManager.Debug($"[S] Prober System Initialize");
                        //var cdx = this.GetGPLoader().Connect();

                        //this.MotionManager().StageEMGStop();
                        //this.MotionManager().LoaderEMGStop();
                        this.FoupOpModule().InitModule();

                        // <-- 251031 sebas Init 수정으로 인한 제외
                        //var loaderInitTask = Task.Run(() =>
                        //{
                        //    // this.LoaderController().InitModule();

                        //    LoggerManager.Debug($"LoaderSystemInit() Start");
                        //    retVal.ErrorCode = this.LoaderController().LoaderSystemInit();
                        //    LoggerManager.Debug($"LoaderSystemInit() End, Result = {retVal.ErrorCode}");

                        //    return retVal;
                        //});

                        // retVal = await loaderInitTask;
                        // -->
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }

                    //LastMoveLockState 체크 머신이닛 할지, 안할지
                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = CheckLastLockStatus();
                    }

                    //retVal = EventCodeEnum.NONE;
                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        var stageInitTask = Task.Run(() =>
                        {
                            LoggerManager.Debug($"StageSystemInit() Start");
                            retVal.ErrorCode = this.StageModuleState.StageSystemInit();
                            LoggerManager.Debug($"StageSystemInit() End, Result = {retVal.ErrorCode}");
                            return retVal;
                        });

                        retVal = await stageInitTask;
                    }
                    else
                    {
                        //retVal.ErrorCode = EventCodeEnum.MONITORING_MACHINE_INIT_ERROR;
                        //this.NotifyManager().Notify(EventCodeEnum.MONITORING_MACHINE_INIT_ERROR);

                        this.NotifyManager().Notify(retVal.ErrorCode);

                        return retVal;
                    }

                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        var IORet = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOZUPLAMPON, true);
                        if (IORet == IORet.NO_ERR)
                        {
                            LoggerManager.Debug($"PDS Activated - Chuck discharging...");
                        }

                        retVal.ErrorCode = SetWaferObjectStatus();
                    }
                    else
                    {
                        //LoggerManager.Debug($"Initialize Error = {retVal}");
                        LoggerManager.Debug($"Prober System Initialize ERROR, Code = {retVal}");
                        //this.NotifyManager().Notify(EventCodeEnum.MONITORING_MACHINE_INIT_ERROR);


                        this.NotifyManager().Notify(retVal.ErrorCode);

                        return retVal;
                    }

                    bool freeColdMode = false; //프리콜드 모드일때는 로더도어 무조건 열고, 아니면 닫는다.

                    if (SystemManager.SysteMode == SystemModeEnum.Single)
                    {
                        if (retVal.ErrorCode == EventCodeEnum.NONE)
                        {
                            if (freeColdMode)
                            {
                                await Task.Run(() =>
                                {
                                    retVal.ErrorCode = this.StageModuleState.LoaderDoorOpen();
                                });
                            }
                            else
                            {
                                await Task.Run(() =>
                                {
                                    retVal.ErrorCode = this.StageModuleState.LoaderDoorClose();
                                });
                            }
                        }
                    }
                    // Module Stat init
                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        foreach (var Sequence in this.SequenceEngineManager().Services)
                        {
                            if (Sequence is ILotOPModule)
                            {
                                foreach (IStateModule module in (Sequence as ILotOPModule).RunList)
                                {
                                    module.CommandRecvSlot.ClearToken();

                                    if (module.CommandRecvProcSlot != null)
                                    {
                                        module.CommandRecvProcSlot.ClearToken();
                                    }

                                    retVal.ErrorCode = module.ClearState();

                                    module.ReasonOfError.Confirmed();

                                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Debug($"[StageSupervisor] SystemInit() : ClearState is faild. Module = {module.GetType().ToString()}, Return value = {retVal.ErrorCode}");

                                        break;
                                    }
                                }
                                (Sequence as IStateModule).CommandRecvSlot.ClearToken();

                                if ((Sequence as IStateModule).CommandRecvProcSlot != null)
                                {
                                    (Sequence as IStateModule).CommandRecvProcSlot.ClearToken();
                                }

                                retVal.ErrorCode = (Sequence as IStateModule).ClearState();

                            }
                            else if (Sequence is ILoaderOPModule)
                            {
                                foreach (IStateModule module in (Sequence as ILoaderOPModule).RunList)
                                {
                                    retVal.ErrorCode = module.ClearState();
                                }
                                retVal.ErrorCode = (Sequence as IStateModule).ClearState();
                            }
                            else if (Sequence is IStateModule)
                            {
                                retVal.ErrorCode = (Sequence as IStateModule).ClearState();
                            }

                            if (retVal.ErrorCode != EventCodeEnum.NONE)
                            {
                                break;
                            }
                        }
                    }

                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        this.SoakingModule().PreHeatEvent?.Invoke(this, new EventArgs());
                        if (SystemManager.SysteMode == SystemModeEnum.Single)
                        {
                            retVal.ErrorCode = await Task<EventCodeEnum>.Run(() =>
                            {
                                return StageModuleState.LoaderHomeOffsetMove();
                            });
                        }
                        LoggerManager.Debug($"LoaderHomeMove End");

                        retVal.ErrorCode = await Task<EventCodeEnum>.Run(() =>
                        {
                            return StageModuleState.StageHomeOffsetMove();
                        });
                        LoggerManager.Debug($"StageHomeMove End");

                        if (this.StageModuleState.CardChangeModule().GetCCType() == ProberInterfaces.CardChange.EnumCardChangeType.CARRIER)
                        {
                            if (retVal.ErrorCode != EventCodeEnum.NONE)
                            {
                                // StageHomeOffsetMove 실패했을 경우에 여기를 탐.
                                LoggerManager.Prolog(PrologType.INFORMATION, retVal.ErrorCode, description: $"CELL{StageModuleState.LoaderController().GetChuckIndex()} Card Holder Check Skipped.");
                            }
                            else
                            {
                                retVal.ErrorCode = await Task<EventCodeEnum>.Run(() =>
                                {
                                    var prev = (this.StageModuleState.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist;

                                    if (((this.StageModuleState.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).MoveToCardHolderPosEnable) == true)
                                    {
                                        retVal.ErrorCode = StageModuleState.ZCLEARED();//-->IDLE
                                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                                        {
                                            return Task.FromResult(retVal.ErrorCode);
                                        }
                                        retVal.ErrorCode = StageModuleState.CCZCLEARED();//-->CC State
                                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                                        {
                                            return Task.FromResult<EventCodeEnum>(retVal.ErrorCode);
                                        }

                                        retVal.ErrorCode = StageModuleState.MoveToCardHolderPositionAndCheck();
                                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                                        {
                                            return Task.FromResult<EventCodeEnum>(retVal.ErrorCode);
                                        }
                                    }
                                    else
                                    {
                                        if (StageModuleState.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE.IOOveride.Value == EnumIOOverride.NONE)
                                        {
                                            var hntret = StageModuleState.IOManager().IOServ.MonitorForIO(StageModuleState.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, true, 100, 15000);
                                            if (hntret != 0)
                                            {
                                                hntret = StageModuleState.IOManager().IOServ.MonitorForIO(StageModuleState.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, false, 100, 15000);
                                                if (hntret != 0)
                                                {
                                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIHOLDER_ON_TOPPLATE in  {this.GetType()}");
                                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                                    return Task.FromResult<EventCodeEnum>(retVal.ErrorCode);
                                                }
                                                StageModuleState.CardChangeModule().SetIsCardExist(false);

                                            }
                                            StageModuleState.CardChangeModule().SetIsCardExist(true);
                                        }
                                    }


                                    if ((StageModuleState.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist != prev)
                                    {
                                        this.MetroDialogManager().ShowMessageDialog("[Information]", $"CELL{StageModuleState.LoaderController().GetChuckIndex()} Card State Changed. Card Exist {prev} to {(StageModuleState.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist} ", EnumMessageStyle.AffirmativeAndNegative);
                                        LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.GP_CardChange_CARD_STATE_CHANGED, description: $"CELL{StageModuleState.LoaderController().GetChuckIndex()} Car State Changed. Card Exist {prev} to {(StageModuleState.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).IsCardExist} ");
                                        StageModuleState.CardChangeModule().SaveSysParameter();
                                    }

                                    StageModuleState.ZCLEARED();//-->IDLE
                                    return Task.FromResult(retVal.ErrorCode);
                                });

                                if (retVal.ErrorCode != EventCodeEnum.NONE)
                                {
                                    await this.MetroDialogManager().ShowMessageDialog("[Information]", $"CELL{StageModuleState.LoaderController().GetChuckIndex()} Check card holder failed. " +
                                        $"MoveToCardHolderPosEnable: {((this.StageModuleState.CardChangeModule().CcSysParams_IParam as ProberInterfaces.CardChange.ICardChangeSysParam).MoveToCardHolderPosEnable)}, " +
                                        $"ErrorCode:{retVal.ErrorCode}", EnumMessageStyle.AffirmativeAndNegative);
                                }

                            }

                        }

                        //Touch Sensor Setup Info Check.
                        if (retVal.ErrorCode == EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"Touch Sensor Param Check Start");
                            await CheckTouchSensorSetup();
                        }
                        //LoggerManager.Debug($"[End] Prober System Initialize");
                        //LoggerManager.Debug($"[{DateTime.Now.ToLocalTime()}] : Prober System Initialize END");

                        MachineInitEndEvent?.Invoke(this, new EventArgs());

                        SetStageInitState(CellInitModeEnum.NormalEnd);

                        if (SystemManager.SysteMode != SystemModeEnum.Multiple)
                        {
                            this.LampManager().RequestWarningLamp();
                            this.LampManager().ClearRequestLamp();
                        }
                        //this.GetStagePIV().SetStageState(GEMStageStateEnum.ONLINE);
                        //SetStageMode(GPCellModeEnum.ONLINE);


                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Prolog(PrologType.INFORMATION, retVal.ErrorCode);
                            this.NotifyManager().Notify(EventCodeEnum.MONITORING_MACHINE_INIT_ERROR);
                        }
                    }
                    else
                    {
                        LoggerManager.Prolog(PrologType.INFORMATION, retVal.ErrorCode);
                        this.NotifyManager().Notify(EventCodeEnum.MONITORING_MACHINE_INIT_ERROR);
                        // failed machine init
                    }


                    //LoggerManager.Debug($"Prober System Initialize END");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.MONITORING_MACHINE_INIT_ERROR;
                    this.NotifyManager().Notify(EventCodeEnum.MONITORING_MACHINE_INIT_ERROR);
                    //LoggerManager.Debug($"Card Chnage Module ERROR...");
                    LoggerManager.Debug($"LOTModule or CardChanger Module ERROR, Code = {retVal}");
                }

                if (retVal.ErrorCode == EventCodeEnum.NONE)
                {
                    //if (StageIsLotReady())
                    //    GetStagePIV().SetStageState(GEMStageStateEnum.OFFLINE);
                    //else
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.ONLINE);
                    if (this.LoaderController().GetconnectFlag())
                    {
                        this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.AVAILABLE);
                    }
                }
            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.MONITORING_MACHINE_INIT_ERROR);

                var errordata = ConvertToExceptionErrorCode(err);
                LoggerManager.Exception(err, $"Message:{errordata.Message}  ErrorCode:{errordata.ErrorCode}  ReturnValue:{errordata.ReturnValue}");
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.OFFLINE);
                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNAVAILABLE);
            }
            finally
            {
                //머신이닛 이후 SystemMoveLockDefault 체크 이후 Lock을 할지,
                //다른 Lock Reason을 넣어줄 경우에는 UnLock할 방법이 없으니 MANUAL동작으로 Unlock시켜 풀어줄 수 있게 하자.
                if (GetStageInitState() != CellInitModeEnum.NormalEnd)
                {
                    this.LotOPModule().SetErrorState();
                    SetStageInitState(CellInitModeEnum.ErrorEnd);
                }

                if (StageMoveLockParam.LockAfterInit.Value == true)
                {
                    SetStageLock(ReasonOfStageMoveLock.MANUAL);
                    LoggerManager.Debug($"SystemMoveLockDefault is LOCK, Reason: {ReasonOfStageMoveLock.MANUAL}");
                }
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
            //OnChangedWaferObject(this, new WaferObjectEventArgs(WaferObject));
            return retVal;
        }

        public async Task<EventCodeEnum> LoaderInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            IEventManager EventManager = this.EventManager();
            try
            {
                EventManager.RaisingEvent(typeof(MachineInitOnEvent).FullName);

                await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Loader Initialize");

                //this.MotionManager().LoaderEMGStop();
                retVal = this.FoupOpModule().FoupInitState();
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);
                this.FoupOpModule().InitProcedures();
                ResultValidate(MethodBase.GetCurrentMethod(), retVal);

                LoggerManager.Debug($"[Start] Prober System Initialize");
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.IDLE ||
                    this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    var loaderInitTask = Task.Run(() =>
                    {
                        // this.LoaderController().InitModule();
                        retVal = this.LoaderController().LoaderSystemInit();

                        return retVal;
                    });

                    retVal = await loaderInitTask;

                    if (retVal == EventCodeEnum.NONE)
                    {
                        bool freeColdMode = false; //프리콜드 모드일때는 로더도어 무조건 열고, 아니면 닫는다.
                        if (freeColdMode)
                        {
                            retVal = this.StageModuleState.LoaderDoorOpen();
                        }
                        else
                        {
                            retVal = this.StageModuleState.LoaderDoorClose();
                        }

                        // Module Stat init

                        foreach (var Sequence in this.SequenceEngineManager().Services)
                        {
                            if (Sequence is ILoaderOPModule)
                            {
                                foreach (IStateModule module in (Sequence as ILoaderOPModule).RunList)
                                {
                                    retVal = module.ClearState();
                                }
                                (Sequence as IStateModule).ClearState();
                            }
                        }

                        if (retVal == EventCodeEnum.NONE)
                        {
                            MachineInitEndEvent?.Invoke(this, new EventArgs());

                            //EventManager.RaisingEvent(typeof(MachineInitCompletedEvent).FullName);

                            if (SystemManager.SysteMode == SystemModeEnum.Single)
                            {
                                retVal = await Task<EventCodeEnum>.Run(() =>
                                {
                                    return StageModuleState.LoaderHomeOffsetMove();
                                });
                                LoggerManager.Debug($"LoaderHomeMove End");
                            }
                        }
                        else
                        {
                            // failed machine init
                        }
                    }
                    else
                    {

                    }

                }
                else
                {
                    retVal = EventCodeEnum.UNDEFINED;
                    //LoggerManager.Debug($"Card Chnage Module ERROR...");
                    LoggerManager.Debug($"Lot State Module ERROR, Code = {retVal}");
                }

            }
            catch (Exception err)
            {
                var errordata = ConvertToExceptionErrorCode(err);
                LoggerManager.Exception(err, $"Message:{errordata.Message}  ErrorCode:{errordata.ErrorCode}  ReturnValue:{errordata.ReturnValue}");
            }
            finally
            {
                await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }

            //OnChangedWaferObject(this, new WaferObjectEventArgs(WaferObject));

            return retVal;
        }
        public StageSupervisor()
        {

        }

        public void StageSupervisorStateTransition(StageState state)
        {
            try
            {
                StageMovePrevState = StageMoveState;
                StageMoveState = state.GetState();

                LoggerManager.Debug($"[{this.GetType().Name}], StageSupervisorStateTransition() : Change State : [{StageMovePrevState}] ==> [{StageMoveState}]");

                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    StageMoveInfo info = new StageMoveInfo();

                    info.ChuckIndex = this.LoaderController().GetChuckIndex();
                    info.StageMove = state.GetState().ToString();

                    this.LoaderController()?.UpdateStageMove(info);
                }

                this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.STAGEMOVESTATE, StageMoveState.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public int InitStageSupervisor()
        {
            try
            {
                int retVal = -1;

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public CatCoordinates ReadRelPos()
        {
            CatCoordinates RelPos = new CatCoordinates();

            try
            {
                double XPos = 0;
                double YPos = 0;
                double ZPos = 0;
                double CPos = 0;

                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref XPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref YPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref ZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref CPos);

                RelPos.X.Value = XPos;
                RelPos.Y.Value = YPos;
                RelPos.Z.Value = ZPos;
                RelPos.T.Value = CPos;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RelPos;
        }

        public EventCodeEnum LoadMoveDll()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            DllImporter.DllImporter dllimporter = new DllImporter.DllImporter();
            try
            {
                ModuleInfo moduleinfo;
                // 신규 장비 추가(EnumLoaderMovingMethodType 추가 등)시 구분해서 처리해 주어야 함
                moduleinfo = new ModuleInfo(new ModuleDllInfo(MoveParam.OPUSVMoveDLLName.Value, MoveParam.OPUSVClassName.Value, 1000, true));

                Tuple<bool, Assembly> ret = dllimporter.LoadDLL(moduleinfo.DllInfo);

                IStageMove module = dllimporter.Assignable<IStageMove>(ret.Item2).FirstOrDefault();

                retVal = module.InitModule();

                if (retVal != EventCodeEnum.NONE)
                {
                    return EventCodeEnum.PARAM_ERROR;
                }

                StageModuleState = module;

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[StageSupervisor] LoadMoveDll(): Error occurred while loading Dll. Err = {0}", err.Message));
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.ONLINE);

                    StageMoveState = StageStateEnum.IDLE;
                    PinZClearance = -500;

                    PinMinRegRange = this.CoordinateManager().StageCoord.PinReg.PinRegMin.Value;
                    PinMaxRegRange = this.CoordinateManager().StageCoord.PinReg.PinRegMax.Value;

                    retval = LoadTouchSensorObject();

                    if (retval == EventCodeEnum.NONE)
                    {
                        TouchSensorParam = TouchSensorObject.TouchSensorParam_IParam as TouchSensorSysParameter;
                    }

                    WaferMaxThickness = 1500;
                    WaferRegRange = -3000;

                    PnpLightJog = new LightJogViewModel(
                        maxLightValue: 255,
                        minLightValue: 0);

                    PnpMotionJog = new HexagonJogViewModel();

                    try
                    {
                        LoadSlotInfo();

                        retval = SetWaferObjectStatus();
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Debug($"SetWaferObjectStatus(): Exception occurred. Err = {err.Message}");
                    }

                    StageMoveLockStatus = new StageMoveLockStatus(this.LoaderController().GetChuckIndex());

                    LoadStageMoveLockStatusParam();
                    LoadStageMoveLockParam();

                    retval = NCObject.Init();

                    VisionManager = this.VisionManager();

                    #region //..MarkObject
                    Mark = new MarkObject();
                    MarkObject = Mark;
                    Mark.SetAlignState(AlignStateEnum.IDLE);

                    Extensions_IParam.CollectCommonElement(MarkObject, this.GetType().Name);
                    #endregion

                    MachineInitEvent?.Invoke(this, new EventArgs());

                    if (CylType.StageCylinderType.MoveWaferCam != null)
                    {
                        Func<bool> examFunc = () =>
                        {
                            bool ret = false;

                            if (this.CardChangeModule().GetCCType() != ProberInterfaces.CardChange.EnumCardChangeType.DIRECT_CARD)
                            {
                                if (StageModuleState.GetState() == StageStateEnum.CARDCHANGE)
                                {
                                    ret = true;
                                }
                                else
                                {
                                    ret = false;
                                }
                            }

                            return ret;
                        };

                        CylType.StageCylinderType.MoveWaferCam.SetExtInterLock(examFunc);
                    }

                    StageModeChange(GPCellModeEnum.ONLINE);

                    probeCardId = this.CardChangeModule().GetProbeCardID();

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
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum CheckWaferStatus(bool isExist)
        {
            EventCodeEnum RetVal;

            try
            {
                RetVal = StageModuleState.CheckWaferStatus(isExist);

                //Emul에서는 io가 default true이므로 waferObject Type을 추가로 확인
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL && isExist)
                {
                    var waferType = WaferObject.GetWaferType();
                    if (waferType == EnumWaferType.UNDEFINED)
                    {
                        RetVal = EventCodeEnum.UNDEFINED;
                    }
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                //LoggerManager.Error($ex);
                LoggerManager.Exception(err);

            }
            return RetVal;
        }

        public void UpdateWaferStatusAndState()
        {
            try
            {
                var wafertype = this.WaferObject.GetWaferType();

                if (SlotInformation != null && SlotInformation.WaferStatus == EnumSubsStatus.EXIST)
                {
                    // Check : Backup 정보를 항상 사용하는 것이 맞는가?
                    // Backup 정보가 제대로 갱신되지 않는 경우가 없는가?

                    this.WaferObject.SetWaferStatus(EnumSubsStatus.EXIST, SlotInformation.WaferType, SlotInformation.WaferID, SlotInformation.OriginSlotIndex);
                    this.LoaderController().SetTransferWaferSize((EnumWaferSize)SlotInformation.WaferSize);

                    if (SlotInformation.WaferState == EnumWaferState.PROCESSED)
                    {
                        this.WaferObject.SetWaferState(EnumWaferState.PROCESSED);
                    }
                    else if (SlotInformation.WaferState == EnumWaferState.PROBING)
                    {
                        this.WaferObject.SetWaferState(EnumWaferState.PROBING);
                    }
                    else if (SlotInformation.WaferState == EnumWaferState.TESTED)
                    {
                        this.WaferObject.SetWaferState(EnumWaferState.TESTED);
                    }
                    else if (SlotInformation.WaferState == EnumWaferState.MISSED)
                    {
                        this.WaferObject.SetWaferState(EnumWaferState.SKIPPED);
                    }
                    else
                    {
                        this.WaferObject.SetWaferState(EnumWaferState.UNPROCESSED);
                    }
                }
                else
                {
                    this.WaferObject.SetWaferStatus(EnumSubsStatus.EXIST, wafertype, "", 0);
                    this.WaferObject.SetWaferState(EnumWaferState.UNPROCESSED);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private EventCodeEnum CheckVacuumStatus()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if ((StageModuleState.VacuumOnOff(false, extraVacReady: false) == EventCodeEnum.NONE) && (StageModuleState.WaitForVacuum(true) == EventCodeEnum.NONE))
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.Chuck_Vacuum_Error;
                }
            }
            catch (Exception err)
            {
                this.WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);

                retval = EventCodeEnum.Chuck_Vacuum_Error;
                this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
            }

            return retval;
        }
        public EventCodeEnum SetWaferObjectStatus()
        {
            EventCodeEnum retval = StageModuleState.VacuumOnOff(true, extraVacReady: true);

            EventCodeEnum waferNotExistRetVal;
            EventCodeEnum waferExistRetVal;

            // TRUE => NONE (NOT_EXIST)
            // FALSE => NONE (EXIST)

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    if (this.SlotInformation.WaferStatus == EnumSubsStatus.EXIST)
                    {
                        waferNotExistRetVal = EventCodeEnum.UNDEFINED;
                    }
                    else
                    {
                        waferNotExistRetVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    waferNotExistRetVal = StageModuleState.MonitorForVacuum(true, 100, 3000);
                }
            }
            catch (Exception err)
            {
                waferNotExistRetVal = EventCodeEnum.Chuck_Vacuum_Error;
                retval = EventCodeEnum.Chuck_Vacuum_Error;
                this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);

                LoggerManager.Exception(err);
            }

            // VAC = NOT_EXIST
            if (waferNotExistRetVal == EventCodeEnum.NONE)
            {
                //// EMUL
                //if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                //{
                //    this.WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                //    retval = CheckVacuumStatus();
                //}
                //else
                //{
                // Normal case) BACKUP = NOT_EXIST
                if (this.SlotInformation.WaferStatus == EnumSubsStatus.NOT_EXIST)
                {
                    this.WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                    retval = CheckVacuumStatus();
                }
                // Abnormal case) BACKUP = !NOT_EXIST
                else
                {
                    if (CheckUsingHandler() == true && StageModuleState.IsHandlerholdWafer())
                    {
                        UpdateWaferStatusAndState();

                        retval = CheckVacuumStatus();

                        // 의도하여 변경
                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        this.WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                        retval = CheckVacuumStatus();

                        // 의도하여 변경
                        retval = EventCodeEnum.Wafer_Missing_Error;
                        this.NotifyManager().Notify(EventCodeEnum.Wafer_Missing_Error);
                    }
                }
                //}
            }
            else
            {// EXIST
                try
                {
                    if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                    {
                        if (this.SlotInformation.WaferStatus == EnumSubsStatus.EXIST)
                        {
                            waferExistRetVal = EventCodeEnum.NONE;
                        }
                        else
                        {
                            waferExistRetVal = EventCodeEnum.UNDEFINED;
                        }
                    }
                    else
                    {
                        waferExistRetVal = StageModuleState.MonitorForVacuum(false, 100, 3000);
                        if ((this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_0.IOOveride.Value == EnumIOOverride.NONE) ||
                           (this.IOManager().IO.Outputs.DOCHUCK_EXTRA_AIRON_2.IOOveride.Value == EnumIOOverride.NONE))
                        {
                            LoggerManager.Debug($"[StageSupervisor] SetWaferObjectStatus(): Check Wafer Exist Before Chuck Vac Off : {waferExistRetVal} ");
                            StageModuleState.ChuckMainVacOff();
                            waferExistRetVal = StageModuleState.MonitorForVacuum(false, 100, 3000);
                            LoggerManager.Debug($"[StageSupervisor] SetWaferObjectStatus(): Check Wafer Exist After Chuck Vac Off  : {waferExistRetVal} ");
                        }
                    }
                }
                catch (Exception err)
                {
                    this.NotifyManager().Notify(EventCodeEnum.Chuck_Vacuum_Error);
                    waferExistRetVal = EventCodeEnum.UNDEFINED;
                    retval = EventCodeEnum.Chuck_Vacuum_Error;

                    LoggerManager.Exception(err);
                }

                // VAC = EXIST
                if (waferExistRetVal == EventCodeEnum.NONE)
                {
                    UpdateWaferStatusAndState();
                }
                else
                {
                    if (CheckUsingHandler() == true && StageModuleState.IsHandlerholdWafer())
                    {
                        UpdateWaferStatusAndState();

                        retval = CheckVacuumStatus();

                        // 의도하여 변경
                        retval = EventCodeEnum.NONE;
                    }
                    else
                    {
                        this.WaferObject.SetWaferStatus(EnumSubsStatus.UNKNOWN);

                        retval = CheckVacuumStatus();

                        // 의도하여 변경
                        retval = EventCodeEnum.Wafer_Missing_Error;
                        this.NotifyManager().Notify(EventCodeEnum.Wafer_Missing_Error);
                    }
                }
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public bool IsExistParamFile(String paramPath)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(paramPath)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(paramPath));

                return File.Exists(paramPath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void DefaultSetting()
        {
            try
            {
                ObservableCollection<String> list = new ObservableCollection<String>();
                //list.Add("WaferAlign");
                list.Add("LowStandard");
                list.Add("HighStandard");
                list.Add("PadStandard");
                this.setups = list;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum MOVETONEXTDIE()
        {
            EventCodeEnum RetVal;

            try
            {
                RetVal = StageModuleState.MOVETONEXTDIE();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return RetVal;
        }

        private IProbeCommandToken _RequestToken;
        public IProbeCommandToken RequestToken
        {
            get { return _RequestToken; }
            set { _RequestToken = value; }
        }

        Task _ClickToMoveTask;
        private AsyncCommand<object> _ClickToMoveLButtonDownCommand;
        public ICommand ClickToMoveLButtonDownCommand
        {
            get
            {
                if (null == _ClickToMoveLButtonDownCommand)
                    _ClickToMoveLButtonDownCommand = new AsyncCommand<object>(ClickToMoveLButtonDown, showWaitMessage: "Move to clicked position");
                return _ClickToMoveLButtonDownCommand;
            }
        }

        public async Task ClickToMoveLButtonDown(object enableClickToMove)
        {
            if ((bool)enableClickToMove == false)
                return;

            if (this.MotionManager().StageAxesBusy())
                return;

            if (_ClickToMoveTask?.IsCompleted == false)
                return;

            _ClickToMoveTask = new Task(() =>
            {
                MoveStageRepeatRelMove(MoveTargetPosX, MoveTargetPosY);
            });
            _ClickToMoveTask.Start();
            await _ClickToMoveTask;
        }
        public void MoveStageToTargetPos(object enableClickToMove)
        {
            if ((bool)enableClickToMove == false)
                return;
            if (this.MotionManager().StageAxesBusy())
                return;
            MoveStageRepeatRelMove(MoveTargetPosX, MoveTargetPosY);
        }
        //.. 현재 Actual 값 가져오는법 . CoordinateManager Binding.
        //double xact = MotionManager.ProbeAxisEx.ProbeAxisProviders[0].Status.Position.Actual;
        public void MoveStageRepeatRelMove(double posX, double posY)
        {
            try
            {
                StageMoveFlag_Display = true;
                bool pressmove = false;
                //들어온 시간 측정
                Stopwatch stw = new Stopwatch();
                stw.Start();
                bool startimeout = true;
                while (startimeout)
                {
                    if (stw.ElapsedMilliseconds > (long)250)
                    {
                        startimeout = false;
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            if (Mouse.LeftButton == MouseButtonState.Pressed)
                            {
                                pressmove = true;
                            }
                        }));
                    }
                }

                var axisx = this.MotionManager().GetAxis(EnumAxisConstants.X);
                var axisy = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                double xvel = axisx.Param.Speed.Value;
                double yvel = axisy.Param.Speed.Value;
                double xacc = axisx.Param.Acceleration.Value;
                double yacc = axisy.Param.Acceleration.Value;
                double velmaxval = 3.0;
                double velcurval = 1.0;

                //this.WaferAligner().HeightSearchIndex(posX, posY);

                double accOverrideThreshold = 5000.0;
                if (posX < accOverrideThreshold | posY < accOverrideThreshold)
                {
                    _StageModuleState.MoveStageRepeatRelMove(posX, posY, xvel, xacc / 10.0, yvel, yacc / 10.0);
                }
                else
                {
                    _StageModuleState.MoveStageRepeatRelMove(posX, posY, xvel, xacc, yvel, yacc);
                }

                Stopwatch pressstw = new Stopwatch();
                pressstw.Start();
                long prevMotionTime = pressstw.ElapsedMilliseconds;
                while (pressmove)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        if (Mouse.LeftButton != MouseButtonState.Pressed)
                        {
                            pressmove = false;
                        }
                    }));
                    System.Threading.Thread.Sleep(10);
                    if (pressmove == false)
                        break;

                    if (pressstw.ElapsedMilliseconds > 5000)
                    {
                        //가속도높이기
                        velcurval += 0.1;
                        xvel *= velcurval;
                        yvel *= velcurval;

                        if (velmaxval < velcurval)
                        {
                            xvel = axisx.Param.Speed.Value * velmaxval;
                            yvel = axisy.Param.Speed.Value * velmaxval;
                        }
                    }

                    _StageModuleState.MoveStageRepeatRelMove(posX, posY, xvel, xacc, yvel, yacc);
                    if (pressstw != null)
                        prevMotionTime = pressstw.ElapsedMilliseconds;

                    xvel = axisx.Param.Speed.Value;
                    yvel = axisy.Param.Speed.Value;
                }
                LoggerManager.Debug($"ClickToMove(): Move to position ({posX:0.00}, {posY:0.00})");
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "MoveStageRepeatRelMove () : Error occurred.");
                LoggerManager.Exception(err);
            }
            finally
            {
                StageMoveFlag_Display = false;
            }
        }

        public CellInitModeEnum GetStageInitState()
        {
            try
            {
                lock (StageInitlock)
                {
                    return StageInitState;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void SetStageInitState(CellInitModeEnum e)
        {
            try
            {
                lock (StageInitlock)
                {
                    StageInitState = e;
                    if (e == CellInitModeEnum.NormalEnd)
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageMachineInitCompletedEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();

                        this.GEMModule().ClearAlarmOnly();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public EventCodeEnum ClearData()
        {
            try
            {
                EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

                return RetVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        public bool CheckAxisBusy()
        {
            try
            {
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.AxisBusy == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. StageSupervisor - CheckAxisBusy() : Error occured.");
                throw;
            }

            return true;
        }

        public bool CheckAxisIdle()
        {
            try
            {
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.State != EnumAxisState.IDLE)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.Assert(true);
                LoggerManager.Debug($"{err.ToString()}. StageSupervisor - CheckAxisIdle() : Error occured.");
                throw;
            }

            return true;
        }

        public EventCodeEnum CheckAvailableStageAbsMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                var axisPZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);
                // AxisX SWLimit 
                if (axisX != null)
                {
                    if (axisX.Param.PosSWLimit.Value < xPos)
                    {
                        LoggerManager.Error($"X Axis position prohibited by positive software limit. Position = {xPos:0.00}, SW Limit = {axisX.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisX.Param.NegSWLimit.Value > xPos)
                    {
                        LoggerManager.Error($"X Axis position prohibited by negative software position limit. Position = {xPos:0.00}, SW Limit = {axisX.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"X Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //AxisY SWLimit
                if (axisY != null)
                {
                    if (axisY.Param.PosSWLimit.Value < yPos)
                    {
                        LoggerManager.Error($"Y Axis position prohibited by positive software position limit. Position = {yPos:0.00}, SW Limit = {axisY.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisY.Param.NegSWLimit.Value > yPos)
                    {
                        LoggerManager.Error($"Y Axis position prohibited by negative software position limit. Position = {yPos:0.00}, SW Limit = {axisY.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"Y Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //AxisZ
                if (axisZ != null)
                {
                    if (axisZ.Param.PosSWLimit.Value < zPos)
                    {
                        LoggerManager.Error($"Z Axis position prohibited by positive software position limit. Position = {zPos:0.00}, SW Limit = {axisZ.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;


                    }
                    else if (axisZ.Param.NegSWLimit.Value > zPos)
                    {
                        LoggerManager.Error($"Z Axis position prohibited by negative software position limit. Position = {zPos:0.00}, SW Limit = {axisZ.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"Z Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;

                }
                //AxisT
                if (axisT != null)
                {
                    if (axisT.Param.PosSWLimit.Value < tPos)
                    {
                        LoggerManager.Error($"T Axis position prohibited by positive software position limit. Position = {tPos:0.00}, SW Limit = {axisT.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisT.Param.NegSWLimit.Value > tPos)
                    {
                        LoggerManager.Error($"T Axis position prohibited by negative software position limit. Position = {tPos:0.00}, SW Limit = {axisT.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"T Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //AxisPZ
                if (axisPZ != null)
                {
                    if (axisPZ.Param.PosSWLimit.Value < PZPos)
                    {
                        LoggerManager.Error($"PZ Axis position prohibited by positive software position limit. Position = {PZPos:0.00}, SW Limit = {axisPZ.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisPZ.Param.NegSWLimit.Value > PZPos)
                    {
                        LoggerManager.Error($"PZ Axis position prohibited by negative software position limit. Position = {PZPos:0.00}, SW Limit = {axisPZ.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"PZ Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;

                }

                //busy
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.AxisBusy == true)
                    {
                        break;
                    }
                    else
                    {
                        stagebusy = false;
                    }
                }

                //state
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.State != EnumAxisState.IDLE)
                    {
                        return EventCodeEnum.MOTION_STAGESTATE_NOT_IDLE;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
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

        public EventCodeEnum CheckAvailableStageRelMove(double xPos, double yPos, double zPos, double tPos, double PZPos, ref bool stagebusy)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            try
            {
                var axisX = this.MotionManager().GetAxis(EnumAxisConstants.X);
                var axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                var axisZ = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                var axisT = this.MotionManager().GetAxis(EnumAxisConstants.C);
                var axisPZ = this.MotionManager().GetAxis(EnumAxisConstants.PZ);

                double axisXActualPos = 0.0;
                double axisYActualPos = 0.0;
                double axisZActualPos = 0.0;
                double axisTActualPos = 0.0;
                double axisPZActualPos = 0.0;
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref axisXActualPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref axisYActualPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref axisZActualPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref axisTActualPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.PZ, ref axisPZActualPos);


                // AxisX
                if (axisX != null)
                {
                    if (axisX.Param.PosSWLimit.Value < xPos + axisXActualPos)
                    {
                        LoggerManager.Error($"X Axis position prohibited by positive software position limit. Position = {xPos + axisXActualPos:0.00}, SW Limit = {axisX.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisX.Param.NegSWLimit.Value > xPos + axisXActualPos)
                    {
                        LoggerManager.Error($"X Axis position prohibited by negative software position limit. Position = {xPos + axisXActualPos:0.00}, SW Limit = {axisX.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"X Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;

                }


                //AxisY
                if (axisY != null)
                {
                    if (axisY.Param.PosSWLimit.Value < yPos + axisYActualPos)
                    {
                        LoggerManager.Error($"Y Axis position prohibited by positive software position limit. Position = {yPos + axisYActualPos:0.00}, SW Limit = {axisY.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisY.Param.NegSWLimit.Value > yPos + axisYActualPos)
                    {
                        LoggerManager.Error($"Y Axis position prohibited by negative software position limit. Position = {yPos + axisYActualPos:0.00}, SW Limit = {axisY.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;

                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"Y Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //AxisZ
                if (axisZ != null)
                {
                    if (axisZ.Param.PosSWLimit.Value < zPos + axisZActualPos)
                    {
                        LoggerManager.Error($"Z Axis position prohibited by positive software position limit. Position = {zPos + axisZActualPos:0.00}, SW Limit = {axisZ.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    }
                    else if (axisZ.Param.NegSWLimit.Value > zPos + axisZActualPos)
                    {
                        LoggerManager.Error($"Z Axis position prohibited by negative software position limit. Position = {zPos + axisZActualPos:0.00}, SW Limit = {axisZ.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"Z Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //AxisT
                if (axisT != null)
                {
                    if (axisT.Param.PosSWLimit.Value < tPos + axisTActualPos)
                    {
                        LoggerManager.Error($"T Axis position prohibited by positive software position limit. Position = {tPos + axisTActualPos:0.00}, SW Limit = {axisT.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;

                    }
                    else if (axisT.Param.NegSWLimit.Value > tPos + axisTActualPos)
                    {
                        LoggerManager.Error($"T Axis position prohibited by negative software position limit. Position = {tPos + axisTActualPos:0.00}, SW Limit = {axisT.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"T Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //AxisPZ
                if (axisPZ != null)
                {
                    if (axisPZ.Param.PosSWLimit.Value < PZPos + axisPZActualPos)
                    {
                        LoggerManager.Error($"PZ Axis position prohibited by positive software position limit. Position = {PZPos + axisPZActualPos:0.00}, SW Limit = {axisPZ.Param.PosSWLimit.Value}");
                        return EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    }
                    else if (axisPZ.Param.NegSWLimit.Value > PZPos + axisPZActualPos)
                    {
                        LoggerManager.Error($"PZ Axis position prohibited by negative software position limit. Position = {PZPos + axisPZActualPos:0.00}, SW Limit = {axisPZ.Param.NegSWLimit.Value}");
                        return EventCodeEnum.MOTION_NEG_SW_LIMIT_ERROR;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    LoggerManager.Error($"PZ Axis is null");
                    return EventCodeEnum.MOTION_INVALID_AXIS_OBJECT_ERROR;
                }

                //busy
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.AxisBusy == true)
                    {
                        break;
                    }
                    else
                    {
                        stagebusy = false;
                    }
                }

                //state
                foreach (var axis in this.MotionManager().StageAxes.ProbeAxisProviders)
                {
                    if (axis.Status.State != EnumAxisState.IDLE)
                    {
                        return EventCodeEnum.MOTION_STAGESTATE_NOT_IDLE;
                    }
                    else
                    {
                        ret = EventCodeEnum.NONE;
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

        public ExceptionReturnData ConvertToExceptionErrorCode(Exception err)
        {
            ExceptionReturnData exdata;
            try
            {
                throw err;
            }
            catch (MotionException motionexception)
            {
                exdata = new ExceptionReturnData(motionexception.Message, motionexception.ErrorCode, motionexception.ReturnValue);
            }
            catch (InOutException ioexception)
            {
                exdata = new ExceptionReturnData(ioexception.Message, ioexception.ErrorCode, ioexception.ReturnValue);

            }
            catch (VisionException visionexception)
            {
                exdata = new ExceptionReturnData(visionexception.Message, visionexception.ErrorCode, visionexception.ReturnValue);
            }
            catch (Exception exception)
            {
                exdata = new ExceptionReturnData(exception.Message, EventCodeEnum.DOT_NET_ERROR, -1);
            }

            return exdata;
        }

        public EventCodeEnum SaveNCSysObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = this.NCObject.SaveSysParameter();
            return retVal;
        }

        public EventCodeEnum LoadNCSysObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = this.NCObject.LoadSysParameter();



            return retVal;
        }

        public EventCodeEnum SaveTouchSensorObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = this.TouchSensorObject.SaveSysParameter();
            return retVal;
        }

        public EventCodeEnum LoadTouchSensorObject()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            retVal = this.TouchSensorObject.LoadSysParameter();



            return retVal;
        }

        #region // Remote services
        public void InitStageService(int stageAbsIndex = 0)
        {
            try
            {
                this.AbsoluteIndex = stageAbsIndex;
                LoggerManager.Debug($"Stage supervisor service initialized.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public async Task InitLoaderClient()
        {
            try
            {
                await this.LoaderController().ConnectLoaderService();
                this.EnvControlManager().InitConnect();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SetDynamicMode(DynamicModeEnum dynamicModeEnum)
        {
            try
            {
                this.LotOPModule().LotInfo.DynamicMode.Value = dynamicModeEnum;
                LoggerManager.Debug($"Set to DynamicMode : {dynamicModeEnum}");
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
                LoggerManager.Debug($"DeInit StageSupervisor Channel.");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
        public void InitServiceData()
        {
            this.LoaderController().BroadcastLotState(false);
        }

        public void BindDispService(string uri)
        {
            this.StageCommunicationManager().BindDispService(uri);
        }
        public void BindDelegateEventService(string uri)
        {
            try
            {
                this.StageCommunicationManager().BindDelegateEventService(uri);

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //widget mode인지아닌지 확인하고, widget 상태가 아니라면, 강제로 widget 으로 변경한뒤, message 띄워줌. 
                    if (this.ViewModelManager().MainWindowWidget.DataContext == null)
                    {
                        Visibility v = Visibility.Hidden;


                        v = (System.Windows.Application.Current.MainWindow).Visibility;

                        if (v == Visibility.Visible)
                        {
                            this.ViewModelManager().ViewTransitionAsync(new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"));
                            (System.Windows.Application.Current.MainWindow).Hide();

                            this.ViewModelManager().UpdateWidget();
                            this.ViewModelManager().MainWindowWidget.Show();
                        }

                        string title = "Error Message";
                        string message = $"[Cell#{this.LoaderController().GetChuckIndex()}]\r\n" +
                            $"The widget change to lock state changes as it connected with loader.";
                        this.MetroDialogManager().ShowMessageDialog(title, message, EnumMessageStyle.Affirmative);
                        MessageBox.Show(message, title);
                    }
                }));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void BindDataGatewayService(string uri)
        {
            try
            {
                this.StageCommunicationManager().BindDataGatewayService(uri);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetMoveTargetPos(double xpos, double ypos)
        {
            MoveTargetPosX = xpos;
            MoveTargetPosY = ypos;
        }
        public void SetWaferMapCam(EnumProberCam cam)
        {
            WaferObject.MapViewAssignCamType.Value = cam;
        }

        public List<DeviceObject> GetDevices()
        {
            return WaferObject.GetSubsInfo().Devices as List<DeviceObject>;
        }
        public byte[] GetWaferObject()
        {

            byte[] compressedData = null;
            try
            {
                var bytes = SerializeManager.SerializeToByte(WaferObject, typeof(WaferObject));
                compressedData = bytes;


            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetWaferObject(): Error occurred. Err = {err.Message}");
            }
            return compressedData;

        }

        public byte[] GetProbeCardObject()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(ProbeCardInfo, typeof(ProbeCard));

                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetProbeCardObject(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public byte[] GetMarkObject()
        {
            byte[] compressedData = null;

            try
            {
                var bytes = SerializeManager.SerializeToByte(MarkObject, typeof(MarkObject));

                compressedData = bytes;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"GetMarkObject(): Error occurred. Err = {err.Message}");
            }

            return compressedData;
        }

        public byte[] GetDIEs()
        {
            byte[] retval = null;

            try
            {
                retval = this.ObjectToByteArray(Wafer.GetSubsInfo().DIEs);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public byte[] GetNCObject()
        {
            byte[] compressedData = null;
            try
            {
                //var bytes = SerializeManager.SerializeToByte(NCObject, typeof(NeedleCleanObject));
                //compressedData = bytes;
                compressedData = NCObject.GetNCObjectByteArray();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return compressedData;
        }

        public void SetWaitCancelDialogHashCode(string hashCode)
        {
            WaitCancelDialogHashCode = hashCode;
        }
        public string GetWaitCancelDialogHashCode()
        {
            return WaitCancelDialogHashCode;
        }

        public EnumSubsStatus GetWaferStatus()
        {
            return WaferObject.WaferStatus;
        }

        public EnumWaferType GetWaferType()
        {
            return WaferObject.GetWaferType();
        }

        public async Task<EventCodeEnum> DoWaferAlign()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //this.WaferAligner().DoWaferAlign();

                EnumMessageDialogResult ret;

                if (this.StageMode != GPCellModeEnum.MAINTENANCE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message - Wafer Align", "It can only process in maintenance mode. Please check the mode again.", EnumMessageStyle.Affirmative);
                    retval = EventCodeEnum.NONE;
                    return retval;
                }
                if (this.SoakingModule().GetModuleMessage().Equals(SoakingStateEnum.AUTOSOAKING_RUNNING.ToString()))
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message - Wafer Align", "Auto Soaking is running. Please check the mode again.", EnumMessageStyle.Affirmative);
                    retval = EventCodeEnum.NONE;
                    return retval;
                }

                //ret = await messageDialog.ShowDialog("Wafer alignment", "Are you sure you want to wafer alignment?", "OK", "Cancel");
                if (IsRecoveryMode)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("ChuckVacuum Recovery Mode", "Please try again after recovery.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    if (StageMoveState != StageStateEnum.Z_UP)
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer alignment", "Are you sure you want to wafer alignment?", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            if (this.SequenceEngineManager().GetMovingState() == true)
                            {
                                retval = this.WaferAligner().DoManualOperation();

                                if (retval == EventCodeEnum.NONE)
                                {
                                    ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Succeed Wafer alignment", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    if (retval == EventCodeEnum.WAFER_NOT_EXIST_EROOR)
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Wafer alignment fail - Not Exist Wafer.", EnumMessageStyle.Affirmative);
                                    }
                                    else if (retval == EventCodeEnum.MARK_Move_Failure)
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Wafer alignment fail - Mark Align Fail.", EnumMessageStyle.Affirmative);
                                    }
                                    else if (retval == EventCodeEnum.WAFERALIGN_TEMP_DEVIATION_OUTOFRANGE)
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Wafer alignment fail - Temp. Deviation Out of Range.", EnumMessageStyle.Affirmative);
                                    }
                                    else if (retval == EventCodeEnum.WAFER_ALIGN_FAIL_MARK_DIFF_TOLERANCE)
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", this.WaferAligner().ManuallAlignmentErrTxt, EnumMessageStyle.Affirmative);
                                    }
                                    else if (retval == EventCodeEnum.Wafer_Index_Align_Edge_Failure)
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", $"Wafer alignment fail.\n{this.WaferAligner().ManuallAlignmentErrTxt}", EnumMessageStyle.Affirmative);
                                    }
                                    else if (retval == EventCodeEnum.WAFERTHICKNESS_VALIDATION_FAILED)
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", $"Wafer alignment fail.\n{this.WaferAligner().ManuallAlignmentErrTxt}", EnumMessageStyle.Affirmative);
                                    }
                                    else
                                    {
                                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer Align", "Wafer alignment fail", EnumMessageStyle.Affirmative);
                                    }
                                }
                            }
                            else
                            {
                                ret = await this.MetroDialogManager().ShowMessageDialog("Wafer alignment", "Can not wafer alignment.", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                    else
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Wafer alignment", "Can not Wafer alignment.\nCurrent Zup State. Please try after ZDown", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DoLot()
        {
            try
            {

                (this).CommandManager().SetCommand<ILotOpStart>(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void LotPause()
        {
            try
            {

                (this).CommandManager().SetCommand<ILotOpPause>(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public async Task DoSystemInit(bool showMessageDialogFlag = true)
        {
            //await this.SystemInit();

            EnumMessageDialogResult ret;

            //EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            ErrorCodeResult retval = new ErrorCodeResult();

            try
            {
                if (this.SequenceEngineManager().GetMovingState() == true)
                {
                    if (IsRecoveryMode)
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("ChuckVacuum Recovery Mode", "Please try again after recovery.", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        if (showMessageDialogFlag)
                            ret = await this.MetroDialogManager().ShowMessageDialog("System Initialize", "Are you sure you want to System Initialize?", EnumMessageStyle.AffirmativeAndNegative);
                        else
                            ret = EnumMessageDialogResult.AFFIRMATIVE;

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            LoggerManager.Debug($"Start Time = {DateTime.Now.ToString()}");

                            retval = await this.StageSupervisor().SystemInit();

                            if (retval.ErrorCode != EventCodeEnum.NONE)
                            {

                                LoggerManager.Prolog(PrologType.INFORMATION, retval.ErrorCode);
                                if (retval.ErrorCode == EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS)
                                {
                                    this.NotifyManager().Notify(EventCodeEnum.MOTION_STAGE_INIT_ERROR);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    await this.MetroDialogManager().ShowMessageDialog("System Initialize", $"Failed, Reason = {retval.ErrorCode} \n have to check that " +
                                        $"card up module and then manual operation ", EnumMessageStyle.Affirmative);
                                }
                                else if (retval.ErrorCode == EventCodeEnum.GP_CardChange_CHECK_TO_LATCH)
                                {
                                    this.NotifyManager().Notify(EventCodeEnum.MOTION_STAGE_INIT_ERROR);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    await this.MetroDialogManager().ShowMessageDialog("System Initialize", $"Failed, Reason = {retval.ErrorCode} \n have to check that " +
                                        $"card latch and then manual operation ", EnumMessageStyle.Affirmative);
                                }
                                else if (retval.ErrorCode == EventCodeEnum.STAGEMOVE_LOCK)
                                {
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    retval.ErrorMsg = "The stage is currently locked. Please try again after Unlock.";
                                    await this.MetroDialogManager().ShowMessageDialog("System Initialize", $"Failed, Reason = {retval.ErrorMsg}", EnumMessageStyle.Affirmative);
                                }
                                else if (retval.ErrorCode == EventCodeEnum.TESTERHEAD_PURGE_AIR_ERROR)
                                {
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    retval.ErrorMsg = "Tester head purge air off. Please check.";
                                    await this.MetroDialogManager().ShowMessageDialog("System Initialize", $"Failed, Reason = {retval.ErrorMsg}", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    this.NotifyManager().Notify(EventCodeEnum.MOTION_STAGE_INIT_ERROR);
                                    SemaphoreSlim semaphore = new SemaphoreSlim(0);
                                    this.EventManager().RaisingEvent(typeof(MachineInitFailEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    await this.MetroDialogManager().ShowMessageDialog("System Initialize", $"Failed, Reason = {retval.ErrorCode}", EnumMessageStyle.Affirmative);
                                }
                            }
                            else
                            {
                                ret = await this.MetroDialogManager().ShowMessageDialog("System Initialize", "Succeed Initialize", EnumMessageStyle.Affirmative);
                            }

                            LoggerManager.Debug($"End Time = {DateTime.Now.ToString()}");
                        }
                    }
                }
                else
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("System Initialize", "Can not Initialize.", EnumMessageStyle.AffirmativeAndNegative);
                }

                //(this).CommandManager().SetCommand<ISystemInit>(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.LoaderRemoteMediator()?.GetServiceCallBack()?.StageJobFinished();
            }
        }

        public async Task<EventCodeEnum> DoPinAlign()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                EnumMessageDialogResult ret;
                //this.CommandManager().SetCommand<IDOPINALIGN>(this);

                //ret = await messageDialog.ShowDialog("Pin Align", "Are you sure you want to pin alignment?", "OK", "Cancel");
                if (this.StageMode != GPCellModeEnum.MAINTENANCE)
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message - Pin Align", "It can only process in maintenance mode. Please check the mode again.", EnumMessageStyle.Affirmative);
                    retval = EventCodeEnum.NONE;
                    return retval;
                }
                if (this.SoakingModule().GetModuleMessage().Equals(SoakingStateEnum.AUTOSOAKING_RUNNING.ToString()))
                {
                    await this.MetroDialogManager().ShowMessageDialog("Error Message - Pin Align", "Auto Soaking is running. Please check the mode again.", EnumMessageStyle.Affirmative);
                    retval = EventCodeEnum.NONE;
                    return retval;
                }
                if (IsRecoveryMode)
                {
                    ret = await this.MetroDialogManager().ShowMessageDialog("ChuckVacuum Recovery Mode", "Please try again after recovery.", EnumMessageStyle.Affirmative);
                }
                else
                {
                    if (StageMoveState != StageStateEnum.Z_UP)
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Are you sure you want to pin alignment?", EnumMessageStyle.AffirmativeAndNegative);

                        if (ret == EnumMessageDialogResult.AFFIRMATIVE)
                        {
                            if (this.SequenceEngineManager().GetMovingState() == true)
                            {
                                retval = this.PinAligner().DoManualOperation();

                                PinAlignResultes AlignResult = (this.PinAligner().PinAlignInfo as PinAlignInfo).AlignResult;

                                if (retval == EventCodeEnum.NONE)
                                {
                                    StringBuilder stb = new StringBuilder();
                                    stb.Append("Pin Alignment is done successfully.");
                                    stb.Append(System.Environment.NewLine);

                                    stb.Append($"Source : {AlignResult.AlignSource}");
                                    stb.Append(System.Environment.NewLine);

                                    if (AlignResult != null)
                                    {
                                        List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                                        foreach (var result in eachPinResultsSortList)
                                        {
                                            stb.Append($" - Pin #{result.PinNum}. Shift = ");
                                            stb.Append($"X: {result.DiffX,4:0.0}um, ");
                                            stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                                            stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                                            stb.Append($" Height: {result.Height,7:0.0}um");
                                            stb.Append(System.Environment.NewLine);
                                        }
                                    }

                                    ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", stb.ToString(), EnumMessageStyle.Affirmative);
                                    //ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Succeed pin alignment", EnumMessageStyle.Affirmative);
                                }
                                else
                                {
                                    StringBuilder stb = new StringBuilder();
                                    stb.Append($"Pin Alignment is failed.");
                                    stb.Append(System.Environment.NewLine);

                                    stb.Append($"Source : {AlignResult.AlignSource}");
                                    stb.Append(System.Environment.NewLine);

                                    //stb.Append($"Reason : {this.PinAligner().ReasonOfError.Reason}");
                                    stb.Append($"Reason : {this.PinAligner().ReasonOfError.GetLastEventMessage()}");
                                    stb.Append(System.Environment.NewLine);

                                    string FailDescription = this.PinAligner().MakeFailDescription();

                                    if (FailDescription != string.Empty)
                                    {
                                        stb.Append(FailDescription);
                                        stb.Append(System.Environment.NewLine);
                                    }

                                    if (AlignResult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE)
                                    {
                                        stb.Append($"Center Diff X: {AlignResult.CardCenterDiffX,4:0.0}, Y: {AlignResult.CardCenterDiffY,4:0.0}, Z:{AlignResult.CardCenterDiffZ,4:0.0}");
                                        stb.Append(System.Environment.NewLine);
                                    }

                                    if (AlignResult != null)
                                    {
                                        List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                                        foreach (var result in eachPinResultsSortList)
                                        {
                                            stb.Append($" - Pin #{result.PinNum}. Shift = ");
                                            stb.Append($"X: {result.DiffX,4:0.0}um, ");
                                            stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                                            stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                                            stb.Append($" Height: {result.Height,7:0.0}um");
                                            stb.Append(System.Environment.NewLine);
                                        }
                                    }

                                    await this.MetroDialogManager().ShowMessageDialog("Pin Align", stb.ToString(), EnumMessageStyle.Affirmative, "OK");
                                }
                            }
                            else
                            {
                                ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Can not pin alignment.", EnumMessageStyle.Affirmative);
                            }
                        }
                    }
                    else
                    {
                        ret = await this.MetroDialogManager().ShowMessageDialog("Pin alignment", "Can not Pin alignment.\nCurrent Zup State. Please try after ZDown", EnumMessageStyle.Affirmative);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //this.LoaderRemoteMediator()?.GetServiceCallBack()?.StageJobFinished();
            }
            return retval;
        }

        public void SetAcceptUpdateDisp(bool flag)
        {
            this.StageCommunicationManager().SetAcceptUpdateDisp(flag);
        }


        public List<string> GetStageDebugDates()
        {
            List<string> debugdates = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "Debug");

            try
            {
                var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

                var files = directory.GetFiles();
                foreach (var debugfile in files)
                {
                    foreach (Match m in reg.Matches(debugfile.Name))
                    {
                        debugdates.Add(m.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return debugdates;
        }

        public List<string> GetStageTempDates()
        {
            List<string> dates = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "TEMP");

            try
            {
                var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    foreach (Match m in reg.Matches(file.Name))
                    {
                        dates.Add(m.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dates;
        }

        public List<string> GetStagePinDates()
        {
            List<string> dates = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "PIN");

            try
            {
                var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    foreach (Match m in reg.Matches(file.Name))
                    {
                        dates.Add(m.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dates;
        }
        public List<string> GetStagePMIDates()
        {
            List<string> dates = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "PMI");

            try
            {
                var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    foreach (Match m in reg.Matches(file.Name))
                    {
                        dates.Add(m.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dates;
        }

        public List<string> GetStageLotDates()
        {
            List<string> dates = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "LOT");

            try
            {
                var reg = new Regex(@"\d{4}\-\d{2}\-\d{2}");

                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    foreach (Match m in reg.Matches(file.Name))
                    {
                        dates.Add(m.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return dates;
        }
        private EventCodeEnum CopyDebugLog(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = null;

            logdate = date;
            string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
            string tempdebugpath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "Debug";
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "Debug");

            if (!Directory.Exists(tempdebugpath))
                Directory.CreateDirectory(tempdebugpath);
            try
            {
                var files = directory.GetFiles();
                foreach (var debugfile in files)
                {
                    if (debugfile.Name.Contains(logdate))
                    {
                        if (Directory.Exists(tempdebugpath))
                        {
                            debugfile.CopyTo(tempdebugpath + "\\" + $"{debugfile.Name}", true);
                        }

                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum CopyPinLog(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = null;
            logdate = date;
            //logdate = GetLogDate(changedate);
            //string today = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
            string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
            string tempdebugpath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "PIN";
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "PIN");

            if (!Directory.Exists(tempdebugpath))
                Directory.CreateDirectory(tempdebugpath);
            try
            {
                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    if (file.Name.Contains(logdate))
                    {
                        if (Directory.Exists(tempdebugpath))
                        {
                            file.CopyTo(tempdebugpath + "\\" + $"{file.Name}", true);
                        }

                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum CopyPMILog(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = null;
            logdate = date;
            //logdate = GetLogDate(changedate);
            //string today = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
            string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
            string tempdebugpath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "PMI";
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "PMI");

            if (!Directory.Exists(tempdebugpath))
                Directory.CreateDirectory(tempdebugpath);
            try
            {
                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    if (file.Name.Contains(logdate))
                    {
                        if (Directory.Exists(tempdebugpath))
                        {
                            file.CopyTo(tempdebugpath + "\\" + $"{file.Name}", true);
                        }

                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        private EventCodeEnum CopyTEMPLog(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = null;
            logdate = date;
            //logdate = GetLogDate(changedate);
            //string today = $"{DateTime.Today.Year}-{DateTime.Today.Month.ToString().PadLeft(2, '0')}-{DateTime.Today.Day.ToString().PadLeft(2, '0')}";
            string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
            string tempdebugpath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "TEMP";
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "TEMP");

            if (!Directory.Exists(tempdebugpath))
                Directory.CreateDirectory(tempdebugpath);
            try
            {
                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    if (file.Name.Contains(logdate))
                    {
                        if (Directory.Exists(tempdebugpath))
                        {
                            file.CopyTo(tempdebugpath + "\\" + $"{file.Name}", true);
                        }

                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }

        private EventCodeEnum CopyLOTLog(string date)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;
            string logdate = null;
            logdate = date;
            string lotpath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";
            string lotdebugpath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + "\\" + "LOT";
            DirectoryInfo directory = new DirectoryInfo(LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "LOT");

            if (!Directory.Exists(lotdebugpath))
                Directory.CreateDirectory(lotdebugpath);
            try
            {
                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    if (file.Name.Contains(logdate))
                    {
                        if (Directory.Exists(lotdebugpath))
                        {
                            file.CopyTo(lotdebugpath + "\\" + $"{file.Name}", true);
                        }

                    }
                }
                ret = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ret;
        }
        public byte[] GetLog(string date)
        {

            byte[] log = new byte[0];
            try
            {
                //string zippath = LoggerManager.LoggerManagerParam.DebugLoggerParam.LogDirPath + ".zip";
                string zippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + ".zip";
                string tempzippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + ".zip";
                string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";

                if (!Directory.Exists(temppath))
                {
                    Directory.CreateDirectory(temppath);
                }

                DirectoryInfo directory = new DirectoryInfo(
                    temppath);
                //if (!directory.Exists)
                //    return null;


                string extractPath = directory.FullName;
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;

                //원하는 파일들 옴기기
                CopyDebugLog(date);
                CopyPinLog(date);
                CopyPMILog(date);
                CopyTEMPLog(date);
                //

                //if (!File.Exists(zippath))
                //    ZipFile.CreateFromDirectory(
                //        LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder, zippath);
                if (!File.Exists(zippath))
                    ZipFile.CreateFromDirectory(
                        temppath, zippath);

                log = File.ReadAllBytes(zippath);

                //파일들 삭제 하기 
                File.Delete(zippath);
                Directory.Delete(temppath, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return log;
        }
        public byte[] GetRMdataFromFileName(string filename)
        {
            byte[] RM = new byte[0];
            try
            {
                string path = Path.Combine(this.ResultMapManager().LocalUploadPath, Path.GetFileNameWithoutExtension(filename), filename);
                if (!File.Exists(path))
                {
                    LoggerManager.Debug($"[GetRMdataFromFileName] : Not the correct path {path}");
                }
                else
                {
                    RM = File.ReadAllBytes(path);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return RM;
        }

        public byte[] GetODTPdataFromFileName(string filename)
        {
            byte[] ODTP = new byte[0];
            try
            {
                string path = Path.Combine(this.ODTPManager().LocalUploadPath, filename);
                if (!File.Exists(path))
                {
                    LoggerManager.Debug($"[GetRMdataFromFileName] : Not the correct path {path}");
                }
                else
                {
                    ODTP = File.ReadAllBytes(path);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return ODTP;
        }
        public byte[] GetLogFromFileName(EnumUploadLogType logtype, List<string> data)
        {
            byte[] log = new byte[0];
            try
            {
                //string zippath = LoggerManager.LoggerManagerParam.DebugLoggerParam.LogDirPath + ".zip";
                string zippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + ".zip";
                string tempzippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + ".zip";
                string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";

                if (!Directory.Exists(temppath))
                {
                    Directory.CreateDirectory(temppath);
                }

                DirectoryInfo directory = new DirectoryInfo(
                    temppath);
                //if (!directory.Exists)
                //    return null;


                string extractPath = directory.FullName;
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;
                switch (logtype)
                {
                    case EnumUploadLogType.Debug:
                        for (int i = 0; i < data.Count; i++)
                        {
                            CopyDebugLog(data[i]);
                        }
                        break;
                    case EnumUploadLogType.Temp:
                        for (int i = 0; i < data.Count; i++)
                        {
                            CopyTEMPLog(data[i]);
                        }
                        break;
                    case EnumUploadLogType.PMI:
                        for (int i = 0; i < data.Count; i++)
                        {
                            CopyPMILog(data[i]);
                        }
                        break;
                    case EnumUploadLogType.PIN:
                        for (int i = 0; i < data.Count; i++)
                        {
                            CopyPinLog(data[i]);
                        }
                        break;
                    case EnumUploadLogType.LOT:
                        for (int i = 0; i < data.Count; i++)
                        {
                            CopyLOTLog(data[i]);
                        }
                        break;
                    default:
                        break;
                }

                if (!File.Exists(zippath))
                    ZipFile.CreateFromDirectory(
                        temppath, zippath);

                log = File.ReadAllBytes(zippath);

                //파일들 삭제 하기 
                File.Delete(zippath);
                Directory.Delete(temppath, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return log;
        }
        public byte[] GetLogFromFilename(List<string> debug, List<string> temp, List<string> pin, List<string> pmi, List<string> lot)
        {

            byte[] log = new byte[0];
            try
            {
                //string zippath = LoggerManager.LoggerManagerParam.DebugLoggerParam.LogDirPath + ".zip";
                string zippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + ".zip";
                string tempzippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp" + ".zip";
                string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "temp";

                if (!Directory.Exists(temppath))
                {
                    Directory.CreateDirectory(temppath);
                }

                DirectoryInfo directory = new DirectoryInfo(
                    temppath);
                //if (!directory.Exists)
                //    return null;


                string extractPath = directory.FullName;
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;
                for (int i = 0; i < debug.Count; i++)
                {
                    CopyDebugLog(debug[i]);
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    CopyTEMPLog(temp[i]);
                }
                for (int i = 0; i < pin.Count; i++)
                {
                    CopyPinLog(pin[i]);
                }
                for (int i = 0; i < pmi.Count; i++)
                {
                    CopyPMILog(pmi[i]);
                }
                for (int i = 0; i < lot.Count; i++)
                {
                    CopyLOTLog(lot[i]);
                }
                //원하는 파일들 옴기기

                //

                //if (!File.Exists(zippath))
                //    ZipFile.CreateFromDirectory(
                //        LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder, zippath);
                if (!File.Exists(zippath))
                    ZipFile.CreateFromDirectory(
                        temppath, zippath);

                log = File.ReadAllBytes(zippath);

                //파일들 삭제 하기 
                File.Delete(zippath);
                Directory.Delete(temppath, true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return log;
        }

        public byte[] GetPinImageFromStage(List<string> pinImage)
        {
            byte[] log = new byte[0];
            try
            {
                string SaveBasePath = this.FileManager().GetImageSavePath(EnumProberModule.PINALIGNER, true, "SIZE_VALIDATION");
                string zippath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + ".zip";
                string temppath = LoggerManager.LoggerManagerParam.FilePath + "\\" + LoggerManager.LoggerManagerParam.DevFolder + "\\" + "SIZE_VALIDATION";

                if (!Directory.Exists(SaveBasePath))
                {
                    Directory.CreateDirectory(SaveBasePath);
                }

                string[] folders = Directory.GetDirectories(SaveBasePath);

                if (!Directory.Exists(temppath))
                {
                    Directory.CreateDirectory(temppath);
                }

                if (folders.Length != 0)
                {
                    foreach (var folder in folders)
                    {
                        string SaveBasePath_inside_folder = folder;
                        DirectoryInfo directory = new DirectoryInfo(SaveBasePath_inside_folder);
                        var files = directory.GetFiles();
                        string final_folder_name = directory.Name;

                        string tempsubpath = temppath + "\\" + final_folder_name;

                        if (!Directory.Exists(tempsubpath))
                        {
                            Directory.CreateDirectory(tempsubpath);
                        }

                        foreach (var file in files)
                        {
                            foreach (var date in pinImage)
                            {
                                if (file.Name.Contains(date.Replace("-", "")))
                                {
                                    file.CopyTo(tempsubpath + "\\" + $"{file.Name}", true);
                                }
                            }
                        }
                    }

                    if (!File.Exists(zippath))
                        ZipFile.CreateFromDirectory(
                            temppath, zippath);

                    log = File.ReadAllBytes(zippath);

                    //파일들 삭제 하기 
                    File.Delete(zippath);
                    Directory.Delete(temppath, true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return log;
        }
        public byte[] GetDevice()
        {
            byte[] device = new byte[0];
            try
            {
                string zippath = this.FileManager().GetDeviceRootPath() + "\\" + this.FileManager().GetDeviceName() + ".zip";
                DirectoryInfo directory = new DirectoryInfo(this.FileManager().GetDeviceRootPath() + "\\" + this.FileManager().GetDeviceName());
                if (!directory.Exists)
                    return null;

                string extractPath = directory.FullName;
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;

                if (!File.Exists(zippath))
                    ZipFile.CreateFromDirectory(this.FileManager().GetDeviceRootPath() + "\\" + this.FileManager().GetDeviceName(), zippath);

                device = File.ReadAllBytes(zippath);

                File.Delete(zippath);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return device;
        }


        /// <param name="device"></param>
        /// <param name="devicename"></param>
        /// <param name="lotid"></param>
        /// <param name="loaddev"></param>
        /// <param name="foupnumber"></param>
        /// <param name="showprogress"></param>
        /// <param name="checkreserve"></param> : Lot 상태와 Recipe 가 Load 중인걸 보고 예약에 대한 판단을 할지말지 설정 (false 인경우는 wafer 예약 전 lot 는 run 중이지만 recipe 를 변경해야 하는 경우)
        public void SetDevice(byte[] device, string devicename, string lotid, string lotCstHashCode, bool loaddev = true, int foupnumber = -1,
            bool showprogress = true, bool manualDownload = false)
        {
            try
            {
                this.DeviceModule().SetDevice(device, devicename, lotid, lotCstHashCode, loaddev, foupnumber, showprogress, manualDownload);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetNeedChangeParaemterInDeviceInfo(NeedChangeParameterInDevice needChangeParameter)
        {
            try
            {
                this.DeviceModule().SetNeedChangeParaemterInDeviceInfo(needChangeParameter);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public string GetDeviceName()
        {
            return this.FileManager().GetDeviceName();
        }

        public void SetEMG(EventCodeEnum errorCode)
        {
            this.MotionManager().StageEMGStop();
            LoggerManager.Error($"Error occuured : {errorCode}");
        }
        #endregion




        public async Task ChangeDeviceFuncUsingName(string devName)
        {
            IFileManager FileManager = this.FileManager();

            Autofac.IContainer Container = this.GetContainer();

            try
            {
                //this.ViewModelManager().Lock(this.GetHashCode(), "Wait", "Changing Device");

                if (!string.IsNullOrEmpty(devName))
                {
                    await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Change Device");

                    await Task.Run(async () =>
                    {
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        this.EventManager().RaisingEvent(typeof(StageRecipeReadStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                        semaphore.Wait();

                        FileManager.ChangeDevice(devName);

                        this.LotOPModule().SetDeviceName(devName);

                        try
                        {
                            this.ParamManager().DevDBElementDictionary.Clear();
                            var modules = Container.Resolve<IEnumerable<IFactoryModule>>().Where(module => module is IHasDevParameterizable);
                            foreach (var v in modules)
                            {
                                EventCodeEnum loadDevResult = EventCodeEnum.UNDEFINED;
                                try
                                {
                                    IHasDevParameterizable module = v as IHasDevParameterizable;
                                    loadDevResult = module.LoadDevParameter();
                                    loadDevResult = module.InitDevParameter();

                                    if (module is IStageSupervisor)
                                    {
                                        (module as IStageSupervisor)?.SetWaferObjectStatus();
                                    }

                                }
                                catch (Exception err)
                                {
                                    LoggerManager.Exception(err);

                                    throw new Exception($"Error Module is {v}");
                                }
                            }

                            this.ParamManager().LoadDevElementInfoFromDB();
                        }
                        catch (Exception err)
                        {
                            //LoggerManager.Error($"[DeviceChangeViewModel - ChangeDeviceFunc()] Error occurred while Load Device Parameter. {e.Message}");
                            LoggerManager.Exception(err);

                        }
                        finally
                        {
                            await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                        }

                        //foreach (var device in DeviceInfoCollection)
                        //{
                        //    if (device.Name == devName)
                        //    {
                        //        device.IsNowDevice = true;
                        //    }
                        //    else
                        //    {
                        //        device.IsNowDevice = false;
                        //    }
                        //}

                        this.StageSupervisor().SetWaferObjectStatus();

                        IProbingModule ProbingModule = this.ProbingModule();
                        ProbingModule.SetProbingMachineIndexToCenter();
                    });
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
            finally
            {
            }

        }

        //public bool IsAvailableLoaderRemoteMediator()
        //{
        //    if (this.LoaderRemoteMediator() == null)
        //        return false;
        //    if (this.LoaderRemoteMediator().ServiceCallBack != null)
        //    {
        //        if ((this.LoaderRemoteMediator().ServiceCallBack as IClientChannel).State != CommunicationState.Faulted
        //             & this.LoaderController().GetconnectFlag() == true)
        //            return true;
        //        else
        //        {
        //            //this.StageCommunicationManager().init
        //            return false;
        //        }
        //    }

        //    else
        //        return false;
        //}

        private object VacuumLockObj = new object();
        public void SetVacuum(bool ison)
        {
            try
            {
                lock (VacuumLockObj)
                {
                    //true - ON , false - OFF
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_0, ison);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_1, ison);
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOCHUCKAIRON_2, ison);

                    if (ison)
                    {
                        this.StageSupervisor().SetWaferObjectStatus();
                    }
                    else
                    {
                        this.StageSupervisor().WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum WaferHighViewIndexCoordMove(long mix, long miy)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_HIGH_CAM);
                MachineIndex mcoord = cam.GetCurCoordMachineIndex();

                long x = mcoord.XIndex;
                long y = mcoord.YIndex;

                MachineCoordinate mccoord = new MachineCoordinate();
                WaferCoordinate wfcoord = new WaferCoordinate();
                WaferCoordinate wfcoord_offset = new WaferCoordinate();
                WaferCoordinate wfcoord_LL = new WaferCoordinate();
                WaferCoordinate wfcoord_next = new WaferCoordinate();

                wfcoord = this.CoordinateManager().WaferHighChuckConvert.CurrentPosConvert();
                wfcoord_LL = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)x, (int)y);
                wfcoord_offset.X.Value = wfcoord.X.Value - wfcoord_LL.X.Value;
                wfcoord_offset.Y.Value = wfcoord.Y.Value - wfcoord_LL.Y.Value;

                wfcoord_next = this.WaferAligner().MachineIndexConvertToDieLeftCorner(mix, miy);
                wfcoord_next.X.Value = wfcoord_next.X.Value + wfcoord_offset.X.Value;
                wfcoord_next.Y.Value = wfcoord_next.Y.Value + wfcoord_offset.Y.Value;
                mccoord = this.CoordinateManager().WaferHighChuckConvert.ConvertBack(wfcoord_next);

                if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, mccoord.GetX()) != EventCodeEnum.NONE)
                {
                    return EventCodeEnum.UNDEFINED;
                }

                if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, mccoord.GetY()) != EventCodeEnum.NONE)
                {
                    return EventCodeEnum.UNDEFINED;
                }

                retval = StageModuleState.WaferHighViewIndexMove(mix, miy);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum WaferLowViewIndexCoordMove(long mix, long miy)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                ICamera cam = this.VisionManager().GetCam(EnumProberCam.WAFER_LOW_CAM);
                MachineIndex mcoord = cam.GetCurCoordMachineIndex();

                long x = mcoord.XIndex;
                long y = mcoord.YIndex;

                MachineCoordinate mccoord = new MachineCoordinate();
                WaferCoordinate wfcoord = new WaferCoordinate();
                WaferCoordinate wfcoord_offset = new WaferCoordinate();
                WaferCoordinate wfcoord_LL = new WaferCoordinate();
                WaferCoordinate wfcoord_next = new WaferCoordinate();

                wfcoord = this.CoordinateManager().WaferLowChuckConvert.CurrentPosConvert();
                wfcoord_LL = this.WaferAligner().MachineIndexConvertToDieLeftCorner((int)x, (int)y);
                wfcoord_offset.X.Value = wfcoord.X.Value - wfcoord_LL.X.Value;
                wfcoord_offset.Y.Value = wfcoord.Y.Value - wfcoord_LL.Y.Value;

                wfcoord_next = this.WaferAligner().MachineIndexConvertToDieLeftCorner(mix, miy);
                wfcoord_next.X.Value = wfcoord_next.X.Value + wfcoord_offset.X.Value;
                wfcoord_next.Y.Value = wfcoord_next.Y.Value + wfcoord_offset.Y.Value;

                mccoord = this.CoordinateManager().WaferLowChuckConvert.ConvertBack(wfcoord_next);

                if (this.MotionManager().CheckSWLimit(EnumAxisConstants.X, mccoord.GetX()) != EventCodeEnum.NONE)
                {
                    return retval;
                }

                if (this.MotionManager().CheckSWLimit(EnumAxisConstants.Y, mccoord.GetY()) != EventCodeEnum.NONE)
                {
                    return retval;
                }

                retval = StageModuleState.WaferLowViewIndexMove(mix, miy);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetProbeCardObject(IProbeCard param)
        {
            ProbeCardInfo = param;
        }

        public Element<AlignStateEnum> GetAlignState(AlignTypeEnum AlignType)
        {
            Element<AlignStateEnum> retval = null;

            try
            {
                switch (AlignType)
                {
                    case AlignTypeEnum.Wafer:
                        retval = WaferObject.AlignState;
                        break;
                    case AlignTypeEnum.Pin:
                        retval = ProbeCardInfo.AlignState;
                        break;
                    case AlignTypeEnum.Mark:
                        retval = MarkObject.AlignState;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public SubstrateInfoNonSerialized GetSubstrateInfoNonSerialized()
        {
            SubstrateInfoNonSerialized retval = new SubstrateInfoNonSerialized();

            //retval.WrapperDIEs = new WrapperDIEs();

            try
            {
                ISubstrateInfo tmp = this.WaferObject.GetSubsInfo();


                retval.WaferObjectChangedToggle = tmp.WaferObjectChangedToggle;
                retval.WaferCenter = tmp.WaferCenter;

                //retval.LoadingAngle = tmp.LoadingAngle;
                //retval.ContactCount = tmp.ContactCount;
                //retval.WaferType = tmp.WaferType;

                //double[,] dst = new double[src.GetLength(0), src.GetLength(1)];
                //Array.Copy(src, dst, src.Length);

                //for (int i = 0; i < length; i++)
                //{

                //}

                //retval.WrapperDIEs.DIEs = new DeviceObject[tmp.DIEs.GetLength(0), tmp.DIEs.GetLength(1)];
                //Array.Copy(tmp.DIEs, retval.WrapperDIEs.DIEs, tmp.DIEs.Length);

                //retval.PMIDIEs = tmp.PMIDIEs;
                //retval.CurrentDie = tmp.CurrentDie;
                //retval.SequenceProcessedCount = tmp.SequenceProcessedCount;
                retval.ActualDieSize = tmp.ActualDieSize;
                retval.ActualDeviceSize = tmp.ActualDeviceSize;
                //retval.WaferCenterOffset = tmp.WaferCenterOffset;
                //retval.MachineCoordZeroPosX = tmp.MachineCoordZeroPosX;
                //retval.MachineCoordZeroPosY = tmp.MachineCoordZeroPosY;
                //retval.WaferSequareness = tmp.WaferSequareness;
                //retval.MachineSequareness = tmp.MachineSequareness;
                //retval.AveWaferThick = tmp.AveWaferThick;
                //retval.ActualThickness = tmp.ActualThickness;
                //retval.RefDieLeftCorner = tmp.RefDieLeftCorner;
                //retval.isProbingDone = tmp.isProbingDone;
                //retval.OperatorID = tmp.OperatorID;

                //if(retval.OperatorID == null)
                //{
                //    retval.OperatorID = string.Empty;
                //}

                //retval.OperatorLevel = tmp.OperatorLevel;
                //retval.CPCount = tmp.CPCount;
                //retval.RetestCount = tmp.RetestCount;
                //retval.TestedDieCount = tmp.TestedDieCount;
                //retval.PassedDieCount = tmp.PassedDieCount;
                //retval.FailedDieCount = tmp.FailedDieCount;
                //retval.CurTestedDieCount = tmp.CurTestedDieCount;
                //retval.CurPassedDieCount = tmp.CurPassedDieCount;
                //retval.CurFailedDieCount = tmp.CurFailedDieCount;
                //retval.Yield = tmp.Yield;
                //retval.RetestYield = tmp.RetestYield;
                //retval.ChuckTemperature = tmp.ChuckTemperature;
                //retval.ProbingStartTime = tmp.ProbingStartTime;
                //retval.ProbingEndTime = tmp.ProbingEndTime;
                //retval.LoadingTime = tmp.LoadingTime;
                //retval.UnloadingTime = tmp.UnloadingTime;
                retval.DutCenX = tmp.DutCenX;
                retval.DutCenY = tmp.DutCenY;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public WaferObjectInfoNonSerialized GetWaferObjectInfoNonSerialize()
        {
            WaferObjectInfoNonSerialized retVal = new WaferObjectInfoNonSerialized();
            try
            {
                retVal.MapViewRenderMode = this.WaferObject.MapViewControlMode;
                retVal.MapViewStageSyncEnable = this.WaferObject.MapViewStageSyncEnable;
                retVal.MapViewCurIndexVisiablity = this.WaferObject.MapViewCurIndexVisiablity;
                retVal.TopLeftToBottomRightLineVisible = this.WaferObject.TopLeftToBottomRightLineVisible;
                retVal.BottomLeftToTopRightLineVisible = this.WaferObject.BottomLeftToTopRightLineVisible;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void WaferIndexUpdated(long xindex, long yindex)
        {
            try
            {
                this.WaferObject.SetCurrentMIndex(xindex, yindex);
            }
            catch (Exception err)
            {
                LoggerManager.Error(err.Message);
            }
        }

        public EventCodeEnum CheckPinPadParameterValidity()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ProbeCardInfo.CheckPinPadParameterValidity();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum GetPinDataFromPads()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = this.ProbeCardInfo.GetPinDataFromPads();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public PROBECARD_TYPE GetProbeCardType()
        {
            PROBECARD_TYPE retval = PROBECARD_TYPE.Cantilever_Standard;

            try
            {
                retval = this.ProbeCardInfo.ProbeCardDevObjectRef.ProbeCardType.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public int DutPadInfosCount()
        {
            int retval = 0;

            try
            {
                retval = this.WaferObject.GetSubsInfo().Pads.DutPadInfos.Count();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum InitGemConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.GEMModule().GemCommManager?.InitConnectService() ?? EventCodeEnum.UNDEFINED;
                if (retVal == EventCodeEnum.NONE)// 마지막에 AVAILABLE(Lot할당할 수 있는 상태)로 만들어줘야함. 
                {
                    if (StageMode == GPCellModeEnum.ONLINE)
                    {
                        this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.AVAILABLE);
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DeInitGemConnectService()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMode == GPCellModeEnum.ONLINE)
                {
                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNAVAILABLE);
                }
                retVal = this.GEMModule().GemCommManager?.DeInitConnectService() ?? EventCodeEnum.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public long InitSetStageHostFlag { get; set; } = -1;

        public EventCodeEnum SetStageMode(GPCellModeEnum cellmodeenum)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                PIVInfo pivinfo = new PIVInfo() { FoupNumber = this.GetParam_Wafer().GetOriginFoupNumber() };
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                var prestagemode = StageMode;
                if ((StageMode != cellmodeenum) || InitSetStageHostFlag == -1)// 셀에 로더가 Disconnect 되었다가 다시 붙으면 
                {
                    this.VisionManager().AllStageCameraStopGrab();
                    switch (cellmodeenum)
                    {
                        case GPCellModeEnum.OFFLINE:
                            {
                                if (prestagemode == GPCellModeEnum.MAINTENANCE)
                                {
                                    this.EventManager().RaisingEvent(typeof(SetupEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(SetupEndEvent).FullName));
                                }

                                this.EventManager().RaisingEvent(typeof(StageOffineEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                semaphore.Wait();
                                //this.StagePIV.SetStageState(GEMStageStateEnum.OFFLINE);
                                //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(StageOffineEvent).FullName));
                                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNAVAILABLE);
                                StreamingMode = StreamingModeEnum.STREAMING_OFF;
                                InitSetStageHostFlag = -1;
                                probeCardId = this.CardChangeModule().GetProbeCardID();
                                break;
                            }
                        case GPCellModeEnum.ONLINE:
                            {
                                if (prestagemode == GPCellModeEnum.MAINTENANCE)
                                {
                                    this.EventManager().RaisingEvent(typeof(SetupEndEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(SetupEndEvent).FullName));
                                }

                                //this.StagePIV.SetStageState(GEMStageStateEnum.ONLINE);

                                if ((StageMode != cellmodeenum) | InitSetStageHostFlag == -1)
                                {
                                    var eventRet = this.EventManager().RaisingEvent(typeof(StageOnlineEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                                    semaphore.Wait();
                                    if (eventRet == EventCodeEnum.NONE)
                                    {
                                        InitSetStageHostFlag = 1;
                                    }
                                    else
                                    {
                                        InitSetStageHostFlag = -1;
                                    }

                                    this.ParamManager().VerifyLotVIDsCheckBeforeLot();

                                    if (this.LoaderController().GetconnectFlag())
                                    {
                                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                        {
                                            //widget mode인지아닌지 확인하고, widget 상태가 아니라면, 강제로 widget 으로 변경한뒤, message 띄워줌. 
                                            if (this.ViewModelManager().MainWindowWidget.DataContext == null)
                                            {
                                                Visibility v = Visibility.Hidden;


                                                v = (System.Windows.Application.Current.MainWindow).Visibility;

                                                if (v == Visibility.Visible)
                                                {
                                                    this.ViewModelManager().ViewTransitionAsync(new Guid("6223DFD5-EFAA-4B49-AB70-D8A5F03FA65D"));
                                                    (System.Windows.Application.Current.MainWindow).Hide();

                                                    this.ViewModelManager().UpdateWidget();
                                                    this.ViewModelManager().MainWindowWidget.Show();
                                                }

                                                string title = "Error Message";
                                                string message = $"[Cell#{this.LoaderController().GetChuckIndex()}]\r\n" +
                                                    $"The widget change to lock state changes as it changes to online mode.";
                                                this.MetroDialogManager().ShowMessageDialog(title, message, EnumMessageStyle.Affirmative);
                                                MessageBox.Show(message, title);
                                            }
                                        }));
                                    }
                                }
                                //InitSetStageHostFlag = this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(StageOnlineEvent).FullName));
                                // TODO : V19
                                if (this.ProbeCardInfo.CardChangeModule().GetProbeCardID()?.Equals(probeCardId) == false)
                                {
                                    this.EventManager().RaisingEvent(typeof(StageProbecardInitalStateEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();
                                }

                                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.LOTOP_IDLE);
                                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.ONLINE);
                                this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.AVAILABLE);

                                StreamingMode = StreamingModeEnum.STREAMING_OFF;
                                break;
                            }
                        case GPCellModeEnum.MAINTENANCE:
                            {
                                bool Retval = false;

                                //soaking module이 runnning중인지를 확인하고 soaking module을 동작처리를 정리한다.                                                                       
                                Retval = this.LotOPModule().RunList.All(
                                    item =>
                                    item.ModuleState.GetState() != ModuleStateEnum.RUNNING &&
                                    item.ModuleState.GetState() != ModuleStateEnum.PENDING
                                    //item.ModuleState.GetState() != ModuleStateEnum.ERROR
                                    //&&item.ModuleState.GetState() != ModuleStateEnum.RECOVERY
                                    );

                                if (this.WaferTransferModule().NeedToRecovery)
                                {
                                    this.EventManager().RaisingEvent(typeof(SetupStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();

                                    //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(SetupStartEvent).FullName));
                                    StreamingMode = StreamingModeEnum.STREAMING_OFF;
                                    InitSetStageHostFlag = -1;
                                    probeCardId = this.CardChangeModule().GetProbeCardID();
                                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.MAINTENANCE);
                                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNAVAILABLE);
                                }
                                else if (Retval)
                                {
                                    this.EventManager().RaisingEvent(typeof(SetupStartEvent).FullName, new ProbeEventArgs(this, semaphore));
                                    semaphore.Wait();
                                    //this.GEMModule().SetEvent(this.GEMModule().GetEventNumberFormEventName(typeof(SetupStartEvent).FullName));

                                    StreamingMode = StreamingModeEnum.STREAMING_OFF;
                                    if (StageModuleState.GetState() != StageStateEnum.Z_UP)
                                    {
                                        LoggerManager.Debug($"Move to zcleared position for maintenance mode");
                                        var ret = StageModuleState.ZCLEARED();

                                        if (ret != EventCodeEnum.NONE)
                                        {
                                            LoggerManager.Error("Error occured while move to zcleard position when set maintenance mode");
                                        }
                                        else
                                        {
                                            this.ManualContactModule().ManualContactZDownStateTransition();
                                        }
                                    }
                                    InitSetStageHostFlag = -1;
                                    probeCardId = this.CardChangeModule().GetProbeCardID();
                                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.MAINTENANCE);
                                    this.GEMModule().GetPIVContainer().SetStageState(GEMStageStateEnum.UNAVAILABLE);
                                    if (this.SoakingModule().ModuleState.GetState() == ModuleStateEnum.ERROR)
                                    {
                                        this.SoakingModule().ClearState();
                                        LoggerManager.Debug("Soaking ClearState when change to MAINTENANCE mode");
                                    }
                                }
                                else
                                {
                                    retVal = EventCodeEnum.UNDEFINED;
                                    this.MetroDialogManager().ShowMessageDialog("CellMode Setting Warning", "CellMode cannot be changed. Please try again", EnumMessageStyle.Affirmative);
                                    LoggerManager.Error($"Error - Cannot change StageMode(Current:{StageMode.ToString()}, Target:{cellmodeenum.ToString()}");
                                }
                                break;

                            }
                        default:
                            break;
                    }

                    if (EventCodeEnum.NONE == retVal)
                    {
                        GPCellModeEnum preStageMode = StageMode;
                        StageModeChange(cellmodeenum);
                        LoggerManager.Debug($"Cell stageMode change - Pre:{preStageMode.ToString()}, Cur:{cellmodeenum.ToString()}");
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                this.MetroDialogManager().ShowMessageDialog("CellMode Setting Warning", "CellMode cannot be changed. Please try again", EnumMessageStyle.Affirmative);
                LoggerManager.Error($"Error(Exception) - Cannot change StageMode(Current:{StageMode.ToString()}, Target:{cellmodeenum.ToString()}");
                retVal = EventCodeEnum.UNDEFINED;
            }
            return retVal;
        }

        private void StageModeChange(GPCellModeEnum mode)
        {
            try
            {
                StageMode = mode;
                LoggerManager.SetStageMode(StageMode.ToString());
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
        public (GPCellModeEnum, StreamingModeEnum) GetStageMode()
        {
            try
            {
                return (StageMode, StreamingMode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return (GPCellModeEnum.OFFLINE, StreamingModeEnum.UNDEFINED);
            }
        }

        public (DispFlipEnum disphorflip, DispFlipEnum dispverflip) GetDisplayFlipInfo()
        {
            (DispFlipEnum disphorflip, DispFlipEnum dispverflip) ret = (DispFlipEnum.NONE, DispFlipEnum.NONE);
            try
            {
                ret = (this.VisionManager().GetDispHorFlip(), this.VisionManager().GetDispVerFlip());
                return ret;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return ret;
            }
        }

        public (bool reverseX, bool reverseY) GetReverseMoveInfo()
        {
            (bool reverseX, bool reverseY) ret = (false, false);
            try
            {
                ret = (this.CoordinateManager().GetReverseManualMoveX(),
                    this.CoordinateManager().GetReverseManualMoveY());
                return ret;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return ret;
            }
        }

        public EventCodeEnum NotifySystemErrorToConnectedCells(EnumLoaderEmergency emgtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.MonitoringManager().RecievedFromLoaderEMG(emgtype);
                retVal = this.EnvControlManager().ValveManager.SetEMGSTOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum SetEMGColdFunction()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.EnvControlManager().ValveManager.SetEMGSTOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public string[] LoadEventLog(string lFileName)
        {
            return LoggerManager.LoadEventLog(lFileName);
        }

        public string GetLotErrorMessage()
        {
            string retMessage = null;
            try
            {
                if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.PAUSED)
                {
                    EventCodeInfo lasteventCodeInfo = this.LotOPModule().PauseSourceEvent;

                    if (lasteventCodeInfo != null)
                    {
                        string PauseBaseMsg = $"Occurred time : {lasteventCodeInfo.OccurredTime}\n" +
                                              $"Occurred location : {lasteventCodeInfo.ModuleType}\n" +
                                              $"Reason : {lasteventCodeInfo.Message}";

                        // ModuleType에 따라 로직 구성 가능

                        if (lasteventCodeInfo.ModuleType == ModuleEnum.WaferAlign)
                        {
                            if (this.WaferAligner().GetIsModify())
                            {
                                retMessage = PauseBaseMsg + "\n" + "Switch the stage to the maintanance mode, and press the recovery button on wafer align setup screen to proceed";
                            }
                        }
                        else if (lasteventCodeInfo.ModuleType == ModuleEnum.PinAlign)
                        {
                            if (this.PinAligner().ModuleState.GetState() == ModuleStateEnum.RECOVERY || this.PinAligner().ModuleState.GetState() == ModuleStateEnum.ERROR)
                            {
                                retMessage = PauseBaseMsg + "\n" + "Switch the stage to the maintanance mode, and press the recovery button on wafer align setup screen to proceed";
                            }
                            else
                            {
                                retMessage = PauseBaseMsg;
                            }
                        }
                        else
                        {
                            retMessage = PauseBaseMsg;
                        }
                    }
                    else if (this.GEMModule().GetPIVContainer().DevDownResult.Value == 0)
                    {
                        retMessage = "DownLoad Recipe Error : Switch the stage to the maintanance mode, and change to device change screen";
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retMessage;
        }

        public EventCodeEnum HandlerVacOnOff(bool val, int stageindex = -1)
        {
            EventCodeEnum errorCode = EventCodeEnum.UNDEFINED;
            IORet retIO = IORet.ERROR;
            try
            {
                EnumWaferSize device = this.LoaderController().GetTransferWaferSize();
                switch (device)
                {
                    case EnumWaferSize.INVALID:
                        break;
                    case EnumWaferSize.UNDEFINED:
                        break;
                    case EnumWaferSize.INCH6:
                        retIO = this.IOManager().IOServ.WriteBit(
                            this.IOManager().IO.Outputs.DOBERNOULLI_6INCH, val);
                        break;
                    case EnumWaferSize.INCH8:
                        retIO = this.IOManager().IOServ.WriteBit(
                            this.IOManager().IO.Outputs.DOBERNOULLI_8INCH, val);
                        break;
                    case EnumWaferSize.INCH12:
                        retIO = this.IOManager().IOServ.WriteBit(
                            this.IOManager().IO.Outputs.DOBERNOULLI_12INCH, val);
                        break;
                }
                errorCode = EventCodeEnum.NONE;
            }
            catch (InOutException ioexception)
            {
                errorCode = ioexception.ErrorCode;
                throw new InOutException($"{ioexception.Message}", ioexception.InnerException, ioexception.ErrorCode, ioexception.ReturnValue, MethodBase.GetCurrentMethod());
            }
            catch (Exception err)
            {
                LoggerManager.Error($"TurnoffBernoulli(): Exception occurred. Err = {err.Message}");
            }
            return errorCode;
        }

        public bool CheckUsingHandler(int stageindex = -1)
        {
            var BernoulliTopHandlerAttached = this.CoordinateManager().StageCoord.BernoulliTopHandlerAttached.Value;
            var device = this.StageSupervisor().WaferObject.GetPhysInfo().WaferSubstrateType.Value;
            if ((BernoulliTopHandlerAttached == true) && (device == WaferSubstrateTypeEnum.Thin))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<List<string>> UpdateLogFile()
        {
            return LoggerManager.UpdateLogFile();
        }

        public byte[] OpenLogFile(string selectedFilePath)
        {
            return LoggerManager.OpenLogFile(selectedFilePath);
        }

        public void ChangeLotMode(LotModeEnum mode)
        {
            try
            {
                LoggerManager.Debug($"[StageSupervisor], ChangeLotMode() : Lot mode is changed. Before = {this.LotOPModule().LotInfo.LotMode}, After = {mode}");

                this.LotOPModule().LotInfo.LotMode.Value = mode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetLotModeByForcedLotMode()
        {
            try
            {
                var RetestDevParam = this.RetestModule().GetRetestIParam() as RetestObject.RetestDeviceParam;
                if (RetestDevParam != null)
                {
                    switch (RetestDevParam.ForcedLotMode.Value)
                    {
                        case ForcedLotModeEnum.UNDEFINED:
                            break;
                        case ForcedLotModeEnum.ForcedCP1:
                            ChangeLotMode(LotModeEnum.CP1);
                            break;
                        case ForcedLotModeEnum.ForcedMPP:
                            ChangeLotMode(LotModeEnum.MPP);
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
        }

        public void SetErrorCodeAlarm(EventCodeEnum errorCode)
        {
            try
            {
                this.NotifyManager().Notify(errorCode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public void StopBeforeProbingCmd(bool stopBeforeProbing)
        {
            try
            {
                this.LotOPModule().LotDeviceParam.StopOption.StopBeforeProbing.Value = stopBeforeProbing;
                LoggerManager.Error($"StopBeforeProbing Option : {stopBeforeProbing}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void StopAfterProbingCmd(bool stopAfterProbing)
        {
            try
            {
                this.LotOPModule().LotDeviceParam.StopOption.StopAfterProbing.Value = stopAfterProbing;
                LoggerManager.Error($"StopAfterProbing Option : {stopAfterProbing}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnceStopBeforeProbingCmd(bool onceStopBeforeProbing)
        {
            try
            {
                this.LotOPModule().LotDeviceParam.StopOption.OnceStopBeforeProbing.Value = onceStopBeforeProbing;
                LoggerManager.Error($"OnceStopBeforeProbing Option : {onceStopBeforeProbing}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void OnceStopAfterProbingCmd(bool onceStopAfterProbing)
        {
            try
            {
                this.LotOPModule().LotDeviceParam.StopOption.OnceStopAfterProbing.Value = onceStopAfterProbing;
                LoggerManager.Error($"OnceStopAfterProbing Option : {onceStopAfterProbing}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum CheckManualZUpState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (this.StageMoveState == StageStateEnum.Z_UP) //스테이지 무브 스테이트 zup이 아닌경우엔 메뉴얼 zup 스테이트도 false 로 바꿔져야함.
                {
                    if (this.ManualContactModule().IsZUpState == true)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                }
                else
                {
                    this.ManualContactModule().IsZUpState = false;
                }
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.UNDEFINED;
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum DoPinPadMatch_FirstSequence()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            if (this.GetParam_ProbeCard().GetAlignState() != AlignStateEnum.DONE)
            {
                this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                    "Required to finish pin alignment previously.",
                    EnumMessageStyle.Affirmative);
            }
            else if (this.GetParam_Wafer().GetAlignState() != AlignStateEnum.DONE)
            {
                this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                    "Required to finish wafer alignment previously.",
                    EnumMessageStyle.Affirmative);
            }
            else
            {

                PinPadMatchModule.PinPadMatchModule PTPAModule = new PinPadMatchModule.PinPadMatchModule();
                var ret = PTPAModule.DoPinPadMatch();
                if (ret == EventCodeEnum.NONE)
                {
                    MachineIndex MI = new MachineIndex();
                    MachineIndex TI = this.Wafer.GetPhysInfo().TeachDieMIndex.Value;

                    retVal = this.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);
                    if (MI.XIndex == TI.XIndex && MI.YIndex == TI.YIndex)
                    {

                        WaferCoordinate Wafer = new WaferCoordinate();
                        PinCoordinate pin = new PinCoordinate();
                        double od = this.ProbingModule().OverDrive;
                        double zc = this.ProbingModule().ZClearence;

                        //Wafer = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);
                        Wafer = this.WaferAligner().MachineIndexConvertToProbingCoord((int)MI.XIndex, (int)MI.YIndex);
                        pin.X.Value = ProbeCardInfo.ProbeCardDevObjectRef.PinCenX;
                        pin.Y.Value = ProbeCardInfo.ProbeCardDevObjectRef.PinCenY;
                        pin.Z.Value = ProbeCardInfo.ProbeCardDevObjectRef.PinHeight;
                        zc = this.ProbingModule().CalculateZClearenceUsingOD(od, zc);

                        retVal = StageModuleState.MovePadToPin(Wafer, pin, zc);
                        this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.DONE);
                    }
                    else
                    {
                        this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                        this.MetroDialogManager().ShowMessageDialog("PinPadMatch Index NotMatch", $"ProbingSequnce index(X,Y)({MI.XIndex},{MI.YIndex}) , Teach Die index(X,Y)({TI.XIndex},{TI.YIndex})  ",
                        EnumMessageStyle.Affirmative);
                        retVal = EventCodeEnum.UNDEFINED;
                    }
                }
                else
                {
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                    this.MetroDialogManager().ShowMessageDialog("PinPadMatch Error", $"errorCode:{ret}",
                    EnumMessageStyle.Affirmative);
                }
            }
            return retVal;
        }
        public EventCodeEnum DO_ManualZUP()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (this.GetParam_ProbeCard().GetAlignState() != AlignStateEnum.DONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                        "Required to finish pin alignment previously.",
                        EnumMessageStyle.Affirmative);
                }
                else if (this.GetParam_Wafer().GetAlignState() != AlignStateEnum.DONE)
                {
                    this.MetroDialogManager().ShowMessageDialog("Z Up", "Could not proceed Z up operation! \n" +
                        "Required to finish wafer alignment previously.",
                        EnumMessageStyle.Affirmative);
                }
                else
                {
                    MachineIndex MI = new MachineIndex();
                    //retVal = this.ProbingModule().ProbingSequenceModule().GetFirstSequence(ref MI);
                    MI = this.Wafer.GetPhysInfo().TeachDieMIndex.Value;

                    // Setter에서 MoveToPosition() 호출 됨. ZUP과 StageMove 동작 충돌 가능
                    //this.ManualContactModule().MXYIndex = new Point(MI.XIndex, MI.YIndex);

                    LoggerManager.Prolog(PrologType.INFORMATION, EventCodeEnum.Pin_To_Pad_Alignment_OK);


                    bool needtoask = true;
                    if (this.CardChangeModule().IsZifRequestedState(true, writelog: true) == EventCodeEnum.NONE)
                    {
                        needtoask = false;
                    }

                    var isZupok = true;
                    if (needtoask)
                    {
                        var msg = this.MetroDialogManager().ShowMessageDialog("Z Up", $"Zif Lock State is Unlock. Are you sure Zup?",
                                    EnumMessageStyle.AffirmativeAndNegative);
                        if (msg.Result == EnumMessageDialogResult.NEGATIVE)
                        {
                            isZupok = false;
                        }
                    }

                    if (isZupok)
                    {
                        // if()
                        // 1. param setting 버튼을 눌렀는지
                        // 2. first contact , all contact height 값이 있는지  == 0이 아니면,

                        double od = this.ProbingModule().OverDrive;
                        this.ManualContactModule().ZUpMode(MI.XIndex, MI.YIndex, od);
                        this.LoaderController().SetTitleMessage(this.LoaderController().GetChuckIndex(), "Manual Z_UP");
                    }
                    else
                    {
                        LoggerManager.Debug($"Do not action zup. User canceled zup.");
                    }
                    retVal = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DO_ManualZDown()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                this.ManualContactModule().ZDownMode();
                this.LoaderController().SetTitleMessage(this.LoaderController().GetChuckIndex(), "Manual Z_Down");
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DoManualSoaking()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum DoManualPinAlign(bool CheckStageMode = true)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (CheckStageMode)
                {
                    if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                    {
                        if (this.StageMode != GPCellModeEnum.MAINTENANCE)
                        {
                            this.MetroDialogManager().ShowMessageDialog("Error Message - Pin Align", "It can only process in maintenance mode. Please check the mode again.", EnumMessageStyle.Affirmative);
                            retval = EventCodeEnum.NONE;
                            return retval;
                        }
                    }
                }

                if (this.SoakingModule().GetModuleMessage().Equals(SoakingStateEnum.AUTOSOAKING_RUNNING.ToString()))
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message - Pin Align", "Auto Soaking is running. Please check the mode again.", EnumMessageStyle.Affirmative);
                    retval = EventCodeEnum.NONE;
                    return retval;
                }
                if (this.SequenceEngineManager().GetMovingState() == true)
                {
                    retval = this.PinAligner().DoManualOperation();

                    PinAlignResultes AlignResult = (this.PinAligner().PinAlignInfo as PinAlignInfo).AlignResult;

                    if (retval == EventCodeEnum.NONE)
                    {
                        StringBuilder stb = new StringBuilder();
                        stb.Append("Pin Alignment is done successfully.");
                        stb.Append(System.Environment.NewLine);

                        stb.Append($"Source : {AlignResult.AlignSource}");
                        stb.Append(System.Environment.NewLine);

                        if (AlignResult != null)
                        {
                            List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                            foreach (var result in eachPinResultsSortList)
                            {
                                stb.Append($" - Pin #{result.PinNum}. Shift = ");
                                stb.Append($"X: {result.DiffX,4:0.0}um, ");
                                stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                                stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                                stb.Append($" Height: {result.Height,7:0.0}um");
                                stb.Append(System.Environment.NewLine);
                            }
                        }
                        this.MetroDialogManager().ShowMessageDialog("Pin Align", stb.ToString(), EnumMessageStyle.Affirmative);
                        //ret = await messageDialog.ShowDialog("Pin Align", "Succeed pin alignment", EnumMessageStyle.Affirmative);
                        //ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Succeed pin alignment", EnumMessageStyle.Affirmative);
                    }
                    else
                    {
                        StringBuilder stb = new StringBuilder();
                        stb.Append($"Pin Alignment is failed.");
                        stb.Append(System.Environment.NewLine);

                        stb.Append($"Source : {AlignResult.AlignSource}");
                        stb.Append(System.Environment.NewLine);

                        //stb.Append($"Reason : {this.PinAligner().ReasonOfError.Reason}");
                        stb.Append($"Reason : {this.PinAligner().ReasonOfError.GetLastEventMessage()}");
                        stb.Append(System.Environment.NewLine);

                        string FailDescription = this.PinAligner().MakeFailDescription();

                        if (FailDescription != string.Empty)
                        {
                            stb.Append(FailDescription);
                            stb.Append(System.Environment.NewLine);
                        }

                        if (AlignResult.Result == EventCodeEnum.PIN_CARD_CENTER_TOLERANCE)
                        {
                            stb.Append($"Center Diff X: {AlignResult.CardCenterDiffX}, Y: {AlignResult.CardCenterDiffY}, Z:{AlignResult.CardCenterDiffZ}");
                            stb.Append(System.Environment.NewLine);
                        }


                        if (AlignResult != null)
                        {
                            List<EachPinResult> eachPinResultsSortList = AlignResult.EachPinResultes.OrderBy(x => x.PinNum).ToList();

                            foreach (var result in eachPinResultsSortList)
                            {
                                stb.Append($" - Pin #{result.PinNum}. Shift = ");
                                stb.Append($"X: {result.DiffX,4:0.0}um, ");
                                stb.Append($"Y: {result.DiffY,4:0.0}um, ");
                                stb.Append($"Z: {result.DiffZ,4:0.0}um, ");
                                stb.Append($" Height: {result.Height,7:0.0}um");
                                stb.Append(System.Environment.NewLine);
                            }
                        }

                        this.MetroDialogManager().ShowMessageDialog("Pin Align", stb.ToString(), EnumMessageStyle.Affirmative, "OK");
                        //ret = await messageDialog.ShowDialog("Pin Align", "Fail pin alignment", EnumMessageStyle.Affirmative);
                        //ret = await this.MetroDialogManager().ShowMessageDialog("Pin Align", "Fail pin alignment", EnumMessageStyle.Affirmative);
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DoManualWaferAlign(bool CheckStageMode = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (CheckStageMode)
                {
                    if (this.StageMode != GPCellModeEnum.MAINTENANCE)
                    {
                        this.MetroDialogManager().ShowMessageDialog("Error Message - Wafer Align", "It can only process in maintenance mode. Please check the mode again.", EnumMessageStyle.Affirmative);
                        retVal = EventCodeEnum.NONE;

                        return retVal;
                    }
                }

                if (this.SoakingModule().GetModuleMessage().Equals(SoakingStateEnum.AUTOSOAKING_RUNNING.ToString()))
                {
                    this.MetroDialogManager().ShowMessageDialog("Error Message - Pin Align", "Auto Soaking is running. Please check the mode again.", EnumMessageStyle.Affirmative);
                    retVal = EventCodeEnum.NONE;

                    return retVal;
                }

                if (this.SequenceEngineManager().GetMovingState() == true)
                {
                    retVal = this.WaferAligner().DoManualOperation();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //manual로 align 후 실패로 나왔다면 end하여 state를 idle로 taransition 해놓자.(Lot run중에서는 Error Pause갔다 Resume하면 풀리지만 manual은 resume이 없으니..)
                if (this.WaferAligner().ModuleState.GetState() == ModuleStateEnum.ERROR)
                {
                    this.WaferAligner().End();
                }
            }
            return retVal;
        }
        public ILightAdmin LightAdmin { get; set; }
        public void LoadLUT()
        {
            try
            {
                LightAdmin = this.LightAdmin();
                LightAdmin.LoadLUT();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public bool GetRunState()
        {
            bool Retval = false;
            try
            {

                Retval = this.LotOPModule().RunList.All(
                    item =>
                    item.ModuleState.GetState() != ModuleStateEnum.RUNNING);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Retval;
        }


        public EventCodeEnum SetStageLock(ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                //중복 reason Add 안하기
                if (StageMoveLockStatus.LastStageMoveLockReasonList.Where(x => x.Equals(reason)).Count() == 0)
                {
                    StageMoveLockStatus.LastStageMoveLockReasonList.Add(reason);
                }
                else
                {
                    LoggerManager.Debug($"Reasons for duplication, reason: {reason}");
                }

                if (this.LotOPModule().ModuleState.State == ModuleStateEnum.IDLE || this.LotOPModule().ModuleState.State == ModuleStateEnum.PAUSED)
                {
                    if (GetRunState())
                    {
                        StageMoveLockStatus.LastStageMoveLockState = StageLockMode.LOCK;
                        LockMode = StageLockMode.LOCK;
                        StageModuleState.StageLock();
                        LoggerManager.Debug($"Stage Locked Reason: {reason}");
                    }
                    else
                    {
                        LoggerManager.Debug($"There is a module that is currently running in the RunList.");
                        LockMode = StageLockMode.RESERVE_LOCK;
                    }
                }
                else
                {
                    LockMode = StageLockMode.RESERVE_LOCK;
                }

                SaveStageMoveLockStatusParam();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        private List<ReasonOfStageMoveLock> TempList = new List<ReasonOfStageMoveLock>();

        public EventCodeEnum SetStageUnlock(ReasonOfStageMoveLock reason)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (StageMoveLockStatus.LastStageMoveLockReasonList.Count > 0)
                {
                    StageMoveLockStatus.LastStageMoveLockReasonList.RemoveAll(x => x.Equals(reason));
                }
                if (StageMoveLockStatus.LastStageMoveLockReasonList.Count == 0)
                {
                    if (LockMode == StageLockMode.LOCK)
                    {
                        StageModuleState.StageUnlock();
                        LoggerManager.Debug($"Stage Unlocked Reason: {reason}");
                    }
                    else
                    {
                        LoggerManager.Debug($"Already Unlock State");
                    }
                    StageMoveLockStatus.LastStageMoveLockState = StageLockMode.UNLOCK;
                    LockMode = StageLockMode.UNLOCK;
                }
                else
                {
                    string RemainReasons = "";
                    for (int i = 0; i < StageMoveLockStatus.LastStageMoveLockReasonList.Count; i++)
                    {
                        RemainReasons += StageMoveLockStatus.LastStageMoveLockReasonList[i];
                        RemainReasons += ",";
                    }
                    RemainReasons = RemainReasons.Substring(0, RemainReasons.Length - 1);
                    this.MetroDialogManager().ShowMessageDialog("Stage UnLock", $"There are reasons of lock that remain." +
                        $"\nCount: {StageMoveLockStatus.LastStageMoveLockReasonList.Count}" +
                        $"\nReasons: {RemainReasons}",
                        EnumMessageStyle.Affirmative);
                }
                SaveStageMoveLockStatusParam();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public StageLockMode GetStageLockMode()
        {
            return this.LockMode;
        }

        public bool IsForcedDoneMode()
        {
            bool retVal = false;
            try
            {
                if (this.LoaderController().ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public bool IsMovingState()
        {
            bool retVal = false;
            try
            {
                retVal = this.SequenceEngineManager().isMovingState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public void Set_TCW_Mode(bool isOn)
        {
            try
            {
                if ((_TCWMode == TCW_Mode.ON && isOn) ||
                    (_TCWMode == TCW_Mode.OFF && !isOn))
                {
                    LoggerManager.Debug($"Set_TCW_MODE : Skip. PREV TCW Mode:{_TCWMode}");
                    return;
                }

                if (isOn)
                {
                    LoggerManager.Debug($"Set_TCW_MODE : ON ,PREV TCW Mode:{_TCWMode}");
                    _TCWMode = TCW_Mode.ON;
                }
                else
                {
                    LoggerManager.Debug($"Set_TCW_MODE : OFF ,PREV TCW Mode:{_TCWMode}");
                    _TCWMode = TCW_Mode.OFF;
                    this.ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    this.ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                    this.Wafer.SetAlignState(AlignStateEnum.IDLE);
                    LoggerManager.Debug("ProbeCard Align_State: IDLE");
                    LoggerManager.Debug("WaferObject Align_State: IDLE");
                }
                this.LoaderController().SetTCW_Mode(_TCWMode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public TCW_Mode Get_TCW_Mode()
        {
            return _TCWMode;
        }

        public void LoaderConnected()
        {
            try
            {
                LoggerManager.Debug($"LoaderConnected!!");
                SemaphoreSlim semaphore = new SemaphoreSlim(0);
                this.EventManager().RaisingEvent(typeof(LoaderConnectedEvent).FullName, new ProbeEventArgs(this, semaphore));
                semaphore.Wait();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public EventCodeEnum ClearWaferStatus()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                LoggerManager.Debug($"[ClearWaferStatus] (Save_SlotInfo) Start");

                SlotInformation.WaferStatus = EnumSubsStatus.NOT_EXIST;
                SlotInformation.WaferType = EnumWaferType.UNDEFINED;
                SlotInformation.OCRAngle = -1;
                SlotInformation.WaferID = "";
                SlotInformation.WaferSize = SubstrateSizeEnum.UNDEFINED;
                SlotInformation.OriginSlotIndex = -1;
                SlotInformation.LoadingAngle = -1;
                SlotInformation.UnloadingAngle = -1;
                SlotInformation.SlotIndex = -1;
                SlotInformation.FoupIndex = -1;
                SlotInformation.WaferState = EnumWaferState.UNDEFINED;
                SlotInformation.OriginHolder = new ModuleID();

                var ret = SaveSlotInfo();

                LoggerManager.Debug($"[ClearWaferStatus] (Save_SlotInfo) Done RetVal:{ret}");

                this.WaferObject.SetWaferStatus(EnumSubsStatus.NOT_EXIST);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
}
