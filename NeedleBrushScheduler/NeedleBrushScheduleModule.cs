using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NeedleBrushScheduler
{
    public class NeedleBrushScheduleModule : ISchedulingModule, INotifyPropertyChanged
    {
        public bool Initialized { get; set; } = false;

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public NeedleBrushScheduleModule()
        {

        }
        public void DeInitModule()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

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

        public bool IsExecute()
        {
            try
            {
                bool retVal = false;


                if (this.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if (this.LotOPModule().ModuleStopFlag)
                    {
                        retVal = false;
                        return retVal;
                    }

                    // TODO: 로직 확인
                    //LOT RUNNING일때 보는 조건
                    //if ((this.PMIModule().SequenceEngineManager().GetRunState()) &&
                    //     (this.PMIModule().StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.DONE) &&
                    //     (this.PMIModule().StageSupervisor().MarkObject.GetAlignState() == AlignStateEnum.DONE)
                    //   )
                    //{
                    //    retVal = true;
                    //}
                }
                else
                {
                    //LOT가 RUNNING이 아니고 커맨드가 날라올때 

                }

                return retVal;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }

        public EventCodeEnum ParamValidation()
        {
            return EventCodeEnum.NONE;
        }
    }
}
