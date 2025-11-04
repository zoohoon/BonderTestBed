using LogModule;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Temperature;
using System;

namespace InternalCommands
{
    public class ChangeTemperatureToSetTemp : ProbeCommand, IChangeTemperatureToSetTemp
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class ChangeTemperatureToSetTempWhenWaferTransfer : ProbeCommand, IChangeTemperatureToSetTempWhenWaferTransfer
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class ChangeTempToSetTempFullReach : ProbeCommand, IChangeTempToSetTempFullReach
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class SetTempForFrontDoorOpen : ProbeCommand, ISetTempForFrontDoorOpen
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class ReturnToDefaltSetTemp : ProbeCommand, IReturnToDefaltSetTemp
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class EndTempEmergencyError : ProbeCommand , IEndTempEmergencyError
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class ChangeTemperatureToSetTempAfterConnectTempController : ProbeCommand, IChangeTemperatureToSetTempAfterConnectTempController
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }

    public class TemperatureSettingTriggerOccurrence : ProbeCommand, ITemperatureSettingTriggerOccurrence
    {
        public override bool Execute()
        {
            ITempController TempController = this.TempController();
            bool setCommandResult = false;
            try
            {

                setCommandResult = SetCommandTo(TempController);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return setCommandResult;
        }
    }
}
