using NotifyEventModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using LogModule;
using System.Threading;
using ProberInterfaces.Event;
using FoupModules;
using System.Collections.Generic;
using System.Linq;

namespace FoupProcedureManagerProject
{
    [Serializable]
    public class FoupCover_Close : FoupBehavior
    {
        public FoupCover_Close()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.Cover.Close();

                StateTransition(retVal);

                if (FoupModule.Cover.EnumState != FoupCoverStateEnum.CLOSE)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.Cover.Inputs;
                Outputs = FoupModule.Cover.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "COVER CLOSE")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "COVER OPEN")
                    {
                        requiredInput.Value = false;
                    }
                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int openInput = -1;
            int openOutput = -1;
            int closeInput = -1;
            int closeOutput = -1;
            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "COVER CLOSE")
                    {
                        openInput = input.MonitorForIO(true);
                        if (openInput == 1)
                        {
                            openOutput = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutput = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");
                    }

                    if (input.Alias.Value == "COVER OPEN")
                    {
                        closeInput = input.MonitorForIO(false);
                        if (closeInput == 1)
                        {
                            closeOutput = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutput = CompareOutput(input.Alias.Value, true);
                        }

                        if (closeOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");
                    }
                }

                if(openOutput != 1 || closeOutput != 1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED;
                }
                else
                {
                    if (openInput != 1 && closeInput != 1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; //idle
                    }
                    else if(openInput == 1 && closeInput == 1)
                    {
                        retVal = EventCodeEnum.NONE; //done
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR;
                    }
                }

                if(IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    [Serializable]
    public class FoupCover_Open : FoupBehavior
    {
        public FoupCover_Open()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.Cover.Open();

                StateTransition(retVal);

                if (FoupModule.Cover.EnumState != FoupCoverStateEnum.OPEN)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
              
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.Cover.Inputs;
                Outputs = FoupModule.Cover.Outputs;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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

    [Serializable]
    public class FoupDockingPort40_Out : FoupBehavior
    {
        public FoupDockingPort40_Out()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.DockingPort40.Out();

                StateTransition(retVal);

                if (FoupModule.DockingPort40.GetState() != DockingPort40StateEnum.OUT)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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

    [Serializable]
    public class FoupDockingPort40_In : FoupBehavior
    {
        public FoupDockingPort40_In()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.DockingPort40.In();

                StateTransition(retVal);

                if (FoupModule.DockingPort40.GetState() != DockingPort40StateEnum.IN)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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

    [Serializable]
    public class FoupCassetteOpener_Lock : FoupBehavior
    {
        public FoupCassetteOpener_Lock()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.CassetteOpener.Lock();

                StateTransition(retVal);

                if (FoupModule.CassetteOpener.GetState() != FoupCassetteOpenerStateEnum.LOCK)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);
                System.Threading.Thread.Sleep(2000);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.CassetteOpener.Inputs;
                Outputs = FoupModule.CassetteOpener.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "OPENER LOCK")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "OPENER UNLOCK")
                    {
                        requiredInput.Value = false;
                    }
                    else if (requiredInput.Alias.Value == "COVER VACUUM")
                    {
                        requiredInput.Value = false;
                    }
                    else if (requiredInput.Alias.Value == "MAPPING OUT")
                    {
                        requiredInput.Value = true;
                    }


                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int openInput = -1;
            int openOutput = -1;
            List<int> closeInput = new List<int>();
            List<int> closeOutput = new List<int>();
            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "OPENER LOCK")
                    {
                        openInput = input.MonitorForIO(true);
                        if (openInput == 1)
                        {
                            openOutput = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutput = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"IO : {input.Alias.Value}, {retVal}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");
                    }

                    if (input.Alias.Value == "OPENER UNLOCK" || input.Alias.Value == "COVER VACUUM")
                    {
                        int closeOutputResult = -1;
                        int closeInputResult = input.MonitorForIO(false);
                        if (closeInputResult == 1)
                        {
                            closeOutputResult = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutputResult = CompareOutput(input.Alias.Value, true);
                        }

                        if (closeOutputResult != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"IO : {input.Alias.Value}, {retVal}", true);
                        }

                        closeInput.Add(closeInputResult);
                        closeOutput.Add(closeOutputResult); 
                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");

                    }
                }

                bool allCloseInputsAre1 = closeInput.All(input => input == 1);
                bool allCloseOutputsAre1 = closeOutput.All(output => output == 1);


                if(openOutput != 1 || !allCloseOutputsAre1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED; // 입출력이 일치하지 않음
                }
                else
                {
                    if (openInput == 1 && allCloseInputsAre1)
                    {
                        retVal = EventCodeEnum.NONE; // 완료 상태 (done)
                    }
                    else if(openInput != 1 && !allCloseInputsAre1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; // 대기 상태 (idle)
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR; // 입출력 포트 에러
                    }

                }

                if (IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class FoupCassetteOpener_Unlock : FoupBehavior
    {
        public FoupCassetteOpener_Unlock()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                System.Threading.Thread.Sleep(2000);

                retVal = FoupModule.CassetteOpener.Unlock();

                StateTransition(retVal);

                if (FoupModule.CassetteOpener.GetState() != FoupCassetteOpenerStateEnum.UNLOCK)
                {
                    retVal = EventCodeEnum.FOUP_ILLEGAL;
                }

                //if (this.State != FoupBehaviorStateEnum.Done)
                //{
                //    retVal = EventCodeEnum.FOUP_ILLEGAL;
                //}

                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.CassetteOpener.Inputs;
                Outputs = FoupModule.CassetteOpener.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "OPENER LOCK")
                    {
                        requiredInput.Value = false;
                    }
                    else if (requiredInput.Alias.Value == "OPENER UNLOCK")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "COVER VACUUM")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "MAPPING OUT")
                    {
                        requiredInput.Value = true;
                    }


                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            List<int> openInput = new List<int>();
            List<int> openOutput = new List<int>();
            int closeInput = -1;
            int closeOutput = -1;

            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "OPENER UNLOCK" || input.Alias.Value == "COVER VACUUM")
                    {
                        int openOutputResult = -1;
                        int openInputResult = input.MonitorForIO(true);
                        if (openInputResult == 1)
                        {
                            openOutputResult = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutputResult = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutputResult != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"IO : {input.Alias.Value}, {retVal}", true);
                        }

                        openOutput.Add(openOutputResult);
                        openInput.Add(openInputResult); 
                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");

                    }

                    if (input.Alias.Value == "OPENER LOCK")
                    {
                        closeInput = input.MonitorForIO(false);
                        if (closeInput == 1)
                        {
                            closeOutput = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutput = CompareOutput(input.Alias.Value, true);
                        }

                        if (closeOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"IO : {input.Alias.Value}, {retVal}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");

                    }
                }

                bool allOpenInputsAre1 = openInput.All(input => input == 1);
                bool allOpenOutputsAre1 = openOutput.All(output => output == 1);


                if(!allOpenOutputsAre1 || closeOutput != 1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED; // 입출력이 일치하지 않음
                }
                else
                {
                    if (closeInput == 1 && allOpenInputsAre1)
                    {
                        retVal = EventCodeEnum.NONE; // 완료 상태 (done)
                    }
                    else if (closeInput != 1 && !allOpenInputsAre1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; // 대기 상태 (idle)
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR; // 입출력 포트 에러
                    }
                }

                if (IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class FoupDockingPort_In : FoupBehavior
    {
        public FoupDockingPort_In()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.DockingPort.In();

                StateTransition(retVal);

                if (FoupModule.DockingPort.GetState() != DockingPortStateEnum.IN)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.DockingPort.Inputs;
                Outputs = FoupModule.DockingPort.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "CST IN")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "CST OUT")
                    {
                        requiredInput.Value = false;
                    }
                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            int openInput = -1;
            int openOutput = -1;
            int closeInput = -1;
            int closeOutput = -1;
            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "CST IN")
                    {
                        openInput = input.MonitorForIO(true);
                        if (openInput == 1)
                        {
                            openOutput = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutput = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"IO : {input.Alias.Value}, {retVal}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");
                    }

                    if (input.Alias.Value == "CST OUT")
                    {
                        closeInput = input.MonitorForIO(false);
                        if (closeInput == 1)
                        {
                            closeOutput = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutput = CompareOutput(input.Alias.Value, true);
                        }

                        if (closeOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");
                    }
                }

                if (openOutput != 1 || closeOutput != 1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED;
                }
                else
                {
                    if (openInput != 1 && closeInput != 1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; //idle
                    }
                    else if (openInput == 1 && closeInput == 1)
                    {
                        retVal = EventCodeEnum.NONE; //done
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR;
                    }
                }

                if (IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class FoupDockingPort_Out : FoupBehavior
    {
        public FoupDockingPort_Out()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.DockingPort.Out();

                StateTransition(retVal);

                if (FoupModule.DockingPort.GetState() != DockingPortStateEnum.OUT)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.DockingPort.Inputs;
                Outputs = FoupModule.DockingPort.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "CST IN")
                    {
                        requiredInput.Value = false;
                    }
                    else if (requiredInput.Alias.Value == "CST OUT")
                    {
                        requiredInput.Value = true;
                    }
                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            int openInput = -1;
            int openOutput = -1;
            int closeInput = -1;
            int closeOutput = -1;
            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "CST OUT")
                    {
                        openInput = input.MonitorForIO(true);
                        if (openInput == 1)
                        {
                            openOutput = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutput = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");

                    }

                    if (input.Alias.Value == "CST IN")
                    {
                        closeInput = input.MonitorForIO(false);
                        if (closeInput == 1)
                        {
                            closeOutput = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutput = CompareOutput(input.Alias.Value, true);
                        }

                        if (closeOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }
                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");

                    }
                }

                if (openOutput != 1 || closeOutput != 1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED;
                }
                else
                {
                    if (openInput != 1 && closeInput != 1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; //idle
                    }
                    else if (openInput == 1 && closeInput == 1)
                    {
                        retVal = EventCodeEnum.NONE; //done
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR;
                    }
                }

                if (IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }

    [Serializable]
    public class FoupDockingPlate_Lock : FoupBehavior
    {
        public FoupDockingPlate_Lock()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                EnumCSTCtrl lockstate = EnumCSTCtrl.NONE;
                if (SystemManager.SysteMode != SystemModeEnum.Single && !(FoupModule is EmulFoupModule))
                {
                    lockstate= FoupModule.GPLoader.ReadCassetteCtrlState(FoupModule.FoupIndex);
                }

                if (lockstate == EnumCSTCtrl.CSTLOCKED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = FoupModule.DockingPlate.Lock();
                    StateTransition(retVal);
                    if (FoupModule.DockingPlate.GetState() != DockingPlateStateEnum.LOCK)
                    {
                        ///ClampLockFailEvent
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupModule.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        FoupModule.EventManager().RaisingEvent(typeof(ClampLockFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        FoupModule.RisingRFIDEvent(false, " ");

                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                    else
                    {
                        ///ClampLockEvent
                        //this.E84Module().SetClampSignal(0, true);//  이건 머지?
                        if (retVal == EventCodeEnum.NONE)
                        {
                            PIVInfo pivinfo = new PIVInfo(foupnumber: FoupModule.FoupIndex + 1);
                            SemaphoreSlim semaphore = new SemaphoreSlim(0);
                            FoupModule.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                            semaphore.Wait();

                            FoupModule.OccurFoupClampStateChangedEvent(FoupModule.FoupIndex + 1, true);

                            //FoupModule.ReadRFID();
                            FoupModule.Read_CassetteID();


                        }
                        else
                        {
                            LoggerManager.Debug($"FoupDockingPlate_Lock.Run(): DockingPlate.Lock Behavior is Invalid.");
                        }
                    }
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.DockingPlate.Inputs;
                Outputs = FoupModule.DockingPlate.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "PRESENCE1")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "PRESENCE2")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "CST LOCK")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "CST UNLOCK")
                    {
                        requiredInput.Value = false;
                    }
                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            int presence = -1;
            int openInput = -1;
            int openOutput = -1;
            int closeInput = -1;
            int closeOutput = -1;
            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "PRESENCE1" || input.Alias.Value == "PRESENCE2")
                    {
                        presence = input.MonitorForIO(true);
                        if (presence != 1)
                        {
                            retVal = EventCodeEnum.IO_PORT_ERROR;
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            break;
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value}, must be satisfied with the value of TRUE");
                    }

                    if(input.Alias.Value == "CST LOCK")
                    {
                        openInput = input.MonitorForIO(true);
                        if (openInput == 1)
                        {
                            openOutput = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutput = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }
                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");
                    }

                    if (input.Alias.Value == "CST UNLOCK")
                    {
                        closeInput = input.MonitorForIO(false);
                        if (closeInput == 1)
                        {
                            closeOutput = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutput = CompareOutput(input.Alias.Value, true);

                        }

                        if (closeOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");
                    }
                }

                if (openOutput != 1 || closeOutput != 1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED;
                }
                else
                {
                    if (openInput != 1 && closeInput != 1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; //idle
                    }
                    else if (openInput == 1 && closeInput == 1)
                    {
                        retVal = EventCodeEnum.NONE; //done
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR;
                    }
                }

                if (IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }


    [Serializable]
    public class FoupDockingPlate_Unlock : FoupBehavior
    {
        public FoupDockingPlate_Unlock()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                EnumCSTCtrl lockstate = EnumCSTCtrl.NONE;
                if (SystemManager.SysteMode != SystemModeEnum.Single && !(FoupModule is EmulFoupModule))
                {
                    lockstate = FoupModule.GPLoader.ReadCassetteCtrlState(FoupModule.FoupIndex);
                }

                if (lockstate == EnumCSTCtrl.CSTUNLOCKED)
                {
                    retVal = EventCodeEnum.NONE;
                }
                else
                {
                    retVal = FoupModule.DockingPlate.Unlock();

                    StateTransition(retVal);

                    if (FoupModule.DockingPlate.GetState() != DockingPlateStateEnum.UNLOCK)
                    {                    ///ClampUnlockFailEvent
                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupModule.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        FoupModule.EventManager().RaisingEvent(typeof(ClampUnlockFailEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                    else
                    {
                        //this.E84Module().SetClampSignal(0, false);
                        //FoupModule.ChangeFoupServiceStatus(GEMFoupStateEnum.READY_TO_UNLOAD);
                        //this.GEMModule().GetPIVContainer().SetFoupState(FoupModule.FoupIndex + 1, GEMFoupStateEnum.READY_TO_UNLOAD);

                        PIVInfo pivinfo = new PIVInfo(foupnumber: FoupModule.FoupIndex + 1);
                        SemaphoreSlim semaphore = new SemaphoreSlim(0);
                        FoupModule.EventManager().RaisingEvent(typeof(ClampUnlockEvent).FullName, new ProbeEventArgs(this, semaphore, pivinfo));
                        semaphore.Wait();

                        FoupModule.OccurFoupClampStateChangedEvent(FoupModule.FoupIndex + 1, false);

                    }
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                Inputs = FoupModule.DockingPlate.Inputs;
                Outputs= FoupModule.DockingPlate.Outputs;
                SetRequiredIO();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                RequiredInputs = new System.Collections.ObjectModel.ObservableCollection<IOPortDescripter<bool>>();
                foreach (var input in Inputs)
                {
                    var requiredInput = new IOPortDescripter<bool>();
                    requiredInput.Alias.Value = input.Alias.Value;
                    if (requiredInput.Alias.Value == "PRESENCE1")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "PRESENCE2")
                    {
                        requiredInput.Value = true;
                    }
                    else if (requiredInput.Alias.Value == "CST LOCK")
                    {
                        requiredInput.Value = false;
                    }
                    else if (requiredInput.Alias.Value == "CST UNLOCK")
                    {
                        requiredInput.Value = true;
                    }
                    RequiredInputs.Add(requiredInput);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {

            EventCodeEnum retVal = EventCodeEnum.NONE;
            int presence = -1;
            int openInput = -1;
            int openOutput = -1;
            int closeInput = -1;
            int closeOutput = -1;
            try
            {
                List<string> IO = new List<string>();

                foreach (var input in Inputs)
                {
                    if (input.Alias.Value == "PRESENCE1" || input.Alias.Value == "PRESENCE2")
                    {
                        presence = input.MonitorForIO(true);
                        if (presence != 1)
                        {
                            retVal = EventCodeEnum.IO_PORT_ERROR;
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            break;
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value}, must be satisfied with the value of TRUE");
                    }


                    if (input.Alias.Value == "CST UNLOCK")
                    {
                        openInput = input.MonitorForIO(true);
                        if (openInput == 1)
                        {
                            openOutput = CompareOutput(input.Alias.Value, true);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried TRUE.");
                            openOutput = CompareOutput(input.Alias.Value, false);
                        }

                        if (openOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of TRUE");
                    }

                    if (input.Alias.Value == "CST LOCK")
                    {
                        closeInput = input.MonitorForIO(false);
                        if (closeInput == 1)
                        {
                            closeOutput = CompareOutput(input.Alias.Value, false);
                        }
                        else
                        {
                            IO.Add(input.Alias.Value + "is requried FALSE.");
                            closeOutput = CompareOutput(input.Alias.Value, true);
                        }

                        if (closeOutput != 1)
                        {
                            IO.Add(input.Alias.Value + " Output not matched.");
                            LoggerManager.RecoveryLog($"{retVal} IO : {input.Alias.Value}", true);
                        }

                        LoggerManager.RecoveryLog($"{input.Alias.Value} Input/Output, must be satisfied with the value of FALSE");
                    }
                }


                if (openOutput != 1 || closeOutput != 1)
                {
                    retVal = EventCodeEnum.IO_NOT_MATCHED;
                }
                else
                {
                    if (openInput != 1 && closeInput != 1)
                    {
                        retVal = EventCodeEnum.UNDEFINED; //idle
                    }
                    else if (openInput == 1 && closeInput == 1)
                    {
                        retVal = EventCodeEnum.NONE; //done
                    }
                    else
                    {
                        retVal = EventCodeEnum.IO_PORT_ERROR;
                    }
                }

                if (IO.Count > 0)
                {
                    FoupModule.ShowIOErrorMessage(IO);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
    }
    [Serializable]
    public class FoupTilt_Up : FoupBehavior 
    {
        public FoupTilt_Up()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.Tilt.Up();

                StateTransition(retVal);

                if (FoupModule.Tilt.GetState() != TiltStateEnum.UP)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
             
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
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
    [Serializable]
    public class FoupTilt_Down : FoupBehavior
    {
        public FoupTilt_Down()
        {

        }
        public override EventCodeEnum Run()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = FoupModule.Tilt.Down();

                StateTransition(retVal);

                if (FoupModule.Tilt.GetState() != TiltStateEnum.DOWN)
                {
                    retVal = EventCodeEnum.FOUP_ERROR;
                }
                //this.EventManager().RaisingEvent(typeof(ClampLockEvent).FullName);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum SetRequiredIO()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public override EventCodeEnum InitBehavior()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
        public override EventCodeEnum CheckIOState()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
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
