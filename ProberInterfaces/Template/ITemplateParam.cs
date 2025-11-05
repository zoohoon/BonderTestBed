namespace ProberInterfaces.Template
{

    using ProberErrorCode;
    using System.Collections.ObjectModel;

    public interface ITemplateFileParam : IParam
    {
        ITemplateParameter Param { get; set; }
        EventCodeEnum SetDefaultTemplates();
    }
    public interface ITemplateParam
    {
        CategoryModuleBase SubRoutineModule { get; }
        ObservableCollection<CategoryModuleBase> TemlateModules { get; }
        ObservableCollection<ControlTemplateParameter> ControlTemplates { get; }
        ObservableCollection<RecoveryTemplateParameter> RecoveryTemplates { get; }
    }

}
