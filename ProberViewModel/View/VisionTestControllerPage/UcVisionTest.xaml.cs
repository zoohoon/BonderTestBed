using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace VisionTestView
{
    /// <summary>
    /// UcVisionTest.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcVisionTest : UserControl, IMainScreenView, IFactoryModule
    {
        public UcVisionTest()
        {
            try
            {
                InitializeComponent();

                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    if (this.VisionManager().CameraDescriptor != null)
                //    {

                //        foreach (var item in this.VisionManager().CameraDescriptor.Cams)
                //        {
                //            this.VisionManager().SetDisplayChannel(item, Display);
                //        }

                //    }
                //});
            }
            catch (Exception err)
            {
                throw;
            }
        }

        readonly Guid _ViewGUID = new Guid("367FF140-F606-B65B-7C17-1A743AD06ED6");
        public Guid ScreenGUID { get { return _ViewGUID; } }
       
    }

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

                if (item is ICamera)
                {
                    return DisplayViewDataTemplate;
                }

            }
            catch (Exception err)
            {
                throw err;
            }
            return itemDataTemplate;
        }

    }
}
