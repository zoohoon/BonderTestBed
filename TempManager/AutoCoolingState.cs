//using LogModule;
//using ProberErrorCode;
//using ProberInterfaces;
//using ProberInterfaces.Command;
//using ProberInterfaces.Command.Internal;
//using ProberInterfaces.Enum;
//using ProberInterfaces.Temperature;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Temperature;

//namespace TempControl
//{
//    public abstract class AutoCoolingProcStateBase : AutoCoolingProcState
//    {
//        protected AutoCooling AutoCooling { get; }
//        protected TempController TempController { get; }

//        public AutoCoolingProcStateBase(AutoCooling module, TempController tempModdule)
//        {
//            try
//            {
//                this.AutoCooling = module;
//                this.TempController = tempModdule;
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//        }

//        public override bool CanExecute(IProbeCommandToken token)
//        {
//            return false;
//        }
//    }

//    public class AutoCoolingIdleState : AutoCoolingProcStateBase
//    {
//        public AutoCoolingIdleState(AutoCooling module, TempController tempModdule) : base(module, tempModdule)
//        {
//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
//            try
//            {
//                bool IsEnableAutoCooling = TempController.TempControllerDevParameter.IsEnableAutoCoolingControl.Value;

//                if (IsEnableAutoCooling == true)
//                {
//                    double curTemp = TempController.CurTemp * 10;
//                    double activateTemp = TempController.TempControllerDevParameter.AutoCoolingDeactivatingTemp.Value;

//                    if (activateTemp < curTemp)
//                    {
//                        double setTemp = TempController.SetTemp * 10;
//                        double activatingOffset = TempController.TempControllerDevParameter.AutoCoolingActivatingOffset.Value;
//                        double diffTemp = curTemp - setTemp;

//                        if (activatingOffset <= diffTemp)
//                        {
//                            retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingOnState(AutoCooling, TempController));
//                        }
//                        else
//                        {
//                            retVal = EventCodeEnum.NONE;
//                        }
//                    }
//                }
//                else
//                {
//                    retVal = EventCodeEnum.NONE;
//                }

//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//            return retVal;
//        }

//        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.IDLE;
//    }

//    public class AutoCoolingOnState : AutoCoolingProcStateBase
//    {
//        DateTime StartTime;
//        bool IsOnAutoCooling = false;
//        IOPortDescripter<bool> DOCHUCKCOOLAIRON = null;

//        public AutoCoolingOnState(AutoCooling module, TempController tempModdule) : base(module, tempModdule)
//        {
//            try
//            {
//                Type type = TempController.IOManager().IO.Outputs.GetType();
//                PropertyInfo propertyInfo = type.GetProperty("DOCHUCKCOOLAIRON");
//                DOCHUCKCOOLAIRON = (IOPortDescripter<bool>)propertyInfo.GetValue(TempController.IOManager().IO.Outputs);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.NONE;
//            try
//            {

//                double curTemp = TempController.CurTemp * 10;
//                double activateTemp = TempController.TempControllerDevParameter.AutoCoolingDeactivatingTemp.Value;

//                double setTemp = TempController.SetTemp * 10;
//                double deactivatingOffset = TempController.TempControllerDevParameter.AutoCoolingDectivatingOffset.Value;
//                double diffTemp = curTemp - setTemp;

//                if ((diffTemp <= deactivatingOffset) || (curTemp <= activateTemp))
//                {
//                    retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingDonePerformState(AutoCooling, TempController));
//                }
//                else
//                {
//                    if (!IsOnAutoCooling)
//                    {
//                        //ON
//                        try
//                        {
//                            TempController.IOManager().IOServ.WriteBit(DOCHUCKCOOLAIRON, true);
//                        }
//                        catch (Exception err)
//                        {
//                            throw new Exception("Error occurred while trying to make IO(DOCHUCKCOOLAIRON) true");
//                        }
//                        finally
//                        {
//                            StartTime = DateTime.Now;
//                            IsOnAutoCooling = true;
//                        }
//                    }

//                    DateTime curTime = DateTime.Now;
//                    TimeSpan timeSpan = curTime - StartTime;
//                    double activatingDuration = TempController.TempControllerDevParameter.AutoCoolingActivatingDurationSec.Value;

//                    if (activatingDuration <= timeSpan.TotalSeconds)
//                    {
//                        Trace.WriteLine($"[AutoCoolingOnState] Activating Time : {timeSpan.TotalSeconds} sec");
//                        retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingOffState(AutoCooling, TempController));
//                    }

