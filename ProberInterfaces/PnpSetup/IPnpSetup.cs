using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;

namespace ProberInterfaces.PnpSetup
{
    using LogModule;
    using Newtonsoft.Json;
    using ProberErrorCode;
    using ProberInterfaces.Enum;
    using ProberInterfaces.LightJog;
    using SharpDXRender;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;


    public enum PNPCommandButtonType
    {
        PADJOGLEFT,
        PADJOGRIGHT,
        PADJOGUP,
        PADJOGDOWN,
        PADJOGSELECT,
        PADJOGLEFTUP,
        PADJOGRIGHTUP,
        PADJOGLEFTDOWN,
        PADJOGRIGHTDOWN,
        ONEBUTTON,
        TWOBUTTON,
        THREEBUTTON,
        FOURBUTTON,
        FIVEBUTTON
    }

    public interface IPnpSetup : IUseLightJog
    {

        IStageSupervisor StageSupervisor { get; }
        IVisionManager VisionManager { get; }
        IPnpManager PnpManager { get; }
        bool IsSeleted { get; set; }
        //List<byte[]> PackagableParams { get; set; }
        EnumSetupProgressState ProcessingType { get; set; }
        Visibility TreeviewItemVisibility { get; set; }
        object MainViewTarget { get; set; }
        object MiniViewTarget { get; set; }
        ImageBuffer ImgBuffer { get; set; }
        float MapSize { get; set; }
        double MapTransparency { get; set; }
        IDisplayPort DisplayPort { get; set; }
        MachineIndex MapIndex { get; set; }
        EnumProberCam CameraType { get; set; }

        BitmapImage MainViewImageSource { get; set; }

        int CurrMaskingLevel { get; set; }
        ICommand ViewSwapCommand { get; }
        //ICommand SetZeroCommand { get; }
        Visibility MiniViewZoomVisibility { get; set; }
        Visibility MainViewZoomVisibility { get; set; }
        String StepLabel { get; set; }
        String StepSecondLabel { get; set; }
        bool StepLabelActive { get; set; }
        bool StepSecondLabelActive { get; set; }
        UserControlFucEnum UseUserControl { get; set; }
        double TargetRectangleWidth { get; set; }
        double TargetRectangleHeight { get; set; }
        double TargetRectangleLeft { get; set; }
        double TargetRectangleTop { get; set; }
        IPnpAdvanceSetupViewModel AdvanceSetupViewModel { get; set; }
        void UpdateLabel();

        CancellationTokenSourcePack PadJogSelectTokenPack { get; set; }

        PNPCommandButtonDescriptor PadJogUp { get; set; }
        PNPCommandButtonDescriptor PadJogDown { get; set; }
        PNPCommandButtonDescriptor PadJogLeft { get; set; }
        PNPCommandButtonDescriptor PadJogRight { get; set; }
        PNPCommandButtonDescriptor PadJogSelect { get; set; }
        PNPCommandButtonDescriptor PadJogLeftUp { get; set; }
        PNPCommandButtonDescriptor PadJogRightUp { get; set; }
        PNPCommandButtonDescriptor PadJogLeftDown { get; set; }
        PNPCommandButtonDescriptor PadJogRightDown { get; set; }

        PNPCommandButtonDescriptor OneButton { get; set; }
        PNPCommandButtonDescriptor TwoButton { get; set; }
        PNPCommandButtonDescriptor ThreeButton { get; set; }
        PNPCommandButtonDescriptor FourButton { get; set; }
        PNPCommandButtonDescriptor FiveButton { get; set; }

        SideViewExpanderDescriptor ExpanderItem_01 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_02 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_03 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_04 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_05 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_06 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_07 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_08 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_09 { get; set; }
        SideViewExpanderDescriptor ExpanderItem_10 { get; set; }
        RenderLayer SharpDXLayer { get; set; }
        JogMode JogType { get; set; }
        void SetPNPDataDescriptor(PNPDataDescription dataDescriptor);
        EventCodeEnum BindingPNPSetup();
        //SideViewTextBlockDescriptor SideViewTextBlock { get; set; }

        //void CleanUp();
        void SwitchCamera(ICamera cam);
        void SetPackagableParams();
        void SetAdvanceSetupView(IPnpAdvanceSetupView view);
        void SetAdvanceSetupViewModel(IPnpAdvanceSetupViewModel viewmodel);
    }

    public interface IHasAdvancedSetup
    {
        void CloseAdvanceSetupView();
    }

    public interface IPNPCommandButtonDescriptor
    {

    }

    [XmlInclude(typeof(ImageSource))]
    [XmlInclude(typeof(Visibility))]
    [Export(typeof(IPNPCommandButtonDescriptor))]

