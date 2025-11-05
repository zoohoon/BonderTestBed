using ProberInterfaces.Focus;
using System;
using System.Collections.Generic;
using System.Linq;
using ProberErrorCode;
using LogModule;
using ProberInterfaces;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocusingManager
{
    using Autofac;
    using ProberInterfaces.Param;
    using System.Collections.ObjectModel;
    using System.Windows;

    public class FocusManager : IFocusManager, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;
        public static IContainer FocusContainer { get; set; }
        private static bool isConfiureDependecies = false;

        private Dictionary<string, IFocusing> _FocusList;
        public Dictionary<string, IFocusing> FocusList
        {
            get { return _FocusList; }
            set
            {
                if (value != _FocusList)
                {
                    _FocusList = value;
                }
            }
        }

        private ModuleDllInfo _FocusDllInfo;
        public ModuleDllInfo FocusDllInfo
        {
            get { return _FocusDllInfo; }
            set
            {
                if (value != _FocusDllInfo)
                {
                    _FocusDllInfo = value;
                    RaisePropertyChanged();
                }
            }
        }


        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
                    FocusDllInfo = new ModuleDllInfo();

                    FocusDllInfo.AssemblyName = "Focusing.dll";

                    isConfiureDependecies = false;
                    FocusList = new Dictionary<string, IFocusing>();

                    retval = ConfigureDependencies();


                    if (retval != EventCodeEnum.NONE)
                    {
                        LoggerManager.Error($"ConfigureDependencies() Failed");
                    }

                    Initialized = true;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum ConfigureDependencies()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (isConfiureDependecies == false)
                {
                    var builder = new ContainerBuilder();

                    //String strFolder = System.IO.Directory.GetCurrentDirectory();
                    String strFolder = AppDomain.CurrentDomain.BaseDirectory;

                    List<Assembly> allAssemblies = new List<Assembly>();

                    string LoadDLLPath = Path.Combine(strFolder, FocusDllInfo.AssemblyName);

                    Assembly ass = Assembly.LoadFrom(LoadDLLPath);

                    if (ass != null)
                    {
                        allAssemblies.Add(ass);

                        foreach (var type in ass.GetTypes())
                        {
                            var cmdInterfaceTypes = type.GetInterfaces().Where(x => typeof(IFocusing).IsAssignableFrom(x)).ToList();

                            if (cmdInterfaceTypes.Count > 0)
                            {

                            }
                            //if (cmdInterfaceTypes.Count == 2)
                            //{
                            //    var foundType = cmdInterfaceTypes.Where(item => item != typeof(IProbeCommand)).First();
                            //    string cmdName = CommandNameGen.Generate(foundType);
                            //}
                            //else
                            //{
                            //    //err
                            //}

                        }
                    }

                    foreach (var assembly in allAssemblies)
                    {
                        builder.RegisterAssemblyModules(assembly);
                    }

                    FocusContainer = builder.Build();

                    isConfiureDependecies = true;

                    //string teststr = "NomalFocusing";
                    //var focus = FocusContainer.ResolveNamed<FocusingBase>(teststr);
                    //focus.Focusing();

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"Duplicate invocation");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public IFocusing GetFocusingModel(ModuleDllInfo info)
        {
            IFocusing focusingModel = null;

            try
            {
                string key = string.Empty;

                info.ClassName = new ObservableCollection<string>(info.ClassName.Distinct());
                if ((info.NameSpaceName != null) && (info.ClassName != null) && (info.ClassName.Count == 1))
                {
                    key = info.NameSpaceName + "." + info.ClassName[0];
                }

                focusingModel = FocusContainer.ResolveNamed<FocusingBase>(key);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return focusingModel;
        }

        public EventCodeEnum ValidationFocusParam(FocusParameter param)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                bool IsInValid = false;

                if (param.FocusMaxStep.Value == 0)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : FocusMaxStep is invalid. Value = {param.FocusMaxStep.Value}");

                    IsInValid = true;
                }

                if (param.FocusRange.Value == 0)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : FocusRange is invalid. Value = {param.FocusRange.Value}");

                    IsInValid = true;
                }

                if (param.FocusingAxis.Value == EnumAxisConstants.Undefined)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : FocusingAxis is invalid. Value = {param.FocusingAxis.Value}");

                    IsInValid = true;
                }

                if (param.FocusingCam.Value == EnumProberCam.INVALID || param.FocusingCam.Value == EnumProberCam.UNDEFINED)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : FocusingCam is invalid. Value = {param.FocusingCam.Value}");

                    IsInValid = true;
                }

                if (param.FocusingROI.Value.Width <= 0)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : FocusingROI's Width is invalid. Value = {param.FocusingROI.Value.Width}");

                    IsInValid = true;
                }

                if (param.FocusingROI.Value.Height <= 0)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : FocusingROI's Height is invalid. Value = {param.FocusingROI.Value.Height}");

                    IsInValid = true;
                }


                if (param.OutFocusLimit.Value <= 0)
                {
                    LoggerManager.Debug($"[FocusManager], ValidationFocusParam() : OutFocusLimit is invalid. Value = {param.OutFocusLimit.Value}");

                    IsInValid = true;
                }

                if (IsInValid == false)
                {
                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    retval = EventCodeEnum.PARAM_ERROR;
                }

                //if ((param.FocusMaxStep.Value == 0) ||
                //    (param.FocusRange.Value == 0) ||
                //    (param.FocusingAxis.Value == EnumAxisConstants.Undefined) ||
                //    (param.FocusingCam.Value == EnumProberCam.INVALID) || (param.FocusingCam.Value == EnumProberCam.UNDEFINED) ||
                //    (param.FocusingROI.Value.Width <= 0) || (param.FocusingROI.Value.Height <= 0) ||
                //    (param.OutFocusLimit.Value <= 0)
                //    )
                //{
                //    retval = EventCodeEnum.PARAM_ERROR;
                //}
                //else
                //{
                //    retval = EventCodeEnum.NONE;
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void MakeDefalutFocusParam(EnumProberCam cam, EnumAxisConstants axis, FocusParameter param, double range)
        {
            param.FocusingCam.Value = cam;
            param.FocusingAxis.Value = axis;

            param.FocusMaxStep.Value = 20;
            param.FocusRange.Value = range;
            param.DepthOfField.Value = 1;
            param.FocusThreshold.Value = 10000;
            param.FlatnessThreshold.Value = 20;
            param.PeakRangeThreshold.Value = 40;
            param.PotentialThreshold.Value = 20;
            param.CheckPotential.Value = true;
            param.CheckThresholdFocusValue.Value = true;
            param.CheckFlatness.Value = true;
            param.CheckDualPeak.Value = true;

            param.FocusingROI.Value = new Rect(0, 0, 960, 960);
            param.OutFocusLimit.Value = 40;

            if (param.LightParams == null)
            {
                param.LightParams = new ObservableCollection<LightValueParam>();
            }
        }

        public void MakeDefalutFocusParam(EnumProberCam cam, EnumAxisConstants axis, FocusParameter param)
        {
            param.FocusingCam.Value = cam;
            param.FocusingAxis.Value = axis;

            param.FocusMaxStep.Value = 20;
            param.FocusRange.Value = 200;
            param.DepthOfField.Value = 1;
            param.FocusThreshold.Value = 10000;
            param.FlatnessThreshold.Value = 20;
            param.PeakRangeThreshold.Value = 40;
            param.PotentialThreshold.Value = 20;
            param.CheckPotential.Value = true;
            param.CheckThresholdFocusValue.Value = true;
            param.CheckFlatness.Value = true;
            param.CheckDualPeak.Value = true;

            param.FocusingROI.Value = new Rect(0, 0, 960, 960);
            param.OutFocusLimit.Value = 40;

            param.LightParams = new ObservableCollection<LightValueParam>();
        }
    }
}
