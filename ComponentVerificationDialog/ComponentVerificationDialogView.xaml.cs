using LogModule;
using System;
using System.Reflection;
using System.Windows;

namespace ComponentVerificationDialog
{
    public partial class ComponentVerificationDialogView : Window
    {
        public ComponentVerificationDialogView()
        {
            InitializeComponent();

            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            Version assemblyVersion = assemblyName.Version;
            Title = $"Component Verification (ver {assemblyVersion.ToString()})";
        }

        private static ComponentVerificationDialogView componentVerificationDialogView;

        public static ComponentVerificationDialogView GetInstance()
        {
            if (componentVerificationDialogView == null)
            {
                componentVerificationDialogView = new ComponentVerificationDialogView();
            }
            return componentVerificationDialogView;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;

                // ViewModel에 Dialog Close 상태 Update
                if (CompVerifyUC.DataContext is ComponentVerificationDialogViewModel vm)
                {
                    vm.UpdateDialogOpenCloseState(false);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                // ViewModel에 Dialog Open 상태 Update
                if (CompVerifyUC.DataContext is ComponentVerificationDialogViewModel vm)
                {
                    vm.UpdateDialogOpenCloseState(true);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
