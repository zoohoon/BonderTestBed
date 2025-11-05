using LogModule;
using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProberInterfaces;
using RelayCommandBase;
using LoaderBase;
using VirtualKeyboardControl;

namespace ProberViewModel
{
    public class GPFoupSettingVM : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private GPFoupSettingWindow window = null;

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

        private EnumCSTCtrl _SelectedType1;
        public EnumCSTCtrl SelectedType1
        {
            get { return _SelectedType1; }
            set
            {
                if (value != _SelectedType1)
                {
                    _SelectedType1 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumCSTCtrl _SelectedType2;
        public EnumCSTCtrl SelectedType2
        {
            get { return _SelectedType2; }
            set
            {
                if (value != _SelectedType2)
                {
                    _SelectedType2 = value;
                    RaisePropertyChanged();
                }
            }
        }
        private EnumCSTCtrl _SelectedType3;
        public EnumCSTCtrl SelectedType3
        {
            get { return _SelectedType3; }
            set
            {
                if (value != _SelectedType3)
                {
                    _SelectedType3 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _EmulScanCount;
        public int EmulScanCount
        {
            get { return _EmulScanCount; }
            set
            {
                _EmulScanCount = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand<Object> _TextBoxClickCommand;
        public ICommand TextBoxClickCommand
        {
            get
            {
                if (null == _TextBoxClickCommand) _TextBoxClickCommand = new RelayCommand<Object>(TextBoxClickCommandFunc);
                return _TextBoxClickCommand;
            }
        }
        private void TextBoxClickCommandFunc(Object param)
        {
            System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
            string value = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 10);
         
            if (int.TryParse(value, out int result) && result > 0 && result < 14)
            {
                LoaderModule.EmulScanCount = result;
                tb.Text = value;
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            else
            {
                tb.Text = "0";
                LoaderModule.EmulScanCount = 0;
            }
        }
        public GPFoupSettingVM()
        {
         
        }

        public bool Show(ILoaderModule loaderModule)
        {
            bool isCheck = false;
            LoaderModule = loaderModule;
            GPLoader = LoaderModule.GetGPLoader();
            try
            {
                if (window != null && window.Visibility == Visibility.Visible)
                {
                    window.Close();
                }

                String retValue = String.Empty;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    window = new GPFoupSettingWindow();
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

        private RelayCommand<Object> _ChangedTypeCommand;
        public ICommand ChangedTypeCommand
        {
            get
            {
                if (null == _ChangedTypeCommand) _ChangedTypeCommand = new RelayCommand<Object>(ChangedTypeCommandFunc);
                return _ChangedTypeCommand;
            }
        }

        private void ChangedTypeCommandFunc(object obj)
        {
            try
            {
             

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _SetValueCmd1;
        public ICommand SetValueCmd1
        {
            get
            {
                if (null == _SetValueCmd1) _SetValueCmd1 = new RelayCommand(SetValueCmd1Func);
                return _SetValueCmd1;
            }
        }
        private void SetValueCmd1Func()
        {
            try
            {
                (GPLoader as GPLoaderRouter.GPLoader).WriteCSTCtrlCommand(0, SelectedType1);
                //FoupOpModule.GetFoupController(1).Service.FoupModule.InitState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SetValueCmd2;
        public ICommand SetValueCmd2
        {
            get
            {
                if (null == _SetValueCmd2) _SetValueCmd2 = new RelayCommand(SetValueCmd2Func);
                return _SetValueCmd2;
            }
        }
        private void SetValueCmd2Func()
        {
            try
            {

                (GPLoader as GPLoaderRouter.GPLoader).WriteCSTCtrlCommand(1, SelectedType2);
                //FoupOpModule.GetFoupController(2).Service.FoupModule.InitState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _SetValueCmd3;
        public ICommand SetValueCmd3
        {
            get
            {
                if (null == _SetValueCmd3) _SetValueCmd3 = new RelayCommand(SetValueCmd3Func);
                return _SetValueCmd3;
            }
        }
        private void SetValueCmd3Func()
        {
            try
            {
                (GPLoader as GPLoaderRouter.GPLoader).WriteCSTCtrlCommand(2, SelectedType3);
                //FoupOpModule.GetFoupController(3).Service.FoupModule.InitState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}
