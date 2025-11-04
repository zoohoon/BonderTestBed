using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProberInterfaces;
using ModuleFactory;
using Autofac;
using RelayCommandBase;
using LogModule;
using System;
using DBManagerModule;

namespace FileSystem.Tests
{
    [TestClass]
    public class TestBaseProberSystem : IFactoryModule
    {
        public static IContainer Container;

        //[AssemblyInitialize]
        //public static void AssemblyInitialize(TestContext testContext)
        //{
        //}

        [TestInitialize]
        public void Initialize()
        {
            if (System.Windows.Application.Current == null)
            {
                // Create the Application instance
                new System.Windows.Application();
            }

            SystemManager.SysteMode = SystemModeEnum.Multiple;
            Container = ModuleResolver.ConfigureDependencies();

            Extensions_IModule.SetContainer(null, Container);
            Extensions_ICommand.SetStageContainer(null, Container);

            SystemManager.LoadParam();
            SystemModuleCount.LoadParam();
            LoggerManager.Init();

            DBManager.Open();
        }
    }

    [TestClass()]
    public class FileManagerTests : IFactoryModule
    {
        public static IContainer Container;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            SystemManager.SysteMode = SystemModeEnum.Multiple;
            Container = ModuleResolver.ConfigureDependencies();

            Extensions_IModule.SetContainer(null, Container);
            Extensions_ICommand.SetStageContainer(null, Container);
        }

        public static int index = 1;

        [DataTestMethod]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, false, "\\MFImage\\", "MFResult")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, false, "\\MFImage\\ResultImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\Success\\PassImage_Distance_Rect_Cir")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\TargetImage")]
        [DataRow(EnumProberModule.CARDCHANGE, IMAGE_SAVE_TYPE.BMP, true, "\\MFImage\\FailImage\\", "TargetImage")]
        [DataRow(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, false, "\\EDGE", "\\Edge")]
        [DataRow(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\", "WaferID\\Sucess\\[CamType}]_X#_Y#")]
        [DataRow(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\", "WaferID\\Fail\\[CamType]_X#_Y#_Pattern")]
        [DataRow(EnumProberModule.WAFERALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\", "WaferID\\Fail\\[CamType]_X#_Y#_")]
        [DataRow(EnumProberModule.SOAKING, IMAGE_SAVE_TYPE.BMP, true, "\\Focusing_Image\\SuccessImage\\success_img")]
        [DataRow(EnumProberModule.SOAKING, IMAGE_SAVE_TYPE.BMP, true, "\\Focusing_Image\\SuccessImage\\OutOfCondision\\fail_img")]
        [DataRow(EnumProberModule.SOAKING, IMAGE_SAVE_TYPE.BMP, true, "\\Focusing_Image\\FailImage\\fail_img")]
        [DataRow(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\SINGLEPINBLOB\\SINGLEPINBLOB\\", "PinNo#__Threshold__PASS")]
        [DataRow(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\HIGHKEYBLOB\\", "PinNo#__Threshold__PASS")]
        [DataRow(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\SINGLEPINBLOB\\SINGLEPINBLOB\\", "PinNo#__Threshold__ORIGINAL")]
        [DataRow(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\HIGHKEYBLOB\\", "PinNo#__Threshold__ORIGINAL")]
        [DataRow(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\SINGLEPINBLOB\\SINGLEPINBLOB\\", "PinNo#__Threshold__FAIL")]
        [DataRow(EnumProberModule.PINALIGNER, IMAGE_SAVE_TYPE.BMP, true, "\\HIGHKEYBLOB\\", "PinNo#__Threshold__FAIL")]
        [DataRow(EnumProberModule.VISIONPROCESSING, IMAGE_SAVE_TYPE.BMP, true, "\\PM\\FAIL\\", "TargetImage")]
        [DataRow(EnumProberModule.VISIONPROCESSING, IMAGE_SAVE_TYPE.BMP, true, "\\PM\\FAIL\\", "Patternimage")]
        public void GetImageSavePathByModuleTypeTest(EnumProberModule moduletype, IMAGE_SAVE_TYPE type, bool AppendCurrentTime, params string[] paths)
        {
            try
            {
                SystemManager.LoadParam();
                SystemModuleCount.LoadParam();
                LoggerManager.Init();

                // Send ParamPath
                Container.Resolve<IFileManager>(new NamedParameter("ParamPath", "C:\\ProberSystem\\C01"));

                this.FileManager().GetImageSaveFullPath(moduletype, type, AppendCurrentTime, paths);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }
}