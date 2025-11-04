using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProberInterfaces.Foup;
using System.Runtime.CompilerServices;
using LogModule;

namespace FoupProcedureManagerProject
{
    public class FoupProcedureStateMaps : LinkedList<FoupProcedureStateMap>, IFoupProcedureStateMaps
    {

        IEnumerator<IFoupProcedureStateMap> IEnumerable<IFoupProcedureStateMap>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class FoupProcedureStateMap : INotifyPropertyChanged, IFoupProcedureStateMap
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private FoupProcedure _Procedure;
        public FoupProcedure Procedure
        {
            get { return _Procedure; }
            set
            {
                _Procedure = value;
                RaisePropertyChanged();
            }
        }
        private FoupProcedureStateBase _ProcedureState = new FoupProcedureIdle();
        public FoupProcedureStateBase ProcedureState
        {
            get { return _ProcedureState; }
            set
            {
                _ProcedureState = value;
                RaisePropertyChanged();

                ProcedureStateEnum = _ProcedureState.GetState();
            }
        }

        private FoupProcedureStateEnum _ProcedureStateEnum;
        public FoupProcedureStateEnum ProcedureStateEnum
        {
            get { return _ProcedureStateEnum; }
            set
            {
                _ProcedureStateEnum = value;
                RaisePropertyChanged();
            }
        }


        public EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                //retVal = PreSafetiesRun();

                retVal = BehaviorRun();


                //retVal = PostSafetiesRun();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum RecoveryRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                PreSafetiesRun();

                retVal = BehaviorRun();

                if(retVal == EventCodeEnum.NONE)
                {
                    PostSafetiesRun();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        private EventCodeEnum PreSafetiesRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = Procedure.PreSafetiesRun();

                if (retVal != EventCodeEnum.NONE)
                {
                    //this.ProcedureState = new FoupProcedurePreSafetyError();
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

        private EventCodeEnum BehaviorRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                LoggerManager.RecoveryLog($"Foup SEQ. {Procedure.Caption} STRART.");

                retVal = Procedure.BehaviorRun();

                if (retVal != EventCodeEnum.NONE)
                {
                    this.ProcedureState = new FoupProcedureBehaviorError();
                    LoggerManager.RecoveryLog($"Foup SEQ. {Procedure.Caption} ERROR.   Result = {retVal}", true);
                }
                else
                {
                    LoggerManager.RecoveryLog($"Foup SEQ. {Procedure.Caption} DONE.   Result = {retVal}");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private EventCodeEnum PostSafetiesRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = Procedure.PostSafetiesRun();

                if (retVal != EventCodeEnum.NONE)
                {
                    //this.ProcedureState = new FoupProcedurePostSafetyError();
                }
                else
                {
                    this.ProcedureState = new FoupProcedureDone();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public EventCodeEnum PreviousRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Procedure.ReverseProcedure.PreSafetiesRun();

                retVal = Procedure.ReverseProcedure.BehaviorRun();

                if (retVal == EventCodeEnum.NONE)
                {
                    Procedure.ReverseProcedure.PostSafetiesRun();

                    this.ProcedureState = new FoupProcedureIdle();
                }
                else
                {
                    this.ProcedureState = new FoupProcedureBehaviorError();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
