using System;
using System.Collections.Generic;

namespace LampModule
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Lamp;
    using System.Xml.Serialization;

    [Serializable]
    public class LampSystemParameter : ISystemParameterizable, IParamNode, IParam
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
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


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        [XmlIgnore, JsonIgnore]
        public String Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner { get; set; }
        
        [XmlIgnore, JsonIgnore]
        public String FilePath { get; } = "";
        [XmlIgnore, JsonIgnore]
        public String FileName { get; } = "LampSystemParameter.json";

        private List<ModuleMonitorCombination> _ModuleLampCombination = new List<ModuleMonitorCombination>();
        public List<ModuleMonitorCombination> ModuleLampCombination
        {
            get { return _ModuleLampCombination; }
            set { _ModuleLampCombination = value; }
        }
        public LampSystemParameter()
        { }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            
            try
            {
                _ModuleLampCombination = new List<ModuleMonitorCombination>();
                String lotOpModuleStr = this.LotOPModule().GetType().Name;
                //==> LotOPModule
                
                //====> Error
                _ModuleLampCombination.Add(new ModuleMonitorCombination(
                    moduleState:        ModuleStateEnum.ERROR,
                    redLampStatus:      LampStatusEnum.On,  //==> R
                    yellowLampStatus:   LampStatusEnum.Off, //==> Y
                    blueLampStatus:     LampStatusEnum.Off, //==> B
                    buzzerStatus:       LampStatusEnum.Off, //==> Buzzer
                    priority:           AlarmPriority.Emergency,
                    id:lotOpModuleStr));

                //====> Running
                _ModuleLampCombination.Add(new ModuleMonitorCombination(
                    moduleState:        ModuleStateEnum.RUNNING,
                    redLampStatus:      LampStatusEnum.Off,
                    yellowLampStatus:   LampStatusEnum.Off,
                    blueLampStatus:     LampStatusEnum.On,
                    buzzerStatus:       LampStatusEnum.Off,
                    id:lotOpModuleStr));
                
                //====> Paused
                _ModuleLampCombination.Add(new ModuleMonitorCombination(
                    moduleState:        ModuleStateEnum.PAUSED,
                    redLampStatus:      LampStatusEnum.Off,
                    yellowLampStatus:   LampStatusEnum.Off,
                    blueLampStatus:     LampStatusEnum.BlinkOn,
                    buzzerStatus:       LampStatusEnum.Off,
                    id:lotOpModuleStr));

                //====> Idle
                _ModuleLampCombination.Add(new ModuleMonitorCombination(
                    moduleState:        ModuleStateEnum.IDLE,
                    redLampStatus:      LampStatusEnum.Off,
                    yellowLampStatus:   LampStatusEnum.On,
                    blueLampStatus:     LampStatusEnum.Off,
                    buzzerStatus:       LampStatusEnum.Off,
                    id:lotOpModuleStr));

                //====> Done
                _ModuleLampCombination.Add(new ModuleMonitorCombination(
                    moduleState:        ModuleStateEnum.DONE,
                    redLampStatus:      LampStatusEnum.Off,
                    yellowLampStatus:   LampStatusEnum.BlinkOn,
                    blueLampStatus:     LampStatusEnum.Off,
                    buzzerStatus:       LampStatusEnum.Off,
                    id:lotOpModuleStr));

                String loaderOpModuleStr = this.LoaderOPModule().GetType().Name;
                //==> LoaderOPModule

                //====> Error
                _ModuleLampCombination.Add(new ModuleMonitorCombination(
                    moduleState:        ModuleStateEnum.ERROR,
                    redLampStatus:      LampStatusEnum.On,
                    yellowLampStatus:   LampStatusEnum.Off,
                    blueLampStatus:     LampStatusEnum.Off,
                    buzzerStatus:       LampStatusEnum.Off,
                    priority:           AlarmPriority.Emergency,
                    id:loaderOpModuleStr));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return EventCodeEnum.NONE;
        }
    }
}
