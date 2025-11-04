using System;

namespace Cognex.Command.CognexCommandPack.Tune
{
    using LogModule;
    using System.Xml;
    public class TuneConfigEx : CognexEVCommand
    {
        public String Percent { get; set; } = "100";
        public String NumRead { get; set; } = "0";
        public String CurrentPassed { get; set; } = "0";
        public String CurrentString { get; set; } = "**********";
        public String CurrentScore { get; set; } = "0";
        public String BestPassed { get; set; } = "0";
        public String BestString { get; set; } = "**********";
        public String BestScore { get; set; } = "0";
        public override bool ParseResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
                return false;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(response);

            using (XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc))
            {
                Status = GetNextNodeValue(xmlReader, nameof(Status));
                Percent = GetNextNodeAttribute(xmlReader, "TuneStep", nameof(Percent));
                NumRead = GetNextNodeValue(xmlReader, nameof(NumRead));
                CurrentPassed = GetNextNodeValue(xmlReader, "Passed");
                CurrentString = GetNextNodeValue(xmlReader, "String");
                CurrentScore = GetNextNodeValue(xmlReader, "Score");
                BestPassed = GetNextNodeValue(xmlReader, "Passed");
                BestString = GetNextNodeValue(xmlReader, "String");
                BestScore = GetNextNodeValue(xmlReader, "Score");
            }

            bool result = false;
            try
            {
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
