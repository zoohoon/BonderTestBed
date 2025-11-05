using System;

namespace Cognex.Command.CognexCommandPack.Tune
{
    using LogModule;
    using System.Xml;
    public class GetConfigTune : CognexEVCommand
    {
        public String UseLimits { get; set; } = "0";
        public String LightEnable { get; set; } = "1";
        public String SizeEnable { get; set; } = "1";
        public String FilterEnable { get; set; } = String.Empty;

        public override bool ParseResponse(string response)
        {
            if (base.ParseResponse(response) == false)
                return false;

            bool result = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                using (XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc))
                {
                    Status = GetNextNodeValue(xmlReader, nameof(Status));
                    UseLimits = GetNextNodeValue(xmlReader, nameof(UseLimits));
                    LightEnable = GetNextNodeValue(xmlReader, "Enable");
                    SizeEnable = GetNextNodeValue(xmlReader, "Enable");
                    FilterEnable = GetNextNodeValue(xmlReader, "Enable");
                }

                do
                {
                    if (Status == "1")
                    {
                        result = true;
                        break;
                    }
                    if (Status == "0")//==> Unrecognized command.
                        break;
                    if (Status == "-2")//==> The command could not be executed.
                        break;
                    if (Status == "6")//==>User does not have Full Access to execute the command.
                        break;
                } while (false);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
    }
}
