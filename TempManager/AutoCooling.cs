//using LogModule;
//using ProberErrorCode;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Temperature;

//namespace TempControl
//{
//    public class AutoCooling
//    {
//        private AutoCoolingProcStateBase AutoCoolingProcState;
//        private TempController TempController;
//        public AutoCooling(TempController TempController)
//        {
//            try
//            {
//            this.TempController = TempController;
//            AutoCoolingProcState = new AutoCoolingIdleState(this, TempController);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                 throw;
//            }
//        }

//        public EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                retVal = AutoCoolingProcState.Execute();
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Error($"[{this.GetType().Name} - Execute()] Error occurred while Call Execute()");
//            }
//            return retVal;
//        }

//        public EventCodeEnum AutoCoolingStateTransition(AutoCoolingProcStateBase state)
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//            this.AutoCoolingProcState = state;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                 throw;
//            }
//            return retVal;
//        }
//    }
//}
