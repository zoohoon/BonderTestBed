namespace ProberInterfaces.Category
{
    using System.Collections.ObjectModel;
    public class InitDataSettingTabel
    {
        private ObservableCollection<SettingData> _DataTabel
             = new ObservableCollection<SettingData>();

        public ObservableCollection<SettingData> DataTabel
        {
            get { return _DataTabel; }
            set { _DataTabel = value; }
        }

    }

    public class SettingData
    {
        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        private object _Value;

        public object Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public SettingData()
        {

        }

        public SettingData(string description, object value)
        {
            Description = description;
            Value = value;
        }
    }

}
