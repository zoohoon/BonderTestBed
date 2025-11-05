using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class FileSavePath : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private EnumProberModule _ModuleType;
        public EnumProberModule ModuleType
        {
            get { return _ModuleType; }
            set
            {
                if (value != _ModuleType)
                {
                    _ModuleType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Path = string.Empty;
        public string Path
        {
            get { return _Path; }
            set
            {
                if (value != _Path)
                {
                    _Path = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FileSavePath(EnumProberModule moduletype, string path)
        {
            try
            {
                this.ModuleType = moduletype;
                this.Path = path;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
