using LogModule;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroDialogInterfaces;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.DialogControl;
using ProberInterfaces.Enum;
using RelayCommandBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SingleInputDialogServiceProvider
{
    public class SingleInputDialogService : INotifyPropertyChanged, ISingleInputDialogService, IModule
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        //public event SingleInputDialogShowEventHandler SingleInputDialogShow;
        //public event SingleInputGetInputDataEventHandler SingleInputGetInputData;


        //public event SingleInputDialogCloseEventHandler SingleInputDialogClose;

        public bool Initialized { get; set; } = false;

        private string _SubLabel;
        public string SubLabel
        {
            get { return _SubLabel; }
            set
            {
                if (value != _SubLabel)
                {
                    _SubLabel = value;
                    RaisePropertyChanged(nameof(SubLabel));
                }
            }
        }

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


        //private MetroWindow MetroWindow;
        //private SingleInputDialog singleInputDialog;

        private SingleInputDialog _singleInputDialog;

        public SingleInputDialog singleInputDialog
        {
            get { return _singleInputDialog; }
            set { _singleInputDialog = value; }
        }


        EnumMessageDialogResult dialogRet = EnumMessageDialogResult.NEGATIVE;

        public EnumMessageDialogResult GetDialogResult()
        {
            return dialogRet;
        }

        public CustomDialog GetDialog()
        {
            CustomDialog retval = null;

            try
            {
                retval = singleInputDialog;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
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


        public void SetData(string label, string posBtnLabel, string negBtnLabel, ICommand cmdAffirmative, ICommand cmdNegative, string sublabel)
        {
            try
            {
                this.LabelName = label;

                this.PositiveLabelName = posBtnLabel;
                this.NegetiveLabelName = negBtnLabel;

                this.InputData = string.Empty;

                this.cmdAffirmativeButtonClick = cmdAffirmative;
                this.cmdNegativeButtonClick = cmdNegative;

                this.SubLabel = sublabel;
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
                        singleInputDialog = new SingleInputDialog();
                        singleInputDialog.DataContext = this;
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

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        //public async Task<EnumMessageDialogResult> ShowDialog(string Label = "Input", string posBtnLabel = "OK", string negBtnLabel = "Cancel")
        //{
        //    dialogRet = EnumMessageDialogResult.NEGATIVE;
        //    InputData = string.Empty;

        //    try
        //    {
        //        this.LabelName = Label;
        //        this.PositiveLabelName = posBtnLabel;
        //        this.NegetiveLabelName = negBtnLabel;

        //        if (SingleInputDialogShow != null & (this.StageCommunicationManager()?.IsEnableDialogProxy() == true) &&
        //            this.LoaderController().GetconnectFlag() == true)
        //        {
        //            dialogRet = await SingleInputDialogShow(Label, posBtnLabel, negBtnLabel);
        //            return dialogRet;
        //        }

        //        await this.MetroDialogManager().ShowWindow(singleInputDialog);

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

        //private async Task AffirmativeButtonClick()
        //{
        //    try
        //    {
        //        dialogRet = EnumMessageDialogResult.AFFIRMATIVE;
        //        this.MetroDialogManager().CloseWindow(singleInputDialog);
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
        //        this.MetroDialogManager().CloseWindow(singleInputDialog);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }
        //}

        public string GetInputData()
        {
            try
            {
                //if (SingleInputGetInputData != null & (this.StageCommunicationManager()?.IsEnableDialogProxy() == true) &&
                //    this.LoaderController().GetconnectFlag() == true)
                //{
                //    InputData = SingleInputGetInputData();
                //    return InputData;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return InputData;
        }
    }
}
