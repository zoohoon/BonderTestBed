using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces
{
    public class SelectableItem<T> : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public SelectableItem(T item)
        {
            Item = item;
        }

        public T Item { get; set; }

        bool _isSelected;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value == _isSelected)
                {
                    return;
                }

                _isSelected = value;

                RaisePropertyChanged();

                //if (PropertyChanged != null)
                //{
                //    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("IsSelected"));
                //}
            }
        }

        //public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
