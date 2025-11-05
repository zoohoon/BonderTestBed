using LogModule;
using System;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using LoaderBase;

namespace ProberViewModel
{
    public class GPLoaderJobVM : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GPLoaderJobWindow window = null;



        private IGPLoader _GPLoader;
        public IGPLoader GPLoader
        {
            get { return _GPLoader; }
            set
            {
                if (value != _GPLoader)
                {
                    _GPLoader = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ILoaderModule _LoaderModule;
        public ILoaderModule LoaderModule
        {
            get { return _LoaderModule; }
            set
            {
                if (value != _LoaderModule)
                {
                    _LoaderModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        


        public GPLoaderJobVM()
        {

        }

        public bool Show(ILoaderModule loader)
        {
            bool isCheck = false;
            LoaderModule = loader;
            
            try
            {
                if (window != null && window.Visibility == Visibility.Visible)
                {
                    window.Close();
                }

                String retValue = String.Empty;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    window = new GPLoaderJobWindow();
                    window.DataContext = this;
                    window.Show();
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return isCheck;
        }

    

   
    }
}
