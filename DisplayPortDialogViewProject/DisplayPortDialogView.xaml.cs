using LogModule;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UcDisplayPortDialogView
{
    public partial class DisplayPortDialogView : MahApps.Metro.Controls.Dialogs.CustomDialog
    {
        public DisplayPortDialogView()
        {
            InitializeComponent();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private static DisplayPortDialogView View;

        public static DisplayPortDialogView GetInstance()
        {
            if (View == null)
                try
                {
                    {
                        View = new DisplayPortDialogView();
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                    throw;
                }
            return View;
        }
    }
}
