using SecsGemServiceInterface;
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
using System.Windows.Shapes;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    /// <summary>
    /// CommandSelectWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommandSelectWindow : Window
    {
        public string SelectedCommand { get; private set; }

        public CommandSelectWindow()
        {
            InitializeComponent();

            // EnumCarrierAction과 EnumRemoteCommand의 모든 값을 가져와서
            // 문자열 리스트로 변환합니다. 
            var combinedCommands = Enum.GetValues(typeof(EnumCarrierAction))
                                        .Cast<EnumCarrierAction>()
                                        .Select(e => e.ToString())
                                        .Union(Enum.GetValues(typeof(EnumRemoteCommand))
                                        .Cast<EnumRemoteCommand>()
                                        .Select(e => e.ToString()))
                                        .OrderBy(e => e) // 선택 사항: 정렬
                                        .ToList();

            // 수정된 부분: 문자열 리스트를 CommandComboBox의 ItemsSource로 설정합니다.
            CommandComboBox.ItemsSource = combinedCommands;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommandComboBox.SelectedValue != null)
            {
                // SelectedValue는 이제 문자열이므로, 캐스팅은 필요 없습니다.
                SelectedCommand = CommandComboBox.SelectedValue.ToString();
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select a command.");
            }
        }
    }
}
