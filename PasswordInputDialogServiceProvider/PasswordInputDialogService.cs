using LogModule;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PasswordInputDialogServiceProvider
{
    public class PasswordInputDialogService : INotifyPropertyChanged, IModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool Initialized { get; set; } = false;

        private string _LabelName;
        public string LabelName
        {
            get { return _LabelName; }
            set
            {
                if (value != _LabelName)
                {
                    _LabelName = value;
                    RaisePropertyChanged(nameof(LabelName));
                }
            }
        }

        private string _PositiveLabelName;
        public string PositiveLabelName
        {
            get { return _PositiveLabelName; }
            set
            {
                if (value != _PositiveLabelName)
                {
                    _PositiveLabelName = value;
                    RaisePropertyChanged(nameof(PositiveLabelName));
                }
            }
        }

        private string _NegativeLabelName;
        public string NegetiveLabelName
        {
            get { return _NegativeLabelName; }
            set
            {
                if (value != _NegativeLabelName)
                {
                    _NegativeLabelName = value;
                    RaisePropertyChanged(nameof(NegetiveLabelName));
                }
            }
        }

        private string _InputData;
        public string InputData
        {
            get { return _InputData; }
            set
            {
                if (value != _InputData)
                {
                    _InputData = value;
                    RaisePropertyChanged(nameof(InputData));
                }
            }
        }

        private PasswordInputDialog _passwordInputDialog;

        public PasswordInputDialog passwordInputDialog
        {
            get { return _passwordInputDialog; }
            set { _passwordInputDialog = value; }
        }

        EnumMessageDialogResult dialogRet = EnumMessageDialogResult.NEGATIVE;

        public EnumMessageDialogResult GetDialogResult()
        {
            return dialogRet;
        }

        public void SetDialogResult(EnumMessageDialogResult result)
        {
            try
            {
                dialogRet = result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        passwordInputDialog = new PasswordInputDialog();
                        passwordInputDialog.DataContext = this;
                    });

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                retval = EventCodeEnum.UNDEFINED;
            }

            return retval;
        }

        public CustomDialog GetDialog()
        {
            CustomDialog retval = null;

            try
            {
                retval = passwordInputDialog;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void SetData(string Label, string posBtnLabel, string negBtnLabel, ICommand cmdAffirmative, ICommand cmdNegative)
        {
            try
            {
                InputData = string.Empty;
                passwordInputDialog.pswdBox.Password = string.Empty;

                this.LabelName = Label;
                this.PositiveLabelName = posBtnLabel;
                this.NegetiveLabelName = negBtnLabel;

                this.cmdAffirmativeButtonClick = cmdAffirmative;
                this.cmdNegativeButtonClick = cmdNegative;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void DeInitModule()
        {
        }

        //public async Task<EnumMessageDialogResult> ShowDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel")
        //{
        //    dialogRet = EnumMessageDialogResult.NEGATIVE;
        //    InputData = string.Empty;
        //    passwordInputDialog.pswdBox.Password = string.Empty;

        //    try
        //    {
        //        this.LabelName = Label;
        //        this.PositiveLabelName = posBtnLabel;
        //        this.NegetiveLabelName = negBtnLabel;

        //        await this.MetroDialogManager().ShowWindow(passwordInputDialog);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return dialogRet;
        //}

        private ICommand _cmdAffirmativeButtonClick;
        public ICommand cmdAffirmativeButtonClick
        {
            get { return _cmdAffirmativeButtonClick; }
            set
            {
                if (value != _cmdAffirmativeButtonClick)
                {
                    _cmdAffirmativeButtonClick = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _cmdNegativeButtonClick;
        public ICommand cmdNegativeButtonClick
        {
            get { return _cmdNegativeButtonClick; }
            set
            {
                if (value != _cmdNegativeButtonClick)
                {
                    _cmdNegativeButtonClick = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private AsyncCommand<object> _cmdAffirmativeButtonClick;
        //public ICommand cmdAffirmativeButtonClick
        //{
        //    get
        //    {
        //        if (null == _cmdAffirmativeButtonClick) _cmdAffirmativeButtonClick= new AsyncCommand<object>(AffirmativeButtonClick);
        //        return _cmdAffirmativeButtonClick;
        //    }
        //}

        //private async void AffirmativeButtonClick(object obj)
        //{
        //    try
        //    {
        //        var passwordBox = obj as PasswordBox;
        //        var password = passwordBox?.Password;
        //        this.InputData = password;

        //        dialogRet = EnumMessageDialogResult.AFFIRMATIVE;
        //        await this.MetroDialogManager().CloseWindow(passwordInputDialog);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        //private AsyncCommand _cmdNegativeButtonClick;
        //public ICommand cmdNegativeButtonClick
        //{
        //    get
        //    {
        //        if (null == _cmdNegativeButtonClick) _cmdNegativeButtonClick = new AsyncCommand(NegativeButtonClick);
        //        return _cmdNegativeButtonClick;
        //    }
        //}

        //private async Task NegativeButtonClick()
        //{
        //    try
        //    {
        //        dialogRet = EnumMessageDialogResult.NEGATIVE;
        //        await this.MetroDialogManager().CloseWindow(passwordInputDialog);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public string GetInputData()
        {
            return passwordInputDialog.pswdBox.Password;
        }
    }

}
