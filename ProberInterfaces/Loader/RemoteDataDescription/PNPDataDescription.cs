using System;
using System.Collections.Generic;

namespace ProberInterfaces //Loader.RemoteDataDescription
{
    using ProberInterfaces.Enum;
    using ProberInterfaces.PnpSetup;
    using ProberInterfaces.State;
    using SharpDXRender.RenderObjectPack;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Windows;

    [DataContract]
    public class PNPDataDescription
    {

        #region //..Buttons
        private PNPCommandButtonDescriptor _OneButton;
        [DataMember]
        public PNPCommandButtonDescriptor OneButton
        {
            get { return _OneButton; }
            set { _OneButton = value; }
        }

        private PNPCommandButtonDescriptor _TwoButton;
        [DataMember]
        public PNPCommandButtonDescriptor TwoButton
        {
            get { return _TwoButton; }
            set { _TwoButton = value; }
        }

        private PNPCommandButtonDescriptor _ThreeButton;
        [DataMember]
        public PNPCommandButtonDescriptor ThreeButton
        {
            get { return _ThreeButton; }
            set { _ThreeButton = value; }
        }

        private PNPCommandButtonDescriptor _FourButton;
        [DataMember]
        public PNPCommandButtonDescriptor FourButton
        {
            get { return _FourButton; }
            set { _FourButton = value; }
        }

        private PNPCommandButtonDescriptor _FiveButton;
        [DataMember]
        public PNPCommandButtonDescriptor FiveButton
        {
            get { return _FiveButton; }
            set { _FiveButton = value; }
        }

        private PNPCommandButtonDescriptor _PadJogLeft = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogLeft
        {
            get { return _PadJogLeft; }
            set { _PadJogLeft = value; }
        }

        private PNPCommandButtonDescriptor _PadJogRight = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogRight
        {
            get { return _PadJogRight; }
            set { _PadJogRight = value; }
        }


        private PNPCommandButtonDescriptor _PadJogUp = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogUp
        {
            get { return _PadJogUp; }
            set { _PadJogUp = value; }
        }

        private PNPCommandButtonDescriptor _PadJogDown = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogDown
        {
            get { return _PadJogDown; }
            set { _PadJogDown = value; }
        }

        private PNPCommandButtonDescriptor _PadJogSelect;
        [DataMember]
        public PNPCommandButtonDescriptor PadJogSelect
        {
            get { return _PadJogSelect; }
            set { _PadJogSelect = value; }
        }

        private PNPCommandButtonDescriptor _PadJogLeftUp = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogLeftUp
        {
            get { return _PadJogLeftUp; }
            set { _PadJogLeftUp = value; }
        }

        private PNPCommandButtonDescriptor _PadJogRightUp = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogRightUp
        {
            get { return _PadJogRightUp; }
            set { _PadJogRightUp = value; }
        }

        private PNPCommandButtonDescriptor _PadJogLeftDown = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogLeftDown
        {
            get { return _PadJogLeftDown; }
            set { _PadJogLeftDown = value; }
        }

        private PNPCommandButtonDescriptor _PadJogRightDown = new PNPCommandButtonDescriptor();
        [DataMember]
        public PNPCommandButtonDescriptor PadJogRightDown
        {
            get { return _PadJogRightDown; }
            set { _PadJogRightDown = value; }
        }
        #endregion

        //#region //.. Expanders
        //private SideViewExpanderDescriptor _ExpanderItem_01 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_01
        //{
        //    get { return _ExpanderItem_01; }
        //    set { _ExpanderItem_01 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_02 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_02
        //{
        //    get { return _ExpanderItem_02; }
        //    set { _ExpanderItem_02 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_03 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_03
        //{
        //    get { return _ExpanderItem_03; }
        //    set { _ExpanderItem_03 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_04 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_04
        //{
        //    get { return _ExpanderItem_04; }
        //    set { _ExpanderItem_04 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_05 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_05
        //{
        //    get { return _ExpanderItem_05; }
        //    set { _ExpanderItem_05 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_06 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_06
        //{
        //    get { return _ExpanderItem_06; }
        //    set { _ExpanderItem_06 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_07 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_07
        //{
        //    get { return _ExpanderItem_07; }
        //    set { _ExpanderItem_07 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_08 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_08
        //{
        //    get { return _ExpanderItem_08; }
        //    set { _ExpanderItem_08 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_09 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_09
        //{
        //    get { return _ExpanderItem_09; }
        //    set { _ExpanderItem_09 = value; }
        //}
        //private SideViewExpanderDescriptor _ExpanderItem_10 = new SideViewExpanderDescriptor();
        //[DataMember]
        //public SideViewExpanderDescriptor ExpanderItem_10
        //{
        //    get { return _ExpanderItem_10; }
        //    set { _ExpanderItem_10 = value; }
        //}

