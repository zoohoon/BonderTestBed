using ExtraCameraDialogVM;
using LogModule;
using System;
using System.Windows;

namespace ExtraCameraMainDialog
{
    public partial class ExtraCameraDialog : Window
    {
        ExtraCameraDialogViewModel ViewModel = null;

        public ExtraCameraDialog()
        {
            try
            {
                InitializeComponent();
                ViewModel = new ExtraCameraDialogViewModel();
                this.DataContext = ViewModel;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool HasCamera()
        {
            bool retVal = false;
            try
            {
                retVal = ViewModel.HasCamera();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ViewModel.Dispose();
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
