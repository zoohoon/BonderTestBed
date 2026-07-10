using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows; // Window 상속을 위해 필요
using System.Windows.Forms; // 참조 추가 필요 (System.Windows.Forms)

namespace ManualJogView
{
    /// <summary>
    /// Jog6AxisWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Jog6AxisWindow : Window
    {
        public Jog6AxisWindow()
        {
            InitializeComponent();
            // 1. 항상 맨 위 설정
            this.Topmost = true;

            // 2. 창 이동 가능하게 설정 (TitleBar가 없으므로)
            this.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) this.DragMove(); };

            // 3. 로드 시 위치 설정
            this.Loaded += Jog6AxisWindow_Loaded;
            this.WindowStartupLocation = WindowStartupLocation.Manual;

            // 창 이동 기능 (드래그)
            this.MouseLeftButtonDown += (s, e) => {
                if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
            };
        }
        private void Jog6AxisWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 두 번째 모니터가 있는지 확인
            Screen[] screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                // 두 번째 모니터 (Index 1) 선택
                Screen secondMonitor = screens[0];

                // 원하는 좌표 (예: 두 번째 모니터 시작점 + 100, 100)
                // 좌표는 모니터의 WorkingArea 기준
                this.Left = secondMonitor.WorkingArea.Left + 0;
                this.Top = secondMonitor.WorkingArea.Top + 0;
            }
            else
            {
                // 모니터가 하나면 그냥 메인 화면에 띄움
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }
}
