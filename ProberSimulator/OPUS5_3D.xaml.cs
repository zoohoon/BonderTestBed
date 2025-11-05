using LogModule;
using ProberInterfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ProberSimulator
{
    /// <summary>
    /// OPUS5_3D.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OPUS5_3D : UserControl, INotifyPropertyChanged, IStage3DModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public OPUS5_3D()
        {
            try
            {
                InitializeComponent();
                //this.DataContext = this;
                // Add to view port 
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //StringBuilder eventstr = new StringBuilder();

        //void WaferCamMiddleValueCheck(object sender, RoutedEventArgs args)
        //{
        //    BoolAni.RaiseWaferCamMiddleEvent();

        //}

        private Point3D _OldCamPosition;
        public Point3D CamPosition
        {
            get { return (Point3D)this.GetValue(CamPositionProperty); }
            set
            {
                this.SetValue(CamPositionProperty, value);
            }
        }
        public static readonly DependencyProperty CamPositionProperty =
                        DependencyProperty.Register(nameof(CamPosition),
                        typeof(Point3D), typeof(OPUS5_3D),
                        new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCamPositionChanged)));

        private static void OnCamPositionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                OPUS5_3D test = sender as OPUS5_3D;
                test.ChangeCamPosition(sender, e);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ChangeCamPosition(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Point3D potion;
                if (e.NewValue is Point3D)
                {
                    potion = (Point3D)e.NewValue;

                    Point3DAnimation pa = new Point3DAnimation(new Point3D(this._OldCamPosition.X, this._OldCamPosition.Y, this._OldCamPosition.Z), new Point3D(potion.X, potion.Y, potion.Z), new Duration(new TimeSpan(0, 0, 1)));
                    ViewCamera.BeginAnimation(PerspectiveCamera.PositionProperty, pa);

                    _OldCamPosition = potion;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        private Vector3D _OldCamLookDirection;

        public Vector3D CamLookDirection
        {
            get { return (Vector3D)this.GetValue(CamLookDirectionProperty); }
            set
            {
                this.SetValue(CamLookDirectionProperty, value);
            }
        }
        public static readonly DependencyProperty CamLookDirectionProperty =
                                DependencyProperty.Register(nameof(CamLookDirection),
                                                            typeof(Vector3D),
                                                            typeof(OPUS5_3D), new FrameworkPropertyMetadata(new PropertyChangedCallback(CamLookDirectionChanged)));
        //new FrameworkPropertyMetadata(null, OnCurrentReadingChanged));
        private static void CamLookDirectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            OPUS5_3D test = sender as OPUS5_3D;
            test.ChangeCamLookDirection(sender, e);
        }

        private void ChangeCamLookDirection(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Vector3D vector;
                if (e.NewValue is Vector3D)
                {
                    vector = (Vector3D)e.NewValue;

                    Vector3DAnimation va = new Vector3DAnimation(new Vector3D(this._OldCamLookDirection.X, this._OldCamLookDirection.Y, this._OldCamLookDirection.Z), new Vector3D(vector.X, vector.Y, vector.Z), new Duration(new TimeSpan(0, 0, 1)));
                    ViewCamera.BeginAnimation(PerspectiveCamera.LookDirectionProperty, va);

                    _OldCamLookDirection = vector;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private Vector3D _OldCamUpDirection;
        public Vector3D CamUpDirection
        {
            get { return (Vector3D)this.GetValue(CamUpDirectionProperty); }
            set
            {
                this.SetValue(CamUpDirectionProperty, value);
            }
        }
        public static readonly DependencyProperty CamUpDirectionProperty =
                                DependencyProperty.Register(nameof(CamUpDirection),
                                typeof(Vector3D), typeof(OPUS5_3D),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(CamUpDirectionChanged)));
        //new FrameworkPropertyMetadata(null, OnCurrentReadingChanged));

        private static void CamUpDirectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                OPUS5_3D test = sender as OPUS5_3D;
                test.ChangeCamUpDirection(sender, e);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ChangeCamUpDirection(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Vector3D vector;
                if (e.NewValue is Vector3D)
                {
                    vector = (Vector3D)e.NewValue;

                    Vector3DAnimation va = new Vector3DAnimation(new Vector3D(this._OldCamUpDirection.X, this._OldCamUpDirection.Y, this._OldCamUpDirection.Z), new Vector3D(vector.X, vector.Y, vector.Z), new Duration(new TimeSpan(0, 0, 1)));
                    ViewCamera.BeginAnimation(PerspectiveCamera.UpDirectionProperty, va);

                    _OldCamUpDirection = vector;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
