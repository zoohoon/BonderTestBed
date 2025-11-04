using System;

namespace Cognex.Command.CognexCommandPack.Light
{
    using LogModule;
    using System.Xml;
    public class GetConfigLightPower : CognexEVCommand
    {
        public String Float { get; set; } = "0";
        public override bool ParseResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
                return false;

            bool result = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                using (XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc))
                {
                    Status = GetNextNodeValue(xmlReader, nameof(Status));
                    Float = GetNextNodeValue(xmlReader, nameof(Float));
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
