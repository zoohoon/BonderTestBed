using System;
using System.Collections.Generic;
using ProberErrorCode;
using ProberInterfaces;
using System.Windows.Controls;
using System.Windows;
using System.Resources;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Data;
using AccountModule;
using LogModule;

namespace CUIServices
{
    public static class CUIService
    {
        private static Dictionary<Guid, int> MaskingDictionary;
        private static Dictionary<Guid, Guid> TargetViewDictionary;
        private static Dictionary<Guid, bool> VisibilityDictionary;

        public static CUIElementCollection param;

        private const string ResourcesPath = "Properties.Resources";

        public static EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = LoadCUIParam();

                retval = SetMaskingDictionary();
                retval = SetTargetViewDictionary();
                retval = SetVisibilityDictionary();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private static EventCodeEnum SetMaskingDictionary()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (MaskingDictionary == null)
                {
                    MaskingDictionary = new Dictionary<Guid, int>();
                }

                foreach (var info in param.Infos)
                {
                    if (MaskingDictionary.ContainsKey(info.CUIGUID) == false)
                    {
                        MaskingDictionary.Add(info.CUIGUID, info.MaskingLevel);
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return RetVal;
        }

        private static EventCodeEnum SetTargetViewDictionary()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (TargetViewDictionary == null)
                {
                    TargetViewDictionary = new Dictionary<Guid, Guid>();
                }

                foreach (var info in param.Infos)
                {
                    if (TargetViewDictionary.ContainsKey(info.CUIGUID) == false)
                    {
                        TargetViewDictionary.Add(info.CUIGUID, info.TargetViewGUID);
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return RetVal;
        }

        private static EventCodeEnum SetVisibilityDictionary()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                if (VisibilityDictionary == null)
                {
                    VisibilityDictionary = new Dictionary<Guid, bool>();
                }

                foreach (var info in param.Infos)
                {
                    if (VisibilityDictionary.ContainsKey(info.CUIGUID) == false)
                    {
                        VisibilityDictionary.Add(info.CUIGUID, info.Visibility);
                    }
                }

                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw err;
            }

            return RetVal;
        }

        public static EventCodeEnum SaveCUIParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = Extensions_IParam.SaveParameter(null, param);
                if (RetVal != EventCodeEnum.NONE)
                {
                    throw new Exception($"[CUIService - SaveAutoTiltSysFile] Faile SaveParameter");
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                throw err;
            }

            return RetVal;
        }

        private static EventCodeEnum LoadCUIParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            param = new CUIElementCollection();

            try
            {
                ////임시 코드.
                //param.SetDefaultParam();
                //RetVal = Extensions_IParam.SaveParameter(null, param);

                //if (RetVal == EventCodeEnum.NONE)
                //{
                //}


                IParam tmpParam = null;
                RetVal = Extensions_IParam.LoadParameter(null, ref tmpParam, typeof(CUIElementCollection));

                if (RetVal == EventCodeEnum.NONE)
                {
                    param = tmpParam as CUIElementCollection;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.UNDEFINED;
                throw err;
            }

            return RetVal;
        }

