namespace ProberInterfaces.PnpSetup
{
    using ProberErrorCode;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    //public interface IPnp : ICategoryNodeItem
    //{
    //}

    public interface IPnpCategoryForm : INotifyPropertyChanged
    {
        ObservableCollection<ITemplateModule> Categories { get; set; }
        EventCodeEnum SaveParameter();

        EventCodeEnum ValidationCategoryStep(object parameter);
    }
}
