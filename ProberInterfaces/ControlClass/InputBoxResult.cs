using LogModule;
using System;

namespace ProberInterfaces.ControlClass
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class InputBoxResult : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _UserCoordX;
        public int UserCoordX
        {
            get { return _UserCoordX; }
            set
            {
                if (value != _UserCoordX)
                {
                    _UserCoordX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _UserCoordY;
        public int UserCoordY
        {
            get { return _UserCoordY; }
            set
            {
                if (value != _UserCoordY)
                {
                    _UserCoordY = value;
                    RaisePropertyChanged();
                }
            }
        }

        public InputBoxResult()
        {

        }
        public InputBoxResult(int usercoordx, int usercoordy)
        {
            try
            {
            this.UserCoordX = usercoordx;
            this.UserCoordY = usercoordy;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }

    public class LoginDialogResult : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _ID;
        public string ID
        {
            get { return _ID; }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    RaisePropertyChanged();
                }
            }
        }


        private string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                if (value != _Password)
                {
                    _Password = value;
                    RaisePropertyChanged();
                }
            }
        }

        public LoginDialogResult()
        {

        }
        public LoginDialogResult(string id, string password)
        {
            try
            {
            this.ID = id;
            this.Password = password;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
