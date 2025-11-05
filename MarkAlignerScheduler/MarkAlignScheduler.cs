using MarkAlignerScheduleParameter;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using LogModule;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace MarkAlignerScheduler
{
    public class MarkAlignScheduler : ISchedulingModule, INotifyPropertyChanged
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


        private IStateModule _MarkAlignModule;
        public IStateModule MarkAlignModule
        {
            get { return _MarkAlignModule; }
            set
            {
                if (value != _MarkAlignModule)
                {
                    _MarkAlignModule = value;
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

        public MarkAlignScheduler(IStateModule Module)
        {
            _MarkAlignModule = Module;
        }

        public MarkAlignScheduler()
        {
        }

        private MarkAlignScheduleParameter _Param;
        public MarkAlignScheduleParameter Param
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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    _MarkAlignModule = this.MarkAligner();
                    retval = LoadDevParameter();

                    //init 하는 코드 하고 나서 Idle 상태로 초기화

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
            tmpParam = new MarkAlignScheduleParameter();
            tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
            RetVal = this.LoadParameter(ref tmpParam, typeof(MarkAlignScheduleParameter));

            if (RetVal == EventCodeEnum.NONE)
            {
                Param = tmpParam as MarkAlignScheduleParameter;
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
            if (this.LotOPModule().ModuleState.GetState() == ModuleStateEnum.RUNNING)
            {
                if (this.LotOPModule().ModuleStopFlag)
                {
                    retVal = false;
                    return retVal;
                }
                if (MarkAlignModule.StageSupervisor().MarkObject.GetAlignState() == AlignStateEnum.IDLE &&
                    MarkAlignModule.SequenceEngineManager().GetRunState() &&
                    MarkAlignModule.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.DONE &&
                    MarkAlignModule.StageSupervisor().ProbeCardInfo.GetAlignState() == AlignStateEnum.DONE)
                {
                    retVal = true;
                }
            }
            else
            {

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

    }
}
