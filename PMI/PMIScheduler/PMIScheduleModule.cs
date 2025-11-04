using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.AlignEX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PMIScheduler
{
    public class PMIScheduleModule : ISchedulingModule, INotifyPropertyChanged
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

        public PMIScheduleModule()
        {

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

        public void DeInitModule()
        {
        }

        public bool IsExecute()
        {
            try
            {
                bool retVal = false;

                if (this.PMIModule().ForcedDone == EnumModuleForcedState.ForcedDone)
                {
                    return false;
                }

                if (this.LotOPModule().InnerState.GetModuleState() == ModuleStateEnum.RUNNING)
                {
                    if (this.LotOPModule().ModuleStopFlag)
                    {
                        retVal = false;
                        return retVal;
                    }

                    //this.PMIModule().StageSupervisor().MarkObject.SetAlignState(AlignStateEnum.DONE);

                    //LOT RUNNING일때 보는 조건
                    if ((this.SequenceEngineManager().GetRunState()) &&
                         (this.StageSupervisor().WaferObject.GetAlignState() == AlignStateEnum.DONE) &&
                         (this.StageSupervisor().MarkObject.GetAlignState() == AlignStateEnum.DONE)
                       )
                    {
                        retVal = true;
                    }
                }
                else
                {
                    //LOT가 RUNNING이 아니고 커맨드가 날라올때 

                }

                return retVal;
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
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
