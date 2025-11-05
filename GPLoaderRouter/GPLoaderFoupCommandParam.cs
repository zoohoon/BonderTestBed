using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPLoaderRouter
{
    using GPLoaderRouter.FoupCommands;    
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

    public class GPLoaderFoupCommandParam : ISystemParameterizable
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
                GPLoaderFoupCommandList = new List<IGPLoaderCSTCtrlCommand>();
                GPLoaderFoupCommandList.Add(new Foup_CoverClose(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_CoverClose(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_CoverClose(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_CoverOpen(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_CoverOpen(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_CoverOpen(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_CoverLock(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_CoverLock(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_CoverLock(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_CoverUnlock(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_CoverUnlock(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_CoverUnlock(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_CstLock(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_CstLock(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_CstLock(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_CstUnlock(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_CstUnlock(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_CstUnlock(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_DPIn(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_DPIn(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_DPIn(SubstrateSizeEnum.INCH6));

                GPLoaderFoupCommandList.Add(new Foup_DPOut(SubstrateSizeEnum.INCH12));
                GPLoaderFoupCommandList.Add(new Foup_DPOut(SubstrateSizeEnum.INCH8));
                GPLoaderFoupCommandList.Add(new Foup_DPOut(SubstrateSizeEnum.INCH6));
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

        public string FileName { get; } = "GPLoaderFoupCommandParameter.json";

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

        private List<IGPLoaderCSTCtrlCommand> _GPLoaderFoupCommandList;
        public List<IGPLoaderCSTCtrlCommand> GPLoaderFoupCommandList
        {
            get { return _GPLoaderFoupCommandList; }
            set
            {
                if (value != _GPLoaderFoupCommandList)
                {
                    _GPLoaderFoupCommandList = value;
                }
            }
        }
    }
}
