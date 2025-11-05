using System;
using System.Collections.Generic;

namespace Cognex.Command.CognexCommandPack.Filter
{
    using LogModule;
    using System.Threading;
    using System.Xml;

    public class GetFilterOperationList : CognexEVCommand
    {
        public List<FilterOperation> FilterOperationList { get; set; } = new List<FilterOperation>();

        public override bool ParseResponse(string response)
        {
            FilterOperationList = new List<FilterOperation>();

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

                    while (true)
                    {
                        FilterOperation filterOperation = new FilterOperation();

                        if (xmlReader.ReadToFollowing(nameof(FilterOperation)) == false)
                            break;

                        filterOperation.Id = xmlReader.GetAttribute(nameof(filterOperation.Id));
                        filterOperation.Type = xmlReader.GetAttribute(nameof(filterOperation.Type));
                        filterOperation.Name = xmlReader.GetAttribute(nameof(filterOperation.Name));
                        filterOperation.AutoTuned = xmlReader.GetAttribute(nameof(filterOperation.AutoTuned));

                        List<String> param = new List<String>();
                        xmlReader.Read();

                        while (true)
                        {

                            if (xmlReader.Name == nameof(filterOperation.Param))
                            {
                                xmlReader.Read();//==> Start <Param>

                                String paramValue = xmlReader.Value;
                                param.Add(paramValue);

                                xmlReader.Read();//==> End <Param>
                                xmlReader.Read();//==> End <Param>
                            }

                            if (xmlReader.Name == nameof(FilterOperation))
                                break;

                            //_delays.DelayFor(1);
                            Thread.Sleep(1);
                        }

                        filterOperation.Param = param;
                        FilterOperationList.Add(filterOperation);

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

    public class FilterOperation
    {
        public String Id { get; set; }
        public String Type { get; set; }
        public String Name { get; set; }
        public String AutoTuned { get; set; }
        public List<String> Param { get; set; } = new List<String>();
    }
}
