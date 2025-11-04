using LogModule;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace UcCircularShapedJog
{
    /// <summary>
    /// SelectBtn.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectBtn : UserControl
    {
        public SelectBtn()
        {
            InitializeComponent();
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mainGrid.Opacity = 0.5;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void mainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mainGrid.Opacity = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                mainGrid.Opacity = 1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }
}