        //#endregion

        #region SideViewer

        public SideViewTextBlockDescriptor _SideViewTextBlock = new SideViewTextBlockDescriptor();
        [DataMember]
        public SideViewTextBlockDescriptor SideViewTextBlock
        {
            get { return _SideViewTextBlock; }
            set { _SideViewTextBlock = value; }
        }

        public ObservableCollection<SideViewTextBlockDescriptor> _SideViewTextBlocks = new ObservableCollection<SideViewTextBlockDescriptor>();
        [DataMember]
        public ObservableCollection<SideViewTextBlockDescriptor> SideViewTextBlocks
        {
            get { return _SideViewTextBlocks; }
            set { _SideViewTextBlocks = value; }
        }

        private SideViewMode _SideViewDisplayMode;
        [DataMember]
        public SideViewMode SideViewDisplayMode
        {
            get { return _SideViewDisplayMode; }
            set { _SideViewDisplayMode = value; }
        }

        private Visibility _SideViewTargetVisibility;
        [DataMember]
        public Visibility SideViewTargetVisibility
        {
            get { return _SideViewTargetVisibility; }
            set { _SideViewTargetVisibility = value; }
        }

        private Visibility _SideViewSwitchVisibility;
        [DataMember]
        public Visibility SideViewSwitchVisibility
        {
            get { return _SideViewSwitchVisibility; }
            set { _SideViewSwitchVisibility = value; }
        }

        private bool _SideViewExpanderVisibility;
        [DataMember]
        public bool SideViewExpanderVisibility
        {
            get { return _SideViewExpanderVisibility; }
            set { _SideViewExpanderVisibility = value; }
        }

        private bool _SideViewTextVisibility;
        [DataMember]
        public bool SideViewTextVisibility
        {
            get { return _SideViewTextVisibility; }
            set { _SideViewTextVisibility = value; }
        }

        private VerticalAlignment _SideViewVerticalAlignment;
        [DataMember]
        public VerticalAlignment SideViewVerticalAlignment
        {
            get { return _SideViewVerticalAlignment; }
            set { _SideViewVerticalAlignment = value; }
        }

        private HorizontalAlignment _SideViewHorizontalAlignment;
        [DataMember]
        public HorizontalAlignment SideViewHorizontalAlignment
        {
            get { return _SideViewHorizontalAlignment; }
            set { _SideViewHorizontalAlignment = value; }
        }

        private double _SideViewWidth;
        [DataMember]
        public double SideViewWidth
        {
            get { return _SideViewWidth; }
            set { _SideViewWidth = value; }
        }

        private double _SideViewHeight;
        [DataMember]
        public double SideViewHeight
        {
            get { return _SideViewHeight; }
            set { _SideViewHeight = value; }
        }

        private Thickness _SideViewMargin;
        [DataMember]
        public Thickness SideViewMargin
        {
            get { return _SideViewMargin; }
            set { _SideViewMargin = value; }
        }

        private string _SideViewTitle;
        [DataMember]
        public string SideViewTitle
        {
            get { return _SideViewTitle; }
            set { _SideViewTitle = value; }
        }

        private double _SideViewTitleFontSize;
        [DataMember]
        public double SideViewTitleFontSize
        {
            get { return _SideViewTitleFontSize; }
            set { _SideViewTitleFontSize = value; }
        }

        //private Brush _SideViewTitleFontColor;
        ////[DataMember]
        //public Brush SideViewTitleFontColor
        //{
        //    get { return _SideViewTitleFontColor; }
        //    set { _SideViewTitleFontColor = value; }
        //}

