using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ProberInterfaces.ResultMap.Script
{
    public interface IMapScriptElement
    {

    }

    [Serializable]
    public class MapScriptElement : IMapScriptElement, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized]
        private SolidColorBrush _Background;
        [JsonIgnore]
        public SolidColorBrush Background
        {
            get { return _Background; }
            set
            {
                if (value != _Background)
                {
                    _Background = value;
                    RaisePropertyChanged();
                }
            }
        }

        public MapScriptElement()
        {
            this.Text = string.Empty;
            Background = Brushes.Gray;
        }

        public MapScriptElement(string text, SolidColorBrush background = null)
        {
            this.Text = text;

            if (background == null)
            {
                this.Background = Brushes.Gray;
            }
            else
            {
                this.Background = background;
            }

        }
    }
}