        public static int GetMaskingLevel(Guid GUID)
        {
            bool ret = false;
            int MaskingLevel = -1;

            try
            {
                if (MaskingDictionary != null)
                {
                    ret = MaskingDictionary.TryGetValue(GUID, out MaskingLevel);
                }
                if (ret == false)
                {
                    MaskingLevel = 0;
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return MaskingLevel;
        }

        public static int SetMaskingLevel(Guid GUID, int value)
        {
            bool ret = false;
            int MaskingLevel = 0;

            try
            {
                ret = MaskingDictionary.TryGetValue(GUID, out MaskingLevel);

                if (ret == false)
                {
                    //MaskingLevel = -1;

                    MaskingDictionary.Add(GUID, value);
                    CUIElementInfo tmpcuiinfo = param.MakeCUIElementInfo(GUID, value, new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), false, true, "Unknown Control");
                    LoggerManager.Debug($"[CUIService] SetMaskingLevel() Curr GUID : {GUID}, Value : {value}");
                    param.Infos.Add(tmpcuiinfo);
                    SaveCUIParam();
                }
                else
                {
                    MaskingDictionary[GUID] = value;
                    var findContent = param.Infos.Find(i => i.CUIGUID == GUID);
                    if (findContent != null)
                    {
                        LoggerManager.Debug($"[CUIService] SetMaskingLevel() prev Masking Lv : {findContent.MaskingLevel}, curr Masking Lv : {value}, Curr user name : {AccountManager.CurrentUserInfo.UserName}");

                        findContent.MaskingLevel = value;
                        SaveCUIParam();
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return MaskingLevel;
        }

        public static Guid GetTargetViewGUID(Guid GUID)
        {
            bool ret;

            Guid TargetViewGUID;

            try
            {
                ret = TargetViewDictionary.TryGetValue(GUID, out TargetViewGUID);

                if (ret == false)
                {
                    throw new Exception($"[CUIService - GetTargetViewGUID()] Can't find the target GUID. {GUID.ToString()}");
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return TargetViewGUID;
        }

        public static bool GetVisibility(Guid GUID)
        {
            bool ret;

            bool visibility;

            try
            {
                ret = VisibilityDictionary.TryGetValue(GUID, out visibility);

                if (ret == false)
                {
                    //throw new Exception();
                }
            }
            catch (Exception err)
            {
                throw new Exception($"[CUIService - GetVisibility()] {err}");
            }

            return visibility;
        }

        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            try
            {
                var type = objectToCheck.GetType();
                return type.GetMethod(methodName) != null;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static bool HasProperty(this object objectToCheck, string propertyName)
        {
            try
            {
                var type = objectToCheck.GetType();
                return type.GetProperty(propertyName) != null;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static Type FindAncestor<Type>(DependencyObject dependencyObject) where Type : class
        {
            try
            {
                DependencyObject target = dependencyObject;
                do
                {
                    target = VisualTreeHelper.GetParent(target);
                }
                while (target != null && !(target is Type));

                return target as Type;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static void ShowToolTip(object sender)
        {
            try
            {
                if ((sender is FrameworkElement))
                {
                    bool valid = HasProperty(sender, "GUID");

                    if (valid == true)
                    {
                        Guid guid;
                        var property = sender.GetType().GetProperty("GUID");

                        guid = (Guid)(property.GetValue(sender));

                        if (guid != Guid.Empty)
                        {
                            IToolTipInfoBase tooltipinfo = null;
                            CUIElementInfo cuiElementInfo = null;

                            cuiElementInfo = param.Infos.Find(x => x.CUIGUID == guid);

                            if (cuiElementInfo != null)
                            {
                                tooltipinfo = cuiElementInfo.tooltipinfo;
                            }

                            if (tooltipinfo != null)
                            {
                                DataTemplate tooltipDataTemplatate = null;
                                var visualRoot = FindAncestor<IResourceProvider>((DependencyObject)sender);

                                if (visualRoot != null)
                                {
                                    tooltipDataTemplatate = FindTooltipTemplate(tooltipinfo);
                                    SettingToolTipInfo(tooltipinfo, visualRoot);
                                    OpenToolTip(tooltipinfo, tooltipDataTemplatate, sender);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static DataTemplate FindTooltipTemplate(IToolTipInfoBase tooltipinfo)
        {
            try
            {
                DataTemplate tooltipTemplate = null;

                var hasToolTipTemplate = Application.Current.TryFindResource(tooltipinfo.TemplateName);

                if (hasToolTipTemplate != null)
                {
                    tooltipTemplate = (DataTemplate)Application.Current.Resources[tooltipinfo.TemplateName];
                }

                return tooltipTemplate;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static bool SettingToolTipInfo(IToolTipInfoBase tooltipinfo, IResourceProvider visualRoot)
        {
            try
            {
                bool retVal = true;

                ToolTipTitleDescription toolTipTitleDescription;
                toolTipTitleDescription = GetToolTipTitleAndDescription(tooltipinfo, visualRoot);

                tooltipinfo.Title = toolTipTitleDescription.Title;
                tooltipinfo.Description = toolTipTitleDescription.Description;

                return retVal;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static ToolTipTitleDescription GetToolTipTitleAndDescription(IToolTipInfoBase tooltipinfo, IResourceProvider visualRoot)
        {
            ToolTipTitleDescription toolTipTitleDescription = new ToolTipTitleDescription();
            Assembly assembly = visualRoot.GetType().Assembly;

            if (assembly != null)
            {
                try
                {
                    string BaseName = assembly.GetName().Name;
                    BaseName = BaseName + "." + ResourcesPath;
                    ResourceManager rm = new ResourceManager(BaseName, assembly);

                    if (rm != null)
                    {
                        toolTipTitleDescription.Title = GetStringFromResourceManager(rm, tooltipinfo.Resource_Title);
                        toolTipTitleDescription.Description = GetStringFromResourceManager(rm, tooltipinfo.Resource_Description);
                    }
                }
                catch (Exception err)
                {
                    throw err;
                }
            }

            return toolTipTitleDescription;
        }

        private static ToolTip TooltipControl;
        private static void OpenToolTip(IToolTipInfoBase tooltipinfo, DataTemplate tooltipTemplate, object sender)
        {
            try
            {
                if (tooltipTemplate != null)
                {
                    TooltipControl = new ToolTip();

                    TooltipControl.Width = tooltipinfo.Width;
                    TooltipControl.Height = tooltipinfo.Height;

                    TooltipControl.Content = tooltipinfo;
                    TooltipControl.ContentTemplate = tooltipTemplate;

                    TooltipControl.Placement = tooltipinfo.PlacementMode;
                    TooltipControl.PlacementTarget = (UIElement)sender;

                    ToolTipService.SetShowDuration(TooltipControl, tooltipinfo.Duration);
                    TooltipControl.IsOpen = true;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }


        private static string GetStringFromResourceManager(ResourceManager rm, string key)
        {
            string getString = null;

            try
            {
                if (key != null)
                {
                    getString = rm.GetString(key);
                }
                else
                {
                    getString = "UNKNOWN";
                }
            }
            catch (Exception err)
            {
                throw err;
            }

            return getString;
        }

        public static ToolTip GetToolTipInResource()
        {
            try
            {
                ToolTip retval = new ToolTip();

                // First, Get Resource Key using GUID 
                // Second, Get ToolTip Information

                return retval;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static void ToggleMaskingReleaseMode(ICUIControl toggleControl)
        {
            FrameworkElement frameworkElement = toggleControl as FrameworkElement;

            try
            {
                if(frameworkElement != null)
                {
                    toggleControl.IsReleaseMode ^= true;

                    if (toggleControl.IsReleaseMode == false)
                    {
                        //if (toggleControl.IsEnableBindingBase != null)
                        //{
                        //    frameworkElement.SetBinding(FrameworkElement.IsEnabledProperty, toggleControl.IsEnableBindingBase);
                        //}
                        ApplyMasking(toggleControl);
                    }
                    else
                    {
                        //if (toggleControl.IsEnableBindingBase == null)
                        //{
                        //    var bindingBase = BindingOperations.GetBindingBase(frameworkElement, FrameworkElement.IsEnabledProperty);

                        //    if (bindingBase != null)
                        //    {
                        //        toggleControl.IsEnableBindingBase = bindingBase;
                        //    }
                        //}

                        //frameworkElement.IsEnabled = true;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static void ApplyMasking(ICUIControl toggleControl)
        {
            FrameworkElement frameworkElement = toggleControl as FrameworkElement;
            try
            {
                if (frameworkElement != null &&
                    AccountManager.CurrentUserInfo != null)
                {
                    if (AccountManager.IsUserLevelAboveThisNum(toggleControl.MaskingLevel))
                    {
                        if (toggleControl.IsEnableBindingBase != null)
                        {
                            frameworkElement.SetBinding(FrameworkElement.IsEnabledProperty, toggleControl.IsEnableBindingBase);
                        }
                    }
                    else
                    {
                        if (toggleControl.IsEnableBindingBase == null)
                        {
                            var bindingBase = BindingOperations.GetBindingBase(frameworkElement, FrameworkElement.IsEnabledProperty);

                            if (bindingBase != null)
                            {
                                toggleControl.IsEnableBindingBase = bindingBase;
                            }
                        }
                        frameworkElement.IsEnabled = false;
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private struct ToolTipTitleDescription
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}
