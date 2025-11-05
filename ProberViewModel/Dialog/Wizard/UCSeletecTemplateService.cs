using System;
using System.Threading.Tasks;

namespace SelectTemplateControl
{
    using LogModule;
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces;
    using ProberInterfaces.Wizard;
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    public class UCSeletecTemplateService : INotifyPropertyChanged, IFactoryModule
    {


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private MetroWindow MetroWindow;
        private UCSeleteTemplate SeleteTemplateDialog;
        //private IWizardCategoryVM WizardCategoryVM;

        private ITemplate _HasTemplateModule;
        public ITemplate HasTemplateModule
        {
            get { return _HasTemplateModule; }
            set
            {
                if (value != _HasTemplateModule)
                {
                    _HasTemplateModule = value;
                    NotifyPropertyChanged("HasTemplateModule");
                }
            }
        }

        private string _TemplatePath;
        public string TemplatePath
        {
            get { return _TemplatePath; }
            set
            {
                if (value != _TemplatePath)
                {
                    _TemplatePath = value;
                    NotifyPropertyChanged("TemplatePath");
                }
            }
        }


        private int _SeletedTemplateIndex;
        public int SeletedTemplateIndex
        {
            get { return _SeletedTemplateIndex; }
            set
            {
                if (value != _SeletedTemplateIndex)
                {
                    _SeletedTemplateIndex = value;

                    NotifyPropertyChanged("SeletedTemplateIndex");
                }
            }
        }


        public UCSeletecTemplateService()
        {
            try
            {
                MetroWindow = this.MetroDialogManager().GetMetroWindow() as MetroWindow;
                SeleteTemplateDialog = new UCSeleteTemplate();
                SeleteTemplateDialog.DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public async Task ShowDialog(ITemplate module)
        {
            try
            {
                HasTemplateModule = module;
                TemplatePath = module.TemplateParameter.Param.BasePath;
                SeletedTemplateIndex = -1;

                await Application.Current.Dispatcher.Invoke((async () =>
                {
                    await MetroWindow.ShowMetroDialogAsync(SeleteTemplateDialog);
                    await SeleteTemplateDialog.WaitUntilUnloadedAsync();
                }));

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private AsyncCommand<object> _TemplateChangedCommand;
        public ICommand TemplateChangedCommand
        {
            get
            {
                if (null == _TemplateChangedCommand) _TemplateChangedCommand
                        = new AsyncCommand<object>(TemplateChanged);
                return _TemplateChangedCommand;
            }
        }

        private async Task TemplateChanged(object param)
        {
            try
            {


                //EventCodeEnum retVal = HasTemplateModule.CheckTemplate(false,(int)param);
                //if (retVal == EventCodeEnum.NONE)
                //{
                //    TemplatePath = TemplatePath + "\\" + HasTemplateModule.TemplateParam.Param.TemplateInfos[(int)param].Name;
                //} 
                //else
                //{

                //        MessageDialogResult ret = await MetroWindow.ShowMessageAsync(
                //                  "Template Load Failed."
                //                  , "Failed to load DLL of selected Template.Please update DLL first and try again.");

                //    //SeletedTemplateIndex = -1;


                //}
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "TemplateChanged() : Error occured.");
                LoggerManager.Exception(err);

            }
        }


        private AsyncCommand _cmdAffirmativeButtonClick;
        public ICommand cmdAffirmativeButtonClick
        {
            get
            {
                if (null == _cmdAffirmativeButtonClick) _cmdAffirmativeButtonClick
                        = new AsyncCommand(AffirmativeButtonClick);
                return _cmdAffirmativeButtonClick;
            }
        }

        private async Task AffirmativeButtonClick()
        {
            try
            {
                //if(SeletedTemplateIndex != -1)
                //{
                //    EventCodeEnum retVal = HasTemplateModule.CheckTemplate(true, SeletedTemplateIndex);
                //    if (retVal == EventCodeEnum.NONE)
                //    {
                //        TemplatePath = TemplatePath + "\\" + HasTemplateModule.TemplateParam.Param.TemplateInfos[SeletedTemplateIndex].Name;
                //        HasTemplateModule.SetTemplate(SeletedTemplateIndex);
                //    }
                //    await MetroWindow.HideMetroDialogAsync(SeleteTemplateDialog);

                //}
                //else
                //{
                //    //MessageBox 선택해 주십시오.
                //}



            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "AffirmativeButtonClick() : Error occured.");
                LoggerManager.Exception(err);

            }
        }

        private AsyncCommand _cmdNegativeButtonClick;
        public ICommand cmdNegativeButtonClick
        {
            get
            {
                if (null == _cmdNegativeButtonClick) _cmdNegativeButtonClick
                        = new AsyncCommand(NegativeButtonClick);
                return _cmdNegativeButtonClick;
            }
        }

        private async Task NegativeButtonClick()
        {
            try
            {
                await MetroWindow.HideMetroDialogAsync(SeleteTemplateDialog);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err + "NegativeButtonClick() : Error occured.");
                LoggerManager.Exception(err);
            }

        }

    }
}