        //private Brush _SideViewTitleBackground;
        ////[DataMember]
        //public Brush SideViewTitleBackground
        //{
        //    get { return _SideViewTitleBackground; }
        //    set { _SideViewTitleBackground = value; }
        //}

        private string _SideViewTitleFontColorString;
        [DataMember]
        public string SideViewTitleFontColorString
        {
            get { return _SideViewTitleFontColorString; }
            set { _SideViewTitleFontColorString = value; }
        }

        private string _SideViewTitleBackgroundString;
        [DataMember]
        public string SideViewTitleBackgroundString
        {
            get { return _SideViewTitleBackgroundString; }
            set { _SideViewTitleBackgroundString = value; }
        }


        #endregion



        #region //.. DisplayPort
        private String _StepLabel;
        [DataMember]

        public String StepLabel
        {
            get { return _StepLabel; }
            set { _StepLabel = value; }
        }

        private String _StepSecondLabel;
        [DataMember]

        public String StepSecondLabel
        {
            get { return _StepSecondLabel; }
            set { _StepSecondLabel = value; }
        }

        private double _TargetRectangleWidth;
        [DataMember]
        public double TargetRectangleWidth
        {
            get { return _TargetRectangleWidth; }
            set { _TargetRectangleWidth = value; }
        }

        private double _TargetRectangleHeight;
        [DataMember]
        public double TargetRectangleHeight
        {
            get { return _TargetRectangleHeight; }
            set { _TargetRectangleHeight = value; }
        }

        private UserControlFucEnum _UseUserControl;
        [DataMember]
        public UserControlFucEnum UseUserControl
        {
            get { return _UseUserControl; }
            set { _UseUserControl = value; }
        }

        private bool _DisplayClickToMoveEnalbe;
        [DataMember]
        public bool DisplayClickToMoveEnalbe
        {
            get { return _DisplayClickToMoveEnalbe; }
            set { _DisplayClickToMoveEnalbe = value; }
        }

        private bool _DisplayIsHitTestVisible;
        [DataMember]
        public bool DisplayIsHitTestVisible
        {
            get { return _DisplayIsHitTestVisible; }
            set { _DisplayIsHitTestVisible = value; }
        }


        #endregion

        #region //..UI
        private string _MainViewTarget;
        [DataMember]
        public string MainViewTarget
        {
            get { return _MainViewTarget; }
            set { _MainViewTarget = value; }
        }

        private string _MiniViewTarget;
        [DataMember]
        public string MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set { _MiniViewTarget = value; }
        }

        private Visibility _MiniViewTargetVisibility;
        [DataMember]
        public Visibility MiniViewTargetVisibility
        {
            get { return _MiniViewTargetVisibility; }
            set { _MiniViewTargetVisibility = value; }
        }

        private Visibility _MiniViewSwapVisibility;
        [DataMember]
        public Visibility MiniViewSwapVisibility
        {
            get { return _MiniViewSwapVisibility; }
            set { _MiniViewSwapVisibility = value; }
        }
        private Visibility _LightJogVisibility;
        [DataMember]
        public Visibility LightJogVisibility
        {
            get { return _LightJogVisibility; }
            set { _LightJogVisibility = value; }
        }

        private Visibility _MotionJogVisibility;
        [DataMember]
        public Visibility MotionJogVisibility
        {
            get { return _MotionJogVisibility; }
            set { _MotionJogVisibility = value; }
        }

        private JogMode _JogType;
        [DataMember]
        public JogMode JogType
        {
            get { return _JogType; }
            set { _JogType = value; }
        }

        private bool _MotionJogEnabled = true;
        [DataMember]
        public bool MotionJogEnabled
        {
            get { return _MotionJogEnabled; }
            set { _MotionJogEnabled = value; }
        }

        private HorizontalAlignment _MiniViewHorizontalAlignment;
        [DataMember]
        public HorizontalAlignment MiniViewHorizontalAlignment
        {
            get { return _MiniViewHorizontalAlignment; }
            set { _MiniViewHorizontalAlignment = value; }
        }

        private VerticalAlignment _MiniViewVerticalAlignment;
        [DataMember]
        public VerticalAlignment MiniViewVerticalAlignment
        {
            get { return _MiniViewVerticalAlignment; }
            set { _MiniViewVerticalAlignment = value; }
        }

