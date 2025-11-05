using ProberErrorCode;
using ProberInterfaces.Communication.Scenario;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ProberInterfaces
{
    public interface IHasTesterScenarioModule
    {
        ITesterScenarioModule ScenarioModule { get; set; }
    }
    public interface ITesterComDriver
    {
        EventCodeEnum Connect(object connectparam);
        EventCodeEnum DisConnect();
        object GetState();
        string Read();
        //void WriteSTB(int? command);

        void WriteSTB(object command);

        void WriteString(string query_command);
        void Reset();

        //EventCodeEnum StartReceive();
        ObservableCollection<CollectionComponent> CommandCollection { get; set; }
    }

    public class CollectionComponent : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SolidColorBrush _Color;
        public SolidColorBrush Color
        {
            get { return _Color; }
            set
            {
                if (value != _Color)
                {
                    _Color = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
