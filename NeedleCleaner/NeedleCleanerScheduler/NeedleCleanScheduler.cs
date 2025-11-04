using LogModule;
using NeedleCleanerScheduleParameter;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AlignEX;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.NeedleClean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NeedleCleanerScheduler
{
    public class NeedleCleanScheduler : ISchedulingModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private IStateModule _NeedleCleanModule;
        public IStateModule NeedleCleanModule
        {
            get { return _NeedleCleanModule; }
            set
            {
                if (value != _NeedleCleanModule)
                {
                    _NeedleCleanModule = value;
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
        public SubModuleStateBase SubModuleState { get; set; }

        public NeedleCleanScheduler()
        {

        }
        public NeedleCleanScheduler(IStateModule Module)
        {
            _NeedleCleanModule = Module;
        }
        private NeedleCleanScheduleDevParameter _Param;
        public NeedleCleanScheduleDevParameter Param
        {
            get { return _Param; }
            set
            {
                if (value != _Param)
                {
                    _Param = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum Execute()
        {
            return SubModuleState.Execute();
        }

        public SubModuleStateEnum GetState()
        {
            return SubModuleState.GetState();
        }

        public EventCodeEnum ClearData()
        {
            return SubModuleState.ClearData();
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            //init 하는 코드 하고 나서 Idle 상태로 초기화
            try
            {
                if (Initialized == false)
                {
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

        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new NeedleCleanScheduleDevParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(NeedleCleanScheduleDevParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    Param = tmpParam as NeedleCleanScheduleDevParameter;
                }

                DevParam = Param;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = this.SaveParameter(Param);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public bool IsExecute()
        {
            bool retVal = false;
            try
            {

                if (_NeedleCleanModule.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING || _NeedleCleanModule.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.ABORT)
                {
                    if (_NeedleCleanModule.LotOPModule().ModuleStopFlag)
                    {
                        retVal = false;
                        return retVal;
                    }
                    //LOT RUNNING일때 보는 조건
                    if (_NeedleCleanModule.SequenceEngineManager().GetRunState())
                    {
                        retVal = true;
                    }
                }
                else
                {
                    //LOT가 RUNNING이 아니고 커맨드가 날라올때                 
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
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
