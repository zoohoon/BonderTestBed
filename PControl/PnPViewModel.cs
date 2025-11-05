using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnPControl
{
    using System.Collections.ObjectModel;
    using ProberInterfaces.AlignEX;
    using System.ComponentModel;
    using ProberInterfaces;
    using Autofac;
    using ProberInterfaces.Enum;
    using ProberInterfaces.Error;
    using ProberInterfaces.WaferAlign;
    using ProberInterfaces.WaferAlignEX;
    using NLog.Fluent;
    using ParamHelper;
    using DllImporter;
    using System.Reflection;
    using System.IO;
    using ProberInterfaces.PnpSetup;
    using System.Windows.Controls;
    using RelayCommandBase;
    using System.Windows.Input;
    using SubstrateObjects;
    using System.Collections;
    using System.Windows.Data;
    using ProberInterfaces.LightJog;
    using ProberInterfaces.HexagonJog;

    public class PnPViewModel : 
        INotifyPropertyChanged, IFactoryModule, IProberMainScreen/*, IPnpSetupScreen*/
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public int InitPriority { get; set; }
        private Autofac.IContainer Container;
        private IProberStation prober;
        private IStageSupervisor StageSupervisor;
        private IWaferAligner WaferAligner;

        public IProberStation Prober
        {
            get
            {
                return Container.Resolve<IProberStation>();
            }
        }

        private IZoomObject _ZoomObject;
        public IZoomObject ZoomObject
        {
            get { return _ZoomObject; }
            set
            {
                if (value != _ZoomObject)
                {
                    _ZoomObject = value;
                    NotifyPropertyChanged("ZoomObject");
                }
            }
        }
        private int _CurrMaskingLevel;
        public int CurrMaskingLevel
        {
            get { return _CurrMaskingLevel; }
            private set
            {
                if (value != _CurrMaskingLevel)
                {
                    _CurrMaskingLevel = value;
                    NotifyPropertyChanged("CurrMaskingLevel");
                }
            }
        }


        public ErrorCodeEnum InitModule(Autofac.IContainer container)
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
            Container = container;
            StageSupervisor = container.Resolve<IStageSupervisor>();
            WaferAligner = container.Resolve<IWaferAligner>();
            prober = container.Resolve<IProberStation>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public void FirstButton()
        {
            
        }

        public void SecondButton()
        {
            
        }

        private ObservableCollection<ModuleDllInfo> _setups;
        public ObservableCollection<ModuleDllInfo> setups
        {
            get { return _setups; }
            set
            {
                if (value != _setups)
                {
                    _setups = value;
                    NotifyPropertyChanged("setups");
                }
            }
        }

        private ObservableCollection<IPnpSetup> _PnpSetups;
        public ObservableCollection<IPnpSetup> PnpSteps           
        {
            get { return _PnpSetups; }
            set
            {
                if (value != _PnpSetups)
                {
                    _PnpSetups = value;
                    NotifyPropertyChanged("PnpSetups");
                }
            }
        }

        private ObservableCollection<IPnpSetupScreen> _PnpSetupScreen;
        public ObservableCollection<IPnpSetupScreen> PnpSetupScreen
        {
            get { return _PnpSetupScreen; }
            set
            {
                if (value != _PnpSetupScreen)
                {
                    _PnpSetupScreen = value;
                    NotifyPropertyChanged("PnpSetupScreen");
                }
            }
        }



        private ObservableCollection<PNPCommandButtonDescriptor> _Buttons;
        public ObservableCollection<PNPCommandButtonDescriptor> Buttons
        {
            get { return _Buttons; }
            set
            {
                if (value != _Buttons)
                {
                    _Buttons = value;
                    NotifyPropertyChanged("Buttons");
                }
            }
        }

        private object _MiniViewTarget;
        public object MiniViewTarget
        {
            get { return _MiniViewTarget; }
            set
            {
                if (value != _MiniViewTarget)
                {
                    _MiniViewTarget = value;
                    NotifyPropertyChanged("MiniViewTarget");
                }
            }
        }

        private string _Target;
        public string Target
        {
            get { return _Target; }
            set
            {
                if (value != _Target)
                {
                    _Target = value;
                    NotifyPropertyChanged("Target");
                }
            }
        }


        private object _MainViewTarget;
        public object MainViewTarget
        {
            get { return _MainViewTarget; }
            set
            {
                if (value != _MainViewTarget)
                {
                    _MainViewTarget = value;
                    NotifyPropertyChanged("MainViewTarget");
                }
            }
        }

        private RelayCommand _UIModeChangeCommand;
        public ICommand UIModeChangeCommand
        {
            get
            {
                if (null == _UIModeChangeCommand) _UIModeChangeCommand = new RelayCommand(UIModeChange);
                return _UIModeChangeCommand;
            }
        }

        private void UIModeChange()
        {

        }

        private RelayCommand _PlusCommand;
        public ICommand PlusCommand
        {
            get
            {
                if (null == _PlusCommand) _PlusCommand = new RelayCommand(Plus);
                return _PlusCommand;
            }
        }

        private void Plus()
        {
            try
            {
            string Plus = string.Empty;
            ZoomObject.ZoomLevel--;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private RelayCommand _MinusCommand;
        public ICommand MinusCommand
        {
            get
            {
                if (null == _MinusCommand) _MinusCommand = new RelayCommand(Minus);
                return _MinusCommand;
            }
        }

        private void Minus()
        {
            try
            {
            string Minus = string.Empty;
            ZoomObject.ZoomLevel++;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        private RelayCommand _PrevCommand;
        public ICommand PrevCommand
        {
            get
            {
                if (null == _PrevCommand) _PrevCommand = new RelayCommand(Prev);
                return _PrevCommand;
            }
        }

        private void Prev()
        {
            string Prev = string.Empty;
        }

        private RelayCommand _NextCommand;
        public ICommand NextCommand
        {
            get
            {
                if (null == _NextCommand) _NextCommand = new RelayCommand(Next);
                return _NextCommand;
            }
        }

        private void Next()
        {
            string Next = string.Empty;
        }

        private RelayCommand _UpCursorCommand;
        public ICommand UpCursorCommand
        {
            get
            {
                if (null == _UpCursorCommand) _UpCursorCommand = new RelayCommand(UpCursor);
                return _UpCursorCommand;
            }
        }

        private void UpCursor()
        {
            string UpCursor = string.Empty;
        }

        private RelayCommand _DownCursorCommand;
        public ICommand DownCursorCommand
        {
            get
            {
                if (null == _DownCursorCommand) _DownCursorCommand = new RelayCommand(DownCursor);
                return _DownCursorCommand;
            }
        }

        private void DownCursor()
        {
            string DownCursor = string.Empty;
        }

        private RelayCommand _RightCursorCommand;
        public ICommand RightCursorCommand
        {
            get
            {
                if (null == _RightCursorCommand) _RightCursorCommand = new RelayCommand(RightCursor);
                return _RightCursorCommand;
            }
        }

        private void RightCursor()
        {
            string RightCursor = string.Empty;
        }

        private RelayCommand _LeftCursorCommand;
        public ICommand LeftCursorCommand
        {
            get
            {
                if (null == _LeftCursorCommand) _LeftCursorCommand = new RelayCommand(LeftCursor);
                return _LeftCursorCommand;
            }
        }

        private void LeftCursor()
        {
            string LeftCursor = string.Empty;
        }

        private RelayCommand _EnterCursorCommand;
        public ICommand EnterCursorCommand
        {
            get
            {
                if (null == _EnterCursorCommand) _EnterCursorCommand = new RelayCommand(EnterCursor);
                return _EnterCursorCommand;
            }
        }
        

        
        public string ScreenLabel { get; set ; }
        public string ParamPath { get; set; }
        public ObservableCollection<IPnpSetup> PnpSetupStep { get; set; }
        public ObservableCollection<IPnpSetup> PnpRecoveryStep { get; set; }
        public IPnpSetup SeletedStep { get; set; }

        private void EnterCursor()
        {
            string EnterCursor = string.Empty;
        }

        private void InitObjects()
        {
            try
            {
            PnpSteps = new ObservableCollection<IPnpSetup>();
            Buttons = new ObservableCollection<PNPCommandButtonDescriptor>();
            PnpSetupScreen = new ObservableCollection<IPnpSetupScreen>();
            
            ZoomObject = (IZoomObject)StageSupervisor.WaferObject;
            MainViewTarget = StageSupervisor.VisionManager.GetCam(EnumProberCam.PIN_LOW_CAM);
            MiniViewTarget = StageSupervisor.WaferObject;
            //Target = "Map";

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        public ErrorCodeEnum InitModule(Autofac.IContainer container, object param)
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
            Container = container;
            StageSupervisor = container.Resolve<IStageSupervisor>();
            WaferAligner = container.Resolve<IWaferAligner>();
            object deserializedObj;

            InitObjects();

            PNPCommandButtonDescriptor button = new PNPCommandButtonDescriptor();

            for (int i = 0; i < 4; i++)
            {
                button = new PNPCommandButtonDescriptor();
                button.Caption = "Button" + string.Format("{0}", i + 1);
                button.Command = (RelayCommand)UIModeChangeCommand;
                //Buttons.Add(button);
            }
            string filepath = "C:\\ProberSystem\\Parameters\\WaferAlignerParameter\\SetupSteps\\SetupSteps.xml";

            DllImporter DLLImporter = new DllImporter();
            try
            {
                if (param is string)
                {
                    string key = (string)param;

                    if (Directory.Exists(System.IO.Path.GetDirectoryName(filepath)) == false)
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filepath));
                    }

                    if (File.Exists(filepath) == false)
                    {
                        //DefaultSetting();
                        //ParamServices.Serialize(filepath, this.setups, new Type[] { typeof(ObservableCollection<ModuleDllInfo>) });
                    }
                    // Deserialize
                    deserializedObj = ParamServices.Deserialize(
                    filepath,
                    typeof(ObservableCollection<ModuleDllInfo>));

                    setups = (ObservableCollection<ModuleDllInfo>)deserializedObj;

                    //IPnpSetup setupStep;
                    //IPnpSetupScreen setupScreen;
                    foreach (ModuleDllInfo info in setups)
                    {
                        Tuple<bool, Assembly> ret = DLLImporter.LoadDLL(info);

                        if (ret != null && ret.Item1 == true)
                        {
                            if(info.AssemblyName == ret.Item2.ManifestModule.Name)
                            {                               
                                //setupStep = DLLImporter.Assignable<IPnpSetup>(ret.Item2);
                                
                                //if (setupStep != null)
                                //{
                                //    //WaferAlignModuleBase waModule = (WaferAlignModuleBase)setupStep;
                                //    //waModule.InitModule(container);
                                //    setupStep.InitModule(container);

                                //    //setupStep.PnpSetups.Add(setupStep);

                                //    //setupScreen.PnpSetups.Add(setupStep);
                                //    //PnpSetupScreens = Prober.PnpSetupScreen;
                                    
                                //    //PnpSetups.Add(setupStep);
                                    
                                //    //Enum으로 현재 타입 구분 (Wafer인지 Pin인지)  
                                    
                                //    //Prober.PnpSetupScreen[0].PnpSetups = PnpSetups;
                                //   // Prober.PnpSetupScreen[0].PnpSetups[0].Buttons = PnpSetups[0].Buttons;
                                    
                                //    PnpSetupScreen = Prober.PnpSetupScreen;                                                                      
                                //}
                                //else
                                //{
                                //    Log.Error(string.Format("Assembly validation failed. Type should be {0}", typeof(IPnpSetup)));
                                //}
                                //setupScreen = DLLImporter.Assignable<IPnpSetupScreen>(ret.Item2);
                                //if (setupScreen != null)
                                //{
                                //    //WaferAlignModuleBase waModule = (WaferAlignModuleBase)setupStep;
                                //    //waModule.InitModule(container);
                                //    setupScreen.InitModule(container);
                                //    PnpSetupScreen.Add(setupScreen);
                                //}
                                //else
                                //{
                                //    Log.Error(string.Format("Assembly validation failed. Type should be {0}", typeof(IPnpSetup)));
                                //}
                            }
                        }
                    }
                }
                
            }
            catch (Exception err)
            {
                Log.Error("PnPViewModel - InitModule Error : " + err.ToString());
                return retVal;
            }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return retVal;
        }

        public void DefaultPNPParamSetting()
        {
            
        }

        public ErrorCodeEnum LoadPnpParam()
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum SetContainer(Autofac.IContainer container)
        {
            throw new NotImplementedException();
        }
        public ErrorCodeEnum InitPage(object parameter = null)
        {
            return ErrorCodeEnum.NONE;
        }
    }
}
