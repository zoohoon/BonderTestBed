using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace NeedleBrushProcesser
{
    public class NormalNeedleBrushProcessModule : IProcessingModule, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> Common PNP Declaration

        public NormalNeedleBrushProcessModule()
        {

        }
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

        public SubModuleMovingStateBase MovingState { get; set; }

        #endregion

        public bool Initialized { get; set; } = false;

        #region ==> Common PNP Function
        /// <summary>
        /// 실제 프로세싱 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoExecute()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MovingState.Moving();

                // TODO: 로직 확인
                if (IsExecute())
                {
                }

                MovingState.Stop();

                if (retVal == EventCodeEnum.NONE)
                {
                    // Set module state as done state
                    SubModuleState = new SubModuleDoneState(this);
                }
                else
                {
                    // Set module state as Error state
                    SubModuleState = new SubModuleErrorState(this);
                }

                this.LotOPModule().MapScreenToLotScreen();
                // Show PMI Result Screen
                //
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [DoExecute()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// 현재 Parameter Check 및 Init하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoClearData()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [DoClearData()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// Recovery때 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [DoRecovery()] : {err}");
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        /// <summary>
        /// Recovery 종료할 때 하는 코드
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum DoExitRecovery()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [DoExitRecovery()] : {err}");
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        /// <summary>
        /// SubModule이 Processing 가능한지 판단하는 조건 
        /// </summary>
        /// <returns></returns>
        public bool IsExecute()
        {
            bool retVal = false;

            try
            {
                // TODO: 로직 확인
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [IsExecute()] : {err}");
                LoggerManager.Exception(err);

                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// 현재 단계의 Parameter Setting 이 다 되었는지 확인하는 함수.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ParamValidation()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ClearData()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.ClearData();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum Execute()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.Execute();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum Recovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.Recovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum ExitRecovery()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.ExitRecovery();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SubModuleStateEnum GetState()
        {
            SubModuleStateEnum retval = SubModuleStateEnum.UNDEFINED;

            try
            {
                retval = SubModuleState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MovingStateEnum GetMovingState()
        {
            MovingStateEnum retval = MovingStateEnum.STOP;

            try
            {
                retval = MovingState.GetState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ClearState()
        {
            try
            {
                SubModuleState = new SubModuleIdleState(this);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        /// <summary>
        /// PNP 모듈 init 해주는 함수 한번만 호출된다.
        /// </summary>
        /// <returns></returns>
        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    SubModuleState = new SubModuleIdleState(this);
                    MovingState = new SubModuleStopState(this);

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
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [InitModule()] : {err}");
                LoggerManager.Exception(err);
            }

            return retval;
        }

        /// <summary>
        /// PNP 모듈 deinit 해주는 함수
        /// </summary>
        /// <returns></returns>
        public void DeInitModule()
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[NormalNeedleBrushProcessModule] [DeInitModule()] : {err}");
                LoggerManager.Exception(err);
            }
        }

        public bool IsParameterChanged(bool issave = false)
        {
            return false;
        }
        #endregion
    }

  
}
