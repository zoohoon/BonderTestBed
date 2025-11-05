using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Viewer3DModel
{
    /// <summary>
    /// Viewer3DModelV.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Viewer3DModelV : UserControl, INotifyPropertyChanged, IStage3DModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static Viewer3DModelV Viewer3Dobj;
        //Viewer3DViewModel viewer3Dvm = null;

        public Viewer3DModelV()
        {
            try
            {
                InitializeComponent();

                Viewer3DModelV.Viewer3Dobj = this;
                //this.DataContext = this;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private Point3D _CamPosition = new Point3D(0, 785.84, 1255.72);
        public Point3D CamPosition
        {
            get { return _CamPosition; }
            set
            {
                if (value != _CamPosition)
                {
                    _CamPosition = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Vector3D _CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
        public Vector3D CamLookDirection
        {
            get { return _CamLookDirection; }
            set
            {
                if (value != _CamLookDirection)
                {
                    _CamLookDirection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Vector3D _CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
        public Vector3D CamUpDirection
        {
            get { return _CamUpDirection; }
            set
            {
                if (value != _CamUpDirection)
                {
                    _CamUpDirection = value;
                    RaisePropertyChanged();
                }
            }
        }


        //public CameraViewPositionType CameraViewPositionType
        //{
        //    get { return (CameraViewPositionType)GetValue(CameraViewPositionTypeProperty); }
        //    set
        //    {
        //        SetValue(CameraViewPositionTypeProperty, value);
        //    }
        //}
        //public static readonly DependencyProperty CameraViewPositionTypeProperty =
        //                DependencyProperty.Register(nameof(CameraViewPositionType),
        //                typeof(CameraViewPositionType), typeof(Viewer3DModelV),
        //                new FrameworkPropertyMetadata(new PropertyChangedCallback(CameraViewPositionTypeChanged)));

        //private static void CameraViewPositionTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    try
        //    {
        //        //Viewer3DModelV test = sender as Viewer3DModelV;
        //        //Viewer3DViewModel vm = test.DataContext as Viewer3DViewModel;

        //        Viewer3Dobj?.CameraViewPosition(sender, e);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //public void CameraViewPosition(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    try
        //    {
        //        CameraViewPositionType position;
        //        if (e.NewValue is CameraViewPositionType)
        //        {
        //            position = (CameraViewPositionType)e.NewValue;

        //            switch (position)
        //            {
        //                case CameraViewPositionType.VIEW1:
        //                    View1Change();
        //                    break;
        //                case CameraViewPositionType.VIEW2:
        //                    View2Change();
        //                    break;
        //                case CameraViewPositionType.VIEW3:
        //                    View3Change();
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        //public void View1Change()
        //{
        //    try
        //    {
        //        CamPosition = new Point3D(10, 785.84, 1255.72);
        //        CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
        //        CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        //public void View2Change()
        //{
        //    try
        //    {
        //        CamPosition = new Point3D(50, 785.84, 1255.72);
        //        CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
        //        CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}
        //public void View3Change()
        //{
        //    try
        //    {
        //        CamPosition = new Point3D(100, 785.84, 1255.72);
        //        CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
        //        CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}



    //private void V1(object sender, RoutedEventArgs e)
    //{
    //    CamPosition = new Point3D(1878.06, 785.84, 1255.72);
    //    CamLookDirection = new Vector3D(-0.76, -0.33, -0.57);
    //    CamUpDirection = new Vector3D(-0.26, 0.945, -0.2);
    //}

    //private void V2(object sender, RoutedEventArgs e)
    //{
    //    CamPosition = new Point3D(2194.0483210074, 2359.44116228299, 2172.89179474712);
    //    CamLookDirection = new Vector3D(-0.540324542809065, -0.614077908017054, -0.575289241446052);
    //    CamUpDirection = new Vector3D(-0.420403285444348, 0.78924541359795, -0.447607813495474);
    //}
}
}