//                    retVal = EventCodeEnum.NONE;
//                }

//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//            return retVal;
//        }

//        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
//    }

//    public class AutoCoolingOffState : AutoCoolingProcStateBase
//    {
//        DateTime StartTime;
//        bool IsOffAutoCooling = false;
//        IOPortDescripter<bool> DOCHUCKCOOLAIRON = null;

//        public AutoCoolingOffState(AutoCooling module, TempController tempModdule) : base(module, tempModdule)
//        {
//            try
//            {
//                Type type = TempController.IOManager().IO.Outputs.GetType();
//                PropertyInfo propertyInfo = type.GetProperty("DOCHUCKCOOLAIRON");
//                DOCHUCKCOOLAIRON = (IOPortDescripter<bool>)propertyInfo.GetValue(TempController.IOManager().IO.Outputs);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.NONE;
//            try
//            {

//                double curTemp = TempController.CurTemp * 10;
//                double activateTemp = TempController.TempControllerDevParameter.AutoCoolingDeactivatingTemp.Value;

//                double setTemp = TempController.SetTemp * 10;
//                double deactivatingOffset = TempController.TempControllerDevParameter.AutoCoolingDectivatingOffset.Value;
//                double diffTemp = curTemp - setTemp;

//                if ((diffTemp <= deactivatingOffset) || (curTemp <= activateTemp))
//                {
//                    retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingDonePerformState(AutoCooling, TempController));
//                }
//                else
//                {
//                    DateTime curTime;
//                    TimeSpan timeSpan;

//                    if (!IsOffAutoCooling)
//                    {
//                        //OFF
//                        try
//                        {
//                            TempController.IOManager().IOServ.WriteBit(DOCHUCKCOOLAIRON, false);
//                        }
//                        catch (Exception err)
//                        {
//                            throw new Exception("Error occurred while trying to make IO(DOCHUCKCOOLAIRON) false");
//                        }
//                        finally
//                        {
//                            StartTime = DateTime.Now;
//                            IsOffAutoCooling = true;
//                        }
//                    }

//                    curTime = DateTime.Now;
//                    timeSpan = curTime - StartTime;
//                    double activatingDuration = TempController.TempControllerDevParameter.AutoCoolingDeactivatingDurationSec.Value;

//                    if (activatingDuration <= timeSpan.TotalSeconds)
//                    {
//                        Trace.WriteLine($"[AutoCoolingOnState] Deactivating Time : {timeSpan.TotalSeconds} sec");
//                        retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingOnState(AutoCooling, TempController));
//                    }

//                    retVal = EventCodeEnum.NONE;
//                }

//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//            return retVal;
//        }

//        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.RUNNING;
//    }

//    public class AutoCoolingDonePerformState : AutoCoolingProcStateBase
//    {
//        IOPortDescripter<bool> DOCHUCKCOOLAIRON = null;

//        public AutoCoolingDonePerformState(AutoCooling module, TempController tempModdule) : base(module, tempModdule)
//        {
//            try
//            {
//                Type type = TempController.IOManager().IO.Outputs.GetType();
//                PropertyInfo propertyInfo = type.GetProperty("DOCHUCKCOOLAIRON");
//                DOCHUCKCOOLAIRON = (IOPortDescripter<bool>)propertyInfo.GetValue(TempController.IOManager().IO.Outputs);
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.NONE;

//            //OFF
//            try
//            {
//                TempController.IOManager().IOServ.WriteBit(DOCHUCKCOOLAIRON, false);
//            }
//            catch (Exception err)
//            {
//                throw new Exception("Error occurred while trying to make IO(DOCHUCKCOOLAIRON) false");
//            }

//            retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingDoneState(AutoCooling, TempController));
//            return retVal;
//        }

//        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
//    }

//    public class AutoCoolingDoneState : AutoCoolingProcStateBase
//    {
//        public AutoCoolingDoneState(AutoCooling module, TempController tempModdule) : base(module, tempModdule)
//        {
//        }

//        public override EventCodeEnum Execute()
//        {
//            EventCodeEnum retVal = EventCodeEnum.NONE;
//            try
//            {
//                retVal = AutoCooling.AutoCoolingStateTransition(new AutoCoolingIdleState(AutoCooling, TempController));
//            }
//            catch (Exception err)
//            {
//                LoggerManager.Exception(err);
//                throw;
//            }
//            return retVal;
//        }

//        public override ModuleStateEnum GetModuleState() => ModuleStateEnum.DONE;
//    }
//}
