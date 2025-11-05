namespace GPLoaderRouter
{
    using GPLoaderRouter.RobotCommands;
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class GPLoaderRobotCommandParam : ISystemParameterizable
    {
        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                SetDefaultParam();
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;
            try
            {
                //TODO 이어서 추가.
                GPLoaderRobotCommandList = new List<IGPLoaderRobotCommand>();
                GPLoaderRobotCommandList.Add(new Buffer_Pick());
                GPLoaderRobotCommandList.Add(new Buffer_Put());
                GPLoaderRobotCommandList.Add(new CardBuffer_Pick());
                GPLoaderRobotCommandList.Add(new CardBuffer_Put());
                GPLoaderRobotCommandList.Add(new CardChange_Pick());
                GPLoaderRobotCommandList.Add(new CardChange_Put());
                GPLoaderRobotCommandList.Add(new Cassette_Pick());
                GPLoaderRobotCommandList.Add(new Cassette_Put());
                GPLoaderRobotCommandList.Add(new CSTDrawer_Pick());
                GPLoaderRobotCommandList.Add(new CSTDrawer_Put());
                GPLoaderRobotCommandList.Add(new FixedTray_Pick());
                GPLoaderRobotCommandList.Add(new FixedTray_Put());
                GPLoaderRobotCommandList.Add(new PreAligner_Pick());
                GPLoaderRobotCommandList.Add(new PreAligner_Put());
                GPLoaderRobotCommandList.Add(new Stage_Pick());
                GPLoaderRobotCommandList.Add(new Stage_Put());
                GPLoaderRobotCommandList.Add(new CardIDRead_Move());

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public void SetElementMetaData()
        {

        }

        public string FilePath { get; } = "";

        public string FileName { get; } = "GPLoaderRobotCommandParameter.json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        public List<object> Nodes { get; set; }

        private List<IGPLoaderRobotCommand> _GPLoaderRobotCommandList;
        public List<IGPLoaderRobotCommand> GPLoaderRobotCommandList
        {
            get { return _GPLoaderRobotCommandList; }
            set
            {
                if (value != _GPLoaderRobotCommandList)
                {
                    _GPLoaderRobotCommandList = value;
                }
            }
        }
    }
}
