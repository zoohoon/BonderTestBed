using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AlignEX;
using ProberInterfaces.Command.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SchedulerStandardModule
{
    public class WaferAlignScheduler : ISchedulingModule, INotifyPropertyChanged, ILotReadyAble
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IStateModule _WaferAlignModule;
        public IStateModule WaferAlignModule
        {
            get { return _WaferAlignModule; }
            set
            {
                if (value != _WaferAlignModule)
                {
                    _WaferAlignModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IParam DevParam { get; set; }
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

        public List<object> Nodes { get; set; }

        public WaferAlignScheduler()
        {
        }
        public WaferAlignScheduler(IStateModule Module)
        {
            _WaferAlignModule = Module;
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    WaferAlignModule = this.WaferAligner();
                    //init 하는 코드 하고 나서 Idle 상태로 초기화
                    //  SubModuleState = new SubModuleIdleState(this);

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

        public void DeInitModule()
        {
        }

        public bool IsExecute()
        {
            bool retVal = false;
            try
            {
                if (this.LotOPModule().ModuleStopFlag)
                {
                    retVal = false;
                    return retVal;
                }
                bool isWaferStateOK =
                    WaferAlignModule.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.IDLE &&
                    WaferAlignModule.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                    WaferAlignModule.StageSupervisor().WaferObject.GetSubsInfo().WaferType == EnumWaferType.STANDARD;

                if (WaferAlignModule.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if (WaferAlignModule.SequenceEngineManager().GetRunState() &&
                      isWaferStateOK &&
                      (WaferAlignModule.StageSupervisor().WaferObject.GetState() == EnumWaferState.UNPROCESSED ||
                      WaferAlignModule.StageSupervisor().WaferObject.GetState() == EnumWaferState.PROBING) &&
                      this.GetParam_Wafer().GetAlignState() == AlignStateEnum.IDLE)
                    {
                        retVal = true;
                    }
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public bool IsLotReady(out string msg)
        {
            bool retVal = false;
            msg = null;
            try
            {
                retVal = WaferAlignModule.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.IDLE &&
                WaferAlignModule.StageSupervisor().WaferObject.GetState() == EnumWaferState.UNPROCESSED &&
                WaferAlignModule.StageSupervisor().WaferObject.GetStatus() == EnumSubsStatus.EXIST &&
                WaferAlignModule.StageSupervisor().WaferObject.GetSubsInfo().WaferType == EnumWaferType.STANDARD;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
    }
}
