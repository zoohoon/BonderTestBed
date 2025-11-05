using CylType;
using LogModule;
using MetroDialogInterfaces;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.SequenceRunner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SequenceRunner
{
    #region ==> Help

    public class BehaviorResult : IBehaviorResult
    {
        private EventCodeEnum _ErrorCode = EventCodeEnum.UNDEFINED;
        public EventCodeEnum ErrorCode
        {
            get { return _ErrorCode; }
            set
            {
                _ErrorCode = value;
            }
        }

        private IBehaviorResult _InnerError;
        public IBehaviorResult InnerError
        {
            get { return _InnerError; }
            private set
            {
                _InnerError = value;
            }
        }

        private bool _PosNegBranch = true;
        public bool PosNegBranch
        {
            get { return _PosNegBranch; }
            set
            {
                _PosNegBranch = value;
            }
        }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
            }
        }
        private string _Reason;
        public string Reason
        {
            get { return _Reason; }
            set
            {
                _Reason = value;
            }
        }
        public BehaviorResult()
        {
            if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
            {
                ErrorCode = EventCodeEnum.NONE;
            }
            else
            {
                ErrorCode = EventCodeEnum.UNDEFINED;
            }


        }

        /// <summary>
        /// InnerError확인하여(InnerError가 Undefined인 상태, 오블젝트가 처음 생성될때 Undefined로 설정 됨)
        /// 처음 발생한 에러를 찾아내는 Recursive Method
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum GetRootCause()
        {
            EventCodeEnum rootCause = EventCodeEnum.UNDEFINED;
            try
            {
                if (InnerError != null)
                {
                    if (InnerError.ErrorCode != EventCodeEnum.UNDEFINED)
                    {
                        rootCause = InnerError.GetRootCause();
                        LoggerManager.Debug($"GetRootCause(): ErrorCode = {this.ErrorCode}, InnerError = {rootCause}");
                    }
                    else
                    {
                        rootCause = this.ErrorCode;
                    }
                }
                else
                {
                    rootCause = this.ErrorCode;
                }
            }
            catch (Exception err)
            {
                rootCause = this.ErrorCode;
                LoggerManager.Error($"GetRootCause(): Error occurred. Err = {err.Message}");
            }
            return rootCause;
        }

        public void SetInnerError(IBehaviorResult innererror)
        {
            InnerError = new BehaviorResult();
            InnerError = innererror;
        }
    }

    //1차 ccs
    [Serializable]
    public class SequenceBehaviorStruct : ISequenceBehaviorStruct
    {
        #region ==> Hidden
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
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


        [XmlIgnore, JsonIgnore]
        public string FilePath { get; } = "CardChange";
        [XmlIgnore, JsonIgnore]
        public string FileName { get; } = "CardChangeSequence.ccs";

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[CCStructForXML] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retval;
        }

        #endregion

        #region ==> 모든 Behavior의 정보를 담고 있는 Collection.
        private ObservableCollection<BehaviorGroupItem> _CollectionBaseBehavior;
        public ObservableCollection<BehaviorGroupItem> CollectionBaseBehavior
        {
            get { return _CollectionBaseBehavior; }
            set
            {
                _CollectionBaseBehavior = value;
            }
        }
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<ISequenceBehaviorGroupItem> ICollectionBaseBehavior
        {
            get { return new ObservableCollection<ISequenceBehaviorGroupItem>(_CollectionBaseBehavior); }
        }
        #endregion

        #region ==> 동작 순서.
        private ObservableCollection<SequenceBehavior> _BehaviorOrder;
        public ObservableCollection<SequenceBehavior> BehaviorOrder
        {
            get { return _BehaviorOrder; }
            set
            {
                _BehaviorOrder = value;
            }
        }
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<ISequenceBehavior> ICollectionLoadBehaviorOrder
        {
            get { return new ObservableCollection<ISequenceBehavior>(_BehaviorOrder); }
        }
        #endregion

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public void SetElementMetaData()
        {

        }
    }
    #endregion

    [Serializable]
    public class SequenceBehaviors : ISequenceBehaviors, INotifyPropertyChanged, ISystemParameterizable, IProbeCommandParameter
    {
        #region ==> Hidden

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; } = new List<object>();
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
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


        public string FilePath { get; } = "CardChange";
        public string FileName { get; } = "CardChangeSequence.ccs";

        #endregion

        public SequenceBehaviors()
        {
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                foreach (var v in ISequenceBehaviorCollection)
                {
                    retval = v.InitModule();

                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"v.InitModule() Failed");
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[SequenceBehaviors] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }

        #region <<Property>>
        private ObservableCollection<BehaviorGroupItem> _SequenceBehaviorCollection = new ObservableCollection<BehaviorGroupItem>();
        public ObservableCollection<BehaviorGroupItem> SequenceBehaviorCollection
        {
            get { return _SequenceBehaviorCollection; }
            set
            {
                if (value != _SequenceBehaviorCollection)
                {
                    _SequenceBehaviorCollection = value;
                    RaisePropertyChanged();
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public ObservableCollection<ISequenceBehaviorGroupItem> ISequenceBehaviorCollection
        {
            get { return new ObservableCollection<ISequenceBehaviorGroupItem>(_SequenceBehaviorCollection); }
        }

        [NonSerialized]
        private string _Command = string.Empty;
        [ParamIgnore, JsonIgnore]
        public string Command
        {
            get { return _Command; }
            set
            {
                if (value != _Command)
                {
                    _Command = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = SetDefaultParam();
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }

        public static implicit operator SequenceBehaviors(SequenceBehavior v)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class BehaviorGroupItem : ISequenceBehaviorGroupItem, INotifyPropertyChanged
    {
        #region <<NotifyPropertyChanged>>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        public string ParamLabel { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
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


        public ObservableCollection<ISequenceBehaviorRun> PreSafetyList { get; set; }
        = new ObservableCollection<ISequenceBehaviorRun>();
        public ObservableCollection<ISequenceBehaviorRun> PostSafetyList { get; set; }
        = new ObservableCollection<ISequenceBehaviorRun>();

        private SequenceBehavior _Behavior;
        public SequenceBehavior Behavior
        {
            get { return _Behavior; }
            set
            {
                _Behavior = value;
                RaisePropertyChanged(nameof(Behavior));
            }
        }

        public BehaviorGroupItem ReverseBehaviorGroupItem { get; set; }


        //public ObservableCollection<ISequenceBehaviorRun> IPreSafetyList
        //{
        //    get { return new ObservableCollection<ISequenceBehaviorRun>(PreSafetyList); }//이 부분 물어보기,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
        //}

        //public ObservableCollection<ISequenceBehaviorRun> IPostSafetyList
        //{
        //    get { return new ObservableCollection<ISequenceBehaviorRun>(PostSafetyList); }
        //}
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public ObservableCollection<ISequenceBehaviorRun> IPreSafetyList
        {
            get { return new ObservableCollection<ISequenceBehaviorRun>(PreSafetyList); }
        }
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public ObservableCollection<ISequenceBehaviorRun> IPostSafetyList
        {
            get { return new ObservableCollection<ISequenceBehaviorRun>(PostSafetyList); }
        }

        [XmlIgnore, JsonIgnore]
        public ISequenceBehavior IBehavior
        {
            get { return Behavior as ISequenceBehavior; }
        }
        [XmlIgnore, JsonIgnore]
        public ISequenceBehaviorGroupItem IReverseBehaviorGroupItem
        {
            get { return ReverseBehaviorGroupItem as ISequenceBehaviorGroupItem; }
        }
        [XmlIgnore, JsonIgnore]
        public ISequenceBehaviorState IBehaviorState
        {
            get { return BehaviorState as ISequenceBehaviorState; }
        }


        protected string _BehaviorID;
        public string BehaviorID
        {
            get { return _BehaviorID; }
            set { _BehaviorID = value; }
        }
        [XmlIgnore, JsonIgnore]
        public String SequenceDescription
        {
            get { return Behavior.SequenceDescription; }
        }

        [XmlIgnore, JsonIgnore]
        public String NextID_Positive
        {
            get { return Behavior.NextID_Positive; }
        }

        [XmlIgnore, JsonIgnore]
        public String NextID_Negative
        {
            get { return Behavior.NextID_Negative; }
        }

        private SequenceBehaviorState _BehaviorState;
        [XmlIgnore, JsonIgnore]
        public SequenceBehaviorState BehaviorState
        {
            get { return _BehaviorState; }
            set
            {
                _BehaviorState = value;
                RaisePropertyChanged(nameof(BehaviorState));
            }
        }

        private SequenceBehaviorStateEnum _StateEnum;
        public SequenceBehaviorStateEnum StateEnum
        {
            get { return _StateEnum; }
            set
            {
                _StateEnum = value;
                RaisePropertyChanged(nameof(StateEnum));
            }
        }

        public BehaviorGroupItem()
        {
            SequenceBehaviorStateTransition(new SequenceBehaviorIdleState());
        }

        public BehaviorGroupItem(ISequenceBehavior _choicedName)
        {
            Behavior = _choicedName as SequenceBehavior;
        }

        public void SequenceBehaviorStateTransition(ISequenceBehaviorState _BehaviorState)
        {
            BehaviorState = _BehaviorState as SequenceBehaviorState;
            StateEnum = _BehaviorState.GetState();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                InitState();
                ReverseBehaviorGroupItem?.InitModule();

                //foreach (var preSafety in PreSafetyList)//PreSafetyList를 싱글쪽에서 안쓰는데 이것때문에 문제가 생겨서 삭제함
                //{
                //    (preSafety as SequenceSafety).InputPorts = new List<IOPortDescripter<bool>>();
                //    (preSafety as SequenceSafety).OutputPorts = new List<IOPortDescripter<bool>>();
                //    (preSafety as SequenceSafety).InitModule();
                //}

                //Behavior.InputPorts = new List<IOPortDescripter<bool>>();
                //Behavior.OutputPorts = new List<IOPortDescripter<bool>>();
                //Behavior.InitModule();

                //foreach (var postSafety in PostSafetyList)
                //{
                //    (postSafety as SequenceSafety).InputPorts = new List<IOPortDescripter<bool>>();
                //    (postSafety as SequenceSafety).OutputPorts = new List<IOPortDescripter<bool>>();
                //    (postSafety as SequenceSafety).InitModule();
                //}
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void InitState()
        {
            SequenceBehaviorStateTransition(new SequenceBehaviorIdleState());
        }

        #region << RUN >>

        public async Task<IBehaviorResult> PreSafetyRun()
        {
            IBehaviorResult ccRet = new BehaviorResult();
            try
            {
                ISequenceRunner SequenceRunner = this.SequenceRunner();

                if (BehaviorState.GetState() == SequenceBehaviorStateEnum.IDLE)
                {
                    ccRet.ErrorCode = EventCodeEnum.NONE;

                    foreach (var v in PreSafetyList)
                    {
                        LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Start {v.GetType().Name}.");

                        ccRet = await v.Run();

                        LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] End {v.GetType().Name}.");

                        if (ccRet != null && ccRet.ErrorCode != EventCodeEnum.NONE)
                        {
                            break;
                        }
                    }

                    if (ccRet != null && ccRet.ErrorCode == EventCodeEnum.NONE)
                    {
                        SequenceBehaviorStateTransition(new SequenceSafetyPreValidState());
                    }
                    else
                    {
                        SequenceBehaviorStateTransition(new SequenceSafetyPreInvalidState());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ccRet;
        }

        public async Task<IBehaviorResult> BehaviorRun()
        {
            IBehaviorResult ccRet = new BehaviorResult();
            try
            {
                ISequenceRunner SequenceRunner = this.SequenceRunner();

                if (BehaviorState.GetState() == SequenceBehaviorStateEnum.PRE_SAFETY_VALID)
                {
                    SequenceBehaviorStateTransition(new SequenceBehaviorRunningState());

                    ccRet.ErrorCode = EventCodeEnum.NONE;

                    LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Start {_Behavior.GetType().Name}.");

                    ccRet = await _Behavior.Run();

                    LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] End {_Behavior.GetType().Name}.");

                    if (ccRet != null && ccRet.ErrorCode == EventCodeEnum.NONE)
                    {
                        SequenceBehaviorStateTransition(new SepenceBehaviorDoneState());
                    }
                    else
                    {
                        SequenceBehaviorStateTransition(new SequenceBehaviorErrorState());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ccRet;
        }

        public async Task<IBehaviorResult> PostSafetyRun()
        {
            IBehaviorResult ccRet = new BehaviorResult();
            try
            {
                ISequenceRunner SequenceRunner = this.SequenceRunner();

                if (BehaviorState.GetState() == SequenceBehaviorStateEnum.DONE)
                {
                    ccRet.ErrorCode = EventCodeEnum.NONE;

                    foreach (var v in PostSafetyList)
                    {
                        LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] Start {_Behavior.GetType().Name}.");

                        ccRet = await v.Run();

                        LoggerManager.Debug($"[{SequenceRunner.GetSequenceRunnerStateEnum().ToString()}] End {_Behavior.GetType().Name}.");

                        if (ccRet != null && ccRet.ErrorCode != EventCodeEnum.NONE)
                        {
                            break;
                        }
                    }

                    if (ccRet != null && ccRet.ErrorCode == EventCodeEnum.NONE)
                    {
                        SequenceBehaviorStateTransition(new SequenceBehaviorClearState());
                    }
                    else
                    {
                        SequenceBehaviorStateTransition(new SequenceSafetyPostInvalidState());
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return ccRet;
        }
        #endregion

        #region << ToString & Clone >>
        public override string ToString()
        {
            string retString = this.Behavior.description;
            try
            {
                if (retString == null || retString == "")
                    retString = this.Behavior.GetType().Name.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retString;
        }

        public static T Clone<T>(T source)
        {
            T t;
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                t = (T)formatter.Deserialize(stream);
            }
            return t;
        }

        public static explicit operator BehaviorGroupItem(ObservableCollection<ISequenceBehaviorGroupItem> v)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [Serializable]
    public class END_Behavior : SequenceBehavior, IEndBehavior
    {
        public END_Behavior()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(END_Behavior);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"Behavior: {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                retVal.ErrorCode = EventCodeEnum.BEHAVIOR_END;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while END_Behavior Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    abstract public class SequenceBehavior : ISequenceBehavior, ISequenceBehaviorRun
    {
        private bool _isRecoverySeq = false;
        public bool isRecoverySeq
        {
            get { return _isRecoverySeq; }
            set
            {
                _isRecoverySeq = value;
                RaisePropertyChanged();
            }
        }

        //[XmlIgnore, JsonIgnore]
        public ISequenceBehavior ReverseBehavior { get; set; }
        
        private List<IOPortDescripter<bool>> _OutputPorts
            = new List<IOPortDescripter<bool>>();
        [XmlIgnore, JsonIgnore]
        public List<IOPortDescripter<bool>> OutputPorts
        {
            get { return _OutputPorts; }
            set { _OutputPorts = value; }
        }

        private List<IOPortDescripter<bool>> _InputPorts
            = new List<IOPortDescripter<bool>>();
        [XmlIgnore, JsonIgnore]
        public List<IOPortDescripter<bool>> InputPorts
        {
            get { return _InputPorts; }
            set { _InputPorts = value; }
        }

        private BehaviorFlag _Flag;
        public BehaviorFlag Flag
        {
            get { return _Flag; }
            set
            {
                _Flag = value;
                RaisePropertyChanged();
            }
        }
        [XmlIgnore, JsonIgnore]
        public string _SequenceDescription;
        public string SequenceDescription
        {
            get { return _SequenceDescription; }
            set
            {
                _SequenceDescription = value;
                RaisePropertyChanged();
            }
        }

        //IsExistCard()함수에서 Card Doking 유무만 판단하고 Log는 안찍어도 되기 때문에 IsExistCard()함수 내에서 쓰는 Behavior클래스는 Log_Flag사용 
        private bool _log_Flag = true;
        public bool log_Flag
        {
            get { return _log_Flag; }
            set
            {
                _log_Flag = value;
                RaisePropertyChanged();
            }
        }
        [XmlIgnore, JsonIgnore]
        public string description { get; set; }

        [XmlIgnore, JsonIgnore]
        public bool branchValue = false;

        protected string _BehaviorID;
        public string BehaviorID
        {
            get { return _BehaviorID; }
            set { _BehaviorID = value; }
        }
        [XmlIgnore, JsonIgnore]
        protected string _NextID_Positive;
        public string NextID_Positive
        {
            get { return _NextID_Positive; }
            set
            {
                _NextID_Positive = value;
                RaisePropertyChanged();
            }
        }
        [XmlIgnore, JsonIgnore]
        protected string _NextID_Negative;
        public string NextID_Negative
        {
            get { return _NextID_Negative; }
            set
            {
                _NextID_Negative = value;
                RaisePropertyChanged();
            }
        }

        [XmlIgnore, JsonIgnore]
        public long WaitingTimeRemain { get; set; }

        public virtual int InitModule()
        {
            int retVal = -1;
            try
            {
                SetReverseBehavior();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region << ToString & Clone >>
        public override string ToString()
        {
            string retString = this.description;
            try
            {
                if (retString == null || retString == "")
                    retString = this.GetType().Name.ToString();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retString;
        }

        public static T Clone<T>(T source)
        {
            T t;
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                t = (T)formatter.Deserialize(stream);
            }
            return t;
        }
        #endregion

        public virtual void SetReverseBehavior()
        {
            try
            {
                if (this.ReverseBehavior != null)
                {
                    this.ReverseBehavior.ReverseBehavior = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public abstract Task<IBehaviorResult> Run();

        public virtual EventCodeEnum InitRun()
        {
            return EventCodeEnum.UNDEFINED;
        }

        protected ICardChangeSysParam GetSysParam()
        {
            ICardChangeSysParam ccParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
            return ccParam;
        }
        //public abstract Task<IBehaviorResult> SetPreSquenceList_Run()
        //{
        //    IC
        //}

        protected string IOErrorDescription(IOPortDescripter<bool> io, bool targetLevel, string errorclass)
        {
            string retVal = "";
            retVal = $"Error occured while monitor for io of {io.Description} \n" +
                        $"Target Level : {targetLevel}\n" +
                        $"Current Level : {io.Value}\n" +
                        $"Maintain Time: {io.MaintainTime.Value}\n" +
                        $"TimeOut: {io.TimeOut.Value}\n" +
                        $"Error Behavior class name : {errorclass.GetType().Name}\n";
            return retVal;
        }

        protected ICardChangeDevParam GetDevParam()
        {
            ICardChangeDevParam ccParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
            return ccParam;
        }

        protected EventCodeEnum IOResultValidation(object funcname, IORet ioret)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            if (ioret != IORet.NO_ERR)
            {
                LoggerManager.Debug($"ResultValidate Fail :  Error code = {ioret.ToString()}, fucntion name = {funcname.ToString()}");
            }
            else
            {
                ret = EventCodeEnum.NONE;
            }

            return ret;
        }
        protected EventCodeEnum IntResultValidation(object funcname, int intret)
        {
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            if (intret != 0)
            {
                LoggerManager.Debug($"ResultValidate Fail :  Error code = {intret.ToString()}, fucntion name = {funcname.ToString()}");
            }
            else
            {
                ret = EventCodeEnum.NONE;
            }

            return ret;
        }

        #region ==> Common Function
        protected async Task<int> WaitForCLAMPLock(bool bShowMsgBox = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLP_LOCK");
                IOPortDescripter<bool> DICLP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DICLP_UNLOCK");
                IOPortDescripter<bool> DICLP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool clp_Lock = false, clp_Unlock = false;

                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DICLP_LOCK, out clp_Lock);
                        this.IOManager().IOServ.ReadBit(DICLP_UNLOCK, out clp_Unlock);

                        if (clp_Lock == true && clp_Unlock == false)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (ccDevParam.CLAMPLock_Timeout.Value < difTime)
                        {
                            IsLoop = false;
                            TMO_EXIT = true;
                            if (bShowMsgBox == true)
                            {
                                StringBuilder sb = new StringBuilder();

                                sb.Append("Timeout=Waiting CLAMP Lock! It is over ");
                                sb.Append(ccDevParam.CLAMPLock_Timeout.Value / 1000);
                                sb.Append(" sec!");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");
                            }
                        }
                        WaitingTimeRemain = ccDevParam.CLAMPLock_Timeout.Value - difTime;
                    }
                });

                await t;

                retVal = 0;
                WaitingTimeRemain = 0;

                if (TMO_EXIT)
                {
                    retVal = 1;
                    WaitingTimeRemain = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitForCLAMPUnlock(bool bShowMsgBox = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                bool IsLoop = true;
                bool TMO_EXIT = false;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLP_LOCK");
                IOPortDescripter<bool> DICLP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DICLP_UNLOCK");
                IOPortDescripter<bool> DICLP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool clp_Lock = false, clp_Unlock = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DICLP_LOCK, out clp_Lock);
                        this.IOManager().IOServ.ReadBit(DICLP_UNLOCK, out clp_Unlock);

                        if (clp_Lock == false && clp_Unlock == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (ccDevParam.CLAMPLock_Timeout.Value < difTime)
                        {
                            IsLoop = false;
                            TMO_EXIT = true;
                            if (bShowMsgBox == true)
                            {
                                StringBuilder sb = new StringBuilder();

                                sb.Append("Timeout=Waiting CLAMP Unlock! It is over ");
                                sb.Append(ccDevParam.CLAMPLock_Timeout.Value / 1000);
                                sb.Append(" sec!");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");
                            }
                        }
                        WaitingTimeRemain = ccDevParam.CLAMPLock_Timeout.Value - difTime;
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitZIFLock(bool bshowMsg = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                long mZIFLockTimeout;
                bool IsLoop = true;
                bool TMO_EXIT = false;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                mZIFLockTimeout = ccDevParam.ZIFLock_Timeout.Value;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool zif_unlock = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out zif_unlock);
                        if (zif_unlock == false)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (mZIFLockTimeout < difTime)
                        {
                            if (bshowMsg == true)
                            {
                                StringBuilder sb = new StringBuilder();
                                IsLoop = false;

                                sb.Append("Timeout=Waiting ZIF Lock 60sec");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");

                                //VBTRACE "Timeout=Waiting ZIF Lock 60sec"        '-- Fixed by Yang 151228
                                //GoTo TMO_EXIT
                            }
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitZIFUnlock(bool bshowMsg = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                long mZIFLockTimeout;
                bool IsLoop = true;
                bool TMO_EXIT = false;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                mZIFLockTimeout = ccDevParam.ZIFLock_Timeout.Value;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool zif_unlock = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out zif_unlock);
                        if (zif_unlock == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (mZIFLockTimeout < difTime)
                        {
                            if (bshowMsg == true)
                            {
                                StringBuilder sb = new StringBuilder();
                                IsLoop = false;

                                sb.Append("Timeout=Waiting ZIF Unlock 60sec");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");

                                //VBTRACE "Timeout=Waiting ZIF Lock 60sec"        '-- Fixed by Yang 151228
                                //GoTo TMO_EXIT
                            }
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitZIFUnlockType2(bool bshowMsg = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                //long mZIFLockTimeout;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DIZIF_LOCK");
                IOPortDescripter<bool> DIZIF_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                bool zif_unlock = false, zif_lock = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out zif_unlock);
                        this.IOManager().IOServ.ReadBit(DIZIF_LOCK, out zif_lock);
                        if (zif_unlock == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (30000 < difTime)
                        {
                            if (bshowMsg == true)
                            {
                                StringBuilder sb = new StringBuilder();
                                IsLoop = false;

                                sb.Append("Timeout=Waiting ZIF Unlock 30sec");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");
                                //VBTRACE "Timeout=Waiting ZIF Lock 60sec"        '-- Fixed by Yang 151228
                                //GoTo TMO_EXIT
                            }
                        }
                        WaitingTimeRemain = 30000 - difTime;
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected bool IsFrontDoorOpenEx(bool bshowMsg = true)
        {
            bool retVal = false;
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIFRONTDOORCLOSE");
                IOPortDescripter<bool> DIFRONTDOORCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DIFRONTDOOROPEN");
                IOPortDescripter<bool> DIFRONTDOOROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                bool bDIFRONTDOORCLOSE = false, bDIFRONTDOOROPEN = false;

                this.IOManager().IOServ.ReadBit(DIFRONTDOORCLOSE, out bDIFRONTDOORCLOSE);

                if (bDIFRONTDOORCLOSE == false)
                {
                    if (ccDevParam.FrontDoorOpenSensorAttached.Value == true)
                    {
                        this.IOManager().IOServ.ReadBit(DIFRONTDOOROPEN, out bDIFRONTDOOROPEN);
                        if (bDIFRONTDOOROPEN == true)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                else
                {
                    retVal = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task ZIFUNLockRequest(bool bShowResultMsg = true)
        {
            try
            {
                int delay1;
                int delay2;
                //VBTRACE "modPCM TPLockRequest() - start"
                //delay1 = c_Int(GetIniItem(OPUS_INI, "PulseWidth", "UserDefine", "0", True))
                //delay2 = c_Int(GetIniItem(OPUS_INI, "DelayForCheck", "UserDefine", "0", True))
                delay1 = 0;
                delay2 = 0;

                int userdefine = 0;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();
                //c_Int(GetIniItem(OPUS_INI, "ZIFTYPE", "UserDefine", "0", True))

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                _type = this.IOManager().IO.Outputs.GetType();
                _propertyInfo = _type.GetProperty("DOZIF_LOCK_REQ");
                IOPortDescripter<bool> DOZIF_LOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);

                _propertyInfo = _type.GetProperty("DOZIF_UNLOCK_REQ");
                IOPortDescripter<bool> DOZIF_UNLOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);

                if (userdefine == 2)
                {
                    if (ccDevParam.BlZIFoutputMode.Value == true)
                    {
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                        System.Threading.Thread.Sleep(delay1);
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                        System.Threading.Thread.Sleep(delay2);
                    }
                    else
                    {
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                        System.Threading.Thread.Sleep(delay1);
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                        System.Threading.Thread.Sleep(delay2);
                    }
                }
                else
                {
                    int YOKOGAWA = 0;
                    //c_Int(GetIniItem(OPUS_INI, "ZIFTYPE", "UserDefine", "0", True))

                    if (YOKOGAWA == 0)
                    {
                        if (ccDevParam.BlZIFoutputMode.Value == true)
                        {
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                            System.Threading.Thread.Sleep(200);
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                            System.Threading.Thread.Sleep(200);
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        if (ccDevParam.ZIFSEQTYPE.Value == 0)
                        {
                            if (ccDevParam.BlZIFoutputMode.Value == true)
                            {
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                                System.Threading.Thread.Sleep(delay1);
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                                System.Threading.Thread.Sleep(15000);
                            }
                            else
                            {
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                                System.Threading.Thread.Sleep(delay1);
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                                System.Threading.Thread.Sleep(15000);
                            }
                        }
                        else if (ccDevParam.ZIFSEQTYPE.Value == 1)
                        {
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                            this.IOManager().IOServ.WriteBit(DOZIF_UNLOCK_REQ, false);
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }

                bool bDIZIF_UNLOCK = false;
                this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);
                if (bDIZIF_UNLOCK == false)
                {
                    if (ccDevParam.ZIFSEQTYPE.Value == 0)
                    {
                        if (await WaitZIFUnlock(bShowResultMsg) == 1)
                        {
                            //VBTRACE "Time out!! Wait ZunlockIF "
                            //ZIFUNLockRequest = 1
                            //Exit Function
                        }
                    }
                    else
                    {
                        if (await WaitZIFUnlockType2(bShowResultMsg) == 1)
                        {
                            //VBTRACE "Time out!! Wait ZunlockIF "
                            //ZIFUNLockRequest = 1
                            //Exit Function
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        protected async Task<int> ZIFLockRequest(bool bCheckZIFunlock = true)
        {
            int retVal = 0;
            try
            {
                int delay1;
                int delay2;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();
                //VBTRACE "modPCM TPLockRequest() - start"
                //delay1 = c_Int(GetIniItem(OPUS_INI, "ZIFTYPE", "PulseWidth", "200", True))
                //delay2 = c_Int(GetIniItem(OPUS_INI, "ZIFTYPE", "DelayForCheck", "1000", True))
                delay1 = 200;
                delay2 = 1000;

                int userdefine = 0;
                //c_Int(GetIniItem(OPUS_INI, "ZIFTYPE", "UserDefine", "0", True))

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                _propertyInfo = _type.GetProperty("DIZIF_LOCK");
                IOPortDescripter<bool> DIZIF_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                _type = this.IOManager().IO.Outputs.GetType();
                _propertyInfo = _type.GetProperty("DOZIF_LOCK_REQ");
                IOPortDescripter<bool> DOZIF_LOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);

                _propertyInfo = _type.GetProperty("DOZIF_UNLOCK_REQ");
                IOPortDescripter<bool> DOZIF_UNLOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);

                if (userdefine == 2)
                {
                    if (ccDevParam.BlZIFoutputMode.Value == true)
                    {
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                        System.Threading.Thread.Sleep(delay1);
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                        System.Threading.Thread.Sleep(delay2);
                    }
                    else
                    {
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                        System.Threading.Thread.Sleep(delay1);
                        this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                        System.Threading.Thread.Sleep(delay2);
                    }
                }
                else
                {
                    int YOKOGAWA = 0;
                    //c_Int(GetIniItem(OPUS_INI, "ZIFTYPE", "UserDefine", "0", True))

                    if (YOKOGAWA == 0)
                    {
                        if (ccDevParam.BlZIFoutputMode.Value == true)
                        {
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                            System.Threading.Thread.Sleep(delay1);
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                            System.Threading.Thread.Sleep(delay2);
                        }
                        else
                        {
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                            System.Threading.Thread.Sleep(delay1);
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                            System.Threading.Thread.Sleep(delay2);
                        }
                    }
                    else
                    {
                        if (ccDevParam.ZIFSEQTYPE.Value == 0)
                        {
                            if (ccDevParam.BlZIFoutputMode.Value == true)
                            {
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                                System.Threading.Thread.Sleep(delay1);
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                                System.Threading.Thread.Sleep(15000);
                            }
                            else
                            {
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                                System.Threading.Thread.Sleep(delay1);
                                this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                                System.Threading.Thread.Sleep(15000);
                            }
                        }
                        else if (ccDevParam.ZIFSEQTYPE.Value == 1)
                        {
                            this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, true);
                            this.IOManager().IOServ.WriteBit(DOZIF_UNLOCK_REQ, true);
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }

                bool bDIZIF_UNLOCK = false, bDIZIF_LOCK = false;
                if (bCheckZIFunlock == true)
                {
                    if (ccDevParam.ZIFSEQTYPE.Value == 0)
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);
                        if (bDIZIF_UNLOCK == true)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("ZIF Unlock state!\r\n");
                            sb.Append("Ok : Check ZIF Lock\r\n");
                            sb.Append("Cancel : No card in prober");

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative);

                            switch (result)
                            {
                                case EnumMessageDialogResult.AFFIRMATIVE:
                                    if (await WaitZIFLock() == 1)
                                    {
                                        //VBTRACE "Time out!! Wait ZIF lock"
                                        this.IOManager().IOServ.WriteBit(DOZIF_UNLOCK_REQ, true);
                                        retVal = 1;
                                        return retVal;
                                    }
                                    System.Threading.Thread.Sleep(500);
                                    break;
                                case EnumMessageDialogResult.NEGATIVE:
                                    System.Threading.Thread.Sleep(500);
                                    retVal = 0;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);
                        this.IOManager().IOServ.ReadBit(DIZIF_LOCK, out bDIZIF_LOCK);

                        if (bDIZIF_UNLOCK == true || bDIZIF_LOCK == false)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("ZIF Unlock state!\r\n");
                            sb.Append("Ok : Check ZIF Lock\r\n");
                            sb.Append("Cancel : No card in prober");

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative);

                            switch (result)
                            {
                                case EnumMessageDialogResult.AFFIRMATIVE:
                                    if (await WaitZIFLockType2() == 1)
                                    {
                                        //VBTRACE "Time out!! Wait ZIF lock"
                                        retVal = 1;
                                        return retVal;
                                    }
                                    System.Threading.Thread.Sleep(500);
                                    this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                                    this.IOManager().IOServ.WriteBit(DOZIF_UNLOCK_REQ, true);
                                    break;
                                case EnumMessageDialogResult.NEGATIVE:
                                    System.Threading.Thread.Sleep(500);
                                    this.IOManager().IOServ.WriteBit(DOZIF_LOCK_REQ, false);
                                    this.IOManager().IOServ.WriteBit(DOZIF_UNLOCK_REQ, true);

                                    retVal = 0;
                                    break;
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> MoveCCRotatorUp(bool bflag = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                long mZIFLockTimeout;
                bool IsLoop = true;
                bool TMO_EXIT = false;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                mZIFLockTimeout = ccDevParam.ZIFLock_Timeout.Value; // default => 60000

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool zif_unlock = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out zif_unlock);
                        if (zif_unlock == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (mZIFLockTimeout < difTime)
                        {
                            if (bflag == true)
                            {
                                StringBuilder sb = new StringBuilder();
                                IsLoop = false;

                                sb.Append("Timeout=Waiting ZIF Unlock 60sec");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative);

                                //VBTRACE "Timeout=Waiting ZIF Lock 60sec"        '-- Fixed by Yang 151228
                                //GoTo TMO_EXIT
                            }
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitCardHolderTubOpen()
        {
            int retVal = 0;
            try
            {
                long tstart;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_TUB_CLOSE");
                IOPortDescripter<bool> DICH_TUB_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DICH_TUB_OPEN");
                IOPortDescripter<bool> DICH_TUB_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool bDICH_TUB_CLOSE = false, bDICH_TUB_OPEN = false;

                InputPorts.Add(DICH_TUB_CLOSE);
                InputPorts.Add(DICH_TUB_OPEN);

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DICH_TUB_CLOSE, out bDICH_TUB_CLOSE);
                        this.IOManager().IOServ.ReadBit(DICH_TUB_OPEN, out bDICH_TUB_OPEN);

                        if (bDICH_TUB_CLOSE == false && bDICH_TUB_OPEN == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (60000 < difTime)
                        {
                            StringBuilder sb = new StringBuilder();
                            IsLoop = false;

                            sb.Append("TIMEOUT=waiting card holder tub !OPEN! 60sec");

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");

                            TMO_EXIT = true;
                            //VBTRACE "Timeout=Waiting ZIF Lock 60sec"        '-- Fixed by Yang 151228
                            //GoTo TMO_EXIT
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitCardHolderTubClose()
        {
            int retVal = 0;
            try
            {
                long tstart;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_TUB_CLOSE");
                IOPortDescripter<bool> DICH_TUB_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DICH_TUB_OPEN");
                IOPortDescripter<bool> DICH_TUB_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool bDICH_TUB_CLOSE = false, bDICH_TUB_OPEN = false;

                InputPorts.Add(DICH_TUB_CLOSE);
                InputPorts.Add(DICH_TUB_OPEN);

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DICH_TUB_CLOSE, out bDICH_TUB_CLOSE);
                        this.IOManager().IOServ.ReadBit(DICH_TUB_OPEN, out bDICH_TUB_OPEN);

                        if (bDICH_TUB_CLOSE == true && bDICH_TUB_OPEN == false)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (60000 < difTime)
                        {
                            StringBuilder sb = new StringBuilder();
                            IsLoop = false;

                            sb.Append("Timeout=Waiting Card Holder Tub !Close! 60sec");

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");

                            TMO_EXIT = true;
                            //VBTRACE "Timeout=Waiting ZIF Lock 60sec"        '-- Fixed by Yang 151228
                            //GoTo TMO_EXIT
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitForTPLock(bool bShowMsgBox = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DITP_LOCK");
                IOPortDescripter<bool> DITP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DITP_UNLOCK");
                IOPortDescripter<bool> DITP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                bool bDITP_LOCK = false, bDITP_UNLOCK = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DITP_LOCK, out bDITP_LOCK);
                        this.IOManager().IOServ.ReadBit(DITP_UNLOCK, out bDITP_UNLOCK);


                        if (bDITP_LOCK == true && bDITP_UNLOCK == false)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (ccDevParam.TPLock_Timeout.Value < difTime)
                        {
                            IsLoop = false;
                            TMO_EXIT = true;
                            if (bShowMsgBox == true)
                            {
                                StringBuilder sb = new StringBuilder();

                                sb.Append("Timeout=Waiting TP Lock! It is over ");
                                sb.Append(ccDevParam.TPLock_Timeout.Value / 1000);
                                sb.Append(" sec!");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");
                            }
                        }
                        WaitingTimeRemain = ccDevParam.TPLock_Timeout.Value - difTime;
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitForTPUNLock(bool bShowMsgBox = true)
        {
            int retVal = 0;
            try
            {
                long tstart;
                bool IsLoop = true;
                bool TMO_EXIT = false;
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DITP_LOCK");
                IOPortDescripter<bool> DITP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DITP_UNLOCK");
                IOPortDescripter<bool> DITP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                bool bDITP_LOCK = false, bDITP_UNLOCK = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DITP_LOCK, out bDITP_LOCK);
                        this.IOManager().IOServ.ReadBit(DITP_UNLOCK, out bDITP_UNLOCK);

                        if (bDITP_LOCK == false && bDITP_UNLOCK == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (ccDevParam.TPLock_Timeout.Value < difTime)
                        {
                            IsLoop = false;
                            TMO_EXIT = true;
                            if (bShowMsgBox == true)
                            {
                                StringBuilder sb = new StringBuilder();

                                sb.Append("Timeout=Waiting TP Unlock! It is over ");
                                sb.Append(ccDevParam.TPLock_Timeout.Value / 1000);
                                sb.Append(" sec!");

                                EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");
                            }
                        }
                        WaitingTimeRemain = ccDevParam.TPLock_Timeout.Value - difTime;
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public async Task<int> CLAMPLockRequest(bool bWait = true)
        {
            int retVal = 0;
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                Type _type = this.IOManager().IO.Outputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DOCLP_LOCK_REQ");
                IOPortDescripter<bool> DOCLP_LOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);

                _propertyInfo = _type.GetProperty("DOCLP_UNLOCK_REQ");
                IOPortDescripter<bool> DOCLP_UNLOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                if (ccDevParam.CLPLKSEQTYPE.Value == 0)
                {
                    this.IOManager().IOServ.WriteBit(DOCLP_LOCK_REQ, false);
                    System.Threading.Thread.Sleep(1000);
                    this.IOManager().IOServ.WriteBit(DOCLP_LOCK_REQ, true);
                    System.Threading.Thread.Sleep(5000);

                    if (bWait == true)
                    {
                        if (await WaitForCLAMPLock() == 1)
                        {
                            retVal = 1;
                            //VBTRACE "modPCM CLAMPLockRequest() - failed"
                            //Exit Function
                        }
                    }
                }
                else
                {
                    this.IOManager().IOServ.WriteBit(DOCLP_LOCK_REQ, true);
                    this.IOManager().IOServ.WriteBit(DOCLP_UNLOCK_REQ, false);
                    System.Threading.Thread.Sleep(5000);

                    if (bWait == true)
                    {
                        if (await WaitForCLAMPLock() == 1)
                        {
                            retVal = 1;
                            //VBTRACE "modPCM CLAMPLockRequest() - failed"
                            //Exit Function
                        }
                    }
                }

                retVal = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> CLAMPUNLockRequest(bool bWait = true)
        {
            int retVal = 0;
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                Type _type = this.IOManager().IO.Outputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DOCLP_LOCK_REQ");
                IOPortDescripter<bool> DOCLP_LOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);

                _propertyInfo = _type.GetProperty("DOCLP_UNLOCK_REQ");
                IOPortDescripter<bool> DOCLP_UNLOCK_REQ = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                if (ccDevParam.CLPLKSEQTYPE.Value == 0)
                {
                    this.IOManager().IOServ.WriteBit(DOCLP_UNLOCK_REQ, false);
                    System.Threading.Thread.Sleep(1000);
                    this.IOManager().IOServ.WriteBit(DOCLP_UNLOCK_REQ, true);
                    System.Threading.Thread.Sleep(5000);

                    if (bWait == true)
                    {
                        if (await WaitForCLAMPUnlock() == 1)
                        {
                            retVal = 1;
                            //VBTRACE "modPCM CLAMPLockRequest() - failed"
                            //Exit Function
                        }
                    }
                }
                else
                {
                    this.IOManager().IOServ.WriteBit(DOCLP_LOCK_REQ, false);
                    this.IOManager().IOServ.WriteBit(DOCLP_UNLOCK_REQ, true);
                    System.Threading.Thread.Sleep(5000);

                    if (bWait == true)
                    {
                        if (await WaitForCLAMPUnlock() == 1)
                        {
                            retVal = 1;
                            //VBTRACE "modPCM CLAMPLockRequest() - failed"
                            //Exit Function
                        }
                    }
                }

                retVal = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitForCLAMPLock()
        {
            int retVal = 0;
            try
            {
                long tstart;
                long gDbWaitForCLAMPLock = 60000;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLP_LOCK");
                IOPortDescripter<bool> DICLP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DICLP_UNLOCK");
                IOPortDescripter<bool> DICLP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool bDICLP_LOCK = false, bDICLP_UNLOCK = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DICLP_LOCK, out bDICLP_LOCK);
                        this.IOManager().IOServ.ReadBit(DICLP_UNLOCK, out bDICLP_UNLOCK);

                        if (bDICLP_LOCK == true && bDICLP_UNLOCK == false)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (gDbWaitForCLAMPLock < difTime)
                        {
                            StringBuilder sb = new StringBuilder();
                            IsLoop = false;

                            sb.Append("Timeout=Waiting CLAMP Lock! It is over ");
                            sb.Append(gDbWaitForCLAMPLock / 1000);
                            sb.Append(" sec!");

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");

                            TMO_EXIT = true;
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        protected async Task<int> WaitZIFLockType2()
        {
            int retVal = 0;
            try
            {
                long tstart;
                long gDbWaitForCLAMPLock = 60000;
                bool IsLoop = true;
                bool TMO_EXIT = false;

                tstart = DateTime.Now.Ticks / 10000;

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DIZIF_LOCK");
                IOPortDescripter<bool> DIZIF_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                bool bDIZIF_UNLOCK = false, bDIZIF_LOCK = false;

                Task t = Task.Run(async () =>
                {
                    while (IsLoop)
                    {
                        this.IOManager().IOServ.ReadBit(DIZIF_LOCK, out bDIZIF_LOCK);
                        this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);

                        if (bDIZIF_UNLOCK == false && bDIZIF_LOCK == true)
                        {
                            IsLoop = false;
                            continue;
                        }

                        long difTime = (DateTime.Now.Ticks / 10000) - tstart;
                        if (gDbWaitForCLAMPLock < difTime)
                        {
                            StringBuilder sb = new StringBuilder();
                            IsLoop = false;

                            sb.Append("Timeout=Waiting CLAMP Lock! It is over ");
                            sb.Append(gDbWaitForCLAMPLock / 1000);
                            sb.Append(" sec!");

                            EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Message", sb.ToString(), EnumMessageStyle.Affirmative, "YES", "NO");

                            TMO_EXIT = true;
                        }
                    }
                });

                await t;

                if (TMO_EXIT)
                    retVal = 1;
                else
                    retVal = 0;
                WaitingTimeRemain = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool IsFrontDoorOpenEx()
        {
            bool retVal = false;
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIFRONTDOORCLOSE");
                IOPortDescripter<bool> DIFRONTDOORCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DIFRONTDOOROPEN");
                IOPortDescripter<bool> DIFRONTDOOROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                bool frontdoorClose = false, frontdoorOpen = false;
                this.IOManager().IOServ.ReadBit(DIFRONTDOORCLOSE, out frontdoorClose);
                this.IOManager().IOServ.ReadBit(DIFRONTDOOROPEN, out frontdoorOpen);

                if (frontdoorClose == false)
                {
                    if (ccDevParam.FrontDoorOpenSensorAttached.Value == true)
                    {
                        if (frontdoorOpen == true)
                            retVal = true;
                        else
                            retVal = false;
                    }
                    else
                        retVal = true;
                }
                else
                    retVal = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    #region CardChangeBehaviorClass
    [Serializable]
    public class DockingSequence : SequenceBehavior
    {
        public DockingSequence()
        {
            this.Flag = BehaviorFlag.DOCK;
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = new UndockingSequence();

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class UndockingSequence : SequenceBehavior
    {
        public UndockingSequence()
        {
            this.Flag = BehaviorFlag.UNDOCK;
        }
        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = new DockingSequence();

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Select_Load_Unload : SequenceBehavior
    {
        public Select_Load_Unload()
        {
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Select_Dock_Undock : SequenceBehavior
    {
        public Select_Dock_Undock()
        {
        }
        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class CC_ASK_CHANGE_TO_LOAD_SEQUENCE : SequenceBehavior
    {
        public CC_ASK_CHANGE_TO_LOAD_SEQUENCE()
        {
            this.description = "ASK CHANGE TO LOAD SEQUENCE";
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class CC_CHECK_CARDCHANGE_DONE : SequenceBehavior
    {
        public CC_CHECK_CARDCHANGE_DONE()
        {
            this.description = "CHECK CARDCHANGE DONE";
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class OnlyCheck_TUBUp : SequenceBehavior
    {
        public OnlyCheck_TUBUp()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Only Check TUB Up";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class OnlyCheck_SwingDown : SequenceBehavior
    {
        public OnlyCheck_SwingDown()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Only Check Swing Up";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class OnlyCheck_CardMoveOutDone : SequenceBehavior
    {
        public OnlyCheck_CardMoveOutDone()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Only Check Card Move Out Done";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class OnlyCheck_CardHolderDown : SequenceBehavior
    {
        public OnlyCheck_CardHolderDown()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Only Check Card Holder Down";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_ClampLock : SequenceBehavior
    {
        public Check_ClampLock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check ClampLock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_ClampUnlock : SequenceBehavior
    {
        public Check_ClampUnlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check ClampUnlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_FrontDoorClose : SequenceBehavior
    {
        public Check_FrontDoorClose()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check FrontDoor Close";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = new Check_FrontDoorOpen();

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_FrontDoorOpen : SequenceBehavior
    {
        public Check_FrontDoorOpen()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check FrontDoorOpen";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = new Check_FrontDoorClose();

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_RemoveCardHolerOnPlate : SequenceBehavior
    {
        public Check_RemoveCardHolerOnPlate()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check Remove Card Holer On Plate";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_PutCardHolerOnPlate : SequenceBehavior
    {
        public Check_PutCardHolerOnPlate()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check Put Card Holer On Plate";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TesterHeadDockingDone : SequenceBehavior
    {
        public Check_TesterHeadDockingDone()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check Tester Head Docking Done";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TesterHeadUndockingDone : SequenceBehavior
    {
        public Check_TesterHeadUndockingDone()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check Tester Head Undocking Done";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TesterHorisontalPos : SequenceBehavior
    {
        public Check_TesterHorisontalPos()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check Tester Horisontal Pos";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TPLock : SequenceBehavior
    {
        public Check_TPLock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check TP Lock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TPUnlock : SequenceBehavior
    {
        public Check_TPUnlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check TP Unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TUBDown : SequenceBehavior
    {
        public Check_TUBDown()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check TUB Down";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_TUBUp : SequenceBehavior
    {
        public Check_TUBUp()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check TUB Up";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class CC_CHECK_ZIF_LOCK : SequenceBehavior
    {
        public CC_CHECK_ZIF_LOCK()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "CHECK ZIF LOCK";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Check_ZIF_Unlock : SequenceBehavior
    {
        public Check_ZIF_Unlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Check ZIF Unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_AutoSwingDownPlate : SequenceBehavior
    {
        public Do_AutoSwingDownPlate()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Auto Swing Down Plate";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_AutoSwingUpPlate : SequenceBehavior
    {
        public Do_AutoSwingUpPlate()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Auto Swing Up Plate";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_CarrierDown : SequenceBehavior
    {
        public Do_CarrierDown()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Carrier Down";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_CarrierUp : SequenceBehavior
    {
        public Do_CarrierUp()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Carrier Up";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_ManipulatorLock : SequenceBehavior
    {
        public Do_ManipulatorLock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Manipulator Lock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_ManipulatorUnlock : SequenceBehavior
    {
        public Do_ManipulatorUnlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Manipulator Unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_MoveToCarrierDownPos : SequenceBehavior
    {
        public Do_MoveToCarrierDownPos()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Move To Carrier Down Pos";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_PlateMoveIN : SequenceBehavior
    {
        public Do_PlateMoveIN()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Plate Move IN";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_PlateMoveOut : SequenceBehavior
    {
        public Do_PlateMoveOut()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Plate Move Out";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_CardHolderTubDown : SequenceBehavior
    {
        public Do_CardHolderTubDown()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Card Holder Tub Down";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Do_CardHolderTubUp : SequenceBehavior
    {
        public Do_CardHolderTubUp()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Do Card Holder Tub Up";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Request_Clamplock : SequenceBehavior
    {
        public Request_Clamplock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request Clamplock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Request_ClampUnlock : SequenceBehavior
    {
        public Request_ClampUnlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request Clamp Unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Request_TPLOCK : SequenceBehavior
    {
        public Request_TPLOCK()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request TP LOCK";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }

        private async Task<int> TPLockRequest(bool bWait = true)
        {
            int retVal = 0;
            try
            {
                //VBTRACE "modPCM TPLockRequest() - start"
                IOPortDescripter<bool> DOTP_LOCK_REQ = InputPorts.Find(io => io.Key.Value.Equals("DOTP_LOCK_REQ"));
                IOPortDescripter<bool> DOTP_UNLOCK_REQ = InputPorts.Find(io => io.Key.Value.Equals("DOTP_UNLOCK_REQ"));

                this.IOManager().IOServ.WriteBit(DOTP_LOCK_REQ, true);
                this.IOManager().IOServ.WriteBit(DOTP_UNLOCK_REQ, false);
                System.Threading.Thread.Sleep(5000);

                if (bWait == true)
                {
                    if (await WaitForTPLock() == 1)
                    {
                        retVal = 1;

                        //VBTRACE "modPCM TPLockRequest() - failed"
                        //Exit Function
                    }
                    else
                    {
                        retVal = 0;
                    }
                }
                else
                    retVal = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class Request_TPUNLOCK : SequenceBehavior
    {
        public Request_TPUNLOCK()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request TP UNLOCK";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }

        private async Task<int> TPUNLockRequest(bool bWait = true)
        {
            int retVal = 0;
            try
            {
                //VBTRACE "modPCM TPLockRequest() - start"

                IOPortDescripter<bool> DOTP_LOCK_REQ = InputPorts.Find(io => io.Key.Value.Equals("DOTP_LOCK_REQ"));
                IOPortDescripter<bool> DOTP_UNLOCK_REQ = InputPorts.Find(io => io.Key.Value.Equals("DOTP_UNLOCK_REQ"));

                this.IOManager().IOServ.WriteBit(DOTP_LOCK_REQ, false);
                this.IOManager().IOServ.WriteBit(DOTP_UNLOCK_REQ, true);

                System.Threading.Thread.Sleep(5000);

                if (bWait == true)
                {
                    if (await WaitForTPUNLock() == 1)
                    {
                        retVal = 1;

                        //VBTRACE "modPCM TPLockRequest() - failed"
                        //Exit Function
                    }
                    else
                    {
                        retVal = 0;
                    }
                }
                else
                    retVal = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
    [Serializable]
    public class Request_ZIFunlock : SequenceBehavior
    {
        public Request_ZIFunlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request ZIF unlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }

    }


    [Serializable]
    public class TesterHeadRotLock : SequenceBehavior
    {
        public TesterHeadRotLock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "TesterHeadRotLock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Delay:{(int)this.IOManager().IO.Outputs.DOTESTER_HEAD_LOCK.MaintainTime.Value}");
                retVal.ErrorCode = EventCodeEnum.NONE;

                if (this.IOManager().IO.Outputs.DOTESTER_HEAD_LOCK.IOOveride.Value == EnumIOOverride.NONE)
                {
                    LoggerManager.Debug($"Zif lock action affect pin position. break pinalign, pinpadmatch state.");

                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                }


                IORet ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_HEAD_LOCK, true);// active pulse
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_HEAD_UNLOCK, false);// active pulse
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                Thread.Sleep((int)this.IOManager().IO.Outputs.DOTESTER_HEAD_LOCK.MaintainTime.Value);// maintain

                
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class TesterHeadRotUnlock : SequenceBehavior
    {
        public TesterHeadRotUnlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "TesterHeadRotUnlock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Delay:{(int)this.IOManager().IO.Outputs.DOTESTER_HEAD_UNLOCK.MaintainTime.Value}");
                retVal.ErrorCode = EventCodeEnum.NONE;

                if (this.IOManager().IO.Outputs.DOTESTER_HEAD_UNLOCK.IOOveride.Value == EnumIOOverride.NONE)
                {
                    LoggerManager.Debug($"Zif lock action affect pin position. break pinalign, pinpadmatch state.");

                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                }


                IORet ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_HEAD_UNLOCK, true);// active pulse
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_HEAD_LOCK, false);// active pulse
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                Thread.Sleep((int)this.IOManager().IO.Outputs.DOTESTER_HEAD_UNLOCK.MaintainTime.Value);// maintain


                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class Request_ZIFCommandLowActive : SequenceBehavior
    {
        public Request_ZIFCommandLowActive()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request_ZIFCommandLowActive";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Delay:{(int)this.IOManager().IO.Outputs.DOZIF_LOCK_REQ.MaintainTime.Value}");
                retVal.ErrorCode = EventCodeEnum.NONE;

                if (this.IOManager().IO.Outputs.DOZIF_LOCK_REQ.IOOveride.Value == EnumIOOverride.NONE)
                {
                    LoggerManager.Debug($"Zif lock action affect pin position. break pinalign, pinpadmatch state.");

                    this.StageSupervisor().ProbeCardInfo.SetAlignState(AlignStateEnum.IDLE);
                    this.StageSupervisor().ProbeCardInfo.SetPinPadAlignState(AlignStateEnum.IDLE);
                }

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IORet ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOZIF_LOCK_REQ, true);// active pulse

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                Thread.Sleep((int)this.IOManager().IO.Outputs.DOZIF_LOCK_REQ.MaintainTime.Value);// delay를 위해서 MaintainTime을 사용했음.

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOZIF_LOCK_REQ, false);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                Thread.Sleep(5000);// 테스터가 유효한 Input값을 보낼때까지 4~5초가 걸림. 파라미터화하기.
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class Request_ZIFlock : SequenceBehavior
    {
        public Request_ZIFlock()
        {
            try
            {
                this.Flag = BehaviorFlag.NONE;
                this.description = "Request ZIF lock";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override void SetReverseBehavior()
        {
            try
            {
                ReverseBehavior = null;

                base.SetReverseBehavior();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                retVal.ErrorCode = EventCodeEnum.NONE;

                System.Threading.Thread.Sleep(2000);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    #endregion
    #endregion

    #region ==> Group Prober Card Change Behavior

    /*
     * PCardPod(Prober Card Pod) : Chuck 옆에 Prober Card를 올릴 수 있는 지지대
     * -> Up
     * -> Down
     * 
     * TopPlateSol(Top Plate Solenoid) : 상판의 Solenoid Locker, Pogo에 진공 흡착된 Card의 낙하 방지용
     */

    #region ==> Check
    //==> Prober Card가 Card Pod에 있는지 확인
    [Serializable]
    public class GP_CheckPCardIsOnPCardPod : SequenceBehavior
    {
        public GP_CheckPCardIsOnPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardIsOnPCardPod);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool diupmodule_cardexist_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool diupmodule_vacu_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out diupmodule_vacu_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //retVal.ErrorCode = EventCodeEnum.NONE;
                if ((diupmodule_cardexist_sensor == true) && (diupmodule_vacu_sensor == true))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardCahnge_NOT_EXIST_CARD_ON_CARD_POD;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Card Exist sensor : {diupmodule_cardexist_sensor}, Vacu sensor : {diupmodule_vacu_sensor}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardIsOnPCardPod Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card가 Card Pod에 없는지 확인
    [Serializable]
    public class GP_CheckPCardIsNotOnPCardPod : SequenceBehavior
    {
        public GP_CheckPCardIsNotOnPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardIsNotOnPCardPod);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //retVal.ErrorCode = EventCodeEnum.NONE;

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                var ccSysParam = (this.CardChangeModule().CcSysParams_IParam) as ICardChangeSysParam;

                // CardPod 베큠을 켜고 체크를 함.
                var iowrtret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                if (iowrtret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //bool diupmodule_cardexist_sensor;
                var eioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, false, 1000, 15000);
                if (eioret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    //diupmodule_cardexist_sensor = false;
                }

                //bool diupmodule_vacu_sensor;
                var ioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 15000);
                if (ioret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    //diupmodule_vacu_sensor = false;
                }

                if ((eioret == 0) && (ioret == 0))
                {
                    // CardPod 카드가 없으면 카드팟 베큠을 끈다.
                    iowrtret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                    if (iowrtret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        Task.FromResult<IBehaviorResult>(retVal);
                    }

                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Card Exist sensor : {eioret}, Vacu sensor : {ioret}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardIsNotOnPCardPod Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod이 올려져 있는지 확인
    [Serializable]
    public class GP_CheckPCardPodIsUp : SequenceBehavior
    {
        public GP_CheckPCardPodIsUp()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardPodIsUp);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool diupmodule_left_sensor;
                //var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_DOWN_STATUS;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                bool diupmodule_right_sensor;
                //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_DOWN_STATUS;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}


                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    diupmodule_left_sensor = true;
                }

                intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    diupmodule_right_sensor = true;
                }




                if (diupmodule_left_sensor && diupmodule_right_sensor)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_DOWN_STATUS;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : 'true' is up, Left : {diupmodule_left_sensor}, Right : {diupmodule_right_sensor}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardPodIsUp Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod이 내려져 있는지 확인
    [Serializable]
    public class GP_CheckPCardPodIsDown : SequenceBehavior
    {
        public GP_CheckPCardPodIsDown()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardPodIsDown);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool diupmodule_left_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool diupmodule_right_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (diupmodule_left_sensor == false && diupmodule_right_sensor == false)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : 'false' is down, Left -> {diupmodule_left_sensor}, Right -> {diupmodule_right_sensor}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardPodIsDown Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card 가 Pogo에 있는지 확인
    [Serializable]
    public class GP_CheckPCardIsOnPogo : SequenceBehavior
    {
        public GP_CheckPCardIsOnPogo()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardIsOnPogo);
        }
        public GP_CheckPCardIsOnPogo(bool writeLog) : this()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardIsOnPogo);
            this.log_Flag = writeLog;
        }

        public override Task<IBehaviorResult> Run()
        {
            if (this.log_Flag == true)
            {
                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");
            }

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool dipogocard_vacu_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (dipogocard_vacu_sensor == false)
                {
                    if (this.log_Flag == true)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Pogo Card Vacu");
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO;
                    }
                }
                else
                {
                    if (this.log_Flag == true)
                    {
                        LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} Done");
                    }
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardIsOnPogo Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card가 Pogo에 없는지 확인
    [Serializable]
    public class GP_CheckPCardIsNotOnPogo : SequenceBehavior
    {
        public GP_CheckPCardIsNotOnPogo()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardIsNotOnPogo);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool dipogocard_vacu_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_TOUCHED_ON_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (dipogocard_vacu_sensor == true)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_TOUCHED_ON_POGO;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Pogo Card Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardIsNotOnPogo Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> 상판의 낙하 방지용 Locker가 Lock 되어 있는지 확인
    [Serializable]
    public class GP_CheckTopPlateSolIsLock : SequenceBehavior
    {
        public GP_CheckTopPlateSolIsLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckTopPlateSolIsLock);
        }

        public GP_CheckTopPlateSolIsLock(bool writeLog) : this()
        {
            this.log_Flag = writeLog;
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckTopPlateSolIsLock);
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool ditplate_pclatch_sensor_lock;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool ditplate_pclatch_sensor_unlock;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var iomonitorret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 100, 500, this.log_Flag);

                if (iomonitorret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    ditplate_pclatch_sensor_lock = true;
                }


                iomonitorret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 100, 500, this.log_Flag);

                if (iomonitorret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    ditplate_pclatch_sensor_unlock = false;
                }

                if ((ditplate_pclatch_sensor_lock == true) && (ditplate_pclatch_sensor_unlock == false))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    if (log_Flag == true)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Lock sensor : {ditplate_pclatch_sensor_lock}, Unlock sensor : {ditplate_pclatch_sensor_unlock}");
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckTopPlateSolIsLock Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> 상판의 낙하 방지용 Locker가 Unlock 되어 있는지 확인
    [Serializable]
    public class GP_CheckTopPlateSolIsUnLock : SequenceBehavior
    {
        public GP_CheckTopPlateSolIsUnLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckTopPlateSolIsUnLock);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // TODO : 현재 이 Behavior는 Unlock이 아닌 경우, Unlock을 시키고 다시 확인 하는 로직이기 때문에, ReadBit 후, 바로 에러코드를 리턴해서는 안된다.
                bool ditplate_pclatch_sensor_lock;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_LOCKED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                bool ditplate_pclatch_sensor_unlock;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_LOCKED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                var iomonitorret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 100, 500);

                if (iomonitorret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    ditplate_pclatch_sensor_lock = false;
                }


                iomonitorret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 100, 500);

                if (iomonitorret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    ditplate_pclatch_sensor_unlock = true;
                }

                // 사실상 Retry Code
                if (ditplate_pclatch_sensor_lock && ditplate_pclatch_sensor_unlock == false)
                {
                    // WriteBit가 이상한 경우, 에러 코드 리턴.
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_LOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    iomonitorret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 100, 500);

                    if (iomonitorret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        ditplate_pclatch_sensor_lock = false;
                    }


                    iomonitorret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 100, 500);

                    if (iomonitorret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        ditplate_pclatch_sensor_unlock = true;
                    }

                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 15000);
                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 15000);

                    //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                    //if (ioret != IORet.NO_ERR)
                    //{
                    //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_LOCKED;
                    //    return Task.FromResult<IBehaviorResult>(retVal);
                    //}

                    //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                    //if (ioret != IORet.NO_ERR)
                    //{
                    //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_LOCKED;
                    //    return Task.FromResult<IBehaviorResult>(retVal);
                    //}
                }

                // OK
                if ((ditplate_pclatch_sensor_lock == false) && (ditplate_pclatch_sensor_unlock == true))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_LOCKED;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Lock sensor : {ditplate_pclatch_sensor_lock}, Unlock sensor : {ditplate_pclatch_sensor_unlock}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckTopPlateSolIsUnLock Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Tester가 상판에 잘 Docking 되어 있는지 확인
    [Serializable]
    public class GP_CheckTesterIsOnTopPlate : SequenceBehavior
    {
        public GP_CheckTesterIsOnTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckTesterIsOnTopPlate);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                //bool ditplatein_sensor;
                //this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
                var ditplateinRet = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, true, 7000);
                if (ditplateinRet != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //bool ditester_docking_sensor;
                //this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);
                var ditestdocksensorRet = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, true);
                if (ditestdocksensorRet != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                if (cardChangeParam.GPTesterVacSeqSkip == false)
                {
                    //bool dipogotester_vacu_sensor = false;
                    //var dipogotestervacRet = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
                    var dipogotestervacRet = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, true, 7000);
                    if (dipogotestervacRet != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    // OK
                    if ((ditplateinRet == 0) && (ditestdocksensorRet == 0) && (dipogotestervacRet == 0))
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                    else
                    {
                        bool ditplatein_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
                        bool ditester_docking_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);
                        bool dipogotester_vacu_sensor = false;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Top Plate In : {ditplatein_sensor},  Docking : {ditester_docking_sensor}, Tester vacuum sensor : {dipogotester_vacu_sensor}");
                    }
                }
                else
                {
                    if ((ditplateinRet == 0) && (ditestdocksensorRet == 0))
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                    else
                    {
                        bool ditplatein_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
                        bool ditester_docking_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Top Plate In : {ditplatein_sensor},  Docking : {ditester_docking_sensor}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckTesterIsOnTopPlate Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card가 상판에 걸려 있는지 확인 : Card가 Pogo에 흡착은 떨어지고 Top Plate Locker 에 걸려 있는 상태
    [Serializable]
    public class GP_CheckPCardIsStuckInTopPlate : SequenceBehavior
    {
        public GP_CheckPCardIsStuckInTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckPCardIsStuckInTopPlate);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;

                IBehaviorResult cardIsNotOnPogoResult = new BehaviorResult();
                command = new GP_CheckPCardIsOnPogo();//=> 상판에 Card가 흡착 안 되어 있는지 확인
                cardIsNotOnPogoResult = await command.Run();


                if (cardIsNotOnPogoResult.ErrorCode != EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_STUCK_IN_POGO;
                    return retVal;
                }


                IBehaviorResult topPlateSolIsLock = new BehaviorResult();
                command = new GP_CheckTopPlateSolIsLock();//=> 상판에 Card Sol Lock이 닫혀 있는지 확인
                topPlateSolIsLock = await command.Run();

                if (topPlateSolIsLock.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_STUCK_IN_POGO;
                    return retVal;
                }

                if (cardIsNotOnPogoResult.ErrorCode == EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO && topPlateSolIsLock.ErrorCode == EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"{this.GetType().Name} : Card is not stuck in Pogo");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_STUCK_IN_POGO;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardIsStuckInTopPlate Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    //==> Card가 CardPod에 존재하는지 체크
    [Serializable]
    public class GP_CheckCardOnCardPod : SequenceBehavior
    {
        public GP_CheckCardOnCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckCardOnCardPod);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //SequenceBehavior command = null;

                bool diupmodule_left_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);

                bool diupmodule_right_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);

                bool diupmodule_vacu_sensor;
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);

                bool diupmodule_touch_left;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, out diupmodule_touch_left);

                bool diupmodule_touch_right;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, out diupmodule_touch_right);

                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacu_sensor write error");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_RECOVERY_FAIL);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    System.Threading.Thread.Sleep(2500);
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out diupmodule_vacu_sensor);
                    if (ioret != IORet.NO_ERR)
                    {
                        LoggerManager.Error($"[GP CC]=> {this.GetType().Name} diupmodule_card_vac_sensor read error");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_RECOVERY_FAIL);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    if (diupmodule_vacu_sensor == false)
                    {
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                        if (ioret != IORet.NO_ERR)
                        {
                            LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacu_sensor write error");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_RECOVERY_FAIL);
                            return Task.FromResult<IBehaviorResult>(retVal);
                        }
                    }

                }
                bool diupmodule_cardexist_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);

                if (diupmodule_left_sensor && diupmodule_right_sensor && (diupmodule_cardexist_sensor || diupmodule_vacu_sensor))
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card Exists(Upmodule)");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (diupmodule_cardexist_sensor)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card Exists(Upmodule)");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if ((diupmodule_left_sensor || diupmodule_right_sensor) &&
                    (diupmodule_cardexist_sensor == false) && (diupmodule_vacu_sensor == false) &&
                    (diupmodule_touch_left == false) && (diupmodule_touch_right == false))
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardCahnge_NOT_EXIST_CARD_ON_CARD_POD;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card pod raised status(NOT EXIST CARD ON CARD POD)");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (diupmodule_left_sensor || diupmodule_right_sensor) // && (diupmodule_vacu_sensor == true))
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card pod raised status");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err, $"Error Occured while GP_CheckCardOnCardPod Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card가 Stage에 존재 하는지 확인
    [Serializable]
    public class GP_CheckCardIsInStage : SequenceBehavior
    {
        public GP_CheckCardIsInStage()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckCardIsInStage);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //SequenceBehavior command = null;

                // TODO : 스테이지에 카드가 존재하는 조건이 3가지 존재하고 있으며, 이 함수에서 3가지 조건을 다 보게끔 되어 있음.
                // 추후에는, 각 조건별 구분을 통해 분리하고, 로더쪽에서 조합된 무언가를 쓰도록 변경할 것.
                // 따라서, IO 리턴 결과로 에러코드를 즉시 리턴하면 안됨.
                //==> Check Card is on upmodule
                bool diupmodule_left_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChnage_NOT_EXIST_CARD_IN_STAGE;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                bool diupmodule_right_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChnage_NOT_EXIST_CARD_IN_STAGE;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                bool diupmodule_cardexist_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChnage_NOT_EXIST_CARD_IN_STAGE;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (diupmodule_left_sensor && diupmodule_right_sensor && diupmodule_cardexist_sensor)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CHECK_TO_CARD_UP_MOUDLE;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card Exists(Upmodule)");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Check Card is on Pogo
                bool dipogocard_vacu_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);

                if (dipogocard_vacu_sensor == true)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card Exists(Pogo)");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Check Card is stuck in Pogo
                bool ditplate_pclatch_sensor_lock;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                bool ditplate_pclatch_sensor_unlock;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);

                //==> Pogo vauum은 흡착 안 되어있는데 Card Lock은 걸려 있는 경우
                if (dipogocard_vacu_sensor == false && ditplate_pclatch_sensor_lock && ditplate_pclatch_sensor_unlock == false)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} : Card Exists(Pogo - stuck)");

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckPCardIsStuckInTopPlate Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    #endregion

    #region ==> Move
    //==> Chuck을 Loader 쪽으로 이동 시킴 : Card를 Loader로 부터 받기 위해
    [Serializable]
    public class GP_MoveChuckToLoader : SequenceBehavior
    {
        //==> Chuck이 Prober Card 받기 위해 Handler 쪽으로 접근
        public GP_MoveChuckToLoader()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_MoveChuckToLoader);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Get parameter");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                if (cardChangeParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    var handoffPos = cardChangeParam.CardTransferPos.Value;
                    var offsetX = cardChangeParam.CardTransferOffsetX.Value;
                    var offsetY = cardChangeParam.CardTransferOffsetY.Value;
                    var offsetZ = cardChangeParam.CardTransferOffsetZ.Value;
                    var offsetT = cardChangeParam.CardTransferOffsetT.Value;

                    var loadPosX = this.CoordinateManager().StageCoord.ChuckLoadingPosition.X.Value;
                    var loadPosY = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Y.Value;
                    var loadPosZ = 0;// this.CoordinateManager().StageCoord.ChuckLoadingPosition.Z.Value;
                    var loadPosT = 0d;
                    //loadPosZ = this.MotionManager().GetAxis(EnumAxisConstants.Z).Param.NegSWLimit.Value;
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.MoveLoadingOffsetPosition(
                        handoffPos.X.Value - loadPosX + offsetX,
                        handoffPos.Y.Value - loadPosY + offsetY,
                        //handoffPos.Z.Value - loadPosZ + offsetZ,
                        loadPosZ,
                        handoffPos.T.Value - loadPosT + offsetT);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move loading position");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        LoggerManager.Debug($"[Success] {this.GetType().Name} : Move Card loading position");
                    }
                }
                else
                {
                    //==> Move chuck to Loader
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.MoveLoadingPosition(0);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move loading position");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    //==> Rotate Theta
                    double theta = cardChangeParam.GP_LoadingPosT;
                    var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, theta * 10000,
                    //   axist.Param.Speed.Value, axist.Param.Acceleration.Value);
                    // TODO : AbsMove를 쓰면 안되지만, 당장 문제가 되지 않을 것 같아, 넣어놓은 임시방편이다. 추후에 바꿔야 하긴 함.
                    retVal.ErrorCode = this.MotionManager().AbsMove(EnumAxisConstants.C, theta * 10000);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Rotate Theta, T : {theta}");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_MoveChuckToLoader Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Chuck을 Pogo Contact 위치 까지 Z Up
    [Serializable]
    public class GP_DockRaiseChuckToPogo : SequenceBehavior
    {
        public GP_DockRaiseChuckToPogo()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_DockRaiseChuckToPogo);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GP_CheckPCardPodIsUp();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }


                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                // TODO : Task.Delay가 최선인지 확인 필요
                System.Threading.Thread.Sleep(3000);

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : System parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                LoggerManager.Debug($"GP_DockRaiseChuckToPogo(): CardContactPosZ = {cardChangeDevParam.GP_CardContactPosZ:0.00}, GP_ContactCorrectionZ = {cardChangeSysParam.GP_ContactCorrectionZ:0.00} ");

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeDevParam.GP_CardContactPosZ + cardChangeSysParam.GP_ContactCorrectionZ;

                LoggerManager.Debug($"GP_DockRaiseChuckToPogo(): Contact Pos = {zAbsPos:0.00} ");

                double bigZUpZPos = zAbsPos - 20000;

                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {zAbsPos}");
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return retVal;
                }

                //==> Big Z Up
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                // System.Threading.Thread.Sleep(500);

                //==> Contact Z Up to Pogo
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Contact Z Up, {zAbsPos}");
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                //    return retVal;
                //}

                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Small Z up, {smallZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;

                while (runflag)
                {
                    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;

                        LoggerManager.Debug("DIUPMODULE_LEFT_SENSOR off");

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        return retVal;
                    }

                    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;

                        LoggerManager.Debug("DIUPMODULE_RIGHT_SENSOR off");

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;

                        return retVal;
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;
                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if (apos >= zAbsPos)
                        {
                            runflag = false;
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value / 10, zaxis.Param.Acceleration.Value / 20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);

                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;

                        LoggerManager.Debug($"SW Limit Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");

                        runflag = false;

                        return retVal;
                    }
                    Thread.Sleep(2);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_DockRaiseChuckToPogo Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;

                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);

                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos:0.00}, {curYPos:0.00}, {curZPos:0.00}, {curTPos:0.00}");
            }

            return retVal;
        }
    }

    //==> Chuck을 Pogo Contact 위치 까지 Z Up
    [Serializable]
    public class GP_UnDockRaiseChuckToPogo : SequenceBehavior
    {
        public GP_UnDockRaiseChuckToPogo()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_UnDockRaiseChuckToPogo);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GP_CheckPCardPodIsUp();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;

                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.GP_Undock_ContactCorrectionZ + cardChangeSysParam.GP_Undock_CardContactPosZ;
                double bigZUpZPos = zAbsPos - 20000;

                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {zAbsPos}");
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return retVal;
                }

                //==> Big Z Up
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ////==> Contact Z Up to Pogo
                ////retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Contact Z Up, {bigZUpZPos}");
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                //    return retVal;
                //}

                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Small Z up, {smallZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;
                while (runflag)
                {
                    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;

                        LoggerManager.Debug("DIUPMODULE_LEFT_SENSOR off");

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;

                        return retVal;
                    }

                    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;

                        LoggerManager.Debug("DIUPMODULE_RIGHT_SENSOR off");

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;

                        return retVal;
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;

                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if (apos >= zAbsPos)
                        {
                            // 정상탈출
                            runflag = false;
                            retVal.ErrorCode = EventCodeEnum.NONE;

                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");

                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value / 10, zaxis.Param.Acceleration.Value / 20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);

                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;

                        LoggerManager.Debug($"SW Limit Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");

                        runflag = false;

                        return retVal;
                    }
                    Thread.Sleep(2);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_UnDockRaiseChuckToPogo Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);

                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }
            return retVal;
        }
    }

    //==> Chuck을 안전한 위치 까지 Z Down
    [Serializable]
    public class GP_DropChuckSafety : SequenceBehavior
    {
        public GP_DropChuckSafety()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_DropChuckSafety);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double curZPos = 0;
                this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curZPos);

                double ejectChuckPos = curZPos - 10000;

                //==> Eject chuck from Chuck
                if (ejectChuckPos > zAxis.Param.HomeOffset.Value)
                {
                    //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_SAFE_POSITION;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode}");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }

                //==> Chuck down to Home Offset
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, zAxis.Param.HomeOffset.Value, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_SAFE_POSITION;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Down to Home Offset, EventCode:{retVal.ErrorCode}");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move to SW Neg Limit");
                //    return retVal;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_DropChuckSafety Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Pogo에 붙여져 있는 Card를 내림, 안전한 위치 까지 Z Down
    [Serializable]
    public class GP_DropPCardSafety : SequenceBehavior
    {
        public GP_DropPCardSafety()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_DropPCardSafety);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double firstcurZpos = 0;
                double curZPos = 0;
                double ejectChuckPos = 0;
                bool DICARDCHANGEVACU;
                bool diCardExistOnPod;
                bool safeDownSuccess = false;
                this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref firstcurZpos);
                double limitPosMargin = 1200;
                for (int i = 0; i < 10; i++)
                {
                    ejectChuckPos = firstcurZpos - ((i + 1) * 500);
                    if (ejectChuckPos > zAxis.Param.HomeOffset.Value)
                    {
                        //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} retry count:{i}");
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                            return retVal;
                        }
                        LoggerManager.Error($"{this.GetType().Name} : CardPod Sensor Check Count:{i} Pre_Position :{ejectChuckPos}");

                    }
                    System.Threading.Thread.Sleep(2000);
                    var ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                    ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diCardExistOnPod);
                    LoggerManager.Error($"{this.GetType().Name} : CardPod Sensor Pre_Check Count:{i} CardExistPod:{diCardExistOnPod} , CardVacuumPod{DICARDCHANGEVACU}");
                    if (diCardExistOnPod == true && DICARDCHANGEVACU == true)
                    {
                        // 6번은 내려와야지 Card 가 내려왔다 라고 보기 위한 카운트 확인 코드.
                        // 추후에는 거리로 판단으로 수정 필요. (3mm)
                        if (i >= 6)
                        {
                            safeDownSuccess = true;
                            break;
                        }
                    }
                    else
                    {
                        if (i >= 9)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while undock that retry count overed. retry count:{i}");
                            ejectChuckPos = firstcurZpos;
                            retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);

                            var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                            //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                            if (ioretval != IORet.NO_ERR)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                return retVal;
                            }

                            var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                            if (intret != 0)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                return retVal;
                            }

                            intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                            if (intret != 0)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                return retVal;
                            }


                            var sollockCMD = new GP_CheckTopPlateSolIsLock();
                            retVal = await sollockCMD.Run();
                            if (retVal.ErrorCode != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                return retVal;
                            }
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);

                            ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                            if (ioretval != IORet.NO_ERR)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of PogoVacuum on Error in undock sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                return retVal;
                            }
                            intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);
                            if (intret != 0)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for DIPOGOCARD_VACU_SENSOR true Error in undock sequence");
                                ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                            }

                            this.StageSupervisor().StageModuleState.CCZCLEARED();

                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                            return retVal;
                        }
                        else
                        {
                            ejectChuckPos = firstcurZpos + (100 * (i + 1));
                            if (ejectChuckPos < firstcurZpos + limitPosMargin)
                            {
                                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                                if (retVal.ErrorCode != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while undock motion move error " +
                                        $"Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} retry count = {i}");
                                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                                    this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curZPos);

                                    if (curZPos >= firstcurZpos)
                                    {
                                        var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                                        //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                                        if (ioretval != IORet.NO_ERR)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                            return retVal;
                                        }

                                        var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                                        if (intret != 0)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                            return retVal;
                                        }

                                        intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                                        if (intret != 0)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                            return retVal;
                                        }


                                        var sollockCMD = new GP_CheckTopPlateSolIsLock();
                                        retVal = await sollockCMD.Run();
                                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                            return retVal;
                                        }
                                    }

                                    return retVal;
                                }

                                LoggerManager.Error($"{this.GetType().Name} : CardPod Sensor Check Count:{i} After_Position :{ejectChuckPos}");
                                ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                                ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diCardExistOnPod);
                                LoggerManager.Error($"{this.GetType().Name} : CardPod Sensor After_Check Count:{i} CardExistPod:{diCardExistOnPod} , CardVacuumPod{DICARDCHANGEVACU}");
                                if (diCardExistOnPod == true && DICARDCHANGEVACU == true)
                                {
                                    //카드가 흡착된걸 확인 
                                    //카드확인이 되더라도 내리면서 다시 확인해야 하기때문에 해당 위치에서는 별도의 동작없이 넘어감.
                                    //위로 올라가면 카드가 pod에 붙어도 실링에도 붙어서 안떨어질수 있기 때문에. 
                                }
                                else
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while undock that retry count overed. retry count:{i}");
                                    ejectChuckPos = firstcurZpos;
                                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);

                                    var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                                    //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                                    if (ioretval != IORet.NO_ERR)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }

                                    var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                                    if (intret != 0)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }

                                    intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                                    if (intret != 0)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }

                                    var sollockCMD = new GP_CheckTopPlateSolIsLock();
                                    retVal = await sollockCMD.Run();
                                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                        return retVal;
                                    }
                                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);

                                    ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                                    if (ioretval != IORet.NO_ERR)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of PogoVacuum on Error in undock sequence");
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }
                                    intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);
                                    if (intret != 0)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for DIPOGOCARD_VACU_SENSOR true Error in undock sequence");
                                        ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                                    }

                                    this.StageSupervisor().StageModuleState.CCZCLEARED();

                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                    return retVal;
                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} EjectChuckPos is Dangerous pos. EjectChuckPos:{ejectChuckPos} Eject Limit Pos:{firstcurZpos + limitPosMargin} retry count:{i}");
                                //LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} Down count:{downcount}");
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while undock that retry count overed. retry count:{i}");
                                ejectChuckPos = firstcurZpos;
                                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);

                                var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                                if (ioretval != IORet.NO_ERR)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                    return retVal;
                                }

                                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                                if (intret != 0)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                    return retVal;
                                }

                                intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                                if (intret != 0)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                    return retVal;
                                }

                                var sollockCMD = new GP_CheckTopPlateSolIsLock();
                                retVal = await sollockCMD.Run();
                                if (retVal.ErrorCode != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                    return retVal;
                                }
                                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);

                                ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                                if (ioretval != IORet.NO_ERR)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of PogoVacuum on Error in undock sequence");
                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                    return retVal;
                                }
                                intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);
                                if (intret != 0)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for DIPOGOCARD_VACU_SENSOR true Error in undock sequence");
                                    ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                                }

                                this.StageSupervisor().StageModuleState.CCZCLEARED();

                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                return retVal;
                            }
                        }
                    }
                }

                if (!safeDownSuccess)
                {
                    LoggerManager.Error($"[FAIL] SafeDown FAIL WHILE UNDOCK");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                    return retVal;
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                var command = new GP_CheckPCardIsOnPCardPod();//=> Prober Card Pod에 Prober Card가 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                bool isFirst = false;
                //10mm drop and check card 
                for (int i = 0; i < 3; i++)
                {
                    if (isFirst)
                    {
                        i = 0;
                        isFirst = false;
                    }
                    double downpos = firstcurZpos - ((i + 1) * 10000);
                    double chuckpos = 0;

                    LoggerManager.Debug($"chuck's going down position:{downpos}");

                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, downpos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} retry count:{i}");
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                        return retVal;
                    }
                    this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref chuckpos);

                    LoggerManager.Debug($"chuck down position:{chuckpos}");

                    this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                    this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diCardExistOnPod);

                    if (diCardExistOnPod == true && DICARDCHANGEVACU == true)
                    {
                        //카드가 흡착된걸 확인 
                    }
                    else
                    {
                        LoggerManager.Error($"Error occured while chuck down during undock CardExistPod:{diCardExistOnPod}, CardPodVac:{DICARDCHANGEVACU}");
                        ejectChuckPos = firstcurZpos;

                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);

                        var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);

                        if (ioretval != IORet.NO_ERR)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of PogoVacuum on Error in undock sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }
                        var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);
                        if (intret != 0)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for DIPOGOCARD_VACU_SENSOR true Error in undock sequence");
                            ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                        }

                        ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                        if (ioretval != IORet.NO_ERR)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }

                        intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                        if (intret != 0)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }

                        intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                        if (intret != 0)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }


                        var sollockCMD = new GP_CheckTopPlateSolIsLock();
                        retVal = await sollockCMD.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                            return retVal;
                        }

                        this.StageSupervisor().StageModuleState.CCZCLEARED();

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;


                        return retVal;
                    }
                    System.Threading.Thread.Sleep(1000);
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move to ZCleared");
                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_DropPCardSafety Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
            }
            return retVal;
        }
    }

    //==> CCObservation Page에서 사용 하는 Card Pod Up
    // CardAlignPos로 이동 후 CardPod UP
    [Serializable]
    public class GP_RaisePCardPodAfterMoveCardAlignPos : SequenceBehavior
    {
        public GP_RaisePCardPodAfterMoveCardAlignPos()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_RaisePCardPodAfterMoveCardAlignPos);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    return retVal;
                }

                if (cardChangeSysParam.CardAlignState == AlignStateEnum.DONE)
                {
                    SequenceBehavior command = new GP_MoveChuckToLoader();
                    retVal = await command.Run();

                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        IORet ioret = IORet.ERROR;
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, true);
                        if (ioret != IORet.NO_ERR)
                        {
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }

                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, false);
                        if (ioret != IORet.NO_ERR)
                        {
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }

                        var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 1000, 15000);
                        if (intret != 0)
                        {
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }

                        intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 1000, 15000);
                        if (intret != 0)
                        {
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                            return retVal;
                        }

                        command = new GP_CheckPCardPodIsUp();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                    }
                    else
                    {
                        LoggerManager.Debug("GP_RaisePCardPodAfterMoveCardAlignPos.GP_MoveChuckToLoader() Align Position Move Fail");
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_ALIGN_FAIL;
                    LoggerManager.Debug($"GP_RaisePCardPodAfterMoveCardAlignPos Fail. cardChangeSysParam.CardAlignState : {cardChangeSysParam.CardAlignState}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_RaisePCardPod Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    //==> Card Pod을 Up
    [Serializable]
    public class GP_RaisePCardPod : SequenceBehavior
    {
        public GP_RaisePCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_RaisePCardPod);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //SequenceBehavior command = new GP_CheckPCardIsNotOnPCardPod();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return retVal;
                //}
                #region Pod 올리기 전 척 위치 확인 로직

                bool isDangerousPosition = false;// True: Dagerous Position, False: Safety Position
                var zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double curZpos = zaxis.Status.Position.Ref;
                double safetyZPos = this.CoordinateManager().StageCoord.SafePosZAxis;
                double posZMargin = 100.0; // 기존 ThreeLeg움직이기 전 체크하는 함수(CheckHardwareInterference())에서 사용된 값.
                bool doCardPodUp;
                bool doCardPodDown;

                if (curZpos > (safetyZPos + posZMargin))
                {
                    //위험한 위치이지만 이미 Card Pod이 올라가 있다면 안전장치로 부른 것이므로 넘어가도록 함.
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, out doCardPodUp);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, out doCardPodDown);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    if (doCardPodUp == true && doCardPodDown == false)
                    {
                        isDangerousPosition = false;
                    }
                    else if (doCardPodUp == doCardPodDown)
                    {
                        // Up이 되어 있는 상태에서 장비 전원이 나간 경우.
                        // 복동식 솔레노이드인 CardPod Up, Down Output이 value가 같다는 건, Elmo가 Reset되어 출력이 0, 0으로 나가고 있는 상황이다.
                        // Input 센서 상으로는 Up상태이기 때문에 Up은 1, Down은 0을 한번 내줘야 한다.
                        isDangerousPosition = false;
                    }
                    else
                    {
                        isDangerousPosition = true;// Safe Pos보다 높고, 현재 Pod이 내려가 있다라고 했을 때에는 Pod Up하면 안되도록 함.
                    }
                }
                else
                {
                    isDangerousPosition = false;
                }
                #endregion

                if(isDangerousPosition == false)
                {
                    SequenceBehavior command;
                    IORet ioret = IORet.ERROR;
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, true);
                    //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, false);
                    //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 1000, 15000);
                    if (intret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 1000, 15000);
                    if (intret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    command = new GP_CheckPCardPodIsUp();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.MOTION_DANGEROUS_POS;
                    LoggerManager.Error($"GP_RaisePCardPod(): EventCode:{retVal.ErrorCode}, " +
                        $"Unable to engage Card POD actuator on out of loading scope. Current Chuck Position Z = {curZpos:0.00}, " +
                        $"SafePosAxis Value Z = {safetyZPos:0.00}, Margin Z = {posZMargin:0.00}");

                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_RaisePCardPod Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    //==> Card Pod을 Down
    [Serializable]
    public class GP_DropPCardPod : SequenceBehavior
    {
        public GP_DropPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_DropPCardPod);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, false);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, true);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                // System.Threading.Thread.Sleep(6000);

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, false, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                //intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, false, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                command = new GP_CheckPCardPodIsDown();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_DropPCardPod Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    //==> Card Pod을 Forced로 Down한다.
    // 물리적으로는 카드팟과 카드암의 카드가 닿아있지 않은 상태에서도 Exist 센서가 인식되어 Drop Pod을 못하는 경우 사용하기 위한 Behavior
    [Serializable]
    public class GP_ForcedDropPCardPod : SequenceBehavior
    {
        public GP_ForcedDropPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_ForcedDropPCardPod);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                // Pod 내려가는 시간.
                Thread.Sleep(1500);

                SequenceBehavior command = new GP_CheckPCardPodIsDown();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ForcedDropPCardPod Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }
    #endregion

    #region ==> Fall Protector

    [Serializable]
    public class GP_RecoveryPogoVacuumOff : SequenceBehavior
    {
        public GP_RecoveryPogoVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_TopPlateSolUnLock);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                System.Threading.Thread.Sleep(2000);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                System.Threading.Thread.Sleep(2000);

                bool DICARDCHANGEVACU;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (DICARDCHANGEVACU == false)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_RecoveryPogoVacuumOff Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Top Plate Locker 를 Lcok
    [Serializable]
    public class GP_TopPlateSolUnLock : SequenceBehavior
    {
        public GP_TopPlateSolUnLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_TopPlateSolUnLock);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GP_CheckPCardIsNotOnPogo();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, false);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                //intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                // System.Threading.Thread.Sleep(4000);

                command = new GP_CheckTopPlateSolIsUnLock();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                this.CardChangeModule().IsCardStuck = false;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_TopPlateSolUnLock Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    //==> Top Plate Locker 를 Unlcok
    [Serializable]
    public class GP_TopPlateSolLock : SequenceBehavior
    {
        public GP_TopPlateSolLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_TopPlateSolLock);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GP_CheckPCardIsOnPogo();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                //intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                // System.Threading.Thread.Sleep(4000);

                command = new GP_CheckTopPlateSolIsLock();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_TopPlateSolLock Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }
    #endregion

    #region ==> Vacuum
    //==> Card Pod Vacuum On
    [Serializable]
    public class GP_PCardPodVacuumOn : SequenceBehavior
    {
        public GP_PCardPodVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PCardPodVacuumOn);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                // System.Threading.Thread.Sleep(1000);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                if (ioret == IORet.NO_ERR && intret == 0)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                //var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                //bool DICARDCHANGEVACU;
                //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                //    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                //if (DICARDCHANGEVACU == false)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                //}
                //else
                //{
                //    retVal.ErrorCode = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_PCardPodVacuumOn Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod Vacuum Off
    [Serializable]
    public class GP_PCardPodVacuumOff : SequenceBehavior
    {
        public GP_PCardPodVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PCardPodVacuumOff);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);
                // System.Threading.Thread.Sleep(1000);

                bool DICARDCHANGEVACU;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (DICARDCHANGEVACU)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_OFF_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_PCardPodVacuumOff Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class GP_PCardPodUndockVacuumOn : SequenceBehavior
    {
        public GP_PCardPodUndockVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PCardPodUndockVacuumOn);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Error($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.GP_Undock_ContactCorrectionZ + cardChangeSysParam.GP_Undock_CardContactPosZ;

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                // System.Threading.Thread.Sleep(1000);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 5000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                    // return retVal;
                }

                //var intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 15000);
                //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                bool DICARDCHANGEVACU;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (DICARDCHANGEVACU == false)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        LoggerManager.Debug($"Undock Z up retry : {i + 1} / 5, ZPos:{zAbsPos}");

                        zAbsPos += 100;
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                        var intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 5000);
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                        intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 5000);

                        ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                        System.Threading.Thread.Sleep(500);
                        if (DICARDCHANGEVACU == true)
                        {
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            break;
                        }
                    }

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            zAbsPos -= 100;

                            LoggerManager.Debug($"Undock Z down retry {i + 1} / 10, ZPos:{zAbsPos}");

                            ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                            var intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 5000);
                            retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                            ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                            intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 5000);

                            ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                            System.Threading.Thread.Sleep(500);
                            if (DICARDCHANGEVACU == true)
                            {
                                retVal.ErrorCode = EventCodeEnum.NONE;
                                break;
                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_PCardPodVacuumOn Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Pogo에 Card가 접촉되는 면에 Vacuum On
    [Serializable]
    public class GP_PogoPCardContactVacuumOn : SequenceBehavior
    {
        public GP_PogoPCardContactVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PogoPCardContactVacuumOn);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    return retVal;
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeDevParam.GP_CardContactPosZ + cardChangeSysParam.GP_ContactCorrectionZ;


                //IORet writeioret = IORet.ERROR;

                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, DOPOGOCARD_VACU On");

                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);

                var intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 20000);
                if (intioret != 0)
                {
                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, DIPOGOCARD_VACU_SENSOR is false");
                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Retry POGOVacuum On Start (each Z 100um UP, Retry Total Count: 5)");

                    for (int i = 0; i < 5; i++)
                    {
                        zAbsPos += 100;

                        LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Retry POGOVacuum On Retry Count{i + 1}, Z Position:{zAbsPos}");

                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);

                        LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Writebit: DOPOGOCARD_VACU :false, DOPOGOCARD_VACU_RELEASE : true");

                        //System.Threading.Thread.Sleep(1000);
                        intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 1500);
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                        
                        LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Writebit: DOPOGOCARD_VACU :true, DOPOGOCARD_VACU_RELEASE : false");

                        intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);

                        if (intioret == 0)
                        {
                            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, DIPOGOCARD_VACU_SENSOR is On.");

                            break;
                        }
                    }

                    if (intioret != 0)
                    {
                        LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Retry POGOVacuum On Start (each Z 100um Down, Retry Total Count: 6)");

                        for (int i = 0; i < 6; i++)
                        {
                            zAbsPos -= 100;
                            
                            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Retry POGOVacuum On Retry Count{i + 1}, Z Position:{zAbsPos}");
                            
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                            
                            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Writebit: DOPOGOCARD_VACU :false, DOPOGOCARD_VACU_RELEASE : true");

                            intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 1500);
                            retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                            
                            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Writebit: DOPOGOCARD_VACU :true, DOPOGOCARD_VACU_RELEASE : false");

                            intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);

                            if (intioret == 0)
                            {
                                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, DIPOGOCARD_VACU_SENSOR is On.");
                                
                                break;
                            }
                        }
                    }
                }

                bool DIFOGOCARDVACU;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out DIFOGOCARDVACU);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                if (DIFOGOCARDVACU == false)
                {
                    //배큠 끄고 릴리즈 불어주고 내리는 시퀀스  
                    //문제임 개문제임 
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, false);

                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 1000, 15000);

                    if (intioret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 1000, 15000);
                    if (intioret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    System.Threading.Thread.Sleep(3000);

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    System.Threading.Thread.Sleep(3000);

                    intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 5000);
                    if (intioret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    //조금씩 내리면서 베큠이 카드팟에 베큠이 들어오는지 확인하는 시퀀스가 추가 되어야함.
                    // TODO : Check
                    zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    double firstcurZpos = 0;
                    double curZPos = 0;
                    double ejectChuckPos = 0;
                    double prevPos = 0;
                    bool DICARDCHANGEVACU;
                    bool diCardExistOnPod;
                    bool safeDownSuccess = false;
                    this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref firstcurZpos);
                    double limitPosMargin = 1200;

                    for (int i = 0; i < 10; i++)
                    {
                        //this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curZPos);
                        ejectChuckPos = firstcurZpos - ((i + 1) * 500);         //1000
                        prevPos = firstcurZpos - (i * 500);
                        if (ejectChuckPos > zAxis.Param.HomeOffset.Value)
                        {
                            //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                            retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                            if (retVal.ErrorCode != EventCodeEnum.NONE)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} retry count:{i}");
                                //  this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                                return retVal;
                            }
                        }
                        System.Threading.Thread.Sleep(2000);
                        var ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                        ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diCardExistOnPod);

                        if (diCardExistOnPod == true && DICARDCHANGEVACU == true)
                        {
                            LoggerManager.Error($"{this.GetType().Name} : Z Down Retry Count:{i}");
                            // 6번은 내려와야지 Card 가 내려왔다 라고 보기 위한 카운트 확인 코드.
                            // 추후에는 거리로 판단으로 수정 필요. (3mm)
                            if (i >= 6)
                            {
                                safeDownSuccess = true;
                                break;
                            }
                        }
                        else
                        {
                            //ejectChuckPos = firstcurZpos + (100 * (i + 1));
                            ejectChuckPos = prevPos;
                            if (ejectChuckPos < firstcurZpos + limitPosMargin)
                            {
                                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                                if (retVal.ErrorCode != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while undock motion move error " +
                                        $"Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} retry count = {i}");
                                    // this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                                    this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curZPos);

                                    if (curZPos >= firstcurZpos)
                                    {
                                        var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                                        //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                                        if (ioretval != IORet.NO_ERR)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                            return retVal;
                                        }

                                        var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                                        if (intret != 0)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                            return retVal;
                                        }

                                        intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                                        if (intret != 0)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                            return retVal;
                                        }


                                        var sollockCMD = new GP_CheckTopPlateSolIsLock();
                                        retVal = await sollockCMD.Run();
                                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                                        {
                                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                            return retVal;
                                        }
                                    }

                                    return retVal;
                                }

                                ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out DICARDCHANGEVACU);
                                ioreturn = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diCardExistOnPod);
                                if (diCardExistOnPod == true && DICARDCHANGEVACU == true)
                                {
                                    //카드가 흡착된걸 확인 
                                    //카드확인이 되더라도 내리면서 다시 확인해야 하기때문에 해당 위치에서는 별도의 동작없이 넘어감.
                                    //위로 올라가면 카드가 pod에 붙어도 실링에도 붙어서 안떨어질수 있기 때문에. 
                                }
                                else
                                {

                                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while undock that retry count overed. retry count:{i}");
                                    ejectChuckPos = firstcurZpos;
                                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);

                                    var ioretval = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, true);
                                    //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                                    if (ioretval != IORet.NO_ERR)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while IO operation of latch lock in undock sequence");
                                        this.StageSupervisor().MotionManager().DisableAxes();
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }

                                    var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000);
                                    if (intret != 0)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch lock in undock sequence");
                                        this.StageSupervisor().MotionManager().DisableAxes();
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }

                                    intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000);
                                    if (intret != 0)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of latch unlock in undeock sequence");
                                        this.StageSupervisor().MotionManager().DisableAxes();
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                                        return retVal;
                                    }


                                    var sollockCMD = new GP_CheckTopPlateSolIsLock();
                                    retVal = await sollockCMD.Run();
                                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                                    {
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while checking latch lock in undock sequence");
                                        this.StageSupervisor().MotionManager().DisableAxes();
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                        return retVal;
                                    }

                                    this.StageSupervisor().StageModuleState.CCZCLEARED();

                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                    return retVal;

                                }
                            }
                            else
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} EjectChuckPos is Dangerous pos. EjectChuckPos:{ejectChuckPos} Eject Limit Pos:{firstcurZpos + limitPosMargin} retry count:{i}");
                                //LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos} EventCode:{retVal.ErrorCode} Down count:{downcount}");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                //this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                                return retVal;
                            }

                        }

                    }
                    if (!safeDownSuccess)
                    {
                        LoggerManager.Error($"[FAIL] SafeDown FAIL WHILE ZDOWN");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_STUCK_IN_POGO;
                        return retVal;
                    }

                    // 카드팟에 카드가 계속 인식이 된채로 특정 포지션 까지 내린 후 Blow를 꺼준다.
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                        return retVal;
                    }

                    // TODO : Check

                    //await messageDialog.ShowDialog("POGO Vaccum error", "POGO Vaccum Sensor Not Detected", "OK", "Cancel", EnumMessageStyle.Affirmative);
                    await this.MetroDialogManager().ShowMessageDialog("POGO Vacuum error", "POGO Vacuum Sensor Not Detected", EnumMessageStyle.Affirmative, "OK", "Cancel");

                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_PogoPCardContactVacuumOn Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                // Contact 실패 했을 경우 다시 내리게 되는데 이 때 Blow를 불고 내린다.
                // 내리고 나서 Blow를 끄는 부분을 넣긴 했지만 혹시나 중간에 Return 되거나 예외가 발생했을 경우 Blow를 계속 분채로 냅두면 안되기 때문에
                // Finally에 처리함. 꺼져있는 상태에서 false로 또 끄는것은 문제 없음.
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                }
            }
            return retVal;
        }
    }



    [Serializable]
    public class GP_PogoPCardContactVacuumOn_Manual : SequenceBehavior
    {
        public GP_PogoPCardContactVacuumOn_Manual()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PogoPCardContactVacuumOn_Manual);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }


                //IORet writeioret = IORet.ERROR;

                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                var intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 20000);
                if (intioret != 0)
                {
                    await this.MetroDialogManager().ShowMessageDialog("POGO Vacuum error", "POGO Vacuum Sensor Not Detected", EnumMessageStyle.Affirmative, "OK", "Cancel");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    return retVal;
                }

                SequenceBehavior command = null;
                command = new GP_TesterClamped();//=> Tester Vac, Card Vac 상태를 보고 Clamped ON(Mani가 Tester를 가져갈 수 없음) 또는 UnClamped ON 시킴(Mani가 Tester를 가져갈 수 있음)
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_PogoPCardContactVacuumOn Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }

    //==> Pogo에 Card가 접촉되는 면에 Vacuum Off
    [Serializable]
    public class GP_PogoPCardContactVacuumOff : SequenceBehavior
    {
        public GP_PogoPCardContactVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PogoPCardContactVacuumOff);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;
                command = new GP_CheckPCardIsOnPCardPod();//=> Prober Card Pod에 Prober Card가 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    var existRet = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, true, 1000, 15000);

                    if (existRet != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    else
                    {
                        // Use exist sensor
                        LoggerManager.Debug($"GP_PogoPCardContactVacuumOff(): Use exist sensor. ");
                        
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                System.Threading.Thread.Sleep(1000);

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                System.Threading.Thread.Sleep(7000);

                //CardSubVac Off
                var cardSubVac = this.IOManager().IO.Outputs.DOPOGOCARD_VACU_SUB;

                if (cardSubVac.IOOveride.Value == EnumIOOverride.NONE)
                {
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_SUB, false);
                    if (ioret == IORet.NO_ERR)
                    {
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE_SUB, true);
                        Thread.Sleep(1000);
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE_SUB, false);
                    }
                }

                //      Move To when chuck down 
                //ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                //    return retVal;
                //}

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 15000);

                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 15000);

                bool dipogocard_vacu_sensor;

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                if (dipogocard_vacu_sensor == true)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_POGO_SIDE_VAC_OFF_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"Error Occured while GP_PogoPCardContactVacuumOff Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                }
            }
            return retVal;
        }
    }

    //==> 상판에 Tester가 접촉되는 면에 Vacuum On
    [Serializable]
    public class GP_UpPlateTesterContactVacuumOn : SequenceBehavior
    {
        public GP_UpPlateTesterContactVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_UpPlateTesterContactVacuumOn);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, true, 1000, 15000);

                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, true, 15000);

                // System.Threading.Thread.Sleep(1000);

                bool DIFOGOTESTERVACU;

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out DIFOGOTESTERVACU);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                if (DIFOGOTESTERVACU == false)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_ON_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

                if(retVal.ErrorCode == EventCodeEnum.NONE)
                {
                    SequenceBehavior command = null;
                    command = new GP_TesterClamped();//=> Tester Vac, Card Vac 상태를 보고 Clamped ON(Mani가 Tester를 가져갈 수 없음) 또는 UnClamped ON 시킴(Mani가 Tester를 가져갈 수 있음)
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_UpPlateTesterContactVacuumOn Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    //==> 상판에 Tester가 접촉되는 면에 Vacuum Off
    [Serializable]
    public class GP_UpPlateTesterContactVacuumOff : SequenceBehavior
    {
        public GP_UpPlateTesterContactVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_UpPlateTesterContactVacuumOff);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                    return retVal;
                }

                SequenceBehavior command = null;

                command = new GP_CheckPCardIsNotOnPogo();//=> POGO에 Card가 없는지 확인.
                retVal = await command.Run();

                if (retVal.ErrorCode == EventCodeEnum.NONE) // Card가 없다.
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    System.Threading.Thread.Sleep(1000);

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE, true);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    System.Threading.Thread.Sleep(6000);

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, false, 1000, 15000);
                    if (intret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, false, 15000);

                    bool dipogotester_vacu_sensor;

                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);

                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    if (dipogotester_vacu_sensor == true)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR;
                        LoggerManager.Error($"[GP_UpPlateTesterContactVacuumOff Fail] {this.GetType().Name} : Vacu");
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }

                    if (retVal.ErrorCode == EventCodeEnum.NONE)
                    {
                        command = new GP_TesterClamped();//=> Tester Vac, Card Vac 상태를 보고 Clamped ON(Mani가 Tester를 가져갈 수 없음) 또는 UnClamped ON 시킴(Mani가 Tester를 가져갈 수 있음)
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
                    }
                    else
                    {
                        LoggerManager.Debug($"[FAIL - dipogotester_vacu_sensor:true] TesterVacuumOff Fail : {retVal.ErrorCode}");
                    }
                }
                else if (retVal.ErrorCode == EventCodeEnum.GP_CardChange_CARD_IS_TOUCHED_ON_POGO)// Card가 있다.
                {
                    LoggerManager.Debug($"[FAIL - GP_CheckPCardIsNotOnPogo] TesterVacuumOff Fail : {retVal.ErrorCode}");
                }
                else
                {
                    LoggerManager.Debug($"[FAIL - GP_CheckPCardIsNotOnPogo] TesterVacuumOff Fail : {retVal.ErrorCode}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_UpPlateTesterContactVacuumOff Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }

    //==> 상판에 Tester가 접촉되는 면에 Purge Air Off
    [Serializable]
    public class GP_UpPlateTesterPurgeAirOff : SequenceBehavior
    {
        public GP_UpPlateTesterPurgeAirOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_UpPlateTesterPurgeAirOff);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_UpPlateTesterPurgeAirOff Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> 상판에 Tester가 접촉되는 면에 Purge Air On
    [Serializable]
    public class GP_UpPlateTesterPurgeAirOn : SequenceBehavior
    {
        public GP_UpPlateTesterPurgeAirOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_UpPlateTesterPurgeAirOn);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_UpPlateTesterPurgeAirOn Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod Output 0, 0 인 경우를 체크하고 그렇다면 현재 Input에 맞게 Output on
    [Serializable]
    public class GP_CheckCardPodOutputStatus : SequenceBehavior
    {
        public GP_CheckCardPodOutputStatus()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckCardPodOutputStatus);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            bool doCardPodUp;
            bool doCardPodDown;
            bool diCardPodUp_left;
            bool diCardPodUp_right;

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                SequenceBehavior command = null;
                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                // 카드팟 Up Down Output 상태 확인
                // Output Up, Down 모두 0, 0 인 경우 input 센서를 보고 그에 맞는 Output출력 내기.
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, out doCardPodUp);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, out doCardPodDown);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // 0, 0 or 1, 1 (복동식 솔레노이드 인 카드팟 output의 상태가 둘이 같은 경우는 Elmo가 리셋되었거나 Elmo에서 의도적으로 만든 경우 - 1,0 또는 0,1이 될 수 있게 output 출력을 내줘야 함.)
                if (doCardPodUp == doCardPodDown)
                {
                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Card pod output status is abnormal.  DOUPMODULE_UP : {doCardPodUp}, DOUPMODULE_DOWN : {doCardPodDown}");
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diCardPodUp_left);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diCardPodUp_right);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Card pod Up Sensor. DIUPMODULE_LEFT_SENSOR : {diCardPodUp_left}, DIUPMODULE_RIGHT_SENSOR : {diCardPodUp_right}");

                    if (diCardPodUp_left == true && diCardPodUp_right == true)
                    {
                        command = new GP_RaisePCardPod();//=> Card Pod Up
                        retVal = command.Run().Result;

                        if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return Task.FromResult<IBehaviorResult>(retVal); }
                    }
                    else if (diCardPodUp_left == false && diCardPodUp_right == false)
                    {
                        //// 파라미터에 저장된 마지막 상태가 카드가 카드팟 위에 있었다!
                        //if (cardChangeParam.IsCardExist && !this.CardChangeModule().IsExistCard())
                        //{
                        //    // Output이 0, 0인 상태이고, 카드팟이 올라가있지 않다고 판단이 되었지만 파라미터에 저장된 마지막 상태가 카드팟 위에 카드가 있다고 하면
                        //    // 혹시나 센서가 잘못 체크됐을 수 도 있으니 Drop Pod을 하지 않는다.
                        //    // 이 경우에는 Init Fail이 되고 직접 카드팟과 카드에 상태를 확인 후 매뉴얼로 팟을 올리던지 내리던지 해야 한다.
                        //    // IsCardExist는 현재 GP에서 set되고 있지 않음, GOP만 카드 로드시에 set해 줌.
                        //    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Card status. IsCardExist : {cardChangeParam.IsCardExist}, IsDocked : {cardChangeParam.IsDocked}");
                        //    if (cardChangeParam.CardChangeType.Value == EnumCardChangeType.CARRIER)
                        //    {
                        //        retVal.ErrorCode = EventCodeEnum.GP_CardChange_INIT_FAIL;
                        //        return Task.FromResult<IBehaviorResult>(retVal);
                        //    }
                        //    else
                        //    {
                        //        //direct card 방식에서는 iscardexist가 set된다는 것은 이상한 상황이다. 그냥 센서를 믿고 하위 로직에서 droppcardpot을 호출하도록 한다.
                        //    }
                        //}


                        //droppcardpod 내부에서 Card Pod Vac, Sensor 체크를 하고 있기 때문에 믿고 호출하도록 한다.
                        if (cardChangeParam.CardChangeType.Value == EnumCardChangeType.DIRECT_CARD)
                        {
                            command = new GP_DropPCardPod();//=> Card Pod 베큠 및 Exist 센서 체크 후 없으면 Down
                            retVal = command.Run().Result;
                        }
                        else if (cardChangeParam.CardChangeType.Value == EnumCardChangeType.CARRIER)
                        {
                            command = new GOP_DropPCardPod();//=> Card Pod 베큠 및 Exist 센서 체크 후 없으면 Down
                            retVal = command.Run().Result;
                        }

                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                            this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            return Task.FromResult<IBehaviorResult>(retVal);
                        }
                    }
                    else
                    {
                        //Invalid State (Left Up, Right Up 다른 상태)
                        LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Card Pod Invalid State. DIUPMODULE_LEFT_SENSOR : {diCardPodUp_left}, DIUPMODULE_RIGHT_SENSOR : {diCardPodUp_right}");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_INIT_FAIL;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                    LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}, Card pod output status is normal.  DOUPMODULE_UP : {doCardPodUp}, DOUPMODULE_DOWN : {doCardPodDown}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CheckCardPodOutputStatus Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    #endregion

    #region ==> Align
    //==> Align 수행
    [Serializable]
    public class GP_CardAlign : SequenceBehavior
    {
        public GP_CardAlign()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CardAlign);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Card Align
                EventCodeEnum result = this.GPCardAligner().Align();
                if (result != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = result;

                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CardAlign Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Chuck 위치를 Pogo에 Card Contact 한 위치(X,Y,T)로 이동
    [Serializable]
    public class GP_SetAlignPos : SequenceBehavior
    {
        public GP_SetAlignPos()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_SetAlignPos);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);

                }

                ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject tAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                //this.MotionManager().AbsMove(xAxis, cardChangeParam.GP_CardContactPosX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                //this.MotionManager().AbsMove(yAxis, cardChangeParam.GP_CardContactPosY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                //this.MotionManager().AbsMove(EnumAxisConstants.C, (cardChangeParam.GP_CardContactPosT));
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(xAxis, cardChangeSysParam.GP_Undock_CardContactPosX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(yAxis, cardChangeSysParam.GP_Undock_CardContactPosY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(tAxis, cardChangeSysParam.GP_Undock_CardContactPosT, tAxis.Param.Speed.Value, tAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_SetAlignPos Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    #endregion

    #region ==> Recovery
    //==> Card 가 Stuck 되어 있을때 Recovery
    public class GP_PCardSutckRecovery : SequenceBehavior
    {
        public GP_PCardSutckRecovery()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PCardSutckRecovery);
        }

        public override async Task<IBehaviorResult> Run()
        {
            //==> Tester의 홀더에 있는 Prober Card를 Undocking 시킨다.
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            bool cardVacError = false;
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[GP CC] Fail to Clear state");
                //    return retVal;
                //}

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }

                ////==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return retVal;
                //}

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();

                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                SequenceBehavior command = null;

                //<조건 검사>
                command = new GP_CheckPCardIsNotOnPCardPod();//=> Prober Card Pod에 Prober Card가 없는지 확인.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                command = new GP_CheckPCardIsStuckInTopPlate();//=> 상판에 Card가 걸려 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                command = new GP_CheckTesterIsOnTopPlate();//=> 상판에 Tester가 정상적으로 올려졌는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                //<전처리>
                command = new GP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                //<회수>
                command = new GP_RaisePCardPod();//=> Prober Card Pod 상승
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                command = new GP_CheckPCardPodIsUp();//==> Card pod이 올려져 있는지 다시 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                command = new GP_SetAlignPos();//=> Chuck을 이전에 Align한 위치로 이동 시킴
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                command = new GP_UnDockRaiseChuckToPogo();//=> Chuck을 상승 시켜서 Pogo에 부착 시킨다.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                bool isRecoverySuccess = false;
                command = new GP_PCardPodVacuumOn();//=> Prober Card Pod Vacu를 흡착하여 Card 흡착
                retVal = await command.Run();

                if (retVal.ErrorCode == EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR
                    | retVal.ErrorCode == EventCodeEnum.GP_CardChange_IO_ERROR)
                {
                    //IO Error  
                    isRecoverySuccess = false;
                    cardVacError = true;
                    //return retVal;
                }
                else if (retVal.ErrorCode == EventCodeEnum.NONE)
                {
                    //Success
                    isRecoverySuccess = true;
                }
                else
                {
                    return retVal;
                }

                //TODO 카드가 있냐 없냐에 따라 달라지는 시퀀스
                //카드가 
                if (isRecoverySuccess == false)
                {
                    ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    double ejectChuckPos = 0;
                    double curZPos = 0;
                    this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curZPos);
                    ejectChuckPos = curZPos - 30000;

                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                    //this.StageSupervisor().StageModuleState.StageRelMove(this.MotionManager().GetAxis(EnumAxisConstants.Z), -30000);
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GP CC] Fail to Zcleared");
                        return retVal;
                    }
                    bool diupmodule_cardexist_sensor;
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);

                    if (ioret != IORet.NO_ERR)
                    {
                        LoggerManager.Error($"[GP CC]=> {this.GetType().Name} diupmodule_cardexist_sensor read error");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    if (diupmodule_cardexist_sensor == false)
                    {

                        //command = new GP_TopPlateSolUnLock();//=> 상판 Sol UnLock
                        //retVal = await command.Run();
                        //if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                        command = new GP_PCardPodVacuumOff();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GP CC] Fail to Clear state");
                            return retVal;
                        }

                        command = new GP_DropPCardPod();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                        return retVal;
                    }

                    //}
                }
                else
                {
                    //카드가 있을때
                    //<중간 완료 검사>
                    command = new GP_CheckPCardIsOnPCardPod();//=> Prober Card Pod에 Prober Card가 있는지 확인
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                    command = new GP_PogoPCardContactVacuumOff();//=> Pogo Vacuum Off 시켜서 Card를 흡착 해제
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                    command = new GP_TopPlateSolUnLock();//=> 상판 Sol UnLock
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                    command = new GP_PogoPCardContactVacuumOff();//=> Pogo Vacuum Off 시켜서 Card를 흡착 해제
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }


                    command = new GP_DropPCardSafety();//=> Chuck 홈 위치까지 내림
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                    //==> Docking
                    command = new GP_DockPCardTopPlate();//=> Card를 Pogo에 Docking 전에 Align
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }
                }


                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }

                this.CardChangeModule().IsCardStuck = false;

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_PCardSutckRecovery Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                this.StageSupervisor().StageModuleState.CCZCLEARED();

                this.StageSupervisor().StageModuleState.ZCLEARED();
                if (cardVacError)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                }
            }
            return retVal;
        }
    }
    #endregion

    #region ==> Dock : 직접 사용하는 Command
    //==> Card를 Loader로 부터 받기 위한 준비
    [Serializable]
    public class GP_ReadyToGetCard : SequenceBehavior
    {
        public GP_ReadyToGetCard()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_ReadyToGetCard);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                SequenceBehavior command = null;

                //==> 척을 안전한 Z 위치로 이동
                command = new GP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Tester Vacuum을 켠다.
                if (cardChangeSysParam.GPTesterVacSeqSkip != true)
                {
                    command = new GP_UpPlateTesterContactVacuumOn();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }
                }


                //<조건 검사>
                //==> Card가 Card Pod에 없는지 확인
                command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Chuck을 Loader 쪽으로 이동 시킴
                command = new GP_MoveChuckToLoader();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Card pod을 올림
                command = new GP_RaisePCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ReadyToGetCard Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }

    [Serializable]
    public class GP_DD_ReadyToLoading : SequenceBehavior
    {
        public GP_DD_ReadyToLoading()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_DD_ReadyToLoading);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                SequenceBehavior command = null;

                //==> 척을 안전한 Z 위치로 이동
                command = new GP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Tester Vacuum을 켠다.
                if (cardChangeSysParam.GPTesterVacSeqSkip != true)
                {
                    command = new GP_UpPlateTesterContactVacuumOn();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }
                }


                //<조건 검사>
                //==> Card가 Card Pod에 없는지 확인
                command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                // Drop the POD
                command = new GP_DropPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Chuck을 Loader 쪽으로 이동 시킴
                command = new GP_MoveChuckToLoader();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ReadyToGetCard Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }
    [Serializable]
    public class GP_NotExistCheckCardHandoff : SequenceBehavior
    {
        public GP_NotExistCheckCardHandoff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_NotExistCheckCardHandoff);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                //ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return retVal;
                //}

                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}




                SequenceBehavior command = null;


                //==> 척을 안전한 Z 위치로 이동
                command = new GP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {

                    LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                    return retVal;


                }


                command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }


            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ReadyToGetCard Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }
    [Serializable]
    public class GP_PrepareCardHandoff : SequenceBehavior
    {
        public GP_PrepareCardHandoff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_PrepareCardHandoff);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                SequenceBehavior command = null;

                //==> 척을 안전한 Z 위치로 이동
                command = new GP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Tester Vacuum을 켠다.
                if (cardChangeSysParam.GPTesterVacSeqSkip != true)
                {
                    command = new GP_UpPlateTesterContactVacuumOn();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }
                }


                //<조건 검사>
                //==> Card가 Card Pod에 없는지 확인
                command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Chuck을 Loader 쪽으로 이동 시킴
                command = new GP_MoveChuckToLoader();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Card pod을 올림
                command = new GP_RaisePCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ReadyToGetCard Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }
    /// <summary>
    /// Direct dock type card only.
    /// </summary>
    [Serializable]
    public class GP_Handoff : SequenceBehavior
    {
        public GP_Handoff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_Handoff);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // [ISSD-3431]
                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                //ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}
                //if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                //{
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                //[ISSD-3431] 이미 이전 Prepare Behavior에 마지막에서 수행한 동작임.
                //==> Card pod을 올림
                //command = new GP_RaisePCardPod();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                SequenceBehavior command = null;

                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Get parameter");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                var handoffPos = cardChangeParam.CardTransferPos.Value;
                var offsetX = cardChangeParam.CardTransferOffsetX.Value;
                var offsetY = cardChangeParam.CardTransferOffsetY.Value;
                var offsetZ = cardChangeParam.CardTransferOffsetZ.Value;
                var offsetT = cardChangeParam.CardTransferOffsetT.Value;

                var loadPosX = this.CoordinateManager().StageCoord.ChuckLoadingPosition.X.Value;
                var loadPosY = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Y.Value;
                var loadPosZ = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Z.Value;
                var loadPosT = 0d;

                LoggerManager.Debug($"HandOff Position = ({handoffPos.X.Value - loadPosX + offsetX:0.00}, {handoffPos.Y.Value - loadPosY + offsetY:0.00}, {handoffPos.Z.Value - loadPosZ + offsetZ:0.00}), Angle = {handoffPos.T.Value - loadPosT + offsetT:0.0000}");

                //var targetZHeight = handoffPos.Z.Value + offsetZ;
                //if(targetZHeight < this.MotionManager().GetAxis(EnumAxisConstants.Z).Param.NegSWLimit.Value)
                //{
                //    targetZHeight = this.MotionManager().GetAxis(EnumAxisConstants.Z).Param.NegSWLimit.Value + 100.0;
                //    LoggerManager.Debug($"HandOff Position: Z Neg. Limit = {this.MotionManager().GetAxis(EnumAxisConstants.Z).Param.NegSWLimit.Value:0.00}, Updated Pos = {targetZHeight:0.00}");
                //}

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.MoveLoadingOffsetPosition(
                    handoffPos.X.Value - loadPosX + offsetX,
                    handoffPos.Y.Value - loadPosY + offsetY,
                    0,
                    handoffPos.T.Value - loadPosT + offsetT);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    command = null;
                    command = new GP_DropPCardPod();
                    var cmdRetuls = command.Run().Result;
                    if (cmdRetuls.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GP CC]=> EventCode:{retVal.ErrorCode}");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move loading position");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zInterval = cardChangeParam.DD_CardDockZInterval.Value; // 파라미터
                double zMaxLimit = cardChangeParam.DD_CardDockZMaxValue.Value; //파라미터


                double zAbsPos = zAxis.Param.ClearedPosition.Value + zInterval;
                IORet ioret = IORet.UNKNOWN;
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);

                while (true)
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Pod Z Up to {zAbsPos:0.00}");
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                        this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_MOVE_ERROR);
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(100);
                        int ioMonitorRet = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 100, 250);

                        if (ioMonitorRet == 0)
                        {
                            LoggerManager.Debug($"GP_Handoff(): Vac. detected @{zAbsPos:0.00}, Update transfer offset height({cardChangeParam.CardTransferOffsetZ.Value:0.00})");

                            cardChangeParam.CardTransferOffsetZ.Value = zAbsPos;
                            this.CardChangeModule().SaveSysParameter();

                            retVal.ErrorCode = EventCodeEnum.NONE;

                            break;
                        }
                        else if (ioMonitorRet != 0)
                        {
                            zAbsPos += zInterval;
                            if (zAbsPos > zMaxLimit)
                            {
                                int lastInterVal = 100;
                                zAbsPos = zMaxLimit + lastInterVal;
                                for (int i = 0; i < 3; i++)
                                {
                                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                                    {
                                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Pod Z Up to {zAbsPos:0.00}");
                                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                                        this.NotifyManager().Notify(EventCodeEnum.GP_CardChange_MOVE_ERROR);
                                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                                        return Task.FromResult<IBehaviorResult>(retVal);
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(100);
                                        int ioMonitorRetVal = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 100, 250);

                                        if (ioMonitorRetVal == 0)
                                        {
                                            LoggerManager.Debug($"GP_Handoff(): Vac. detected @{zAbsPos:0.00}, Update transfer offset height({cardChangeParam.CardTransferOffsetZ.Value:0.00})");
                                            
                                            cardChangeParam.CardTransferOffsetZ.Value = zAbsPos;
                                            this.CardChangeModule().SaveSysParameter();
                                            
                                            retVal.ErrorCode = EventCodeEnum.NONE;
                                            
                                            return Task.FromResult<IBehaviorResult>(retVal);
                                        }

                                        zAbsPos += lastInterVal;
                                    }
                                }


                                LoggerManager.Error($"[FAIL Z Move] {this.GetType().Name} : Z Pos zMaxLimit : {zMaxLimit}, Target : {zAbsPos}");
                                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                                this.StageSupervisor().StageModuleState.CCZCLEARED();
                                
                                command = null;
                                command = new GP_DropPCardPod();
                                var cmdRetuls = command.Run().Result;
                                if (cmdRetuls.ErrorCode != EventCodeEnum.NONE)
                                {
                                    LoggerManager.Error($"[GP_DropPCardPod] Fail => EventCode:{cmdRetuls.ErrorCode}");
                                }
                                
                                this.NotifyManager().Notify(EventCodeEnum.MOTION_DANGEROUS_POS);
                                retVal.ErrorCode = EventCodeEnum.MOTION_DANGEROUS_POS;
                                return Task.FromResult<IBehaviorResult>(retVal);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ReadyToGetCard Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    /// <summary>
    /// Direct undock type card only.
    /// </summary>
    [Serializable]
    public class GP_HandoffForCardReturn : SequenceBehavior
    {
        public GP_HandoffForCardReturn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_HandoffForCardReturn);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // [ISSD-3431]
                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                //ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return retVal;
                //}

                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    return retVal;
                //}
                //if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                //{
                //    return retVal;
                //}

                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Get parameter");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                var handoffPos = cardChangeParam.CardTransferPos.Value;
                var offsetX = cardChangeParam.CardTransferOffsetX.Value;
                var offsetY = cardChangeParam.CardTransferOffsetY.Value;
                var offsetZ = cardChangeParam.CardTransferOffsetZ.Value + cardChangeParam.DD_CardUndockZOffeset.Value; //update
                var offsetT = cardChangeParam.CardTransferOffsetT.Value;

                var loadPosX = this.CoordinateManager().StageCoord.ChuckLoadingPosition.X.Value;
                var loadPosY = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Y.Value;
                var loadPosZ = this.CoordinateManager().StageCoord.ChuckLoadingPosition.Z.Value;
                var loadPosT = 0d;

                LoggerManager.Debug($"GP_HandoffForCardReturn Position = ({handoffPos.X.Value - loadPosX + offsetX:0.00}, {handoffPos.Y.Value - loadPosY + offsetY:0.00}, {handoffPos.Z.Value - loadPosZ + offsetZ:0.00}), Angle = {handoffPos.T.Value - loadPosT + offsetT:0.0000}");

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.MoveLoadingOffsetPosition(
                    handoffPos.X.Value - loadPosX + offsetX,
                    handoffPos.Y.Value - loadPosY + offsetY,
                    0,
                    handoffPos.T.Value - loadPosT + offsetT);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move loading position");
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, offsetZ, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                //  retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, offsetZ, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move loading position");
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_HandoffForCardReturn Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Card Docking이 완료 된 후 정리 작업을 수행
    [Serializable]
    public class GP_ClearCardDocking : SequenceBehavior
    {
        public GP_ClearCardDocking()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_ClearCardDocking);
        }
        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

                SequenceBehavior command = null;

                command = new GP_CheckPCardIsOnPogo();//=> Card가 Pogo에 제대로 흡착 되어 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                command = new GP_CheckPCardIsNotOnPCardPod();//=> Card 가 Card Pod에 없는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                command = new GP_DropPCardPod();//=> Card Pod을 내림
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                //==> 틀어진 로그 기록
                //if (this.GPCardAligner().ObservationCard() == false)
                //{
                //    LoggerManager.Debug($"[FAIL] {this.GetType().Name} :Write Log ");
                //    return retVal;
                //}

                ////==> Stage Module을 UnLock 상태로 만듬.
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.UnLockCCState();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[GP CC] Fail to Clear state");
                //    return retVal;
                //}
                
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] MOVE To Zcleared");
                    return retVal;
                }

                LoggerManager.Debug($"[GP CC]=> {this.GetType().Name} Done");
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ClearCardDocking Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }
    //==> Card UnDocking이 완료된 후 정리 작업을 수행
    [Serializable]
    public class GP_ClearCardUnDocking : SequenceBehavior
    {
        public GP_ClearCardUnDocking()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_ClearCardUnDocking);
        }
        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                SequenceBehavior command = null;

                //<완료 검사>
                command = new GP_CheckPCardIsNotOnPogo();//=> 상판에 Card가 없는지 확인.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                command = new GP_CheckPCardIsOnPCardPod();//=> Card Pod에 카드 있는지 검사
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                // 매뉴얼 언도킹만 진행했을 경우에도 카드팟 베큠을 꺼넣은 채로 있을 수 있기 때문에
                // 카드 언로드 쪽에서 언도킹 이후 베큠 끄는 부분을 넣음. (ISSD-4171)
                //if (this.CardChangeModule().GetCCDockType() != EnumCardDockType.DIRECTDOCK)
                //{
                //    command = new GP_PCardPodVacuumOff();//=> Card Pod Vacuum 을 Off 함으로서 Loader가 Card를 가져 갈 수 있도록 준비
                //    retVal = await command.Run();
                //    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                //} 
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"Error Occured while GP_ClearCardUnDocking Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }
    //==> Card Chagne가 완료된 후 정리 작업을 수행
    [Serializable]
    public class GP_ClearCardChange : SequenceBehavior
    {
        public GP_ClearCardChange()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_ClearCardChange);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;

                //==> 척을 안전한 Z 위치로 이동
                command = new GP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                //==> Prober Card Pod에 카드 없는지 확인
                command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //=> Card Pod을 내림
                command = new GP_DropPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> Chuck Theta를 0 도로 복원
                LoggerManager.Debug($"[GP CC] Theta reset 0");

                //retVal.ErrorCode = this.MotionManager().AbsMove(EnumAxisConstants.C, 0);
                var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, 0, axist.Param.Speed.Value, axist.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Rotate Theta, T : 0");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }

                //==> Stage Move를  상태로 바꿈
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_ClearCardChange Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }
    //==> Card를 Pogo에 Docking
    [Serializable]
    public class GP_DockPCardTopPlate : SequenceBehavior
    {
        public GP_DockPCardTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_DockPCardTopPlate);
        }

        public override async Task<IBehaviorResult> Run()
        {
            //==> [Chuck에 있는 Prober Card를 Tester의 홀더에 Dock 시킨다.]

            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
                    //this.CardChangeModule().SetCardDockedWithPogoStatus();
                    return retVal;
                }

                this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKING);

                SequenceBehavior command = null;

                command = new GP_PCardPodVacuumOn();//=> Card Pod Vacuum을 킨다.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                //<조건 검사>
                command = new GP_CheckPCardIsOnPCardPod();//=> Card Pod에 카드가 올려져 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                //**
                command = new GP_CheckPCardIsNotOnPogo();//=> POGO에 Card가 없는지 확인.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_CheckTesterIsOnTopPlate();//=> 상판에 Tester가 올려져 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_CheckTopPlateSolIsUnLock();//=> 상판에 Locker가 Unlock 되어 있는지 확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                //<전처리>
                command = new GP_DropChuckSafety();//==> Chuck을 안전한 Z 위치로 이동
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_CardAlign();//=> Card Align
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_DockRaiseChuckToPogo();//=> Chuck을 Contact 위치 까지 이동 시켜서 Card를 Pogo에 부착
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                //**
                command = new GP_PogoPCardContactVacuumOn();//=> Pogo Card Vacu On 시켜서 Card를 Pogo에 흡착
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_PCardPodVacuumOff();//=> Card Pod Vacuum을 해제하여 Card를 Card Pod으로 부터 분리
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_TopPlateSolLock();//=> 상판 Locker를 Lock 시킴
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_TesterClamped();//=> Tester Vac, Card Vac 상태를 보고 Clamped ON(Mani가 Tester를 가져갈 수 없음) 또는 UnClamped ON 시킴(Mani가 Tester를 가져갈 수 있음)
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                command = new GP_DropChuckSafety();//=> Card는 Pogo에 붙인 상태로 Chuck을 안전한 위치로 이동 시킴
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                //<완료 검사>
                command = new GP_ClearCardDocking();//=> Card Docking 정리
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_DockPCardTopPlate Seq ErrMsg:{err.Message}");
               
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }



    #endregion 
    //==> Card를 Pogo로 부터 UnDocking
    [Serializable]
    public class GP_UndockPCardTopPlate : SequenceBehavior
    {
        public GP_UndockPCardTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_UndockPCardTopPlate);
        }

        public override async Task<IBehaviorResult> Run()
        {
            //==> Tester의 홀더에 있는 Prober Card를 Undocking 시킨다.
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                    return retVal;
                }

                EnumCardDockingStatus lastStatusBeforeUndocking = this.CardChangeModule().GetCardDockingStatus();

                this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKING);

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                //this.StageSupervisor().StageModuleState.LockCCState();

                await Task.Run(() =>
                {
                    if (this.StageSupervisor().MarkObject.GetAlignState() != AlignStateEnum.DONE)
                    {

                        if (cardChangeSysParam.CardDockType.Value != EnumCardDockType.DIRECTDOCK)
                        {
                            retVal.ErrorCode = this.MarkAligner().DoMarkAlign();
                        }
                        else
                        {
                            retVal.ErrorCode = EventCodeEnum.NONE;
                        }

                    }
                });

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                bool isthreelegup = false;
                bool isthreelegdown = false;
                bool usinghandler = this.StageSupervisor().CheckUsingHandler(this.LoaderController().GetChuckIndex());
                // TODO : ThreeLeg Check 문제 찾고 고쳐야 함.
                if (usinghandler != true)
                {
                    retVal.ErrorCode = this.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                    }

                    retVal.ErrorCode = this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdown);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {

                    }
                }
                if (isthreelegup != false || isthreelegdown != true || usinghandler == true)
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.Handlerrelease(15000);
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
                }

                SequenceBehavior command = null;

                bool diupmodule_cardexist_sensor;
                var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=> {this.GetType().Name} diupmodule_cardexist_sensor read error");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL; this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    return retVal;
                }

                bool diupmodule_vacu_sensor;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out diupmodule_vacu_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacu_sensor read error");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL; this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    return retVal;
                }

                if (diupmodule_vacu_sensor == false)
                {
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                    if (ioret != IORet.NO_ERR)
                    {
                        LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacu_sensor write error");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                        this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                        ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out diupmodule_vacu_sensor);
                        if (ioret != IORet.NO_ERR)
                        {
                            LoggerManager.Error($"[GP CC]=> {this.GetType().Name} diupmodule_cardexist_sensor read error");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                            this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            return retVal;
                        }
                        if (diupmodule_vacu_sensor == false)
                        {
                            ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                            if (ioret != IORet.NO_ERR)
                            {
                                LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacu_sensor write error");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal;
                            }
                        }

                    }
                }


                bool dipogocard_vacu_sensor = false;

                if (lastStatusBeforeUndocking != EnumCardDockingStatus.STUCKED)
                {
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                    if (ioret != IORet.NO_ERR) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} dipogocard_vacu_sensor read error"); retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL; this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    if (dipogocard_vacu_sensor == false)
                    {
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                        if (ioret != IORet.NO_ERR)
                        {
                            LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} dipogocard_vacu_sensor write error");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                            this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(1000);
                            ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                            if (ioret != IORet.NO_ERR)
                            {
                                LoggerManager.Error($"[GP CC]=> {this.GetType().Name} dipogocard_vacu_sensor read error");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                                return retVal;
                            }
                            if (dipogocard_vacu_sensor == false)
                            {
                                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                                System.Threading.Thread.Sleep(3000);
                                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                                if (ioret != IORet.NO_ERR)
                                {
                                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} dipogocard_vacu_sensor write error");
                                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                                    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 카드가 Stuck되어 있는 상태에서는 베큠이 잡히지 않는다. 하지만 카드를 언로드(언도킹)을 해야 하는 경우 카드 베큠 상태를 보지 않고 가능하도록 한다.
                    LoggerManager.Debug($"GP_UndockPCardTopPlate. Skip checking card vac because card status is stucked");
                    bool forced_Cardvac_True = true;
                    dipogocard_vacu_sensor = forced_Cardvac_True;
                }

                bool ditplate_pclatch_sensor_lock;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);

                bool ditplate_pclatch_sensor_unlock;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);

                if (cardChangeSysParam.CardDockType.Value == EnumCardDockType.DIRECTDOCK)
                {
                    if (diupmodule_vacu_sensor == true)
                    {
                        ditplate_pclatch_sensor_lock = true;
                        ditplate_pclatch_sensor_unlock = false;
                    }
                    if (dipogocard_vacu_sensor == true)
                    {
                        ditplate_pclatch_sensor_lock = true;
                        ditplate_pclatch_sensor_unlock = false;
                    }
                }

                //카드가 팟에 얹어져 있을때 

                if (diupmodule_cardexist_sensor == true && diupmodule_vacu_sensor == true && dipogocard_vacu_sensor == false)
                {
                    //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    //if (retVal.ErrorCode != EventCodeEnum.NONE)
                    //{
                    //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_SAFE_POSITION;
                    //    LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    //    return retVal;
                    //}

                    command = new GP_MoveChuckToLoader();//=> Chuck을 Loader로 이동
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    //<완료 검사>
                    command = new GP_ClearCardUnDocking();//=> Card UnDocking 정리
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                }
                else if (dipogocard_vacu_sensor && ditplate_pclatch_sensor_lock && ditplate_pclatch_sensor_unlock == false)
                {
                    //<조건 검사>
                    command = new GP_CheckPCardIsNotOnPCardPod();//=> Card Pod에 Card가 없는지 확인.
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    //**
                    if (lastStatusBeforeUndocking != EnumCardDockingStatus.STUCKED)
                    {
                        command = new GP_CheckPCardIsOnPogo();//=> 상판에 Card가 흡착 되어 있는지 확인.
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
                    }
                    else
                    {
                        LoggerManager.Debug($"GP_UndockPCardTopPlate. Skip checking card vac because card status is stucked");
                    }
                    //**
                    command = new GP_CheckTesterIsOnTopPlate();//=> 상판에 Tester가 정상적으로 올려졌는지 확인
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_CheckTopPlateSolIsLock();//=> 상판에의 Locker가 Lock 되어 있는지 확인
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    //<전처리>
                    command = new GP_DropChuckSafety();//==> Chuck을 안전한 Z 위치로 이동
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    //<회수>
                    command = new GP_RaisePCardPod();//=> Prober Card Pod 상승
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_CheckPCardPodIsUp();//==> Card Pod이 올려져 있는지 다시 확인
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_SetAlignPos();//=> Chuck을 Pogo Contact 위치(X,Y,T) 위치로 이동 시킴
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_UnDockRaiseChuckToPogo();//=> Chuck을 상승 시켜서 Pogo에 부착 시킨다.
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
                    if (cardChangeSysParam.CardDockType.Value != EnumCardDockType.DIRECTDOCK)
                    {
                        command = new GP_PCardPodUndockVacuumOn();//=> Card Pod Vacuum를 켜서 Card Pod에 Card를 흡착 시킴
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                            var ret = CheckforZCleared();
                            if (ret == EventCodeEnum.NONE)
                            {
                                this.StageSupervisor().StageModuleState.CCZCLEARED();

                                command = new GP_DropPCardPod();
                                retVal = await command.Run();
                                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                            }

                            this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                            retVal.ErrorCode = EventCodeEnum.CARD_CHANGE_FAIL;
                            return retVal;
                        }

                    }

                    command = new GP_PogoPCardContactVacuumOff();//=> Pogo Vacuum Off 시켜서 Card를 흡착 해제
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                        var ret = CheckforZCleared();
                        if (ret == EventCodeEnum.NONE)
                        {
                            this.StageSupervisor().StageModuleState.CCZCLEARED();

                            command = new GP_DropPCardPod();
                            retVal = await command.Run();
                            if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); return retVal; }

                        }
                        retVal.ErrorCode = EventCodeEnum.CARD_CHANGE_FAIL;
                        return retVal;
                    }

                    command = new GP_TopPlateSolUnLock();//=> 상판 Locker를 Unlock 시킴
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_TesterClamped();//=> Tester Vac, Card Vac 상태를 보고 Clamped ON(Mani가 Tester를 가져갈 수 없음) 또는 UnClamped ON 시킴(Mani가 Tester를 가져갈 수 있음)
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_DropPCardSafety();//=> Card를 안전한 Z 위치 로 이동
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    command = new GP_MoveChuckToLoader();//=> Chuck을 Loader로 이동
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                    //<완료 검사>
                    command = new GP_ClearCardUnDocking();//=> Card UnDocking 정리
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                }
                else
                {
                    LoggerManager.Error($"Error Occured while GP_UndockPCardTopPlate Seq  Check Vacuum and Something");
                    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                }
            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                
                LoggerManager.Error($"Error Occured while GP_UndockPCardTopPlate Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;

            EventCodeEnum CheckforZCleared()
            {
                EventCodeEnum ret = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                try
                {

                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);

                    bool ditplate_pclatch_sensor_lock;
                    this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);

                    bool ditplate_pclatch_sensor_unlock;
                    this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                    if (ditplate_pclatch_sensor_lock == true && ditplate_pclatch_sensor_unlock == false)
                    {
                        ret = EventCodeEnum.NONE;
                    }

                }
                catch (Exception err)
                {
                    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                    
                    LoggerManager.Error($"Error Occured while GP_UndockPCardTopPlate Seq ErrMsg:{err.Message}");
                    
                    retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                }
                return ret;
            }
        }
    }

    [Serializable]
    public class GP_TesterClamped : SequenceBehavior
    {
        public GP_TesterClamped()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_TesterClamped);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                bool pogo_tester_vac;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out pogo_tester_vac);

                bool pogo_card_vac;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out pogo_card_vac);

                if (pogo_tester_vac == false && pogo_card_vac == false)
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_CLAMPED, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while WriteBit for io of DOTESTER_CLAMPED in GP_TesterClamped sequence");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_UNCLAMPED, true);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while WriteBit for io of DOTESTER_UNCLAMPED in GP_TesterClamped sequence");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
                else
                {
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_UNCLAMPED, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while WriteBit for io of DOTESTER_UNCLAMPED in GP_TesterClamped sequence");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTESTER_CLAMPED, true);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while WriteBit for io of DOTESTER_CLAMPED in GP_TesterClamped sequence");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_TesterClamped Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    #region 상판에 카드가 있는지 척을 올려 검사 
    //==> UpModule로 상판에 카드가 걸려있는지 없는지 확인
    [Serializable]
    public class CardCheckByUpModule : SequenceBehavior
    {
        public CardCheckByUpModule()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(CardCheckByUpModule);
        }
        public override async Task<IBehaviorResult> Run()
        {

            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();
            ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();

                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                SequenceBehavior command = null;

                //<조건 검사>

                bool dipogocard_vacu_sensor = false;
                bool ditplate_pclatch_sensor_lock = false;
                bool ditplate_pclatch_sensor_unlock = false;

                command = new GP_CheckPCardIsNotOnPCardPod();//=> Prober Card Pod에 Prober Card가 없는지 확인.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                IORet ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                    return retVal;
                }
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                    return retVal;
                }
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                    return retVal;
                }

                if (dipogocard_vacu_sensor == false &&
                     ditplate_pclatch_sensor_lock == true &&
                     ditplate_pclatch_sensor_unlock == false)
                {
                    command = new GP_DropChuckSafety();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                    command = new GP_RaisePCardPod();//=> Prober Card Pod 상승
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                    command = new GP_CheckPCardPodIsUp();//==> Card pod이 올려져 있는지 다시 확인
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }


                    ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                    if (cardChangeSysParam == null)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : System parameter is Not Setted");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        return retVal;
                    }

                    ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                    if (cardChangeDevParam == null)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        return retVal;

                    }

                    ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                    ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                    ProbeAxisObject tAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(xAxis, cardChangeDevParam.GP_CardContactPosX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(yAxis, cardChangeDevParam.GP_CardContactPosY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(tAxis, cardChangeDevParam.GP_CardContactPosT, tAxis.Param.Speed.Value, tAxis.Param.Acceleration.Value);
                    if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }


                    double zAbsPos = cardChangeDevParam.GP_CardContactPosZ + cardChangeSysParam.GP_ContactCorrectionZ;
                    double bigZUpZPos = zAbsPos - 40000;

                    double cardCheckposition = zAbsPos - Math.Abs(cardChangeSysParam.GP_CardCheckOffsetZ);

                    //==> SW Limit Check
                    if (cardCheckposition > zAxis.Param.PosSWLimit.Value)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {cardCheckposition}");
                        retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                        return retVal;
                    }

                    //==> Big Z Up
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}");
                        
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                        return retVal;
                    }

                    //==> Zup to Check Card pos
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, cardCheckposition, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : CardCheck Z Up, {cardCheckposition}");
                        
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                        
                        return retVal;
                    }

                    double curXPos = 0;
                    double curYPos = 0;
                    double curZPos = 0;
                    double curTPos = 0;

                    this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                    this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                    
                    LoggerManager.Debug($"GPCC Card Check Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");

                    //////////////////////////////////////////////////////////////////////////////////////////////////////

                    bool diupmodule_cardexist_sensor = false;
                    for (int i = 0; i < 5; i++)
                    {
                        System.Threading.Thread.Sleep(1000);
                        ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);
                        if (ioret != IORet.NO_ERR)
                        {
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                            return retVal;
                        }
                        if (diupmodule_cardexist_sensor == true)
                            break;
                    }


                    double ejectChuckPos = 0;
                    double curActPos = 0;
                    this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curActPos);
                    ejectChuckPos = curActPos - 20000;

                    if (ejectChuckPos > zAxis.Param.HomeOffset.Value)
                    {
                        //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_SAFE_POSITION;
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Eject chuck {ejectChuckPos}");
                            return retVal;
                        }
                    }

                    //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                    //if (retVal.ErrorCode != ErrorCodeEnum.NONE)
                    //{
                    //    retVal.ErrorCode = ErrorCodeEnum.GP_CardChange_RECOVERY_FAIL;
                    //    return retVal;
                    //}
                    if (diupmodule_cardexist_sensor == true)
                    {
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GP CC] Fail to Clear state");
                            return retVal;
                        }

                        command = new GP_DropPCardPod();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
                        this.CardChangeModule().IsCardStuck = true;
                        //this.CardChangeModule().SetCardDockedWithPogoStatus();
                        this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
                    }
                    else
                    {
                        command = new GP_TopPlateSolUnLock();//=> 상판 Sol UnLock
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GP CC] Fail to Clear state");
                            return retVal;
                        }

                        command = new GP_DropPCardPod();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                        this.CardChangeModule().IsCardStuck = false;
                        this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                    }
                }
                else
                {
                    this.CardChangeModule().IsCardStuck = false;
                    //this.CardChangeModule().SetCardDockedWithPogoStatus();
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GP CC] Fail to Clear state");
                    return retVal;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_CardCheckByUpModule Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }

    }
    [Serializable]
    public class GP_CheckProbeCard_RaiseChuckWithUpModule : SequenceBehavior
    {
        public GP_CheckProbeCard_RaiseChuckWithUpModule()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GP_CheckProbeCard_RaiseChuckWithUpModule);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GP_CheckPCardIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                IORet ioret = IORet.ERROR;
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, true);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, false);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                command = new GP_CheckPCardPodIsUp();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GP_RaisePCardPod Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }
    #endregion

    #endregion


    #region ==> MPT Prober Card Change Behavior

    /*
   * PCardPod(Prober Card Pod) : Chuck 옆에 Prober Card를 올릴 수 있는 지지대
   * -> Up
   * -> Down
   * 
   * TopPlateSol(Top Plate Solenoid) : 상판의 Solenoid Locker, Pogo에 진공 흡착된 Card의 낙하 방지용
   */

    #region ==> Check
    //==> Prober Card가 Card Pod에 있는지 확인
    [Serializable]
    public class GOP_CheckPCardIsOnPCardPod : SequenceBehavior
    {
        public GOP_CheckPCardIsOnPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsOnPCardPod);
            this.SequenceDescription = "Check probe card is on card pod";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                //===> Check Carrier On Pod
                var intret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;                   
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                //===> Check Carrier On Pod
                var intret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                if (intret2 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR;
                //===> Check Carrier On Pod
                var vintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_VACU_SENSOR, true, DIUPMODULE_VACU_SENSOR.MaintainTime.Value, DIUPMODULE_VACU_SENSOR.TimeOut.Value);
                if (vintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = retVal.Reason = IOErrorDescription(DIUPMODULE_VACU_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                int entret = -1;
                //===> Check Card  Exist
                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                {
                    IOPortDescripter<bool> DIUPMODULE_CARDEXIST_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR;
                    entret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_CARDEXIST_SENSOR, true, DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value, DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value);
                    if (entret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(DIUPMODULE_CARDEXIST_SENSOR, true, this.GetType().Name);

                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.STAGEMOVE_WAFER_CAM_EXPENDED;
                    retVal.Reason = $"Wafer cam bridge state {StageCylinderType.MoveWaferCam.State.ToString()}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if ((intret == 0) && (intret2 == 0) && (vintret == 0) && (entret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : intret:{intret} intret2:{intret2} vintret:{vintret} entret:{entret}");
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : intret:{intret} intret2:{intret2} vintret:{vintret} entret:{entret}\n";
                }            
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CheckPCardIsOnPCardPod Seq ErrMsg:{err.Message},  Event Code : {retVal.ErrorCode}\n";
                
                LoggerManager.Error($"Error Occured while GOP_CheckPCardIsOnPCardPod Seq ErrMsg:{err.Message},  Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Carrier가 Buffer나 Arm에 없고 Pod에 있는 경우 Pod에 진짜 있는지 확인하는 애
    [Serializable]
    public class GOP_ReturnCardPodState : SequenceBehavior
    {
        public GOP_ReturnCardPodState()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ReturnCardPodState);
            this.SequenceDescription = "Check Carrier is on card pod";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IORet ioret;
                bool diupmodule_left_sensor;
                bool diupmodule_right_sensor;
                bool diupmodule_touch_sensor_L;
                bool diupmodule_touch_sensor_R;
                bool diupmodule_vacuum_sensor;
                bool diupmodule_card_exist;
                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_left_sensor read error");
                }

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_right_sensor read error");
                }

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, out diupmodule_touch_sensor_L);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_touch_sensor_L read error");
                }

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, out diupmodule_touch_sensor_R);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_touch_sensor_R read error");
                }

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, out diupmodule_vacuum_sensor);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacuum_sensor read error");
                }

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_card_exist);
                if (ioret != IORet.NO_ERR)
                {
                    LoggerManager.Error($"[GP CC]=>  {this.GetType().Name} diupmodule_vacuum_sensor read error");
                }

                // DIUPMODULE_VACU_SENSOR 를 사용하지 않는 경우 해당 Flag를 무시하도록 처리
                bool useVaccumeSensor = true;
                if (EnumIOOverride.NONE != this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR.IOOveride.Value)
                {
                    useVaccumeSensor = false;
                }

                if ((diupmodule_card_exist == true) && (diupmodule_left_sensor == true) && (diupmodule_right_sensor == true) && (diupmodule_touch_sensor_L == true) && (diupmodule_touch_sensor_R == true) && (useVaccumeSensor ? diupmodule_vacuum_sensor == true : true))
                {//카드팟 위에 카드가 있을 때 
                    retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                }
                else if ((diupmodule_card_exist == false) && (diupmodule_left_sensor == true) && (diupmodule_right_sensor == true) && (diupmodule_touch_sensor_L == true) && (diupmodule_touch_sensor_R == true) && (useVaccumeSensor ? diupmodule_vacuum_sensor == true : true))
                {//카드팟 위에 뭐가 있을 때 
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_ON_POD;
                }
                //팟 위에 아무것도 없고 팟도 내려가 있을 때
                else if ((diupmodule_card_exist == false) && (diupmodule_left_sensor == false) && (diupmodule_right_sensor == false) && (diupmodule_touch_sensor_L == false) && (diupmodule_touch_sensor_R == false) && (useVaccumeSensor ? diupmodule_vacuum_sensor == false : true))
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                }
                else//뭔가 이상이 있을 때
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                }

                LoggerManager.Debug($"[GP CC]=>  {this.GetType().Name}, retVal.ErrorCode : {retVal.ErrorCode}" +
                    $"\ndiupmodule_left_sensor : {diupmodule_left_sensor}" +
                    $"\ndiupmodule_right_sensor : {diupmodule_right_sensor}" +
                    $"\ndiupmodule_touch_sensor_L : {diupmodule_touch_sensor_L}" +
                    $"\ndiupmodule_touch_sensor_R : {diupmodule_touch_sensor_R}" +
                    $"\ndiupmodule_vacuum_sensor : {diupmodule_vacuum_sensor}" +
                    $"\nuseVaccumeSensor_flag : {useVaccumeSensor}" +
                    $"\ndiupmodule_card_exist : {diupmodule_card_exist}");
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while {this.GetType()} Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";

                LoggerManager.Error($"Error Occured while {this.GetType()} Seq ErrMsg:{err.Message}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    //==> Carrier가 Card Pod에 있는지 확인
    [Serializable]
    public class GOP_CheckCarrierIsOnPCardPod : SequenceBehavior
    {
        public GOP_CheckCarrierIsOnPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckCarrierIsOnPCardPod);
            this.SequenceDescription = "Check carrier os on probe card pod";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                
                IOPortDescripter<bool> DIUPMODULE_LEFT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR;
                var rintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_LEFT_SENSOR, true, DIUPMODULE_LEFT_SENSOR.MaintainTime.Value, DIUPMODULE_LEFT_SENSOR.TimeOut.Value);
                if (rintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_LEFT_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                IOPortDescripter<bool> DIUPMODULE_RIGHT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR;
                var lintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_RIGHT_SENSOR, true, DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value, DIUPMODULE_RIGHT_SENSOR.TimeOut.Value);
                if (lintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_RIGHT_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier On Pod
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                var intret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);                    
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                var intret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                if (intret2 != 0)
                {
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);                    
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR;
                var vintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_VACU_SENSOR, true, DIUPMODULE_VACU_SENSOR.MaintainTime.Value, DIUPMODULE_VACU_SENSOR.TimeOut.Value);
                if (vintret != 0)
                {
                    retVal.Reason = IOErrorDescription(DIUPMODULE_VACU_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                if ((intret == 0) && (intret2 == 0) && (rintret == 0) && (lintret == 0) && (vintret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : intret:{intret}  intret2:{intret2} rintret:{rintret} lintret:{lintret} vintret:{vintret}\n");
                    retVal.Reason += $"intret:{intret}  intret2:{intret2} rintret:{rintret} lintret:{lintret} vintret:{vintret}\n";
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CheckCarrierIsOnPCardPod Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Debug($"Error Occured while GOP_CheckCarrierIsOnPCardPod Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    //==> Carrier가 Card Pod에 없는지 확인
    [Serializable]
    public class GOP_CheckCarrierIsNotOnPCardPod : SequenceBehavior
    {
        public GOP_CheckCarrierIsNotOnPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckCarrierIsNotOnPCardPod);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                //===> Check Carrier On Pod
                IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR;
                var vintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_VACU_SENSOR, false, DIUPMODULE_VACU_SENSOR.MaintainTime.Value, DIUPMODULE_VACU_SENSOR.TimeOut.Value);
                if (vintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIUPMODULE_VACU_SENSOR in GOP_CheckCarrierIsNotOnPCardPod");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier On Pod
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                var intret1 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, false, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (intret1 != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIUPMODULE_TOUCH_SENSOR_L in GOP_CheckCarrierIsOnPCardPod");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                var intret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, false, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                if (intret2 != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIUPMODULE_TOUCH_SENSOR_R in GOP_CheckCarrierIsOnPCardPod");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Card On Pod
                IOPortDescripter<bool> DIUPMODULE_CARDEXIST_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR;
                var entret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_CARDEXIST_SENSOR, false, DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value, DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value);
                if (entret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIUPMODULE_CARDEXIST_SENSOR in GOP_CheckCarrierIsOnPCardPod");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                if ((intret1 == 0) && (intret2 == 0) && (vintret == 0) && (entret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : intret1:{intret1} intret2:{intret2} vintret:{vintret} entret:{entret} ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_CheckCarrierIsOnPCardPod Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Card가 Card Pod에 없는지 확인
    [Serializable]
    public class GOP_CheckPCardIsNotOnTopPlate : SequenceBehavior
    {
        public GOP_CheckPCardIsNotOnTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsNotOnTopPlate);
            this.SequenceDescription = "Check probe card is not on topplate";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIHOLDER_ON_TOPPLATE = this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE;

                //===> Check Card On Topplate
                var intret = this.IOManager().IOServ.MonitorForIO(DIHOLDER_ON_TOPPLATE, false, DIHOLDER_ON_TOPPLATE.MaintainTime.Value, DIHOLDER_ON_TOPPLATE.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIHOLDER_ON_TOPPLATE, false, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{ 
                //    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); 
                //    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CheckPCardIsNotOnTopPlate Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}\n";

                LoggerManager.Error($"Error Occurred while GOP_CheckPCardIsNotOnTopPlate Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    

    //==> Card가 Card Pod에 없는지 확인 ** GOP에서는 확인할 수 없는 구조로 되어있음. 
    [Serializable]
    public class GOP_CheckPCardIsNotOnCarrier : SequenceBehavior
    {
        public GOP_CheckPCardIsNotOnCarrier()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsNotOnCarrier);
            this.SequenceDescription = "Check Probe card is not on carrier";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
         

                //===> Check WaferCam and Retract Bridge
                int ret = -1;
                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.EXTEND)
                {
                    ret = StageCylinderType.MoveWaferCam.Retract();
                    if (ret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.STAGEMOVE_WAFER_CAM_EXPENDED;
                        retVal.Reason = $"{retVal.ErrorCode}";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }



                // chck vac, rot, exist
                IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR;
                //===> Check Carrier On Pod
                var vintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_VACU_SENSOR, true, DIUPMODULE_VACU_SENSOR.MaintainTime.Value, DIUPMODULE_VACU_SENSOR.TimeOut.Value);
                if (vintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_VACU_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // chck vac, rot, exist
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                //===> Check Carrier On Pod
                var intret1 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (intret1 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                //===> Check Carrier On Pod
                var intret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                if (intret2 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                IOPortDescripter<bool> DIUPMODULE_CARDEXIST_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR;
                //===> Check Carrier On Pod
                var entret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_CARDEXIST_SENSOR, false, DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value, DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value);
                if (entret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_CARDEXIST_SENSOR, false, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if ((intret1 == 0) && (intret2 == 0) && (vintret == 0) && (entret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : intret1:{intret1} intert2:{intret2} entret:{entret} vintret:{vintret} ");
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : intret1:{intret1} intert2:{intret2} entret:{entret} vintret:{vintret}" +
                        $"\nEventCode = {retVal.ErrorCode}";
                }
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return Task.FromResult<IBehaviorResult>(retVal); }


            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occurred while GOP_CheckPCardIsNotOnCarrier Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_CheckPCardIsNotOnCarrier Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod이 올려져 있는지 확인
    [Serializable]
    public class GOP_CheckPCardPodIsUp : SequenceBehavior
    {
        public GOP_CheckPCardPodIsUp()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardPodIsUp);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var lintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 100, 1000);
                if (lintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_LEFT_SENSOR in GOP_CheckPCardPodIsUp sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var rintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 100, 1000);
                if (rintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_RIGHT_SENSOR in GOP_CheckPCardPodIsUp sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                if ((lintret == 0) && (rintret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_DOWN_STATUS;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} :  Left : {lintret}, Right : {rintret}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_CheckPCardPodIsUp Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod이 내려져 있는지 확인
    [Serializable]
    public class GOP_CheckPCardPodIsDown : SequenceBehavior
    {
        public GOP_CheckPCardPodIsDown()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardPodIsDown);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {

                IOPortDescripter<bool> DIUPMODULE_LEFT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR;
                var rintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_LEFT_SENSOR, false, DIUPMODULE_LEFT_SENSOR.MaintainTime.Value, DIUPMODULE_LEFT_SENSOR.TimeOut.Value);
                if (rintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_LEFT_SENSOR, false, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                IOPortDescripter<bool> DIUPMODULE_RIGHT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR;
                var lintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_RIGHT_SENSOR, false, DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value, DIUPMODULE_RIGHT_SENSOR.TimeOut.Value);
                if (lintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_RIGHT_SENSOR, false, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                if ((lintret == 0) && (rintret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : 'false' is down, Left -> {lintret}, Right -> {rintret}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_CheckPCardPodIsDown Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card 가 Pogo에 있는지 확인
    [Serializable]
    public class GOP_CheckPCardIsOnTopPlate : SequenceBehavior
    {
        public GOP_CheckPCardIsOnTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsOnTopPlate);
            this.SequenceDescription = "Check probe card is on topplate";
        }
        public GOP_CheckPCardIsOnTopPlate(bool writeLog) : this()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsOnTopPlate);
            this.log_Flag = writeLog;
        }

        public override Task<IBehaviorResult> Run()
        {
            if (log_Flag == true)
            {
                LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            }

            IBehaviorResult retVal = new BehaviorResult();

            try
            {

                IOPortDescripter<bool> DIHOLDER_ON_TOPPLATE = this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE;

                //===> Check Card On Topplate
                var intret = this.IOManager().IOServ.MonitorForIO(DIHOLDER_ON_TOPPLATE, true, DIHOLDER_ON_TOPPLATE.MaintainTime.Value, DIHOLDER_ON_TOPPLATE.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIHOLDER_ON_TOPPLATE, true, this.GetType().Name);
                    if (log_Flag == true)
                    {
                        LoggerManager.Error(retVal.Reason);
                    }
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CheckPCardIsOnTopPlate Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode} ";
                
                LoggerManager.Error($"Error Occurred while GOP_CheckPCardIsOnTopPlate Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card 가 CardPod에 있는지 확인
    [Serializable]
    public class GOP_CheckPCardIsExist : SequenceBehavior
    {
        public GOP_CheckPCardIsExist()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsExist);
            this.SequenceDescription = "Check probe card is on probe card pod";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                bool expectedret = true;
                string cardId = this.CardChangeModule().GetProbeCardID();

                if ((this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam).IsCardExist
                    && !string.IsNullOrEmpty(cardId) && cardId.ToLower() != "holder")                      
                {
                    // Card Exist 
                    expectedret = true;
                }
                else
                {
                    // Only Holder Exist 
                    expectedret = false;
                }


                IOPortDescripter<bool> DIUPMODULE_CARDEXIST_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR;
                IOPortDescripter<bool> DICARD_EXIST_ON_CARDPOD = this.IOManager().IO.Inputs.DICARD_EXIST_ON_CARDPOD;
                //===> Check Card In Stage
                var intret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_CARDEXIST_SENSOR, expectedret, DIUPMODULE_CARDEXIST_SENSOR.MaintainTime.Value, DIUPMODULE_CARDEXIST_SENSOR.TimeOut.Value);
                var intret2 = this.IOManager().IOServ.MonitorForIO(DICARD_EXIST_ON_CARDPOD, expectedret, DICARD_EXIST_ON_CARDPOD.MaintainTime.Value, DICARD_EXIST_ON_CARDPOD.TimeOut.Value);
                if (intret != 0 || intret2 != 0)
                {
                    if (expectedret == true)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                    }
                    if (intret != 0)
                    {                        
                        retVal.Reason = IOErrorDescription(DIUPMODULE_CARDEXIST_SENSOR, expectedret, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                    }
                    if (intret2 != 0)
                    {                        
                        retVal.Reason = IOErrorDescription(DICARD_EXIST_ON_CARDPOD, expectedret, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

                return Task.FromResult<IBehaviorResult>(retVal);

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CheckPCardIsExist Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Debug($"Error Occured while GOP_CheckPCardIsExist Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);

        }
    }

    //==> Card 가 Pogo에 있는지 확인
    [Serializable]
    public class GOP_CheckPCardIsNotExist : SequenceBehavior
    {
        public GOP_CheckPCardIsNotExist()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsNotExist);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Card In Stage
                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_CARDEXIST_SENSOR in GOP_CheckPCardIsNotExist ");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChage_EXIST_CARD_ON_CARD_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = EventCodeEnum.NONE;


            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_CheckPCardIsExist Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);

        }
    }
    //==> 상판의 낙하 방지용 Locker가 Lock 되어 있는지 확인
    [Serializable]
    public class GOP_CheckTopPlateSolIsLock : SequenceBehavior
    {
        public GOP_CheckTopPlateSolIsLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckTopPlateSolIsLock);
        }
        public GOP_CheckTopPlateSolIsLock(bool writeLog) : this()
        {
            this.log_Flag = writeLog;
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckTopPlateSolIsLock);
        }
        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                var lintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, true, 1000, 15000, this.log_Flag);
                if (lintret != 0)
                {
                    if (log_Flag == true)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DITPLATE_PCLATCH_SENSOR_LOCK in GOP_CheckTopPlateSolIsLock sequence");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                }

                var uintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, false, 1000, 15000, this.log_Flag);
                if (uintret != 0)
                {
                    if (log_Flag == true)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DITPLATE_PCLATCH_SENSOR_UNLOCK in GOP_CheckTopPlateSolIsLock sequence");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }

                if ((lintret == 0) && (uintret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    if (log_Flag == true)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Lock sensor : {lintret}, Unlock sensor : {uintret}");
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_PROBE_CARD_LATCH_IS_UNLOCKED;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_CheckTopPlateSolIsLock Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> 상판의 낙하 방지용 Locker가 Unlock 되어 있는지 확인
    [Serializable]
    public class GOP_CheckTopPlateSolIsUnLock : SequenceBehavior
    {
        public GOP_CheckTopPlateSolIsUnLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckTopPlateSolIsUnLock);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                // TODO : 현재 이 Behavior는 Unlock이 아닌 경우, Unlock을 시키고 다시 확인 하는 로직이기 때문에, ReadBit 후, 바로 에러코드를 리턴해서는 안된다.

                // TODO : cardAttach 센서가 추가되고 나서 상판에 없는지 확인해야함. 

                // rot check 
                //SequenceBehavior command = new GOP_CheckPCardIsOnTopPlate();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} EventCode: {retVal.ErrorCode}");
                //    return retVal;
                //}


                // 사실상 Retry Code
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    // unlock 가 이상한 경우, 에러 코드 리턴.

                    if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                    {
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotUnLock(60000); // unlock 다시 시도 
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_RECOVERY_FAIL;
                        
                        LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} EventCode: {retVal.ErrorCode}, Retry CCRotUnLock Fail");
                    }

                    // 다시 Rot lock, 위험하니까 zcleard 부르지 않기.
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        SequenceBehavior command = new GOP_TopPlateSolLock();
                        retVal = await command.Run();
                        
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} EventCode: {retVal.ErrorCode}");
                        }
                    }
                }

                // OK
                if (retVal.ErrorCode == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} EventCode: {retVal.ErrorCode}");
                }
                else
                {
                    bool diupmodule_cardexist_sensor;
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, out diupmodule_cardexist_sensor);
                    //bool dicarrier_on_pod;
                    //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, out dicarrier_on_pod);
                    bool diupmodule_left_sensor;
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, out diupmodule_left_sensor);

                    bool diupmodule_right_sensor;
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, out diupmodule_right_sensor);

                    //bool dicard_on_topplate;
                    //ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, out dicard_on_topplate);

                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} :  diupmodule_cardexist_sensor:{diupmodule_cardexist_sensor},  diupmodule_left_sensor:{diupmodule_left_sensor}, diupmodule_right_sensor:{diupmodule_right_sensor} ");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_CheckTopPlateSolIsUnLock Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }

    //==> Tester가 상판에 잘 Docking 되어 있는지 확인
    [Serializable]
    public class GOP_CheckTesterIsOnTopPlate : SequenceBehavior
    {
        public GOP_CheckTesterIsOnTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckTesterIsOnTopPlate);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                //bool ditplatein_sensor;
                //this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
                var ditplateinRet = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, true, 7000);
                if (ditplateinRet != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //bool ditester_docking_sensor;
                //this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);
                var ditestdocksensorRet = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, true);
                if (ditestdocksensorRet != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                if (cardChangeParam.GPTesterVacSeqSkip == false)
                {
                    //bool dipogotester_vacu_sensor = false;
                    //var dipogotestervacRet = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
                    var dipogotestervacRet = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, true, 7000);
                    if (dipogotestervacRet != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    // OK
                    if ((ditplateinRet == 0) && (ditestdocksensorRet == 0) && (dipogotestervacRet == 0))
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                    else
                    {
                        bool ditplatein_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
                        bool ditester_docking_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);
                        bool dipogotester_vacu_sensor = false;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Top Plate In : {ditplatein_sensor},  Docking : {ditester_docking_sensor}, Tester vacuum sensor : {dipogotester_vacu_sensor}");
                    }
                }
                else
                {
                    if ((ditplateinRet == 0) && (ditestdocksensorRet == 0))
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                    else
                    {
                        bool ditplatein_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATEIN_SENSOR, out ditplatein_sensor);
                        bool ditester_docking_sensor;
                        this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITESTER_DOCKING_SENSOR, out ditester_docking_sensor);
                        //bool dipogotester_vacu_sensor = false;
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_UNDOCKED;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Top Plate In : {ditplatein_sensor},  Docking : {ditester_docking_sensor}");
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_CheckTesterIsOnTopPlate Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card가 상판에 걸려 있는지 확인 : Card가 Pogo에 흡착은 떨어지고 Top Plate Locker 에 걸려 있는 상태
    [Serializable]
    public class GOP_CheckPCardIsStuckInTopPlate : SequenceBehavior
    {
        public GOP_CheckPCardIsStuckInTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckPCardIsStuckInTopPlate);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;

                IBehaviorResult cardIsNotOnPogoResult = new BehaviorResult();
                command = new GOP_CheckPCardIsOnTopPlate();//=> 상판에 Card가 흡착 안 되어 있는지 확인
                cardIsNotOnPogoResult = await command.Run();


                if (cardIsNotOnPogoResult.ErrorCode != EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_STUCK_IN_POGO;
                    return retVal;
                }


                IBehaviorResult topPlateSolIsLock = new BehaviorResult();
                command = new GOP_CheckTopPlateSolIsLock();//=> 상판에 Card Sol Lock이 닫혀 있는지 확인
                topPlateSolIsLock = await command.Run();

                if (topPlateSolIsLock.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_STUCK_IN_POGO;
                    return retVal;
                }

                if (cardIsNotOnPogoResult.ErrorCode == EventCodeEnum.GP_CardChange_CARD_IS_NOT_TOUCHED_ON_POGO && topPlateSolIsLock.ErrorCode == EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"{this.GetType().Name} : Card is not stuck in Pogo");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_IS_NOT_STUCK_IN_POGO;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_CheckPCardIsStuckInTopPlate Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }



    //==> Card가 Stage에 존재 하는지 확인
    [Serializable]
    public class GOP_CheckCardIsNotInStage : SequenceBehavior
    {
        public GOP_CheckCardIsNotInStage()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CheckCardIsNotInStage);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // GOP 에서는 카드팟에 카드또는 캐리어가 있으면 안됨. 
                // 카드가 도킹되어있는지는 모르지만 카드팟에는 없다는 것을 확인하는 시퀀스

                //SequenceBehavior command = null;

                //===> Check Carrier down
                var rintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, false, 1000, 15000);
                if (rintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_LEFT_SENSOR in {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier down
                var lintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, false, 1000, 15000);
                if (lintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_RIGHT_SENSOR in {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier Not On Pod
                var vintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 15000);
                if (vintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_VACU_SENSOR in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier Not On Pod
                var ltintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, false, 1000, 15000);
                if (ltintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_L in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var rtintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, false, 1000, 15000);
                if (rtintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_R in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Card Not On Pod
                var entret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, false, 1000, 15000);
                if (entret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_CARDEXIST_SENSOR in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_EXIST_CARD_IN_STAGE;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                if ((rintret == 0) && (lintret == 0) && (vintret == 0) && (ltintret == 0) && (rtintret == 0) && (entret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                    
                    LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} : Card Not Exist On Pod");
                    
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_CheckCardIsNotInStage Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    #endregion

    #region ==> Move
    //==> Chuck을 Loader 쪽으로 이동 시킴 : Card를 Loader로 부터 주거나 받기 위해
    [Serializable]
    public class GOP_MoveChuckToLoader : SequenceBehavior
    {
        //==> Chuck이 Prober Card 받기 위해 Handler 쪽으로 접근
        public GOP_MoveChuckToLoader()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_MoveChuckToLoader);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {             
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                

                ICardChangeSysParam cardChangeParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Get parameter");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Move chuck to Loader
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.MoveLoadingPosition(0);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move loading position");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Rotate Theta
                double theta = cardChangeParam.GP_LoadingPosT;
                var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, theta * 10000,
                //   axist.Param.Speed.Value, axist.Param.Acceleration.Value);

                // TODO : AbsMove를 쓰면 안되지만, 당장 문제가 되지 않을 것 같아, 넣어놓은 임시방편이다. 추후에 바꿔야 하긴 함.
                retVal.ErrorCode = this.MotionManager().AbsMove(EnumAxisConstants.C, theta * 10000);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Rotate Theta, T : {theta}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_MOVE_TO_LOADER;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_MoveChuckToLoader Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Chuck을 Pogo Contact 위치 까지 Z Up
    [Serializable]
    public class GOP_DockRaiseChuckToPogo : SequenceBehavior
    {
        public GOP_DockRaiseChuckToPogo()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DockRaiseChuckToPogo);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GOP_CheckPCardPodIsUp();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }


                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                // TODO : Task.Delay가 최선인지 확인 필요
                await Task.Delay(3000);

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : System parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeDevParam.GP_CardContactPosZ + cardChangeSysParam.GP_ContactCorrectionZ;
                double bigZUpZPos = zAbsPos - 20000;

                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {zAbsPos}");
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return retVal;
                }

                //==> Big Z Up
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                //await Task.Delay(500);

                //==> Contact Z Up to Pogo
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Contact Z Up, {zAbsPos}");
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                //    return retVal;
                //}

                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Small Z up, {smallZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;
                while (runflag)
                {
                    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        
                        LoggerManager.Debug("DIUPMODULE_LEFT_SENSOR off");
                        
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        
                        return retVal;
                    }

                    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        
                        LoggerManager.Debug("DIUPMODULE_RIGHT_SENSOR off");
                        
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        
                        return retVal;
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;
                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if (apos >= zAbsPos)
                        {
                            runflag = false;
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                            
                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value / 10, zaxis.Param.Acceleration.Value / 20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                        
                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        
                        LoggerManager.Debug($"SW Limit Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                        
                        runflag = false;
                        
                        return retVal;
                    }

                    Thread.Sleep(2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_DockRaiseChuckToPogo Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }

            return retVal;
        }
    }

    //==> Chuck을 Pogo Contact 위치 까지 Z Up
    [Serializable]
    public class GOP_RaiseChuckToTopPlate : SequenceBehavior
    {
        public GOP_RaiseChuckToTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RaiseChuckToTopPlate);
            this.SequenceDescription = "Raise chuck to topplate";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                if (cardChangeSysParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change System Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (cardChangeDevParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change Device Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);

                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.CardDockPosZ.Value;
                double bigZUpZPos = zAbsPos - 40000;

                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    double curr_Z = 0;
                    this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error Code : {retVal.ErrorCode}, Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Current Z : {curr_Z:0.000}" +
                        $"Target : {bigZUpZPos}" +
                        $"Curr_Z : {curr_Z:0.000}");

                    retVal.Reason = $" Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}\n" +
                        $"Target : {bigZUpZPos}\n" +
                        $"Curr_Z : {curr_Z:0.000}\n";

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Big Z Up
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    double curr_Z = 0;
                    this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} Error Code : {retVal.ErrorCode}" +
                        $"\nTarget : {bigZUpZPos}" +
                        $"\nCurr_Z : {curr_Z}");

                    retVal.Reason = $"Target : {bigZUpZPos}\nCurr_Z : {curr_Z}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    double curr_Z = 0;
                    this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} Error Code : {retVal.ErrorCode}" +
                        $"\nTarget : {smallZUpZPos}" +
                        $"\nCurr_Z : {curr_Z}");

                    retVal.Reason = $"Target : {smallZUpZPos}\nCurr_Z : {curr_Z}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;
                while (runflag)
                {
                    IOPortDescripter<bool> DIUPMODULE_LEFT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR;
                    int iret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_LEFT_SENSOR, true, DIUPMODULE_LEFT_SENSOR.MaintainTime.Value, DIUPMODULE_LEFT_SENSOR.TimeOut.Value);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;                       
                        retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    IOPortDescripter<bool> DIUPMODULE_RIGHT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR;
                    iret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_RIGHT_SENSOR, true, DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value, DIUPMODULE_RIGHT_SENSOR.TimeOut.Value);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;                        
                        retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, this.GetType().Name);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;
                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if (apos >= zAbsPos)
                        {
                            runflag = false;
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            
                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                            
                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value / 10, zaxis.Param.Acceleration.Value / 20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                        
                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                        double curr_Z = 0;
                        this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error Code : {retVal.ErrorCode}, Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Current Z : {curr_Z:0.000}" +
                            $"Target : {absPos}" +
                            $"Curr_Z : {curr_Z:0.000}");

                        retVal.Reason = $" Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}\n" +
                            $"Target : {absPos}\n" +
                            $"Curr_Z : {curr_Z:0.000}\n";

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    Thread.Sleep(2);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_RaiseChuckToTopPlate Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_RaiseChuckToTopPlate Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class GOP_RaiseChuckBig : SequenceBehavior
    {
        public GOP_RaiseChuckBig()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RaiseChuckBig);
            this.SequenceDescription = "Raise chuck Big Z UP";
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                if (cardChangeSysParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change System Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (cardChangeDevParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change Device Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);

                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.CardDockPosZ.Value;
                double bigZUpZPos = zAbsPos - 40000;

                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    double curr_Z = 0;
                    this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error Code : {retVal.ErrorCode}, Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Current Z : {curr_Z:0.000}" +
                        $"Target : {bigZUpZPos}" +
                        $"Curr_Z : {curr_Z:0.000}");

                    retVal.Reason = $" Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}\n" +
                        $"Target : {bigZUpZPos}\n" +
                        $"Curr_Z : {curr_Z:0.000}\n";

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Big Z Up
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    double curr_Z = 0;
                    this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} Error Code : {retVal.ErrorCode}" +
                        $"\nTarget : {bigZUpZPos}" +
                        $"\nCurr_Z : {curr_Z}");

                    retVal.Reason = $"Target : {bigZUpZPos}\nCurr_Z : {curr_Z}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_RaiseChuckBig Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_RaiseChuckBig Seq ErrMsg:{err.Message},  Event Code : {retVal.ErrorCode}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class GOP_RaiseChuckSmall : SequenceBehavior
    {
        public GOP_RaiseChuckSmall()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RaiseChuckSmall);
            this.SequenceDescription = "Raise chuck Small Z UP";
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                if (cardChangeSysParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change System Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (cardChangeDevParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change Device Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.CardDockPosZ.Value;


                double bigZUpZPos = zAbsPos - 40000;
                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double posZ = 0;
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref posZ);
                
                if (posZ < bigZUpZPos)//만약에 현재 척의 높이가 bigZUpZPos보다 낮으면 bigZUp하고 그 위에는 천천히 올림
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);//현재 위치가 bigZUpZPos 낮을때는 Big ZUp후에 smallZUpZPos가야함
                    
                    LoggerManager.Debug($"Current ZPos is lower than BigZupPos. First, it comes up to BigZup. BigZUp : {bigZUpZPos}");

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        double curr_Z = 0;
                        this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} Error Code : {retVal.ErrorCode}" +
                            $"\nTarget : {bigZUpZPos}" +
                            $"\nCurr_Z : {curr_Z}");

                        retVal.Reason = $"Target : {bigZUpZPos}\nCurr_Z : {curr_Z}\n";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    double curr_Z = 0;
                    this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} Error Code : {retVal.ErrorCode}" +
                        $"\nTarget : {smallZUpZPos}" +
                        $"\nCurr_Z : {curr_Z}");

                    retVal.Reason = $"Target : {smallZUpZPos}\nCurr_Z : {curr_Z}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;
                while (runflag)
                {
                    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;
                    
                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if (apos >= zAbsPos)
                        {
                            runflag = false;
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            
                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value / 10, zaxis.Param.Acceleration.Value / 20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                        
                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                        double curr_Z = 0;
                        this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error Code : {retVal.ErrorCode}, Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Current Z : {curr_Z:0.000}" +
                            $"Target : {absPos}" +
                            $"Curr_Z : {curr_Z:0.000}");

                        retVal.Reason = $" Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}\n" +
                            $"Target : {absPos}\n" +
                            $"Curr_Z : {curr_Z:0.000}\n";

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    Thread.Sleep(2);
                }


            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_RaiseChuckToTopPlate Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_RaiseChuckToTopPlate Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}");
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;

                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class GOP_MoveSafeFromDoor : SequenceBehavior
    {
        public GOP_MoveSafeFromDoor()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_MoveSafeFromDoor);
            this.SequenceDescription = "CCZCLEARED and move Contact Position";
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                SequenceBehavior command = null;

                //==> GOP_PCardPodVacuumOn                
                command = new GOP_PCardPodVacuumOn();//=> Prober Card Pod Vacu를 흡착하여 Card 흡착
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                    return retVal;
                }

                //==> CC Clearance                 
                command = new GOP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                    return retVal;
                }


                //==> MoveToCenter
                command = new GOP_SetCardContactPos();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                    return retVal;
                }

                //if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_MoveSafeFromDoor Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occured while GOP_MoveSafeFromDoor Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }
    }

    [Serializable]
    public class GOP_CardDoorClose : SequenceBehavior
    {
        public GOP_CardDoorClose()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CardDoorClose);
            this.SequenceDescription = "Close card Door";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                var ret = this.StageSupervisor().StageModuleState.CardDoorClose();
                Task.Delay(1000).Wait();
                if (ret != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.STAGEMOVE_IS_LOADER_DOOR_CLOSE_ERROR;
                    retVal.Reason = $"CardDoorClose() fail.\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CardDoorClose Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_CardDoorClose Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }

    }
    [Serializable]
    public class GOP_CardDoorOpen : SequenceBehavior
    {
        public GOP_CardDoorOpen()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CardDoorOpen);
            this.SequenceDescription = "Open card Door";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                var ret = this.StageSupervisor().StageModuleState.CardDoorOpen();
                Task.Delay(1000).Wait();
                if (ret != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.STAGEMOVE_IS_LOADER_DOOR_OPEN_ERROR;
                    retVal.Reason = $"EventCode = {retVal.ErrorCode}";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_CardDoorOpen Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_CardDoorOpen Seq ErrMsg:{err.Message}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }

    }
 
    //==> 척을 올려 Card exist 센서가 홀더를 볼수 있는 위치로 이동
    [Serializable]
    public class GOP_RaiseChuckCheckHolderExist : SequenceBehavior
    {
        public GOP_RaiseChuckCheckHolderExist()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RaiseChuckCheckHolderExist);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Chuck down to Home Offset                
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_SAFE_POSITION;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Down to Home Offset, EventCode:{retVal.ErrorCode}");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                #region ==> retract wafer cam bridge

                int iret = -1;
                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.EXTEND)
                {
                    iret = StageCylinderType.MoveWaferCam.Retract();
                    if (iret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.STAGEMOVE_WAFER_CAM_EXPENDED;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }

                #endregion

                #region ==> check upmodule down state : 하나라도 none이 아니면 IsCardExist true
                //===> Check Carrier down
                var rintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, false, 1000, 15000);
                if (rintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_LEFT_SENSOR in {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier down
                var lintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, false, 1000, 15000);
                if (lintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_RIGHT_SENSOR in {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_UP_STATUS;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier Not On Pod
                var vintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 15000);
                if (vintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_VACU_SENSOR in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Carrier Not On Pod
                var ltintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, false, 1000, 15000);
                if (ltintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_L in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var rtintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, false, 1000, 15000);
                if (rtintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_R in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //===> Check Card Not On Pod
                var entret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, false, 1000, 15000);
                if (entret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_CARDEXIST_SENSOR in  {this.GetType()}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_EXIST_CARD_IN_STAGE;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if ((rintret == 0) && (lintret == 0) && (vintret == 0) && (ltintret == 0) && (rtintret == 0) && (entret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                    
                    LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} : Card Exist On Pod");
                }
                else
                {
                    this.CardChangeModule().SetIsCardExist(true);
                }
                #endregion

                #region ==> raise chuck
                // 카드팟에 카드가 없을시 아래 동작

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject tAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double zAbsPos = cardChangeSysParam.CardHolderCheckPosZ.Value;
                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {zAbsPos}");
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(xAxis, cardChangeSysParam.CardHolderCheckPosX.Value, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(yAxis, cardChangeSysParam.CardHolderCheckPosY.Value, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(tAxis, cardChangeSysParam.CardHolderCheckPosT.Value, tAxis.Param.Speed.Value, tAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, cardChangeSysParam.CardHolderCheckPosZ.Value, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }


                #endregion

                #region ==> check card exist sensor & holder on topplate 에 따라서 IsCardExist true/false

                entret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, false, 1000, 15000);
                if (entret != 0)
                {// card on toppate
                    entret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_CARDEXIST_SENSOR, true, 1000, 15000);
                    if (entret != 0)//error 
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIUPMODULE_CARDEXIST_SENSOR in  {this.GetType()}");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_NOT_EXIST_CARD_IN_STAGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    var hntret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, true, 1000, 15000);
                    if (hntret != 0)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIHOLDER_ON_TOPPLATE in  {this.GetType()}");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_EXIST_CARD_IN_STAGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    this.CardChangeModule().SetIsCardExist(true);
                }
                else
                {//card not on topplate 
                    var hntret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, false, 1000, 15000);
                    if (hntret != 0)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIHOLDER_ON_TOPPLATE in  {this.GetType()}");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_EXIST_CARD_IN_STAGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    this.CardChangeModule().SetIsCardExist(false);
                }

                #endregion



                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_RaiseChuckCheckHolderExist Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;

                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Error($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }



    [Serializable]
    public class GOP_RaiseChuckAndUnlock : SequenceBehavior
    {
        public GOP_RaiseChuckAndUnlock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RaiseChuckAndUnlock);
            this.SequenceDescription = "Z up and ROT unlock";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var lintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 100, 1000);
                if (lintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_LEFT_SENSOR in GOP_RaiseChuckAndUnlock sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = "Error occured while monitor for io of DIUPMODULE_LEFT_SENSOR in GOP_RaiseChuckAndUnlock sequence, Target Level : true";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var rintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 100, 1000);
                if (rintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_RIGHT_SENSOR in GOP_RaiseChuckAndUnlock sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = "Error occured while monitor for io of DIUPMODULE_RIGHT_SENSOR in GOP_RaiseChuckAndUnlock sequence, Target Level : true";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                if ((lintret == 0) && (rintret == 0))
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_DOWN_STATUS;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} :  Left : {lintret}, Right : {rintret}");
                    retVal.Reason = $"[FAIL] {this.GetType().Name} :  Left : {lintret}, Right : {rintret}, EventCode = {retVal.ErrorCode}";
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : SystemParameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Parameter is Not Setted,  EventCode = {retVal.ErrorCode}";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : DeviceParameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Parameter is Not Setted,  EventCode = {retVal.ErrorCode}";

                    return Task.FromResult<IBehaviorResult>(retVal);

                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.CardDockPosZ.Value;
                double bigZUpZPos = zAbsPos - 40000;

                //==> SW Limit Check
                if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {zAbsPos}");
                    retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Target : {zAbsPos}, EventCode = {retVal.ErrorCode}";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Big Z Up
                //retVal.ErrorCode = this.MotionManager().AbsMove(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value, zAxis.Param.Acceleration.Value);

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Big Z up, {bigZUpZPos}, EventCode = {retVal.ErrorCode}";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                //=> Card Pod Vacuum을 해제하여 Card를 Card Pod으로 부터 분리
                //command = new GOP_PCardPodVacuumOff();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                // unlock 동작 
                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotUnLock(60000); // 실제 동작
                }
                else
                {
                    int ret = -1;
                    ret = StageCylinderType.MoveWaferCam.Retract();
                    if (ret != 0)
                    {
                        //ERrror
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        retVal.Reason = $"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else // ret == 0
                    {
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotLock(60000);
                    }


                }
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : CCRotUnLock, {retVal.ErrorCode}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : CCRotUnLock, {retVal.ErrorCode},  EventCode:{retVal.ErrorCode}";

                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                //retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);

                //if (retVal.ErrorCode != EventCodeEnum.NONE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Contact Z up, {zAbsPos}");
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                //    return Task.FromResult<IBehaviorResult>(retVal);
                //}

                double smallZUpZPos = zAbsPos - 5000;
                zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Small Z up, {smallZUpZPos}");
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Contact Z up, {zAbsPos},  EventCode:{retVal.ErrorCode}";
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                //List<IOPortDescripter> monitorInputs = new List<IOPortDescripter>();
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR);
                //monitorInputs.Add(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR);

                bool runflag = true;
                while (runflag)
                {
                    int iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR, true, 10, 15000);
                    
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        
                        LoggerManager.Debug("DIUPMODULE_LEFT_SENSOR off");
                        
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    iret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR, true, 10, 15000);
                    if (iret != (int)IORet.NO_ERR)
                    {
                        runflag = false;
                        
                        LoggerManager.Debug("DIUPMODULE_RIGHT_SENSOR off");
                        
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    ProbeAxisObject zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                    double apos = zaxis.Param.PosSWLimit.Value;
                    this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                    double pos = Math.Abs(200.0);
                    double absPos = pos + apos;

                    if (absPos < zaxis.Param.PosSWLimit.Value)
                    {
                        if (apos >= zAbsPos)
                        {
                            runflag = false;
                            retVal.ErrorCode = EventCodeEnum.NONE;
                            
                            LoggerManager.Debug($"RaiseChuck In Position, Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                            
                            break;
                        }

                        this.MotionManager().RelMove(zaxis, pos, zaxis.Param.Speed.Value / 10, zaxis.Param.Acceleration.Value / 20);
                        this.MotionManager().GetActualPos(zaxis.AxisType.Value, ref apos);
                        
                        LoggerManager.Debug($"Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                    }
                    else
                    {
                        //Sw limit
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        
                        LoggerManager.Error($"SW Limit Current Z Pos : {apos} PosSWLimit:{zaxis.Param.PosSWLimit.Value}");
                        
                        runflag = false;

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    Thread.Sleep(2);
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_RaiseChuckAndUnlock Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occured while GOP_RaiseChuckToTopPlate Seq ErrMsg:{err.Message}");
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;
                
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Chuck을 안전한 위치 까지 Z Down
    [Serializable]
    public class GOP_DropChuckSafety : SequenceBehavior
    {
        public GOP_DropChuckSafety()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DropChuckSafety);
            this.SequenceDescription = "chuck down and CCZCLEARED";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);

                double curZPos = 0;
                this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref curZPos);

                double ejectChuckPos = curZPos - 10000;

                //==> Eject chuck from Chuck
                if (ejectChuckPos > zAxis.Param.HomeOffset.Value)
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, ejectChuckPos, zAxis.Param.Speed.Value / 15, zAxis.Param.Acceleration.Value / 15);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.SetInnerError(new BehaviorResult()
                        {
                            ErrorCode = retVal.ErrorCode,
                            Reason = $"{zAxis.AxisType.Value} Axis error.\n" +
                            $"Target pos : {ejectChuckPos}\n" +
                            $"Error Behavior class name : {this.GetType().Name }\n"
                        });
                        LoggerManager.Error($"[FAIL] { zAxis.AxisType.Value} Axis error.\n" +
                            $"Target pos : {ejectChuckPos}\n" +
                            $"Error Behavior class name : {this.GetType().Name }\n" +
                            $"EventCode:{retVal.ErrorCode}");
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }

                //==> Chuck down to Home Offset                
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.SetInnerError(new BehaviorResult() { ErrorCode = retVal.ErrorCode, Reason = $"CCZCLEARED fail.\n" });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : CCZCLEARED fail. Down to Home Offset, EventCode:{retVal.ErrorCode}");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_DropChuckSafety Seq ErrMsg:{err.Message}, EventCode:{retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occured while GOP_DropChuckSafety Seq ErrMsg:{err.Message}, EventCode:{retVal.ErrorCode}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Pogo에 붙여져 있는 Card를 내림, 안전한 위치 까지 Z Down
    [Serializable]
    public class GOP_DropPCardSafety : SequenceBehavior
    {
        public GOP_DropPCardSafety()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DropPCardSafety);
            this.SequenceDescription = "Drop Probe card";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {


                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.GP_CARDCHANGE_STATE_ERROR,
                        Reason = "Not card change state.\n" +
                        $"Target state : {StageStateEnum.CARDCHANGE.ToString()}" +
                        $"current state : {state.ToString()}"
                    });
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double firstcurZpos = 0;
                //double curZPos = 0;
                //double ejectChuckPos = 0;
                //bool DICARDCHANGEVACU;
                //bool diCardExistOnPod;
                bool safeDownSuccess = false;
                this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref firstcurZpos);
                //double limitPosMargin = 1200;



                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = $"output sensor error {this.IOManager().IO.Outputs.DOUPMODULE_VACU.Description}\n" +
                        $" writeBit -> {this.IOManager().IO.Outputs.DOUPMODULE_VACU.Value}\n" +
                        $"Error Behavior class name : {this.GetType().Name}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR;
                var intret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_VACU_SENSOR, true, DIUPMODULE_VACU_SENSOR.MaintainTime.Value, DIUPMODULE_VACU_SENSOR.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;                    
                    retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                    return Task.FromResult<IBehaviorResult>(retVal);

                }

                // chck vac, rot, exist
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                var vintret1 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (vintret1 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;                    
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                var vintret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);     ;
                if (vintret2 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

        
                var tintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, true, 1000, 15000);
                if (tintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeSysParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change System Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (cardChangeDevParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change Device Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);

                }
                var isRotRls = false;
                var isRotFls = false;
                this.StageSupervisor().MotionManager().IsRls(EnumAxisConstants.ROT, ref isRotRls);
                this.StageSupervisor().MotionManager().IsFls(EnumAxisConstants.ROT, ref isRotFls);
                if ((vintret1 == 0) && (vintret2 == 0) && (tintret == 0) && (isRotRls || !isRotFls))
                {

                    zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    double zAbsPos = cardChangeSysParam.GP_Undock_ContactCorrectionZ
                       + cardChangeSysParam.CardDockPosZ.Value;
                    double bigZUpZPos = zAbsPos - 30000;

                    //==> SW Limit Check
                    if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                    {
                        retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                        double curr_Z = 0;
                        this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error Code : {retVal.ErrorCode}, Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}, Current Z : {curr_Z:0.000}" +
                            $"Target : {bigZUpZPos}" +
                            $"Curr_Z : {curr_Z:0.000}");

                        retVal.Reason = $" Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}\n" +
                            $"Target : {bigZUpZPos}\n" +
                            $"Curr_Z : {curr_Z:0.000}\n";

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, bigZUpZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        double curr_Z = 0;
                        this.MotionManager().GetRefPos(zAxis.AxisType.Value, ref curr_Z);
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} Error Code : {retVal.ErrorCode}" +
                            $"\nTarget : {bigZUpZPos}" +
                            $"\nCurr_Z : {curr_Z}");

                        retVal.Reason = $"Target : {bigZUpZPos}\nCurr_Z : {curr_Z}\n";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    
                    vintret1 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                    if (vintret1 != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    vintret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                    if (vintret2 != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);

                    }


                    IOPortDescripter<bool> DIHOLDER_ON_TOPPLATE = this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE;
                    tintret = this.IOManager().IOServ.MonitorForIO(DIHOLDER_ON_TOPPLATE, true, DIHOLDER_ON_TOPPLATE.MaintainTime.Value, DIHOLDER_ON_TOPPLATE.TimeOut.Value);
                    if (tintret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    // Todo: Check card attated sensor
                    if ((vintret1 == 0) && (vintret2 == 0) && (tintret == 0) && (isRotRls || !isRotFls))
                    {
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move to ZCleared");
                            retVal.Reason = "CCZCLEARED() fail";
                            return Task.FromResult<IBehaviorResult>(retVal);
                        }
                        else
                        {
                            safeDownSuccess = true;
                        }
                    }
                }

                if (!safeDownSuccess)
                {
                    LoggerManager.Error($"[FAIL] SafeDown FAIL WHILE UNDOCK");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                    retVal.Reason = "[FAIL] SafeDown FAIL WHILE UNDOCK";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Move to ZCleared";
                    
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move to ZCleared");
                    
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occurred while GOP_DropPCardSafety Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;

                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);

                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> GOP_RaiseChuckSmall의 역동작(카드 올려져 있는 상태에서 조금 내리는 동작)
    [Serializable]
    public class GOP_DropPCardSafetySmall : SequenceBehavior
    {
        public GOP_DropPCardSafetySmall()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DropPCardSafetySmall);
            this.SequenceDescription = "A little bit Drop Probe card";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : It is not Card Change state";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double firstcurZpos = 0;
                bool safeDownSuccess = false;
                this.MotionManager().GetActualPos(zAxis.AxisType.Value, ref firstcurZpos);

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while WriteBit for io of DOUPMODULE_VACU in GOP_DropPCardSafetySmall sequence" +
                        $"\nWriteBit -> true");
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Error occurred while WriteBit for io of DOUPMODULE_VACU in GOP_DropPCardSafetySmall sequence " +
                        $"\nEvent Code : {retVal.ErrorCode}" +
                        $"\nWriteBit -> true";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

          
                // chck vac, rot, exist
                IOPortDescripter<bool> DIUPMODULE_VACU_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR;
                //===> Check Carrier On Pod
                var intret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_VACU_SENSOR, true, DIUPMODULE_VACU_SENSOR.MaintainTime.Value, DIUPMODULE_VACU_SENSOR.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_VACU_SENSOR, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // chck vac, rot, exist
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                //===> Check Carrier On Pod
                var vintret1 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (vintret1 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                //===> Check Carrier On Pod
                var vintret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                if (vintret2 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                IOPortDescripter<bool> DIHOLDER_ON_TOPPLATE = this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE;

                //===> Check Card On Topplate
                var tintret = this.IOManager().IOServ.MonitorForIO(DIHOLDER_ON_TOPPLATE, true, DIHOLDER_ON_TOPPLATE.MaintainTime.Value, DIHOLDER_ON_TOPPLATE.TimeOut.Value);
                if (tintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIHOLDER_ON_TOPPLATE, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;

                var isRotRls = false;
                var isRotFls = false;

                this.StageSupervisor().MotionManager().IsRls(EnumAxisConstants.ROT, ref isRotRls);
                this.StageSupervisor().MotionManager().IsFls(EnumAxisConstants.ROT, ref isRotFls);

                if ((vintret1 == 0) && (vintret2 == 0) && (tintret == 0) && (isRotRls || !isRotFls))
                {

                    zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                    double zAbsPos = cardChangeSysParam.GP_Undock_ContactCorrectionZ
                       + cardChangeSysParam.CardDockPosZ.Value;
                    double smallZDowmZPos = zAbsPos - 40000;

                    //==> SW Limit Check
                    if (zAbsPos > zAxis.Param.PosSWLimit.Value)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}" +
                            $"\nTarget : {smallZDowmZPos}");
                        retVal.ErrorCode = EventCodeEnum.MOTION_POS_SW_LIMIT_ERROR;
                        retVal.Reason = $"[FAIL] {this.GetType().Name} : Z Pos SW Limit : {zAxis.Param.PosSWLimit.Value}" +
                            $"\nTarget : {smallZDowmZPos}";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, smallZDowmZPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 10);
                    
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Contact Z up, {zAbsPos}");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                        retVal.Reason = $"[FAIL] {this.GetType().Name} : Contact Z up, {zAbsPos}";

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        safeDownSuccess = true;
                    }

                    if (!safeDownSuccess)
                    {
                        LoggerManager.Error($"[FAIL] SafeDown FAIL WHILE UNDOCK");
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                        retVal.Reason = "[FAIL] SafeDown FAIL WHILE UNDOCK";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        LoggerManager.Debug($"Success GOP_DropPCardSafetySmall");
                    }
                
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_DropPCardSafety_Small Seq ErrMsg:{err.Message}");
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;
                
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> GOP_RaiseChuckBig의 역동작(카드 올려져 있는 상태에서 안전한 위치까지 내리는 동작)
    [Serializable]
    public class GOP_DropPCardSafetyBig : SequenceBehavior
    {
        public GOP_DropPCardSafetyBig()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DropPCardSafetyBig);
            this.SequenceDescription = "PCard down and CCZCLEARED";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();

                if (state != StageStateEnum.CARDCHANGE)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : It is not Card Change state";
                    
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                bool safeDownSuccess = false;

                // chck vac, rot, exist
                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_L = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L;
                //===> Check Carrier On Pod
                var vintret1 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_L, true, DIUPMODULE_TOUCH_SENSOR_L.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_L.TimeOut.Value);
                if (vintret1 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_L, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DIUPMODULE_TOUCH_SENSOR_R = this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R;
                //===> Check Carrier On Pod
                var vintret2 = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_TOUCH_SENSOR_R, true, DIUPMODULE_TOUCH_SENSOR_R.MaintainTime.Value, DIUPMODULE_TOUCH_SENSOR_R.TimeOut.Value);
                if (vintret2 != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;                    
                    retVal.Reason = IOErrorDescription(DIUPMODULE_TOUCH_SENSOR_R, true, this.GetType().Name);                    
                    LoggerManager.Error(retVal.Reason);
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                IOPortDescripter<bool> DIHOLDER_ON_TOPPLATE = this.IOManager().IO.Inputs.DIHOLDER_ON_TOPPLATE;

                //===> Check Card On Topplate
                var tintret = this.IOManager().IOServ.MonitorForIO(DIHOLDER_ON_TOPPLATE, true, DIHOLDER_ON_TOPPLATE.MaintainTime.Value, DIHOLDER_ON_TOPPLATE.TimeOut.Value);
                if (tintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIHOLDER_ON_TOPPLATE, true, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);

                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;


                var isRotRls = false;
                var isRotFls = false;
                this.StageSupervisor().MotionManager().IsRls(EnumAxisConstants.ROT, ref isRotRls);
                this.StageSupervisor().MotionManager().IsFls(EnumAxisConstants.ROT, ref isRotFls);
                if ((vintret1 == 0) && (vintret2 == 0) && (tintret == 0) && (isRotRls || !isRotFls))
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move to ZCleared");
                        retVal.Reason = $"[FAIL] {this.GetType().Name} : Move to ZCleared";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    else
                    {
                        safeDownSuccess = true;
                    }

                }
                if (!safeDownSuccess)
                {
                    LoggerManager.Error($"[FAIL] SafeDown FAIL WHILE UNDOCK" +
                        $"\nsafeDownSuccess : {safeDownSuccess}");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                    retVal.Reason = $"[FAIL] SafeDown FAIL WHILE UNDOCK " +
                        $"\nsafeDownSuccess : {safeDownSuccess}";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    LoggerManager.Debug($"Success GOP_DropPCardSafetyBig");
                }
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Move to ZCleared");
                    retVal.Reason = $"[FAIL] {this.GetType().Name} : Move to ZCleared";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_DropPCardSafetyBig Seq ErrMsg:{err.Message}, EventCode:{retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_DropPCardSafetyBig Seq ErrMsg:{err.Message}");
            }
            finally
            {
                double curXPos = 0;
                double curYPos = 0;
                double curZPos = 0;
                double curTPos = 0;
                
                this.MotionManager().GetActualPos(EnumAxisConstants.X, ref curXPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref curYPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.Z, ref curZPos);
                this.MotionManager().GetActualPos(EnumAxisConstants.C, ref curTPos);
                
                LoggerManager.Debug($"GPCC Card Contact Position X,Y,Z,T Pos : {curXPos}, {curYPos}, {curZPos}, {curTPos}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Card Pod을 Up
    [Serializable]
    public class GOP_RaisePCardPod : SequenceBehavior
    {
        public GOP_RaisePCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RaisePCardPod);
            this.SequenceDescription = "Check probe card pod raise";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                #region Pod 올리기 전 척 위치 확인 로직
                bool isDangerousPosition = false;// True: Dagerous Position, False: Safety Position
                var zaxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double curZpos = zaxis.Status.Position.Ref;
                double safetyZPos = this.CoordinateManager().StageCoord.SafePosZAxis;
                double posZMargin = 100.0; // 기존 ThreeLeg움직이기 전 체크하는 함수(CheckHardwareInterference())에서 사용된 값.
                bool doCardPodUp;
                bool doCardPodDown;

                if (curZpos > (safetyZPos + posZMargin))
                {
                    //위험한 위치이지만 이미 Card Pod이 올라가 있다면 안전장치로 부른 것이므로 넘어가도록 함.
                    var ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, out doCardPodUp);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = $"Output sensor error during verification for Chuck position safety check. {this.IOManager().IO.Outputs.DOUPMODULE_UP.Description}\n" +
                                                    $" ReadBit Value -> {this.IOManager().IO.Outputs.DOUPMODULE_UP.Value}\n" +
                                                    $"Error Behavior class name : {this.GetType().Name}\n";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, out doCardPodDown);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = $"Output sensor error during verification for Chuck position safety check. {this.IOManager().IO.Outputs.DOUPMODULE_DOWN.Description}\n" +
                                                    $" ReadBit Value -> {this.IOManager().IO.Outputs.DOUPMODULE_DOWN.Value}\n" +
                                                    $"Error Behavior class name : {this.GetType().Name}\n";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    if (doCardPodUp == true && doCardPodDown == false)
                    {
                        isDangerousPosition = false;
                    }
                    else
                    {
                        isDangerousPosition = true;// Safe Pos보다 높고, 현재 Pod이 내려가 있다라고 했을 때에는 Pod Up하면 안되도록 함.
                    }
                }
                else
                {
                    isDangerousPosition = false;
                }
                #endregion

                if (isDangerousPosition == false)
                {
                    StageStateEnum state = this.StageSupervisor().StageModuleState.GetState(); // CCStage아니면  Error 
                    if (state != StageStateEnum.CARDCHANGE)
                    {
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                        retVal.SetInnerError(new BehaviorResult()
                        {
                            ErrorCode = EventCodeEnum.GP_CARDCHANGE_STATE_ERROR,
                            Reason = "Not card change state.\n" +
                            $"Target state : {StageStateEnum.CARDCHANGE.ToString()}" +
                            $"current state : {state.ToString()}"
                        });
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    var ioretu = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, true);
                    //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                    if (ioretu != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = $"output sensor error {this.IOManager().IO.Outputs.DOUPMODULE_UP.Description}\n" +
                            $" writeBit -> {this.IOManager().IO.Outputs.DOUPMODULE_UP.Value}\n" +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    var ioretd = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, false);
                    //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                    if (ioretd != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = $"output sensor error {this.IOManager().IO.Outputs.DOUPMODULE_DOWN.Description}\n" +
                            $" writeBit -> {this.IOManager().IO.Outputs.DOUPMODULE_DOWN.Value}\n" +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }


                    IOPortDescripter<bool> DIUPMODULE_LEFT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR;
                    var rintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_LEFT_SENSOR, true, DIUPMODULE_LEFT_SENSOR.MaintainTime.Value, DIUPMODULE_LEFT_SENSOR.TimeOut.Value);
                    if (rintret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(DIUPMODULE_LEFT_SENSOR, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    IOPortDescripter<bool> DIUPMODULE_RIGHT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR;
                    var lintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_RIGHT_SENSOR, true, DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value, DIUPMODULE_RIGHT_SENSOR.TimeOut.Value);
                    if (lintret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        retVal.Reason = IOErrorDescription(DIUPMODULE_RIGHT_SENSOR, true, this.GetType().Name);
                        LoggerManager.Error(retVal.Reason);
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }


                    if ((ioretu == 0) && (ioretd == 0) && (rintret == 0) && (lintret == 0))
                    {
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_IS_DOWN_STATUS;
                        LoggerManager.Error($"GOP_RaisePCardPod(): EventCode:{retVal.ErrorCode}");
                        retVal.Reason = $"GOP_RaisePCardPod(): EventCode:{retVal.ErrorCode}\n";

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.MOTION_DANGEROUS_POS;
                    LoggerManager.Error($"GOP_RaisePCardPod(): EventCode:{retVal.ErrorCode}, " +
                        $"Unable to engage Card POD actuator on out of loading scope. Current Chuck Position Z = {curZpos:0.00}, " +
                        $"SafePosAxis Value Z = {safetyZPos:0.00}, Margin Z = {posZMargin:0.00}");

                    retVal.Reason = $"GOP_RaisePCardPod(): EventCode:{retVal.ErrorCode}, Chuck is currently in a location where CardPodUp is not possible.\n";

                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_RaisePCardPod Seq ErrMsg:{err.Message},  EventCode:{retVal.ErrorCode}\n";
                
                LoggerManager.Error($"Error Occurred while GOP_RaisePCardPod Seq ErrMsg:{err.Message},  EventCode:{retVal.ErrorCode}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Card Pod을 Down
    [Serializable]
    public class GOP_DropPCardPod : SequenceBehavior
    {
        public GOP_DropPCardPod()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DropPCardPod);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = new GOP_CheckCarrierIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_UP, false);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_DOWN, true);
                //IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                IOPortDescripter<bool> DIUPMODULE_LEFT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_LEFT_SENSOR;
                var rintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_LEFT_SENSOR, false, DIUPMODULE_LEFT_SENSOR.MaintainTime.Value, DIUPMODULE_LEFT_SENSOR.TimeOut.Value);
                if (rintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_LEFT_SENSOR, false, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return retVal;
                }



                IOPortDescripter<bool> DIUPMODULE_RIGHT_SENSOR = this.IOManager().IO.Inputs.DIUPMODULE_RIGHT_SENSOR;
                var lintret = this.IOManager().IOServ.MonitorForIO(DIUPMODULE_RIGHT_SENSOR, false, DIUPMODULE_RIGHT_SENSOR.MaintainTime.Value, DIUPMODULE_RIGHT_SENSOR.TimeOut.Value);
                if (lintret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = IOErrorDescription(DIUPMODULE_RIGHT_SENSOR, false, this.GetType().Name);
                    LoggerManager.Error(retVal.Reason);
                    return retVal;
                }



                command = new GOP_CheckPCardPodIsDown();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;

                LoggerManager.Error($"Error Occured while GOP_DropPCardPod Seq ErrMsg:{err.Message}");
            }
            return retVal;
        }
    }

    //==> ZIF 커맨드 전에 Test Header에 Dock 해도 되는지 체크 
    [Serializable]
    public class GOP_THDockReady : SequenceBehavior
    {
        public GOP_THDockReady()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_THDockReady);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                // mani down 확인 후 
                IOPortDescripter<bool> DITESTER_HEAD_HORI = this.IOManager().IO.Inputs.DITESTER_HEAD_HORI;
                var intret = this.IOManager().IOServ.MonitorForIO(DITESTER_HEAD_HORI, false, DITESTER_HEAD_HORI.MaintainTime.Value, DITESTER_HEAD_HORI.TimeOut.Value);// IO Maintain, TimeOut으로 쓰게한다? 
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IOPortDescripter<bool> DITH_DOWN = this.IOManager().IO.Inputs.DITH_DOWN;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_DOWN, true, DITH_DOWN.MaintainTime.Value, DITH_DOWN.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IOPortDescripter<bool> DITH_UP = this.IOManager().IO.Inputs.DITH_UP;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_UP, false, DITH_UP.MaintainTime.Value, DITH_UP.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.CardChangeModule().IsZifRequestedState(lock_request: false);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                
                retVal.ErrorCode = this.CardChangeModule().IsTeadClampLockRequestedState(lock_request: false);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                
                // testerhead lock == mani cannot up/down
                retVal.ErrorCode = this.CardChangeModule().IsTeadLockRequestedState(false);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // mani lock == mani cannot up/down
                intret = (int)this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOMANILOCK, true);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                retVal.ErrorCode = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occurred while GOP_THDockReady Seq ErrMsg:{err.Message}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> ZIF 커맨드 전에 Test Header에 UnDock 해도 되는지 체크 
    [Serializable]
    public class GOP_THUndockReady : SequenceBehavior
    {
        public GOP_THUndockReady()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_THUndockReady);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                // mani down 확인 후 
                IOPortDescripter<bool> DITESTER_HEAD_HORI = this.IOManager().IO.Inputs.DITESTER_HEAD_HORI;
                var intret = this.IOManager().IOServ.MonitorForIO(DITESTER_HEAD_HORI, false, DITESTER_HEAD_HORI.MaintainTime.Value, DITESTER_HEAD_HORI.TimeOut.Value);// IO Maintain, TimeOut으로 쓰게한다? 
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IOPortDescripter<bool> DITH_DOWN = this.IOManager().IO.Inputs.DITH_DOWN;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_DOWN, true, DITH_DOWN.MaintainTime.Value, DITH_DOWN.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IOPortDescripter<bool> DITH_UP = this.IOManager().IO.Inputs.DITH_UP;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_UP, false, DITH_UP.MaintainTime.Value, DITH_UP.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.CardChangeModule().IsZifRequestedState(lock_request: true);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

              
                // testerhead lock == mani cannot up/down
                retVal.ErrorCode = this.CardChangeModule().IsTeadLockRequestedState(true);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // mani lock == mani cannot up/down
                intret = (int)this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOMANILOCK, true);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }


                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occurred while GOP_THUndockReady Seq ErrMsg:{err.Message}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    //==> ZIF Dock 커맨드 후에 마더보드에 connect되었는지, 다른 연결이 풀리진 않았는지 체크 
    [Serializable]
    public class GOP_THDockClearedCheck : SequenceBehavior
    {
        public GOP_THDockClearedCheck()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_THDockClearedCheck);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {               

                // mani down 확인 후 
                IOPortDescripter<bool> DITESTER_HEAD_HORI = this.IOManager().IO.Inputs.DITESTER_HEAD_HORI;
                var intret = this.IOManager().IOServ.MonitorForIO(DITESTER_HEAD_HORI, false, DITESTER_HEAD_HORI.MaintainTime.Value, DITESTER_HEAD_HORI.TimeOut.Value);// IO Maintain, TimeOut으로 쓰게한다? 
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IOPortDescripter<bool> DITH_DOWN = this.IOManager().IO.Inputs.DITH_DOWN;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_DOWN, true, DITH_DOWN.MaintainTime.Value, DITH_DOWN.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                IOPortDescripter<bool> DITH_UP = this.IOManager().IO.Inputs.DITH_UP;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_UP, false, DITH_UP.MaintainTime.Value, DITH_UP.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.CardChangeModule().IsZifRequestedState(lock_request: true);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.CardChangeModule().IsTeadClampLockRequestedState(lock_request: true);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // testerhead lock == mani cannot up/down
                retVal.ErrorCode = this.CardChangeModule().IsTeadLockRequestedState(true);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // mani lock == mani cannot up/down
                intret = (int)this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOMANILOCK, true);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_THDockClearedCheck Seq ErrMsg:{err.Message}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> ZIF UnDock 커맨드 후에 마더보드에  disconnect되었는지, 다른 연결이 풀리진 않았는지 체크 
    [Serializable]
    public class GOP_THUndockClearedCheck : SequenceBehavior
    {
        public GOP_THUndockClearedCheck()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_THUndockClearedCheck);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DITESTER_HEAD_HORI = this.IOManager().IO.Inputs.DITESTER_HEAD_HORI;
                var intret = this.IOManager().IOServ.MonitorForIO(DITESTER_HEAD_HORI, false, DITESTER_HEAD_HORI.MaintainTime.Value, DITESTER_HEAD_HORI.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DITH_DOWN = this.IOManager().IO.Inputs.DITH_DOWN;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_DOWN, true, DITH_DOWN.MaintainTime.Value, DITH_DOWN.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                IOPortDescripter<bool> DITH_UP = this.IOManager().IO.Inputs.DITH_UP;
                intret = this.IOManager().IOServ.MonitorForIO(DITH_UP, false, DITH_UP.MaintainTime.Value, DITH_UP.TimeOut.Value);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.CardChangeModule().IsZifRequestedState(lock_request: false);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.CardChangeModule().IsTeadClampLockRequestedState(lock_request: false);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                
                // tester head lock == mani cannot up/down
                retVal.ErrorCode = this.CardChangeModule().IsTeadLockRequestedState(false);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                // mani unlock == mani can up/down
                intret = (int)this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOMANILOCK, false);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }



                retVal.ErrorCode = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occurred while GOP_THUndockClearedCheck Seq ErrMsg:{err.Message}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    #endregion

    #region ==> Fall Protector

    [Serializable]
    public class GOP_RecoveryPogoVacuumOff : SequenceBehavior
    {
        public GOP_RecoveryPogoVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RecoveryPogoVacuumOff);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                await Task.Delay(2000);
                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }
                await Task.Delay(2000);

                var vintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 15000);
                if (vintret != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occured while monitor for io of DIPOGOCARD_VACU_SENSOR in GOP_RecoveryPogoVacuumOff sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_OFF_ERROR;
                    return retVal;
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;

                LoggerManager.Error($"Error Occured while GOP_RecoveryPogoVacuumOff Seq ErrMsg:{err.Message}");
            }
            return retVal;
        }
    }
    //==> Top Plate Locker 를 Lcok
    [Serializable]
    public class GOP_TopPlateSolUnLock : SequenceBehavior
    {
        public GOP_TopPlateSolUnLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_TopPlateSolUnLock);
            this.SequenceDescription = "ROT unlock";
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {


                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.GP_CARDCHANGE_STATE_ERROR,
                        Reason = "Not card change state.\n" +
                        $"Target state : {StageStateEnum.CARDCHANGE.ToString()}" +
                        $"current state : {state.ToString()}"
                    });
                    return retVal;
                }

                //// vac on 
                //SequenceBehavior command = new GOP_PCardPodVacuumOn();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //// card On Topplate
                //command = new GOP_CheckPCardIsOnTopPlate();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }


                // unlock 동작 
                int ret = -1;
                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotUnLock(60000); // 실제 동작
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_UNLOCK_FAIL;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : CCRotUnLock() fail, {retVal.ErrorCode}");
                        retVal.Reason = $"Please check the ROT Axis.\n" +
                            $"TimeOut : 60000ms\n" +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return retVal;
                    }
                }
                else
                {

                    ret = StageCylinderType.MoveWaferCam.Retract();
                    if (ret != 0)
                    {
                        //ERrror
                        await this.MetroDialogManager().ShowMessageDialog("GOP_CardUnLock error", "MoveWaferCam bridge is Extended", EnumMessageStyle.Affirmative);

                        retVal.SetInnerError(new BehaviorResult() { ErrorCode = EventCodeEnum.STAGEMOVE_WAFER_CAM_EXPENDED, Reason = "Wafer Cam. Move Failed.\n" });

                        retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_UNLOCK_FAIL;
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        retVal.Reason = $"MoveWaferCam bridge is Extended. Please check the Wafer Bridge.\n " +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return retVal;
                    }
                    else // ret == 0
                    {
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotLock(60000);
                    }

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_UNLOCK_FAIL;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : CCRotUnLock, {retVal.ErrorCode}");
                        retVal.Reason = $"Please check the ROT Axis.\n" +
                            $"TimeOut : 60000ms\n" +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return retVal;
                    }
                }

                //command = new GOP_CheckTopPlateSolIsUnLock();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                this.CardChangeModule().IsCardStuck = false;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_TopPlateSolUnLock Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occured while GOP_TopPlateSolUnLock Seq ErrMsg:{err.Message}");
            }
            return retVal;
        }
    }

    //==> Top Plate Locker 를 Unlcok
    [Serializable]
    public class GOP_TopPlateSolLock : SequenceBehavior
    {
        public GOP_TopPlateSolLock()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_TopPlateSolLock);
            this.SequenceDescription = "ROT lock";
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                int ret = -1;
                

                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.RETRACT)
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotLock(60000);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_LOCK_FAIL;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : CCRotLock() fail, {retVal.ErrorCode}");
                        retVal.Reason = $"Please check the ROT Axis.\n" +
                            $"TimeOut : 60000ms\n" +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return retVal;
                    }
                }
                else
                {
                    ret = StageCylinderType.MoveWaferCam.Retract();
                    if (ret != 0)
                    {
                        //ERrror
                        await this.MetroDialogManager().ShowMessageDialog("GOP_TopPlateSolLock error", "MoveWaferCam bridge is Extended", EnumMessageStyle.Affirmative, "OK", "Cancel");
                        retVal.SetInnerError(new BehaviorResult() { ErrorCode = EventCodeEnum.STAGEMOVE_WAFER_CAM_EXPENDED, Reason = "Wafer Cam. Move Failed.\n" });
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }
                    else // ret == 0
                    {
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCRotLock(60000);
                    }

                    ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_LOCK_FAIL;
                        LoggerManager.Error($"[FAIL] {this.GetType().Name} : CCRotUnLock, {retVal.ErrorCode}");
                        retVal.Reason = $"Please check the ROT Axis.\n" +
                            $"TimeOut : 60000ms\n" +
                            $"Error Behavior class name : {this.GetType().Name}\n";
                        return retVal;
                    }
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occured while GOP_TopPlateSolLock Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occured while GOP_TopPlateSolLock Seq ErrMsg:{err.Message}, , EventCode = {retVal.ErrorCode}");
            }
            return retVal;

        }
    }
    /// <summary>
    /// Doaking시 Card가 상판에 붙고 ROT Lock이 완료 되었을 때 IsDocked를 업데이트해 주는 Behavior
    /// </summary>
    [Serializable]
    public class GOP_TopPlateSolLocked : SequenceBehavior
    {
        public GOP_TopPlateSolLocked()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_TopPlateSolLocked);
            this.SequenceDescription = "ROT locked";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                bool isFls = false;
                bool isRls = false;

                this.StageSupervisor().MotionManager().IsFls(EnumAxisConstants.ROT, ref isFls);
                this.StageSupervisor().MotionManager().IsRls(EnumAxisConstants.ROT, ref isRls);

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    isFls = true;
                    isRls = false;
                }

                if (isFls && isRls == false)
                {
                    this.CardChangeModule().SetIsDocked(true);
                    
                    retVal.ErrorCode = this.CardChangeModule().LoaderNotifyCardStatus();
                    
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GOP_TopPlateSolLocked(): LoaderNotifyCardStatus() Result is not NONE. Loader Notify error. ErrorCode ={retVal.ErrorCode}");
                    }

                    retVal.ErrorCode = EventCodeEnum.NONE;
                    
                    LoggerManager.Debug($"GOP_TopPlateSolLocked(): State. isFlst = {isFls}, isRls = {isRls}, SetIsDocked = true, ErrorCode ={retVal.ErrorCode}");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_LOCK_FAIL;
                    
                    LoggerManager.Debug($"GOP_TopPlateSolLocked(): State error. isFlst = {isFls}, isRls = {isRls}, ErrorCode ={retVal.ErrorCode}");
                }

                return Task.FromResult<IBehaviorResult>(retVal);
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                LoggerManager.Error($"Error Occurred while GOP_TopPlateSolLocked Seq ErrMsg:{err.Message},  Event Code : {retVal.ErrorCode}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    /// <summary>
    /// Undoaking시 ROT Unock이 완료 되었을 때 IsDocked를 업데이트해 주는 Behavior
    /// </summary>
    [Serializable]
    public class GOP_TopPlateSolUnlocked : SequenceBehavior
    {
        public GOP_TopPlateSolUnlocked()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_TopPlateSolUnlocked);
            this.SequenceDescription = "ROT unlocked";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                bool isFls = false;
                bool isRls = false;

                this.StageSupervisor().MotionManager().IsFls(EnumAxisConstants.ROT, ref isFls);
                this.StageSupervisor().MotionManager().IsRls(EnumAxisConstants.ROT, ref isRls);

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    isFls = false;
                    isRls = true;
                }

                if (isFls == false && isRls)
                {
                    this.CardChangeModule().SetIsDocked(false);
                    
                    retVal.ErrorCode = this.CardChangeModule().LoaderNotifyCardStatus();
                    
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Debug($"GOP_TopPlateSolUnlocked(): LoaderNotifyCardStatus() Result is not NONE. Loader Notify error. ErrorCode ={retVal.ErrorCode}");
                    }

                    retVal.ErrorCode = EventCodeEnum.NONE;
                    
                    LoggerManager.Debug($"GOP_TopPlateSolUnlocked(): State. isFlst = {isFls}, isRls = {isRls}, ErrorCode ={retVal.ErrorCode}");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CARD_ROT_UNLOCK_FAIL;
                    
                    LoggerManager.Debug($"GOP_TopPlateSolUnlocked(): State error. isFlst = {isFls}, isRls = {isRls}, ErrorCode ={retVal.ErrorCode}");
                }

                return Task.FromResult<IBehaviorResult>(retVal);
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                LoggerManager.Error($"Error Occurred while GOP_TopPlateSolUnlocked Seq ErrMsg:{err.Message},  Event Code : {retVal.ErrorCode}");
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    #endregion

    #region ==> Vacuum
    //==> Card Pod Vacuum On
    [Serializable]
    public class GOP_PCardPodVacuumOn : SequenceBehavior
    {
        public GOP_PCardPodVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_PCardPodVacuumOn);
            this.SequenceDescription = "Probe Card Pod Vacuum On";
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = $"output sensor error {this.IOManager().IO.Outputs.DOUPMODULE_VACU.Description}\n" +
                        $" writeBit -> {this.IOManager().IO.Outputs.DOUPMODULE_VACU.Value}\n" +
                        $"Error Behavior class name : {this.GetType().Name}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occurred while GOP_PCardPodVacuumOn Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_PCardPodVacuumOn Seq ErrMsg:{err.Message},  Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Card Pod Vacuum Off
    [Serializable]
    public class GOP_PCardPodVacuumOff : SequenceBehavior
    {
        public GOP_PCardPodVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_PCardPodVacuumOff);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.GP_CARDCHANGE_STATE_ERROR,
                        Reason = "Not card change state.\n" +
                        $"Target state : {StageStateEnum.CARDCHANGE.ToString()}" +
                        $"current state : {state.ToString()}"
                    });
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                IOResultValidation(MethodBase.GetCurrentMethod(), ioret);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    retVal.Reason = $"output sensor error {this.IOManager().IO.Outputs.DOUPMODULE_VACU.Description}\n" +
                        $" writeBit -> {this.IOManager().IO.Outputs.DOUPMODULE_VACU.Value}\n" +
                        $"Error Behavior class name : {this.GetType().Name}\n";
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                retVal.ErrorCode = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occurred while GOP_PCardPodVacuumOff Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Debug($"Error Occurred while GOP_PCardPodVacuumOff Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    [Serializable]
    public class GOP_PCardPodUndockVacuumOn : SequenceBehavior
    {
        public GOP_PCardPodUndockVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_PCardPodUndockVacuumOn);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeSysParam.GP_Undock_CardContactPosZ;

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var vintret1 = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, true, 1000, 15000);
                if (vintret1 != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_L in GOP_PCardPodUndockVacuumOn sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

                var vintret2 = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, true, 1000, 15000);
                if (vintret2 != 0)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR in GOP_PCardPodUndockVacuumOn sequence");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARRIER_NOT_ON_POD;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }


                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        LoggerManager.Debug($"Undock Z up retry : {i + 1} / 5, ZPos:{zAbsPos}");
                        
                        zAbsPos += 100;
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                        var intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 5000);
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                        ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                        intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 5000);

                        vintret1 = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, true, 1000, 15000);
                        if (vintret1 != 0)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_L in GOP_PCardPodUndockVacuumOn sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                            return Task.FromResult<IBehaviorResult>(retVal);
                        }

                        vintret2 = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, true, 1000, 15000);
                        if (vintret2 != 0)
                        {
                            LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_TOUCH_SENSOR_R in GOP_PCardPodUndockVacuumOn sequence");
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                            return Task.FromResult<IBehaviorResult>(retVal);
                        }
                    }

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            zAbsPos -= 100;
                            
                            LoggerManager.Debug($"Undock Z down retry {i + 1} / 10, ZPos:{zAbsPos}");
                            
                            ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);
                            var intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, false, 1000, 5000);
                            retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                            retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                            ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, true);
                            intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 5000);

                            vintret1 = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_L, true, 1000, 15000);
                            if (vintret1 != 0)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_VACU_SENSOR_L in GOP_PCardPodUndockVacuumOn sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                                return Task.FromResult<IBehaviorResult>(retVal);
                            }
                            else
                            {
                                retVal.ErrorCode = EventCodeEnum.NONE;
                            }

                            vintret2 = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_TOUCH_SENSOR_R, true, 1000, 15000);
                            if (vintret2 != 0)
                            {
                                LoggerManager.Error($"[FAIL] {this.GetType().Name} : Error occurred while monitor for io of DIUPMODULE_VACU_SENSOR_R in GOP_PCardPodUndockVacuumOn sequence");
                                retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_POD_VAC_ON_ERROR;
                                return Task.FromResult<IBehaviorResult>(retVal);
                            }
                            else
                            {
                                retVal.ErrorCode = EventCodeEnum.NONE;
                            }
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occutred while GOP_PCardPodVacuumOn Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    //==> Pogo에 Card가 접촉되는 면에 Vacuum On
    [Serializable]
    public class GOP_PogoPCardContactVacuumOn : SequenceBehavior
    {
        public GOP_PogoPCardContactVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_PogoPCardContactVacuumOn);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                //if (state != StageStateEnum.CARDCHANGE)
                //{
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                //    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                //    return retVal;
                //}

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    return retVal;
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return retVal;
                }

                ProbeAxisObject zAxis = this.MotionManager().GetAxis(EnumAxisConstants.Z);
                double zAbsPos = cardChangeDevParam.GP_CardContactPosZ + cardChangeSysParam.GP_ContactCorrectionZ;


                //IORet writeioret = IORet.ERROR;

                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                var intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 20000);
                if (intioret != 0)
                {

                    for (int i = 0; i < 5; i++)
                    {
                        zAbsPos += 100;
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                        //Task.Delay(1000).Wait();
                        intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 1500);
                        retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                        this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                        intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);

                        if (intioret == 0)
                            break;
                    }

                    if (intioret != 0)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            zAbsPos -= 100;
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                            //Task.Delay(1000).Wait();
                            intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 1500);
                            retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(zAxis, zAbsPos, zAxis.Param.Speed.Value / 10, zAxis.Param.Acceleration.Value / 2);
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                            this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, true);
                            intioret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, true, 3000);

                            if (intioret == 0)
                                break;
                        }
                    }
                }

                var vintret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIUPMODULE_VACU_SENSOR, true, 1000, 15000);

                if (vintret != 0)
                {
                    //배큠 끄고 릴리즈 불어주고 내리는 시퀀스  
                    //문제임 개문제임 
                    var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOTPLATE_PCLATCH_LOCK, false);

                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 1000, 15000);

                    if (intioret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 1000, 15000);
                    if (intioret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    //intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, false, 15000);
                    //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                    //intret = this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, true, 15000);
                    //IntResultValidation(MethodBase.GetCurrentMethod(), intret);

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    await Task.Delay(3000);

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    await Task.Delay(3000);

                    intioret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 5000);
                    if (intioret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }

                    //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 15000);

                    ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                    if (ioret != IORet.NO_ERR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                        return retVal;
                    }
                    await Task.Delay(5000);

                    // TODO : Check
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_MOVE_ERROR;
                        return retVal;
                    }

                    // TODO : Check

                    //await messageDialog.ShowDialog("POGO Vaccum error", "POGO Vaccum Sensor Not Detected", "OK", "Cancel", EnumMessageStyle.Affirmative);
                    await this.MetroDialogManager().ShowMessageDialog("POGO Vacuum error", "POGO Vacuum Sensor Not Detected", EnumMessageStyle.Affirmative, "OK", "Cancel");

                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_CARD_AND_POGO_CONTACT_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_PogoPCardContactVacuumOn Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }
    }

    //==> Pogo에 Card가 접촉되는 면에 Vacuum Off
    [Serializable]
    public class GOP_PogoPCardContactVacuumOff : SequenceBehavior
    {
        public GOP_PogoPCardContactVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_PogoPCardContactVacuumOff);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                await Task.Delay(1000);

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                await Task.Delay(7000);

                //      Move To when chuck down 
                //ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                //if (ioret != IORet.NO_ERR)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                //    return retVal;
                //}

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 1000, 15000);

                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, false, 15000);

                bool dipogocard_vacu_sensor;

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR, out dipogocard_vacu_sensor);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                if (dipogocard_vacu_sensor == true)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_POGO_SIDE_VAC_OFF_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

                //if (this.IOManager().IO.Inputs.DIPOGOCARD_VACU_SENSOR.Value)
                //{
                //    retVal.ErrorCode = EventCodeEnum.GP_CardChange_POGO_SIDE_VAC_OFF_ERROR;
                //    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                //}
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_PogoPCardContactVacuumOff Seq ErrMsg:{err.Message}");
            }
            finally
            {
                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOCARD_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                }
            }
            return retVal;
        }
    }

    //==> 상판에 Tester가 접촉되는 면에 Vacuum On
    [Serializable]
    public class GOP_UpPlateTesterContactVacuumOn : SequenceBehavior
    {
        public GOP_UpPlateTesterContactVacuumOn()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_UpPlateTesterContactVacuumOn);
        }

        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, true, 1000, 15000);

                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, true, 15000);

                //await Task.Delay(1000);

                bool DIFOGOTESTERVACU;

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out DIFOGOTESTERVACU);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                if (DIFOGOTESTERVACU == false)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_ON_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"Error Occurred while GOP_UpPlateTesterContactVacuumOn Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> 상판에 Tester가 접촉되는 면에 Vacuum Off
    [Serializable]
    public class GOP_UpPlateTesterContactVacuumOff : SequenceBehavior
    {
        public GOP_UpPlateTesterContactVacuumOff()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_UpPlateTesterContactVacuumOff);
        }

        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                var ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                await Task.Delay(1000);

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE, true);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }
                await Task.Delay(6000);

                ioret = this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOPOGOTESTER_VACU_RELEASE, false);
                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                var intret = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, false, 1000, 15000);
                if (intret != 0)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                //this.IOManager().IOServ.WaitForIO(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, false, 15000);

                bool dipogotester_vacu_sensor;

                ioret = this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DIPOGOTESTER_VACU_SENSOR, out dipogotester_vacu_sensor);

                if (ioret != IORet.NO_ERR)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_IO_ERROR;
                    return retVal;
                }

                if (dipogotester_vacu_sensor == true)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_TOP_PLATE_AND_TESTER_VAC_OFF_ERROR;
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Vacu");
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_UpPlateTesterContactVacuumOff Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }
    }
    #endregion


    #region ==> Align
    //==> Align 수행
    [Serializable]
    public class GOP_CardAlign : SequenceBehavior
    {
        public GOP_CardAlign()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CardAlign);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[MPT CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                //==> Card Align
                EventCodeEnum result = this.GPCardAligner().Align();
                if (result != EventCodeEnum.NONE)
                {
                    retVal.ErrorCode = result;

                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"Error Occurred while GOP_CardAlign Seq ErrMsg:{err.Message}");
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    //==> Chuck 위치를 Pogo에 Card Contact 한 위치(X,Y,T)로 이동
    [Serializable]
    public class GOP_SetAlignPos : SequenceBehavior
    {
        public GOP_SetAlignPos()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_SetAlignPos);
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (cardChangeSysParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Device parameter is Not Setted");
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_ERROR_OCCURED_WHILE_CHUCK_MOVE_TO_POGO;
                    return Task.FromResult<IBehaviorResult>(retVal);

                }

                ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject tAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
                //this.MotionManager().AbsMove(xAxis, cardChangeParam.GP_CardContactPosX, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                //this.MotionManager().AbsMove(yAxis, cardChangeParam.GP_CardContactPosY, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                //this.MotionManager().AbsMove(EnumAxisConstants.C, (cardChangeParam.GP_CardContactPosT));
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(xAxis, cardChangeSysParam.CardDockPosX.Value, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(yAxis, cardChangeSysParam.CardDockPosY.Value, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(tAxis, cardChangeSysParam.CardDockPosT.Value, tAxis.Param.Speed.Value, tAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return Task.FromResult<IBehaviorResult>(retVal); }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occurred while GOP_SetAlignPos Seq ErrMsg:{err.Message}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    #endregion

    #region //==> GOP_RetractWaferCam
    [Serializable]
    public class GOP_RetractWaferCam : SequenceBehavior
    {
        public GOP_RetractWaferCam()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_RetractWaferCam);
            this.SequenceDescription = "Check location of the wafer camera.";
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //===> Check WaferCam and Retract Bridge
                int iret = -1;
                if (StageCylinderType.MoveWaferCam.State == CylinderStateEnum.EXTEND)
                {
                    iret = StageCylinderType.MoveWaferCam.Retract();
                    if (iret != 0)
                    {
                        retVal.ErrorCode = EventCodeEnum.STAGEMOVE_WAFER_CAM_EXPENDED;
                        retVal.Reason = $"Error Occurred while GOP_RetractWaferCam Seq ErrMsg:{retVal.ErrorCode}";

                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }

                retVal.ErrorCode = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occurred while GOP_RetractWaferCam Seq ErrMsg:{err.Message}";
                
                LoggerManager.Error($"Error Occurred while GOP_RetractWaferCam Seq ErrMsg:{err.Message}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    #endregion

    #region ==> SetCardContactPos
    [Serializable]
    public class GOP_SetCardContactPos : SequenceBehavior
    {
        public GOP_SetCardContactPos()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_SetCardContactPos);
            this.SequenceDescription = "Move card contact position";
        }
        public override Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
               
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (cardChangeSysParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change System Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ICardChangeDevParam cardChangeDevParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
                if (cardChangeDevParam == null)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = EventCodeEnum.PARAM_ERROR,
                        Reason = "Card Change Device Parameter is null\n"
                    });
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Parameter is Not Setted");
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                ProbeAxisObject xAxis = this.MotionManager().GetAxis(EnumAxisConstants.X);
                ProbeAxisObject yAxis = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                ProbeAxisObject tAxis = this.MotionManager().GetAxis(EnumAxisConstants.C);
         

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(xAxis, cardChangeSysParam.CardDockPosX.Value, xAxis.Param.Speed.Value, xAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = retVal.ErrorCode,
                        Reason = "CC_AxisMoveToPos() fail.\n" +
                        $"{xAxis.AxisType.Value} Axis \n" +
                        $"CardDockPosX : {cardChangeSysParam.CardDockPosX.Value} \n" +
                        $"Speed : {xAxis.Param.Speed.Value}\n" +
                        $"Acceleration : {xAxis.Param.Acceleration.Value}\n"
                    });
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(yAxis, cardChangeSysParam.CardDockPosY.Value, yAxis.Param.Speed.Value, yAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = retVal.ErrorCode,
                        Reason = "CC_AxisMoveToPos() fail.\n" +
                       $"{yAxis.AxisType.Value} Axis \n" +
                       $"CardDockPosY : {cardChangeSysParam.CardDockPosY.Value} \n" +
                       $"Speed : {yAxis.Param.Speed.Value}\n" +
                       $"Acceleration : {yAxis.Param.Acceleration.Value}\n"
                    });
                    return Task.FromResult<IBehaviorResult>(retVal);
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(tAxis, cardChangeSysParam.CardDockPosT.Value, tAxis.Param.Speed.Value, tAxis.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    retVal.SetInnerError(new BehaviorResult()
                    {
                        ErrorCode = retVal.ErrorCode,
                        Reason = "CC_AxisMoveToPos() fail.\n" +
                      $"{tAxis.AxisType.Value} Axis \n" +
                      $"CardDockPosT : {cardChangeSysParam.CardDockPosT.Value} \n" +
                      $"Speed : {tAxis.Param.Speed.Value}\n" +
                      $"Acceleration : {tAxis.Param.Acceleration.Value}\n"
                    });
                    return Task.FromResult<IBehaviorResult>(retVal);
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occurred while GOP_SetAlignPos Seq ErrMsg:{err.Message}, EventCode = {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_SetAlignPos Seq ErrMsg:{err.Message},  EventCode = {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }
    #endregion

    #region ==> Recovery
    //==> Card 가 Stuck 되어 있을때 Recovery
    public class GOP_PCardSutckRecovery : SequenceBehavior
    {
        public GOP_PCardSutckRecovery()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_PCardSutckRecovery);
        }

        public override async Task<IBehaviorResult> Run()
        {
            //==> Tester의 홀더에 있는 Prober Card를 Undocking 시킨다.
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            
            bool cardDockError = false;
            
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }


                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] Fail to Clear state");
                    return retVal;
                }


                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();

                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                // ==== Recovery하는 상황 정의 ====
                // 파라미터가 잘못 들어갔다던가 
                // card door에 척쪽 브라켓이 걸렸을 경우--> card door 랑 chuck 이랑 걸려서 걸린채로 움직이면 사고남. 확인할 수 있는 센서가 없어서 쿨런트 케이블 쪽 브라켓 파손되고 축을 죽을려나...? 메뉴얼로 빼야함. 
                // card door에 카드랑 gem 났을 경우 --> 테스터 연결X, 카드O, 캐리어O 이면 ccCleard 후 센터로 이동, 카드가 이미 dock 되어있을 수도 있으니 더이상 움직이지 않는다. 
                // tester unlock 안하고 undock 했을 경우 --> tester 에 붙은 채로  z down 을 했을 것임. 캐리어 있는지 확인하고 있고 카드 없으면 rot pos 만큼  ZUP해서 tester unlock 해야함. 
                // 카드 있는데 dock 할 경우 --> 못막음. 그러므로 만약 카드가 dock 되어있는데 Raise 했으면 축 다 죽음. 메뉴얼로 빼내야함... 
                // 캐리어 없는데 undock하는 경우 --> 이미 undock을 했다면 카드는 척위로 떨어졌을 것. 축 다 죽음. 손으로 빼내야함... 
                // 카드가 가이드 핀에 잘 안맞아서 RaiseChuck했다가 gem 났을 경우 --> 캐리어가 척에 있다면 upmodule 올리고 캐리어 있는지 확인하고 rot unlock 한다음 z down 해야함. rot unlock 하다가 또 끼이면? 
                // ================================
                // ... 일어날 수 없다고 생각하지만 센서가 고장나서 혹시나 혹시나 하는 상황 정의 해보았습니다.. 

                // ==== Recovery하는 동작 정의 ====
                // carrier 있으면 && 카드가 팟에 있음 && 테스터 연결 되어잇음, (zif 락하고 unlock?) : tester 연결 되어있으면 끝냄... 메뉴얼로...
                // carrier 있으면 && 카드가 팟에 있음 && 테스터 연결 안되어있음 (카드문에 gem, 카드-상판 gem) : zdown, MoveToCenter 후 카드 있는지 확인하고 card dock(zup, rot unlock, zup, rot lock). 
                // carrier 있는데 && 카드가 팟에 없으면 : zdown, MoveToCenter : 하지만 docking 되어있는지는 모름, zup해서 card 있는지 확인하고 없으면 error, card detect sensor 필요... 
                // =======> 어쨌든 ROT 시작 전에 캐리어가 있으면 에서 상황이므로 CarrierOnPod Error 내보내기 

                //0 carrier 없으면 가만히.: 하지만 docking 되어있는지는 모름, 사고 난 상황일 수도 있으니 z up 위험해서 그냥 error 내보내기
                // ================================


                SequenceBehavior command = null;

                //<조건 검사>
                command = new GOP_CheckPCardIsOnTopPlate();
                retVal = await command.Run();
                if (retVal.ErrorCode == EventCodeEnum.NONE)
                {
                    // 카드가 상판에 잇거나 스테이지에 없음.

                    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");

                    command = new GOP_CheckPCardIsNotOnCarrier();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    // 카드를 보러감.

                    //==> CC Clearance                 
                    command = new GOP_DropChuckSafety();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    //==> MoveToCenter
                    command = new GOP_SetCardContactPos();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    command = new GOP_CheckCarrierIsOnPCardPod();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    command = new GOP_RaiseChuckToTopPlate();//==> z UP
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    command = new GOP_CheckPCardIsExist();//=> 체크
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");

                        command = new GOP_DropChuckSafety();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                            return retVal;
                        }

                        return retVal;
                    }

                    command = new GOP_DropChuckSafety();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    // 카드가 있으면 정상.

                }
                else
                {// 카드가 팟에 있는 경우

                    command = new GOP_CheckPCardIsNotOnTopPlate();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        return retVal;
                    }

                    command = new GOP_THDockReady();//=> 테스터랑 연결이 안되어있는 지확인
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");

                        // 테스터랑 연결이 되어있으면 그냥 끝냄
                        command = new GOP_THDockClearedCheck();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            return retVal;
                        }

                        command = new GOP_DropChuckSafety();
                        retVal = await command.Run();
                        if (retVal.ErrorCode != EventCodeEnum.NONE)
                        {
                            LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                            return retVal;
                        }

                        cardDockError = false;
                        this.CardChangeModule().IsCardStuck = false;
                        return retVal; // 끝!

                    }

                    command = new GOP_DockPCardTopPlate();//=> card dock(moveToCenter, zup, rot unlock, zup, rot lock, drop, check)
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                    command = new GOP_DropChuckSafety();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                        return retVal;
                    }

                }

                // 아래로는 카드가 상판에 있을 경우 동작 

                command = new GOP_THDockReady();//=> 테스터랑 연결이 안되어있는 지확인
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");

                    // 테스터랑 연결이 되어있으면 그냥 끝냄
                    command = new GOP_THDockClearedCheck();
                    retVal = await command.Run();
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        return retVal;
                    }


                    cardDockError = false;
                    this.CardChangeModule().IsCardStuck = false;// 끝!
                    return retVal;
                }

                command = new Request_ZIFCommandLowActive();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}");
                    return retVal;
                }

                //bool isRecoverySuccess = false;
                command = new GOP_THDockClearedCheck();
                retVal = await command.Run();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    //IO Error  
                    //isRecoverySuccess = false;
                    cardDockError = true;

                    this.CardChangeModule().IsCardStuck = true;
                    //return retVal;
                }
                else
                {
                    this.CardChangeModule().IsCardStuck = false;
                    return retVal;
                }

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] Fail to Clear state");
                    return retVal;
                }

                this.CardChangeModule().IsCardStuck = false;

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_PCardSutckRecovery Seq ErrMsg:{err.Message}");
            }
            finally
            {

                if (cardDockError)
                {
                    retVal.ErrorCode = EventCodeEnum.GP_CardChange_DOCK_FAIL;
                }
                else
                {
                    LoggerManager.Debug($" GOP_PCardSutckRecovery Seq End cardDockError:{cardDockError}");
                }
            }

            return retVal;
        }
    }
    #endregion

    #region ==> Dock : 직접 사용하는 Command
    //==> Card를 Loader로 부터 받기 위한 준비
    [Serializable]
    public class GOP_ReadyToGetCard : SequenceBehavior
    {
        public GOP_ReadyToGetCard()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ReadyToGetCard);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {              
                //==> Stage move 상태를 GP Card Change 상태로 바꾼다.
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.LockCCState();
                ICardChangeSysParam cardChangeSysParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                StageStateEnum state = this.StageSupervisor().StageModuleState.GetState();
                if (state != StageStateEnum.CARDCHANGE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : It is not Card Change state");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                SequenceBehavior command = null;

                //==> 척을 안전한 Z 위치로 이동
                command = new GOP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> 척을 contactpos로 card exist sensor봐야함.
                command = new GOP_SetCardContactPos();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //<조건 검사>
                //==> 캐리어가 Card Pod에 없는지 확인
                command = new GOP_CheckCarrierIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //<조건 검사>
                //==> Card가 TopPlate에 없는지 확인
                command = new GOP_CheckPCardIsNotOnTopPlate();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }


                command = new GOP_TopPlateSolLock();//=> Rot Lock
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Chuck을 Loader 쪽으로 이동 시킴
                command = new GOP_MoveChuckToLoader();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> Card pod을 올림
                command = new GOP_RaisePCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;

                LoggerManager.Error($"Error Occured while GOP_ReadyToGetCard Seq ErrMsg:{err.Message}");
            }
            return retVal;
        }
    }

    //==> Card를 Loader로 부터 받기 위한 준비
    [Serializable]
    public class GOP_ReturnCardCarrier : SequenceBehavior
    {
        public GOP_ReturnCardCarrier()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ReturnCardCarrier);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;

                //<조건 검사>
                //다시 로딩포지션으로 가고 카드도어 열고 캐리어 다시 돌려줌
                //==> Card가 Card Pod에 있는지 확인 (캐리어가 있어야됨)
                command = new GOP_CheckCarrierIsOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                command = new GOP_DropChuckSafety();//==> Chuck을 안전한 Z 위치로 이동
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> {this.GetType().Name} EventCode:{retVal.ErrorCode}"); this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL); return retVal; }

                //==> Chuck을 Loader 쪽으로 이동 시킴 (로딩포지션으로 이동하고)
                command = new GOP_MoveChuckToLoader();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

                //==> vac off
                command = new GOP_PCardPodVacuumOff();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { LoggerManager.Error($"[GOP CC]=> EventCode:{retVal.ErrorCode}"); return retVal; }

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_ReturnCardCarrier Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }
    }


    [Serializable]
    public class GOP_CardChangeVaildation : SequenceBehavior
    {
        string Cardid = null;
        public GOP_CardChangeVaildation(string cardid)
        {
            this.Cardid = cardid;
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_CardChangeVaildation);
        }

        public override async Task<IBehaviorResult> Run()
        {
            string waitCancelDialogHashCode = this.GetHashCode().ToString();

            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            
            IBehaviorResult retVal = new BehaviorResult();
            System.Diagnostics.Stopwatch elapsedStopWatch = new System.Diagnostics.Stopwatch();
            
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                
                if (ccSysParam.WaitTesterResponse.Value == false || this.Cardid.ToLower() == "holder")
                {
                    LoggerManager.Debug($"GOP_CardChangeVaildation(): Tester Interlock Seq Skip [{ccSysParam.WaitTesterResponse.Value} , {this.Cardid}]");
                    
                    return retVal;
                }

                int timeout = 120000;
                elapsedStopWatch.Reset();
                elapsedStopWatch.Start();

                bool runFlag = true;
                bool result = false;
                string messag = string.Empty;

                await this.MetroDialogManager().SetDataWaitCancelDialog(message: "Waiting for tester response", hashcoe: waitCancelDialogHashCode, cancelButtonText: "Abort", restarttimer: true);

                do
                {
                    var Cardinfo = this.TCPIPModule().CardchangeTempInfo;
                    if (Cardinfo != null)
                    {
                        if (Cardinfo.Result)
                        {
                            string Tcardid = Cardinfo.Cardid;
                            if (this.Cardid == Tcardid)
                            {
                                result = true;
                                runFlag = false;
                                
                                LoggerManager.Debug($"GOP_CardChangeVaildation(): ProbeCard({this.Cardid}).");
                                
                                retVal.ErrorCode = EventCodeEnum.NONE;
                            }
                            else
                            {
                                runFlag = false;
                                retVal.ErrorCode = EventCodeEnum.CARD_CHANGE_FAIL;
                                messag = $"[Invaild Parameter] (ProbeCard ID) Tester : {Tcardid} Prober: {this.Cardid}.";
                            }
                        }
                        else
                        {
                            runFlag = false;
                            retVal.ErrorCode = EventCodeEnum.CARD_CHANGE_FAIL;
                            messag = $"[Invaild Parameter] (ProbeCard ID) Tester rejected Card change..";
                        }
                    }

                    if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                    {
                        runFlag = false;
                        messag = $"Timeout occurred. Timeout = {timeout}ms.";
                        retVal.ErrorCode = EventCodeEnum.TESTER_RESPONDS_TIMEOUT;
                    }

                    if (waitCancelDialogHashCode == this.StageSupervisor().GetWaitCancelDialogHashCode())
                    {
                        runFlag = false;
                        messag = $"Cancel by user.";
                        retVal.ErrorCode = EventCodeEnum.CARD_CHANGE_FAIL;
                    }
                    Thread.Sleep(1000);
                } while (runFlag == true);

                if (result == false)
                {
                    LoggerManager.Debug($"GOP_CardChangeVaildation(): {messag}");
                    
                    this.CardChangeModule().ReasonOfError.AddEventCodeInfo(retVal.ErrorCode, messag, this.GetType().Name);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_CardChangeVaildation Seq ErrMsg:{err.Message}");
                
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            finally
            {
                this.TCPIPModule().CardchangeTempInfo = null;
                this.StageSupervisor().SetWaitCancelDialogHashCode(string.Empty);

                await this.MetroDialogManager().ClearDataWaitCancelDialog(restarttimer: true);
            }
            return retVal;
        }
    }

    //==> Card Docking이 완료 된 후 정리 작업을 수행
    [Serializable]
    public class GOP_ClearCardDocking : SequenceBehavior
    {
        public GOP_ClearCardDocking()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ClearCardDocking);
        }
        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
                
                SequenceBehavior command = null;

                command = new GOP_CheckPCardIsOnTopPlate();//=> Card가 Pogo에 제대로 흡착 되어 있는지 확인
                retVal = await command.Run();
                
                if (retVal.ErrorCode != EventCodeEnum.NONE) 
                { 
                    return retVal; 
                }

                command = new GOP_CheckCarrierIsNotOnPCardPod();//=> 캐리어가 가 Card Pod에 없는지 확인
                retVal = await command.Run();
                
                if (retVal.ErrorCode != EventCodeEnum.NONE) 
                { 
                    return retVal; 
                }

                command = new GOP_DropPCardPod();//=> Card Pod을 내림
                retVal = await command.Run();
                
                if (retVal.ErrorCode != EventCodeEnum.NONE) 
                { 
                    return retVal; 
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] MOVE To Zcleared");

                    return retVal;
                }
                
                LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name} Done");

                if (retVal.ErrorCode == EventCodeEnum.NONE)
                {
                    this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_SUCCESS);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_ClearCardDocking Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }
            return retVal;
        }
    }
    //==> Card UnDocking이 완료된 후 정리 작업을 수행
    [Serializable]
    public class GOP_ClearCardUnDocking : SequenceBehavior
    {
        public GOP_ClearCardUnDocking()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ClearCardUnDocking);
            this.SequenceDescription = "finish card undocking";
        }
        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                SequenceBehavior command = null;

                //<완료 검사>
                command = new GOP_CheckPCardIsNotOnTopPlate();//=> 상판에 Card가 없는지 확인.
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                command = new GOP_CheckCarrierIsOnPCardPod();//=> Card Pod에 카드 있는지 검사 ==> GOP 구조상 못함. 
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                command = new GOP_PCardPodVacuumOff();//=> Card Pod Vacuum 을 Off 함으로서 Loader가 Card를 가져 갈 수 있도록 준비
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_ClearCardUnDocking Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return retVal;
        }
    }



    //==> Card Chagne가 완료된 후 정리 작업을 수행
    [Serializable]
    public class GOP_ClearCardChange : SequenceBehavior
    {
        public GOP_ClearCardChange()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ClearCardChange);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;

                //==> Prober 상판에 카드 없는지 확인
                command = new GOP_CheckPCardIsNotOnTopPlate();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> Prober Card Pod에 캐리어 없는지 확인
                command = new GOP_CheckCarrierIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> 척을 안전한 Z 위치로 이동
                command = new GOP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }


                //=> Card Pod을 내림
                command = new GOP_DropPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> Chuck Theta를 0 도로 복원
                LoggerManager.Debug($"[GOP CC] Theta reset 0");

                //retVal.ErrorCode = this.MotionManager().AbsMove(EnumAxisConstants.C, 0);
                var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, 0, axist.Param.Speed.Value, axist.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Rotate Theta, T : 0");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] Fail to Clear state");
                    return retVal;
                }

                //==> Stage Move를  상태로 바꿈
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] Fail to Clear state");
                    return retVal;
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_ClearCardChange Seq ErrMsg:{err.Message}");
            }
            return retVal;
        }
    }

    [Serializable]
    public class GOP_WMBIdentifierVaildation : SequenceBehavior
    {        
        public GOP_WMBIdentifierVaildation()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_WMBIdentifierVaildation);
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();

                // Not_Used 로 Default 설정, string count 볼것.

                if (ccSysParam.WMBIdentifier?.Value == "NA" || string.IsNullOrEmpty(ccSysParam.WMBIdentifier?.Value))
                {
                    LoggerManager.Debug($"GOP_WMBIdentifierVaildation(): Not Used. WMBIdentifier:{ccSysParam.WMBIdentifier?.Value}");
                    retVal.ErrorCode = EventCodeEnum.NONE;// 사용 안함. 
                }
                else
                {
                  
                    int expected_int;
                    var expected_wmb0 = ccSysParam.WMBIdentifier.Value.Substring(0, 1);// 이게 문자이면 error 내기
                    if(int.TryParse(expected_wmb0, out expected_int) == false)
                    {
                        retVal.ErrorCode = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    var expected_wmb1 = ccSysParam.WMBIdentifier.Value.Substring(1, 1);
                    if (int.TryParse(expected_wmb1, out expected_int) == false)
                    {
                        retVal.ErrorCode = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    var expected_wmb2 = ccSysParam.WMBIdentifier.Value.Substring(2, 1);
                    if (int.TryParse(expected_wmb2, out expected_int) == false)
                    {
                        retVal.ErrorCode = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    var expected_wmb3 = ccSysParam.WMBIdentifier.Value.Substring(3, 1);
                    if (int.TryParse(expected_wmb3, out expected_int) == false)
                    {
                        retVal.ErrorCode = EventCodeEnum.PARAM_SET_INVALID_OUT_OF_RANGE;
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }

                    Func<bool, string> addstring = (val) =>
                    {
                        string ret = "0";
                        if (val == true)
                        {
                            ret = "1";
                        }
                        return ret;


                    };

                    Func<string, bool> convertToBool = (expected_value) =>
                    {
                        bool ret = false;
                        if (expected_value == "1")
                        {
                            ret = true;
                        }
                        return ret;
                    };


                    string parse = "";                    
                    var iorst = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIWMB0, convertToBool(expected_wmb0));
                    if(iorst == 0)
                    {
                        parse += addstring(convertToBool(expected_wmb0));
                    }
                    else
                    {
                        parse += "0";
                    }

                    iorst = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIWMB1, convertToBool(expected_wmb1));
                    if (iorst == 0)
                    {
                        parse += addstring(convertToBool(expected_wmb1));
                    }
                    else
                    {
                        parse += "0";
                    }

                    iorst = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIWMB2, convertToBool(expected_wmb2));
                    if (iorst == 0)
                    {
                        parse += addstring(convertToBool(expected_wmb2));
                    }
                    else
                    {
                        parse += "0";
                    }


                    iorst = this.IOManager().IOServ.MonitorForIO(this.IOManager().IO.Inputs.DIWMB3, convertToBool(expected_wmb3));
                    if (iorst == 0)
                    {
                        parse += addstring(convertToBool(expected_wmb3));
                    }
                    else
                    {
                        parse += "0";
                    }



                    if (ccSysParam.WMBIdentifier.Value == parse)
                    {
                        LoggerManager.Debug($"GOP_WMBIdentifierVaildation(): Success, parse value:{parse}");
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                    else
                    {
                        LoggerManager.Debug($"GOP_WMBIdentifierVaildation(): Failed, parse value:{parse}");
                        retVal.ErrorCode = EventCodeEnum.NOT_EXPECTED_TESTER_INTERFACE;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Error($"Error Occured while GOP_WMBIdentifierVaildation Seq ErrMsg:{err.Message}");

                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class GOP_UnLoadeCarrier : SequenceBehavior
    {
        public GOP_UnLoadeCarrier()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_UnLoadeCarrier);
        }
        public override async Task<IBehaviorResult> Run()
        {
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    return retVal;
                }

                SequenceBehavior command = null;

                ////==> Prober 상판에 카드 없는지 확인
                //command = new GOP_CheckPCardIsNotOnTopPlate();
                //retVal = await command.Run();
                //if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> Prober Card Pod에 캐리어 없는지 확인
                command = new GOP_CheckCarrierIsNotOnPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> 척을 안전한 Z 위치로 이동
                command = new GOP_DropChuckSafety();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }

                //=> Card Pod을 내림
                command = new GOP_DropPCardPod();
                retVal = await command.Run();
                if (retVal.ErrorCode != EventCodeEnum.NONE) { return retVal; }

                //==> Chuck Theta를 0 도로 복원
                LoggerManager.Debug($"[GOP CC] Theta reset 0");

                var axist = this.MotionManager().GetAxis(EnumAxisConstants.C);
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CC_AxisMoveToPos(axist, 0, axist.Param.Speed.Value, axist.Param.Acceleration.Value);
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[FAIL] {this.GetType().Name} : Rotate Theta, T : 0");
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                    return retVal;
                }

                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] Fail to Clear state");
                    return retVal;
                }

                //==> Stage Move를  상태로 바꿈
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.ZCLEARED();
                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    LoggerManager.Error($"[GOP CC] Fail to Clear state");
                    return retVal;
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;

                LoggerManager.Error($"Error Occured while GOP_UnLoadeCarrier Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }
    }
    //==> Card를 Pogo에 Docking
    [Serializable]
    public class GOP_DockPCardTopPlate : SequenceBehavior
    {
        public GOP_DockPCardTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_DockPCardTopPlate);
            this.SequenceDescription = "Dock probe card to top plate";
        }

        public override async Task<IBehaviorResult> Run()
        {
            //==> [Chuck에 있는 Prober Card를 Tester의 홀더에 Dock 시킨다.]

            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");
            
            //Undocking 시작하면 Docking sequence 상태초기화
            SequenceBehaviors undockBehaviors = this.CardChangeModule().CCUnDockBehaviors as SequenceBehaviors;
            foreach (var beh in undockBehaviors.SequenceBehaviorCollection)
            {
                beh.StateEnum = SequenceBehaviorStateEnum.IDLE;
            }

            IBehaviorResult retVal = new BehaviorResult();
            bool runflag = true;
            try
            {
                this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKING);

                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    SequenceBehavior Emul = null;
                    IBehaviorResult emul_ROT_LockedResult = new BehaviorResult();
                    Emul = new GOP_TopPlateSolLocked();
                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.DOCKED);
                    emul_ROT_LockedResult = await Emul.Run();
                    //return retVal;
                }
                SequenceBehavior command = null;
                this.CardChangeModule().CurBehaviorIdx = 0;
                IBehaviorResult retractWaferCamResult = new BehaviorResult();
                command = new GOP_RetractWaferCam();//wafer bridge 접기(확인차 접는거)
                retractWaferCamResult = await command.Run();

                SequenceBehaviors dockBehaviors = null;

                dockBehaviors = this.CardChangeModule().CCDockBehaviors as SequenceBehaviors;

                this.CommandManager().SetCommand<IStagecardChangeStart>(this, dockBehaviors);

                while (runflag)
                {
                    Thread.Sleep(200);

                    if (this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.ABORT)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ABORT;
                        runflag = false;
                    }
                    else if (this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.ERROR)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_DOCK_FAIL;
                        retVal.SetInnerError(this.CardChangeModule().GetExecutionResult());
                        runflag = false;
                    }
                    else if (this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.DONE
                        || this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.SUSPENDED)
                    {
                        runflag = false;
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_DockPCardTopPlate Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }
    }
    //==> ThreeLegDown, Up, Down부분 확인해서 True인지 False인지 확인하고 그다음 Down하는거임
    [Serializable]
    public class GOP_ThreeLegCheckAndDown : SequenceBehavior
    {
        public GOP_ThreeLegCheckAndDown()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_ThreeLegCheckAndDown);
            this.SequenceDescription = "Check ThreeLeg And Let Down";
        }
        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();

            try
            {
                //====> ThreeLegDown : Read To Get Card 
                bool isthreelegup = false;
                bool isthreelegdown = false;
                bool usinghandler = this.StageSupervisor().CheckUsingHandler(this.LoaderController().GetChuckIndex());

                // TODO : ThreeLeg Check 문제 찾고 고쳐야 함.
                if (usinghandler != true)
                {
                    retVal.ErrorCode = this.MotionManager().IsThreeLegUp(EnumAxisConstants.TRI, ref isthreelegup);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.Reason = $"Three Leg Up Value : {isthreelegup}";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                    retVal.ErrorCode = this.MotionManager().IsThreeLegDown(EnumAxisConstants.TRI, ref isthreelegdown);

                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        retVal.Reason = $"Three Leg Down Value : {isthreelegdown}";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }

                if (isthreelegup != false || isthreelegdown != true || usinghandler != true) // threelegdown
                {
                    retVal.ErrorCode = this.StageSupervisor().StageModuleState.Handlerrelease(15000);
                    if (retVal.ErrorCode != EventCodeEnum.NONE)
                    {
                        this.NotifyManager().Notify(EventCodeEnum.Three_Leg_Error);
                        retVal.Reason = $"isthreelegup : {isthreelegup}, isthreelegdown : {isthreelegdown}, usinghandler : {usinghandler}";
                        return Task.FromResult<IBehaviorResult>(retVal);
                    }
                }
            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                retVal.Reason = $"Error Occurred while GOP_ThreeLegCheckAndDown Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}";
                
                LoggerManager.Error($"Error Occurred while GOP_ThreeLegCheckAndDown Seq ErrMsg:{err.Message}, Event Code : {retVal.ErrorCode}");
            }

            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }


    #endregion
    //==> Card를 Pogo로 부터 UnDocking
    [Serializable]
    public class GOP_UndockPCardTopPlate : SequenceBehavior
    {
        public GOP_UndockPCardTopPlate()
        {
            this.Flag = BehaviorFlag.NONE;
            this.description = nameof(GOP_UndockPCardTopPlate);
            this.SequenceDescription = "Undock probe card to top plate";
        }

        public override async Task<IBehaviorResult> Run()
        {
            //==> Tester의 홀더에 있는 Prober Card를 Undocking 시킨다.
            LoggerManager.Debug($"[GOP CC]=> {this.GetType().Name}");

            //Undocking 시작하면 Docking sequence 상태초기화
            SequenceBehaviors dockBehaviors = this.CardChangeModule().CCDockBehaviors as SequenceBehaviors;
            foreach (var beh in dockBehaviors.SequenceBehaviorCollection)
            {
                beh.StateEnum = SequenceBehaviorStateEnum.IDLE;
            }

            IBehaviorResult retVal = new BehaviorResult();
            bool runflag = true;
            try
            {
                this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKING);
                if (Extensions_IParam.ProberRunMode == RunMode.EMUL)
                {
                    SequenceBehavior Emul = null;
                    IBehaviorResult emul_ROT_UnlockedResult = new BehaviorResult();
                    Emul = new GOP_TopPlateSolUnlocked();

                    this.CardChangeModule().SetCardDockingStatus(EnumCardDockingStatus.UNDOCKED);
                    emul_ROT_UnlockedResult = await Emul.Run();
                    //return retVal;
                }

                //====> Change CardChangeState
                retVal.ErrorCode = this.StageSupervisor().StageModuleState.CCZCLEARED();

                if (retVal.ErrorCode != EventCodeEnum.NONE)
                {
                    return retVal;
                }
                SequenceBehaviors undockBehaviors = null;
                this.CardChangeModule().CurBehaviorIdx = 0;
                undockBehaviors = this.CardChangeModule().CCUnDockBehaviors as SequenceBehaviors;

                this.CommandManager().SetCommand<IStagecardChangeStart>(this, undockBehaviors);
                while (runflag)
                {
                    Thread.Sleep(200);

                    if (this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.ABORT)
                    {
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_ABORT;
                        runflag = false;
                    }
                    else if (this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.ERROR)
                    {
                        runflag = false;
                        retVal.ErrorCode = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
                        retVal.SetInnerError(this.CardChangeModule().GetExecutionResult());
                    }
                    else if (this.CardChangeModule().CardChangeState.GetState() == CardChangeModuleStateEnum.DONE)
                    {
                        runflag = false;
                        retVal.ErrorCode = EventCodeEnum.NONE;
                    }
                }

            }
            catch (Exception err)
            {
                retVal.ErrorCode = EventCodeEnum.EXCEPTION;
                
                LoggerManager.Error($"Error Occured while GOP_UndockPCardTopPlate Seq ErrMsg:{err.Message}");
            }

            return retVal;
        }

        EventCodeEnum CheckforZCleared()
        {
            EventCodeEnum ret = EventCodeEnum.GP_CardChange_UNDOCK_FAIL;
            try
            {

                this.IOManager().IOServ.WriteBit(this.IOManager().IO.Outputs.DOUPMODULE_VACU, false);

                bool ditplate_pclatch_sensor_lock;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_LOCK, out ditplate_pclatch_sensor_lock);

                bool ditplate_pclatch_sensor_unlock;
                this.IOManager().IOServ.ReadBit(this.IOManager().IO.Inputs.DITPLATE_PCLATCH_SENSOR_UNLOCK, out ditplate_pclatch_sensor_unlock);
                if (ditplate_pclatch_sensor_lock == true && ditplate_pclatch_sensor_unlock == false)
                {
                    ret = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                this.NotifyManager().Notify(EventCodeEnum.CARD_CHANGE_FAIL);
                
                LoggerManager.Error($"Error Occured while GOP_UndockPCardTopPlate Seq ErrMsg:{err.Message}");
                
                ret = EventCodeEnum.EXCEPTION;
            }

            return ret;
        }
    }
}




#endregion




