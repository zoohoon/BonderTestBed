using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProberInterfaces.Param
{
    [Serializable]
    public class CoordinateSystemParameter : INotifyPropertyChanged
    {
        
        public CartesianCoord MarkPosition;
    
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public CoordinateSystemParameter()
        {
            MarkPosition = new CartesianCoord();
        }
    }
}
