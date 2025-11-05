using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RelayCommandBase;

namespace ProberInterfaces
{
    public class SimpleDeviceInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    [Serializable, DataContract]
    public class DutViewControlVM : IDutViewControlVM, INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Task DutAddbyMouseDown()
        {
            throw new NotImplementedException();
        }

        [NonSerialized, JsonIgnore]
        private IStageSupervisor _StageSupervisor;
        [XmlIgnore, IgnoreDataMember]
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged(nameof(StageSupervisor));
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private IMotionManager _MotionManager;
        [XmlIgnore, IgnoreDataMember]
        public IMotionManager MotionManager
        {
            get { return _MotionManager; }
            set
            {
                if (value != _MotionManager)
                {
                    _MotionManager = value;
                    RaisePropertyChanged(nameof(MotionManager));
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private IProbeCard _ProbeCard;
        [XmlIgnore, IgnoreDataMember]
        public IProbeCard ProbeCard
        {
            get { return _ProbeCard; }
            set
            {
                if (value != _ProbeCard)
                {
                    _ProbeCard = value;
                    RaisePropertyChanged(nameof(ProbeCard));
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private IWaferObject _WaferObject;
        [XmlIgnore, IgnoreDataMember]
        public IWaferObject WaferObject
        {
            get { return _WaferObject; }
            set
            {
                if (value != _WaferObject)
                {
                    _WaferObject = value;
                    RaisePropertyChanged(nameof(WaferObject));
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private IVisionManager _VisionManager;
        [XmlIgnore, IgnoreDataMember]
        public IVisionManager VisionManager
        {
            get { return _VisionManager; }
            set
            {
                if (value != _VisionManager)
                {
                    _VisionManager = value;
                    RaisePropertyChanged(nameof(VisionManager));
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private ICamera _CurCam;
        [XmlIgnore, IgnoreDataMember]
        public ICamera CurCam
        {
            get { return _CurCam; }
            set
            {
                if (value != _CurCam)
                {
                    _CurCam = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private Visibility _VisibilityZoomIn;
        [XmlIgnore, IgnoreDataMember]
        public Visibility VisibilityZoomIn
        {
            get { return _VisibilityZoomIn; }
            set
            {
                if (value != _VisibilityZoomIn)
                {
                    _VisibilityZoomIn = value;
                    RaisePropertyChanged();
                }
            }
        }

        [NonSerialized, JsonIgnore]
        private Visibility _VisibilityZoomOut;
        [XmlIgnore, IgnoreDataMember]
        public Visibility VisibilityZoomOut
        {
            get { return _VisibilityZoomOut; }
            set
            {
                if (value != _VisibilityZoomOut)
                {
                    _VisibilityZoomOut = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        [NonSerialized, JsonIgnore]
        private Visibility _VisibilityMoveToCenter;
        [XmlIgnore, IgnoreDataMember]
        public Visibility VisibilityMoveToCenter
        {
            get { return _VisibilityMoveToCenter; }
            set
            {
                if (value != _VisibilityMoveToCenter)
                {
                    _VisibilityMoveToCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private EnumProberCam _CamType;
        [DataMember]
        public EnumProberCam CamType
        {
            get { return _CamType; }
            set
            {
                if (value != _CamType)
                {
                    _CamType = value;
                    RaisePropertyChanged(nameof(CamType));
                }
            }
        }

        private double _ZoomLevel;
        [DataMember]
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (value != _ZoomLevel)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged(nameof(ZoomLevel));
                }
            }
        }

        private bool? _AddCheckBoxIsChecked;
        [DataMember]
        public bool? AddCheckBoxIsChecked
        {
            get { return _AddCheckBoxIsChecked; }
            set
            {
                if (value != _AddCheckBoxIsChecked)
                {
                    _AddCheckBoxIsChecked = value;
                    RaisePropertyChanged(nameof(AddCheckBoxIsChecked));
                }
            }
        }

        private bool? _EnableDragMap;
        [DataMember]
        public bool? EnableDragMap
        {
            get { return _EnableDragMap; }
            set
            {
                if (value != _EnableDragMap)
                {
                    _EnableDragMap = value;
                    RaisePropertyChanged(nameof(EnableDragMap));
                }
            }
        }

        private bool? _ShowCurrentPos;
        [DataMember]
        public bool? ShowCurrentPos
        {
            get { return _ShowCurrentPos; }
            set
            {
                if (value != _ShowCurrentPos)
                {
                    _ShowCurrentPos = value;
                    RaisePropertyChanged(nameof(ShowCurrentPos));
                }
            }
        }

        private bool? _ShowGrid;
        [DataMember]
        public bool? ShowGrid
        {
            get { return _ShowGrid; }
            set
            {
                if (value != _ShowGrid)
                {
                    _ShowGrid = value;
                    RaisePropertyChanged(nameof(ShowGrid));
                }
            }
        }

        private bool? _ShowPad;
        [DataMember]
        public bool? ShowPad
        {
            get { return _ShowPad; }
            set
            {
                if (value != _ShowPad)
                {
                    _ShowPad = value;
                    RaisePropertyChanged(nameof(ShowPad));
                }
            }
        }

        private bool? _ShowPin;
        [DataMember]
        public bool? ShowPin
        {
            get { return _ShowPin; }
            set
            {
                if (value != _ShowPin)
                {
                    _ShowPin = value;
                    RaisePropertyChanged(nameof(ShowPin));
                }
            }
        }

        private bool? _ShowSelectedDut;
        [DataMember]
        public bool? ShowSelectedDut
        {
            get { return _ShowSelectedDut; }
            set
            {
                if (value != _ShowSelectedDut)
                {
                    _ShowSelectedDut = value;
                    RaisePropertyChanged(nameof(ShowSelectedDut));
                }
            }
        }

        private bool _IsEnableMoving;
        [DataMember]
        public bool IsEnableMoving
        {
            get { return _IsEnableMoving; }
            set
            {
                if (value != _IsEnableMoving)
                {
                    _IsEnableMoving = value;
                    RaisePropertyChanged(nameof(IsEnableMoving));
                }
            }
        }

        private double _CurXPos;
        [DataMember]
        public double CurXPos
        {
            get { return _CurXPos; }
            set
            {
                if (value != _CurXPos)
                {
                    _CurXPos = value;
                    RaisePropertyChanged(nameof(CurXPos));
                }
            }
        }

        private double _CurYPos;
        [DataMember]
        public double CurYPos
        {
            get { return _CurYPos; }
            set
            {
                if (value != _CurYPos)
                {
                    _CurYPos = value;
                    RaisePropertyChanged(nameof(CurYPos));
                }
            }
        }

        [XmlIgnore, IgnoreDataMember]
        public IAsyncCommand DutAddMouseDownCommand => null;
    }

    [Serializable, DataContract]
    public class DeviceInfoForDeviceChangeControl : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }

    [Serializable, DataContract]
    public class DeviceInfo : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private DutViewControlVM _DutViewControl = new DutViewControlVM();
        [DataMember]
        public DutViewControlVM DutViewControl
        {
            get { return _DutViewControl; }
            set
            {
                if (value != _DutViewControl)
                {
                    _DutViewControl = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private IDutViewControlVM _DutViewControl = new DutViewControlVM();
        //public IDutViewControlVM DutViewControl
        //{
        //    get { return _DutViewControl; }
        //    set
        //    {
        //        if (value != _DutViewControl)
        //        {
        //            _DutViewControl = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        private string _Name;
        [DataMember]
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SetTemp;
        [DataMember]
        public double SetTemp
        {
            get { return _SetTemp; }
            set
            {
                if (value != _SetTemp)
                {
                    _SetTemp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferSize;
        [DataMember]
        public string WaferSize
        {
            get { return _WaferSize; }
            set
            {
                if (value != _WaferSize)
                {
                    _WaferSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _WaferThickness;
        [DataMember]
        public double WaferThickness
        {
            get { return _WaferThickness; }
            set
            {
                if (value != _WaferThickness)
                {
                    _WaferThickness = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _WaferNotchType;
        [DataMember]
        public string WaferNotchType
        {
            get { return _WaferNotchType; }
            set
            {
                if (value != _WaferNotchType)
                {
                    _WaferNotchType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _WaferNotchAngle;
        [DataMember]
        public double WaferNotchAngle
        {
            get { return _WaferNotchAngle; }
            set
            {
                if (value != _WaferNotchAngle)
                {
                    _WaferNotchAngle = value;
                    RaisePropertyChanged();
                }
            }
        }



        private int _WaferMapCountX;
        [DataMember]
        public int WaferMapCountX
        {
            get { return _WaferMapCountX; }
            set
            {
                if (value != _WaferMapCountX)
                {
                    _WaferMapCountX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _WaferMapCountY;
        [DataMember]
        public int WaferMapCountY
        {
            get { return _WaferMapCountY; }
            set
            {
                if (value != _WaferMapCountY)
                {
                    _WaferMapCountY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DieSizeX;
        [DataMember]
        public double DieSizeX
        {
            get { return _DieSizeX; }
            set
            {
                if (value != _DieSizeX)
                {
                    _DieSizeX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DieSizeY;
        [DataMember]
        public double DieSizeY
        {
            get { return _DieSizeY; }
            set
            {
                if (value != _DieSizeY)
                {
                    _DieSizeY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _DutCount;
        [DataMember]
        public int DutCount
        {
            get { return _DutCount; }
            set
            {
                if (value != _DutCount)
                {
                    _DutCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsNowDevice;
        [DataMember]
        public bool IsNowDevice
        {
            get { return _IsNowDevice; }
            set
            {
                if (value != _IsNowDevice)
                {
                    _IsNowDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurXIndex;
        [DataMember]
        public int CurXIndex
        {
            get { return _CurXIndex; }
            set
            {
                if (value != _CurXIndex)
                {
                    _CurXIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _CurYIndex;
        [DataMember]
        public int CurYIndex
        {
            get { return _CurYIndex; }
            set
            {
                if (value != _CurYIndex)
                {
                    _CurYIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void SetDutViewControl(IFactoryModule facModule, IWaferObject waferObj, IProbeCard probeCard)
        {
            DutViewControl.CurCam = null;
            DutViewControl.VisibilityZoomIn = Visibility.Collapsed;
            DutViewControl.VisibilityZoomOut = Visibility.Collapsed;
            DutViewControl.VisibilityMoveToCenter = Visibility.Collapsed;

            if (waferObj == null || probeCard == null)
            {
                DutViewControl.StageSupervisor = null;
                DutViewControl.MotionManager = null;
                DutViewControl.WaferObject = null;
                DutViewControl.ProbeCard = null;
                DutViewControl.VisionManager = null;
            }
            else
            {
                DutViewControl.StageSupervisor = facModule.StageSupervisor();
                DutViewControl.MotionManager = facModule.MotionManager();
                DutViewControl.WaferObject = waferObj;
                DutViewControl.ProbeCard = probeCard;
                DutViewControl.VisionManager = facModule.VisionManager();
                DutViewControl.CamType = EnumProberCam.UNDEFINED;

                double zoomLevel = (double)((probeCard?.ProbeCardDevObjectRef as IProbeCardDevObject)?.DutList?.Count ?? 1);

                zoomLevel = (zoomLevel / 5);
                zoomLevel = 11 - zoomLevel;
                zoomLevel = 3 < zoomLevel ? zoomLevel : 3;

                DutViewControl.ZoomLevel = zoomLevel;
                DutViewControl.AddCheckBoxIsChecked = false;
                DutViewControl.EnableDragMap = true;
                DutViewControl.ShowCurrentPos = false;
                DutViewControl.ShowGrid = true;
                DutViewControl.ShowPad = true;
                DutViewControl.ShowPin = true;
                DutViewControl.ShowSelectedDut = false;
                DutViewControl.IsEnableMoving = false;
                //DutViewControl.WaferObject = waferObj;

                if (DutViewControl.WaferObject != null)
                    (DutViewControl.WaferObject as IWaferObject).MapViewControlMode = MapViewMode.MapMode;

                //RaisePropertyChanged(nameof(DutViewControl));
            }
        }

        public DeviceInfo()
        {

        }
        public DeviceInfo(string name)
        {
            Name = name;
        }
    }
}
