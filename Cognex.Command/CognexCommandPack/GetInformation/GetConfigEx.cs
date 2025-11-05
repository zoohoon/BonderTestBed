using System;

namespace Cognex.Command.CognexCommandPack.GetInformation
{
    using LogModule;
    using System.Xml;
    public class GetConfigEx : CognexEVCommand
    {
        public String Mark { get; set; } = "4";
        public String Checksum { get; set; } = "2";
        public String LightMode { get; set; } = "1";
        public String LightPower { get; set; } = "2";
        public String Orientation { get; set; } = "0";
        public String RegionY { get; set; } = "190";
        public String RegionX { get; set; } = "70";
        public String RegionHeight { get; set; } = "100";
        public String RegionWidth { get; set; } = "500";
        public String RegionTheta { get; set; } = "0";
        public String RegionPhi { get; set; } = "0";
        public String CharHeight { get; set; } = "42";
        public String CharWidth { get; set; } = "20";
        public String CharSpacing { get; set; } = "1";
        public String FieldString { get; set; } = "**********CS";
        public String FieldDef { get; set; } = String.Empty;
        public String Color { get; set; } = "0";
        public String Accept { get; set; } = "50";
        public String Enable { get; set; } = "0";
        public String Retry { get; set; } = "0";
        public String BComp { get; set; } = "0";
        public String Thresh { get; set; } = "25";
        public String Optimization { get; set; } = "0";
        public String StrokeThickness { get; set; } = "0";
        public String MatchString { get; set; } = String.Empty;
        public String ConfigName { get; set; } = "0";
        public String LEDOverride { get; set; } = "0";
        public String LEDOverrideMask { get; set; } = "0";
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
                    Mark = GetNextNodeValue(xmlReader, nameof(Mark));
                    Checksum = GetNextNodeValue(xmlReader, nameof(Checksum));
                    LightMode = GetNextNodeValue(xmlReader, nameof(LightMode));
                    LightPower = GetNextNodeValue(xmlReader, nameof(LightPower));
                    Orientation = GetNextNodeValue(xmlReader, nameof(Orientation));
                    //==> Region
                    RegionY = GetNextNodeValue(xmlReader, "Row");
                    RegionX = GetNextNodeValue(xmlReader, "Col");
                    RegionHeight = GetNextNodeValue(xmlReader, "High");
                    RegionWidth = GetNextNodeValue(xmlReader, "Wide");
                    RegionTheta = GetNextNodeValue(xmlReader, "Theta");
                    RegionPhi = GetNextNodeValue(xmlReader, "Phi");

                    //==> Char Size
                    CharHeight = GetNextNodeValue(xmlReader, "High");
                    CharWidth = GetNextNodeValue(xmlReader, "Wide");
                    CharSpacing = GetNextNodeValue(xmlReader, nameof(CharSpacing));
                    FieldString = GetNextNodeValue(xmlReader, nameof(FieldString));
                    FieldDef = GetNextNodeValue(xmlReader, nameof(FieldDef));
                    Color = GetNextNodeValue(xmlReader, nameof(Color));
                    Accept = GetNextNodeValue(xmlReader, nameof(Accept));
                    Enable = GetNextNodeValue(xmlReader, nameof(Enable));
                    Retry = GetNextNodeValue(xmlReader, nameof(Retry));
                    BComp = GetNextNodeValue(xmlReader, nameof(BComp));
                    Thresh = GetNextNodeValue(xmlReader, nameof(Thresh));
                    Optimization = GetNextNodeValue(xmlReader, nameof(Optimization));
                    StrokeThickness = GetNextNodeValue(xmlReader, nameof(StrokeThickness));
                    MatchString = GetNextNodeValue(xmlReader, nameof(MatchString));
                    ConfigName = GetNextNodeValue(xmlReader, nameof(ConfigName));
                    LEDOverride = GetNextNodeValue(xmlReader, nameof(LEDOverride));
                    LEDOverrideMask = GetNextNodeValue(xmlReader, nameof(LEDOverrideMask));
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
