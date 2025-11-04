using LogModule;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ColorPaletteControl
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int _GridColumnCount = 4;
        private const int _GridRowCount = 4;
        public Brush SelectBrush { get; set; }
        private Dictionary<int, Brush> _ColorBrushList;
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(Dictionary<int, Brush> colorBrushList) : this()
        {
            try
            {
                _ColorBrushList = colorBrushList;
                MakeUI();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MakeUI()
        {
            try
            {
                for (int i = 0; i < _GridRowCount; i++)
                    paletteGrid.RowDefinitions.Add(new RowDefinition());
                for (int i = 0; i < _GridColumnCount; i++)
                    paletteGrid.ColumnDefinitions.Add(new ColumnDefinition());

                int idx = 0;
                for (int row = 0; row < _GridRowCount; row++)
                {
                    if (idx >= _ColorBrushList.Count)
                        break;
                    for (int col = 0; col < _GridColumnCount; col++)
                    {
                        if (idx >= _ColorBrushList.Count)
                            break;

                        Rectangle colorRect = new Rectangle();
                        colorRect.PreviewMouseDown += Rectangle_PreviewMouseDown;

                        Grid.SetRow(colorRect, row);
                        Grid.SetColumn(colorRect, col);

                        colorRect.Stroke = Brushes.Gray;
                        colorRect.Fill = _ColorBrushList[idx];
                        paletteGrid.Children.Add(colorRect);
                        idx++;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void Rectangle_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Rectangle rect = sender as Rectangle;
                if (rect == null)
                    return;
                SelectBrush = rect.Fill;
                Close();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