    [Serializable, DataContract]
    public class PNPCommandButtonDescriptor : INotifyPropertyChanged, IPNPCommandButtonDescriptor, IFactoryModule
    {

        #region ==> PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        // define a dictionary to map property names to PNPCommandButtonType values
        private static readonly Dictionary<string, PNPCommandButtonType> ButtonTypeMap = new Dictionary<string, PNPCommandButtonType>()
        {
            { "_PadJogLeft", PNPCommandButtonType.PADJOGLEFT },
            { "_PadJogRight", PNPCommandButtonType.PADJOGRIGHT },
            { "_PadJogUp", PNPCommandButtonType.PADJOGUP },
            { "_PadJogDown", PNPCommandButtonType.PADJOGDOWN },
            { "_PadJogSelect", PNPCommandButtonType.PADJOGSELECT },
            { "_PadJogLeftUp", PNPCommandButtonType.PADJOGLEFTUP },
            { "_PadJogRightUp", PNPCommandButtonType.PADJOGRIGHTUP },
            { "_PadJogLeftDown", PNPCommandButtonType.PADJOGLEFTDOWN },
            { "_PadJogRightDown", PNPCommandButtonType.PADJOGRIGHTDOWN },
            { "_OneButton", PNPCommandButtonType.ONEBUTTON },
            { "_TwoButton", PNPCommandButtonType.TWOBUTTON },
            { "_ThreeButton", PNPCommandButtonType.THREEBUTTON },
            { "_FourButton", PNPCommandButtonType.FOURBUTTON },
            { "_FiveButton", PNPCommandButtonType.FIVEBUTTON }
        };

