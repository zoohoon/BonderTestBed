using ProberInterfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UCTaskManagement
{
    /// <summary>
    /// UCTaskManagement.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCTaskManagement : UserControl, IMainScreenView
    {
        readonly Guid _ViewGUID = new Guid("E063E8F5-EE65-8FB5-0CA3-F3FBD7B51B78");
        public Guid ScreenGUID { get { return _ViewGUID; } }

        const double ScaleRate = 1.1;

        public Canvas canvas = new Canvas();
        ScaleTransform scaletransform = new ScaleTransform();

        public UCTaskManagement()
        {
            InitializeComponent();
        }

        //public TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(obj, i);

        //        if (child != null && child is TChildItem)
        //            return (TChildItem)child;

        //        var childOfChild = FindVisualChild<TChildItem>(child);

        //        if (childOfChild != null)
        //            return childOfChild;
        //    }

        //    return null;
        //}

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) //Note that this is a lie, this does not check for a "real" click
            {
                var label = (Label)sender;

                if (label.BorderThickness != null)
                {
                    if (label.BorderThickness.Left == 0)
                    {
                        label.BorderThickness = new Thickness(5);
                    }
                    else
                    {
                        label.BorderThickness = new Thickness(0);
                    }
                }
            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    //    if (e.Delta > 0)
        //    //    {
        //    //        st.ScaleX *= ScaleRate;
        //    //        st.ScaleY *= ScaleRate;
        //    //    }
        //    //    else
        //    //    {
        //    //        st.ScaleX /= ScaleRate;
        //    //        st.ScaleY /= ScaleRate;
        //    //    }
        //    //}
        //}
    }
}


