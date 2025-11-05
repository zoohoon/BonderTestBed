using System;

namespace ProberInterfaces.Lamp
{
    using Newtonsoft.Json;
    using ProberInterfaces;
    using System.Xml.Serialization;

    [Serializable]
    public class ModuleMonitorCombination : LampCombination
    {
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public IStateModule Module { get; set; }
        private Element<ModuleStateEnum> _ModuleState = new Element<ModuleStateEnum>();
        public Element<ModuleStateEnum> ModuleState
        {
            get { return _ModuleState; }
            set { _ModuleState = value; }
        }
        public ModuleMonitorCombination()
        {

        }
        public ModuleMonitorCombination(
            ModuleStateEnum moduleState,
            LampStatusEnum redLampStatus,
            LampStatusEnum yellowLampStatus,
            LampStatusEnum blueLampStatus,
            LampStatusEnum buzzerStatus,
            AlarmPriority priority = AlarmPriority.Normal,
            String id = "")
            : base(redLampStatus, yellowLampStatus, blueLampStatus, buzzerStatus, priority, id)
        {
            ModuleState.Value = moduleState;
        }
        public bool CheckModuleState()
        {
            if (Module == null)
                return false;

            bool result = Module.ModuleState.GetState() == ModuleState.Value;
            return result;
        }
        public String GetModuleState()
        {
            return ModuleState.Value.ToString();
        }
    }
}
