using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProberInterfaces.PnpSetup
{
    using ProberErrorCode;
    using ProberInterfaces.State;
    using SharpDXRender.RenderObjectPack;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public interface IPnpManager : IFactoryModule, INotifyPropertyChanged, IModule
    {
        ICategoryNodeItem SeletedStep { get; }
        IPnpSetup SelectedPnpStep { get; }
        IPnpSetupScreen PnpScreen { get; set; }
        ICategoryNodeItem CurStep { get; }
        IDisplayPort DisplayPort { get; }
        ObservableCollection<ObservableCollection<ICategoryNodeItem>> PnpSteps { get; }
        ObservableCollection<ICategoryNodeItem> PnPNodeItem { get; }
        Task<EventCodeEnum> SetDefaultInitViewModel(object step = null);
        //EventCodeEnum SetNodeSetupState(object module);
        EventCodeEnum SetPnpStps(ObservableCollection<ObservableCollection<ICategoryNodeItem>> pnpsteps);
        EventCodeEnum GetPnpSteps(object module);
        Task<EventCodeEnum> SetSeletedStep(ICategoryNodeItem module);
        Task<EventCodeEnum> SetViewModel(object value, object test = null);
        Guid GetViewGuid(object module, Guid cuiguid);
        EventCodeEnum GetCuiBtnParam(object module, Guid cuiguid, out Guid viewguid, out List<Guid> stepguids, bool extrastep = false);
        EventCodeEnum SetNavListToGUIDs(object module, List<Guid> guids);
        ObservableCollection<ICategoryNodeItem> GetNotFormPnpStps();
        EventCodeEnum ParamValidationSteps();
        ILightJobViewModel PnpLightJog { get; set; }
        IHexagonJogViewModel PnpMotionJog { get; set; }
        ObservableCollection<ObservableCollection<CategoryNameItems>> GetCategoryNameList(string modulename, string interfacename, Guid cuiguid, bool extrastep = false);
        Task<EventCodeEnum> StepPageSwitching(string moduleheader, object parameter);
        void StepPageReload();
        Task<EventCodeEnum> StepCleanup(string moduleheader, object parameter = null);
        bool StepIsParameterChanged(string moduleheader, bool issave);
        EventCodeEnum StepParamValidation(string moduleheader);
        Task SetCurrentStep(string moduleheader);
        PNPDataDescription GetPNPDataDescriptor();
        void ApplyParams(List<byte[]> parameters);
        void CloseAdvanceSetupView();
        List<byte[]> ParamObjectListConvertToByteList(List<object> parameters);
        Task ClosePnpAdavanceSetupWindow();
        void SetSetupState(string moduleheader = null);

        void SetMiniViewTarget(object miniView);
        EnumMoudleSetupState GetSetupState(string moduleheader = null);

        List<RenderContainer> GetRenderContainers();
        void SetDislayPortTargetRectInfo(double left, double top);
        bool IsActivePageSwithcing();
        Task SettingRemotePNP(string modulename, string interfacename, Guid cuiguid);
        Task SettingRemoteRecoveryPNP(string modulename, string interfacename, Guid cuiguid, bool extractstep);
        Task PnpCleanup();
        IDutViewControlVM DutViewControl { get; }

        EventCodeEnum SetAdvancedViewModel(IPnpAdvanceSetupViewModel vm);
        //List<LightValueParam> LastUsedLightParams {get;set;}

        EventCodeEnum RememberLastLightSetting(ICamera cam);
        EventCodeEnum RestoreLastLightSetting(ICamera cam);
    }
}
