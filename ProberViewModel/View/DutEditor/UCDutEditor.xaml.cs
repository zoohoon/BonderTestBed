using LogModule;
using ProberInterfaces;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace UCDutEditor
{
    /// <summary>
    /// UCDutEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCDutEditor : UserControl , IMainScreenView
    {
        public UCDutEditor()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        readonly Guid _ViewGUID = new Guid("78744426-ef1d-4624-a961-4a756669a9b7");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        private void MapViewControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}

