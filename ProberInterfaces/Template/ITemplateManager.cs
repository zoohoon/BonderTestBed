namespace ProberInterfaces.Template
{
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    public interface ITemplateManager : IModule, IFactoryModule
    {
        EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1);

        EventCodeEnum CheckTemplate(ITemplate module, bool applyload = true, int index = -1, int stageindex = -1);
        //EventCodeEnum CheckTemplateUsedType(Type moduletype, bool applyload = true, int index = -1);

        EventCodeEnum InitTemplate(ITemplate module, string fullpath =null, bool applyload = true);
        ITemplateCollection LoadTemplate(object hasTemplateModule);
        void SaveTemplate(object hasTemplateModule);

        ObservableCollection<ITemplateModule> GetModules(ITemplate module);
    }
}
