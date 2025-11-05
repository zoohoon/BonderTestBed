using BonderModuleMove;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonderSupervisor
{
    // 251013 sebas
    public class BonderSupervisor : IBonderSupervisor, INotifyPropertyChanged, IModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        #region => Property
        public bool Initialized { get; set; } = false;

        private IBonderMove _BonderModuleState;

        public IBonderMove BonderModuleState
        {
            get { return _BonderModuleState; }
            set { _BonderModuleState = value; }
        }
        #endregion

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                if(Initialized == false)
                {
                    // 본더모듈 이니셜 위치?
                    BonderMove module = new BonderMove();

                    module.InitModule();

                    BonderModuleState = module;

                    //Initialized = true;
                }
                else
                {

                }
            }
            catch
            {

            }
            return retval;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            // 파라미터 셋팅
            LoggerManager.Debug("[Bonder] BonderSupervisor. LoadSysParameter()");

            return retVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            //StageSupervisor 참고
            //RetVal = this.SaveParameter(MoveParam);
            //RetVal = SaveCylinderManagerParam();
            //SaveStageMoveLockParam();

            LoggerManager.Debug("[Bonder] BonderSupervisor. SaveSysParameter()");

            return RetVal;
        }
        public EventCodeEnum LoadDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //StageSupervisor 참고
                //RetVal = LoadOCRDevParam();
                //RetVal = LoadWaferObject();
                //RetVal = LoadProberCard();

                LoggerManager.Debug("[Bonder] BonderSupervisor. LoadDevParameter()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum SaveDevParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                //StageSupervisor 참고
                //RetVal = SaveWaferObject();
                //RetVal = SaveProberCard();

                LoggerManager.Debug("[Bonder] BonderSupervisor. SaveDevParameter()");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }
        public EventCodeEnum InitDevParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                //StageSupervisor 참고
                //ProbeCardInfo.ProbeCardDevObjectRef.TouchdownCount.Value = 0;

                LoggerManager.Debug("[Bonder] BonderSupervisor. InitDevParameter()");

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public void DeInitModule()
        {
            try
            {

            }
            catch
            {

            }
        }
    }
}
