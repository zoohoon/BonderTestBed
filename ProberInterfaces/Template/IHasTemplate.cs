namespace ProberInterfaces.Template
{
    using ProberInterfaces.Wizard;

    public interface IHasSubModules
    { 
        ISubStepModules SubModules { get; }
    }
    public interface ICheckTemplate
    {

    }

    //public interface IHasTemplate : ICheckTemplate , IHasSubModules
    //{
    //    ITemplateParam SeletedTemplatFile { get; }
    //    ITemplateFileParam TemplateParam { get; }
    //    EventCodeEnum LoadTemplate();
    //    EventCodeEnum SaveTemplate();
    //    ObservableCollection<IWizardStep> GetTemplate();

    //}




    

}
