
using ProberErrorCode;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace ProberInterfaces.Vision
{

    public interface IDrawable
    {
        EventCodeEnum Draw(IDisplay display, ImageBuffer img);
    }

    public interface IControlDrawable : IDrawable
    {
        Color Color { get; set; }
        string StringTypeColor { get; set; }
    }

    public interface ITextDrawable : IDrawable
    {
        Color Fontcolor { get; set; }
        string StringTypeFontColor { get; set; }

        Color BackColor { get; set; }
        string StringTypeBackColor { get; set; }
    }

    [DataContract]
    public abstract class ControlDrawableBase : IControlDrawable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private string _StringTypeColor;
        [DataMember]
        public string StringTypeColor
        {
            get { return _StringTypeColor; }
            set
            {
                if (value != _StringTypeColor)
                {
                    _StringTypeColor = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Color _Color = default(Color);
        public Color Color
        {
            get { return _Color; }
            set
            {
                if (value != _Color)
                {
                    _Color = value;
                    StringTypeColor = _Color.ToString();
                    RaisePropertyChanged();
                }
            }
        }

        public abstract EventCodeEnum Draw(IDisplay display, ImageBuffer img);
    }
}