        private EnumMoudleSetupState _StateSetup;
        [DataMember]
        public EnumMoudleSetupState StateSetup
        {
            get { return _StateSetup; }
            set { _StateSetup = value; }
        }

        private bool _NoneCleanUp;
        [DataMember]
        public bool NoneCleanUp
        {
            get { return _NoneCleanUp; }
            set { _NoneCleanUp = value; }
        }

        private double _RenderWidth;
        [DataMember]
        public double RenderWidth
        {
            get { return _RenderWidth; }
            set { _RenderWidth = value; }
        }

        private double _RenderHeight;
        [DataMember]
        public double RenderHeight
        {
            get { return _RenderHeight; }
            set { _RenderHeight = value; }
        }

        private bool _UseRender;
        [DataMember]
        public bool UseRender
        {
            get { return _UseRender; }
            set { _UseRender = value; }
        }

        private bool _IsExistAdvenceSetting;
        [DataMember]
        public bool IsExistAdvenceSetting
        {
            get { return _IsExistAdvenceSetting; }
            set { _IsExistAdvenceSetting = value; }
        }


        private EnumMoudleSetupState _SetupState;
        [DataMember]
        public EnumMoudleSetupState SetupState
        {
            get { return _SetupState; }
            set { _SetupState = value; }
        }

        private long _MapXIndex;
        [DataMember]
        public long MapXIndex
        {
            get { return _MapXIndex; }
            set { _MapXIndex = value; }
        }

        private long _MapYIndex;
        [DataMember]
        public long MapYIndex
        {
            get { return _MapYIndex; }
            set { _MapYIndex = value; }
        }

        private ModuleDllInfo _AdvanceSetupViewModuleInfo;
        [DataMember]
        public ModuleDllInfo AdvanceSetupViewModuleInfo
        {
            get { return _AdvanceSetupViewModuleInfo; }
            set { _AdvanceSetupViewModuleInfo = value; }
        }

        private ModuleDllInfo _AdvanceSetupViewModelModuleInfo;
        [DataMember]
        public ModuleDllInfo AdvanceSetupViewModelModuleInfo
        {
            get { return _AdvanceSetupViewModelModuleInfo; }
            set { _AdvanceSetupViewModelModuleInfo = value; }
        }

        private byte[] _MainViewImageSource;
        [DataMember]
        public byte[] MainViewImageSource
        {
            get { return _MainViewImageSource; }
            set { _MainViewImageSource = value; }
        }


        #endregion

        private List<byte[]> _Params;
        [DataMember]
        public List<byte[]> Params
        {
            get { return _Params; }
            set { _Params = value; }
        }

        private List<RenderContainer> _RenderContainers;
        [DataMember]
        public List<RenderContainer> RenderContainers
        {
            get { return _RenderContainers; }
            set { _RenderContainers = value; }
        }

    }

    [DataContract]
    public class PnpUIData
    {
        private String _StepLabel;
        [DataMember]

        public String StepLabel
        {
            get { return _StepLabel; }
            set { _StepLabel = value; }
        }

        private String _StepSecondLabel;
        [DataMember]

        public String StepSecondLabel
        {
            get { return _StepSecondLabel; }
            set { _StepSecondLabel = value; }
        }

        private double _TargetRectangleWidth;
        [DataMember]
        public double TargetRectangleWidth
        {
            get { return _TargetRectangleWidth; }
            set { _TargetRectangleWidth = value; }
        }

        private double _TargetRectangleHeight;
        [DataMember]
        public double TargetRectangleHeight
        {
            get { return _TargetRectangleHeight; }
            set { _TargetRectangleHeight = value; }
        }

        private UserControlFucEnum _UseUserControl;
        [DataMember]
        public UserControlFucEnum UseUserControl
        {
            get { return _UseUserControl; }
            set { _UseUserControl = value; }
        }

        private Visibility _SideViewTargetVisibility;
        [DataMember]
        public Visibility SideViewTargetVisibility
        {
            get { return _SideViewTargetVisibility; }
            set { _SideViewTargetVisibility = value; }
        }

    }
}
