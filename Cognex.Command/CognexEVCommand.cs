using System;

namespace Cognex.Command
{
    using LogModule;
    using System.Xml;

    /*
     * Cognex Command, 앞에 EV가 붙는다.
     */
    public abstract class CognexEVCommand
    {
        private String _Command;
        public String Command
        {
            get { return "EV " + _Command; }
        }
        protected String WafID { get; set; }
        protected String Config { get; set; }
        public String CommandFormatFrame { get; set; }
        public String Status { get; set; }
        public CognexEVCommand()
        {
            try
            {
                WafID = "A4";
                Config = "0";

                GenerateCommandFormatFrame(WafID, Config);

                _Command = CommandFormatFrame;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool GenerateCommandFormatFrame(String waferID, String config)
        {
            if (String.IsNullOrEmpty(WafID))
                return false;

            if (String.IsNullOrEmpty(Config))
                return false;

            CommandFormatFrame = $"{this.GetType().Name}({waferID},{config})";

            return true;
        }
        private String AppendCommandFormatArg(int argCount)
        {
            if (String.IsNullOrEmpty(CommandFormatFrame))
                return String.Empty;

            //==> Remove last )

            String commandFormat = CommandFormatFrame.Remove(CommandFormatFrame.Length - 1);
            try
            {

                //==> Add Argument
                for (int i = 0; i < argCount; i++)
                    commandFormat = commandFormat + ",{" + i + "}";

                //==> Close )
                commandFormat = commandFormat + ")";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return commandFormat;
        }
        public bool SetCommandFormatArg(params object[] args)
        {
            /*
             * _Command에 인자를 덫붙인다.
             */

            try
            {
                if (String.IsNullOrEmpty(CommandFormatFrame))
                {
                    return false;
                }

                String commandFormat = AppendCommandFormatArg(args.Length);
                if (String.IsNullOrEmpty(commandFormat))
                {
                    return false;
                }

                _Command = String.Format(commandFormat, args);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return true;
        }
        public virtual bool ParseResponse(String response)
        {
            /*
             * 보낸 Command에 대해 응답(response)을 받고 파싱을 한다.
             * response는 XML 방식의 문자열이다
             */

            bool result = false;

            try
            {
                if (String.IsNullOrEmpty(response))
                    return false;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                using (XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc))
                {
                    Status = GetNextNodeValue(xmlReader, nameof(Status));
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
        protected String GetNextNodeValue(XmlNodeReader xmlReader, String nodeName)
        {
            /*
             * XML에서 다음 노드 값을 가져옴
             * 
             * EX)
             * <Field></Field>
             * <String></String>
             * 
             * Field 의 다음 노드는 String이다.
             */
            try
            {
                xmlReader.ReadToFollowing(nodeName);
                xmlReader.Read();
                return xmlReader.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        protected String GetNextNodeAttribute(XmlNodeReader xmlReader, String nodeName, String attribute)
        {
            /*
             * XML에서 다음 Attribute를 가져 온다.
             * <Field id="A" value="10"></Field>
             * 
             * 'id' Attribute의 다음 'Attribute'는 value 이다.
             */
            try
            {
                xmlReader.ReadToFollowing(nodeName);
                return xmlReader.GetAttribute(attribute);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
