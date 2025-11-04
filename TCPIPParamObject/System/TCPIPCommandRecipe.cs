using ProberErrorCode;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;

namespace TCPIPParamObject
{

    [Serializable]
    public class TCPIPCommandRecipe : CommandRecipe
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

        public TCPIPCommandRecipe() { }

        [XmlIgnore, JsonIgnore]
        public override string FilePath { get; } = "TCPIP";
        [XmlIgnore, JsonIgnore]
        public override string FileName { get; } = "Command_RECIPE_TCPIP.Json";

        public override EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {

                CommandDescriptor desc;

                desc = CommandDescriptor.Create<ITCPIP_RESPONSE>();
                AddRecipe(desc);

                desc = CommandDescriptor.Create<ITCPIP_ACTION>();
                AddRecipe(desc);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public override EventCodeEnum SetEmulParam()
        {
            return this.SetDefaultParam();
        }
    }
}
