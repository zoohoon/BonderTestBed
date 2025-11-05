using ProberInterfaces.PMI;
using System.ComponentModel;

namespace ProberInterfaces
{
    public interface IPMILog
    {
        Element<LOGGING_MODE> LoggingMode { get; set; }
        Element<bool> LogSaveHDDEnable { get; set; }
        Element<bool> LogSaveFTPEnable { get; set; }
        Element<bool> LogSaveNETEnable { get; set; }
        Element<bool> FailImageSaveHDDEnable { get; set; }
        Element<bool> FailImageSaveFTPEnable { get; set; }
        Element<bool> FailImageSaveNETEnable { get; set; }
        Element<bool> PassImageSaveHDDEnable { get; set; }
        Element<bool> PassImageSaveFTPEnable { get; set; }
        Element<bool> PassImageSaveNETEnable { get; set; }
        Element<bool> OriginalImageSaveHDDEnable { get; set; }
    }

    public interface IPMIModuleDevParam : IDeviceParameterizable, INotifyPropertyChanged
    {
        IPMIFocusingDLLInfo FocusingDllInfo { get; set; }
        Element<EnumProberCam> ProcessingCamType { get; set; }
        Element<double> PadGroupingMargin { get; set; }
        Element<ushort> LightValue { get; set; }
        Element<bool> AutoLightEnable { get; set; }
        Element<ushort> AutoLightStart { get; set; }
        Element<ushort> AutoLightEnd { get; set; }
        Element<ushort> AutoLightInterval { get; set; }
        Element<bool> SearchPadEnable { get; set; }
        IPMILog LogInfo { get; set; }
        Element<int> FocusingDutInterval { get; set; }
        Element<OP_MODE> NormalPMI { get; set; }
    }

    public interface IPMIFocusingDLLInfo
    {
        ModuleDllInfo NormalFocusingInfo { get; set; }
        ModuleDllInfo BumpFocusingInfo { get; set; }
    }

}
