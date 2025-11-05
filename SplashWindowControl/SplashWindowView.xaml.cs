using SplashWindowViewModel;
using System;
using System.Windows;

namespace SplashWindowControl
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SplashWindowView : Window
    {
        SplashWindowVM vm = new SplashWindowVM();

        public SplashWindowView(string version)
        {
            InitializeComponent();

            vm.Version = version;

            DataContext = vm;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            vm.IsStoryboardCompleted = true;
        }

        public void StartDBUpdate()
        {
            vm.IsDBUpdate = "Updating DB Parameters";
        }

        public void StopDbUpdate()
        {
            vm.IsDBUpdate = string.Empty;
        }
    }
}