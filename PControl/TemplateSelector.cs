using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using ProberInterfaces.NeedleClean;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using LogModule;

namespace PnPControl
{
    public class TemplateSelector : DataTemplateSelector, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private DataTemplate _MapViewDataTemplate;

        public DataTemplate MapViewDataTemplate
        {
            get { return _MapViewDataTemplate; }
            set
            {
                _MapViewDataTemplate = value;
                RaisePropertyChanged();
            }
        }

        private DataTemplate _DisplayViewDataTemplate;

        public DataTemplate DisplayViewDataTemplate
        {
            get { return _DisplayViewDataTemplate; }
            set
            {
                _DisplayViewDataTemplate = value;
                RaisePropertyChanged();
            }
        }


        private DataTemplate _DutViewDataTemplate;

        public DataTemplate DutViewDataTemplate
        {
            get { return _DutViewDataTemplate; }
            set { _DutViewDataTemplate = value; RaisePropertyChanged(); }
        }

        private DataTemplate _NCDataTemplate;

        public DataTemplate NCDataTemplate
        {
            get { return _NCDataTemplate; }
            set { _NCDataTemplate = value; RaisePropertyChanged(); }
        }

        public DataTemplate _NeedleCleanDataTemplate;
        public DataTemplate NeedleCleanDataTemplate
        {
            get { return _NeedleCleanDataTemplate; }
            set { _NeedleCleanDataTemplate = value; RaisePropertyChanged(); }
        }




        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            #region Require

            //if (item == null) throw new ArgumentNullException();
            if (container == null) throw new ArgumentNullException();

            #endregion

            // Result
            DataTemplate itemDataTemplate = null;
            try
            {
                // Base DataTemplate
                itemDataTemplate = base.SelectTemplate(item, container);
                if (itemDataTemplate != null) return itemDataTemplate;

                // Interface DataTemplate
                FrameworkElement itemContainer = container as FrameworkElement;
                if (itemContainer == null) return null;


                if (item is IWaferObject)
                {
                    return MapViewDataTemplate;
                }
                else if (item is IProbeCard)
                {
                    return DutViewDataTemplate;
                }
                else if (item is ICamera)
                {
                    return DisplayViewDataTemplate;
                }
                else if (item is INeedleCleanViewModel)
                {
                    return NeedleCleanDataTemplate;
                }
                else if (item is INeedleCleanObject)
                {
                    return NCDataTemplate;
                }



                // Return
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return itemDataTemplate;
        }

    }
}
