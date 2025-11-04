using System;
using System.Windows;
using System.Windows.Controls;

namespace WaferAlignControlDialog
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using LogModule;
    using PMIModuleParameter;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Communication.Scenario;
    using ProberInterfaces.MarkAlign;
    using ProberInterfaces.Param;
    using ProberInterfaces.PinAlign;
    using ProberInterfaces.PolishWafer;
    using ProberInterfaces.Temperature;
    using ProberInterfaces.Temperature.DewPoint;
    using ProberInterfaces.WaferAlignEX.Enum;
    using RequestInterface;
    using Temperature.Temp.DewPoint;
    using WAHighStandardModule;

    /// <summary>
    /// WAControlDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WAControlDialog : Window, INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public WAControlDialog()
        {
            try
            {
                this.DataContext = this;
                InitializeComponent();
                DewPointModule = this.EnvControlManager().DewPointManager.DewPointModule;
                PMLowDownAcceptance = this.VisionManager().GetWaferLowPMDownAcceptance();
                DelaywaferCamCylinderExtendedBeforeFocusing = this.MarkAligner().GetDelaywaferCamCylinderExtendedBeforeFocusing();
                MarkDiffTolerance_X = this.MarkAligner().GetMarkDiffTolerance_X();
                MarkDiffTolerance_Y = this.MarkAligner().GetMarkDiffTolerance_Y();

                var markTolerance = this.MarkAligner().GetMarkDiffToleranceOfWA();
                MarkDiffToleranceOfWA_X = markTolerance.Item1;
                MarkDiffToleranceOfWA_Y = markTolerance.Item2;

                IsOnDubugMode = this.WaferAligner().IsOnDubugMode;
                IsOnDebugImagePathBase = this.WaferAligner().IsOnDebugImagePathBase;
                IsOnDebugPadPathBase = this.WaferAligner().IsOnDebugPadPathBase;
                StageSupervisor = this.StageSupervisor();
                TempController = this.TempController();

                ProberID = this.FileManager().GetProberID();

                var pmiDevParam = this.PMIModule().GetPMIDevIParam() as PMIModuleDevParam;
                if (pmiDevParam != null)
                {
                    PMIDelayInFirstGroup = pmiDevParam.DelayInFirstGroup.Value;
                    PMIDelayAfterMoveToPad = pmiDevParam.DelayAfterMoveToPad.Value;
                }
                else
                {
                    PMIDelayInFirstGroup = -1;
                    PMIDelayAfterMoveToPad = -1;
                }

                var pmiSysParam = this.PMIModule().GetPMISysIParam() as PMIModuleSysParam;

                if (pmiSysParam != null)
                {
                    DoPMIDebugImages = pmiSysParam.DoPMIDebugImages;
                }
                else
                {
                    DoPMIDebugImages = false;
                }

                var verifyWaferXYLimit = this.WaferAligner().GetVerifyCenterLimitXYValue();
                VerifyCenterLimitX = verifyWaferXYLimit.Item1;
                VerifyCenterLimitY = verifyWaferXYLimit.Item2;

                MarkVerificationAfterWaferAlign = this.MarkAligner().GetTriggerMarkVerificationAfterWaferAlign();

                // TesterScenarioManager

                ScenarioModule = this.TesterCommunicationManager().ScenarioManager.ScenarioModule;

                // LOT
                LoadedWaferCountUntilBeforeLotStart = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;
                LoadedWaferCountUntilBeforeDeviceChange = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeDeviceChange;

                // Vision
                ImageSaveFilter = this.VisionManager().imageSaveFilter;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public IWaferAligner WaferAligner { get => this.WaferAligner(); }
        public IDewPointModule DewPointModule { get; set; }

        public IMarkAligner MarkAligner { get => this.MarkAligner(); }
        public IPolishWaferModule PolishWafer { get => this.PolishWaferModule(); }

        public IPinAligner PinAligner { get => this.PinAligner(); }

        private ITesterScenarioModule _ScenarioModule;
        public ITesterScenarioModule ScenarioModule
        {
            get { return _ScenarioModule; }
            set
            {
                if (value != _ScenarioModule)
                {
                    _ScenarioModule = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ScenarioSet _myProperty;
        public ScenarioSet MyProperty
        {
            get { return _myProperty; }
            set
            {
                if (value != _myProperty)
                {
                    _myProperty = value;
                    RaisePropertyChanged();
                }
            }
        }

        private HighStandard HighStandardModule { get; set; }
        public Array LowIndexPositions => Enum.GetValues(typeof(EnumLowStandardPosition));
        public Array HighIndexPositions => Enum.GetValues(typeof(EnumHighStandardPosition));
        private string _FullSite = "";
        public string FullSite
        {
            get { return _FullSite; }
            set
            {
                if (value != _FullSite)
                {
                    _FullSite = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsInsertOCommand;
        public bool IsInsertOCommand
        {
            get { return _IsInsertOCommand; }
            set
            {
                if (value != _IsInsertOCommand)
                {
                    _IsInsertOCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IStageSupervisor _StageSupervisor;
        public IStageSupervisor StageSupervisor
        {
            get { return _StageSupervisor; }
            set
            {
                if (value != _StageSupervisor)
                {
                    _StageSupervisor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ITempController _TempController;
        public ITempController TempController
        {
            get { return _TempController; }
            set
            {
                if (value != _TempController)
                {
                    _TempController = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ProberID;
        public string ProberID
        {
            get { return _ProberID; }
            set
            {
                if (value != _ProberID)
                {
                    _ProberID = value;
                    RaisePropertyChanged();
                }
            }
        }
        private int _PMIDelayInFirstGroup;
        public int PMIDelayInFirstGroup
        {
            get { return _PMIDelayInFirstGroup; }
            set
            {
                if (value != _PMIDelayInFirstGroup)
                {
                    _PMIDelayInFirstGroup = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PMIDelayAfterMoveToPad;
        public int PMIDelayAfterMoveToPad
        {
            get { return _PMIDelayAfterMoveToPad; }
            set
            {
                if (value != _PMIDelayAfterMoveToPad)
                {
                    _PMIDelayAfterMoveToPad = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _DoPMIDebugImages;
        public bool DoPMIDebugImages
        {
            get { return _DoPMIDebugImages; }
            set
            {
                if (value != _DoPMIDebugImages)
                {
                    _DoPMIDebugImages = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VerifyCenterLimitX;
        public double VerifyCenterLimitX
        {
            get { return _VerifyCenterLimitX; }
            set
            {
                if (value != _VerifyCenterLimitX)
                {
                    _VerifyCenterLimitX = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VerifyCenterLimitY;
        public double VerifyCenterLimitY
        {
            get { return _VerifyCenterLimitY; }
            set
            {
                if (value != _VerifyCenterLimitY)
                {
                    _VerifyCenterLimitY = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MarkVerificationAfterWaferAlign;
        public bool MarkVerificationAfterWaferAlign
        {
            get { return _MarkVerificationAfterWaferAlign; }
            set
            {
                if (value != _MarkVerificationAfterWaferAlign)
                {
                    _MarkVerificationAfterWaferAlign = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _LoadedWaferCountUntilBeforeLotStart;
        public int LoadedWaferCountUntilBeforeLotStart
        {
            get { return _LoadedWaferCountUntilBeforeLotStart; }
            set
            {
                _LoadedWaferCountUntilBeforeLotStart = value;
                RaisePropertyChanged();
            }
        }

        private int _LoadedWaferCountUntilBeforeDeviceChange;
        public int LoadedWaferCountUntilBeforeDeviceChange
        {
            get { return _LoadedWaferCountUntilBeforeDeviceChange; }
            set
            {
                _LoadedWaferCountUntilBeforeDeviceChange = value;
                RaisePropertyChanged();
            }
        }

        // Assuming this is a collection of available filters
        public List<ImageSaveFilter> ImageSaveFilters { get; } = Enum.GetValues(typeof(ImageSaveFilter)).Cast<ImageSaveFilter>().ToList();

        private ImageSaveFilter _ImageSaveFilter;
        public ImageSaveFilter ImageSaveFilter
        {
            get { return _ImageSaveFilter; }
            set
            {
                _ImageSaveFilter = value;
                RaisePropertyChanged();
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaferAligner.WaferAlignControItems.EdgeFail = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaferAligner.WaferAlignControItems.EdgeFail = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                WaferAligner.WaferAlignControItems.LowFailPos = (EnumLowStandardPosition)(sender as System.Windows.Controls.ComboBox).SelectedItem;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                WaferAligner.WaferAlignControItems.HighFailPos = (EnumHighStandardPosition)(sender as System.Windows.Controls.ComboBox).SelectedItem;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (DewPointModule as DewPointReader).EmulDewPoint = Convert.ToInt32(tb_dewpoint.Text);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        #region <region> Vision </region>

        private string _VisionPMTargetPath;
        public string VisionPMTargetPath
        {
            get { return _VisionPMTargetPath; }
            set
            {
                if (value != _VisionPMTargetPath)
                {
                    _VisionPMTargetPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _VisionPMPatternPath;
        public string VisionPMPatternPath
        {
            get { return _VisionPMPatternPath; }
            set
            {
                if (value != _VisionPMPatternPath)
                {
                    _VisionPMPatternPath = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _VisionPMResultValue;
        public double VisionPMResultValue
        {
            get { return _VisionPMResultValue; }
            set
            {
                if (value != _VisionPMResultValue)
                {
                    _VisionPMResultValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PMAcceptanceValue;
        public int PMAcceptanceValue
        {
            get { return _PMAcceptanceValue; }
            set
            {
                if (value != _PMAcceptanceValue)
                {
                    _PMAcceptanceValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PMLowDownAcceptance;
        public int PMLowDownAcceptance
        {
            get { return _PMLowDownAcceptance; }
            set
            {
                if (value != _PMLowDownAcceptance)
                {
                    _PMLowDownAcceptance = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _VisionImageDecordTargetPath;
        public string VisionImageDecordTargetPath
        {
            get { return _VisionImageDecordTargetPath; }
            set
            {
                if (value != _VisionImageDecordTargetPath)
                {
                    _VisionImageDecordTargetPath = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private void btn_OpenPMTargetPathFileExplore_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "All files(*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                VisionPMTargetPath = openFileDialog.FileName;
            }
        }

        private void btn_OpenPMPatternPathFileExplore_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "All files(*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                VisionPMPatternPath = openFileDialog.FileName;
            }
        }

        private void btn_PatternMatching_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PatternInfomation ptinfo = new PatternInfomation();
                ImageBuffer targetimg = this.VisionManager().LoadImageFile(VisionPMTargetPath);
                targetimg.CamType = EnumProberCam.WAFER_LOW_CAM;
                ptinfo.CamType.Value = EnumProberCam.WAFER_LOW_CAM;
                ptinfo.PMParameter = new ProberInterfaces.Vision.PMParameter();
                ptinfo.PMParameter.PMAcceptance.Value = (int)PMAcceptanceValue;
                ptinfo.PMParameter.ModelFilePath.Value = VisionPMPatternPath;

                var pmResult = this.VisionManager().PatternMatching(ptinfo, this, true, retryautolight: false, img: targetimg);

                if (pmResult.ResultParam != null)
                {
                    if (pmResult.ResultParam.Count > 0)
                    {
                        VisionPMResultValue = pmResult.ResultParam[0].Score;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_PMProcessingSAVE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.VisionManager().SetWaferLowPMDownAcceptance(PMLowDownAcceptance);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GEMModule().GetPIVContainer().TmpFullSite
                    = this.FullSite;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                int x = 0;
                int y = 0;
                if (tb_probingXcoord.Text == null || tb_probingYcoord.Text == null)
                    return;
                else
                {
                    x = Convert.ToInt32(tb_probingXcoord.Text);
                    y = Convert.ToInt32(tb_probingYcoord.Text);
                    this.FullSite = this.GPIB().GetFullSite(x, y);
                    LoggerManager.Debug($"[FullSiteDate : {FullSite}]");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GEMModule().GetPIVContainer().IsInsertOCommand
                     = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GEMModule().GetPIVContainer().IsInsertOCommand
                    = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region Mark Align
        private int _DelaywaferCamCylinderExtendedBeforeFocusing;
        public int DelaywaferCamCylinderExtendedBeforeFocusing
        {
            get { return _DelaywaferCamCylinderExtendedBeforeFocusing; }
            set
            {
                if (value != _DelaywaferCamCylinderExtendedBeforeFocusing)
                {
                    _DelaywaferCamCylinderExtendedBeforeFocusing = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkDiffTolerance_X;
        public double MarkDiffTolerance_X
        {
            get { return _MarkDiffTolerance_X; }
            set
            {
                if (value != _MarkDiffTolerance_X)
                {
                    _MarkDiffTolerance_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkDiffTolerance_Y;
        public double MarkDiffTolerance_Y
        {
            get { return _MarkDiffTolerance_Y; }
            set
            {
                if (value != _MarkDiffTolerance_Y)
                {
                    _MarkDiffTolerance_Y = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkDiffToleranceOfWA_X;
        public double MarkDiffToleranceOfWA_X
        {
            get { return _MarkDiffToleranceOfWA_X; }
            set
            {
                if (value != _MarkDiffToleranceOfWA_X)
                {
                    _MarkDiffToleranceOfWA_X = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _MarkDiffToleranceOfWA_Y;
        public double MarkDiffToleranceOfWA_Y
        {
            get { return _MarkDiffToleranceOfWA_Y; }
            set
            {
                if (value != _MarkDiffToleranceOfWA_Y)
                {
                    _MarkDiffToleranceOfWA_Y = value;
                    RaisePropertyChanged();
                }
            }
        }



        #endregion

        private void btn_MarkSysSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MarkAligner().SetDelaywaferCamCylinderExtendedBeforeFocusing
                    (DelaywaferCamCylinderExtendedBeforeFocusing);
                this.MarkAligner().SetMarkDiffTolerance_X(MarkDiffTolerance_X);
                this.MarkAligner().SetMarkDiffTolerance_Y(MarkDiffTolerance_Y);
                this.MarkAligner().SetMarkDiffToleranceOfWA(MarkDiffToleranceOfWA_X, MarkDiffToleranceOfWA_Y);
                this.MarkAligner().SetTriggerMarkVerificationAfterWaferAlign(MarkVerificationAfterWaferAlign);
                this.MarkAligner().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #region PTPA DEBUG
        private bool _IsOnDubugMode = false;
        // 해당 옵션을 true 로 하면 Low, High Align  시에 오리진 패턴위치 ( Jump Index 0,0 ) 위치의 프로세싱 결과 이미지 저장
        // Wafer Alignment 후에 저장된 패드 위치로 이동해서 이미지 저장.
        public bool IsOnDubugMode
        {
            get { return _IsOnDubugMode; }
            set
            {
                if (value != _IsOnDubugMode)
                {
                    _IsOnDubugMode = value;
                    LoggerManager.Debug($"WaferAlign - IsOnDubugMode set to {_IsOnDubugMode}");
                    RaisePropertyChanged();
                }
            }
        }

        private string _IsOnDebugImagePathBase;
        public string IsOnDebugImagePathBase
        {
            get { return _IsOnDebugImagePathBase; }
            set
            {
                if (value != _IsOnDebugImagePathBase)
                {
                    _IsOnDebugImagePathBase = value;
                    LoggerManager.Debug($"WaferAlign - IsOnDebugImagePathBase set to {IsOnDebugImagePathBase}");
                    RaisePropertyChanged();
                }
            }
        }

        private string _IsOnDebugPadPathBase;
        public string IsOnDebugPadPathBase
        {
            get { return _IsOnDebugPadPathBase; }
            set
            {
                if (value != _IsOnDebugPadPathBase)
                {
                    _IsOnDebugPadPathBase = value;
                    LoggerManager.Debug($"WaferAlign - IsOnDebugPadPathBase set to {_IsOnDebugPadPathBase}");
                    RaisePropertyChanged();
                }
            }
        }

        private void btn_OpenWAPathFileExplore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        IsOnDebugImagePathBase = fbd.SelectedPath + "\\";
                    }
                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_OpenPadPathFileExplore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IsOnDebugPadPathBase = fbd.SelectedPath + "\\";
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_SetPTPADebug_Click(object sender, RoutedEventArgs e)
        {
            this.WaferAligner().IsOnDubugMode = IsOnDubugMode;
            this.WaferAligner().IsOnDebugImagePathBase = IsOnDebugImagePathBase;
            this.WaferAligner().IsOnDebugPadPathBase = IsOnDebugPadPathBase;
        }

        #endregion

        #region Mark Align 
        private void Mark_Align_Move_Error_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALIGN_MOVE_ERROR = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Align_Move_Error_Checked() : MARK_ALIGN_MOVE_ERROR = {MarkAligner.MarkAlignControlItems.MARK_ALIGN_MOVE_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Mark_Align_Move_Error_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALIGN_MOVE_ERROR = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Align_Move_Error_Unchecked() : MARK_ALIGN_MOVE_ERROR = {MarkAligner.MarkAlignControlItems.MARK_ALIGN_MOVE_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void PIN_ALIGN_Failure_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PinAligner.PIN_ALIGN_Failure = true;
                LoggerManager.Debug($"[{this.GetType().Name}], PIN_ALIGN_Failure_Checked() : PIN_ALIGN_Failure = {PinAligner.PIN_ALIGN_Failure}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void PIN_ALIGN_Failure_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PinAligner.PIN_ALIGN_Failure = false;
                LoggerManager.Debug($"[{this.GetType().Name}], PIN_ALIGN_Failure_Unchecked() : PIN_ALIGN_Failure = {PinAligner.PIN_ALIGN_Failure}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Each_Pin_Failure_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PinAligner.Each_Pin_Failure = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Each_Pin_Failure_Checked() : Each_Pin_Failure = {PinAligner.Each_Pin_Failure}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Each_Pin_Failure_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PinAligner.Each_Pin_Failure = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Each_Pin_Failure_Unchecked() : Each_Pin_Failure = {PinAligner.Each_Pin_Failure}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void Mark_Align_Focusing_Failed_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALIGN_FOCUSING_FAILED = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Align_Focusing_Failed_Checked() : MARK_ALIGN_FOCUSING_FAILED = {MarkAligner.MarkAlignControlItems.MARK_ALIGN_FOCUSING_FAILED}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Mark_Align_Focusing_Failed_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALIGN_FOCUSING_FAILED = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Align_Focusing_Failed_Unchecked() : MARK_ALIGN_FOCUSING_FAILED = {MarkAligner.MarkAlignControlItems.MARK_ALIGN_FOCUSING_FAILED}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MARK_Pattern_Failure_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_Pattern_Failure = true;
                LoggerManager.Debug($"[{this.GetType().Name}], MARK_Pattern_Failure_Checked() : MARK_Pattern_Failure = {MarkAligner.MarkAlignControlItems.MARK_Pattern_Failure}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void MARK_Pattern_Failure_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_Pattern_Failure = false;
                LoggerManager.Debug($"[{this.GetType().Name}], MARK_Pattern_Failure_Unchecked() : MARK_Pattern_Failure = {MarkAligner.MarkAlignControlItems.MARK_Pattern_Failure}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Mark_Algin_Pattern_Match_Failed_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALGIN_PATTERN_MATCH_FAILED = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Algin_Pattern_Match_Failed_Checked() : MARK_ALGIN_PATTERN_MATCH_FAILED = {MarkAligner.MarkAlignControlItems.MARK_ALGIN_PATTERN_MATCH_FAILED}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Mark_Algin_Pattern_Match_Failed_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALGIN_PATTERN_MATCH_FAILED = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Algin_Pattern_Match_Failed_Checked() : MARK_ALGIN_PATTERN_MATCH_FAILED = {MarkAligner.MarkAlignControlItems.MARK_ALGIN_PATTERN_MATCH_FAILED}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Mark_Align_Shift_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALIGN_SHIFT = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Align_Shift_Checked() : MARK_ALIGN_SHIFT = {MarkAligner.MarkAlignControlItems.MARK_ALIGN_SHIFT}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Mark_Align_Shift_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                MarkAligner.MarkAlignControlItems.MARK_ALIGN_SHIFT = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Mark_Align_Shift_Unchecked() : MARK_ALIGN_SHIFT = {MarkAligner.MarkAlignControlItems.MARK_ALIGN_SHIFT}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region Polish Wafer

        private void Polishwafer_Centering_Error_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_CENTERING_ERROR = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Centering_Error_Checked() : POLISHWAFER_CENTERING_ERROR = {PolishWafer.PolishWaferControlItems.POLISHWAFER_CENTERING_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Centering_Error_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_CENTERING_ERROR = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Centering_Error_Unchecked() : POLISHWAFER_CENTERING_ERROR = {PolishWafer.PolishWaferControlItems.POLISHWAFER_CENTERING_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Focusing_Error_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_FOCUSING_ERROR = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Focusing_Error_Checked() : POLISHWAFER_FOCUSING_ERROR = {PolishWafer.PolishWaferControlItems.POLISHWAFER_FOCUSING_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Focusing_Error_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_FOCUSING_ERROR = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Focusing_Error_Unchecked() : POLISHWAFER_FOCUSING_ERROR = {PolishWafer.PolishWaferControlItems.POLISHWAFER_FOCUSING_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Cleaing_Error_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEAING_ERROR = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Cleaing_Error_Checked() : POLISHWAFER_CLEAING_ERROR = {PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEAING_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Cleaing_Error_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEAING_ERROR = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Cleaing_Error_Unchecked() : POLISHWAFER_CLEAING_ERROR = {PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEAING_ERROR}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Cleaning_Margin_Exceeded_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEANING_MARGIN_EXCEEDED = true;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Cleaning_Margin_Exceeded_Checked() : POLISHWAFER_CLEANING_MARGIN_EXCEEDED = {PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEANING_MARGIN_EXCEEDED}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void Polishwafer_Cleaning_Margin_Exceeded_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEANING_MARGIN_EXCEEDED = false;
                LoggerManager.Debug($"[{this.GetType().Name}], Polishwafer_Cleaning_Margin_Exceeded_Unchecked() : POLISHWAFER_CLEANING_MARGIN_EXCEEDED = {PolishWafer.PolishWaferControlItems.POLISHWAFER_CLEANING_MARGIN_EXCEEDED}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        private void btn_stageonlinemode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StageSupervisor.SetStageMode(GPCellModeEnum.ONLINE);
                LoggerManager.Debug("(Manul Control)Stage Mode Change to ONLINE");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_stagemaintenancemode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StageSupervisor.SetStageMode(GPCellModeEnum.MAINTENANCE);
                LoggerManager.Debug("(Manul Control)Stage Mode Change to MAINTENANCE");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_stageofflinemode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StageSupervisor.SetStageMode(GPCellModeEnum.OFFLINE);
                LoggerManager.Debug("(Manul Control)Stage Mode Change to OFFLINE");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_proberId_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.FileManager().SetProberID(ProberID);
                this.FileManager().SaveSysParameter();
                LoggerManager.Debug($"(Manul Control)Prober ID Change to {ProberID}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_read_command_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string recv_data = null;
                string commandName = "";
                string argu = "";


                if (sender is System.Windows.Controls.Button)
                {
                    System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
                    if (button.CommandParameter is TesterCommand)
                    {
                        TesterCommand command = button.CommandParameter as TesterCommand;
                        recv_data = command.Name;
                        commandName = recv_data;
                        CommunicationRequestSet findreqset = null;

                        LoggerManager.Debug($"[GPIB Command - {commandName}],'{recv_data}',Command Start.");
                        if (this.GPIB().ProcessReadObject(recv_data, out commandName, out argu, out findreqset) == EventCodeEnum.NONE)
                        {
                            LoggerManager.Debug($"[GPIB Command - {commandName}], OnlyCommand:{commandName} Argument: {argu}");

                            command.Result = findreqset.Request.GetRequestResult()?.ToString();
                            LoggerManager.Debug($"[GPIB Command : {commandName}] {command.Result}");
                            LoggerManager.Debug($"[GPIB Command - {findreqset.Request.GetType().Name}],'{recv_data}',End.");

                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_custom_read_command_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string recv_data = null;
                string commandName = "";
                string argu = "";


                recv_data = txt_custom_command.Text;
                commandName = recv_data;
                CommunicationRequestSet findreqset = null;

                LoggerManager.Debug($"[GPIB Command - {commandName}],'{recv_data}',Command Start.");
                if (this.GPIB().ProcessReadObject(recv_data, out commandName, out argu, out findreqset) == EventCodeEnum.NONE)
                {
                    LoggerManager.Debug($"[GPIB Command - {commandName}], OnlyCommand:{commandName} Argument: {argu}");

                    tb_custom_command_result.Text = findreqset.Request.GetRequestResult()?.ToString();
                    LoggerManager.Debug($"[GPIB Command : {commandName}] {tb_custom_command_result.Text}");
                    LoggerManager.Debug($"[GPIB Command - {findreqset.Request.GetType().Name}],'{recv_data}',End.");

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_setPMIDelayInFirstGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pmiDevParam = this.PMIModule().GetPMIDevIParam() as PMIModuleDevParam;
                if (pmiDevParam != null)
                {
                    pmiDevParam.DelayInFirstGroup.Value = PMIDelayInFirstGroup;
                    LoggerManager.Debug($"(Manul Control)PMI DealyInFirstGroup value set to {PMIDelayInFirstGroup}");
                    this.PMIModule().SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_setpmiDelayAfterMoveToPad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var pmiDevParam = this.PMIModule().GetPMIDevIParam() as PMIModuleDevParam;
                if (pmiDevParam != null)
                {
                    pmiDevParam.DelayAfterMoveToPad.Value = PMIDelayAfterMoveToPad;
                    LoggerManager.Debug($"(Manul Control)PMI DelayAfterMoveToPad value set to {PMIDelayAfterMoveToPad}");
                    this.PMIModule().SaveDevParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ComboBox_TesterScenario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string Scenarioname = TesterScenarioComboBox.SelectedValue?.ToString();

                ScenarioModule = this.TesterCommunicationManager().ScenarioManager.ScenarioModule;
                ScenarioModule.ChangeScenario(Scenarioname);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            WaferAligner.WaferAlignControItems.IsManualRecoveryModifyMode = true;
            LoggerManager.Debug("WaferAligner.WaferAlignControItems.IsManualRecoveryModifyMode set to true");
        }

        private void CheckBox_Unchecked_2(object sender, RoutedEventArgs e)
        {
            WaferAligner.WaferAlignControItems.IsManualRecoveryModifyMode = false;
            LoggerManager.Debug("WaferAligner.WaferAlignControItems.IsManualRecoveryModifyMode set to false");
        }
        
        private void WaferAlignSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.WaferAligner().SetVerifyCenterLimitXYValue(VerifyCenterLimitX, VerifyCenterLimitY);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetLoadedWaferCountUntilBeforeLotStartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var previousValue = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart;

                this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeLotStart = LoadedWaferCountUntilBeforeLotStart;
                LoggerManager.Debug($"[{this.GetType().Name}], SetLoadedWaferCountUntilBeforeLotStartBtn_Click() : LoadedWaferCountUntilBeforeLotStart is changed. Old = {previousValue}, New = {LoadedWaferCountUntilBeforeLotStart}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void SetLoadedWaferCountUntilBeforeDeviceChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var previousValue = this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeDeviceChange;

                this.LotOPModule().LotInfo.LoadedWaferCountUntilBeforeDeviceChange = LoadedWaferCountUntilBeforeDeviceChange;
                LoggerManager.Debug($"[{this.GetType().Name}], SetLoadedWaferCountUntilBeforeDeviceChangeBtn_Click() : LoadedWaferCountUntilBeforeDeviceChange is changed. Old = {previousValue}, New = {LoadedWaferCountUntilBeforeDeviceChange}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void DoPMIDebugImages_CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                var pmiSysParam = this.PMIModule().GetPMISysIParam() as PMIModuleSysParam;

                if (pmiSysParam != null)
                {
                    // 여기에서 CheckBox의 현재 상태를 확인하고 변수를 설정합니다.
                    var checkBox = sender as System.Windows.Controls.CheckBox;
                    bool isChecked = checkBox.IsChecked ?? false; // null이면 false로 처리

                    pmiSysParam.DoPMIDebugImages = isChecked;  // CheckBox의 IsChecked 값을 직접 사용

                    LoggerManager.Debug($"[{this.GetType().Name}], DoPMIDebugImages_CheckBox_Changed() : DoPMIDebugImages value set to {pmiSysParam.DoPMIDebugImages}");

                    this.PMIModule().SaveSysParameter();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ComboBox_ImageCollectionOption_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as System.Windows.Controls.ComboBox;
                if (comboBox == null) return;

                var newValue = (ImageSaveFilter)comboBox.SelectedItem;
                var previousValue = this.VisionManager().imageSaveFilter;

                this.VisionManager().imageSaveFilter = newValue;

                LoggerManager.Debug($"[{this.GetType().Name}], ComboBox_ImageCollectionOption_SelectionChanged() : Selection changed from {previousValue} to {newValue}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckBox_debug_edge_processing_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaferAligner.WaferAlignControItems.IsDebugEdgeProcessing = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void CheckBox_debug_edge_processing_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                WaferAligner.WaferAlignControItems.IsDebugEdgeProcessing = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void btn_OpenVisionImageDecordTargetPathFileExplore_Click(object sender, RoutedEventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.ValidateNames = false; // 이름 유효성 검사를 하지 않음
                openFileDialog.CheckFileExists = false; // 파일이 실제로 존재하는지 확인하지 않음
                openFileDialog.CheckPathExists = true; // 경로가 실제로 존재하는지 확인
                openFileDialog.FileName = "폴더를 선택하세요"; // 파일 이름을 무의미한 값으로 설정하여 사용자가 폴더를 선택할 수 있도록 유도

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    VisionImageDecordTargetPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                }
            }
        }

        private void CheckBox_ImageBuffer_Encord_Checked(object sender, RoutedEventArgs e)
        {
            this.VisionManager().EnableImageBufferToTextFile = true;
        }

        private void CheckBox_ImageBuffer_Encord_UnChecked(object sender, RoutedEventArgs e)
        {
            this.VisionManager().EnableImageBufferToTextFile = false;
        }

        private void btn_ImageDecode_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(VisionImageDecordTargetPath) == false)
            {
                var flag = this.VisionManager().EnableImageBufferToTextFile;

                this.VisionManager().EnableImageBufferToTextFile = false;

                this.VisionManager().ImageBufferFromTextFiles(VisionImageDecordTargetPath);

                this.VisionManager().EnableImageBufferToTextFile = flag;
            }
        }
    }
}
