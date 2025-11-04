using System;
using System.Collections.Generic;

namespace Cognex.Command.CognexCommandPack.GetInformation
{
    using LogModule;
    using System.Threading;
    using System.Xml;

    public class ReadConfig : CognexEVCommand
    {
        public String Passed { get; set; } = "1";
        public String String { get; set; } = "**********";
        public String Score { get; set; } = "0";
        public String TryLog { get; set; } = "0";
        public String TuneLog { get; set; } = "0";
        public String LastRead { get; set; } = "10";
        public String NumPassed { get; set; } = "24";
        public String NumFailed { get; set; } = "0";
        public String NumTunedPassed { get; set; } = "0";
        public String NumTuneFailed { get; set; } = "0";
        public String AvgScore { get; set; } = "96.2941210";
        public String High { get; set; } = "40";
        public String Wide { get; set; } = "21";
        public String Theta { get; set; } = "0";
        public String BaseScore { get; set; } = "97";
        public String ExecutionSeconds { get; set; } = "0.186";
        public List<Orc> OrcList { get; set; } = new List<Orc>();

        public override bool ParseResponse(string response)
        {
            OrcList = new List<Orc>();

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
                    Passed = GetNextNodeValue(xmlReader, nameof(Passed));
                    String = GetNextNodeValue(xmlReader, nameof(String));
                    Score = GetNextNodeValue(xmlReader, nameof(Score));
                    TryLog = GetNextNodeValue(xmlReader, nameof(TryLog));
                    TuneLog = GetNextNodeValue(xmlReader, nameof(TuneLog));
                    LastRead = GetNextNodeValue(xmlReader, nameof(LastRead));
                    NumPassed = GetNextNodeValue(xmlReader, nameof(NumPassed));
                    NumFailed = GetNextNodeValue(xmlReader, nameof(NumFailed));
                    NumTunedPassed = GetNextNodeValue(xmlReader, nameof(NumTunedPassed));
                    NumTuneFailed = GetNextNodeValue(xmlReader, nameof(NumTuneFailed));
                    AvgScore = GetNextNodeValue(xmlReader, nameof(AvgScore));
                    High = GetNextNodeValue(xmlReader, nameof(High));
                    Wide = GetNextNodeValue(xmlReader, nameof(Wide));
                    Theta = GetNextNodeValue(xmlReader, nameof(Theta));
                    BaseScore = GetNextNodeValue(xmlReader, nameof(BaseScore));
                    ExecutionSeconds = GetNextNodeValue(xmlReader, nameof(ExecutionSeconds));

                    while (true)
                    {
                        if (xmlReader.ReadToFollowing(nameof(Orc)) == false)
                            break;

                        Orc orc = new Orc();
                        orc.Char = GetNextNodeValue(xmlReader, nameof(orc.Char));
                        orc.Value = GetNextNodeValue(xmlReader, nameof(orc.Value));
                        orc.Row = GetNextNodeValue(xmlReader, nameof(orc.Row));
                        orc.Col = GetNextNodeValue(xmlReader, nameof(orc.Col));
                        OrcList.Add(orc);

                        //_delays.DelayFor(1);
                        Thread.Sleep(1);
                    }
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
    public class Orc
    {
        public String Char { get; set; }
        public String Value { get; set; }
        public String Row { get; set; }
        public String Col { get; set; }
        public Orc()
        {

        }
        public Orc(string ch, string val, string row, string col)
        {
            Char = ch;
            Value = val;
            Row = row;
            Col = col;
        }
    }
}
