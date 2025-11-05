using System;
using ProberInterfaces;
using System.Xml.Serialization;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;

namespace InternalParamObject
{
    public class InternalParameters : IInternalParameters
    {
        public CommandRecipe CommandRecipe { get; set; }

        public CommandRecipe ConsoleRecipe { get; set; }

        [Serializable]
        public class InternalCommandRecipe : CommandRecipe
        {


            public override EventCodeEnum Init()
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);


                    retval = EventCodeEnum.PARAM_ERROR;
                }

                return retval;
            }
            public InternalCommandRecipe()
            {
            }

            [XmlIgnore, JsonIgnore]
            public override string FilePath { get; } = "Internal";
            [XmlIgnore, JsonIgnore]
            public override string FileName { get; } = "Command_RECIPE_Internal.Json";
            public override EventCodeEnum SetEmulParam()
            {
                return SetDefaultParam();
            }
            public override EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum retVal;

                try
                {

                    string NO_CMD_NAME = CommandNameGen.Generate(typeof(NoCommand));

                    CommandDescriptor desc;

                    #region => AirBlowCommands

                    desc = CommandDescriptor.Create<IAirBlowChuckCleaningCommand>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IAirBlowWaferCleaningCommand>();
                    AddRecipe(desc);

                    #endregion

                    #region => CardChangeCommands

                    #endregion

                    #region => FoupCommands

                    desc = CommandDescriptor.Create<ICassetteLoadCommand>();
                    desc.Parameter = new CallbackParam()
                    {
                        Command = CommandNameGen.Generate(typeof(ICassetteLoadDone)),
                    };
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ICassetteUnLoadCommand>();
                    AddRecipe(desc);

                    #endregion

                    #region => LoaderControllerCommands

                    desc = CommandDescriptor.Create<ILoaderMapCommand>();
                    AddRecipe(desc);

                    #endregion

                    #region => WaferTransferCommands

                    desc = CommandDescriptor.Create<IChuckLoadCommand>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IChuckUnloadCommand>();
                    AddRecipe(desc);

                    #endregion

                    #region => LotCommands

                    desc = CommandDescriptor.Create<ILotOpStart>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ILotOpPause>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ILotOpResume>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ILotOpEnd>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IUnloadAllWafer>();
                    AddRecipe(desc);

                    #endregion

                    #region => GEMCommands

                    desc = CommandDescriptor.Create<IGEM_S6F11>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ICassetteLoadDone>();
                    AddRecipe(desc);

                    #endregion

                    #region => LoaderOpCommands

                    desc = CommandDescriptor.Create<ILoaderOpStart>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ILoaderOpPause>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ILoaderOpResume>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ILoaderOpEnd>();
                    AddRecipe(desc);

                    #endregion

                    #region => PinAlignerCommands

                    desc = CommandDescriptor.Create<IDOPinAlignAfterSoaking>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IDOSamplePinAlignForSoaking>();
                    AddRecipe(desc);

                    #endregion

                    #region => ProbingCommands
                    desc = CommandDescriptor.Create<IGoToStartDie>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IGoToCenterDie>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IMoveToNextDie>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IZUPRequest>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IZDownRequest>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IUnloadWafer>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IZDownAndPause>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ISetBinAnalysisData>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IMoveToDiePosition>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IMoveToDiePositionAndZUp>();
                    AddRecipe(desc);

                    #endregion

                    #region => SoakingCommands

                    desc = CommandDescriptor.Create<IEventSoakingCommand>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IStatusSoakingEndCommand>();
                    AddRecipe(desc);
                    #endregion

                    #region => TempControlCommands
                    desc = CommandDescriptor.Create<IChangeTemperatureToSetTemp>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IChangeTemperatureToSetTempWhenWaferTransfer>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IChangeTempToSetTempFullReach>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<ISetTempForFrontDoorOpen>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IReturnToDefaltSetTemp>();
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create<IChangeTemperatureToSetTempAfterConnectTempController>();

                    #endregion

                    desc = CommandDescriptor.Create<IDOWAFERALIGN>();
                    AddRecipe(desc);

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
                return retVal;
            }
        }

        [Serializable]
        public class ConsoleCommandRecipe : CommandRecipe
        {


            public override EventCodeEnum Init()
            {
                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                try
                {
                    retval = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    LoggerManager.Debug($"[ConsoleCommandRecipe] [Method = Init] [Error = {err}]");
                    retval = EventCodeEnum.PARAM_ERROR;
                }

                return retval;
            }
            public ConsoleCommandRecipe()
            {
            }

            [XmlIgnore, JsonIgnore]
            public override string FilePath { get; } = "Internal";
            [XmlIgnore, JsonIgnore]
            public override string FileName { get; } = "Command_RECIPE_Console.Json";
            public override EventCodeEnum SetEmulParam()
            {
                return SetDefaultParam();
            }
            public override EventCodeEnum SetDefaultParam()
            {
                EventCodeEnum retVal;
                try
                {

                    string NO_CMD_NAME = CommandNameGen.Generate(typeof(NoCommand));

                    CommandDescriptor desc;
                    desc = CommandDescriptor.Create("LotStart");
                    desc.Parameter = new CallbackParam()
                    {
                        Command = CommandNameGen.Generate(typeof(ILotOpStart)),
                    };
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create("LotPause");
                    desc.Parameter = new CallbackParam()
                    {
                        Command = CommandNameGen.Generate(typeof(ILotOpPause)),
                    };
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create("LotResume");
                    desc.Parameter = new CallbackParam()
                    {
                        Command = CommandNameGen.Generate(typeof(ILotOpResume)),
                    };
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create("LotEnd");
                    desc.Parameter = new CallbackParam()
                    {
                        Command = CommandNameGen.Generate(typeof(ILotOpEnd)),
                    };
                    AddRecipe(desc);

                    desc = CommandDescriptor.Create("LotPauseThenEnd");
                    desc.Parameter = new CallbackParam()
                    {
                        Command = CommandNameGen.Generate(typeof(IUnloadAllWafer)),
                    };
                    AddRecipe(desc);

                    retVal = EventCodeEnum.NONE;

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
}