        public PNPCommandButtonDescriptor([CallerMemberName] string propertyName = "")
        {
            try
            {
                // set PNPButtonType based on the property name using the dictionary
                if (ButtonTypeMap.TryGetValue(propertyName, out PNPCommandButtonType buttonType))
                {
                    PNPButtonType = buttonType;
                }
                else
                {
                    LoggerManager.Error($"[{this.GetType().Name}], PNPCommandButtonDescriptor() : Unknown Error.");
                }

                _Visibility = Visibility.Visible;
                _Caption = null;
                _IconCaption = null;
                _CaptionSize = 20;
                _IsEnabled = false;
                _Opacity = 0.2;
                _Softness = 0;
                _RepeatEnable = false;
                _PnpSetups = new ObservableCollection<IPnpSetup>();
                _PnpSetupScreen = new ObservableCollection<IPnpSetupScreen>();
                _ScreenNameList = new ObservableCollection<string>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        [NonSerialized]
        private ImageSource _IconSource;
        public ImageSource IconSource
        {
            get { return _IconSource; }
            set
            {
                if (value != _IconSource)
                {
                    _IconSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte[] _IconSoruceArray;
        [DataMember]
        public byte[] IconSoruceArray
        {
            get { return _IconSoruceArray; }
            set
            {
                if (value != _IconSoruceArray)
                {
                    _IconSoruceArray = value;
                    RaisePropertyChanged();

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        public void SetIconSoruce(string path)
        {
            try
            {
                if (path != string.Empty)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        try
                        {
                            var image = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(path, UriKind.Absolute));

                            byte[] buffer;
                            try
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(image));

                                using (MemoryStream ms = new MemoryStream())
                                {
                                    encoder.Save(ms);
                                    buffer = ms.ToArray();
                                }
                                IconSource = image;
                                IconSoruceArray = buffer;
                            }
                            catch (NotSupportedException)
                            {
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                        }
                        //Stream stream = (IconSource as System.Windows.Media.Imaging.BitmapImage).StreamSource;
                        //IconSoruceArray = new byte[stream.Length];
                        //stream.Read(IconSoruceArray, 0, (int)stream.Length);
                    });
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        public void SetIconSoruceBitmap(System.Drawing.Bitmap bitmap)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            ms.Seek(0, SeekOrigin.Begin);
                            image.StreamSource = ms;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.StreamSource = null;
                            IconSource = image;

                            byte[] buffer;
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            using (MemoryStream mss = new MemoryStream())
                            {
                                encoder.Save(mss);
                                buffer = ms.ToArray();
                            }

                            IconSoruceArray = buffer;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private ImageSource _MiniIconSource;
        public ImageSource MiniIconSource
        {
            get { return _MiniIconSource; }
            set
            {
                if (value != _MiniIconSource)
                {
                    _MiniIconSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private byte[] _MiniIconSoruceArray;
        [DataMember]
        public byte[] MiniIconSoruceArray
        {
            get { return _MiniIconSoruceArray; }
            set
            {
                if (value != _MiniIconSoruceArray)
                {
                    _MiniIconSoruceArray = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }


        public void SetMiniIconSoruce(string path)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        var image = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(path, UriKind.Absolute));


                        byte[] buffer;
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            encoder.Save(ms);
                            buffer = ms.ToArray();
                        }

                        MiniIconSource = image;
                        MiniIconSoruceArray = buffer;
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void SetMiniIconSoruceBitmap(System.Drawing.Bitmap bitmap)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            ms.Seek(0, SeekOrigin.Begin);
                            image.StreamSource = ms;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.StreamSource = null;
                            MiniIconSource = image;

                            byte[] buffer;
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            using (MemoryStream mss = new MemoryStream())
                            {
                                encoder.Save(mss);
                                buffer = ms.ToArray();
                            }

                            MiniIconSoruceArray = buffer;
                        }
                    }
                    catch (Exception err)
                    {
                        LoggerManager.Exception(err);
                    }
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private Visibility _Visibility;
        [DataMember]
        public Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                if (value != _Visibility)
                {
                    _Visibility = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private string _Caption;
        [DataMember]
        public string Caption
        {
            get { return _Caption; }
            set
            {
                if (value != _Caption)
                {
                    _Caption = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private string _IconCaption;
        [DataMember]
        public string IconCaption
        {
            get { return _IconCaption; }
            set
            {
                if (value != _IconCaption)
                {
                    _IconCaption = value;
                    RaisePropertyChanged();

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private double _CaptionSize = 1;
        [DataMember]
        public double CaptionSize
        {
            get { return _CaptionSize; }
            set
            {
                if (value != _CaptionSize)
                {
                    _CaptionSize = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private ObservableCollection<string> _ScreenNameList;
        [DataMember]
        public ObservableCollection<string> ScreenNameList
        {
            get { return _ScreenNameList; }
            set
            {
                if (value != _ScreenNameList)
                {
                    _ScreenNameList = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        [NonSerialized]
        private Guid _CUIGUID;
        [DataMember]
        public Guid CUIGUID
        {
            get { return _CUIGUID; }
            set
            {
                if (value != _CUIGUID)
                {
                    _CUIGUID = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }


        private ICommand _Command = null;
        [XmlIgnore, JsonIgnore]
        public ICommand Command
        {
            get { return _Command; }
            set
            {
                if (value != _Command)
                {
                    _Command = value;
                    if (_Command != null)
                        IsEnabled = true;
                    else
                        IsEnabled = false;
                    RaisePropertyChanged();
                }
            }
        }

        private object _CommandParameter;
        [XmlIgnore, JsonIgnore, DataMember]
        public object CommandParameter
        {
            get { return _CommandParameter; }
            set
            {
                if (value != _CommandParameter)
                {
                    _CommandParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsEnabled = false;
        [DataMember]
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (value != _IsEnabled)
                {
                    _IsEnabled = value;

                    if (_IsEnabled)
                    {
                        Opacity = 1;
                        Lockable = true;
                    }
                    else
                    {
                        Opacity = 0.2;
                        Lockable = false;
                    }

                    RaisePropertyChanged();

                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private bool _Lockable = false;
        [DataMember]
        public bool Lockable
        {
            get { return _Lockable; }
            set
            {
                if (value != _Lockable)
                {
                    _Lockable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsAdvanceSetupButton = false;
        [DataMember]
        public bool IsAdvanceSetupButton
        {
            get { return _IsAdvanceSetupButton; }
            set
            {
                if (value != _IsAdvanceSetupButton)
                {
                    _IsAdvanceSetupButton = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _Opacity = 0.2;
        [DataMember]
        public double Opacity
        {
            get { return _Opacity; }
            set
            {
                if (value != _Opacity)
                {
                    _Opacity = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _Softness;
        [DataMember]
        public double Softness
        {
            get { return _Softness; }
            set
            {
                if (value != _Softness)
                {
                    _Softness = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private bool _RepeatEnable;
        [DataMember]
        public bool RepeatEnable
        {
            get { return _RepeatEnable; }
            set
            {
                if (value != _RepeatEnable)
                {
                    _RepeatEnable = value;
                    RaisePropertyChanged();
                    if (!this.PnPManager().IsActivePageSwithcing())
                    {
                        this.LoaderRemoteMediator()?.GetServiceCallBack()?.PNPButtonsUpdated(this.LoaderRemoteMediator()?.GetPNPDataDescriptor());
                    }
                }
            }
        }

        private PNPCommandButtonType _PNPButtonType;
        [DataMember]
        public PNPCommandButtonType PNPButtonType
        {
            get { return _PNPButtonType; }
            set
            {
                if (value != _PNPButtonType)
                {
                    _PNPButtonType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IPnpSetup> _PnpSetups;
        [XmlIgnore, JsonIgnore, DataMember]
        public ObservableCollection<IPnpSetup> PnpSetups
        {
            get { return _PnpSetups; }
            set
            {
                if (value != _PnpSetups)
                {
                    _PnpSetups = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<IPnpSetupScreen> _PnpSetupScreen;
        [XmlIgnore, JsonIgnore, DataMember]
        public ObservableCollection<IPnpSetupScreen> PnpSetupScreen
        {
            get { return _PnpSetupScreen; }
            set
            {
                if (value != _PnpSetupScreen)
                {
                    _PnpSetupScreen = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void CopyTo(PNPCommandButtonDescriptor descriptor)
        {
            descriptor.Caption = this.Caption;
            descriptor.IsEnabled = this.IsEnabled;
            descriptor.IconSoruceArray = this.IconSoruceArray;
            descriptor.Lockable = this.Lockable;
            descriptor.CaptionSize = this.CaptionSize;
            descriptor.MiniIconSoruceArray = this.MiniIconSoruceArray;
            descriptor.RepeatEnable = this.RepeatEnable;
            descriptor.PNPButtonType = this.PNPButtonType;
            descriptor.IconCaption = this.IconCaption;
        }
    }


    [XmlInclude(typeof(Visibility))]
    [Serializable, DataContract]
    public class SideViewExpanderDescriptor : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public SideViewExpanderDescriptor()
        {
            try
            {
                _Header = "";
                _Description = "";
                _HeaderFontSize = 16;
                _DescriptionFontSize = 12;
                _HeaderColor = Brushes.White;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private string _Header;
        [DataMember]
        public string Header
        {
            get { return _Header; }
            set
            {
                if (value != _Header)
                {
                    _Header = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _HeaderFontSize = 1;
        [DataMember]
        public double HeaderFontSize
        {
            get { return _HeaderFontSize; }
            set
            {
                if (value != _HeaderFontSize)
                {
                    _HeaderFontSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _HeaderColor;
        //[DataMember]
        public Brush HeaderColor
        {
            get { return _HeaderColor; }
            set
            {
                if (value != _HeaderColor)
                {
                    _HeaderColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _Description;
        [DataMember]
        public string Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _DescriptionFontSize = 1;
        [DataMember]
        public double DescriptionFontSize
        {
            get { return _DescriptionFontSize; }
            set
            {
                if (value != _DescriptionFontSize)
                {
                    _DescriptionFontSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Visibility _ExpanderVisibility;
        [DataMember]
        public Visibility ExpanderVisibility
        {
            get { return _ExpanderVisibility; }
            set
            {
                if (value != _ExpanderVisibility)
                {
                    _ExpanderVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public enum SideViewMode
    {
        TEXTBLOCK_MODE,
        EXPANDER_MODE,
        TEXTBLOCK_ONLY,
        EXPANDER_ONLY,
        NOUSE
    }

    [Serializable, DataContract]
    public class SideViewTextBlockDescriptor : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public SideViewTextBlockDescriptor()
        {
            try
            {
                _SideTextContents = "";
                _SideTextFontSize = 16;
                _SideTextFontColor = Brushes.White;
                _SideTextBackground = Brushes.Transparent;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private string _SideTextContents;
        [DataMember]
        public string SideTextContents
        {
            get { return _SideTextContents; }
            set
            {
                if (value != _SideTextContents)
                {
                    _SideTextContents = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _SideTextFontSize;
        [DataMember]
        public double SideTextFontSize
        {
            get { return _SideTextFontSize; }
            set
            {
                if (value != _SideTextFontSize)
                {
                    _SideTextFontSize = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _SideTextFontColor;
        //[DataMember]
        public Brush SideTextFontColor
        {
            get { return _SideTextFontColor; }
            set
            {
                if (value != _SideTextFontColor)
                {
                    _SideTextFontColor = value;
                    RaisePropertyChanged();
                }

                _SideTextFontColorString = SideTextFontColor.ToString();
            }
        }

        private Brush _SideTextBackground;
        //[DataMember]
        public Brush SideTextBackground
        {
            get { return _SideTextBackground; }
            set
            {
                if (value != _SideTextBackground)
                {
                    _SideTextBackground = value;

                    RaisePropertyChanged();
                }

                _SideTextBackgroundString = _SideTextBackground.ToString();
            }
        }

        private string _SideTextFontColorString;
        [DataMember]
        public string SideTextFontColorString
        {
            get { return _SideTextFontColorString; }
            set
            {
                if (value != _SideTextFontColorString)
                {
                    _SideTextFontColorString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _SideTextBackgroundString;
        [DataMember]
        public string SideTextBackgroundString
        {
            get { return _SideTextBackgroundString; }
            set
            {
                if (value != _SideTextBackgroundString)
                {
                    _SideTextBackgroundString = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
}
