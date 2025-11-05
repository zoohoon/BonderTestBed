using LiveCharts.Wpf;
using LogModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Windows.Shapes;

namespace E84SimulatorDialog
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class E84SimulatorDialogView : Window
    {
        private E84SimulatorDialogViewModel vm { get; set; }
        public E84SimulatorDialogView()
        {
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //this.ResizeMode = ResizeMode.NoResize;
            //this.WindowState = WindowState.Maximized;

            InitializeComponent();

            vm = new E84SimulatorDialogViewModel();
            vm.Init();

            this.DataContext = vm;

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (vm != null)
                {
                    vm.Deinit();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Prevent the deletion action
                e.Handled = true;
            }
        }

        private void ChartListView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ListView listView = sender as ListView;
                ScrollViewer sv = FindChild<ScrollViewer>(listView);

                if (sv != null)
                {
                    ListViewHorizontalScrollBar.Maximum = sv.ScrollableWidth;
                    ListViewHorizontalScrollBar.Value = sv.HorizontalOffset;
                    ListViewHorizontalScrollBar.ViewportSize = sv.ViewportWidth;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public static T FindChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                    return (T)child;

                T childItem = FindChild<T>(child);
                if (childItem != null) return childItem;
            }
            return null;
        }

        private void ListViewHorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            ScrollViewer sv = FindChild<ScrollViewer>(ChartListView);

            if (sv != null)
            {
                sv.ScrollToHorizontalOffset(e.NewValue);
            }
        }
    }

    public class DataGridBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// ShowRowNumber
        /// </summary>
        public bool ShowRowNumber { get; set; }

        protected override void OnAttached()
        {
            if (ShowRowNumber)
            {
                AssociatedObject.LoadingRow += AssociatedObject_LoadingRow;
                AssociatedObject.UnloadingRow += AssociatedObject_UnloadingRow;
                AssociatedObject.RowHeaderTemplate = CreateRowHeaderTemplate();

                AssociatedObject.RowHeaderWidth = 40;
            }
        }
        /// <summary>
        /// 로우 언로딩 이벤트 - 삭제시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshRowNumber();
        }
        /// <summary>
        /// 전체 번호 다시 출력
        /// </summary>
        private void RefreshRowNumber()
        {
            int index = 1;

            foreach (var item in AssociatedObject.Items)
            {
                var row = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    UpdateRowHeaderContent(row, index);
                    index++;
                }
            }
        }
        /// <summary>
        /// Row 로딩 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            UpdateRowHeaderContent(e.Row);
        }

        protected override void OnDetaching()
        {
            if (ShowRowNumber)
            {
                AssociatedObject.LoadingRow -= AssociatedObject_LoadingRow;
                AssociatedObject.UnloadingRow -= AssociatedObject_UnloadingRow;
            }
        }

        private void UpdateRowHeaderContent(DataGridRow row, int? index = null)
        {
            if (index == null)
            {
                index = row.GetIndex() + 1;
            }
            row.Header = index;
        }

        private DataTemplate CreateRowHeaderTemplate()
        {
            var template = new DataTemplate();
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("."));
            template.VisualTree = textBlockFactory;
            return template;
        }
    }

    public class EventToCommandBehavior : Behavior<UIElement>
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommandBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseWheel += OnEventFired;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= OnEventFired;
        }

        private void OnEventFired(object sender, MouseWheelEventArgs e)
        {
            if (Command != null && Command.CanExecute(e))
            {
                Command.Execute(e);
            }
        }
    }

}
